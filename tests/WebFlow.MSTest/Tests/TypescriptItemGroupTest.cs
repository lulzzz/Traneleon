using Acklann.WebFlow.Compilation;
using Acklann.WebFlow.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Acklann.WebFlow.Tests
{
    [TestClass]
    public class TypescriptItemGroupTest
    {
        [DataTestMethod]
        [DataRow(true, "/root/src/script.ts")]
        [DataRow(true, "/root/app/src/func.ts")]
        /* === */
        [DataRow(false, "/root/src/_util.ts")]
        [DataRow(false, "/root/test/index.ts")]
        [DataRow(false, "/root/src/index.vash")]
        public void CanAccept_should_accept_typescript_files(bool accept, string filePath)
        {
            // Arrange
            var sut = new TypescriptItemGroup
            {
                Suffix = ".min",
                Exclude = new List<string> { "_*.ts" },
                Include = new List<TypescriptItemGroup.Bundle>
                {
                    new TypescriptItemGroup.Bundle("src/**/*.ts")
                },
            };

            // Act & Assert
            sut.CanAccept(filePath).ShouldBe(accept, Path.GetFileName(filePath));
        }

        [TestMethod]
        public void CreateCompilerOptions_should_return_valid_tsc_options()
        {
            // Arrange
            var sut = new TypescriptItemGroup
            {
                Enabled = true,
                Suffix = ".min",
                WorkingDirectory = TestFile.DirectoryName,
                Include = new List<TypescriptItemGroup.Bundle>
                {
                    new TypescriptItemGroup.Bundle()
                    {
                        Patterns = new List<string> { "*.ts" }
                    }
                }
            };

            var sample1 = TestFile.GetScript1TS().FullName;
            var outDir = Path.Combine(Path.GetTempPath(), nameof(TypescriptItemGroupTest));

            // Act
            var single = sut.CreateCompilerOptions(sample1).First();

            sut.SourceMapDirectory = TestFile.DirectoryName;
            sut.Include[0].OutputFile = Path.Combine(outDir, "build.ts");
            var bundle = (TranspilierSettings)sut.CreateCompilerOptions(sample1).First();

            // Assert
            single.SourceFile.ShouldBe(sample1);
            ((TranspilierSettings)single).OutputDirectory.ShouldBe(Path.GetDirectoryName(sample1));
            ((TranspilierSettings)single).SourceMapDirectory.ShouldBe(Path.GetDirectoryName(sample1));
            ((TranspilierSettings)single).ShouldBundleFiles.ShouldBeFalse();

            bundle.SourceFiles.Length.ShouldBeGreaterThanOrEqualTo(2);
            bundle.SourceMapDirectory.ShouldBe(sut.SourceMapDirectory);
            bundle.OutputDirectory.ShouldBe(outDir);
            bundle.ShouldBundleFiles.ShouldBeTrue();
        }

        [TestMethod]
        public void EnumerateFiles_should_return_included_ts_files()
        {
            // Arrange
            var sut = new TypescriptItemGroup
            {
                Suffix = ".min",
                WorkingDirectory = SampleProject.DirectoryName,
                Include = new List<TypescriptItemGroup.Bundle>
                {
                    new TypescriptItemGroup.Bundle("*.ts") { OutputFile = "wwwroot/scripts/build.ts" }
                }
            };

            // Act
            var outFile = sut.EnumerateFiles().Select(x => Path.GetFileName(x)).ToList();

            sut.Include[0].OutputFile = string.Empty;
            var allFiles = sut.EnumerateFiles().Select(x => Path.GetFileName(x)).ToList();

            // Assert
            allFiles.ShouldNotBeEmpty();
            allFiles.ShouldContain("app.ts", "button.ts");
            allFiles.Count.ShouldBeGreaterThanOrEqualTo(2);

            outFile.Count.ShouldBe(1);
            outFile.ShouldBeSubsetOf(new[] { "build.ts" });
        }
    }
}