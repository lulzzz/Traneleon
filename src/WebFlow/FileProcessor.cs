using Acklann.WebFlow.Compilation;
using Akka.Actor;
using System;
using System.Collections;

namespace Acklann.WebFlow
{
    public class FileProcessor : ReceiveActor
    {
        public FileProcessor() : this(null, new CompilerFactory())
        {
        }

        public FileProcessor(IProgress<ProgressToken> reporter) : this(reporter, new CompilerFactory())
        {
        }

        public FileProcessor(IProgress<ProgressToken> reporter, ICompilerFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _reporter = reporter;
            _compilers = new Hashtable();

            Receive<ICompilierOptions>(HandleMessage, ((x) => !string.IsNullOrEmpty(x.SourceFile)));
        }

        public static Props GetProps(IProgress<ProgressToken> reporter)
        {
            return Props.Create(typeof(FileProcessor), reporter).WithRouter(new Akka.Routing.RoundRobinPool(Environment.ProcessorCount));
        }

        protected void HandleMessage(ICompilierOptions options)
        {
            foreach (Type type in _factory.GetCompilerTypesThatSupports(options))
            {
                ICompiler fileOperator = (_compilers.Contains(type.Name) ? ((ICompiler)_compilers[type.Name]) : _factory.CreateInstance(type));

                if (fileOperator.CanExecute(options))
                {
                    if (!_compilers.Contains(type.Name))
                    {
                        _compilers.Add(type.Name, fileOperator);
                    }

                    ICompilierResult result = fileOperator.Execute(options);
                    _reporter?.Report(new ProgressToken(result));
                    break;
                }
            }
        }

        protected override void PostStop()
        {
            _compilers.Clear();
        }

        #region Private Members

        private readonly IDictionary _compilers;
        private readonly ICompilerFactory _factory;
        private readonly IProgress<ProgressToken> _reporter;

        #endregion Private Members
    }
}