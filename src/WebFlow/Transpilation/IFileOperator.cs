using Acklann.WebFlow.Transpilation.Configuration;

namespace Acklann.WebFlow.Transpilation
{
    public interface IFileOperator
    {
        bool CanExecute(SettingsBase options);

        ResultFile Execute(SettingsBase options);
    }
}