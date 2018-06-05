using EnvDTE80;
using System;
using System.Collections.Generic;

namespace Acklann.WebFlow.Utilities
{
    public static class Extensions
    {
        public static IEnumerable<EnvDTE.Project> GetSelectedProjects(this DTE2 dte)
        {
            if (dte == null) throw new ArgumentNullException(nameof(dte));

            // Checking to see if the user has any project(s) selected.
            // If they don't have any selected return the start-up project.
            bool noProjectsWereSelected = true;
            EnvDTE.SelectedItems selectedItems = dte.SelectedItems;
            if (selectedItems != null)
            {
                foreach (EnvDTE.SelectedItem item in selectedItems)
                    if (item.Project != null)
                    {
                        noProjectsWereSelected = false;
                        yield return item.Project;
                    }
            }

            // Checking to see if their is a start-up project
            // since no project files where selected.
            if (noProjectsWereSelected)
            {
                EnvDTE.SolutionBuild solution = dte.Solution.SolutionBuild;

                if (solution?.StartupProjects != null)
                    foreach (var item in (Array)solution.StartupProjects)
                    {
                        yield return dte.Solution.Item(item);
                    }
            }
        }
    }
}