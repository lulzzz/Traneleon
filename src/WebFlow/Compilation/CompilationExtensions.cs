using Acklann.WebFlow.Compilation.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace Acklann.WebFlow.Compilation
{
    public static class CompilationExtensions
    {
        internal static ICompilierResult GenerateResults<TOption>(this ShellBase shell, TOption options) where TOption : ICompilierOptions
        {
            string json;
            JObject error, file;
            var errorList = new Queue<Error>();
            StreamReader errorStream = shell.StandardError;

            while (!errorStream.EndOfStream)
            {
                json = errorStream.ReadLine();
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        error = JObject.Parse(json);
                        error.TryGetValue("line", out JToken line);
                        error.TryGetValue("file", out JToken path);
                        error.TryGetValue("message", out JToken message);

                        errorList.Enqueue(new Error(
                            (message.HasValues ? message.Value<string>() : string.Empty),
                            (path.HasValues ? path.Value<string>() : string.Empty),
                            (line.HasValues ? line.Value<int>() : 0)
                            ));
                    }
                    catch (JsonReaderException)
                    {
                        System.Diagnostics.Debug.WriteLine(json);
                    }
                }
            }

            file = new JObject();
            StreamReader outStream = shell.StandardOutput;
            while (!outStream.EndOfStream)
            {
                json = outStream.ReadLine();
                if (!string.IsNullOrEmpty(json))
                {
                    foreach (var name in new string[] { "minifiedFile", "intermediateFile", "sourceMapFile", "sourceMapFile2" })
                        try
                        {
                            if (JObject.Parse(json).TryGetValue(name, out JToken value))
                            {
                                file.Add(name, value);
                            }
                        }
                        catch (JsonReaderException)
                        {
                            System.Diagnostics.Debug.WriteLine(json);
                        }
                }
            }

            long executionTime = (shell.ExitTime.Ticks - shell.StartTime.Ticks);
            if (shell.ExitCode == 0)
                switch (options)
                {
                    case TranspilierSettings t:
                        return new TranspilierResult(file["minifiedFile"].Value<string>(), shell.ExitCode, executionTime, errorList.ToArray());
                }

            return new EmptyResult();
        }
    }
}