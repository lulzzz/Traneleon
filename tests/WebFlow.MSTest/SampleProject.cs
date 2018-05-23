using Acklann.GlobN;
using System;

namespace Acklann.WebFlow
{
    public static class SampleProject
    {
        static SampleProject()
        {
            DirectoryName = $"../../../{nameof(WebFlow)}.Sample".ExpandPath(AppContext.BaseDirectory);
        }

        public static string DirectoryName { get; }
    }
}