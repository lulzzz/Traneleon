namespace Acklann.WebFlow.Compilation
{
    public struct Error
    {
        public Error(string description, string file, int lineNumber, int column = 0, int code = 0)
        {
            File = file;
            Code = code;
            Column = column;
            Message = description;
            LineNumber = lineNumber;
        }

        public int Code { get; }

        public int Column { get; }

        public string File { get; }

        public int LineNumber { get; }

        public string Message { get; }
    }
}