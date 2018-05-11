using Acklann.WebFlow.Compilation;
using Acklann.WebFlow.Compilation.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Acklann.WebFlow.Tasks
{
    public class TypescriptItemGroup : ItemGroupBase
    {
        public TypescriptItemGroup()
        {
            Concatenate = true;
            GenerateSourceMaps = true;
            KeepIntermediateFiles = false;
            Exclude = new List<string>();
            Include = new List<string> { "*.ts" };
        }

        [XmlAttribute("concatenate")]
        public bool Concatenate { get; set; }

        [XmlAttribute("keepIntermediateFiles")]
        public bool KeepIntermediateFiles { get; set; }

        [XmlAttribute("generateSourceMaps")]
        public bool GenerateSourceMaps { get; set; }

        [XmlAttribute("sourceDirectory")]
        public string SourceMapDirectory { get; set; }

        public override bool CanAccept(string filePath)
        {
            bool accept = false;
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            switch (extension)
            {
                case ".ts":
                    accept = true;
                    break;

                case ".html":
                case ".cshtml":
                case ".vbhtml":
                    if (Concatenate)
                    {

                    }
                    break;
            }

            return accept && base.CanAccept(filePath);
        }

        public override ICompilierOptions CreateCompilerOptions(string filePath)
        {
            string outDir = (string.IsNullOrEmpty(OutputDirectory) ? WorkingDirectory : OutputDirectory);
            string mapDir = (string.IsNullOrEmpty(SourceMapDirectory) ? outDir : SourceMapDirectory);

            return new TranspilierSettings(filePath, outDir, mapDir, Suffix, KeepIntermediateFiles, GenerateSourceMaps, Concatenate);
        }

        #region Private Members

        private readonly ICollection<string> _entryPoints;

        #endregion Private Members
    }
}