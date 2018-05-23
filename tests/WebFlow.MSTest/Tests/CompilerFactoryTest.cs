using Acklann.WebFlow.Compilation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.IO;
using System.Linq;

namespace Acklann.WebFlow.Tests
{
    [TestClass]
    public class CompilerFactoryTest
    {
        [TestMethod]
        public void CreateInstance_can_create_any_icompiler_type()
        {
            // Arrange
            var cases = (from t in typeof(ICompilerFactory).Assembly.ExportedTypes
                         where !t.IsInterface && !t.IsAbstract && typeof(ICompiler).IsAssignableFrom(t)
                         select t).ToArray();

            var sut = new CompilerFactory();

            // Act & Assert
            foreach (var type in cases)
            {
                sut.CreateInstance(type).ShouldBeAssignableTo<ICompiler>();
            }
        }

        [TestMethod]
        public void CreateInstance_should_return_the_ideal_compiler_foreach_option()
        {
            // Arrange
            string outputDir = Path.GetTempPath();
            var cases = new(Type, ICompilierOptions)[]
            {
                (typeof(SassCompiler), new TranspilierSettings("file.scss", outputDir)),
                (typeof(TypescriptCompiler), new TranspilierSettings("file.ts", outputDir)),
            };

            var sut = new CompilerFactory();

            // Act
            foreach (var item in cases)
            {
                var idealCompiler = sut.CreateInstance(item.Item2);

                // Assert
                idealCompiler.ShouldBeOfType(item.Item1);
            }
        }

        [TestMethod]
        public void GetAllCompilerTypes_should_return_all_type_that_implement_icompiler()
        {
            // Arrange
            var sut = new CompilerFactory();

            // Act
            var results = sut.GetAllCompilerTypes().ToArray();

            // Assert
            results.ShouldNotBeEmpty();
            results.ShouldAllBe(x => !x.IsAbstract);
            results.ShouldAllBe(x => !x.IsInterface);
            results.ShouldAllBe(x => typeof(ICompiler).IsAssignableFrom(x));
            results.ShouldAllBe(x => x.IsDefined(typeof(CapabilityAttribute), false));
        }
    }
}