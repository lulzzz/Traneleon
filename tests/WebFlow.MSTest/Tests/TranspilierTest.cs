using Acklann.WebFlow.Compilation;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            RunCompileTest<TypescriptCompiler>(TestFile.GetScript1TS());
        }

        [TestMethod]
        public void TypescriptCompiler_can_report_errors()
        {
            RunErrorTest<TypescriptCompiler>(TestFile.GetBadScript1TS(), 1);
        }

        [TestMethod]
        public void SassCompiler_can_compile_scss_files()
        {
            RunCompileTest<SassCompiler>(TestFile.GetStyle1SCSS());
        }

        [TestMethod]
        public void SassCompiler_can_report_errors()
        {
            RunErrorTest<SassCompiler>(TestFile.GetBadStyle1SCSS(), 2);
        }

        private static void RunCompileTest<T>(FileInfo sourceFile) where T : ICompiler
        {
            string folder(string name) => Path.Combine(ResultDirectory, name);

            // Arrange
            using (var sut = (ICompiler)Activator.CreateInstance(typeof(T)))
            {
                var cases = new(int, TranspilierSettings)[]
                {
                    (1, new TranspilierSettings(sourceFile.FullName, folder("case1"), folder("case1"), ".min", false, false, false)),
                    (4, new TranspilierSettings(sourceFile.FullName, folder("case2"), folder("case2\\maps"), ".min", true, true, true))
                };

                // Act
                foreach (var (expectedFiles, options) in cases)
                {
                    var canExecute = sut.CanExecute(options);
                    var result = (TranspilierResult)sut.Execute(options);
                    var generatedFiles = Directory.GetFiles(options.OutputDirectory, "*", SearchOption.AllDirectories)
                        .Select(x => Path.GetFileName(x)).ToArray();

                    // Assert
                    canExecute.ShouldBeTrue();
                    result.Succeeded.ShouldBeTrue();
                    generatedFiles.Length.ShouldBe(expectedFiles, string.Join(" + ", generatedFiles));

                    using (ApprovalResults.ForScenario(Path.GetFileName(options.OutputDirectory)))
                    {
                        var contents = new StringBuilder();
                        contents.AppendLine(File.ReadAllText(result.CompiliedFile));
                        if (options.GenerateSourceMaps)
                        {
                            contents.AppendLine("==================================================");
                            contents.AppendLine(File.ReadAllText(Path.Combine(options.SourceMapDirectory, (Path.GetFileName(result.CompiliedFile) + ".map"))));
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
                var options = new TranspilierSettings(sourceFile.FullName, ResultDirectory);

                // Act
                var result = (TranspilierResult)sut.Execute(options);

                // Assert
                result.Succeeded.ShouldBeFalse();
                result.ErrorList.ShouldNotBeEmpty();
                result.ErrorList[0].LineNumber.ShouldBe(errorLine);
                result.ErrorList[0].Message.ShouldNotBeNullOrEmpty();
                File.Exists(result.ErrorList[0].File).ShouldBeTrue();
            }
        }
    }
}