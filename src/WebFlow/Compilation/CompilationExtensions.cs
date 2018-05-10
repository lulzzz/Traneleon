using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace Acklann.WebFlow.Compilation
{
    public static class CompilationExtensions
    {
        internal static IDictionary<string, string> GetGeneratedFiles(this StreamReader reader)
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

        internal static Error[] GetErrors(this StreamReader reader)
        {
            string json;
            JObject error;
            var errorList = new Queue<Error>();

            while (!reader.EndOfStream)
            {
                json = reader.ReadLine();
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        error = JObject.Parse(json);
                        error.TryGetValue("line", out JToken line);
                        error.TryGetValue("file", out JToken path);
                        error.TryGetValue("message", out JToken message);

                        errorList.Enqueue(new Error(
                            ((message?.HasValues ?? false) ? message.Value<string>() : string.Empty),
                            ((path?.HasValues ?? false) ? path.Value<string>() : string.Empty),
                            ((line?.HasValues ?? false) ? line.Value<int>() : 0)
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
    }
}