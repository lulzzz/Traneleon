using Acklann.WebFlow.Commands;
using Acklann.WebFlow.Utilities;
using Akka.Actor;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

namespace Acklann.WebFlow
{
    public delegate ProjectMonitor ProjectMonitorActivator();

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
            _akka = ActorSystem.Create("webflow-vsix");
            _watchList = new ConcurrentDictionary<string, ProjectMonitor>();
            _activator = delegate () { return new ProjectMonitor(null, _akka); };
        }

        internal DTE2 DTE;

        public void Reset()
        {
        }

        internal T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        private void RegisterCommands()
        {
            InitCommand.Initialize(this);
        }

        private void SubscribeToEvents()
        {
            _projectEvents = DTE.Events.SolutionItemsEvents;
            _itemAddedHandler = new _dispProjectItemsEvents_ItemAddedEventHandler(OnProjectItemAdded);
            _projectEvents.ItemAdded += _itemAddedHandler;

            Microsoft.VisualStudio.Shell.Events.SolutionEvents.OnAfterOpenSolution += OnSolutionLoaded;
            Microsoft.VisualStudio.Shell.Events.SolutionEvents.OnAfterCloseSolution += OnSolutionClosed;
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
        }

        private void OnSolutionClosed(object sender = null, EventArgs e = null)
        {
            System.Diagnostics.Debug.WriteLine("solution closed");
        }

        #region Base Members

        protected override void Initialize()
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            //await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            DTE = (DTE2)GetService(typeof(DTE));

            SubscribeToEvents();
            RegisterCommands();
            OnSolutionLoaded();

            var configFile = Path.Combine(Path.GetTempPath(), "webFlow.config");
            Configuration.Project.CreateInstance(configFile).Save();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _akka?.Dispose();
                UserState.Instance?.Dispose();

                foreach (IDisposable obj in _watchList.Values) obj?.Dispose();
                _watchList.Clear();
            }
            base.Dispose(disposing);
        }

        #endregion Base Members

        #region Private Members

        internal readonly ActorSystem _akka;
        internal readonly IDictionary _watchList;
        internal readonly ProjectMonitorActivator _activator;

        private EnvDTE.ProjectItemsEvents _projectEvents;
        private _dispSolutionEvents_OpenedEventHandler _solutionOpenedEventHandler;
        private _dispProjectItemsEvents_ItemAddedEventHandler _itemAddedHandler;
        private _dispProjectItemsEvents_ItemRemovedEventHandler _itemRemovedHandler;
        private _dispProjectItemsEvents_ItemRenamedEventHandler _itemRenamedHandler;

        #endregion Private Members
    }
}