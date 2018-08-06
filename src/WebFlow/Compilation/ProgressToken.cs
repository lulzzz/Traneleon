namespace Acklann.WebFlow.Compilation
{
    public readonly struct ProgressToken
    {
        public ProgressToken(ICompilierResult result, int maxOperations = 0)
        {
            Result = result;
            MaxOperations = maxOperations;
        }

        public readonly int MaxOperations;
        public readonly ICompilierResult Result;
    }
}