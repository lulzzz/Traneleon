using Acklann.WebFlow.Utilities;
using EnvDTE80;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using NuGet.VisualStudio;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Acklann.WebFlow.Commands
{
    internal class CompileOnBuildCommand
    {
        private CompileOnBuildCommand(IMenuCommandService service, DTE2 dte)
        {
            _dte = dte ?? throw new ArgumentNullException(nameof(dte));
            if (service == null) throw new ArgumentNullException(nameof(service));

            _installations = new Dictionary<string, bool>();
            service.AddCommand(new OleMenuCommand(OnCommandInvoked, null, OnQueryingStatus, new CommandID(Symbols.CmdSet.Guid, Symbols.CmdSet.CompileOnBuildCommandId)));
        }

        public void Execute()
        {
        }

        protected void OnCommandInvoked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("setup compile-on-build command invoked.");

            foreach (EnvDTE.Project selectedProject in _dte.GetSelectedProjects(defaultToStartup: false))
            {
                AddConfigCommand.Instance.Execute(selectedProject.FullName);

                // Ensuring the nuget package is installed.
                var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
                var nuget = componentModel.GetService<IVsPackageInstallerServices>();
                string packageId = $"{nameof(Acklann)}.{nameof(WebFlow)}";
                string version = Compilation.ShellBase.Version;
#if DEBUG
                version = $"{version}-rc";
#endif
                if (nuget.IsPackageInstalled(selectedProject, packageId) == false)
                {
                    DialogResult answer = MessageBox.Show("A NuGet package will be installed to augment the MSBuild process, but no files will be added to the project.\r\nThis may require an internet connection.\r\n\r\nDo you want to continue?", nameof(WebFlow), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (answer == DialogResult.Yes)
                        try
                        {
                            _dte.StatusBar.Animate(true, EnvDTE.vsStatusAnimation.vsStatusAnimationSync);
                            var installer = componentModel.GetService<IVsPackageInstaller>();
                            installer.InstallPackage(null, selectedProject, packageId, version, false);

                            string msg = string.Format("{1} | Finished installing the '{0}' package.", packageId, nameof(WebFlow));
                            System.Diagnostics.Debug.WriteLine(msg);
                            _dte.StatusBar.Text = msg;
                        }
                        catch (Exception ex)
                        {
                            string msg = string.Format("{2} | Unable to install the '{0}.{1}' package.", packageId, version, nameof(WebFlow));
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                            System.Diagnostics.Debug.WriteLine(msg);
                            _dte.StatusBar.Text = msg;
                        }
                        finally
                        {
                            _dte.StatusBar.Animate(false, EnvDTE.vsStatusAnimation.vsStatusAnimationSync);
                        }
                }
                else MessageBox.Show(string.Format("The {0} project has already been configured.", selectedProject.Name));
            }
        }

        protected void OnQueryingStatus(object sender, EventArgs e)
        {
            if (sender is OleMenuCommand command)
            {
                EnvDTE.SelectedItem item = _dte.GetSelectedItems().FirstOrDefault();
                if (string.IsNullOrEmpty(item?.Project?.FullName) == false)
                {
                    bool configFileNotExist = string.IsNullOrEmpty(Configuration.Project.FindConfigurationFile(Path.GetDirectoryName(item.Project.FullName)));
                    command.Visible = configFileNotExist;
                }
            }
        }

        #region Singleton

        public static CompileOnBuildCommand Instance { get; private set; }

        public static void Initialize(VSPackage package)
        {
            var service = package.GetService<IMenuCommandService>();
            Instance = new CompileOnBuildCommand(service, package.DTE);
        }

        #endregion Singleton

        #region Private Members

        private readonly DTE2 _dte;
        private readonly IDictionary<string, bool> _installations;

        #endregion Private Members
    }
}