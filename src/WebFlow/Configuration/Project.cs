using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Acklann.WebFlow.Configuration
{
    [XmlRoot("project", Namespace = XMLNS)]
    public partial class Project
    {
        public Project() : this(string.Empty)
        {
        }

        public Project(string filePath)
        {
            FullName = filePath;
            _namespace = new XmlSerializerNamespaces(new XmlQualifiedName[] { new XmlQualifiedName(string.Empty, XMLNS) });
        }

        [XmlIgnore, JsonIgnore]
        public string FullName { get; private set; }

        [XmlIgnore, JsonIgnore]
        public string DirectoryName
        {
            get { return Path.GetDirectoryName(FullName); }
        }

        [XmlAttribute("name")]
        public string Name
        {
            get { return string.IsNullOrEmpty(_name) ? Path.GetFileNameWithoutExtension(FullName) : _name; }
            set { _name = value; }
        }

        [XmlElement("sass")]
        [JsonProperty("sass")]
        public SassItemGroup SassItemGroup { get; set; }

        [XmlElement("typescript")]
        [JsonProperty("typescript")]
        public TypescriptItemGroup TypescriptItemGroup { get; set; }

        public static Project CreateInstance(string filePath)
        {
            return new Project(filePath)
            {
                SassItemGroup = new SassItemGroup(),
                TypescriptItemGroup = new TypescriptItemGroup()
            };
        }

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
            SetDefaults(project);
            return project;
        }

        public static Project LoadXml(string xml)
        {
            var serializer = new XmlSerializer(typeof(Project));
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                return (Project)serializer.Deserialize(stream);
            }
        }

        public static Project LoadJson(string json)
        {
            return JsonConvert.DeserializeObject<Project>(json);
        }

        public static bool TryLoad(string filePath, out Project project, out string error)
        {
            error = null;
            project = null;

            using (Stream file = File.OpenRead(filePath))
            {
                if (Validate(file, out error))
                {
                    file.Position = 0;
                    project = Load(file);
                    project.FullName = filePath;
                    SetDefaults(project);
                }
                else return false;
            }

            return true;
        }

        public static bool Validate(Stream stream, out string error)
        {
            var schema = new XmlSchemaSet();
            foreach (string path in Directory.EnumerateFiles(Path.Combine(Path.GetDirectoryName(typeof(Project).Assembly.Location)), $"{nameof(WebFlow)}.xsd", SearchOption.TopDirectoryOnly))
            {
                schema.Add(XMLNS, path);
            }

            var err = new StringBuilder();
            var doc = XDocument.Load(stream, LoadOptions.SetLineInfo);
            doc.Validate(schema, (sender, e) =>
            {
                err.AppendLine($"[{e.Severity}] {e.Message} at line {e.Exception.LineNumber}");
            });
            stream.Position = 0;
            error = err.ToString();

            return string.IsNullOrEmpty(error);
        }

        public static bool Validate(string filePath, out string error)
        {
            using (Stream file = File.OpenRead(filePath))
            {
                return Validate(file, out error);
            }
        }

        public static void SetDefaults(Project project)
        {
            foreach (IItemGroup itemGroup in project.GetItempGroups())
            {
                itemGroup.WorkingDirectory = project.DirectoryName;
            }
        }

        public void Save() => Save(FullName);

        public void Save(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));

            FullName = filePath;
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
                            string json = JsonConvert.SerializeObject(this, new JsonSerializerSettings()
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                Formatting = Newtonsoft.Json.Formatting.Indented,
                                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                            });
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
                serializer.Serialize(writer, this, _namespace);
            }
        }

        public IEnumerable<IItemGroup> GetItempGroups()
        {
            if (SassItemGroup != null) yield return SassItemGroup;
            if (TypescriptItemGroup != null) yield return TypescriptItemGroup;
        }

        #region Private Members

        private readonly XmlSerializerNamespaces _namespace;
        private string _name;

        #endregion Private Members
    }
}