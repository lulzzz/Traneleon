using System.Diagnostics;
using System.IO;

namespace Acklann.WebFlow.Compilation
{
    public class Win32 : ShellBase
    {
        static Win32()
        {
            ShellBase.LoadModules();
            _executablesDirectory = Path.Combine(ResourceDirectory, "windows");
            using (var proc = new Process())
            {
                proc.StartInfo = new ProcessStartInfo("cmd")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                TryRun(proc, "node", "--version", @"nodejs\node.exe", out _nodePath, out _nodeIsAvailable);
                TryRun(proc, "java", "-version", @"jre\bin\java.exe", out _javaPath, out _javaIsAvailable);
                TryRun(proc, "magick", "-version", @"imageMagick\magick.exe", out _imageMagickPath, out _imageMagickIsAvailable);
            }
        }

        public override void InvokeNode(string args)
        {
            Invoke("node", _nodePath, args);
        }

        public override void InvokeJava(string args)
        {
            Invoke("java", _javaPath, args);
        }

        public override void InvokeImageMagick(string args)
        {
            Invoke("magick", _imageMagickPath, args);
        }

        public override void Invoke(string filename, string args)
        {
            StartInfo.Arguments = args;
            StartInfo.FileName = Path.Combine(_executablesDirectory, filename);
        }

        public override bool CanInvokeJava()
        {
            return _javaIsAvailable;
        }

        public override bool CanInvokeNode()
        {
            return _nodeIsAvailable;
        }

        public override bool CanInvokeImageMagick()
        {
            return _imageMagickIsAvailable;
        }

        private static void TryRun(Process process, string exe, string args, string localPath, out string path, out bool isAvailable)
        {
            localPath = Path.Combine(_executablesDirectory, localPath);
            if (File.Exists(localPath))
            {
                path = localPath;
                isAvailable = true;
            }
            else
            {
                try
                {
                    process.StartInfo.Arguments = $"/c {exe} {args}";
                    process.Start();
                    process.WaitForExit(3000);
                    isAvailable = process.ExitCode == 0;
                    path = string.Empty;
                }
                catch
                {
                    isAvailable = false;
                    path = string.Empty;
                }
            }
        }

        private void Invoke(string variable, string path, string args)
        {
            if (!string.IsNullOrEmpty(path))
            {
                StartInfo.FileName = path;
                StartInfo.Arguments = args;
            }
            else
            {
                StartInfo.FileName = "cmd";
                StartInfo.Arguments = $"/c {variable} {args}";
            }
        }

        #region Private Members

        private static readonly string _executablesDirectory = Path.Combine(ResourceDirectory, "win10");

        private static bool _nodeIsAvailable, _javaIsAvailable, _imageMagickIsAvailable;
        private static string _nodePath, _javaPath, _imageMagickPath;

        #endregion Private Members
    }
}