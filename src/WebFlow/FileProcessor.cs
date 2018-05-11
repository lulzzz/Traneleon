using Acklann.WebFlow.Compilation;
using Akka.Actor;
using Akka.Event;
using System;
using System.Collections;

namespace Acklann.WebFlow
{
    public class FileProcessor : ReceiveActor
    {
        public FileProcessor() : this(null)
        {
        }

        public FileProcessor(ICompilerFactory factory)
        {
            _factory = factory;
            _compilers = new Hashtable();
            _logger = Context.GetLogger();

            Receive<ICompilierOptions>((x) => HandleMessage(x));
        }

        protected void HandleMessage(ICompilierOptions options)
        {
            foreach (Type type in _factory.GetCompilerTypesThatSupports(options))
            {
                ICompiler fileOperator = (_compilers.Contains(type.Name) ? ((ICompiler)_compilers[type.Name]) : _factory.CreateInstance(type));

                if (fileOperator.CanExecute(options))
                {
                    if (!_compilers.Contains(type.Name)) _compilers.Add(type.Name, fileOperator);
                    Sender.Tell(fileOperator.Execute(options));
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
        private readonly ILoggingAdapter _logger;
        private readonly ICompilerFactory _factory;

        #endregion Private Members
    }
}