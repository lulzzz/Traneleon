using System;
using System.IO;

namespace Acklann.WebFlow.Compilation
{
    [Capability(Kind.Transpile, ".ts")]
    public class TypescriptCompiler : CompilerBase<TranspilierSettings>
    {
        protected override bool CanExecute(TranspilierSettings options)
        {
            return Shell.CanInvokeNode() && options.FileType.EndsWith(".ts", StringComparison.OrdinalIgnoreCase);
        }

        protected override void SetArguments(TranspilierSettings options)
        {
            // DOCUMENTATION: https://github.com/Microsoft/TypeScript/wiki/Using-the-Compiler-API

            if (!Directory.Exists(options.OutputDirectory)) Directory.CreateDirectory(options.OutputDirectory);
            if (options.GenerateSourceMaps && !Directory.Exists(options.SourceMapDirectory))
            {
                Directory.CreateDirectory(options.SourceMapDirectory);
            }

            string script = Path.Combine(ShellBase.ResourceDirectory, "tsc.js");
            Shell.InvokeNode($"\"{script}\" {options.ToArgs()}");
        }

        protected override ICompilierResult GetResult(TranspilierSettings options)
        {
            long executionTime = (Shell.ExitTime.Ticks - Shell.StartTime.Ticks);
            Shell.StandardOutput.GetGeneratedFiles().TryGetValue("minifiedFile", out string compiliedFile);

            return new TranspilierResult(Shell.ExitCode, executionTime, Shell.StandardError.GetErrors(), compiliedFile, string.Join(";", options.SourceFiles));
        }
    }
}