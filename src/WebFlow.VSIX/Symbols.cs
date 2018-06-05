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
			public static readonly int SolutionCommandGroupId = 0x0105;
			public static readonly int ToolbarCommandGroupId = 0x0100;
			public static readonly int OptionsCommandGroupId = 0x0106;
			public static readonly int BuildSubMenuGroupId = 0x0101;
			public static readonly int VSToolsMenuGroupId = 0x0104;
			public static readonly int ToolBarMenuId = 0x0701;
			public static readonly int ToolsSubMenuId = 0x0700;
			public static readonly int InitCommandIdId = 0x0800;
			public static readonly int WatchCommandIdId = 0x0801;
			public static readonly int RefreshCommandIdId = 0x0802;
			public static readonly int SettingsCommandIdId = 0x0803;
			public static readonly int TranspileSolutionCommandIdId = 0x0804;
			public static readonly int TranspileSelectionCommandIdId = 0x0805;
			public static readonly int TranspileProjectCommandIdId = 0x0806;
		}
	}
}
