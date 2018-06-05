using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace Acklann.WebFlow
{
    public class OptionsPage : DialogPage
    {
        public OptionsPage()
        {
            Pattern = "*webflow*";
        }

        [Category("General")]
        [LocDisplayName("Profile Location")]
        [Description("The absolute path in which your task profiles are stored.")]
        public string Pattern { get; set; }
    }
}