using Acklann.WebFlow.Configuration;
using System.IO;

namespace Acklann.WebFlow.Compilation
{
    public readonly struct ImageOptimizerOptions : ICompilierOptions
    {
        public ImageOptimizerOptions(CompressionKind kind, string sourceFile, string outFile, int quality = 80, bool progressive = true)
        {
            Quality = quality;
            OutputFile = outFile;
            CompressionKind = kind;
            SourceFile = sourceFile;
            Progressive = progressive;
            FileType = Path.GetExtension(sourceFile).ToLowerInvariant();
        }

        public Kind Kind => Kind.Optimize;

        public int Quality { get; }

        public string FileType { get; }

        public bool Progressive { get; }

        public string SourceFile { get; }

        public string OutputFile { get; }

        public CompressionKind CompressionKind { get; }
    }
}