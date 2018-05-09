using System;
using System.IO;

namespace Acklann.WebFlow.Compilation
{
    [AttributeUsage((AttributeTargets.Class | AttributeTargets.Struct), AllowMultiple = false, Inherited = true)]
    public sealed class CapabilityAttribute : Attribute
    {
        public CapabilityAttribute(Kind kind, params string[] fileTypes)
        {
            Kind = kind;
            SupportedFileTypes = fileTypes;
        }

        public Kind Kind { get; }

        public int Rank { get; set; }

        public string[] SupportedFileTypes { get; }

        public bool Supports(string filePath)
        {
            string ext = Path.GetExtension(filePath);
            foreach (string extension in SupportedFileTypes)
                if (string.Equals(ext, extension, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

            return false;
        }
    }
}