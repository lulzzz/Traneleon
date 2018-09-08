using Acklann.WebFlow.Configuration;
using Acklann.WebFlow.Utilities;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;

namespace Acklann.WebFlow.Commands
{
    public class AddConfigCommand
    {
        public AddConfigCommand(IMenuCommandService commandService, DTE2 dte, IDictionary watchList, ProjectMonitorActivator generator)
        {
            if (commandService == null) throw new ArgumentNullException(nameof(commandService));
            _watchList = watchList ?? throw new ArgumentNullException(nameof(watchList));
            _activator = generator ?? throw new ArgumentNullException(nameof(generator));
            _dte = dte ?? throw new ArgumentNullException(nameof(dte));

            commandService.AddCommand(new OleMenuCommand(OnCommandInvoked, null, OnQueryingStatus, new CommandID(Symbols.CmdSet.Guid, Symbols.CmdSet.AddConfigFileCommandId)));
        }

        public void Execute(string projectFile)
        {
            if (_watchList.Contains(projectFile) == false)
            {
                string filename = Path.Combine(Path.GetDirectoryName(projectFile), VSPackage.ConfigName);
                var config = Project.CreateInstance(filename);
                config.Save();

                ProjectMonitor monitor = _activator.Invoke(projectFile);
                _watchList.Add(projectFile, monitor);
                monitor.Start(config);

                if (UserState.Instance.WatcherEnabled == false) monitor.Pause();
                System.Diagnostics.Debug.WriteLine($"created '{filename}'");
            }
        }

        protected void OnCommandInvoked(object sender, EventArgs e)
        {
            EnvDTE.SelectedItem item = _dte.GetSelectedItems().FirstOrDefault();
            if (!string.IsNullOrEmpty(item?.Project?.FullName))
            {
                Execute(item.Project.FullName);
            }
        }

        protected void OnQueryingStatus(object sender, EventArgs e)
        {
            if (sender is OleMenuCommand command)
            {
                EnvDTE.SelectedItem item = _dte.GetSelectedItems().FirstOrDefault();
                if (string.IsNullOrEmpty(item?.Project?.FullName) == false)
                {
                    bool configFileNotExists = string.IsNullOrEmpty(Project.FindConfigurationFile(Path.GetDirectoryName(item.Project.FullName)));
                    command.Visible = configFileNotExists;
                }
            }
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