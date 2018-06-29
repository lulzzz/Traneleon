using Acklann.WebFlow.Utilities;
using EnvDTE80;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;

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

        public void TranspileProject(IEnumerable<EnvDTE.Project> projects)
        {
            foreach (EnvDTE.Project project in projects)
            {
                if (_watchList.Contains(project.FileName) && (_watchList[project.FileName] is ProjectMonitor monitor))
                {
                    monitor.Compile();
                }
            }
        }

        private void OnCommandInvoked(object sender, EventArgs e)
        {
            foreach (EnvDTE.SelectedItem item in _dte.GetSelectedItems())
            {
                string ext = Path.GetExtension(item.Name ?? string.Empty).ToLowerInvariant();
                if (ext.EndsWith("proj", StringComparison.OrdinalIgnoreCase) && item.Project != null) TranspileProject(new[] { item.Project });
                else if (item.ProjectItem != null) TranspileFile(item.ProjectItem);
            }
        }

        #region Singleton

        public static TranspileCommand Instance { get; private set; }

        public static void Initialize(VSPackage package)
        {
            Instance = new TranspileCommand(package._commandService, package.DTE, package._watchList);
        }

        #endregion Singleton

        #region Private Members

        private readonly DTE2 _dte;
        private readonly IDictionary _watchList;

        #endregion Private Members
    }
}