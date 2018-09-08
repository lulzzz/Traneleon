using System;
using System.IO;
using System.Reflection;

namespace Acklann.Traneleon.Compilation
{
    [Capability(Kind.Transform, ".jpg", ".jpeg", ".png")]
    public class ImageScaler : CompilerBase<ResizeOptions>
    {
        protected override bool CanExecute(ResizeOptions options)
        {
            return Shell.CanInvokeImageMagick() && _capabilities.Supports(options.SourceFile);
        }

        protected override void SetArguments(ResizeOptions options)
        {
            string outDir = Path.GetDirectoryName(options.OutputFile);
            if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

            Shell.InvokeImageMagick($"convert \"{options.SourceFile}\" -resize \"{options.NewSize}\" \"{options.OutputFile}\" ");
        }

        protected override ICompilierResult GetResult(ResizeOptions options)
        {
            return new CompilerResult(Kind.Transform, options.SourceFile, options.OutputFile, Shell);
        }

        #region Private Members

        private static readonly CapabilityAttribute _capabilities = typeof(ImageScaler).GetCustomAttribute<CapabilityAttribute>();

        #endregion Private Members
    }
}