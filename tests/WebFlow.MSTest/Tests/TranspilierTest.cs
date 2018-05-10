using Acklann.WebFlow.Compilation;
using Acklann.WebFlow.Compilation.Configuration;
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
        public void BeforeEach()
        {
            if (Directory.Exists(ResultDirectory)) Directory.Delete(ResultDirectory, recursive: true);
            Directory.CreateDirectory(ResultDirectory);
        }

        [TestMethod]
        public void TypescriptCompiler_can_compile_ts_files()
        {
            RunTest<TypescriptCompiler>(TestFile.GetScript1TS());
        }

        [TestMethod]
        public void SassCompiler_can_compile_scss_files()
        {
            RunTest<SassCompiler>(TestFile.GetStyle1SCSS());
        }

        private static void RunTest<T>(FileInfo sourceFile) where T : ICompiler
        {
            string folder(string name) => Path.Combine(ResultDirectory, name);

            // Arrange
            var sut = (ICompiler)Activator.CreateInstance(typeof(T));

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
}