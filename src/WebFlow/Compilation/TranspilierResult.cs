namespace Acklann.WebFlow.Compilation
{
    public struct TranspilierResult : ICompilierResult
    {
        public TranspilierResult(int exitCode, long elapse, Error[] errors, string outFile)
        {
            Succeeded = (exitCode == 0);
            CompiliedFile = outFile;
            ExecutionTime = elapse;
            ErrorList = errors;
        }

        public bool Succeeded { get; private set; }

        public long ExecutionTime { get; private set; }

        public Error[] ErrorList { get; private set; }

        public string CompiliedFile { get; private set; }

        string ICompilierResult.OutputFile => CompiliedFile;

        public override string ToString()
        {
            return CompiliedFile;
        }
    }
}