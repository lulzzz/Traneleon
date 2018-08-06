namespace Acklann.WebFlow.Compilation
{
    public interface ICompilierOptions
    {
        Kind Kind { get; }

        string FileType { get; }

        string SourceFile { get; }
    }
}