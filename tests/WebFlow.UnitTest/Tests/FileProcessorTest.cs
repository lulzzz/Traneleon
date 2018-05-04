using Acklann.WebFlow.Compilation;
using Acklann.WebFlow.Compilation.Configuration;
using Akka.Actor;
using Akka.TestKit.VsTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Acklann.WebFlow.Tests
{
    [TestClass]
    public class FileProcessorTest : TestKit
    {
        [TestMethod]
        public void HandleMesage_should_process_specified_file()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var settings = Mock.Create<ICompilierOptions>();
            var mockResult = Mock.Create<ICompilierResult>();

            var mockOperator = Mock.Create<ICompiler>();
            mockOperator.Arrange((x) => x.Execute(settings))
                .Returns(mockResult)
                .OccursOnce();
            mockOperator.Arrange((x) => x.CanExecute(settings))
                .Returns(true)
                .OccursOnce();

            var mockSelector = Mock.Create<ICompilerFactory>();
            mockSelector.Arrange((x) => x.GetOperatorTypesThatSupports(settings))
                .Returns(new Type[] { typeof(ICompiler) })
                .OccursOnce();
            mockSelector.Arrange((x) => x.CreateInstance(Arg.IsAny<Type>()))
                .Returns(mockOperator)
                .OccursOnce();

            var sut = ActorOfAsTestActorRef<FileProcessor>(Props.Create<FileProcessor>(mockSelector));

            // Act
            sut.Tell(settings);
            if (File.Exists(tempFile)) File.Delete(tempFile);

            // Assert
            ExpectMsg<ICompilierResult>();
            mockOperator.AssertAll();
            mockSelector.AssertAll();
        }
    }
}