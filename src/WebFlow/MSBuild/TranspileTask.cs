using Acklann.WebFlow.Compilation;
using Acklann.WebFlow.Configuration;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;
using System.Xml.Schema;

namespace Acklann.WebFlow.MSBuild
{
    public class TranspileTask : Task
    {
        public string ConfigFile { get; set; }

        public override bool Execute()
        {
            try
            {
                if (File.Exists(ConfigFile))
                {
                    using (Stream file = File.OpenRead(ConfigFile))
                    {
                        bool fileIsValid = true;
                        bool isaXmlFile = !Path.GetExtension(ConfigFile).Equals(".json", StringComparison.OrdinalIgnoreCase);

                        if (isaXmlFile)
                        {
                            Project.Validate(file, delegate (object sender, ValidationEventArgs e)
                            {
                                if (e.Severity == XmlSeverityType.Error)
                                {
                                    fileIsValid = false;
                                    Log.LogErrorFromException(e.Exception);
                                }
                            });
                        }

                        if (fileIsValid)
                        {
                            var project = Project.Load(file);
                            Log.LogMessage(MessageImportance.Low, $"{nameof(WebFlow)}: Compiling web assets.");
                            ICompilierResult[] results = project.CompileAsync().Result;
                            Log.LogMessage($"{nameof(WebFlow)} -> Compiled {results.Length} files.");
                        }
                        else return false;
                    }
                }
                else Log.LogMessage($"Could not find file at '{ConfigFile}'.");

                return true;
            }
            catch (Exception)
            {
            }

            return false;
        }
    }
}