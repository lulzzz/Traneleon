using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            Version = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
            ResourceDirectory = Path.Combine(Path.GetDirectoryName(assembly.Location), $"modules_{Version}");
            if (!Directory.Exists(ResourceDirectory)) Directory.CreateDirectory(ResourceDirectory);
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

        public static readonly string ResourceDirectory, Version;
        public static bool IsNotLoaded = true;

        public static ShellBase GetShell()
        {
            switch (Environment.OSVersion.Platform)
            {
                default:
                case PlatformID.Win32NT:
                    return new Win32();
            }
        }

        public static void LoadModules(bool force = false)
        {
#if DEBUG
            force = true;
#endif
            string lockFile = Path.Combine(ResourceDirectory, "webflow-lock.json");

            if (IsNotLoaded && (!File.Exists(lockFile) || force))
                try
                {
                    var resources = new Stack<string>();
                    Assembly assembly = typeof(ShellBase).Assembly;
                    foreach (string name in assembly.GetManifestResourceNames())
                    {
                        string extension = Path.GetExtension(name);
                        switch (extension.ToLowerInvariant())
                        {
                            case ".js":
                                string baseName = Path.GetFileNameWithoutExtension(name);
                                string filePath = Path.Combine(ResourceDirectory, $"{baseName.Substring(baseName.LastIndexOf('.') + 1)}{extension}");

                                if (!File.Exists(filePath) || force)
                                    using (Stream stream = assembly.GetManifestResourceStream(name))
                                    using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                                    {
                                        resources.Push(name);
                                        stream.CopyTo(file);
                                        stream.Flush();
                                    }
                                break;

                            case ".zip":
                                if (!File.Exists(lockFile))
                                    using (Stream stream = assembly.GetManifestResourceStream(name))
                                    using (var archive = new ZipArchive(stream))
                                    {
                                        resources.Push(name);
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

                    IsNotLoaded = false;
                    File.WriteAllText(lockFile, JsonConvert.SerializeObject(new { resources }));
                }
                catch (UnauthorizedAccessException) { Debug.WriteLine($"{nameof(ShellBase)}.{nameof(LoadModules)} was unable to access a file."); }

            bool isFile(string filePath) => !(filePath.EndsWith("\\") || filePath.EndsWith("/"));
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