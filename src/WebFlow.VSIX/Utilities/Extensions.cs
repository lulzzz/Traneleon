using EnvDTE80;
using System;
using System.Collections.Generic;

namespace Acklann.WebFlow.Utilities
{
    public static class Extensions
    {
        public static EnvDTE.Project GetProject(this DTE2 dte, string filePath)
        {
            if (dte == null) throw new ArgumentNullException(nameof(dte));

            foreach (EnvDTE.Project project in GetActiveProjects(dte))
                if (project.FileName == filePath)
                {
                    return project;
                }

            return null;
        }

        public static IEnumerable<EnvDTE.Project> GetActiveProjects(this DTE2 dte)
        {
            if (dte == null) throw new ArgumentNullException(nameof(dte));

            EnvDTE.Solution solution = dte.Solution;
            if (solution != null)
                foreach (EnvDTE.Project solutionItem in solution.Projects)
                {
                    foreach (EnvDTE.Project project in getProjectsNestedInSolutionFolder(solutionItem))
                        if (project.Kind != EnvDTE.Constants.vsProjectKindUnmodeled)
                        {
                            yield return project;
                        }
                }

            /// Checking to see of the <EnvDTE.Project> is a solution folder.
            /// Solution folders are treated as projects so I am recursively looking for the actual project(s) within the folder.
            /// If the <EnvDTE.Project> is not a solution folder just return it.
            IEnumerable<EnvDTE.Project> getProjectsNestedInSolutionFolder(EnvDTE.Project project)
            {
                if (project.Kind == EnvDTE.Constants.vsProjectItemKindSolutionItems)
                {
                    foreach (EnvDTE.ProjectItem item in project.ProjectItems)
                    {
                        EnvDTE.Project folder = item?.SubProject;
                        if (folder != null)
                        {
                            foreach (EnvDTE.Project child in getProjectsNestedInSolutionFolder(folder))
                            {
                                yield return child;
                            }
                        }
                    }
                }
                else if (project?.FullName?.EndsWith("proj") ?? false)
                {
                    yield return project;
                }
            }
        }

        public static IEnumerable<EnvDTE.Project> GetSelectedProjects(this DTE2 dte, bool defaultToStartup = true)
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
            if (noProjectsWereSelected && defaultToStartup)
            {
                EnvDTE.SolutionBuild solution = dte.Solution.SolutionBuild;

                if (solution?.StartupProjects != null)
                    foreach (var item in (Array)solution.StartupProjects)
                    {
                        yield return dte.Solution.Item(item);
                    }
            }
        }

        public static IEnumerable<EnvDTE.SelectedItem> GetSelectedItems(this DTE2 dte)
        {
            if (dte == null) throw new ArgumentNullException(nameof(dte));

            EnvDTE.SelectedItems selectedItems = dte.SelectedItems;
            if (selectedItems != null)
            {
                foreach (EnvDTE.SelectedItem item in selectedItems) yield return item;
            }
        }
    }
}