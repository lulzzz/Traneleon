using Acklann.GlobN;
using Acklann.WebFlow.Compilation;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;

namespace Acklann.WebFlow.Utilities
{
    public class Reporter : IObserver<ICompilierResult>
    {
        public Reporter(string projectFile, VSPackage package) : this(projectFile, package._errorListProvider, package._outputPane, package.DTE, package._solution)
        {
        }

        public Reporter(string projectFile, ErrorListProvider errorList, IVsOutputWindowPane windowPane, DTE2 dte, IVsSolution solution)
        {
            if (dte == null) throw new ArgumentNullException(nameof(dte));
            if (solution == null) throw new ArgumentNullException(nameof(solution));
            _pane = windowPane ?? throw new ArgumentNullException(nameof(windowPane));
            _errorList = errorList ?? throw new ArgumentNullException(nameof(errorList));
            _projectFile = projectFile ?? throw new ArgumentNullException(nameof(projectFile));

            _workingDirectory = Path.GetDirectoryName(projectFile);
            _editorGuid = new Guid(EnvDTE.Constants.vsViewKindTextView);
            solution.GetProjectOfUniqueName(projectFile, out _hierarchy);
        }

        public void OnNext(ICompilierResult result)
        {
            if (result.Succeeded)
            {
                RemoveError(result.SourceFile ?? string.Empty);
                _pane.OutputStringThreadSafe($"[{result.Kind}] '{result.SourceFile.GetRelativePath(_workingDirectory)}' => '{result.OutputFile.GetRelativePath(_workingDirectory)}' in {TimeSpan.FromTicks(result.ExecutionTime).ToString(@"mm\:ss\.fff")}\n");
            }
            else
            {
                foreach (CompilerError error in result.ErrorList)
                {
                    var newError = new ErrorTask()
                    {
                        Line = error.Line,
                        Column = error.Column,
                        Text = error.Message,
                        Document = error.File,
                        HierarchyItem = _hierarchy,
                        Category = TaskCategory.BuildCompile,
                        ErrorCategory = TaskErrorCategory.Error
                    };
                    newError.Navigate += delegate (object sender, EventArgs e)
                    {
                        newError.Line++;
                        _errorList.Navigate(newError, _editorGuid);
                        newError.Line--;
                    };
                    _errorList.Tasks.Add(newError);
                }
                _errorList.Show();
            }
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }

        private void RemoveError(Glob filePath)
        {
            for (int i = 0; i < _errorList.Tasks.Count; i++)
            {
                if (filePath.IsMatch(_errorList.Tasks[i].Document))
                {
                    _errorList.Tasks.RemoveAt(i);
                    i = -1;
                }
            }
            _errorList.Tasks.Clear();
        }

        #region Private Members

        private readonly Guid _editorGuid;
        private readonly IVsHierarchy _hierarchy;
        private readonly IVsOutputWindowPane _pane;
        private readonly ErrorListProvider _errorList;
        private readonly string _workingDirectory, _projectFile;

        #endregion Private Members
    }
}