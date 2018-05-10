using Acklann.WebFlow.Compilation.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace Acklann.WebFlow.Compilation
{
    public class SassCompiler : CompilerBase<TranspilierSettings>
    {
        protected override bool CanExecute(TranspilierSettings options)
        {
            return Shell.CanInvokeNode() && options.SourceFile.EndsWith(".scss", StringComparison.OrdinalIgnoreCase);
        }

        protected override void SetArguments(TranspilierSettings options)
        {
            // DOCUMENTATION: https://github.com/sass/node-sass

            if (!Directory.Exists(options.OutputDirectory)) Directory.CreateDirectory(options.OutputDirectory);
            if (options.GenerateSourceMaps && !Directory.Exists(options.SourceMapDirectory))
            {
                Directory.CreateDirectory(options.SourceMapDirectory);
            }

            string script = Path.Combine(ShellBase.ResourceDirectory, "sass.js");
            Shell.InvokeNode($"\"{script}\" {options.ToArgs()}");
        }

        protected override ICompilierResult GetResult(TranspilierSettings options)
        {
            long executionTime = (Shell.ExitTime.Ticks - Shell.StartTime.Ticks);
            IDictionary<string, string> files = Shell.StandardOutput.GetGeneratedFiles();
            files.TryGetValue("minifiedFile", out string compiliedFile);

            return new TranspilierResult(Shell.ExitCode, executionTime, Shell.StandardError.GetErrors(), compiliedFile);
        }
    }
}