namespace Acklann.Traneleon.Compilation
{
    public struct CompilerResult : ICompilierResult
    {
        public CompilerResult(Kind kind, string sourceFile, string outFile, ShellBase shell)
        {
            Kind = kind;
            OutputFile = outFile;
            SourceFile = sourceFile;
            Succeeded = (shell.ExitCode == 0);
            ErrorList = shell.StandardError.GetErrors();
            ExecutionTime = (shell.ExitTime.Ticks - shell.StartTime.Ticks);
        }

        public CompilerResult(Kind kind, bool succeeded, long executionTime, string sourceFile, string outFile, params CompilerError[] errors)
        {
            Kind = kind;
            ErrorList = errors;
            OutputFile = outFile;
            Succeeded = succeeded;
            SourceFile = sourceFile;
            ExecutionTime = executionTime;
        }

        public Kind Kind { get; }

        public bool Succeeded { get; }

        public string SourceFile { get; }

        public string OutputFile { get; }

        public long ExecutionTime { get; }

        public CompilerError[] ErrorList { get; }
    }
}