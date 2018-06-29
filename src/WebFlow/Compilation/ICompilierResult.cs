namespace Acklann.WebFlow.Compilation
{
    public interface ICompilierResult
    {
        Kind Kind { get; }

        bool Succeeded { get; }

        long ExecutionTime { get; }

        string SourceFile { get; }

        string OutputFile { get; }

        CompilerError[] ErrorList { get; }
    }
}