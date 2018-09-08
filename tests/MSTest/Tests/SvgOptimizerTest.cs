using Acklann.Traneleon.Compilation;
using ApprovalTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Collections.Generic;
using System.IO;

namespace Acklann.Traneleon.Tests
{
    [TestClass]
    public class SvgOptimizerTest
    {
        [TestMethod]
        public void Execute_should_compress_svg_files()
        {
            // Arrange
            var sut = new SvgOptimizer();
            var outDir = Path.Combine(Path.GetTempPath(), nameof(SvgOptimizerTest));

            // Act
            if (Directory.Exists(outDir)) Directory.Delete(outDir, recursive: true);

            using (sut)
            {
                var srcFile = TestFile.GetImg7SVG();
                var outFile = Path.Combine(outDir, $"{Path.GetFileNameWithoutExtension(srcFile.FullName)}.min{Path.GetExtension(srcFile.FullName)}");
                var result = sut.Execute(new ImageOptimizerOptions(Configuration.CompressionKind.LossLess, srcFile.FullName, outFile));
                var fileWasOpitimized = (new FileInfo(outFile).Length < srcFile.Length);

                // Assert
                result.Succeeded.ShouldBeTrue();
                fileWasOpitimized.ShouldBeTrue();
            }
        }
    }
}