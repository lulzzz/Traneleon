using Acklann.GlobN;
using Acklann.WebFlow.Compilation;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Acklann.WebFlow.Configuration
{
    public static class ConfigurationExtensions
    {
        public static Task<ICompilierResult[]> CompileAsync(this Project project)
        {
            var factory = new CompilerFactory();
            var tasks = new ConcurrentStack<Task<ICompilierResult>>();
            var compiliers = new ConcurrentDictionary<string, ICompiler>();

            foreach (IItemGroup itemGroup in project.GetItempGroups())
                if (itemGroup.Enabled)
                    foreach (string file in itemGroup.EnumerateFiles())
                    {
                        ICompilierOptions options = itemGroup.CreateCompilerOptions(file);
                        tasks.Push(Task.Run(() => Compile(options, factory, compiliers)));
                    }

            return Task.WhenAll(tasks);
        }

        public static Task<ICompilierResult> CompileAsync(this Project project, string filePath)
        {
            var factory = new CompilerFactory();
            var compiliers = new ConcurrentDictionary<string, ICompiler>();

            foreach (IItemGroup itemGroup in project.GetItempGroups())
                if (itemGroup.Enabled)
                {
                    ICompilierOptions options = itemGroup.CreateCompilerOptions(filePath);
                    return Task.Run(() => Compile(options, factory, compiliers));
                }

            return Task.Run(() => (ICompilierResult)new EmptyResult());
        }

        internal static ICompilierResult Compile(ICompilierOptions options, ICompilerFactory factory, IDictionary compilers)
        {
            foreach (Type type in factory.GetCompilerTypesThatSupports(options))
            {
                ICompiler fileOperator = (compilers.Contains(type.Name) ? (ICompiler)compilers[type.Name] : factory.CreateInstance(type));

                if (fileOperator?.CanExecute(options) ?? false)
                {
                    if (!compilers.Contains(type.Name)) compilers.Add(type.Name, fileOperator);
                    return fileOperator.Execute(options);
                }
            }

            return new EmptyResult();
        }

        internal static bool NotDependency(this string path)
        {
            foreach (Glob pattern in new string[] { "node_modules/", "bower_components/", "packages/" })
                if (pattern.IsMatch(path))
                {
                    return false;
                }
            return true;
        }

        #region Private Members

        private static IDictionary _compilers;
        private static ICompilerFactory _factory;

        #endregion Private Members
    }
}