using System.IO;

namespace Acklann.WebFlow.Compilation
{
    public struct TranspilierSettings : ICompilierOptions
    {
        public TranspilierSettings(string outputFile, params string[] sourceFiles) : this(outputFile, sourceFiles, ".min", Path.GetDirectoryName(outputFile), true, false, (sourceFiles.Length > 1))
        {
        }

        public TranspilierSettings(string outputFile, string[] sourceFiles, string suffix, string sourceMapDirectory, bool generateSourceMaps, bool keepIntermediateFiles, bool shouldBundle)
        {
            Suffix = suffix;
            Kind = Kind.Transpile;
            OutputFile = outputFile;
            SourceFiles = sourceFiles;
            ShouldBundleFiles = shouldBundle;
            SourceMapDirectory = sourceMapDirectory;
            GenerateSourceMaps = generateSourceMaps;
            KeepIntermediateFiles = keepIntermediateFiles;
            GetFileType = (sourceFiles?.Length > 0 ? Path.GetExtension(sourceFiles[0]) : string.Empty);
        }

        public Kind Kind { get; }
        public string Suffix { get; }

        public string OutputFile { get; }
        public string[] SourceFiles { get; }
        public bool GenerateSourceMaps { get; }
        public string SourceMapDirectory { get; }
        public bool KeepIntermediateFiles { get; }
        public bool ShouldBundleFiles { get; }

        public string OutputDirectory
        {
            get { return (string.IsNullOrEmpty(OutputFile) ? string.Empty : Path.GetDirectoryName(OutputFile)); }
        }

        string ICompilierOptions.SourceFile => string.Join(";", SourceFiles);

        public string GetFileType { get; }

        public string ToArgs()
        {
            string escape(string val) => (string.IsNullOrEmpty(val) ? "false" : $"\"{val}\"");
            return $"{KeepIntermediateFiles} {GenerateSourceMaps} {string.Join(";", SourceFiles)} {escape(OutputFile)} {escape(Suffix)} {escape(SourceMapDirectory)} {ShouldBundleFiles}";
        }
    }
}