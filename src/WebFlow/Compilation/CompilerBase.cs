using System;

namespace Acklann.WebFlow.Compilation
{
    public abstract class CompilerBase<TOptions> : ICompiler where TOptions : ICompilierOptions
    {
        public CompilerBase() : this(ShellBase.GetShell())
        {
        }

        public CompilerBase(ShellBase shell)
        {
            Shell = shell;
        }

        protected readonly ShellBase Shell;

        public ICompilierResult Execute(ICompilierOptions options)
        {
            SetArguments((TOptions)options);
            Shell.Start();
            Shell.WaitForExit(10_000);
            return GetResult((TOptions)options);
        }

        public bool CanExecute(ICompilierOptions options) => CanExecute((TOptions)options);

        protected virtual ICompilierResult GetResult(TOptions options)
        {
            return Shell.GenerateResults(options);
        }

        protected abstract void SetArguments(TOptions options);

        protected abstract bool CanExecute(TOptions options);

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Shell?.Dispose();
            }
        }

        #endregion IDisposable
    }
}