using Acklann.WebFlow.Compilation;
using System.Collections.Generic;

namespace Acklann.WebFlow.Configuration
{
    public interface IItemGroup
    {
        bool Enabled { get; set; }

        string Suffix { get; set; }

        string OutputDirectory { get; set; }

        string WorkingDirectory { get; set; }

        bool CanAccept(string filePath);

        IEnumerable<string> EnumerateFiles();

        IEnumerable<ICompilierOptions> CreateCompilerOptions(string filePath);
    }
}