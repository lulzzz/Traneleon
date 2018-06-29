namespace Acklann.WebFlow.Compilation
{
    public struct CompilerError
    {
        public CompilerError(string description, string file, int lineNumber, int column = 0, int code = 0)
        {
            File = file;
            Code = code;
            Column = column;
            Message = description;
            Line = lineNumber;
        }

        public int Code { get; }

        public int Column { get; }

        public string File { get; }

        public int Line { get; }

        public string Message { get; }
    }
}