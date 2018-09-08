using Acklann.Traneleon.Configuration;
using System;
using System.IO;
using System.Reflection;

namespace Acklann.Traneleon.Compilation
{
    [Capability(Kind.Optimize, ".jpg", ".jpeg", ".png")]
    public class ImageOptimizer : CompilerBase<ImageOptimizerOptions>
    {
        protected override bool CanExecute(ImageOptimizerOptions options)
        {
            if (_supportedFileTypes != null)
                foreach (string extension in _supportedFileTypes)
                    if (options.FileType.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

            return false;
        }

        protected override void SetArguments(ImageOptimizerOptions options)
        {
            string outDir = Path.GetDirectoryName(options.OutputFile);
            if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

            switch (Shell.OS)
            {
                default:
                case PlatformID.Win32NT:
                    switch (options.FileType)
                    {
                        case ".jpg":
                        case ".jpeg":
                            UsePingo(options);
                            break;

                        case ".gif":
                            UseGifsicle(options);
                            break;

                        case ".png":
                            if (options.CompressionKind == CompressionKind.LossLess)
                                UsePingo(options);
                            else
                                UsePngquant(options);
                            break;

                        default:
                            throw new NotSupportedException($"{options.FileType} files are not supported.");
                    }
                    break;
            }
        }

        protected override ICompilierResult GetResult(ImageOptimizerOptions options)
        {
            return new CompilerResult(options.Kind, options.SourceFile, options.OutputFile, Shell);
        }

        #region Private Member

        private static readonly string[] _supportedFileTypes = (typeof(ImageOptimizer)).GetCustomAttribute<CapabilityAttribute>()?.SupportedFileTypes;

        private void UsePingo(ImageOptimizerOptions options)
        {
            // Manual: https://css-ig.net/pingo#use

            File.Copy(options.SourceFile, options.OutputFile, overwrite: true);
            string progressive = (options.Progressive ? "-progressive" : string.Empty);

            switch (options.CompressionKind)
            {
                case CompressionKind.LossLess:
                    Shell.Invoke("pingo.exe", $"\"{options.OutputFile}\" {progressive} -fast");
                    break;

                case CompressionKind.Lossy:
                    Shell.Invoke("pingo.exe", $"\"{options.OutputFile}\" -quality={options.Quality} {progressive} -strip=3");
                    break;
            }
        }

        private void UsePngquant(ImageOptimizerOptions options)
        {
            // Manual: https://pngquant.org/#manual

            Shell.Invoke("pngquant.exe", $"\"{options.SourceFile}\" --output \"{options.OutputFile}\" --strip --speed 1 --quality 0-{options.Quality} --force");
        }

        private void UseGifsicle(ImageOptimizerOptions options)
        {
            // Manual: https://www.lcdf.org/gifsicle/man.html

            Shell.Invoke("gifsicle.exe", $"-O3 --batch \"{options.SourceFile}\" --output=\"{options.OutputFile}\"");
        }

        #endregion Private Member
    }
}