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
    public class TranspileCommand
    {
        private TranspileCommand(IMenuCommandService service, DTE2 dte, IDictionary watchList)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            _watchList = watchList ?? throw new ArgumentNullException(nameof(watchList));
            _dte = dte ?? throw new ArgumentNullException(nameof(dte));

            service.AddCommand(new MenuCommand(OnCommandInvoked, new CommandID(Symbols.CmdSet.Guid, Symbols.CmdSet.CompileSolutionCommandIdId)));
            service.AddCommand(new MenuCommand(OnCommandInvoked, new CommandID(Symbols.CmdSet.Guid, Symbols.CmdSet.CompileSelectionCommandIdId)));
        }

        public void TranspileFile(EnvDTE.ProjectItem file)
        {
            if (file != null)
            {
                if (file.ContainingProject != null
                    && _watchList.Contains(file.ContainingProject.FileName)
                    && (_watchList[file.ContainingProject.FileName] is ProjectMonitor monitor))
                {
                    monitor.Compile(file.FileNames[0]);
                }
            }
        }

        public void TranspileProject(string projectFile)
        {
            if (_watchList.Contains(projectFile) && (_watchList[projectFile] is ProjectMonitor monitor))
            {
                monitor.Compile();
            }
        }

        public void TranspileProject(IEnumerable<string> projectFiles)
        {
            foreach (string fileName in projectFiles)
            {
                if (_watchList.Contains(fileName) && (_watchList[fileName] is ProjectMonitor monitor))
                {
                    monitor.Compile();
                }
            }
        }

        public void TranspileProject(IEnumerable<EnvDTE.Project> projects)
        {
            TranspileProject(projects.Select(x => x.FullName));
        }

        private void OnCommandInvoked(object sender, EventArgs e)
        {
            if (sender is MenuCommand command && command.CommandID.ID == Symbols.CmdSet.CompileSolutionCommandIdId)
            {
                foreach (EnvDTE.Project project in _dte.GetActiveProjects())
                {

                }
            }
            else
            {
                foreach (EnvDTE.SelectedItem item in _dte.GetSelectedItems())
                {
                    string ext = Path.GetExtension(item.Name ?? string.Empty).ToLowerInvariant();
                    if (ext.EndsWith("proj", StringComparison.OrdinalIgnoreCase) && item.Project != null) TranspileProject(item.Project.FullName);
                    else if (ext.EndsWith(".sln", StringComparison.OrdinalIgnoreCase)) TranspileProject(_dte.GetActiveProjects());
                    else if (item.ProjectItem != null) TranspileFile(item.ProjectItem);
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