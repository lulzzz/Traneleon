using Acklann.WebFlow.Configuration;
using Acklann.WebFlow.Utilities;
using EnvDTE80;
using System;
using System.Collections;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Xml.Schema;

namespace Acklann.WebFlow.Commands
{
    public class ReloadCommand
    {
        public ReloadCommand(IMenuCommandService commandService, IDictionary watchList, DTE2 dte, ValidationEventHandler validationHandler, ProjectMonitorActivator activator)
        {
            _dte = dte ?? throw new ArgumentNullException(nameof(dte));
            _watchList = watchList ?? throw new ArgumentNullException(nameof(watchList));
            _activator = activator ?? throw new ArgumentNullException(nameof(activator));
            _validationHandler = validationHandler ?? throw new ArgumentNullException(nameof(validationHandler));

            var command = new MenuCommand(OnCommandInvoked, new CommandID(Symbols.CmdSet.Guid, Symbols.CmdSet.ReloadCommandId));
            commandService.AddCommand(command);
        }

        public void Execute(string projectFile, bool autoConfigEnabled = false)
        {
            string directory = Path.GetDirectoryName(projectFile);
            string configFile = Directory.EnumerateFiles(directory, "*webflow*", SearchOption.TopDirectoryOnly).FirstOrDefault();

            if (!string.IsNullOrEmpty(configFile))
            {
                if (_watchList.Contains(projectFile))
                {
                    (_watchList[projectFile] as IDisposable)?.Dispose();
                }

                if (Project.TryLoad(configFile, _validationHandler, out Project config))
                {
                    ProjectMonitor monitor = _activator.Invoke(projectFile);
                    _watchList.Add(projectFile, monitor);
                    monitor?.Start(config);

                    if (UserState.Instance.WatcherEnabled)
                    {
                        string msg = $"{nameof(WebFlow)} | monitoring '{projectFile}' for changes ...";
                        System.Diagnostics.Debug.WriteLine(msg);
                        _dte.StatusBar.Text = msg;
                    }
                    else monitor?.Pause();
                }
            }
            else if (autoConfigEnabled)
            {
                AddConfigCommand.Instance.Execute(projectFile);
            }
        }

        public void Execute(EnvDTE.Project project, bool autoConfigEnabled = false) => Execute(project.FullName, autoConfigEnabled);

        protected void OnCommandInvoked(object sender, EventArgs e)
        {
            foreach (EnvDTE.Project project in _dte.GetSelectedProjects(defaultToStartup: false))
            {
                Execute(project.FullName);
            }
        }

        #region Singleton

        public static ReloadCommand Instance { get; private set; }

        public static void Initialize(VSPackage package)
        {
            var service = package.GetService<IMenuCommandService>();
            Instance = new ReloadCommand(service, package._watchList, package.DTE, package.OnValidationError, package._activator);
        }

        #endregion Singleton

        #region Private Members

        private readonly DTE2 _dte;
        private readonly IDictionary _watchList;
        private readonly ProjectMonitorActivator _activator;
        private readonly ValidationEventHandler _validationHandler;

        #endregion Private Members
    }
}