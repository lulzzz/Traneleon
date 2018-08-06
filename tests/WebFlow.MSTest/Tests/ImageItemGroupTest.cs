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
    public class ImageItemGroupTest
    {
        [DataTestMethod]
        [DataRow(true, "/root/src/photo.jpg")]
        [DataRow(true, "/root/app/src/bg.jpg")]
        /* ===== */
        [DataRow(false, "/root/src/_photo.jpg")]
        [DataRow(false, "/root/src/photo.tiff")]
        [DataRow(false, "/root/src/photo.min.jpg")]
        public void CanAccept_should_accept_image_files(bool accept, string filePath)
        {
            // Arrange
            var sut = new ImageItemGroup
            {
                Suffix = ".min",
                WorkingDirectory = TestFile.DirectoryName,
                Exclude = new List<string> { "_*.*" },
                OptimizationTargets = new List<ImageItemGroup.OptimizationBundle>()
                {
                    new ImageItemGroup.OptimizationBundle()
                    {
                        Include = new List<string>() { "*.jpg" }
                    }
                }
            };

            // Act & Assert
            sut.CanAccept(filePath).ShouldBe(accept, Path.GetFileName(filePath));
        }

        [TestMethod]
        public void CreateCompilerOptions_should_return_valid_image_compression_options()
        {
            // Arrange
            var sut = new ImageItemGroup
            {
                Enabled = true,
                Suffix = ".min",
                WorkingDirectory = Path.GetDirectoryName(TestFile.DirectoryName),
                OptimizationTargets = new List<ImageItemGroup.OptimizationBundle>()
                {
                    new ImageItemGroup.OptimizationBundle()
                    {
                        Include = new List<string>() { "*.jpg" }
                    }
                }
            };

            // Act
            var sample = TestFile.GetImg8JPG().FullName;
            var list = sut.CreateCompilerOptions(sample).ToList();
            var optimizeResult = ((ImageOptimizerOptions)list[0]);

            // Assert
            list.ShouldNotBeEmpty();
            optimizeResult.OutputFile.Contains(Path.GetDirectoryName(sample));
        }

        //[TestMethod]
        public void CreateCompilerOptions_should_return_valid_image_size_options()
        {
            // Arrange
            var sut = new ImageItemGroup
            {
                Enabled = true,
                Suffix = ".min",
                WorkingDirectory = Path.GetDirectoryName(TestFile.DirectoryName)
            };

            // Act
            var sample = TestFile.GetImg8JPG().FullName;

            // Assert
        }
    }
}