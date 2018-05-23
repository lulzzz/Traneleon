using Acklann.WebFlow.Compilation;
using System.Collections.Generic;

namespace Acklann.WebFlow.Configuration
{
    public interface IItemGroup
    {
        bool Enabled { get; set; }

        string Suffix { get; set; }

        string OutputDirectory { get; set; }

        bool CanAccept(string filePath);

        IEnumerable<string> EnumerateFiles();

        ICompilierOptions CreateCompilerOptions(string filePath);
    }
}