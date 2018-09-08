using System;
using System.IO;

namespace Acklann.Traneleon.Compilation
{
    [AttributeUsage((AttributeTargets.Class | AttributeTargets.Struct), AllowMultiple = false, Inherited = true)]
    public sealed class CapabilityAttribute : Attribute
    {
        public CapabilityAttribute(Kind kind, params string[] fileTypes)
        {
            Rank = 1;
            Kind = kind;
            SupportedFileTypes = fileTypes;
        }

        public Kind Kind { get; }

        public int Rank { get; set; }

        public string[] SupportedFileTypes { get; }

        public bool Supports(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;

            string ext = Path.GetExtension(filePath);
            foreach (string extension in SupportedFileTypes)
                if (filePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

            return false;
        }
    }
}