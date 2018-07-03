using Acklann.GlobN;
using Acklann.WebFlow.Compilation;
using Akka.Actor;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Acklann.WebFlow.Configuration
{
    public static class ConfigurationExtensions
    {
        public static Task<ICompilierResult[]> CompileAsync(this Project project, IActorObserver observer = null, int millisecondsTimeout = 30_000)
        {
            return Task.Run(() =>
            {
                using (var akka = ActorSystem.Create(Guid.NewGuid().ToString("n"), Akka.Configuration.ConfigurationFactory.ParseString("akka { loglevel = WARNING }")))
                {
                    if (observer == null) observer = new FileProcessorObserver();
                    IActorRef processor = akka.ActorOf(FileProcessor.GetProps(observer));

                    int max = 0;
                    foreach (IItemGroup itemGroup in project.GetItempGroups())
                        if (itemGroup.Enabled)
                            foreach (string file in itemGroup.EnumerateFiles())
                            {
                                max++;
                                processor.Tell(itemGroup.CreateCompilerOptions(file));
                            }

                    observer?.WaitForCompletion(max, millisecondsTimeout);
                    return observer?.GetResults().ToArray();
                }
            });
        }

        public static ICompilierResult[] Compile(this Project project, IActorObserver observer = null, int millisecondsTimeout = 30_000)
            => CompileAsync(project, observer, millisecondsTimeout).Result;

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