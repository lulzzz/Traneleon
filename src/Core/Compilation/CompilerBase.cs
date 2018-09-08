using System;

namespace Acklann.Traneleon.Compilation
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
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"executing {GetType().Name} ...");
#endif
            try
            {
                SetArguments((TOptions)options);
                Shell.Start();
                Shell.WaitForExit(10_000);
                return GetResult((TOptions)options);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return new CompilerResult(options.Kind, false, 0, options.SourceFile, string.Empty);
            }
        }

        public bool CanExecute(ICompilierOptions options) => CanExecute((TOptions)options);

        protected abstract ICompilierResult GetResult(TOptions options);

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