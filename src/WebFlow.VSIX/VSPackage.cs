using Acklann.GlobN;
using Acklann.WebFlow.Commands;
using Acklann.WebFlow.Compilation;
using Acklann.WebFlow.Utilities;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace Acklann.WebFlow
{
    public delegate ProjectMonitor ProjectMonitorActivator(string projectFile);

    [Guid(Symbols.Package.GuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideOptionPage(typeof(OptionsPage), nameof(WebFlow), "General", 0, 0, true)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class VSPackage : AsyncPackage
    {
        internal const string ConfigName = "webflow.config";
        internal DTE2 DTE;

        internal T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        internal void ClearWatchList()
        {
            foreach (ProjectMonitor monitor in _watchList.Values)
            {
                monitor?.Pause();
                monitor?.Dispose();
            }
            _watchList.Clear();
        }

        internal void LoadSolution(bool autoConfg)
        {
            foreach (EnvDTE.Project project in DTE.GetActiveProjects())
                if (_watchList.Contains(project.FullName) == false)
                {
                    string folder = Path.GetDirectoryName(project.FullName);
                    string configFile = Directory.EnumerateFiles(folder, "*webflow*").FirstOrDefault();
                    string msg = string.Format("{1} | Monitoring '{0}' for changes ...", project.Name, nameof(WebFlow));

                    if (!string.IsNullOrEmpty(configFile))
                    {
                        ProjectMonitor monitor = _activator.Invoke(project.FullName);
                        if (_watchList.Contains(project.FullName)) return;
                        else _watchList.Add(project.FileName, monitor);

                        if (Configuration.Project.TryLoad(configFile, out Configuration.Project config, out string error))
                        {
                            monitor?.Start(config);
                            if (UserState.Instance.WatcherEnabled)
                            {
                                DTE.StatusBar.Text = msg;
                                System.Diagnostics.Debug.WriteLine(msg);
                            }
                            else monitor?.Pause();
                        }
                        else
                        {
                            DTE.StatusBar.Text = $"[{nameof(WebFlow)}] {error}";
                            System.Diagnostics.Debug.WriteLine(error);
                        }
                    }
                    else if (autoConfg && project.IsaWebProject())
                    {
                        var config = Configuration.Project.CreateDefault(Path.Combine(folder, ConfigName), project.Name);
                        config.Save();

                        ProjectMonitor monitor = _activator.Invoke(project.FullName);
                        _watchList.Add(project.FileName, monitor);
                        monitor?.Start(config);
                        if (UserState.Instance.WatcherEnabled)
                        {
                            DTE.StatusBar.Text = msg;
                            System.Diagnostics.Debug.WriteLine(msg);
                        }
                        else monitor?.Pause();
                    }
                }
        }

        internal async void OnValidationError(object sender, System.Xml.Schema.ValidationEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            DTE.StatusBar.Text = $"{nameof(WebFlow)} | Configuration file is not well-formed.";
            _outputPane?.OutputStringThreadSafe($"[{e.Severity}] {e.Message}{Environment.NewLine}");
        }

        private void SubscribeToEvents()
        {
            Microsoft.VisualStudio.Shell.Events.SolutionEvents.OnAfterOpenSolution += OnSolutionLoaded;
            Microsoft.VisualStudio.Shell.Events.SolutionEvents.OnBeforeCloseSolution += OnSolutionClosing;
        }

        private async void InitializeComponents()
        {
            System.Diagnostics.Debug.WriteLine("Initializing components ...");
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            _watchList = new ConcurrentDictionary<string, ProjectMonitor>();

            DTE = (DTE2)GetService(typeof(DTE));
            _outputPane = CreateOutputPane();
            _errorListProvider = new ErrorListProvider(this);
            _solution = (IVsSolution)GetService(typeof(SVsSolution));
            _activator = delegate (string projectFile)
            {
                return new ProjectMonitor(OnValidationError, BeforeFileCompilation, async (token, cwd) => { await AfterFileCompilationAsync(token, cwd); });
            };

            System.Diagnostics.Debug.WriteLine("components initialized!");
        }

        private async Task RegisterCommandsAsync(CancellationToken cancellationToken = default)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            WatchCommand.Initialize(this);
            ReloadCommand.Initialize(this);
            TranspileCommand.Initialize(this);
            AddConfigCommand.Initialize(this);
            CompileOnBuildCommand.Initialize(this);
        }

        private IVsOutputWindowPane CreateOutputPane(string title = nameof(WebFlow))
        {
            var outputWindow = (IVsOutputWindow)GetService(typeof(SVsOutputWindow));
            var guid = new Guid("74DD798D-3460-4724-82DC-82E3EB70AC2B");
            outputWindow.CreatePane(ref guid, title, 1, 1);
            outputWindow.GetPane(ref guid, out IVsOutputWindowPane pane);
            return pane;
        }

        // Event Handlers
        // ================================================================================

        private void OnSolutionClosing(object sender = null, EventArgs e = null)
        {
            System.Diagnostics.Debug.WriteLine("solution closed");

            if (_watchList != null)
                foreach (string projectFile in _watchList.Keys)
                {
                    if (_watchList[projectFile] is ProjectMonitor monitor)
                    {
                        monitor.Pause();
                        System.Diagnostics.Debug.WriteLine($"[{nameof(WebFlow)}] Stopped watching '{monitor.DirectoryName}'.");
                    }
                }
        }

        private async void OnSolutionLoaded(object sender = null, EventArgs e = null)
        {
            System.Diagnostics.Debug.WriteLine("soltion was opened");
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            var userOptions = (OptionsPage)GetDialogPage(typeof(OptionsPage));

            if (_notLoading)
            {
                _notLoading = false;
                System.Diagnostics.Debug.WriteLine("entered soltuion load thread. " + _notLoading);
                foreach (EnvDTE.Project project in DTE.GetActiveProjects())
                {
                    ReloadCommand.Instance.Execute(project, (userOptions.AutoConfig && project.IsaWebProject()));
                }

                //LoadSolution(userOptions.AutoConfig);
                _notLoading = true;
            }
        }

        private void BeforeFileCompilation(ICompilierOptions options, string cwd)
        {
            if (!string.IsNullOrEmpty(options.SourceFile))
            {
                string msg = $"[{options.Kind}]'n {options.SourceFile.ToFriendlyName(cwd)} ...{Environment.NewLine}";
                _outputPane.OutputStringThreadSafe(msg);
                DTE.StatusBar.Text = $"{nameof(WebFlow)} | {msg}";
                DTE.StatusBar.Animate(true, _statusbarIcon);
            }
        }

        private async Task AfterFileCompilationAsync(ProgressToken token, string cwd)
        {
            // Stoping status bar animation.
            DTE.StatusBar.Animate(false, _statusbarIcon);

            if (token.Result.Succeeded)
            {
                // Updating the error-list.
                Glob pattern = token.Result.SourceFile ?? string.Empty;
                for (int i = 0; i < _errorListProvider.Tasks.Count; i++)
                {
                    if (pattern.IsMatch(_errorListProvider.Tasks[i].Document))
                    {
                        _errorListProvider.Tasks.RemoveAt(i);
                        i = -1;
                    }
                }
                _errorListProvider.Tasks.Clear();

                // Updating the output pane with the results.
                string msg = $"[{token.Result.Kind}] '{token.Result.SourceFile.ToFriendlyName(cwd)}' => '{token.Result.OutputFile.ToFriendlyName(cwd)}' in {TimeSpan.FromTicks(token.Result.ExecutionTime).ToString(@"mm\:ss\.fff")}\r\n";
                System.Diagnostics.Debug.WriteLine(msg);
                _outputPane.OutputStringThreadSafe(msg);
            }
            else
            {
                string projectFile = cwd.GetFiles("*.*proj", SearchOption.TopDirectoryOnly).FirstOrDefault();

                // Adding new error to error-list.
                if (_solution.GetProjectOfUniqueName(projectFile, out IVsHierarchy hierarchy) == 0)
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
                            HierarchyItem = hierarchy,
                            Text = $"(WF) {error.Message}",
                            Category = TaskCategory.BuildCompile,
                            ErrorCategory = ((TaskErrorCategory)error.Category)
                        };

                        newError.Navigate += delegate (object sender, EventArgs e)
                        {
                            newError.Line++;
                            _errorListProvider.Navigate(newError, _editorGuid);
                            newError.Line--;
                        };
                        _errorListProvider.Tasks.Add(newError);
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"[{newError.ErrorCategory}] '{Path.GetFileName(newError.Document)}' {newError.Line}");
                        System.Diagnostics.Debug.WriteLine($"{newError.Text}");
#endif
                    }
                    _errorListProvider.Show();
                }
            }
        }

        private void OnConfigChanged(object sender, string configFile)
        {
        }

        #region Base Members

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            //await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            InitializeComponents();
            SubscribeToEvents();
            await RegisterCommandsAsync();
            OnSolutionLoaded();
            System.Diagnostics.Debug.WriteLine($"----- exited {nameof(InitializeAsync)} -----");
        }

        protected override void Dispose(bool disposing)
        {
            System.Diagnostics.Debug.WriteLine("!!!!! Disposing !!!!!");
            if (disposing)
            {
                _errorListProvider?.Dispose();
                UserState.Instance?.Dispose();

                foreach (IDisposable obj in _watchList.Values) obj?.Dispose();
                _watchList.Clear();
            }
            base.Dispose(disposing);
        }

        #endregion Base Members

        #region Private Members

        internal readonly Guid _editorGuid = new Guid(EnvDTE.Constants.vsViewKindTextView);
        internal readonly object _statusbarIcon = (short)Microsoft.VisualStudio.Shell.Interop.Constants.SBAI_General;

        internal IVsSolution _solution;
        internal IDictionary _watchList;
        internal IVsOutputWindowPane _outputPane;
        internal volatile bool _notLoading = true;
        internal ProjectMonitorActivator _activator;
        internal ErrorListProvider _errorListProvider;

        #endregion Private Members
    }
}