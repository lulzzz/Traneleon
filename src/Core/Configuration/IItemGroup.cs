using Acklann.Traneleon.Compilation;
using System.Collections.Generic;

namespace Acklann.Traneleon.Configuration
{
    public interface IItemGroup
    {
        bool Enabled { get; set; }

        string OutputDirectory { get; set; }
        string Suffix { get; set; }
        string WorkingDirectory { get; set; }

        bool CanAccept(string filePath);

        IEnumerable<string> EnumerateFiles();

        IEnumerable<ICompilierOptions> CreateCompilerOptions(string filePath);
    }
}