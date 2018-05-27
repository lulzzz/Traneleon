using Acklann.WebFlow.Commands;
using Acklann.WebFlow.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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
                        new TypescriptItemGroup.Bundle("*.ts"){ EntryPoint = "Shared/_Layout.cshtml"}
                    }
                }
            };

            var sut = new CompileCommand(config.FullName, false);

            // Act
            config.Save();
            if (Directory.Exists(outDir)) Directory.Delete(outDir, recursive: true);
            var exitCode = sut.Execute();
            File.Delete(config.FullName);

            var files = Directory.GetFiles(outDir);

            // Assert
            exitCode.ShouldBe(0);
            files.ShouldNotBeEmpty();
        }

        //[TestMethod]
        public void Execute_should_watch_for_file_changes()
        {
            // Arrange
            int exitCode = -1;
            var outDir = Path.Combine(Path.GetTempPath(), $"{nameof(CompileCommandTest)}2");
            string temp = Path.Combine(SampleProject.DirectoryName, "wwwroot", "scripts", "temp.ts");

            var config = new Project(Path.Combine(SampleProject.DirectoryName, "fake2.config"))
            {
                TypescriptItemGroup = new TypescriptItemGroup
                {
                    Enabled = true,
                    Suffix = ".min",
                    OutputDirectory = outDir,
                    GenerateSourceMaps = true,
                    KeepIntermediateFiles = true,
                    WorkingDirectory = SampleProject.DirectoryName,
                    Include = new List<TypescriptItemGroup.Bundle>
                    {
                        new TypescriptItemGroup.Bundle("*.ts") { EntryPoint = SampleProject.GetAppTS().FullName }
                    }
                }
            };

            var sut = new CompileCommand(config.FullName, true);

            // Act
            config.Save();
            if (Directory.Exists(outDir)) Directory.Delete(outDir, recursive: true);
            Task.Run(async () =>
            {
                long start = DateTime.UtcNow.Ticks;
                await Task.Delay(TimeSpan.FromSeconds(3));
                File.WriteAllText(temp, "console.log('foo bar');");
                sut.StopWatcher();
                System.Diagnostics.Debug.WriteLine($"waited for {TimeSpan.FromTicks(DateTime.UtcNow.Ticks - start)}");
            });
            exitCode = sut.Execute();
            var files = Directory.GetFiles(outDir);

            File.Delete(config.FullName);
            if (File.Exists(temp)) File.Delete(temp);

            // Assert
            exitCode.ShouldBe(0);
            files.ShouldNotBeEmpty();
        }
    }
}