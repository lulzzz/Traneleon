using System;
using System.Collections.Generic;

namespace Acklann.Traneleon.Compilation
{
    public interface ICompilerFactory
    {
        IEnumerable<Type> GetAllCompilerTypes();

        ICompiler CreateInstance(Type compilerType);

        ICompiler CreateInstance(ICompilierOptions options);

        IEnumerable<Type> GetCompilerTypesThatSupports(ICompilierOptions options);
    }
}