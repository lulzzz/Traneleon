namespace Acklann.WebFlow.Compilation
{
    public interface ICompilierResult
    {
        bool Succeeded { get; }

        long ExecutionTime { get; }

        Error[] ErrorList { get; }
    }
}