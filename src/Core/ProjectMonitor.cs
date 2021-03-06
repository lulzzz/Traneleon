﻿using Acklann.Traneleon.Compilation;
using Acklann.Traneleon.Configuration;
using Akka.Actor;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Acklann.Traneleon
{
    public class ProjectMonitor : IDisposable
    {
        public ProjectMonitor(ValidationEventHandler eventHandler = default, Action<ICompilierOptions, string> beforeCompilation = default, Action<ProgressToken, string> afterCompilation = default) : this(eventHandler, beforeCompilation, afterCompilation, CreateWatcher())
        {
        }

        public ProjectMonitor(ValidationEventHandler validationEventHandler, Action<ICompilierOptions, string> beforeCompilationHandler, Action<ProgressToken, string> afterCompiliationHandler, FileSystemWatcher fileSystemWatcher)
        {
            _watcher = fileSystemWatcher ?? throw new ArgumentNullException(nameof(fileSystemWatcher));

            _watcher.Created += OnFileWasModified;
            _watcher.Changed += OnFileWasModified;
            _watcher.Renamed += OnFileWasModified;

            _validationHandler = validationEventHandler;
            _preCompilationHandler = beforeCompilationHandler;
            _postCompilationHandler = afterCompiliationHandler;

            _factory = new CompilerFactory();
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

        public void Compile(DirectoryInfo folder)
        {
            IItemGroup[] itemGroups = _project?.GetItempGroups()?.ToArray();

            if (itemGroups != null)
                foreach (IItemGroup itemGroup in itemGroups.Where(x => x.Enabled))
                {
                    foreach (FileInfo file in folder.EnumerateFiles("*", SearchOption.AllDirectories))
                    {
                        if (itemGroup.CanAccept(file.FullName))
                            foreach (ICompilierOptions options in itemGroup.CreateCompilerOptions(file.FullName))
                            {
                                Process(options);
                            }
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
                    _totalActiveOperations++;
                    _preCompilationHandler?.Invoke(options, _project?.DirectoryName);
                    Task.Run(() =>
                    {
                        using (fileOperator)
                        {
                            ICompilierResult result = fileOperator.Execute(options);
                            _postCompilationHandler?.Invoke(new ProgressToken(result), _project?.DirectoryName);
                            _totalActiveOperations--;
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
                _watcher?.Dispose();
            }
        }

        #endregion IDisposable

        #region Private Members

        private readonly ICompilerFactory _factory;
        private readonly FileSystemWatcher _watcher;
        private readonly ValidationEventHandler _validationHandler;
        private readonly Action<ProgressToken, string> _postCompilationHandler;
        private readonly Action<ICompilierOptions, string> _preCompilationHandler;

        private Project _project;
        private volatile int _totalActiveOperations = 0;

        private static FileSystemWatcher CreateWatcher()
        {
            return new FileSystemWatcher
            {
                Filter = "*",
                IncludeSubdirectories = true
            };
        }

        private static ActorSystem CreateActorSystem() => ActorSystem.Create(nameof(Traneleon), Akka.Configuration.ConfigurationFactory.ParseString("akka { loglevel = WARNING }"));

        #endregion Private Members
    }
}