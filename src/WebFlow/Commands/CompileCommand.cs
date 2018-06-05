using Acklann.GlobN;
using Acklann.NShellit.Attributes;
using Acklann.WebFlow.Compilation;
using Acklann.WebFlow.Configuration;
using System;
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
                if (EnableWatcher) StartWatcher();
                else Compile();

                return 0;
            }
            else throw new FormatException(error);
        }

        public void StartWatcher()
        {
            using (_monitor = new ProjectMonitor(_observer))
            {
                _monitor.Start(_project);
                Log.Debug("monitoring project files for changes...");
                do
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine("\rpress the [Enter] key to exit...");
                } while (shouldNotTerminate());
            }
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Log.Info("exited watch mode.");
            Console.ResetColor();

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
            long startTime = DateTime.Now.Ticks;
            Log.Debug(format($"Build started", '-'));
            ICompilierResult[] results = _project.CompileAsync().Result;
            string elapse = TimeSpan.FromTicks(DateTime.Now.Ticks - startTime).ToString();
            Log.Debug(format($"Build: finished in ({elapse}); {results.Count(x => x.Succeeded)} succeeded, {results.Count(x => !x.Succeeded)} failed", '='));

            string format(string value, char c)
            {
                var line = string.Join("", Enumerable.Repeat(c, 80 - value.Length));
                line = line.Insert((line.Length / 2), $" {value} ");
                return line;
            }
        }

        #region Private Members

        private readonly IActorObserver _observer;

        private Project _project;
        private ProjectMonitor _monitor;
        private volatile bool _continueWatching;

        #endregion Private Members
    }
}