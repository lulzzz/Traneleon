using Acklann.WebFlow.Compilation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Acklann.WebFlow.Configuration
{
    public static class ConfigurationExtensions
    {
        public static IEnumerable<ICompilierOptions> GetCompilierOptions(this Project project)
        {
            var itemGroups = project.GetItempGroups();

            foreach (string file in GetProjectFiles(project))
            {
                foreach (IItemGroup group in itemGroups)
                    if (group.Enabled)
                    {

                    }
            }

            throw new System.NotImplementedException();
        }

        public static IEnumerable<string> GetProjectFiles(this Project project, string pattern = "*")
        {
            foreach (string file in Directory.EnumerateFiles(project.DirectoryName, pattern))
            {
                yield return file;
            }

            foreach (string folder in (from p in Directory.GetDirectories(project.DirectoryName)
                                       where
                                        !p.EndsWith("node_modules", StringComparison.OrdinalIgnoreCase)
                                        &&
                                        !p.EndsWith("bower_components", StringComparison.OrdinalIgnoreCase)
                                       select p))
            {
                foreach (string file in Directory.EnumerateFiles(folder, pattern, SearchOption.AllDirectories))
                {
                    yield return file;
                }
            }
        }
    }
}