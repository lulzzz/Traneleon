namespace Acklann.Traneleon.Compilation
{
    public readonly struct EmptyResult : ICompilierResult
    {
        public bool Succeeded => true;

        public Kind Kind => Kind.None;

        public long ExecutionTime => 0;

        public string SourceFile => string.Empty;

        string ICompilierResult.OutputFile => string.Empty;

        public CompilerError[] ErrorList => new CompilerError[0];
    }
}