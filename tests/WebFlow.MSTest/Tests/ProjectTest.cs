using Acklann.WebFlow.Configuration;
using ApprovalTests;
using ApprovalTests.Reporters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace Acklann.WebFlow.Tests
{
    [TestClass]
    [UseReporter(typeof(BeyondCompare4Reporter))]
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

            bool xmlIsWellFormed; string errorMsg;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
            {
                xmlIsWellFormed = Project.Validate(stream, out errorMsg);
            }
            System.Diagnostics.Debug.WriteLine(contents);

            // Assert
            sut.FullName.ShouldBe(path);
            xmlIsWellFormed.ShouldBeTrue(errorMsg);
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
            var sample = new Project
            {
                Name = "test",
                SassItemGroup = new SassItemGroup()
                {
                    Enabled = true,
                    Suffix = ".min",
                    Exclude = new List<string>() { "_*.scss" },
                    Include = new List<string>()
                    {
                        "*.scss"
                    }
                },
                TypescriptItemGroup = new TypescriptItemGroup()
                {
                    Enabled = true,
                    Suffix = ".min",
                    GenerateSourceMaps = true,
                    KeepIntermediateFiles = false,
                    Exclude = new List<string> { "_*.ts" },
                    Include = new List<TypescriptItemGroup.Bundle>
                    {
                        new TypescriptItemGroup.Bundle("**/views/*.ts"),
                        new TypescriptItemGroup.Bundle("*.ts") { OutputFile = "index.min.js"}
                    }
                },
                ImageItemGroup = new ImageItemGroup()
                {
                    Enabled = true,
                    Suffix = ".min",
                    OptimizationTargets = new List<ImageItemGroup.OptimizationBundle>()
                    {
                        new ImageItemGroup.OptimizationBundle()
                        {
                            Include = new List<string>() { "*.jpg" }
                        },
                        new ImageItemGroup.OptimizationBundle()
                        {
                            Quality = 50,
                            Kind = CompressionKind.Lossy,
                            Include = new List<string>() { "*.png" }
                        }
                    }
                }
            };

            return sample;
        }

        private static void RunDeserializationTest(string sampleFile)
        {
            var result = Project.Load(sampleFile);

            var properties = (from m in typeof(Project).GetProperties()
                              where m.IsDefined(typeof(XmlElementAttribute)) || m.IsDefined(typeof(XmlAttributeAttribute))
                              select m).ToArray();

            // Assert
            properties.ShouldAllBe(x => x.GetValue(result) != null);

            result.FullName.ShouldNotBeNullOrEmpty();
            result.TypescriptItemGroup.Include.Count.ShouldBe(1);
            result.ImageItemGroup.OptimizationTargets.Count.ShouldBeGreaterThan(1);
            result.ImageItemGroup.OptimizationTargets[0].Include.ShouldNotBeEmpty();
            result.TypescriptItemGroup.Include[0].OutputFile.ShouldNotBeNullOrEmpty();
        }
    }
}