using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Acklann.Traneleon.Compilation
{
    public class CompilerFactory : ICompilerFactory
    {
        public CompilerFactory()
        {
            var assemblyTypes = (from t in typeof(CompilerFactory).Assembly.ExportedTypes
                                 where !t.IsInterface && !t.IsAbstract && typeof(ICompiler).IsAssignableFrom(t)
                                 select t);

            _compilerTypes = new LinkedList<(Type, CapabilityAttribute)>();
            foreach (Type type in assemblyTypes)
            {
                var attribute = type.GetCustomAttribute<CapabilityAttribute>();
                if (attribute != null)
                {
                    _compilerTypes.Add((type, attribute));
                }
            }
        }

        public ICompiler CreateInstance(Type compilerType)
        {
            return (ICompiler)Activator.CreateInstance(compilerType);
        }

        public ICompiler CreateInstance(ICompilierOptions options)
        {
            foreach (var prospect in GetCompilerTypesThatSupports(options))
            {
                var compiler = (ICompiler)Activator.CreateInstance(prospect);
                if (compiler.CanExecute(options))
                {
                    return compiler;
                }
            }

            return new NullCompiler();
        }

        public IEnumerable<Type> GetAllCompilerTypes()
        {
            return _compilerTypes.Select(x => x.Type);
        }

        public IEnumerable<Type> GetCompilerTypesThatSupports(ICompilierOptions options)
        {
            return (from prospect in _compilerTypes
                    where
                        prospect.Capability.Kind == options.Kind
                        &&
                        prospect.Capability.Supports(options.FileType)
                    orderby prospect.Capability.Rank
                    select prospect.Type);
        }

        #region Private Members

        private readonly ICollection<(Type Type, CapabilityAttribute Capability)> _compilerTypes;

        #endregion Private Members
    }
}