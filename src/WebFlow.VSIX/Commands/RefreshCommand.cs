using System;
using System.Collections;
using System.ComponentModel.Design;

namespace Acklann.WebFlow.Commands
{
    internal static class RefreshCommand
    {
        public static void Initialize(Guid cmdset, IMenuCommandService service, IDictionary watchList)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            _watchList = watchList ?? throw new ArgumentNullException(nameof(watchList));

            service.AddCommand(new MenuCommand(OnCommandInvoked, new CommandID(cmdset, Symbols.CmdSet.RefreshCommandIdId)));
        }

        private static void OnCommandInvoked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("refesh command invoked");
        }

        #region Private Members

        private static IDictionary _watchList;

        #endregion Private Members
    }
}