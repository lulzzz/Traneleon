using System;
using System.ComponentModel.Design;

namespace Acklann.WebFlow.Commands
{
    public delegate void Reset(bool auto);

    public delegate void Foo();

    public class RefreshCommand
    {
        public RefreshCommand(IMenuCommandService commandService, Action<bool> load, Action clear)
        {
            _loadSolution = load ?? throw new ArgumentNullException(nameof(load));
            _clearSolution = clear ?? throw new ArgumentNullException(nameof(clear));

            commandService.AddCommand(new MenuCommand(OnCommandInvoked, new CommandID(Symbols.CmdSet.Guid, Symbols.CmdSet.RefreshCommandIdId)));
        }

        protected void OnCommandInvoked(object sender, EventArgs e)
        {
            _clearSolution();
            _loadSolution(false);
        }

        #region Singleton

        public static RefreshCommand Instance { get; private set; }

        public static void Initialize(VSPackage package)
        {
            var service = package.GetService<IMenuCommandService>();
            Instance = new RefreshCommand(service, package.LoadSolution, package.ClearWatchList);
        }

        #endregion Singleton

        #region Private Members

        private readonly Action _clearSolution;
        private readonly Action<bool> _loadSolution;

        #endregion Private Members
    }
}