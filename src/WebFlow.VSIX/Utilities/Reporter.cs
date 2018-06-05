using Acklann.WebFlow.Compilation;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace Acklann.WebFlow.Utilities
{
    public class Reporter : IObserver<ICompilierResult>
    {
        public Reporter(bool showOutput, IVsOutputWindowPane windowPane, IVsStatusbar statusbar)
        {
            _pane = windowPane;
            _statusbar = statusbar;
            _showOutput = showOutput;
        }

        public void OnNext(ICompilierResult result)
        {
            if (result.Succeeded)
            {
                
            }
            else
            {

            }
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }

        #region Private Members

        private readonly bool _showOutput;
        private readonly IVsStatusbar _statusbar;
        private readonly IVsOutputWindowPane _pane;

        #endregion Private Members
    }
}