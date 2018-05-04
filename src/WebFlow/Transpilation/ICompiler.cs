namespace Acklann.WebFlow.Transpilation
{
    public interface ICompiler
    {
        bool CanExecute(ICompilierOptions options);

        ICompilierResult Execute(ICompilierOptions options);
    }
}