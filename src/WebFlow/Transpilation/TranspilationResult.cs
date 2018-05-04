namespace Acklann.WebFlow.Transpilation
{
    public struct TranspilationResult
    {
        public TranspilationResult(long elapse, object[] errors)
        {
            Errors = errors;
            ExecutionTime = elapse;
            Succeeded = !(errors?.Length > 0);
        }

        public bool Succeeded { get; }

        public object[] Errors { get; }

        public long ExecutionTime { get; }
    }
}