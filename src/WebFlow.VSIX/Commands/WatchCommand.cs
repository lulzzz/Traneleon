using System;
using System.Collections;
using System.ComponentModel.Design;

namespace Acklann.WebFlow.Commands
{
    internal sealed class WatchCommand
    {
        internal static void Initialize(Guid cmdset, IMenuCommandService service, IDictionary watchList)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            _watchList = watchList ?? throw new ArgumentNullException(nameof(watchList));

            var command = new MenuCommand(OnCommandInvoked, new CommandID(cmdset, Symbols.CmdSet.WatchCommandIdId));
            service.AddCommand(command);
        }

        private static void OnCommandInvoked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("watch command was invoked.");
        }

        #region Private Members

        private static IDictionary _watchList;

        #endregion Private Members
    }
}