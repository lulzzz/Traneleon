namespace Acklann.WebFlow.Compilation
{
    public readonly struct CompilerError
    {
        public CompilerError(string description, string file, int lineNumber, int column = 0, int code = 0, int category = 0)
        {
            File = file;
            Code = code;
            Column = column;
            Line = lineNumber;
            Category = category;
            Message = description;
        }

        public int Category { get; }

        public int Code { get; }

        public int Column { get; }

        public string File { get; }

        public int Line { get; }

        public string Message { get; }
    }
}