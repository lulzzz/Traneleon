namespace Acklann.WebFlow.Compilation.Configuration
{
    public struct TranspilierResult : ICompilierResult
    {
        public TranspilierResult(int exitCode, long elapse, params object[] errors)
        {
            CompiliedFile = "";
            Succeeded = (exitCode == 0);
            ExecutionTime = elapse;
            ErrorList = errors;
        }

        public bool Succeeded { get; private set; }

        public long ExecutionTime { get; private set; }

        public object[] ErrorList { get; private set; }

        public string CompiliedFile { get; private set; }

        public string[] GeneratedFiles => throw new System.NotImplementedException();

        public static TranspilierResult CreateFrom(object exe)
        {
            throw new System.NotImplementedException();
        }
    }
}