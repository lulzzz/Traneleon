using Acklann.GlobN;
using Acklann.WebFlow.Compilation;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Acklann.WebFlow.Configuration
{
    public class TypescriptItemGroup : ItemGroupBase
    {
        public TypescriptItemGroup() : base()
        {
            GenerateSourceMaps = true;
            KeepIntermediateFiles = false;
        }

        [XmlAttribute("keepIntermediateFiles")]
        public bool KeepIntermediateFiles { get; set; }

        [XmlAttribute("generateSourceMaps")]
        public bool GenerateSourceMaps { get; set; }

        [XmlAttribute("sourceDirectory")]
        public string SourceMapDirectory { get; set; }

        [XmlElement("include")]
        public List<Bundle> Include { get; set; }

        [XmlArray("exclude")]
        [XmlArrayItem("pattern")]
        public List<string> Exclude { get; set; }

        public override bool CanAccept(string filePath)
        {
            return CanAccept(filePath, Include, out Bundle tmp);
        }

        public bool CanAccept(string filePath, IEnumerable<Bundle> bundles, out Bundle bundle)
        {
            bundle = null;

            if (!string.IsNullOrEmpty(Suffix) && filePath.EndsWith(string.Concat(Suffix, Path.GetExtension(filePath)), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (Exclude != null)
                foreach (Glob pattern in Exclude)
                    if (pattern.IsMatch(filePath))
                    {
                        return false;
                    }

            if (bundles != null)
                foreach (Bundle set in bundles)
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

            Bundle bundle = GetBundle(Include, filePath);
            if (bundle?.IsNotEmpty ?? false)
            {
                string[] src;
                bool concat = false;
                string outFile, mapDir, outDir;

                if (string.IsNullOrEmpty(bundle.EntryPoint))
                {
                    src = new[] { filePath };
                    outDir = (string.IsNullOrEmpty(OutputDirectory) ? Path.GetDirectoryName(filePath) : OutputDirectory);
                    outFile = Path.ChangeExtension(Path.Combine(outDir, Path.GetFileName(filePath)), ".js");
                }
                else
                {
                    concat = true;
                    if (Path.IsPathRooted(bundle.EntryPoint))
                    {
                        outFile = bundle.EntryPoint;
                        outDir = Path.GetDirectoryName(outFile);
                    }
                    else if (bundle.EntryPoint.EndsWith(".js") || bundle.EntryPoint.EndsWith(".ts"))
                    {
                        outDir = (string.IsNullOrEmpty(OutputDirectory) ? WorkingDirectory : OutputDirectory);
                        outFile = Path.Combine(outDir, bundle.EntryPoint);
                    }
                    else
                    {
                        outFile = GetEntryPoint(bundle);
                        outDir = (string.IsNullOrEmpty(OutputDirectory) ? Path.GetDirectoryName(outFile) : OutputDirectory);
                        outFile = Path.ChangeExtension(Path.Combine(outDir, Path.GetFileName(outFile)), ".js");
                    }

                    src = EnumerateFiles(new[] { bundle }).ToArray();
                }

                mapDir = (string.IsNullOrEmpty(SourceMapDirectory) ? outDir : SourceMapDirectory);
                return new TranspilierSettings(outFile, src, Suffix, mapDir, GenerateSourceMaps, KeepIntermediateFiles, concat);
            }

            return new NullCompilerOptions();
        }

        public override IEnumerable<string> EnumerateFiles()
        {
            return EnumerateFiles(Include);
        }

        public IEnumerable<string> EnumerateFiles(IEnumerable<Bundle> bundles)
        {
            var usedList = new Dictionary<string, bool>();

            foreach (string file in EnumerateFiles("*.ts"))
                if (CanAccept(file, bundles, out Bundle bundle))
                {
                    if (string.IsNullOrEmpty(bundle.EntryPoint)) yield return file;
                    else if (!usedList.ContainsKey(bundle.EntryPoint))
                    {
                        usedList.Add(bundle.EntryPoint, true);
                        string src = GetEntryPoint(bundle);
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
            {
                foreach (Glob pattern in group.Patterns)
                {
                    if (pattern.IsMatch(filePath)) return group;
                }
            }

            return null;
        }

        internal string GetEntryPoint(Bundle bundle)
        {
            string entryPoint = bundle.EntryPoint.ResolvePath(WorkingDirectory, SearchOption.AllDirectories).FirstOrDefault();

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