namespace Acklann.WebFlow.Compilation
{
    public class NullCompiler : ICompiler
    {
        public bool CanExecute(ICompilierOptions options) => true;

        public ICompilierResult Execute(ICompilierOptions options)
        {
            return new EmptyResult();
        }

        public void Dispose()
        {
        }
    }
}