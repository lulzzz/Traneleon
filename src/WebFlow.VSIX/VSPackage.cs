﻿using Acklann.WebFlow.Commands;
using Acklann.WebFlow.Compilation;
using Acklann.WebFlow.Utilities;
using Akka.Actor;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel.Design;
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
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideOptionPage(typeof(OptionsPage), nameof(WebFlow), "General", 0, 0, true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class VSPackage : Package
    {
        public VSPackage()
        {
            _watchList = new ConcurrentDictionary<string, ProjectMonitor>();
            _akka = ActorSystem.Create("webflow");
        }

        internal const string ConfigName = "webflow-compiler.config";
        internal DTE2 DTE;

        internal T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        private void SubscribeToEvents()
        {
            Microsoft.VisualStudio.Shell.Events.SolutionEvents.OnAfterOpenSolution += OnSolutionLoaded;
            Microsoft.VisualStudio.Shell.Events.SolutionEvents.OnBeforeCloseSolution += OnSolutionClosing;
        }

        private void LoadEmbeddedModules()
        {
            DTE.StatusBar.Text = $"[{nameof(WebFlow)}] loading modules ...";
            Task.Run(() =>
            {
                ShellBase.LoadModules();
                System.Diagnostics.Debug.WriteLine($"Loaded the resources at {ShellBase.ResourceDirectory}.");
            });
        }

        private void InitializeComponents()
        {
            System.Diagnostics.Debug.WriteLine("Initializing components ...");

            _outputPane = CreateOutputPane();
            _errorListProvider = new ErrorListProvider(this);
            _solution = (IVsSolution)GetService<SVsSolution>();
            _activator = delegate (string projectFile)
            {
                return new ProjectMonitor(new Reporter(projectFile, this), _akka);
            };

            System.Diagnostics.Debug.WriteLine("components initialized!");
        }

        private void RegisterCommands(CancellationToken cancellationToken = default)
        {
            _commandService = GetService<IMenuCommandService>();

            //await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            WatchCommand.Initialize(this);
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

        private void OnSolutionLoaded(object sender = null, EventArgs e = null)
        {
            System.Diagnostics.Debug.WriteLine("soltion was opened");
            var userOptions = (OptionsPage)GetDialogPage(typeof(OptionsPage));

            if (_notLoading)
            //Task.Run(() =>
            {
                _notLoading = false;
                System.Diagnostics.Debug.WriteLine("entered soltuion load thread. " + _notLoading);

                foreach (EnvDTE.Project project in DTE.GetActiveProjects())
                    if (_watchList.Contains(project.FullName) == false)
                    {
                        string folder = Path.GetDirectoryName(project.FullName);
                        string configFile = Directory.EnumerateFiles(folder, "*webflow*").FirstOrDefault();

                        if (!string.IsNullOrEmpty(configFile))
                        {
                            ProjectMonitor monitor = _activator.Invoke(project.FullName);
                            if (_watchList.Contains(project.FullName)) return;
                            else _watchList.Add(project.FileName, monitor);

                            if (Configuration.Project.TryLoad(configFile, out Configuration.Project config, out string error))
                            {
                                monitor?.Start(config);
                                if (UserState.Instance.WatcherEnabled) DTE.StatusBar.Text = $"Monitoring '{project.Name}' for changes ...";
                                else monitor?.Pause();
                            }
                            else
                            {
                                DTE.StatusBar.Text = $"[{nameof(WebFlow)}] {error}";
                            }
                        }
                        else if (userOptions.AutoConfig && project.IsaWebProject())
                        {
                            var config = Configuration.Project.CreateDefault(Path.Combine(folder, ConfigName));
                            config.Save();

                            ProjectMonitor monitor = _activator.Invoke(project.FullName);
                            _watchList.Add(project.FileName, monitor);
                            monitor?.Start(config);
                            if (UserState.Instance.WatcherEnabled) DTE.StatusBar.Text = $"Monitoring '{project.Name}' for changes ...";
                            else monitor?.Pause();
                        }
                    }
                _notLoading = true;
                //});
            }
        }

        private void OnSolutionClosing(object sender = null, EventArgs e = null)
        {
            System.Diagnostics.Debug.WriteLine("solution closed");
            foreach (string projectFile in _watchList.Keys)
            {
                if (_watchList[projectFile] is ProjectMonitor monitor)
                {
                    monitor.Pause();
                    System.Diagnostics.Debug.WriteLine($"[{nameof(WebFlow)}] Stopped watching '{monitor.DirectoryName}'.");
                }
            }
        }

        #region Base Members

        protected override void Initialize()
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            //await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            DTE = (DTE2)GetService(typeof(DTE));

            InitializeComponents();
            SubscribeToEvents();
            RegisterCommands();
            LoadEmbeddedModules();
            //OnSolutionLoaded();
        }

        protected override void Dispose(bool disposing)
        {
            System.Diagnostics.Debug.WriteLine("!!!!! Disposing !!!!!");
            if (disposing)
            {
                _akka?.Dispose();
                _errorListProvider?.Dispose();
                UserState.Instance?.Dispose();

                foreach (IDisposable obj in _watchList.Values) obj?.Dispose();
                _watchList.Clear();
            }
            base.Dispose(disposing);
        }

        #endregion Base Members

        #region Private Members

        internal ActorSystem _akka;
        internal IVsSolution _solution;
        internal IDictionary _watchList;
        internal IVsOutputWindowPane _outputPane;
        internal volatile bool _notLoading = true;
        internal ProjectMonitorActivator _activator;
        internal IMenuCommandService _commandService;
        internal ErrorListProvider _errorListProvider;

        #endregion Private Members
    }
}