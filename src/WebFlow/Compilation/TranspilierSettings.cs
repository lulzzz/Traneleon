using System.IO;

namespace Acklann.WebFlow.Compilation
{
    public struct TranspilierSettings : ICompilierOptions
    {
        public TranspilierSettings(string sourceFile, string outputDirecotry) : this(sourceFile, outputDirecotry, outputDirecotry, ".min")
        {
        }

        public TranspilierSettings(string sourceFile, string outputDirectory, string sourceMapDirectory, string suffix, bool keepIntermediateFiles = false, bool generateSourceMaps = true, bool bundle = true)
        {
            Suffix = suffix;
            Kind = Kind.Transpile;
            ConcatenateFiles = bundle;
            SourceFile = sourceFile;
            OutputDirectory = outputDirectory;
            SourceMapDirectory = sourceMapDirectory;
            GenerateSourceMaps = generateSourceMaps;
            KeepIntermediateFiles = keepIntermediateFiles;
        }

        public Kind Kind { get; }
        public string Suffix { get; }
        public string SourceFile { get; }
        public string OutputDirectory { get; }
        public string SourceMapDirectory { get; }
        public bool KeepIntermediateFiles { get; }
        public bool GenerateSourceMaps { get; }
        public bool ConcatenateFiles { get; }

        public string Ext()
        {
            return Path.GetExtension(SourceFile);
        }

        public string ToArgs()
        {
            string escape(string val) => (string.IsNullOrEmpty(val) ? "false" : $"\"{val}\"");

            return $"{ConcatenateFiles} {KeepIntermediateFiles} {GenerateSourceMaps} {escape(SourceFile)} {escape(OutputDirectory)} {escape(Suffix)} {escape(SourceMapDirectory)}";
        }
    }
}