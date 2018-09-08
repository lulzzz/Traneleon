using Acklann.GlobN;
using Acklann.Traneleon.Compilation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Acklann.Traneleon.Configuration
{
    public class SassItemGroup : ItemGroupBase
    {
        public SassItemGroup()
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

        [XmlArray("include")]
        [XmlArrayItem("pattern")]
        public List<string> Include { get; set; }

        [XmlArray("exclude")]
        [XmlArrayItem("pattern")]
        public List<string> Exclude { get; set; }

        public override bool CanAccept(string filePath)
        {
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

            if (Include != null)
                foreach (Glob pattern in Include)
                    if (pattern.IsMatch(filePath))
                    {
                        return true;
                    }

            return false;
        }

        public override IEnumerable<ICompilierOptions> CreateCompilerOptions(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));

            IEnumerable<string> sourcFiles;
            string name = Path.GetFileName(filePath);
            if (name.StartsWith("_")) sourcFiles = EnumerateFiles("*.scss").Where(x => Path.GetFileName(x).StartsWith("_") == false);
            else sourcFiles = new[] { filePath };

            foreach (string src in sourcFiles)
            {
                string outDir = (string.IsNullOrEmpty(OutputDirectory) ? Path.GetDirectoryName(src) : OutputDirectory);
                string mapDir = (string.IsNullOrEmpty(SourceMapDirectory) ? outDir : SourceMapDirectory);
                string outFile = Path.ChangeExtension(Path.Combine(outDir, Path.GetFileName(src)), ".css");

                yield return new TranspilierSettings(outFile, new[] { src }, Suffix, mapDir, GenerateSourceMaps, KeepIntermediateFiles, false, outDir);
            }
        }
    }
}