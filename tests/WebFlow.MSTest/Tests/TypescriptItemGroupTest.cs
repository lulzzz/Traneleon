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
                WorkingDirectory = SampleProject.DirectoryName,
                Include = new List<TypescriptItemGroup.Bundle>
                {
                    new TypescriptItemGroup.Bundle()
                    {
                        Patterns = new List<string> { "wwwroot/**/*.ts" },
                        EntryPoint = $"wwwroot/scripts/build.js"
                    }
                }
            };

            var outFile = Path.Combine(SampleProject.DirectoryName, sut.Include[0].EntryPoint);
            var sourceFiles = new[] { SampleProject.GetAppTS().FullName, SampleProject.GetButtonTS().FullName };

            // Act
            var case1 = (TranspilierSettings)sut.CreateCompilerOptions(sourceFiles[0]);
            var case2 = (TranspilierSettings)sut.CreateCompilerOptions(sourceFiles[1]);

            sut.Include[0].EntryPoint = null;
            var case3 = (TranspilierSettings)sut.CreateCompilerOptions(sourceFiles[0]);

            sut.Include[0].EntryPoint = "Shared/_Layout.cshtml";
            var case4 = (TranspilierSettings)sut.CreateCompilerOptions(sourceFiles[0]);

            // Assert
            case1.OutputFile.ShouldBe(outFile);
            case1.ShouldBundleFiles.ShouldBeTrue();
            case1.SourceFiles.ShouldBeSubsetOf(sourceFiles);

            case2.OutputFile.ShouldBe(outFile);
            case2.ShouldBundleFiles.ShouldBeTrue();
            case2.SourceFiles.ShouldBeSubsetOf(sourceFiles);

            case3.SourceFiles.Length.ShouldBe(1);
            case3.ShouldBundleFiles.ShouldBeFalse();
            case3.SourceFiles.ShouldContain(sourceFiles[0]);
            case3.OutputFile.ShouldBe(Path.ChangeExtension(sourceFiles[0], ".js"));

            case4.SourceFiles.Length.ShouldBe(1);
            case4.ShouldBundleFiles.ShouldBeTrue();
            case4.SourceFiles.ShouldBeSubsetOf(sourceFiles);
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
                    new TypescriptItemGroup.Bundle("*.ts") { EntryPoint = "Views/Shared/_Layout.*html" }
                }
            };

            // Act
            var entryPoint = sut.EnumerateFiles().Select(x => Path.GetFileName(x)).ToList();

            sut.Include[0].EntryPoint = string.Empty;
            var allFiles = sut.EnumerateFiles().Select(x => Path.GetFileName(x)).ToList();

            // Assert
            allFiles.ShouldNotBeEmpty();
            entryPoint.ShouldNotBeEmpty();

            entryPoint.ShouldContain("app.ts");
            allFiles.ShouldContain("app.ts", "button.ts");

            entryPoint.Count.ShouldBe(1);
            allFiles.Count.ShouldBeGreaterThanOrEqualTo(2);
        }
    }
}