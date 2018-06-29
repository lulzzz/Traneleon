#define FORCE

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace Acklann.WebFlow.Compilation
{
    public abstract class ShellBase : Process
    {
        static ShellBase()
        {
            Assembly assembly = typeof(ShellBase).Assembly;
            string version = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
            ResourceDirectory = Path.Combine(Path.GetDirectoryName(assembly.Location), $"modules_{version}");
            bool force = false;

#if DEBUG && FORCE
            force = true;
#endif
            foreach (string name in assembly.GetManifestResourceNames())
            {
                string extension = Path.GetExtension(name);
                switch (extension.ToLowerInvariant())
                {
                    //case ".json":
                    //    RestoreNodePackages(assembly.GetManifestResourceStream(name));
                    //    break;

                    case ".js":
                        string baseName = Path.GetFileNameWithoutExtension(name);
                        string filePath = Path.Combine(ResourceDirectory, $"{baseName.Substring(baseName.LastIndexOf('.') + 1)}{extension}");

                        if (!File.Exists(filePath) || force)
                            using (Stream stream = assembly.GetManifestResourceStream(name))
                            using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                            {
                                stream.CopyTo(file);
                                stream.Flush();
                            }
                        break;

                    case ".zip":
                        if (!Directory.Exists(Path.Combine(ResourceDirectory, "node_modules")))
                            using (Stream stream = assembly.GetManifestResourceStream(name))
                            using (var archive = new ZipArchive(stream))
                            {
                                string destination, dir;
                                foreach (ZipArchiveEntry entry in archive.Entries)
                                {
                                    if (isFile(entry.FullName))
                                    {
                                        destination = Path.Combine(ResourceDirectory, entry.FullName);
                                        dir = Path.GetDirectoryName(destination);
                                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                                        entry.ExtractToFile(destination, overwrite: true);
                                    }
                                }
                            }
                        break;
                }
            }

            bool isFile(string filePath) => !(filePath.EndsWith("\\") || filePath.EndsWith("/"));
        }

        public ShellBase() : base()
        {
            StartInfo = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
        }

        public static readonly string ResourceDirectory;

        public static void RestoreNodePackages(Stream stream)
        {
            if (!File.Exists(Path.Combine(ResourceDirectory, "node_modules")))
            {
                using (stream)
                using (var file = new FileStream(Path.Combine(ResourceDirectory, "package.json"), FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(file);
                    stream.Flush();
                }

                using (var node = new Process())
                {
                    node.StartInfo = new ProcessStartInfo()
                    {
                        Arguments = "npm install csso",
                        WorkingDirectory = ResourceDirectory,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    switch (Environment.OSVersion.Platform)
                    {
                        case PlatformID.Win32NT:
                            node.StartInfo.FileName = "cmd";
                            break;
                    }

                    node.Start();
                    node.WaitForExit(600_000);
                }
            }
        }

        public static ShellBase GetShell()
        {
            switch (Environment.OSVersion.Platform)
            {
                default:
                case PlatformID.Win32NT:
                    return new Win32();
            }
        }

        public abstract void InvokeJava(string args);

        public abstract void InvokeNode(string args);

        public abstract void InvokeImageMagick(string args);

        public abstract void Invoke(string filename, string args);

        public abstract bool CanInvokeJava();

        public abstract bool CanInvokeNode();

        public abstract bool CanInvokeImageMagick();
    }
}