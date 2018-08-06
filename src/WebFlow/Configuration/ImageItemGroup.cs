using Acklann.GlobN;
using Acklann.WebFlow.Compilation;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Acklann.WebFlow.Configuration
{
    public class ImageItemGroup : ItemGroupBase
    {
        [XmlArray("exclude")]
        [XmlArrayItem("pattern")]
        public List<string> Exclude { get; set; }

        [XmlIgnore, JsonIgnore]
        public List<ResizeBundle> ResizeTargets { get; set; }

        [XmlElement("optimize")]
        [JsonProperty("optimize")]
        public List<OptimizationBundle> OptimizationTargets { get; set; }

        public override bool CanAccept(string filePath)
        {
            if (Exclude != null)
                foreach (Glob pattern in Exclude)
                    if (pattern.IsMatch(filePath))
                    {
                        return false;
                    }

            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (!string.IsNullOrEmpty(Suffix) && filePath.EndsWith($"{Suffix}{ext}"))
            {
                return false;
            }

            foreach (string supported in new[] { ".jpeg", ".jpg", ".png", ".svg" })
                if (ext == supported)
                {
                    return true;
                }

            return false;
        }

        public override IEnumerable<ICompilierOptions> CreateCompilerOptions(string filePath)
        {
            if (OptimizationTargets != null)
                foreach (OptimizationBundle options in OptimizationTargets)
                    if (options.CanAccept(filePath))
                    {
                        yield return options.AsOptions(filePath, GetOutputFile(filePath));
                    }

            //if (ResizeTargets != null)
            //    foreach (ResizeBundle bundle in ResizeTargets)
            //        if (bundle.CanAccept(filePath))
            //        {
            //            yield return bundle.AsOptions(filePath, null);
            //        }
        }

        #region Options

        public class ResizeBundle : OptionBase
        {
            [XmlAttribute("newSize")]
            public string NewSize { get; set; }

            [XmlAttribute("optimize")]
            public bool Optimize { get; set; } = true;

            [XmlAttribute("preserve")]
            [JsonProperty("preserve")]
            public bool PreserveAspectRatio { get; set; } = true;

            internal override ICompilierOptions AsOptions(string sourceFile, string outputFile)
            {
                throw new System.NotImplementedException();
            }
        }

        public class OptimizationBundle : OptionBase
        {
            [XmlAttribute("quality")]
            public int Quality { get; set; } = 80;

            [XmlAttribute("compression")]
            [JsonProperty("compression")]
            public CompressionKind Kind { get; set; }

            [XmlAttribute("progressive")]
            public bool Progressive { get; set; } = true;

            internal override ICompilierOptions AsOptions(string srcFile, string outFile)
            {
                return new ImageOptimizerOptions(Kind, srcFile, outFile, Quality, Progressive);
            }
        }

        public abstract class OptionBase
        {
            [XmlElement("pattern")]
            [JsonProperty("patterns")]
            public List<string> Include { get; set; }

            internal bool CanAccept(string filePath)
            {
                if (Include != null)
                    foreach (Glob pattern in Include)
                        if (pattern.IsMatch(filePath))
                        {
                            return true;
                        }

                return false;
            }

            internal abstract ICompilierOptions AsOptions(string sourceFile, string outputFile);
        }

        #endregion Options
    }
}