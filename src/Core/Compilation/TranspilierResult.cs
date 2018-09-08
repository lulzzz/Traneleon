namespace Acklann.Traneleon.Compilation
{
    public readonly struct TranspilierResult : ICompilierResult
    {
        public TranspilierResult(int exitCode, long elapse, CompilerError[] errors, string outFile, string sourceFile)
        {
            Kind = Kind.Transpile;
            Succeeded = (exitCode == 0);
            CompiliedFile = outFile;
            SourceFile = sourceFile;
            ExecutionTime = elapse;
            ErrorList = errors;
        }

        public Kind Kind { get; }

        public bool Succeeded { get; }

        public long ExecutionTime { get; }

        public CompilerError[] ErrorList { get; }

        public string CompiliedFile { get; }

        public string SourceFile { get; }

        string ICompilierResult.OutputFile => CompiliedFile;

        public override string ToString()
        {
            return CompiliedFile;
        }
    }
}