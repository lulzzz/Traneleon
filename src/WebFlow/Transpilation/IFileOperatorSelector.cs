using Acklann.WebFlow.Transpilation.Configuration;
using System;
using System.Collections.Generic;

namespace Acklann.WebFlow.Transpilation
{
    public interface IFileOperatorSelector
    {
        IEnumerable<Type> GetAllOperatorTypes();

        IFileOperator CreateInstance(Type type);

        IFileOperator CreateInstance(SettingsBase options);

        IEnumerable<Type> GetOperatorTypesThatSupports(SettingsBase options);
    }
}