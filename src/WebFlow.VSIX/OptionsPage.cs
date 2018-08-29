using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace Acklann.WebFlow
{
    public class OptionsPage : DialogPage
    {
        public OptionsPage()
        {
            AutoConfig = true;
        }

        [Category("General")]
        [LocDisplayName("Add Configuration to Project Automatically")]
        [Description("Determines whether a configuration file should automatically be added to web projects.")]
        public bool AutoConfig { get; set; }
    }
}