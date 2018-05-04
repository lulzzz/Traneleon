using Acklann.WebFlow.Transpilation;
using Acklann.WebFlow.Transpilation.Configuration;
using Akka.Actor;
using Akka.Event;
using System;
using System.Collections;

namespace Acklann.WebFlow
{
    public class Compiler : ReceiveActor
    {
        public Compiler() : this(null)
        {
        }

        public Compiler(IFileOperatorSelector selector)
        {
            _factory = selector;
            _operatoers = new Hashtable();
            _logger = Context.GetLogger();

            Receive<SettingsBase>((x) => HandleMessage(x));
        }

        protected void HandleMessage(SettingsBase options)
        {
            foreach (Type type in _factory.GetOperatorTypesThatSupports(options))
            {
                IFileOperator fileOperator = (_operatoers.Contains(type.Name) ? ((IFileOperator)_operatoers[type.Name]) : _factory.CreateInstance(type));

                if (fileOperator.CanExecute(options))
                {
                    _operatoers.Add(type.Name, fileOperator);
                    ResultFile result = fileOperator.Execute(options);
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
        private readonly IFileOperatorSelector _factory;

        #endregion Private Members
    }
}