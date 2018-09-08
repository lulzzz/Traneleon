using Acklann.NShellit;
using Acklann.NShellit.Extensions;
using Acklann.Traneleon.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Acklann.Traneleon
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            IEnumerable<Type> commandList = (from t in typeof(ICommand).Assembly.ExportedTypes
                                             where !t.IsInterface && !t.IsAbstract && typeof(ICommand).IsAssignableFrom(t)
                                             select t);

            int exitCode = (new Parser().Map<ICommand, int>(args, commandList,
                (cmd) => cmd.Execute(),
                (err) => 1));

            return exitCode;
        }
    }
}