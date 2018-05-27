using Acklann.GlobN;
using Acklann.NShellit.Attributes;
using Acklann.Watcha;
using Acklann.WebFlow.Compilation;
using Acklann.WebFlow.Configuration;
using Akka.Actor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Acklann.WebFlow.Commands
{
    [Command("build", Cmdlet = "Invoke-WebFlow")]
    [Summary("Transpile, minify and bundle all project files.")]
    public sealed class CompileCommand : ICommand
    {
        [UseConstructor]
        public CompileCommand(string configFile, bool enableWatcher)
        {
            EnableWatcher = _continueWatching = enableWatcher;
            ConfigFile = configFile.ExpandPath(Environment.CurrentDirectory, true);

            _observer = new FileProcessorObserver();
        }

        [Required, Parameter('c', "config", Kind = "path")]
        [Summary("The absolute or relative path of the config file.")]
        public string ConfigFile { get; }

        [Parameter('w', "watch")]
        [Summary("Determines whether to monitor the working directory for changes.")]
        public bool EnableWatcher { get; }

        public int Execute()
        {
            if (!File.Exists(ConfigFile)) throw new FileNotFoundException($"Could not find file at '{ConfigFile}'.");
            else if (Project.TryLoad(ConfigFile, out _project, out string error))
            {
                Log.WorkingDirectory = _project.DirectoryName;
                var config = Akka.Configuration.ConfigurationFactory.ParseString(@"
                akka {
                    loglevel = WARNING
                }
");
                using (_akka = ActorSystem.Create(nameof(WebFlow), config))
                {
                    _processor = _akka.ActorOf(Props.Create(typeof(FileProcessor), _observer).WithRouter(new Akka.Routing.RoundRobinPool(Environment.ProcessorCount)));

                    if (EnableWatcher) StartWatcher();
                    else Compile();

                    return 0;
                }
            }
            else throw new FormatException(error);
        }

        public void StartWatcher()
        {
            using (_watcher = new EnhancedFileSystemWatcher(_project.DirectoryName))
            {
                _watcher.EnableRaisingEvents = true;
                _watcher.IncludeSubdirectories = true;
                _watcher.NotifyFilter = (NotifyFilters.LastWrite | NotifyFilters.FileName);
                _watcher.Renamed += OnFileChanged;
                _watcher.Created += OnFileChanged;
                _watcher.ChangeComplete += OnFileChanged;
                Log.Debug($"monitoring '{_project.DirectoryName}' for changes.");

                do
                {
                    Log.Debug("\r\npress the [Enter] key to exit ...");
                } while (shouldNotTerminate());
                Log.Debug("watcher terminated.");
            }

            bool shouldNotTerminate()
            {
                if (Environment.UserInteractive) return (_continueWatching && Console.ReadKey().Key != ConsoleKey.Enter);
                else return _continueWatching;
            }
        }

        public void StopWatcher()
        {
            _continueWatching = false;
        }

        private void Compile()
        {
            int max = 0;
            IEnumerable<IItemGroup> itemGroups = _project?.GetItempGroups()?.ToArray();

            if (itemGroups != null)
                foreach (IItemGroup itemGroup in itemGroups)
                    if (itemGroup.Enabled)
                        foreach (string file in itemGroup.EnumerateFiles())
                        {
                            max++;
                            _processor.Tell(itemGroup.CreateCompilerOptions(file));
                        }
            _observer.WaitForCompletion(max, 30_000);
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            //Console.WriteLine($"{e.ChangeType}: {e.Name}");

            if (e.FullPath.NotDependency())
            {
                if (e.FullPath.Equals(_project.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    Log.Debug($" reloading: '{_project.FullName.GetRelativePath(_project.DirectoryName)}'.");
                    _project = Project.Load(e.FullPath);
                }
                else
                {
                    IEnumerable<IItemGroup> itemGroups = _project?.GetItempGroups()?.ToArray();

                    if (itemGroups != null)
                        foreach (IItemGroup itemGroup in itemGroups)
                            if (itemGroup.Enabled && itemGroup.CanAccept(e.FullPath))
                            {
                                Log.Debug($" processing: '{e.FullPath.GetRelativePath(_project.DirectoryName)}'.");
                                _processor.Tell(itemGroup.CreateCompilerOptions(e.FullPath));
                            }
                }
            }
        }

        #region Private Members

        private readonly IActorObserver _observer;

        private Project _project;
        private IActorRef _processor;
        private volatile bool _continueWatching;

        private ActorSystem _akka;
        private EnhancedFileSystemWatcher _watcher;

        #endregion Private Members
    }
}