using Acklann.GlobN;
using Acklann.WebFlow.Compilation;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Acklann.WebFlow.Tasks
{
    public abstract class ItemGroupBase : IItemGroup
    {
        [XmlIgnore, JsonIgnore]
        public string WorkingDirectory { get; set; }

        [XmlAttribute("enable")]
        public bool Enabled { get; set; }

        [XmlAttribute("suffix")]
        public string Suffix { get; set; }

        [XmlAttribute("outputDirectory")]
        public string OutputDirectory { get; set; }

        [XmlElement("include")]
        public List<string> Include { get; set; }

        [XmlElement("exclude")]
        public List<string> Exclude { get; set; }

        public virtual bool CanAccept(string filePath)
        {
            if (Exclude != null)
                foreach (Glob pattern in Exclude)
                {
                    if (pattern.IsMatch(filePath)) return false;
                }

            if (Include != null)
                foreach (Glob pattern in Include)
                {
                    if (pattern.IsMatch(filePath)) return true;
                }

            return Include?.Count == 0;
        }

        public abstract ICompilierOptions CreateCompilerOptions(string filePath);
    }
}