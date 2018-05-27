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
                        EntryPoint = $"Shared/_Layout.*html"
                    }
                }
            };

            var outFile = SampleProject.GetAppTS().FullName;
            var btn = SampleProject.GetButtonTS().FullName;

            // Act
            var case1 = (TranspilierSettings)sut.CreateCompilerOptions(outFile);
            var case2 = (TranspilierSettings)sut.CreateCompilerOptions(btn);

            sut.Include[0].EntryPoint = null;
            var case3 = (TranspilierSettings)sut.CreateCompilerOptions(btn);

            // Assert
            case1.SourceFile.ShouldBe(outFile);
            case2.SourceFile.ShouldBe(outFile);
            case3.SourceFile.ShouldBe(btn);

            case2.OutputDirectory.ShouldBe(Path.GetDirectoryName(outFile));
            case2.SourceMapDirectory.ShouldBe(Path.GetDirectoryName(outFile));
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
            entryPoint.ShouldNotBeEmpty();
            allFiles.ShouldNotBeEmpty();

            entryPoint.ShouldContain("app.ts");
            allFiles.ShouldContain("app.ts", "button.ts");

            entryPoint.Count.ShouldBe(1);
            allFiles.Count.ShouldBeGreaterThanOrEqualTo(2);
        }
    }
}