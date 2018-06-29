using Acklann.WebFlow.Commands;
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
using System.Runtime.InteropServices;
using System.Threading;

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
        internal DTE2 DTE;

        internal T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        private void SubscribeToEvents()
        {
            _projectEvents = DTE.Events.SolutionItemsEvents;
            _itemAddedHandler = new _dispProjectItemsEvents_ItemAddedEventHandler(OnProjectItemAdded);
            _projectEvents.ItemAdded += _itemAddedHandler;

            Microsoft.VisualStudio.Shell.Events.SolutionEvents.OnAfterOpenSolution += OnSolutionLoaded;
            Microsoft.VisualStudio.Shell.Events.SolutionEvents.OnBeforeCloseSolution += OnSolutionClosing;
        }

        private void InitializeComponents()
        {
            _outputPane = CreateOutputPane();
            _akka = ActorSystem.Create("webflow-vsix");
            _errorListProvider = new ErrorListProvider(this);
            _solution = (IVsSolution)GetService<SVsSolution>();

            _watchList = new ConcurrentDictionary<string, ProjectMonitor>();
            _activator = delegate (string projectFile)
            {
                return new ProjectMonitor(new Reporter(projectFile, this), _akka);
            };
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

        private void OnProjectItemAdded(ProjectItem item)
        {
            System.Diagnostics.Debug.WriteLine("project item added invoked.");
        }

        private void OnSolutionLoaded(object sender = null, EventArgs e = null)
        {
            System.Diagnostics.Debug.WriteLine("soltion was opened");
            AddConfigCommand.Instance.Execute(DTE.GetActiveProjects());
        }

        private void OnSolutionClosing(object sender = null, EventArgs e = null)
        {
            System.Diagnostics.Debug.WriteLine("solution closed");
            foreach (EnvDTE.Project project in DTE.GetActiveProjects())
            {
                if (_watchList.Contains(project.FullName) && (_watchList[project.FullName] is ProjectMonitor monitor))
                {
                    monitor.Pause();
                    System.Diagnostics.Debug.WriteLine($"stopped watching {monitor.DirectoryName}");
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
            CreateOutputPane();
            RegisterCommands();
            OnSolutionLoaded();
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
        internal ProjectMonitorActivator _activator;
        internal IMenuCommandService _commandService;
        internal ErrorListProvider _errorListProvider;

        private EnvDTE.ProjectItemsEvents _projectEvents;
        private _dispProjectItemsEvents_ItemAddedEventHandler _itemAddedHandler;

        #endregion Private Members
    }
}