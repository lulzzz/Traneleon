using Acklann.WebFlow.Utilities;
using System;
using System.Collections;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace Acklann.WebFlow.Commands
{
    internal sealed class WatchCommand
    {
        public WatchCommand(IMenuCommandService commandService, IDictionary watchList)
        {
            if (commandService == null) throw new ArgumentNullException(nameof(commandService));
            _watchList = watchList ?? throw new ArgumentNullException(nameof(watchList));

            var command = new MenuCommand(OnCommandInvoked, new CommandID(Symbols.CmdSet.Guid, Symbols.CmdSet.WatchCommandId)) { Checked = UserState.Instance.WatcherEnabled };
            commandService.AddCommand(command);
        }

        private static void OnCommandInvoked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("watch command was invoked.");

            if (sender is MenuCommand command)
            {
                Task.Run(() =>
                {
                    command.Checked = UserState.Instance.WatcherEnabled = !UserState.Instance.WatcherEnabled;
                    UserState.Instance.Save();
                    foreach (ProjectMonitor monitor in _watchList.Values)
                    {
                        if (command.Checked) monitor?.Resume();
                        else monitor?.Pause();
                    }
                });
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