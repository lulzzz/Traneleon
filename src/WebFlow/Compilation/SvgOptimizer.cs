using System;
using System.IO;

namespace Acklann.WebFlow.Compilation
{
    [Capability(Kind.Optimize, ".svg")]
    public class SvgOptimizer : CompilerBase<ImageOptimizerOptions>
    {
        protected override bool CanExecute(ImageOptimizerOptions options)
        {
            return Shell.CanInvokeNode() && ".svg".Equals(options.FileType, StringComparison.OrdinalIgnoreCase);
        }

        protected override ICompilierResult GetResult(ImageOptimizerOptions options)
        {
            return new CompilerResult(Kind.Optimize, options.SourceFile, options.OutputFile, Shell);
        }

        protected override void SetArguments(ImageOptimizerOptions options)
        {
            /// Documentation: https://github.com/svg/svgo

            string outDir = Path.GetDirectoryName(options.OutputFile);
            if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

            string svgo = Path.Combine(ShellBase.ResourceDirectory, "node_modules", "svgo", "bin", "svgo");
            Shell.InvokeNode($"\"{svgo}\" \"{options.SourceFile}\" -o \"{options.OutputFile}\"");
        }
    }
}