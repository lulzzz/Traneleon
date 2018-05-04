using System;
using System.Collections.Generic;

namespace Acklann.WebFlow.Transpilation
{
    public interface ICompilerFactory
    {
        IEnumerable<Type> GetAllOperatorTypes();

        ICompiler CreateInstance(Type type);

        ICompiler CreateInstance(ICompilierOptions options);

        IEnumerable<Type> GetOperatorTypesThatSupports(ICompilierOptions options);
    }
}