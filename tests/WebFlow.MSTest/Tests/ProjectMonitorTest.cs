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
            var cwd = Path.Combine(Path.GetTempPath(), nameof(ProjectMonitorTest));
            if (Directory.Exists(cwd)) Directory.Delete(cwd, recursive: true);
            Directory.CreateDirectory(cwd);

            int totalExpectedFiles = (2 * 5) + 1;
            var sample1 = Path.Combine(cwd, "style1.scss");
            var sample2 = Path.Combine(cwd, "style2.scss");

            var config = new Project(Path.Combine(cwd, "dev.config"))
            {
                SassItemGroup = new SassItemGroup()
                {
                    Enabled = true,
                    WorkingDirectory = cwd,
                    GenerateSourceMaps = true,
                    KeepIntermediateFiles = true,
                    Include = new List<string> { "*.scss" },
                }
            };

            var observer = Mock.Create<IObserver<ICompilierResult>>();
            observer.Arrange((x) => x.OnNext(Arg.IsAny<ICompilierResult>()))
                .DoInstead<ICompilierResult>((x) => { System.Diagnostics.Debug.WriteLine($"Transpiled: {Path.GetFileName(x.OutputFile)}"); })
                .OccursAtLeast(1);

            var sut = new MockProjectMonitor(observer);

            // Act
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
                Should.CompleteIn(() => sut.WaitForCompletion(totalExpectedFiles), TimeSpan.FromSeconds(30));
            }
            var files = Directory.GetFiles(cwd).Select((x) => Path.GetFileName(x)).ToList();

            // Assert
            files.Count.ShouldBe(totalExpectedFiles);
            files.ShouldContain((x) => x.EndsWith(".min.css"));
            files.ShouldContain((x) => x.EndsWith(".map"));
            observer.AssertAll();
        }

        private class MockProjectMonitor : ProjectMonitor
        {
            public MockProjectMonitor(IObserver<ICompilierResult> observer) : base(observer)
            {
            }

            public volatile int ChangedEventsRaised = 0;

            public void WaitForCompletion(int totalExpectedFiles)
            {
                bool expectedFilesHasNotYetBeenGenerated = true;
                while (expectedFilesHasNotYetBeenGenerated)
                {
                    System.Threading.Thread.Sleep(500);
                    expectedFilesHasNotYetBeenGenerated = Directory.EnumerateFiles(DirectoryName, "*").Count() < totalExpectedFiles;
                }
            }

            protected override void OnFileWasModified(object sender, FileSystemEventArgs e)
            {
                //System.Diagnostics.Debug.WriteLine($"{e.ChangeType}: {e.Name}");
                base.OnFileWasModified(sender, e);
                ChangedEventsRaised++;
            }
        }
    }
}