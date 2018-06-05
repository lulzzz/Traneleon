using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace Acklann.WebFlow.Compilation
{
    internal static class CompilationExtensions
    {
        public static CompilerError[] GetErrors(this StreamReader reader)
        {
            string json;
            JObject error;
            var errorList = new Queue<CompilerError>();

            while (!reader.EndOfStream)
            {
                json = reader.ReadLine();
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        error = JObject.Parse(json);
                        error.TryGetValue("line", out JToken line);
                        error.TryGetValue("code", out JToken code);
                        error.TryGetValue("file", out JToken path);
                        error.TryGetValue("column", out JToken column);
                        error.TryGetValue("message", out JToken message);

                        errorList.Enqueue(new CompilerError(
                            (message?.Value<string>() ?? string.Empty),
                            (path?.Value<string>() ?? string.Empty),
                            (line?.Value<int>() ?? 0),
                            (column?.Value<int>() ?? 0),
                            (code?.Value<int>() ?? 0)
                            ));
                    }
                    catch (JsonReaderException)
                    {
                        System.Diagnostics.Debug.WriteLine(json);
                    }
                }
            }

            return errorList.ToArray();
        }

        public static IDictionary<string, string> GetGeneratedFiles(this StreamReader reader)
        {
            string json;
            JObject obj;
            var files = new Dictionary<string, string>();

            while (!reader.EndOfStream)
            {
                json = reader.ReadLine();
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        obj = JObject.Parse(json);
                        foreach (var name in new string[] { "minifiedFile", "intermediateFile", "sourceMapFile", "sourceMapFile2" })
                            if (obj.TryGetValue(name, out JToken token))
                            {
                                files.Add(name, token.Value<string>());
                                break;
                            }
                    }
                    catch (JsonReaderException)
                    {
                        System.Diagnostics.Debug.WriteLine(json);
                    }
                }
            }

            return files;
        }
    }
}