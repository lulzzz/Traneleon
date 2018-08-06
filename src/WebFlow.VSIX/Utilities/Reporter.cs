using Acklann.GlobN;
using Acklann.WebFlow.Compilation;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Linq;

namespace Acklann.WebFlow.Utilities
{
    public class Reporter : IProgress<ProgressToken>
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

        public async void Report(ProgressToken token)
        {
            if (token.Result.Succeeded)
            {
                UpdateErrorList(token.Result.SourceFile ?? string.Empty);

                string msg = $"[{token.Result.Kind}] '{normalize(token.Result.SourceFile)}' => '{normalize(token.Result.OutputFile)}' in {TimeSpan.FromTicks(token.Result.ExecutionTime).ToString(@"mm\:ss\.fff")}\r\n";
                System.Diagnostics.Debug.WriteLine(msg);
                _pane.OutputStringThreadSafe(msg);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("FAILED");
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                foreach (CompilerError error in token.Result.ErrorList)
                {
                    var newError = new ErrorTask()
                    {
                        CanDelete = true,
                        Line = error.Line,
                        Column = error.Column,
                        Document = error.File,
                        HierarchyItem = _hierarchy,
                        Text = $"(WF) {error.Message}",
                        Category = TaskCategory.BuildCompile,
                        ErrorCategory = ((TaskErrorCategory)error.Category)
                    };

                    newError.Navigate += delegate (object sender, EventArgs e)
                    {
                        newError.Line++;
                        _errorList.Navigate(newError, _editorGuid);
                        newError.Line--;
                    };
                    _errorList.Tasks.Add(newError);
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[{newError.ErrorCategory}] '{Path.GetFileName(newError.Document)}' {newError.Line}");
                    System.Diagnostics.Debug.WriteLine($"{newError.Text}");
#endif
                }
                _errorList.Show();
            }

            string normalize(string path)
            {
                string[] files = path.Split(';');
                if (files.Length > 2) return $"[{files.Length} files]";
                else return string.Join(" + ", (from x in path.Split(';') select x.GetRelativePath(_workingDirectory).TrimStart('.', '/', '\\')));
            }
        }

        private void UpdateErrorList(Glob filePath)
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