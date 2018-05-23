using Acklann.WebFlow.Compilation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Acklann.WebFlow.Configuration
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

        public abstract bool CanAccept(string filePath);

        public abstract ICompilierOptions CreateCompilerOptions(string filePath);

        public virtual IEnumerable<string> EnumerateFiles()
        {
            foreach (string file in EnumerateFiles("*"))
                if (CanAccept(file))
                {
                    yield return file;
                }
        }

        protected internal IEnumerable<string> EnumerateFiles(string pattern)
        {
            foreach (string file in Directory.EnumerateFiles(WorkingDirectory, pattern)) yield return file;

            foreach (string folder in Directory.EnumerateDirectories(WorkingDirectory).Where(x => notDependency(x)))
                foreach (string file in Directory.EnumerateFiles(folder, pattern, SearchOption.AllDirectories))
                {
                    yield return file;
                }

            /* *** local function(s) *** */

            bool notDependency(string path)
            {
                foreach (string folder in new string[] { "node_modules", "bower_components", "packages" })
                    if (path.EndsWith(folder, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                return true;
            }
        }
    }
}