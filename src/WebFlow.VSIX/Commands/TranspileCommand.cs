using Acklann.WebFlow.Utilities;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace Acklann.WebFlow.Commands
{
    public class TranspileCommand
    {
        private TranspileCommand(IMenuCommandService service, DTE2 dte, IDictionary watchList)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            _watchList = watchList ?? throw new ArgumentNullException(nameof(watchList));
            _dte = dte ?? throw new ArgumentNullException(nameof(dte));

            var selectionCommand = new OleMenuCommand(OnCommandInvoked, null, OnQueryingStatus, new CommandID(Symbols.CmdSet.Guid, Symbols.CmdSet.CompileSelectionCommandId));
            service.AddCommand(selectionCommand);

            var solutionCommand = new OleMenuCommand(OnCommandInvoked, null, OnQueryingStatus, new CommandID(Symbols.CmdSet.Guid, Symbols.CmdSet.CompileSolutionCommandId));
            service.AddCommand(solutionCommand);
        }

        public void Execute(string projectFile, params string[] files)
        {
            if (_watchList.Contains(projectFile) && (_watchList[projectFile] is ProjectMonitor monitor))
            {
                foreach (string fn in files)
                    if (Path.GetExtension(fn).EndsWith("proj", StringComparison.OrdinalIgnoreCase))
                    {
                        monitor?.Compile();
                    }
                    else if (Path.HasExtension(fn))
                    {
                        monitor?.Compile(fn);
                    }
                    else if (Directory.Exists(fn))
                    {
                        monitor?.Compile(new DirectoryInfo(fn));
                    }
            }
        }

        public void Execute(IEnumerable<EnvDTE.Project> projects)
        {
            foreach (EnvDTE.Project proj in projects)
                if (!string.IsNullOrEmpty(proj?.FullName))
                {
                    Execute(proj.FullName, proj.FullName);
                }
        }

        protected void OnCommandInvoked(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (sender is MenuCommand command)
                    switch (command.CommandID.ID)
                    {
                        case Symbols.CmdSet.CompileSelectionCommandId:
                            foreach (EnvDTE.SelectedItem item in _dte.GetSelectedItems())
                            {
                                if (item.ProjectItem?.FileCount > 0)
                                {
                                    string selectedFile = item.ProjectItem.FileNames[0];
                                    string projectFile = item.ProjectItem.ContainingProject?.FullName;
                                    Execute(projectFile, selectedFile);
                                }
                                else if (!string.IsNullOrEmpty(item.Project?.FullName))
                                {
                                    Execute(item.Project.FullName, item.Project.FullName);
                                }
                            }
                            break;

                        case Symbols.CmdSet.CompileSolutionCommandId:
                            Execute(_dte.GetActiveProjects());
                            break;
                    }
            });
        }

        protected void OnQueryingStatus(object sender, EventArgs e)
        {
            if (sender is OleMenuCommand command)
            {
                EnvDTE.SelectedItem item = _dte.GetSelectedItems().FirstOrDefault();
                if (string.IsNullOrEmpty(item?.Project?.FullName) == false)
                {
                    bool configFileExist = Directory.EnumerateFiles(Path.GetDirectoryName(item.Project.FullName), "*webflow*").FirstOrDefault() != null;
                    command.Visible = configFileExist;
                }
            }
        }

        #region Singleton

        public static TranspileCommand Instance { get; private set; }

        public static void Initialize(VSPackage package)
        {
            var service = package.GetService<IMenuCommandService>();
            Instance = new TranspileCommand(service, package.DTE, package._watchList);
        }

        #endregion Singleton

        #region Private Members

        private readonly DTE2 _dte;
        private readonly IDictionary _watchList;

        #endregion Private Members
    }
}