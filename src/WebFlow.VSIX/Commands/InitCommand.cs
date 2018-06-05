using Acklann.WebFlow.Utilities;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using Acklann.WebFlow.Utilities;

namespace Acklann.WebFlow.Commands
{
    public sealed class InitCommand
    {
        public InitCommand(IMenuCommandService commandService, DTE2 dte, IDictionary watchList, IVsStatusbar statusbar, ProjectMonitorActivator generator)
        {
            if (commandService == null) throw new ArgumentNullException(nameof(commandService));
            _watchList = watchList ?? throw new ArgumentNullException(nameof(watchList));
            _activator = generator ?? throw new ArgumentNullException(nameof(generator));
            _statusBar = statusbar ?? throw new ArgumentNullException(nameof(statusbar));
            _dte = dte ?? throw new ArgumentNullException(nameof(dte));

            commandService.AddCommand(new MenuCommand((s, e) => Execute(_dte.GetSelectedProjects()), new CommandID(Symbols.CmdSet.Guid, Symbols.CmdSet.InitCommandIdId)));
        }

        public void Execute(IEnumerable<EnvDTE.Project> projects)
        {
            //System.Diagnostics.Debug.WriteLine("invoked init command");
            ProjectMonitor monitor;

            foreach (EnvDTE.Project proj in projects)
            {
                if (_watchList.Contains(proj.FullName) == false)
                {
                    // Creating a .config file if it does not already exists.
                    string folder = Path.GetDirectoryName(proj.FullName);
                    string configFile = Directory.EnumerateFiles(folder, "*webflow*").FirstOrDefault();

                    if (configFile == null)
                    {
                        configFile = Path.Combine(folder, "webFlow.config");
                        Configuration.Project.CreateInstance(configFile).Save();
                    }

                    // Installing the nuget package if not already imported.
                    // TODO: Import the webflow  nuget package
                    //

                    // Initiailizing watcher.
                    monitor = _activator?.Invoke();
                    _watchList.Add(proj.FullName, monitor);
                    monitor?.Start(configFile);
                }
                else
                {
                    monitor = _watchList[proj.FullName] as ProjectMonitor;
                }

                if (UserState.Instance.WatcherEnabled)
                {
                    monitor.Resume();
                    _statusBar.Write($"Monitoring '{monitor.DirectoryName}' for changes...");
                }
                else monitor?.Pasuse();
            }
        }

        #region Singleton

        public static InitCommand Instance { get; private set; }

        public static void Initialize(VSPackage package)
        {
            var service = package.GetService<IMenuCommandService>();
            var statusbar = (IVsStatusbar)package.GetService<SVsStatusbar>();
            Instance = new InitCommand(service, package.DTE, package._watchList, statusbar, package._activator);
        }

        #endregion Singleton

        #region Private Members

        private readonly DTE2 _dte;
        private readonly IDictionary _watchList;
        private readonly IVsStatusbar _statusBar;
        private readonly ProjectMonitorActivator _activator;

        #endregion Private Members
    }
}