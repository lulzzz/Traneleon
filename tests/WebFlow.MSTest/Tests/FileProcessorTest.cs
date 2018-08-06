using Acklann.WebFlow.Compilation;
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
        public void FileProcessor_should_handle_compilationOptions_messages()
        {
            // Arrange
            int occurences = 1;

            var filePath = Path.Combine(Path.GetTempPath(), "file.js");
            var settings = new TranspilierSettings(filePath, Path.ChangeExtension(filePath, ".ts"));
            var mockResult = Mock.Create<ICompilierResult>();

            var mockOperator = Mock.Create<ICompiler>();
            mockOperator.Arrange((x) => x.Execute(settings))
                .Returns(mockResult)
                .Occurs(occurences);
            mockOperator.Arrange((x) => x.CanExecute(settings))
                .Returns(true)
                .Occurs(occurences);

            var mockSelector = Mock.Create<ICompilerFactory>();
            mockSelector.Arrange((x) => x.GetCompilerTypesThatSupports(settings))
                .Returns(new Type[] { typeof(ICompiler) })
                .Occurs(occurences);
            mockSelector.Arrange((x) => x.CreateInstance(Arg.IsAny<Type>()))
                .Returns(mockOperator)
                .Occurs(occurences);

            var mockReporter = Mock.Create<IProgress<ProgressToken>>();
            mockReporter.Arrange((x) => x.Report(Arg.IsAny<ProgressToken>()))
                .Occurs(occurences);

            var sut = ActorOfAsTestActorRef<FileProcessor>(Props.Create<FileProcessor>(mockReporter, mockSelector));

            // Act
            for (int i = 0; i < occurences; i++) sut.Tell(settings);

            // Assert
            mockOperator.AssertAll();
            mockSelector.AssertAll();
            mockReporter.AssertAll();
        }
    }
}