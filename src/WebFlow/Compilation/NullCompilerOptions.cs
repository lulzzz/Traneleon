namespace Acklann.WebFlow.Compilation
{
    public struct NullCompilerOptions : ICompilierOptions
    {
        public Kind Kind => Kind.Bundle;

        public string Ext() => string.Empty;
    }
}