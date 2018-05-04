using Acklann.WebFlow.Transpilation;
using Acklann.WebFlow.Transpilation.Configuration;
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
    public class CompilerTest : TestKit
    {
        [TestMethod]
        public void HandleMesage_should_process_specified_file()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var settings = Mock.Create<SettingsBase>();

            var mockOperator = Mock.Create<IFileOperator>();
            mockOperator.Arrange((x) => x.Execute(settings))
                .Returns(ResultFile.CreateFromMinifiedFile(tempFile))
                .OccursOnce();
            mockOperator.Arrange((x) => x.CanExecute(settings))
                .Returns(true)
                .OccursOnce();

            var mockSelector = Mock.Create<IFileOperatorSelector>();
            mockSelector.Arrange((x) => x.GetOperatorTypesThatSupports(settings))
                .Returns(new Type[] { typeof(IFileOperator) })
                .OccursOnce();
            mockSelector.Arrange((x) => x.CreateInstance(Arg.IsAny<Type>()))
                .Returns(mockOperator)
                .OccursOnce();

            var sut = ActorOfAsTestActorRef<Compiler>(Props.Create<Compiler>(mockSelector));

            // Act
            sut.Tell(settings);
            if (File.Exists(tempFile)) File.Delete(tempFile);

            // Assert
            ExpectMsg<ResultFile>();
            mockOperator.AssertAll();
            mockSelector.AssertAll();
        }
    }
}