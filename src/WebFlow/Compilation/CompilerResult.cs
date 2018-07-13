namespace Acklann.WebFlow.Compilation
{
    public class CompilerResult : ICompilierResult
    {
        public CompilerResult(Kind kind, string sourceFile, params CompilerError[] errors)
        {
            Kind = kind;
            ErrorList = errors;
            SourceFile = sourceFile;
        }

        public Kind Kind { get; }

        public bool Succeeded => false;

        public long ExecutionTime => 0;

        public string SourceFile { get; }

        public string OutputFile => string.Empty;

        public CompilerError[] ErrorList { get; }
    }
}