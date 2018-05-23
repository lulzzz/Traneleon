using Acklann.GlobN;
using Acklann.WebFlow.Compilation;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Acklann.WebFlow.Configuration
{
    public class TypescriptItemGroup : ItemGroupBase
    {
        public TypescriptItemGroup()
        {
            GenerateSourceMaps = true;
            KeepIntermediateFiles = false;
            Include = new List<Bundle>();
        }

        [XmlAttribute("keepIntermediateFiles")]
        public bool KeepIntermediateFiles { get; set; }

        [XmlAttribute("generateSourceMaps")]
        public bool GenerateSourceMaps { get; set; }

        [XmlAttribute("sourceDirectory")]
        public string SourceMapDirectory { get; set; }

        [XmlElement("include")]
        public List<Bundle> Include { get; set; }

        [XmlElement("exclude")]
        public List<string> Exclude { get; set; }

        public override bool CanAccept(string filePath)
        {
            return CanAccept(filePath, out Bundle tmp);
        }

        public bool CanAccept(string filePath, out Bundle bundle)
        {
            bundle = null;

            if (!string.IsNullOrEmpty(Suffix)
                &&
                filePath.EndsWith(string.Concat(Suffix, Path.GetExtension(filePath)), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (Exclude != null)
                foreach (Glob pattern in Exclude)
                    if (pattern.IsMatch(filePath))
                    {
                        return false;
                    }

            if (Include != null)
                foreach (Bundle set in Include)
                    foreach (Glob pattern in set.Patterns)
                        if (pattern.IsMatch(filePath))
                        {
                            bundle = set;
                            return true;
                        }

            return false;
        }

        public override ICompilierOptions CreateCompilerOptions(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));

            string outDir = (string.IsNullOrEmpty(OutputDirectory) ? Path.GetDirectoryName(filePath) : OutputDirectory);
            string mapDir = (string.IsNullOrEmpty(SourceMapDirectory) ? outDir : SourceMapDirectory);
            Bundle bundle = GetBundle(Include, filePath);

            if (bundle.IsNotEmpty)
            {
                bool concat = false;
                string sourceFile = filePath;

                if (!string.IsNullOrEmpty(bundle.EntryPoint))
                {
                    concat = true;
                    sourceFile = GetEntryPoint(bundle);
                }

                return new TranspilierSettings(sourceFile, outDir, mapDir, Suffix, KeepIntermediateFiles, GenerateSourceMaps, concat);
            }

            return new NullCompilerOptions();
        }

        public override IEnumerable<string> EnumerateFiles()
        {
            string src;
            var usedList = new Hashtable();

            foreach (string file in EnumerateFiles("*.ts"))
                if (CanAccept(file, out Bundle bundle))
                {
                    if (string.IsNullOrEmpty(bundle.EntryPoint)) yield return file;
                    else
                    {
                        src = GetEntryPoint(bundle);
                        if (!string.IsNullOrEmpty(src) && !usedList.ContainsKey(src))
                        {
                            usedList.Add(src, true);
                            yield return src;
                        }
                    }
                }
        }

        internal static Bundle GetBundle(IEnumerable<Bundle> bundles, string filePath)
        {
            foreach (Bundle group in bundles)
                foreach (Glob pattern in group.Patterns)
                {
                    if (pattern.IsMatch(filePath)) return group;
                }

            return new Bundle();
        }

        internal string GetEntryPoint(Bundle bundle)
        {
            string entryPoint = bundle.EntryPoint.ResolvePath(WorkingDirectory, false).FirstOrDefault();

            if (string.IsNullOrEmpty(entryPoint)) return string.Empty;
            else if (entryPoint.EndsWith(".ts", StringComparison.OrdinalIgnoreCase)) return entryPoint;
            else
            {
                string wwwroot = Path.Combine(WorkingDirectory, "wwwroot");
                var doc = new HtmlDocument();
                doc.Load(entryPoint);

                HtmlNodeCollection scriptNodes = doc.DocumentNode?.SelectNodes("//script[@src]");
                if (scriptNodes?.Count > 0)
                    foreach (HtmlNode tag in scriptNodes)
                    {
                        bool shouldCompile = tag.GetAttributeValue("data-compile", true);
                        string src = tag.GetAttributeValue("src", string.Empty).TrimStart('~');
                        if (shouldCompile && !string.IsNullOrEmpty(src) && !src.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        {
                            entryPoint = getSource(src.ExpandPath(wwwroot, false));
                            if (!File.Exists(entryPoint)) entryPoint = getSource(src.ExpandPath(WorkingDirectory, false));

                            return entryPoint;
                        }
                    }

                return string.Empty;
            }

            string getSource(string path)
            {
                if (string.IsNullOrEmpty(Suffix)) return Path.ChangeExtension(path, ".ts");
                else
                {
                    string name = Path.GetFileNameWithoutExtension(path);
                    name = (name.EndsWith(Suffix, StringComparison.OrdinalIgnoreCase) ? name.Remove(name.Length - Suffix.Length) : name);

                    return Path.Combine(Path.GetDirectoryName(path), string.Concat(name, ".ts"));
                }
            }
        }

        /* ===== NESTED TYPES ===== */

        public class Bundle
        {
            public Bundle() : this(new string[0])
            {
            }

            public Bundle(params string[] patterns)
            {
                EntryPoint = string.Empty;
                Patterns = new List<string>(patterns);
            }

            [XmlAttribute("entryPoint")]
            public string EntryPoint;

            [XmlElement("pattern")]
            public List<string> Patterns { get; set; }

            [XmlIgnore, JsonIgnore]
            public bool IsNotEmpty
            {
                get { return Patterns?.Count > 0; }
            }
        }
    }
}