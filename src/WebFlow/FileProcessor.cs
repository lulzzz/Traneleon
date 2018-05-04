using Acklann.WebFlow.Compilation;
using Acklann.WebFlow.Compilation.Configuration;
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

        public FileProcessor(ICompilerFactory selector)
        {
            _factory = selector;
            _operatoers = new Hashtable();
            _logger = Context.GetLogger();

            Receive<ICompilierOptions>((x) => HandleMessage(x));
        }

        protected void HandleMessage(ICompilierOptions options)
        {
            foreach (Type type in _factory.GetOperatorTypesThatSupports(options))
            {
                ICompiler fileOperator = (_operatoers.Contains(type.Name) ? ((ICompiler)_operatoers[type.Name]) : _factory.CreateInstance(type));

                if (fileOperator.CanExecute(options))
                {
                    _operatoers.Add(type.Name, fileOperator);
                    ICompilierResult result = fileOperator.Execute(options);
                    Sender.Tell(result);
                    break;
                }
            }
        }

        protected override void PostStop()
        {
            _operatoers.Clear();
        }

        #region Private Members

        private readonly IDictionary _operatoers;
        private readonly ILoggingAdapter _logger;
        private readonly ICompilerFactory _factory;

        #endregion Private Members
    }
}