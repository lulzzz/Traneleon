using Acklann.Traneleon.Compilation;
using ApprovalTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Collections.Generic;
using System.IO;

namespace Acklann.Traneleon.Tests
{
    [TestClass]
    public class ImageOptimizerTest
    {
        [TestMethod]
        public void Execute_should_compress_supported_images()
        {
            // Arrange
            var sut = new ImageOptimizer();
            var fileList = new Stack<string>();
            string outDir = Path.Combine(Path.GetTempPath(), nameof(ImageOptimizerTest));

            var samples = new[]
            {
                TestFile.GetImg10PNG().FullName, TestFile.GetImg8JPG().FullName,
                TestFile.GetImg1aGIF().FullName
            };

            // Act
            if (Directory.Exists(outDir)) Directory.Delete(outDir, recursive: true);

            using (sut)
            {
                foreach (string srcFile in samples)
                {
                    string outFile = Path.Combine(outDir, "lossless", $"{Path.GetFileNameWithoutExtension(srcFile)}.min{Path.GetExtension(srcFile)}");
                    var result = sut.Execute(new ImageOptimizerOptions(Configuration.CompressionKind.LossLess, srcFile, outFile));
                    File.Copy(srcFile, Path.Combine(Path.GetDirectoryName(outFile), Path.GetFileName(srcFile)));
                    fileList.Push(result.OutputFile.Replace(Path.GetTempPath(), ""));

                    outFile = Path.Combine(outDir, "lossy", $"{Path.GetFileNameWithoutExtension(srcFile)}.min{Path.GetExtension(srcFile)}");
                    result = sut.Execute(new ImageOptimizerOptions(Configuration.CompressionKind.Lossy, srcFile, outFile));
                    File.Copy(srcFile, Path.Combine(Path.GetDirectoryName(outFile), Path.GetFileName(srcFile)));
                    fileList.Push(result.OutputFile.Replace(Path.GetTempPath(), ""));
                }
            }

            // Assert
            fileList.Count.ShouldBe(samples.Length * 2);
            Approvals.VerifyAll(fileList, "file");
        }
    }
}