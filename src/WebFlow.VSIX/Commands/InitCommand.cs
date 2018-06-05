using Acklann.WebFlow.Utilities;
using EnvDTE80;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;

namespace Acklann.WebFlow.Commands
{
    public sealed class InitCommand
    {
        public InitCommand(IMenuCommandService commandService, DTE2 dte, IDictionary watchList, ProjectMonitorActivator generator)
        {
            if (commandService == null) throw new ArgumentNullException(nameof(commandService));
            _watchList = watchList ?? throw new ArgumentNullException(nameof(watchList));
            _activator = generator ?? throw new ArgumentNullException(nameof(generator));
            _dte = dte ?? throw new ArgumentNullException(nameof(dte));
            
            commandService.AddCommand(new MenuCommand((s, e) => Execute(_dte.GetSelectedProjects()), new CommandID(Symbols.CmdSet.Guid, Symbols.CmdSet.InitCommandIdId)));
        }

        public void Execute(IEnumerable<EnvDTE.Project> projects)
        {
            foreach (EnvDTE.Project proj in projects)
            {
                System.Diagnostics.Debug.WriteLine($"wokiing on {proj.FullName}");
                if (_watchList.Contains(proj.FullName) == false)
                {
                    string folder = Path.GetDirectoryName(proj.FullName);
                    string configFile = Directory.EnumerateFiles(folder, "*webflow*").FirstOrDefault();
                    if (configFile == null)
                    {
                        configFile = Path.Combine(folder, "webFlow.config");
                        Configuration.Project.CreateInstance(configFile).Save();
                    }

                    ProjectMonitor monitor = _activator?.Invoke();
                    _watchList.Add(proj.FullName, monitor);
                    monitor.Start(configFile);
                }
            }
            System.Diagnostics.Debug.WriteLine("invoked init command");
        }

        #region Singleton

        public static InitCommand Instance { get; private set; }

        public static void Initialize(VSPackage package)
        {
            var service = package.GetService<IMenuCommandService>();
            Instance = new InitCommand(service, package.DTE, package._watchList, package._activator);
        }

        #endregion Singleton

        #region Private Members

        private readonly DTE2 _dte;
        private readonly IDictionary _watchList;
        private readonly ProjectMonitorActivator _activator;

        #endregion Private Members
    }
}