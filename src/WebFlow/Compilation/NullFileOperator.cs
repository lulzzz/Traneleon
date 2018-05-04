namespace Acklann.WebFlow.Compilation
{
    public class NullFileOperator : ICompiler
    {
        public bool CanExecute(ICompilierOptions options) => true;

        public ICompilierResult Execute(ICompilierOptions options)
        {
            return new NullResult();
        }

        public readonly struct NullResult : ICompilierResult
        {
            public bool Succeeded => true;

            public long ExecutionTime => 0;

            public object[] ErrorList => new object[0];
        }
    }
}