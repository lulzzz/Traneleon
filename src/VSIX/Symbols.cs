using System;

namespace Acklann.Traneleon
{
	public static class Symbols
	{
		public struct Package
		{
			public const string GuidString = "D14220FC-C8B7-41FF-95E6-3C5A122354D6";
			public static readonly Guid Guid = new Guid("D14220FC-C8B7-41FF-95E6-3C5A122354D6");
		}
		public struct CmdSet
		{
			public const string GuidString = "4c483e5b-2754-467e-ae2a-6b495e4ef2d4";
			public static readonly Guid Guid = new Guid("4c483e5b-2754-467e-ae2a-6b495e4ef2d4");
			public const int VSToolsMenuGroup = 0x100;
			public const int SolutionExplorerGroup = 0x102;
			public const int FileContextMenuGroup = 0x103;
			public const int ProjectContextMenuGroup = 0x104;
			public const int SolutionContextMenuGroup = 0x110;
			public const int FileCommandGroup = 0x0105;
			public const int ProjectCommandGroup = 0x106;
			public const int SolutionCommandGroup = 0x107;
			public const int SettingsCommandGroup = 0x108;
			public const int PromotionCommandGroup = 0x109;
			public const int ToolsMenu = 0x201;
			public const int WatchCommandId = 0x301;
			public const int ReloadCommandId = 0x302;
			public const int AddConfigFileCommandId = 0x303;
			public const int CompileOnBuildCommandId = 0x304;
			public const int CompileSolutionCommandId = 0x305;
			public const int CompileSelectionCommandId = 0x306;
			public const int DonateCommandId = 0x401;
			public const int SettingsCommandId = 0x402;
			public const int GotoWebsiteCommandId = 0x403;
		}
	}
}
