namespace Acklann.WebFlow.Compilation
{
    public struct NullCompilerOptions : ICompilierOptions
    {
        public Kind Kind => Kind.Minify;

        public string SourceFile => string.Empty;

        public string GetFileType => string.Empty;
    }
}