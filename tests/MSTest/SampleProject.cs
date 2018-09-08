using Acklann.GlobN;
using System;
using System.IO;

namespace Acklann.Traneleon
{
    public static partial class SampleProject
    {
        static SampleProject()
        {
            DirectoryName = $"../../../{nameof(Traneleon)}.Sample".ExpandPath(AppContext.BaseDirectory);
        }

        public static string DirectoryName { get; }

        public static void Clean(string searchPattern = "*")
        {
            foreach (string file in Directory.EnumerateFiles(Path.Combine(DirectoryName, "wwwroot"), searchPattern, SearchOption.AllDirectories))
                foreach (Glob pattern in new[] { "*.min.*", "*.js" })
                    if (pattern.IsMatch(file))
                    {
                        File.Delete(file);
                    }
        }

        public static FileInfo GetFile(string fileName) => TestFile.GetFile(fileName, DirectoryName);
    }
}