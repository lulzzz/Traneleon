using ApprovalTests;
using ApprovalTests.Reporters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace Acklann.WebFlow.Tests
{
    [TestClass]
    [UseReporter(typeof(BeyondCompare4Reporter), typeof(ClipboardReporter))]
    public class ProjectTest
    {
        [TestMethod]
        public void Load_can_deserialize_instance_from_xml()
        {
            RunDeserializationTest(TestFile.GetGoodConfigXML().FullName);
        }

        [TestMethod]
        public void Load_can_deserialize_instance_from_json()
        {
            RunDeserializationTest(TestFile.GetGoodConfigJSON().FullName);
        }

        [TestMethod]
        public void Save_can_serialize_instance_to_xml()
        {
            // Arrange
            var sut = CreateSample();
            var path = Path.Combine(Path.GetTempPath(), "webflow.xml");

            // Act
            sut.Save(path);
            var contents = File.ReadAllText(path);
            if (File.Exists(path)) File.Delete(path);

            // Assert
            Approvals.Verify(contents);
        }

        [TestMethod]
        public void Save_can_serialize_instance_to_json()
        {
            // Arrange
            var sut = CreateSample();
            var path = Path.Combine(Path.GetTempPath(), "webflow.json");

            // Act
            sut.Save(path);
            var contents = File.ReadAllText(path);
            if (File.Exists(path)) File.Delete(path);

            // Assert
            Approvals.Verify(contents);
        }

        private static Project CreateSample()
        {
            var sample = new Project();

            return sample;
        }

        private static void RunDeserializationTest(string sampleFile)
        {
            var result = Project.Load(sampleFile);

            var properties = (from m in typeof(Project).GetProperties()
                              where m.IsDefined(typeof(XmlElementAttribute)) || m.IsDefined(typeof(XmlAttributeAttribute))
                              select m.GetValue(result)).ToArray();

            // Assert
            result.FullName.ShouldNotBeNullOrEmpty();
            properties.ShouldAllBe(x => x != null);
        }
    }
}