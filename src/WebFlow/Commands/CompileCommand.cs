using Acklann.GlobN;
using Acklann.NShellit.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acklann.WebFlow.Commands
{
    [Command("", Cmdlet = "Invoke-WebFlow")]
    [Summary("")]
    public class CompileCommand : ICommand
    {
        public CompileCommand(string configFile)
        {
            ConfigFile = configFile.ExpandPath(Environment.CurrentDirectory, true);
        }

        [Required, Parameter('c', "config", Kind = "path")]
        [Summary("The absolute or relative path of the config file.")]
        public string ConfigFile { get; }

        public int Execute()
        {


            throw new System.NotImplementedException();
        }
    }
}
