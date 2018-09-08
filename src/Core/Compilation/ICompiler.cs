namespace Acklann.Traneleon.Compilation
{
    public interface ICompiler : System.IDisposable
    {
        bool CanExecute(ICompilierOptions options);

        ICompilierResult Execute(ICompilierOptions options);
    }
}