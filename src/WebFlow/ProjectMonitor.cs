using Acklann.WebFlow.Configuration;
using Akka.Actor;
using System;
using System.IO;
using System.Linq;

namespace Acklann.WebFlow
{
    public class ProjectMonitor : IDisposable
    {
        public ProjectMonitor() : this(new Compilation.FileProcessorObserver(), CreateActorSystem(), CreateWatch())
        {
        }

        public ProjectMonitor(IObserver<Compilation.ICompilierResult> observer) : this(observer, CreateActorSystem(), CreateWatch())
        {
        }

        public ProjectMonitor(IObserver<Compilation.ICompilierResult> observer, ActorSystem actorSystem) : this(observer, actorSystem, CreateWatch())
        {
        }

        public ProjectMonitor(IObserver<Compilation.ICompilierResult> observer, ActorSystem actorSystem, FileSystemWatcher fileSystemWatcher)
        {
            _watcher = fileSystemWatcher;
            _watcher.Created += OnFileWasModified;
            _watcher.Changed += OnFileWasModified;
            _watcher.Renamed += OnFileWasModified;

            _akka = actorSystem;
            _processor = _akka.ActorOf(Props.Create(typeof(FileProcessor), observer).WithRouter(new Akka.Routing.RoundRobinPool(Environment.ProcessorCount)));
        }

        public string DirectoryName
        {
            get { return _project?.DirectoryName; }
        }

        public void Start(string filePath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException($"Could not find file at '{filePath}'.");
            Start(Project.Load(filePath));
        }

        public void Start(Project project)
        {
            _project = project ?? throw new ArgumentNullException(nameof(project));
            _watcher.Path = project.DirectoryName;
            _watcher.EnableRaisingEvents = true;
        }

        public void Resume()
        {
            _watcher.EnableRaisingEvents = true;
        }

        public void Pause()
        {
            _watcher.EnableRaisingEvents = false;
        }

        public void Compile(Project project)
        {
            _project = project ?? throw new ArgumentException(nameof(project));

            foreach (IItemGroup itemGroup in project.GetItempGroups())
                if (itemGroup.Enabled)
                    foreach (string file in itemGroup.EnumerateFiles())
                    {
                        _processor.Tell(itemGroup.CreateCompilerOptions(file));
                    }
        }

        public void Compile(string filePath)
        {
            if (filePath.Equals(_project?.FullName))
            {
                if (Project.TryLoad(filePath, out Project project, out string error))
                {
                    _project = project;
                }
            }
            else if (filePath.NotDependency())
            {
                IItemGroup[] itemGroups = _project?.GetItempGroups()?.ToArray();

                if (itemGroups != null)
                    foreach (IItemGroup itemGroup in itemGroups)
                        if (itemGroup.Enabled && itemGroup.CanAccept(filePath))
                        {
                            _processor.Tell(itemGroup.CreateCompilerOptions(filePath));
                        }
            }
        }

        public void Compile() => Compile(_project);

        protected virtual void OnFileWasModified(object sender, FileSystemEventArgs e) => Compile(e.FullPath);

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _watcher?.Dispose();
                _akka?.Dispose();
            }
        }

        #endregion IDisposable

        #region Private Members

        private readonly FileSystemWatcher _watcher;
        private readonly IActorRef _processor;
        private readonly ActorSystem _akka;

        private Project _project;

        private static FileSystemWatcher CreateWatch()
        {
            return new FileSystemWatcher
            {
                Filter = "*",
                IncludeSubdirectories = true
            };
        }

        private static ActorSystem CreateActorSystem() => ActorSystem.Create(nameof(WebFlow), Akka.Configuration.ConfigurationFactory.ParseString("akka { loglevel = WARNING }"));

        #endregion Private Members
    }
}