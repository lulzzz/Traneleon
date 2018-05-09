namespace Acklann.WebFlow.Compilation
{
    public interface ICompiler : System.IDisposable
    {
        bool CanExecute(ICompilierOptions options);

        ICompilierResult Execute(ICompilierOptions options);
    }
}