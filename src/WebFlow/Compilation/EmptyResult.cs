namespace Acklann.WebFlow.Compilation
{
    public class EmptyResult : ICompilierResult
    {
        public bool Succeeded => true;

        public long ExecutionTime => 0;

        public CompilerError[] ErrorList => new CompilerError[0];

        string ICompilierResult.OutputFile => string.Empty;
    }
}