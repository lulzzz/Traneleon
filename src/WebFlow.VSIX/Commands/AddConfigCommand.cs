﻿using Acklann.WebFlow.Configuration;
using Acklann.WebFlow.Utilities;
using EnvDTE80;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Acklann.WebFlow.Commands
{
    public sealed class AddConfigCommand
    {
        public AddConfigCommand(IMenuCommandService commandService, DTE2 dte, IDictionary watchList, ProjectMonitorActivator generator)
        {
            if (commandService == null) throw new ArgumentNullException(nameof(commandService));
            _watchList = watchList ?? throw new ArgumentNullException(nameof(watchList));
            _activator = generator ?? throw new ArgumentNullException(nameof(generator));
            _dte = dte ?? throw new ArgumentNullException(nameof(dte));

            commandService.AddCommand(new MenuCommand((s, e) => Execute(_dte.GetSelectedProjects()), new CommandID(Symbols.CmdSet.Guid, Symbols.CmdSet.AddConfigFileCommandIdId)));
        }

        public void Execute(IEnumerable<EnvDTE.Project> activeProjects)
        {
            Task.Run(() =>
            {
                ProjectMonitor monitor;

                foreach (EnvDTE.Project project in activeProjects)
                {
                    if (_watchList.Contains(project.FullName))
                    {
                        monitor = _watchList[project.FullName] as ProjectMonitor;
                    }
                    else
                    {
                        // Creating a .config file if it does not already exists.
                        string folder = Path.GetDirectoryName(project.FullName);
                        string configFile = Directory.EnumerateFiles(folder, "*webflow*").FirstOrDefault();

                        if (configFile == null)
                        {
                            configFile = Path.Combine(folder, VSPackage.ConfigName);
                            Configuration.Project.CreateDefault(configFile).Save();
                        }

                        // Initiailizing watcher.
                        monitor = _activator.Invoke(project.FullName);
                        _watchList.Add(project.FileName, monitor);

                        if (Project.TryLoad(configFile, out Project config, out string error))
                        {
                            monitor?.Start(config);
                            _dte.StatusBar.Text = $"[{nameof(WebFlow)}] Added the '{project.Name}' project to the watch-list.";
                        }
                        else
                        {
                            _dte.StatusBar.Text = $"[{nameof(WebFlow)}] {error}";
                        }
                    }

                    if (UserState.Instance.WatcherEnabled) monitor?.Resume();
                    else monitor?.Pause();
                }
            });
        }

        #region Singleton

        public static AddConfigCommand Instance { get; private set; }

        public static void Initialize(VSPackage package)
        {
            var service = package.GetService<IMenuCommandService>();
            Instance = new AddConfigCommand(service, package.DTE, package._watchList, package._activator);
        }

        #endregion Singleton

        #region Private Members

        private readonly DTE2 _dte;
        private readonly IDictionary _watchList;
        private readonly ProjectMonitorActivator _activator;

        #endregion Private Members
    }
}