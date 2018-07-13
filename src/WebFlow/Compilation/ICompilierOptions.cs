namespace Acklann.WebFlow.Compilation
{
    public interface ICompilierOptions
    {
        Kind Kind { get; }

        string SourceFile { get; }

        string GetFileType { get; }
    }
}