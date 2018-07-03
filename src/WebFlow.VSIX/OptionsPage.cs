using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace Acklann.WebFlow
{
    public class OptionsPage : DialogPage
    {
        public OptionsPage()
        {
            AutoConfig = true;
            Pattern = "*webflow*";
        }

        [Category("General")]
        [LocDisplayName("Profile Location")]
        [Description("The absolute path in which your task profiles are stored.")]
        public string Pattern { get; set; }

        [Category("General")]
        [LocDisplayName("Add Configuration to Project Automatically")]
        [Description("Determines whether a configuration file should automatically be added to web projects.")]
        public bool AutoConfig { get; set; }
    }
}