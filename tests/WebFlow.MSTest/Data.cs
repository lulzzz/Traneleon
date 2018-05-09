using System;
using System.IO;
using System.Linq;

namespace Acklann.WebFlow
{
	public static partial class TestFile
	{
		public const string FOLDER_NAME = "TestData";

		public static string DirectoryName
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FOLDER_NAME); }
        }

		public static FileInfo GetFile(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            string searchPattern = $"*{Path.GetExtension(fileName)}";

            string appDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FOLDER_NAME);
            return new DirectoryInfo(appDirectory).EnumerateFiles(searchPattern, SearchOption.AllDirectories)
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

		public static FileInfo GetGoodConfigJSON() => GetFile(@"good_config.json");

		public static FileInfo GetGoodConfigXML() => GetFile(@"good_config.xml");

		public static FileInfo GetScript1TS() => GetFile(@"script1.ts");

		public static FileInfo GetStyle1SCSS() => GetFile(@"style1.scss");

		public static FileInfo GetPartialSCSS() => GetFile(@"_partial.scss");

	}
}