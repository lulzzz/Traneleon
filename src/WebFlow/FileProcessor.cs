using Acklann.WebFlow.Compilation;
using Akka.Actor;
using Akka.Event;
using System;
using System.Collections;

namespace Acklann.WebFlow
{
    public class FileProcessor : ReceiveActor
    {
        public FileProcessor() : this(null, new CompilerFactory())
        {
        }

        public FileProcessor(IObserver<ICompilierResult> observer) : this(observer, new CompilerFactory())
        {
        }

        public FileProcessor(IObserver<ICompilierResult> observer, ICompilerFactory factory)
        {
            _factory = factory;
            _observer = observer;
            _compilers = new Hashtable();
            _logger = Context.GetLogger();

            Receive<ICompilierOptions>(HandleMessage, ((x) => _factory != null));
        }

        public static Props GetProps(IObserver<ICompilierResult> observer)
        {
            return Props.Create(typeof(FileProcessor), observer).WithRouter(new Akka.Routing.RoundRobinPool(Environment.ProcessorCount));
        }

        protected void HandleMessage(ICompilierOptions options)
        {
            foreach (Type type in _factory.GetCompilerTypesThatSupports(options))
            {
                ICompiler fileOperator = (_compilers.Contains(type.Name) ? ((ICompiler)_compilers[type.Name]) : _factory.CreateInstance(type));

                if (fileOperator.CanExecute(options))
                {
                    if (!_compilers.Contains(type.Name)) _compilers.Add(type.Name, fileOperator);
                    try
                    {
                        ICompilierResult result = fileOperator.Execute(options);
                        _observer?.OnNext(result);
                    }
                    catch (Exception ex) { _observer?.OnError(ex); }
                    break;
                }
            }
        }

        protected override void PostStop()
        {
            _compilers.Clear();
            _observer?.OnCompleted();
        }

        #region Private Members

        private readonly IDictionary _compilers;
        private readonly ILoggingAdapter _logger;
        private readonly ICompilerFactory _factory;
        private readonly IObserver<ICompilierResult> _observer;

        #endregion Private Members
    }
}