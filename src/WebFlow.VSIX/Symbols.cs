using System;

namespace Acklann.WebFlow
{
	public static class Symbols
	{
		public struct Package
		{
			public const string GuidString = "b7b0e0af-d860-4e41-a041-4d1f62bc423c";
			public static readonly Guid Guid = new Guid("b7b0e0af-d860-4e41-a041-4d1f62bc423c");
		}
		public struct CmdSet
		{
			public const string GuidString = "4c483e5b-2754-467e-ae2a-6b495e4ef2d4";
			public static readonly Guid Guid = new Guid("4c483e5b-2754-467e-ae2a-6b495e4ef2d4");
			public static readonly int ProjectFileContextMenuGroupId = 0x0103;
			public static readonly int ProjectContextMenuGroupId = 0x0102;
			public static readonly int SolutionExplorerGroupId = 0x0105;
			public static readonly int FileContextMenuGroupId = 0x0107;
			public static readonly int ProjectCommandGroupId = 0x0108;
			public static readonly int GlobalCommandGroupId = 0x0110;
			public static readonly int ConfigCommandGroupId = 0x0106;
			public static readonly int ToolbarMenuGroupId = 0x0100;
			public static readonly int VSToolsMenuGroupId = 0x0104;
			public static readonly int InfoGroupId = 0x0109;
			public static readonly int ToolsMenuId = 0x0200;
			public static readonly int ContextMenuId = 0x0202;
			public static readonly int ToolbarMenuId = 0x0201;
			public static readonly int WatchCommandIdId = 0x0301;
			public static readonly int DonateCommandIdId = 0x0302;
			public static readonly int SettingsCommandIdId = 0x0303;
			public static readonly int GotoWebsiteCommandIdId = 0x0307;
			public static readonly int AddConfigFileCommandIdId = 0x0304;
			public static readonly int CompileOnBuildCommandIdId = 0x0305;
			public static readonly int CompileProjectCommandIdId = 0x0309;
			public static readonly int CompileSolutionCommandIdId = 0x0308;
			public static readonly int CompileSelectionCommandIdId = 0x0306;
		}
	}
}
