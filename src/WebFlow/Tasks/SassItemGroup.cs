using Acklann.WebFlow.Compilation;
using Acklann.WebFlow.Compilation.Configuration;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Acklann.WebFlow.Tasks
{
    public class SassItemGroup : ItemGroupBase
    {
        public SassItemGroup()
        {
            GenerateSourceMaps = true;
            KeepIntermediateFiles = false;
            Exclude = new List<string>();
            Include = new List<string> { "*.scss" };
        }

        [XmlAttribute("keepIntermediateFiles")]
        public bool KeepIntermediateFiles { get; set; }

        [XmlAttribute("generateSourceMaps")]
        public bool GenerateSourceMaps { get; set; }

        [XmlAttribute("sourceDirectory")]
        public string SourceMapDirectory { get; set; }

        public override ICompilierOptions CreateCompilerOptions(string filePath)
        {
            string outDir = (string.IsNullOrEmpty(OutputDirectory) ? WorkingDirectory : OutputDirectory);
            string mapDir = (string.IsNullOrEmpty(SourceMapDirectory) ? outDir : SourceMapDirectory);

            return new TranspilierSettings(filePath, outDir, mapDir, Suffix, KeepIntermediateFiles, GenerateSourceMaps, false);
        }
    }
}