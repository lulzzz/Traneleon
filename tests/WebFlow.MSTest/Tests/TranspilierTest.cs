using Acklann.GlobN;
using Acklann.WebFlow.Compilation;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shouldly;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Acklann.WebFlow.Tests
{
    [TestClass]
    [TestCategory("node.exe")]
    [UseReporter(typeof(BeyondCompare4Reporter), typeof(ClipboardReporter))]
    public class TranspilierTest
    {
        private static readonly string ResultDirectory = Path.Combine(Path.GetTempPath(), nameof(TranspilierTest));

        [TestInitialize]
        public void Initialize()
        {
            if (Directory.Exists(ResultDirectory)) Directory.Delete(ResultDirectory, recursive: true);
            Directory.CreateDirectory(ResultDirectory);
        }

        [TestMethod]
        public void TypescriptCompiler_can_compile_ts_files()
        {
            RunCompileTest<TypescriptCompiler>(TestFile.GetScript1TS(), ".js");
        }

        [TestMethod]
        public void TypescriptCompiler_can_report_errors()
        {
            RunErrorTest<TypescriptCompiler>(TestFile.GetBadScript1TS(), 3);
        }

        [TestMethod]
        public void SassCompiler_can_compile_scss_files()
        {
            RunCompileTest<SassCompiler>(TestFile.GetStyle1SCSS(), ".css");
        }

        [TestMethod]
        public void SassCompiler_can_report_errors()
        {
            RunErrorTest<SassCompiler>(TestFile.GetBadStyle1SCSS(), 2);
        }

        private static void RunCompileTest<T>(FileInfo sourceFile, string ext) where T : ICompiler
        {
            string folder(string name) => Path.Combine(ResultDirectory, name);
            string outFile(string outDir) => Path.Combine(folder(outDir), Path.ChangeExtension(sourceFile.Name, ext));

            // Arrange
            using (var sut = (ICompiler)Activator.CreateInstance(typeof(T)))
            {
                var cases = new(int, TranspilierSettings)[]
                {
                    (1, new TranspilierSettings(outFile("case1"), new string[1] { sourceFile.FullName }, ".min", null, false, false, false)),
                    (4, new TranspilierSettings(outFile("case2"), new string[1] { sourceFile.FullName }, ".min", folder("case2/maps"), true, true, true))
                };

                // Act
                foreach (var (totalExpectedFiles, options) in cases)
                {
                    var canExecute = sut.CanExecute(options);
                    var result = (TranspilierResult)sut.Execute(options);
                    var generatedFiles = Directory.GetFiles(options.OutputDirectory, "*", SearchOption.AllDirectories)
                        .Select(x => Path.GetFileName(x)).ToArray();

                    // Assert
                    canExecute.ShouldBeTrue();
                    result.Succeeded.ShouldBeTrue();
                    generatedFiles.Length.ShouldBe(totalExpectedFiles, string.Join(" + ", generatedFiles));

                    using (ApprovalResults.ForScenario(Path.GetFileName(options.OutputDirectory)))
                    {
                        var contents = new StringBuilder();
                        contents.AppendLine(File.ReadAllText(result.CompiliedFile));
                        if (options.GenerateSourceMaps)
                        {
                            contents.AppendLine("==================================================");
                            var map = JObject.Parse(File.ReadAllText(Path.Combine(options.SourceMapDirectory, (Path.GetFileName(result.CompiliedFile) + ".map"))));
                            if (map.TryGetValue("sources", out JToken sources))
                            {
                                var array = sources as JArray;
                                for (int i = 0; i < array.Count; i++)
                                    if (File.Exists(array[i].Value<string>().ExpandPath(options.SourceMapDirectory)))
                                    {
                                        array[i].Replace(".../" + Path.GetFileName(array[i].Value<string>()));
                                    }
                                    else Assert.Fail($"The source map file '{array[i]}' was not generated.");
                            }
                            else Assert.Fail($"The source map for '{result.CompiliedFile}' was not generated.");
                            contents.AppendLine(map.ToString(Formatting.Indented));
                        }

                        Approvals.Verify(contents.ToString());
                    }
                }
            }
        }

        private static void RunErrorTest<T>(FileInfo sourceFile, int errorLine) where T : ICompiler
        {
            // Arrange
            using (var sut = (ICompiler)Activator.CreateInstance(typeof(T)))
            {
                var options = new TranspilierSettings(Path.ChangeExtension(Path.Combine(ResultDirectory, sourceFile.Name), ".js"), sourceFile.FullName);

                // Act
                var result = (TranspilierResult)sut.Execute(options);

                // Assert
                result.Succeeded.ShouldBeFalse();
                result.ErrorList.ShouldNotBeEmpty();
                result.ErrorList[0].Line.ShouldBe(errorLine);
                result.ErrorList[0].Message.ShouldNotBeNullOrEmpty();
                File.Exists(result.ErrorList[0].File).ShouldBeTrue();
            }
        }
    }
}