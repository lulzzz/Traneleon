using Acklann.WebFlow.Compilation;
using Acklann.WebFlow.Configuration;
using Akka.Actor;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Acklann.WebFlow
{
    public class ProjectMonitor : IDisposable
    {
        public ProjectMonitor(ValidationEventHandler eventHandler = default, IProgress<ProgressToken> reporter = default, IObserver<ICompilierOptions> observer = default) : this(eventHandler, reporter, observer, CreateWatcher())
        {
        }

        public ProjectMonitor(ValidationEventHandler validationEventHandler, IProgress<ProgressToken> reporter, IObserver<ICompilierOptions> observer, FileSystemWatcher fileSystemWatcher)
        {
            _watcher = fileSystemWatcher ?? throw new ArgumentNullException(nameof(fileSystemWatcher));

            _watcher.Created += OnFileWasModified;
            _watcher.Changed += OnFileWasModified;
            _watcher.Renamed += OnFileWasModified;

            _reporter = reporter;
            _observer = observer;
            _factory = new CompilerFactory();
            _validationHandler = validationEventHandler;
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
                        foreach (ICompilierOptions options in itemGroup.CreateCompilerOptions(file))
                        {
                            Process(options);
                        }
        }

        public void Compile(string filePath)
        {
            System.Diagnostics.Debug.WriteLine($"modified: '{filePath}'");

            if (filePath.Equals(_project?.FullName))
            {
                if (Project.TryLoad(filePath, _validationHandler, out Project project))
                {
                    _project = project;
                }
                else System.Diagnostics.Debug.WriteLine($"'{filePath}' is not well-formed.");
            }
            else if (filePath.NotDependency())
            {
                IItemGroup[] itemGroups = _project?.GetItempGroups()?.ToArray();

                if (itemGroups != null)
                    foreach (IItemGroup itemGroup in itemGroups)
                        if (itemGroup.Enabled && itemGroup.CanAccept(filePath))
                            foreach (ICompilierOptions options in itemGroup.CreateCompilerOptions(filePath))
                            {
                                Process(options);
                            }
            }
        }

        public void Compile() => Compile(_project);

        internal void Process(ICompilierOptions options)
        {
            foreach (Type type in _factory.GetCompilerTypesThatSupports(options))
            {
                ICompiler fileOperator = _factory.CreateInstance(type);

                if (fileOperator.CanExecute(options))
                {
                    _observer?.OnNext(options);
                    Task.Run(() =>
                    {
                        using (fileOperator)
                        {
                            ICompilierResult result = fileOperator.Execute(options);
                            _reporter?.Report(new ProgressToken(result));
                        }
                    });
                    break;
                }
            }
        }

        protected virtual void OnFileWasModified(object sender, FileSystemEventArgs e)
        {
            if (Path.HasExtension(e.FullPath)) Compile(e.FullPath);
        }

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
                _observer?.OnCompleted();
                _watcher?.Dispose();
            }
        }

        #endregion IDisposable

        #region Private Members

        private readonly ICompilerFactory _factory;
        private readonly FileSystemWatcher _watcher;
        private readonly IProgress<ProgressToken> _reporter;
        private readonly IObserver<ICompilierOptions> _observer;
        private readonly ValidationEventHandler _validationHandler;

        private Project _project;

        private static FileSystemWatcher CreateWatcher()
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