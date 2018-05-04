using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Acklann.WebFlow
{
    [XmlRoot("project", Namespace = XMLNS)]
    public partial class Project
    {
        public Project()
        {
            _namespace = new XmlSerializerNamespaces(new XmlQualifiedName[] { new XmlQualifiedName(string.Empty, XMLNS) });
        }

        [XmlIgnore, JsonIgnore]
        public string FullName { get; internal set; }

        public static Project Load(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(Project));
            return (Project)serializer.Deserialize(stream);
        }

        public static Project Load(string filePath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException($"Cannot find file at '{filePath}'.");

            Project project;
            switch (Path.GetExtension(filePath).ToLowerInvariant())
            {
                default:
                case ".xml":
                    using (Stream file = File.OpenRead(filePath))
                    {
                        project = Load(file);
                    }
                    break;

                case ".json":
                    project = LoadJson(File.ReadAllText(filePath));
                    break;
            }

            project.FullName = filePath;
            return project;
        }

        public static Project LoadJson(string json)
        {
            return JsonConvert.DeserializeObject<Project>(json);
        }

        public static Project LoadXml(string xml)
        {
            var serializer = new XmlSerializer(typeof(Project));
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                return (Project)serializer.Deserialize(stream);
            }
        }

        public void Save() => Save(FullName);

        public void Save(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));

            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                switch (Path.GetExtension(filePath).ToLowerInvariant())
                {
                    default:
                    case ".xml":
                        Save(file);
                        break;

                    case ".json":
                        using (var writer = new StreamWriter(file))
                        {
                            string json = JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
                            writer.WriteLine(json);
                        }
                        break;
                }
            }
        }

        public void Save(Stream file)
        {
            using (var writer = XmlWriter.Create(file, new XmlWriterSettings() { Indent = true }))
            {
                var serializer = new XmlSerializer(typeof(Project));
                serializer.Serialize(file, this, _namespace);
            }
        }

        #region Private Members

        private readonly XmlSerializerNamespaces _namespace;

        #endregion Private Members
    }
}