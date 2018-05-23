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
                WorkingDirectory = Path.GetDirectoryName(TestFile.DirectoryName),
                Include = new List<TypescriptItemGroup.Bundle>
                {
                    new TypescriptItemGroup.Bundle()
                    {
                        Patterns = new List<string> { "*.ts" },
                        EntryPoint = $"{TestFile.FOLDER_NAME}\\{TestFile.GetEntry1HTML().Name}"
                    }
                }
            };

            var sample = TestFile.GetScript2TS().FullName;

            // Act
            var html = (TranspilierSettings)sut.CreateCompilerOptions(sample);

            var tmp = sut.Include[0];
            tmp.EntryPoint = string.Empty;
            var plain = (TranspilierSettings)sut.CreateCompilerOptions(sample);

            // Assert
            plain.SourceFile.ShouldBe(sample);
            html.SourceFile.ShouldBe(TestFile.GetEntrypoint1TS().FullName, StringCompareShould.IgnoreCase);
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

            allFiles.Count.ShouldBe(2);
            entryPoint.Count.ShouldBe(1);
        }
    }
}