namespace Acklann.WebFlow.Transpilation
{
    public interface ICompilierResult
    {
        bool Succeeded { get; }

        long ExecutionTime { get; }

        object[] ErrorList { get; }
    }
}