using Acklann.WebFlow.Utilities;
using EnvDTE80;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using NuGet.VisualStudio;
using System;
using System.ComponentModel.Design;
using System.Linq;
using Task =  System.Threading.Tasks.Task;
using System.Windows.Forms;

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
            System.Diagnostics.Debug.WriteLine("setup compile-on-build command invoked.");

            EnvDTE.Project selectedProject = _dte.GetSelectedProjects(defaultToStartup: false).FirstOrDefault();
            if (selectedProject != null)
            {
                AddConfigCommand.Instance.Execute(new[] { selectedProject });
                // TODO: Add nuget package
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
                    {
                        Task.Run(() =>
                        {
                            try
                            {
                                _dte.StatusBar.Animate(true, EnvDTE.vsStatusAnimation.vsStatusAnimationSync);
                                var installer = componentModel.GetService<IVsPackageInstaller>();
                                installer.InstallPackage(null, selectedProject, packageId, version, false);
                                _dte.StatusBar.Text = string.Format("[{1}] Finished installing the '{0}' package.", packageId, nameof(WebFlow));
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine(ex.Message);
                                _dte.StatusBar.Text = string.Format("[{2}] Unable to install the '{0}.{1}' package.", packageId, version, nameof(WebFlow));
                            }
                            finally
                            {
                                _dte.StatusBar.Animate(false, EnvDTE.vsStatusAnimation.vsStatusAnimationSync);
                            }
                        });
                    }
                }
                else MessageBox.Show(string.Format("The {0} project has already been configured.", selectedProject.Name));
            }
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