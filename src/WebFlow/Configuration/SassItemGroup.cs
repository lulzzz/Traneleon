using Acklann.GlobN;
using Acklann.WebFlow.Compilation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Acklann.WebFlow.Configuration
{
    public class SassItemGroup : ItemGroupBase
    {
        public SassItemGroup()
        {
            GenerateSourceMaps = true;
            KeepIntermediateFiles = false;
            Include = new List<string> { "*.scss" };
        }

        [XmlAttribute("keepIntermediateFiles")]
        public bool KeepIntermediateFiles { get; set; }

        [XmlAttribute("generateSourceMaps")]
        public bool GenerateSourceMaps { get; set; }

        [XmlAttribute("sourceDirectory")]
        public string SourceMapDirectory { get; set; }

        [XmlElement("include")]
        public List<string> Include { get; set; }

        [XmlElement("exclude")]
        public List<string> Exclude { get; set; }

        public override bool CanAccept(string filePath)
        {
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
                foreach (Glob pattern in Include)
                    if (pattern.IsMatch(filePath))
                    {
                        return true;
                    }

            return false;
        }

        public override ICompilierOptions CreateCompilerOptions(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));

            string name = Path.GetFileName(filePath);
            if (name.StartsWith("_") == false)
            {
                string outDir = (string.IsNullOrEmpty(OutputDirectory) ? Path.GetDirectoryName(filePath) : OutputDirectory);
                string mapDir = (string.IsNullOrEmpty(SourceMapDirectory) ? outDir : SourceMapDirectory);

                return new TranspilierSettings(filePath, outDir, mapDir, Suffix, KeepIntermediateFiles, GenerateSourceMaps, false);
            }

            return new NullCompilerOptions();
        }
    }
}