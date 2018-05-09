namespace Acklann.WebFlow.Compilation.Configuration
{
    public struct TranspilierResult : ICompilierResult
    {
        public TranspilierResult(string outFile, int exitCode, long elapse, params Error[] errors)
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
    }
}