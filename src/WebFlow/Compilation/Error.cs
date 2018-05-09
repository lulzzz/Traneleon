namespace Acklann.WebFlow.Compilation
{
    public struct Error
    {
        public Error(string description, string file, int lineNumber)
        {
            File = file;
            Message = description;
            LineNumber = lineNumber;
        }

        public string File { get; }

        public int LineNumber { get; }

        public string Message { get; }
    }
}