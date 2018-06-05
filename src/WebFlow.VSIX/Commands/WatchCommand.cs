using Acklann.WebFlow.Utilities;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections;
using System.ComponentModel.Design;

namespace Acklann.WebFlow.Commands
{
    internal sealed class WatchCommand
    {
        public WatchCommand(IMenuCommandService service, IDictionary watchList)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            _watchList = watchList ?? throw new ArgumentNullException(nameof(watchList));

            var command = new MenuCommand(OnCommandInvoked, new CommandID(Symbols.CmdSet.Guid, Symbols.CmdSet.WatchCommandIdId)) { Checked = UserState.Instance.WatcherEnabled };
            service.AddCommand(command);
        }

        private static void OnCommandInvoked(object sender, EventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("watch command was invoked.");

            if (sender is OleMenuCommand command)
            {
                command.Checked = UserState.Instance.WatcherEnabled = !UserState.Instance.WatcherEnabled;
                foreach (ProjectMonitor monitor in _watchList.Values)
                {
                    if (command.Checked) monitor?.Resume();
                    else monitor?.Pasuse();
                }
                UserState.Instance.Save();
            }
        }

        #region Singleton

        public static WatchCommand Instance { get; private set; }

        public static void Initialize(VSPackage package)
        {
            var service = package.GetService<IMenuCommandService>();
            Instance = new WatchCommand(service, package._watchList);
        }

        #endregion Singleton

        #region Private Members

        private static IDictionary _watchList;

        #endregion Private Members
    }
}