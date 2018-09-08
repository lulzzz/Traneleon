using System.IO;

namespace Acklann.Traneleon.Compilation
{
    public struct TranspilierSettings : ICompilierOptions
    {
        public TranspilierSettings(string outputFile, params string[] sourceFiles) : this(outputFile, sourceFiles, ".min", null, true, false, (sourceFiles.Length > 1), null)
        {
        }

        public TranspilierSettings(string outputFile, string[] sourceFiles, string suffix = ".min", string sourceMapDirectory = null, bool generateSourceMaps = true, bool keepIntermediateFiles = false, bool shouldBundle = false, string outputDirectory = null)
        {
            Suffix = suffix;
            Kind = Kind.Transpile;
            OutputFile = outputFile;
            SourceFiles = sourceFiles;
            ShouldBundleFiles = shouldBundle;
            GenerateSourceMaps = generateSourceMaps;
            KeepIntermediateFiles = keepIntermediateFiles;
            FileType = Path.GetExtension(sourceFiles[0]);
            OutputDirectory = outputDirectory ?? Path.GetDirectoryName(outputFile);
            SourceMapDirectory = sourceMapDirectory ?? OutputDirectory;
        }

        public Kind Kind { get; }
        public string Suffix { get; }

        public string OutputFile { get; }
        public string[] SourceFiles { get; }
        public string OutputDirectory { get; }
        public bool GenerateSourceMaps { get; }
        public string SourceMapDirectory { get; }
        public bool KeepIntermediateFiles { get; }
        public bool ShouldBundleFiles { get; }

        string ICompilierOptions.SourceFile => string.Join(";", SourceFiles);

        public string FileType { get; }

        public string ToArgs()
        {
            string escape(string val) => (string.IsNullOrEmpty(val) ? "false" : $"\"{val}\"");
            return $"{KeepIntermediateFiles} {GenerateSourceMaps} {string.Join(";", SourceFiles)} {escape(OutputFile)} {escape(Suffix)} {escape(SourceMapDirectory)} {ShouldBundleFiles} {escape(OutputDirectory)}";
        }
    }
}