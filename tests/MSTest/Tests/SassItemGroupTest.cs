using Acklann.Traneleon.Compilation;
using Acklann.Traneleon.Configuration;
using ApprovalTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Acklann.Traneleon.Tests
{
    [TestClass]
    public class SassItemGroupTest
    {
        [DataTestMethod]
        [DataRow(true, "/root/src/style.sass")]
        [DataRow(true, "/root/app/src/site.sass")]
        /* ===== */
        [DataRow(false, "/root/src/_base.sass")]
        [DataRow(false, "/root/test/index.sass")]
        [DataRow(false, "/root/src/pre_foo.sass")]
        [DataRow(false, "/root/src/style.min.sass")]
        public void CanAccept_should_accept_sass_files(bool accept, string filePath)
        {
            // Arrange
            var sut = new SassItemGroup
            {
                Suffix = ".min",
                WorkingDirectory = TestFile.DirectoryName,
                Include = new List<string> { "src/**/*.sass" },
                Exclude = new List<string> { "_*.sass", "pre_*.sass" }
            };

            // Act & Assert
            sut.CanAccept(filePath).ShouldBe(accept, Path.GetFileName(filePath));
        }

        [TestMethod]
        public void CreateCompilerOptions_should_return_valid_sass_options()
        {
            // Arrange
            var sut = new SassItemGroup
            {
                Enabled = true,
                Suffix = ".min",
                WorkingDirectory = Path.GetDirectoryName(TestFile.DirectoryName),
                Include = new List<string> { "*.sass" },
            };

            var sample = TestFile.GetStyle1SCSS().FullName;

            // Act
            var single = sut.CreateCompilerOptions(sample).ToList();
            var empty = sut.CreateCompilerOptions(TestFile.GetPartialSCSS().FullName).ToList();

            sut.WorkingDirectory = SampleProject.DirectoryName;
            var multiple = sut.CreateCompilerOptions(SampleProject.GetLayoutSCSS().FullName).Select(x=> x.SourceFile.Remove(0, SampleProject.DirectoryName.Length)).ToArray();

            // Assert
            empty.ShouldBeEmpty();

            single.Count.ShouldBe(1);
            (single[0]).SourceFile.ShouldBe(sample);
            ((TranspilierSettings)single[0]).OutputDirectory.ShouldBe(Path.GetDirectoryName(sample));
            ((TranspilierSettings)single[0]).SourceMapDirectory.ShouldBe(Path.GetDirectoryName(sample));
            ((TranspilierSettings)single[0]).OutputFile.ShouldBe(Path.ChangeExtension(sample, ".css"));

            Approvals.VerifyAll(multiple, "path");
        }
    }
}