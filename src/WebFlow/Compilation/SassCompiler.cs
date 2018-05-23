using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Acklann.WebFlow.Compilation
{
    [Capability(Kind.Transpile, ".scss")]
    public class SassCompiler : CompilerBase<TranspilierSettings>
    {
        protected override bool CanExecute(TranspilierSettings options)
        {
            if (Shell.CanInvokeNode())
            {
                foreach (string extension in _supportedFileTypes)
                    if (options.SourceFile.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
            }

            return false;
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

        #region Private Members

        private readonly string[] _supportedFileTypes = typeof(SassCompiler).GetCustomAttribute<CapabilityAttribute>()?.SupportedFileTypes;

        #endregion Private Members
    }
}