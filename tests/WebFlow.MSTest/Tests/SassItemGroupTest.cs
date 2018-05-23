using Acklann.WebFlow.Compilation;
using Acklann.WebFlow.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Collections.Generic;
using System.IO;

namespace Acklann.WebFlow.Tests
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
                Suffix = ".min",
                WorkingDirectory = Path.GetDirectoryName(TestFile.DirectoryName),
                Include = new List<string> { "*.sass" },
            };

            var sample = TestFile.GetStyle1SCSS().FullName;

            // Act
            var result = (TranspilierSettings)sut.CreateCompilerOptions(sample);
            var empty = sut.CreateCompilerOptions(Path.Combine(TestFile.DirectoryName, "_base.scss"));

            // Assert
            empty.ShouldBeOfType<NullCompilerOptions>();

            result.ShouldBeOfType<TranspilierSettings>();
            result.SourceFile.ShouldBe(sample);
            result.OutputDirectory.ShouldBe(TestFile.DirectoryName);
        }
    }
}