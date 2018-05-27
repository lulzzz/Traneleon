namespace Acklann.WebFlow.Compilation
{
    public interface ICompilierResult
    {
        bool Succeeded { get; }

        long ExecutionTime { get; }

        string OutputFile { get; }

        Error[] ErrorList { get; }
    }
}