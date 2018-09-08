namespace Acklann.Traneleon.Compilation
{
    public interface ICompilierOptions
    {
        Kind Kind { get; }

        string FileType { get; }

        string SourceFile { get; }
    }
}