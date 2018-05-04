namespace Acklann.WebFlow.Compilation
{
    public interface ICompiler
    {
        bool CanExecute(ICompilierOptions options);

        ICompilierResult Execute(ICompilierOptions options);
    }
}