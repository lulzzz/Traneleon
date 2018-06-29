using Acklann.WebFlow.Utilities;
using EnvDTE80;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using NuGet.VisualStudio;
using System;
using System.ComponentModel.Design;

namespace Acklann.WebFlow.Commands
{
    internal class CompileOnBuildCommand
    {
        private CompileOnBuildCommand(IMenuCommandService service, DTE2 dte)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            _dte = dte ?? throw new ArgumentNullException(nameof(dte));

            service.AddCommand(new MenuCommand(OnCommandInvoked, new CommandID(Symbols.CmdSet.Guid, Symbols.CmdSet.CompileOnBuildCommandIdId)));
        }

        private void OnCommandInvoked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("compile-on-build command invoked");

            AddConfigCommand.Instance.Execute(_dte.GetSelectedProjects());
            // TODO: Add nuget package
            System.Diagnostics.Debug.WriteLine("don't forget to import nuget package.");

            if (CheckIfTargetAreInstalled())
            {
                var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
                var nuget = componentModel.GetService<IVsPackageInstaller>();
            }
        }

        private bool CheckIfTargetAreInstalled()
        {
            return false;
        }

        #region Singleton

        public static CompileOnBuildCommand Instance { get; private set; }

        public static void Initialize(VSPackage package)
        {
            Instance = new CompileOnBuildCommand(package._commandService, package.DTE);
        }

        #endregion Singleton

        #region Private Members

        private readonly DTE2 _dte;

        #endregion Private Members
    }
}