using Acklann.WebFlow.Commands;
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
                    string msg = string.Format("[{1}] Monitoring '{0}' for changes...", project.Name, nameof(WebFlow));

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
                return new ProjectMonitor(OnValidationError, reporter: new Reporter(projectFile, this));
            };

            System.Diagnostics.Debug.WriteLine("components initialized!");
        }

        private async Task RegisterCommandsAsync(CancellationToken cancellationToken = default)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            WatchCommand.Initialize(this);
            RefreshCommand.Initialize(this);
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
                LoadSolution(userOptions.AutoConfig);
                _notLoading = true;
            }
        }

        private async void OnValidationError(object sender, System.Xml.Schema.ValidationEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            DTE.StatusBar.Text = $"[{nameof(WebFlow)}] Configuration file is not well-formed.";
            string msg = $"[{e.Severity}] {e.Message}{Environment.NewLine}";
            _outputPane?.OutputStringThreadSafe(msg);
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

        internal IVsSolution _solution;
        internal IDictionary _watchList;
        internal IVsOutputWindowPane _outputPane;
        internal volatile bool _notLoading = true;
        internal ProjectMonitorActivator _activator;
        internal ErrorListProvider _errorListProvider;

        #endregion Private Members
    }
}