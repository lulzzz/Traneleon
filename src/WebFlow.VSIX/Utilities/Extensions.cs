using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;

namespace Acklann.WebFlow.Utilities
{
    public static class Extensions
    {
        public static IEnumerable<EnvDTE.Project> GetProjects(this DTE2 dte)
        {
            if (dte == null) throw new ArgumentNullException(nameof(dte));

            EnvDTE.Solution solution = dte.Solution;
            if (solution != null)
                foreach (EnvDTE.Project solutionItem in solution.Projects)
                {
                    foreach (EnvDTE.Project project in getProjectsNestedInSolutionFolder(solutionItem))
                    {
                        yield return project;
                    }
                }

            /// Checking to see of the <EnvDTE.Project> is a solution folder.
            /// Solution folders are treated as projects so I am recursively looking for the actual project(s) within the folder.
            /// If the <EnvDTE.Project> is not a solution folder just return it.
            IEnumerable<EnvDTE.Project> getProjectsNestedInSolutionFolder(EnvDTE.Project project)
            {
                if (project.Kind == "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}"/* solution folder guid */)
                {
                    foreach (EnvDTE.ProjectItem item in project.ProjectItems)
                    {
                        EnvDTE.Project folder = item?.SubProject;
                        if (folder != null)
                        {
                            foreach (var child in getProjectsNestedInSolutionFolder(folder))
                            {
                                yield return child;
                            }
                        }
                    }
                }
                else if ((project?.FullName?.EndsWith("proj")) ?? false)
                {
                    yield return project;
                }
            }
        }

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

        public static void Write(this IVsStatusbar statusBar, string text)
        {
            statusBar.IsFrozen(out int frozen);

            if (frozen != 0) statusBar.FreezeOutput(0);
            statusBar.SetText(text);
            statusBar.FreezeOutput(1);
        }
    }
}