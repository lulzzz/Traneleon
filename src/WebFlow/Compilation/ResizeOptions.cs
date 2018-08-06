using System.IO;

namespace Acklann.WebFlow.Compilation
{
    [System.Diagnostics.DebuggerDisplay("{ToDebuggerDisplay()}")]
    public readonly struct ResizeOptions : ICompilierOptions
    {
        public ResizeOptions(string srcFile, string outFile, string newSize, bool preserve = true)
        {
            OutputFile = outFile;
            SourceFile = srcFile;
            PreserveAspectRatio = preserve;
            FileType = Path.GetExtension(srcFile);

            NewSize = newSize;
        }

        public Kind Kind => Kind.None;

        public string FileType { get; }

        public string SourceFile { get; }

        public string OutputFile { get; }

        public string NewSize { get; }

        

        public bool PreserveAspectRatio { get; }

        private string ToDebuggerDisplay()
        {
            return $"";
        }
    }
}