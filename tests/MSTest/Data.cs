using System;
using System.IO;
using System.Linq;

namespace Acklann.Traneleon
{
	public static partial class TestFile
	{
		public const string FOLDER_NAME = "TestData";

		public static string GetTempDir(string name) => Path.Combine(Path.GetTempPath(), name);

		public static string DirectoryName
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FOLDER_NAME); }
        }

		public static FileInfo GetFile(string fileName, string directory = null)
        {
            fileName = Path.GetFileName(fileName);
            string searchPattern = $"*{Path.GetExtension(fileName)}";

            if (string.IsNullOrEmpty(directory)) directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FOLDER_NAME);
            return new DirectoryInfo(directory).EnumerateFiles(searchPattern, SearchOption.AllDirectories)
                .First(x => x.Name.Equals(fileName, StringComparison.CurrentCultureIgnoreCase));
        }

        public static string GetContents(this string filePath)
		{
			return File.ReadAllText(filePath);
		}

		public static string GetContents(this FileInfo file)
		{
			return File.ReadAllText(file.FullName);
		}

		public static FileInfo GetBadScript1TS() => GetFile(@"bad_script1.ts");

		public static FileInfo GetBadStyle1SCSS() => GetFile(@"bad_style1.scss");

		public static FileInfo GetDummyJS() => GetFile(@"dummy.js");

		public static FileInfo GetEntry1HTML() => GetFile(@"entry1.html");

		public static FileInfo GetEntrypoint1TS() => GetFile(@"entryPoint1.ts");

		public static FileInfo GetGoodConfigJSON() => GetFile(@"good_config.json");

		public static FileInfo GetGoodConfigXML() => GetFile(@"good_config.xml");

		public static FileInfo GetMockConfigXML() => GetFile(@"mock_config.xml");

		public static FileInfo GetScript1TS() => GetFile(@"script1.ts");

		public static FileInfo GetScript2TS() => GetFile(@"script2.ts");

		public static FileInfo GetStyle1SCSS() => GetFile(@"style1.scss");

		public static FileInfo GetStyle2SCSS() => GetFile(@"style2.scss");

		public static FileInfo GetPartialSCSS() => GetFile(@"_partial.scss");

		public static FileInfo GetImg1aGIF() => GetFile(@"images\img1A.gif");

		public static FileInfo GetImg2BMP() => GetFile(@"images\img2.bmp");

		public static FileInfo GetImg3GIF() => GetFile(@"images\img3.gif");

		public static FileInfo GetImg4ICO() => GetFile(@"images\img4.ico");

		public static FileInfo GetImg5PNG() => GetFile(@"images\img5.png");

		public static FileInfo GetImg10PNG() => GetFile(@"images\lvl2\img10.png");

		public static FileInfo GetImg6TIF() => GetFile(@"images\lvl2\img6.tif");

		public static FileInfo GetImg7SVG() => GetFile(@"images\lvl2\img7.svg");

		public static FileInfo GetImg8JPG() => GetFile(@"images\lvl2\img8.jpg");

		public static FileInfo GetImg9JPG() => GetFile(@"images\lvl2\img9.jpg");

	}

	public static partial class SampleProject
	{
		public static FileInfo GetAppTS() => GetFile(@"scripts\app.ts");

		public static FileInfo GetButtonTS() => GetFile(@"scripts\components\button.ts");

		public static FileInfo GetGlobalCSS() => GetFile(@"stylesheets\global.css");

		public static FileInfo GetGlobalSCSS() => GetFile(@"stylesheets\global.scss");

		public static FileInfo GetLayoutSCSS() => GetFile(@"stylesheets\layout\_layout.scss");

		public static FileInfo GetResetSCSS() => GetFile(@"stylesheets\layout\_reset.scss");

		public static FileInfo GetHomeCSS() => GetFile(@"stylesheets\views\home.css");

		public static FileInfo GetHomeSCSS() => GetFile(@"stylesheets\views\home.scss");

	}
}
