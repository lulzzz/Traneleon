using Acklann.WebFlow.Transpilation.Configuration;

namespace Acklann.WebFlow.Transpilation
{
    public class NullFileOperator : IFileOperator
    {
        public bool CanExecute(SettingsBase options) => true;

        public ResultFile Execute(SettingsBase options)
        {
            return new ResultFile();
        }
    }
}