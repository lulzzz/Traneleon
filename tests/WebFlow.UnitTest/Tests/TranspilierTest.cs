using Acklann.WebFlow.Compilation;
using Acklann.WebFlow.Compilation.Configuration;
using ApprovalTests;
using ApprovalTests.Reporters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.IO;
using System.Linq;

namespace Acklann.WebFlow.Tests
{
    [TestClass]
    [UseReporter(typeof(BeyondCompare4Reporter), typeof(ClipboardReporter))]
    public class TranspilierTest
    {
        private static readonly string ResultDirectory = Path.Combine(Path.GetTempPath(), nameof(TranspilierTest));

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            if (Directory.Exists(ResultDirectory)) Directory.Delete(ResultDirectory, recursive: true);
            Directory.CreateDirectory(ResultDirectory);
        }

        [TestMethod]
        public void TypescriptCompiler_should_compile_ts_files()
        {
            RunTest<TypescriptCompiler>(TestFile.GetScript1TS(), ".js");
        }

        [TestMethod]
        public void SassCompiler_should_compile_scss_files()
        {
            RunTest<SassCompiler>(TestFile.GetStyle1SCSS(), ".css");
        }

        private static void RunTest<T>(FileInfo sourceFile, string newExtension) where T : ICompiler
        {
            // Arrange
            var sut = (ICompiler)Activator.CreateInstance(typeof(T));

            string outputDir = Path.Combine(ResultDirectory, "case1");
            var case1 = new TranspilierSettings(sourceFile.FullName, outputDir, outputDir, ".min", false, false);

            outputDir = Path.Combine(ResultDirectory, "case2");
            var case2 = new TranspilierSettings(sourceFile.FullName, outputDir, outputDir, ".min", true, true);

            // Act
            var canExecute = sut.CanExecute(case1);
            var result1 = (TranspilierResult)sut.Execute(case1);
            var contents = File.ReadAllText(result1.CompiliedFile);
            var compiltedSet1 = Directory.GetFiles(case1.OutputDirectory).Select((x) => Path.GetFileName(x)).ToArray();

            var result2 = (TranspilierResult)sut.Execute(case2);
            var compiltedSet2 = Directory.GetFiles(case1.OutputDirectory).Select((x) => Path.GetFileName(x)).ToArray();

            // Assert
            canExecute.ShouldBeTrue();

            result1.Succeeded.ShouldBeTrue();
            result2.Succeeded.ShouldBeTrue();

            compiltedSet1.Length.ShouldBe(1);
            compiltedSet1.ShouldAllBe(x => !x.EndsWith($"{newExtension}.map"));

            compiltedSet2.Length.ShouldBe(4);
            compiltedSet2.ShouldContain($"{Path.GetFileNameWithoutExtension(sourceFile.Name)}.min{newExtension}");

            Approvals.Verify(contents);
        }
    }
}