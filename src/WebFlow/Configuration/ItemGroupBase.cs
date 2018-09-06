using Acklann.GlobN;
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
        public ItemGroupBase()
        {
            Enabled = true;
            Suffix = ".min";
        }

        [XmlAttribute("enable")]
        public bool Enabled { get; set; }

        [XmlAttribute("outputDirectory")]
        public string OutputDirectory { get; set; }

        [XmlAttribute("suffix")]
        public string Suffix { get; set; }

        [XmlIgnore, JsonIgnore]
        public string WorkingDirectory { get; set; }

        public abstract bool CanAccept(string filePath);

        public abstract IEnumerable<ICompilierOptions> CreateCompilerOptions(string filePath);

        public virtual IEnumerable<string> EnumerateFiles()
        {
            foreach (string file in EnumerateFiles("*"))
                if (CanAccept(file))
                {
                    yield return file;
                }
        }

        internal static string[] GeneratedFolders = new[] { "node_modules", "bower_components", "bin", "obj", "vendor" };

        protected internal IEnumerable<string> EnumerateFiles(string pattern)
        {
            Glob[] patterns = (from g in GeneratedFolders select new Glob($"**/{g}/")).ToArray();

            foreach (string file in Directory.EnumerateFiles(WorkingDirectory, pattern)) yield return file;
            foreach (string folder in Directory.EnumerateDirectories(WorkingDirectory).Where(x => notDependency(x)))
            {
                foreach (string file in Directory.EnumerateFiles(folder, pattern, SearchOption.AllDirectories))
                    if (canAccept(file))
                    {
                        yield return file;
                    }
            }
            /* *** local function(s) *** */

            bool canAccept(string path)
            {
                foreach (Glob glob in patterns)
                    if (glob.IsMatch(path)) return false;
                    else { }

                return true;
            }

            bool notDependency(string path)
            {
                foreach (string folder in GeneratedFolders)
                    if (path.EndsWith(folder, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                return true;
            }
        }

        protected string GetOutputFile(string sourceFile)
        {
            string baseName = Path.GetFileNameWithoutExtension(sourceFile);
            string outDir = (string.IsNullOrEmpty(OutputDirectory) ? Path.GetDirectoryName(sourceFile) : OutputDirectory);
            return Path.Combine(outDir, $"{baseName}{Suffix}{Path.GetExtension(sourceFile)}");
        }
    }
}