namespace Acklann.WebFlow.Transpilation
{
    public struct ResultFile
    {
        public ResultFile(string path, string sourceMapPath, string intermediatePath)
        {
        }

        public static ResultFile CreateFromMinifiedFile(string path)
        {
            return new ResultFile();
        }

        public bool FileExists()
        {
            throw new System.NotImplementedException();
        }
    }
}