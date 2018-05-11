using Acklann.WebFlow.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Collections.Generic;
using System.IO;
using Telerik.JustMock;

namespace Acklann.WebFlow.Tests
{
    [TestClass]
    public class ItemGroupTest
    {
        [DataTestMethod]
        [DataRow(true, "/root/src/script.js")]
        [DataRow(true, "/root/app/src/func.js")]
        /* === */
        [DataRow(false, "/root/src/_util.js")]
        [DataRow(false, "/root/src/script.ts")]
        public void CanAccept_should_accept_files_that_match_the_given_patterns(bool accept, string filePath)
        {
            // Arrange
            var sut = Mock.Create<ItemGroupBase>(Behavior.CallOriginal);
            sut.Suffix = ".min";
            sut.Include = new List<string> { "src/*.js" };
            sut.Exclude = new List<string> { "src/_*.js" };

            // Act & Assert
            sut.CanAccept(filePath).ShouldBe(accept, Path.GetFileName(filePath));
        }
    }
}