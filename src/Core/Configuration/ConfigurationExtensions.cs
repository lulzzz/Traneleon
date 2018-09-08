using Acklann.GlobN;
using Acklann.Traneleon.Compilation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Acklann.Traneleon.Configuration
{
    public static class ConfigurationExtensions
    {
        public static ICompilierResult[] Compile(this Project project, IProgress<ICompilierResult> progress = default)
            => CompileAsync(project, progress).Result;

        public static Task<ICompilierResult[]> CompileAsync(this Project project, IProgress<ICompilierResult> progress = default, System.Threading.CancellationToken cancellationToken = default)
        {
            var factory = new CompilerFactory();
            var tasks = new Stack<Task<ICompilierResult>>();

            foreach (string path in Directory.EnumerateFiles(project.DirectoryName, "*", SearchOption.AllDirectories))
                foreach (IItemGroup itemGroup in project.GetItempGroups())
                    if (itemGroup.Enabled && itemGroup.CanAccept(path))
                        foreach (ICompilierOptions options in itemGroup.CreateCompilerOptions(path))
                        {
                            ICompiler compiler = factory.CreateInstance(options);
                            tasks.Push(Task.Run(() =>
                            {
                                using (compiler)
                                {
                                    if (compiler.CanExecute(options))
                                    {
                                        ICompilierResult result = compiler.Execute(options);
                                        progress?.Report(result);
                                        return result;
                                    }

                                    return new EmptyResult();
                                }
                            }, cancellationToken));
                        }

            return Task.WhenAll(tasks);
        }

        internal static bool NotDependency(this string path)
        {
            foreach (Glob pattern in ItemGroupBase.GeneratedFolders.Select((x) => $"{x}/"))
                if (pattern.IsMatch(path))
                {
                    return false;
                }
            return true;
        }
    }
}