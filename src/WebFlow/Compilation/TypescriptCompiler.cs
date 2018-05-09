using Acklann.WebFlow.Compilation.Configuration;
using System;
using System.IO;

namespace Acklann.WebFlow.Compilation
{
    [Capability(Kind.Transpile, ".ts")]
    public class TypescriptCompiler : CompilerBase<TranspilierSettings>
    {
        protected override bool CanExecute(TranspilierSettings options)
        {
            return Shell.CanInvokeNode() && options.SourceFile.EndsWith(".ts", StringComparison.OrdinalIgnoreCase);
        }

        protected override void SetArguments(TranspilierSettings options)
        {
            // DOCUMENTATION: https://github.com/sass/node-sass

            if (!Directory.Exists(options.OutputDirectory)) Directory.CreateDirectory(options.OutputDirectory);
            if (options.GenerateSourceMaps && !Directory.Exists(options.SourceMapDirectory))
            {
                Directory.CreateDirectory(options.SourceMapDirectory);
            }

            string script = Path.Combine(ShellBase.ResourceDirectory, "tsc.js");
            Shell.InvokeNode($"\"{script}\" {options.ToArgs()}");
        }
    }
}