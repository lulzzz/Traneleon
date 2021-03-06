﻿using Acklann.Traneleon.Compilation;
using Acklann.Traneleon.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Acklann.Traneleon.Tests
{
    [TestClass]
    public class ProjectMonitorTest
    {
        [TestMethod]
        public void Start_should_monitor_project_files_for_changes()
        {
            // Arrange
            var cwd = Path.Combine(Path.GetTempPath(), $"{nameof(ProjectMonitorTest)}_watch".ToLower());

            var sample1 = Path.Combine(cwd, "style1.scss");
            var sample2 = Path.Combine(cwd, "style2.scss");

            var config = new Project(Path.Combine(cwd, "webflow-compiler.config"))
            {
                SassItemGroup = new SassItemGroup()
                {
                    Enabled = true,
                    Suffix = ".min",
                    GenerateSourceMaps = true,
                    KeepIntermediateFiles = true,
                    Include = new List<string> { "*.scss" },
                }
            };
            config.AssignDefaults();

            int calls = 0, expectedCalls = 2;

            void after(ProgressToken token, string path) { calls++; }
            Action waitForFilesToBeGenerated = () => { do { System.Threading.Thread.Sleep(500); } while (calls <= expectedCalls); };

            var sut = new ProjectMonitor(null, null, after);

            // Act
            if (Directory.Exists(cwd)) Directory.Delete(cwd, recursive: true);
            Directory.CreateDirectory(cwd);

            using (sut)
            {
                config.Save();
                TestFile.GetStyle1SCSS().CopyTo(sample2);
                TestFile.GetBadStyle1SCSS().CopyTo(sample1);
                sut.Start(config.FullName);

                var temp = Path.Combine(cwd, "afl94afz.tmp");
                File.WriteAllText(temp, ".active { border: 1px solid red; }");
                File.Delete(sample1);
                File.Move(temp, sample1);
                File.WriteAllText(sample2, ".hide { display: none; }");
                Should.CompleteIn(waitForFilesToBeGenerated, TimeSpan.FromSeconds(30));
            }
            var files = Directory.GetFiles(cwd).Select((x) => Path.GetFileName(x)).ToList();

            // Assert
            files.Count.ShouldBe((expectedCalls * 5) + 1);
            files.ShouldContain((x) => x.EndsWith(".min.css"));
            files.ShouldContain((x) => x.EndsWith(".map"));
            calls.ShouldBeGreaterThanOrEqualTo(expectedCalls);
        }

        [TestMethod]
        public void Compile_should_process_all_project_files()
        {
            // Arrange
            var cwd = SampleProject.DirectoryName;
            var outDir = Path.Combine(Path.GetTempPath(), $"{nameof(ProjectMonitorTest)}_Compile");
            var config = new Project(Path.Combine(cwd, "webflow-compiler.config"))
            {
                OutputDirectory = outDir,
                SassItemGroup = new SassItemGroup()
                {
                    Enabled = true,
                    KeepIntermediateFiles = false,
                    Include = new List<string> { "*.scss" }
                },
                TypescriptItemGroup = new TypescriptItemGroup()
                {
                    Enabled = true,
                    KeepIntermediateFiles = false,
                    Include = new List<TypescriptItemGroup.Bundle>
                    {
                        new TypescriptItemGroup.Bundle("*.ts") { OutputFile = "build.ts" }
                    }
                }
            };
            config.AssignDefaults();

            var expectedCalls = 1;
            var sut = new ProjectMonitor(null, null, (a, b) => { expectedCalls--; });

            // Act
            if (Directory.Exists(outDir)) Directory.Delete(outDir, recursive: true);

            using (sut)
            {
                Should.CompleteIn(() =>
                {
                    sut.Compile(config);
                    while (expectedCalls > 0) System.Threading.Thread.Sleep(500);
                }, TimeSpan.FromSeconds(15));
            }
            var filesGenerated = Directory.GetFiles(outDir, "*min*");

            // Assert
            filesGenerated.ShouldAllBe(x => x.Contains(".min"));
        }
    }
}