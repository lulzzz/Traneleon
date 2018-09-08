using Acklann.GlobN;
using Acklann.Traneleon.Compilation;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Acklann.Traneleon.Configuration
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

            if (filePath.EndsWith(".ts", StringComparison.OrdinalIgnoreCase) == false)
            {
                return false;
            }
            else if (!string.IsNullOrEmpty(Suffix) && filePath.EndsWith($"{Suffix}{Path.GetExtension(filePath)}", StringComparison.OrdinalIgnoreCase))
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

        public override IEnumerable<ICompilierOptions> CreateCompilerOptions(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));

            Bundle bundle = GetBundle(Include, filePath);
            if (bundle?.IsNotNullOrEmpty ?? false)
            {
                string outFile, mapDir, outDir;

                if (string.IsNullOrEmpty(bundle.OutputFile))
                {
                    outDir = (string.IsNullOrEmpty(OutputDirectory) ? Path.GetDirectoryName(filePath) : OutputDirectory);
                    outFile = Path.Combine(outDir, Path.GetFileName(filePath));
                    mapDir = (string.IsNullOrEmpty(SourceMapDirectory) ? outDir : SourceMapDirectory);

                    yield return new TranspilierSettings(outFile, new[] { filePath }, Suffix, mapDir, GenerateSourceMaps, KeepIntermediateFiles, false, outDir);
                }
                else
                {
                    if (Path.IsPathRooted(bundle.OutputFile))
                    {
                        outFile = bundle.OutputFile;
                        outDir = Path.GetDirectoryName(outFile);
                    }
                    else
                    {
                        outFile = GetOutFile(bundle);
                        outDir = Path.GetDirectoryName(outFile);
                    }

                    string[] src = EnumerateFiles(new[] { bundle }).ToArray();

                    mapDir = (string.IsNullOrEmpty(SourceMapDirectory) ? outDir : SourceMapDirectory);
                    yield return new TranspilierSettings(Path.ChangeExtension(outFile, ".js"), src, Suffix, mapDir, GenerateSourceMaps, KeepIntermediateFiles, true, outDir);
                }
            }
        }

        public override IEnumerable<string> EnumerateFiles() => EnumerateFiles(Include, returnOutFileIfExists: true);

        internal IEnumerable<string> EnumerateFiles(IEnumerable<Bundle> bundles, bool returnOutFileIfExists = false)
        {
            string value;
            var duplicateFiles = new Dictionary<string, bool>();

            foreach (string file in EnumerateFiles("*.ts"))
                if (CanAccept(file, bundles, out Bundle bundle))
                {
                    if (string.IsNullOrEmpty(bundle.OutputFile)) value = file;
                    else if (returnOutFileIfExists) value = GetOutFile(bundle);
                    else value = file;

                    if (returnOutFileIfExists && duplicateFiles.ContainsKey(value) == false)
                    {
                        duplicateFiles.Add(value, true);
                        yield return value;
                        continue;
                    }
                    else if (returnOutFileIfExists) continue;
                    else yield return file;
                }
        }

        internal Bundle GetBundle(IEnumerable<Bundle> bundles, string filePath)
        {
            foreach (Bundle group in bundles)
            {
                if (!string.IsNullOrEmpty(group.OutputFile) && Glob.IsMatch(filePath, $"**/{group.OutputFile}")) return group;
                else foreach (Glob pattern in group.Patterns)
                    {
                        if (pattern.IsMatch(filePath)) return group;
                    }
            }

            return null;
        }

        internal string GetEntryPoint(Bundle bundle)
        {
            string entryPoint = bundle.OutputFile.ResolvePath(WorkingDirectory, SearchOption.AllDirectories).FirstOrDefault();

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

        private string GetOutFile(Bundle bundle)
        {
            return Path.Combine((string.IsNullOrEmpty(OutputDirectory) ? WorkingDirectory : OutputDirectory), bundle.OutputFile);
        }

        /* ===== NESTED TYPES ===== */

        public class Bundle
        {
            public Bundle() : this(new string[0])
            {
            }

            public Bundle(params string[] patterns)
            {
                OutputFile = null;
                Patterns = new List<string>(patterns);
            }

            [XmlAttribute("outFile")]
            [JsonProperty("outFile")]
            public string OutputFile;

            [XmlElement("pattern")]
            [JsonProperty("patterns")]
            public List<string> Patterns { get; set; }

            [XmlIgnore, JsonIgnore]
            public bool IsNotNullOrEmpty
            {
                get { return Patterns?.Count > 0; }
            }
        }
    }
}