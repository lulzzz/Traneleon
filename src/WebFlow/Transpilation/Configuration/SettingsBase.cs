namespace Acklann.WebFlow.Transpilation.Configuration
{
    public abstract class SettingsBase
    {
        public SettingsBase(Operand operand, string outputFile, params string[] sourceFiles)
        {
            SourceFiles = sourceFiles;
            OutputFile = outputFile;
            Operand = operand;
        }

        public Operand Operand { get; }

        public string OutputFile { get; }

        public string[] SourceFiles { get; }
    }
}