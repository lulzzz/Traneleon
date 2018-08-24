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
			public static readonly int VSToolsMenuGroupId = 0x100;
			public static readonly int SolutionToolbarGroupId = 0x102;
			public static readonly int FileContextMenuGroupId = 0x103;
			public static readonly int ProjectContextMenuGroupId = 0x104;
			public static readonly int FileCommandGroupId = 0x0105;
			public static readonly int ProjectCommandGroupId = 0x106;
			public static readonly int SolutionCommandGroupId = 0x107;
			public static readonly int SettingsCommandGroupId = 0x108;
			public static readonly int PromotionCommandGroupId = 0x109;
			public static readonly int ToolsMenuId = 0x201;
			public static readonly int WatchCommandIdId = 0x301;
			public static readonly int ReloadCommandIdId = 0x302;
			public static readonly int AddConfigFileCommandIdId = 0x303;
			public static readonly int CompileOnBuildCommandIdId = 0x304;
			public static readonly int CompileSolutionCommandIdId = 0x305;
			public static readonly int CompileSelectionCommandIdId = 0x306;
			public static readonly int DonateCommandIdId = 0x401;
			public static readonly int SettingsCommandIdId = 0x402;
			public static readonly int GotoWebsiteCommandIdId = 0x403;
		}
	}
}
