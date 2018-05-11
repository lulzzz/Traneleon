using Acklann.WebFlow.Compilation;
using System.Collections.Generic;

namespace Acklann.WebFlow.Tasks
{
    public interface IItemGroup
    {
        bool Enabled { get; set; }

        string Suffix { get; set; }

        List<string> Include { get; set; }

        List<string> Exclude { get; set; }

        string OutputDirectory { get; set; }

        bool CanAccept(string filePath);

        ICompilierOptions CreateCompilerOptions(string filePath);
    }
}