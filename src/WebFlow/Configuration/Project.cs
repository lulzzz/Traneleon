using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Acklann.WebFlow.Configuration
{
    [System.Diagnostics.DebuggerDisplay("{ToDebuggerDisplay()}")]
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
        public string DirectoryName
        {
            get { return Path.GetDirectoryName(FullName); }
        }

        [XmlIgnore, JsonIgnore]
        public string FullName { get; private set; }

        [XmlAttribute("name")]
        [JsonProperty("name")]
        public string Name
        {
            get { return string.IsNullOrEmpty(_name) ? Path.GetFileName(Path.GetDirectoryName(FullName)) : _name; }
            set { _name = value; }
        }

        [XmlAttribute("outputDirectory")]
        [JsonProperty("outputDirectory")]
        public string OutputDirectory { get; set; }

        [XmlElement("sass")]
        [JsonProperty("sass")]
        public SassItemGroup SassItemGroup { get; set; }

        [XmlElement("image")]
        [JsonProperty("image")]
        public ImageItemGroup ImageItemGroup { get; set; }

        [XmlElement("typescript")]
        [JsonProperty("typescript")]
        public TypescriptItemGroup TypescriptItemGroup { get; set; }

        public static string FindConfigurationFile(string projectDirectory)
        {
            return (from file in Directory.EnumerateFiles(projectDirectory)
                    let basename = Path.GetFileNameWithoutExtension(file).ToLowerInvariant()
                    where
                        basename == "transpiler"
                        ||
                        basename.Contains(nameof(WebFlow).ToLowerInvariant())
                    select basename).FirstOrDefault();
        }

        public static Project CreateDefault(string filePath, string name = default)
        {
            var proj = new Project(filePath)
            {
                Name = name,
                SassItemGroup = new SassItemGroup()
                {
                    Include = new List<string>() { "*.scss" }
                },
                TypescriptItemGroup = new TypescriptItemGroup()
                {
                    Include = new List<TypescriptItemGroup.Bundle>()
                    {
                        new TypescriptItemGroup.Bundle("*.ts")
                    }
                },
                ImageItemGroup = new ImageItemGroup()
                {
                    OptimizationTargets = new List<ImageItemGroup.OptimizationBundle>()
                    {
                        new ImageItemGroup.OptimizationBundle()
                        {
                            Include = new List<string>() { "*.jpg", "*.png", "*.svg" }
                        }
                    }
                }
            };
            proj.AssignDefaults();
            return proj;
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
            project.AssignDefaults();
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

        public static bool TryLoad(string filePath, out Project project, out string error)
        {
            error = null;
            project = null;

            try
            {
                using (Stream file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (Validate(file, out error))
                    {
                        file.Position = 0;
                        project = Load(file);
                        project.FullName = filePath;
                        project.AssignDefaults();
                    }
                    else return false;
                    return true;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static bool TryLoad(string filePath, ValidationEventHandler handler, out Project project)
        {
            project = null;

            try
            {
                using (Stream file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (Validate(file, handler))
                    {
                        file.Position = 0;
                        project = Load(file);
                        project.FullName = filePath;
                        project.AssignDefaults();
                        return true;
                    }
                }
            }
            catch { }

            return false;
        }

        public static bool Validate(Stream stream, ValidationEventHandler handler)
        {
            var schema = new XmlSchemaSet();
            foreach (string path in Directory.EnumerateFiles(Path.Combine(Path.GetDirectoryName(typeof(Project).Assembly.Location)), $"{nameof(WebFlow)}.xsd", SearchOption.TopDirectoryOnly))
            {
                schema.Add(XMLNS, path);
            }

            //if (handler == null) handler = delegate (object sender, ValidationEventArgs e) { };

            bool isValid = true;
            var doc = XDocument.Load(stream, LoadOptions.SetLineInfo);
            doc.Validate(schema, (sender, e) =>
            {
                handler?.Invoke(sender, e);
                isValid = false;
            });
            stream.Position = 0;

            return isValid;
        }

        public static bool Validate(Stream stream, out string error)
        {
            var err = new StringBuilder();
            Validate(stream, (sender, e) =>
            {
                err.AppendLine($"[{e.Severity}] {e.Message} at line {e.Exception.LineNumber}");
            });
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

        public void AssignDefaults()
        {
            foreach (IItemGroup itemGroup in GetItempGroups())
            {
                itemGroup.WorkingDirectory = DirectoryName;
                if (!string.IsNullOrEmpty(OutputDirectory) && string.IsNullOrEmpty(itemGroup.OutputDirectory))
                {
                    itemGroup.OutputDirectory = OutputDirectory;
                }
                if (itemGroup.Suffix == null)
                {
                    itemGroup.Suffix = ".min";
                }
            }
        }

        public IEnumerable<IItemGroup> GetItempGroups()
        {
            if (SassItemGroup != null) yield return SassItemGroup;
            if (ImageItemGroup != null) yield return ImageItemGroup;
            if (TypescriptItemGroup != null) yield return TypescriptItemGroup;
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

        #region Private Members

        private readonly XmlSerializerNamespaces _namespace;
        private string _name;

        private string ToDebuggerDisplay() => FullName;

        #endregion Private Members
    }
}