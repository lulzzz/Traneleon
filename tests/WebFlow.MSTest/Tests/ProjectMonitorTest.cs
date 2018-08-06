using Acklann.WebFlow.Compilation;
using Acklann.WebFlow.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Acklann.WebFlow.Tests
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
            var reporter = Mock.Create<IProgress<ProgressToken>>();
            reporter.Arrange((x) => x.Report(Arg.IsAny<ProgressToken>()))
                .DoInstead<ProgressToken>((x) => { calls++; })
                .OccursAtLeast(expectedCalls);

            Action waitForFilesToBeGenerated = () => { do { System.Threading.Thread.Sleep(500); } while (calls <= expectedCalls); };

            var sut = new ProjectMonitor(reporter: reporter);

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
            reporter.AssertAll();
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
            var mockObserver = Mock.Create<IProgress<ProgressToken>>();
            mockObserver.Arrange(x => x.Report(Arg.IsAny<ProgressToken>()))
                .DoInstead(() => { expectedCalls--; })
                .OccursAtLeast(expectedCalls);

            var sut = new ProjectMonitor(reporter: mockObserver);

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
            mockObserver.AssertAll();
            filesGenerated.ShouldAllBe(x => x.Contains(".min"));
        }
    }
}