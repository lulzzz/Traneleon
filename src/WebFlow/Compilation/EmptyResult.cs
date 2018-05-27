namespace Acklann.WebFlow.Compilation
{
    public class EmptyResult : ICompilierResult
    {
        public bool Succeeded => true;

        public long ExecutionTime => 0;

        public Error[] ErrorList => new Error[0];

        string ICompilierResult.OutputFile => string.Empty;
    }
}