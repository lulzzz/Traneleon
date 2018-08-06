namespace Acklann.WebFlow.Compilation
{
    public struct NullCompilerOptions : ICompilierOptions
    {
        public Kind Kind => Kind.Optimize;

        public string SourceFile => string.Empty;

        public string FileType => string.Empty;
    }
}