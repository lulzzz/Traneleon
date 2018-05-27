using Acklann.WebFlow.Configuration;
using ApprovalTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Acklann.WebFlow.Tests
{
    [TestClass]
    public class ConfigurationExtensionsTest
    {
        [TestMethod]
        public void CompileAsync_should_transpile_all_project_files()
        {
            // Arrange
            var cwd = SampleProject.DirectoryName;
            var outDir = Path.Combine(Path.GetTempPath(), nameof(ConfigurationExtensionsTest));

            var config = new Project(Path.Combine(cwd, "mock.config"))
            {
                TypescriptItemGroup = new TypescriptItemGroup
                {
                    Suffix = ".min",
                    WorkingDirectory = cwd,
                    OutputDirectory = outDir,
                    GenerateSourceMaps = true,
                    KeepIntermediateFiles = true,
                    Include = new List<TypescriptItemGroup.Bundle>
                    {
                         new TypescriptItemGroup.Bundle("wwwroot/**/*.ts") { EntryPoint = "Views/Shared/_Layout.cshtml" },
                    }
                }
            };

            // Act
            if (Directory.Exists(outDir)) Directory.Delete(outDir, recursive: true);
            config.Save();
            var results = config.CompileAsync().Result;
            var files = Directory.GetFiles(outDir).Select(x => x.Remove(0, outDir.Length + 1)).ToArray();
            if (File.Exists(config.FullName)) File.Delete(config.FullName);

            // Assert
            results.ShouldNotBeEmpty();
            results.ShouldAllBe(x => x.ExecutionTime > 0);
            Approvals.VerifyAll(files, "file");
        }
    }
}