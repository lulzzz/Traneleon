using Acklann.WebFlow.Commands;
using Acklann.WebFlow.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Collections.Generic;
using System.IO;

namespace Acklann.WebFlow.Tests
{
    [TestClass]
    public class CompileCommandTest
    {
        [TestMethod]
        public void Execute_should_compile_all_within_directory()
        {
            // Arrange
            var outDir = Path.Combine(Path.GetTempPath(), nameof(CompileCommandTest));
            var config = new Project(Path.Combine(SampleProject.DirectoryName, "case1.config"))
            {
                TypescriptItemGroup = new TypescriptItemGroup
                {
                    Enabled = true,
                    Suffix = ".min",
                    OutputDirectory = outDir,
                    GenerateSourceMaps = true,
                    KeepIntermediateFiles = true,
                    Include = new List<TypescriptItemGroup.Bundle>
                    {
                        new TypescriptItemGroup.Bundle("*.ts") { OutFile = "Shared/_Layout.cshtml"}
                    }
                }
            };

            // Act
            config.Save();
            var sut = new CompileCommand(config.FullName, false);

            if (Directory.Exists(outDir)) Directory.Delete(outDir, recursive: true);
            var exitCode = sut.Execute();
            File.Delete(config.FullName);

            var files = Directory.GetFiles(outDir);

            // Assert
            exitCode.ShouldBe(0);
            files.ShouldNotBeEmpty();
        }
    }
}