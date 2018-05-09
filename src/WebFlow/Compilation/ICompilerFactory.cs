using System;
using System.Collections.Generic;

namespace Acklann.WebFlow.Compilation
{
    public interface ICompilerFactory
    {
        IEnumerable<Type> GetAllOperatorTypes();

        ICompiler CreateInstance(Type compilerType);

        ICompiler CreateInstance(ICompilierOptions options);

        IEnumerable<Type> GetOperatorTypesThatSupports(ICompilierOptions options);
    }
}