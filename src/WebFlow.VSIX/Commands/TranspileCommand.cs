using System;
using System.Collections;
using System.ComponentModel.Design;

namespace Acklann.WebFlow.Commands
{
    public static class TranspileCommand
    {
        public static void Initialize(Guid cmdset, IMenuCommandService service, IDictionary watchList)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            _watchList = watchList ?? throw new ArgumentNullException(nameof(watchList));

            service.AddCommand(new MenuCommand(OnCommandInvoked, new CommandID(cmdset, Symbols.CmdSet.TranspileProjectCommandIdId)));
            service.AddCommand(new MenuCommand(OnCommandInvoked, new CommandID(cmdset, Symbols.CmdSet.TranspileSolutionCommandIdId)));
            service.AddCommand(new MenuCommand(OnCommandInvoked, new CommandID(cmdset, Symbols.CmdSet.TranspileSelectionCommandIdId)));
        }

        public static void Transpile()
        {
        }

        private static void OnCommandInvoked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("transpile command invoked");
        }

        #region Private Members

        private static IDictionary _watchList;

        #endregion Private Members
    }
}