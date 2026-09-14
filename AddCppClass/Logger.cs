using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell.Interop;

namespace Dwarfovich
{
    internal static class Logger
    {
        private static string name;
        private static IVsOutputWindowPane pane;
        private static IVsOutputWindow output;

        public static void Initialize(IServiceProvider provider, string name)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            output = (IVsOutputWindow)provider.GetService(typeof(SVsOutputWindow));
            Assumes.Present(output);
            Logger.name = name;
        }

        public static void Log(object message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                if (EnsurePane())
                {
                    pane.OutputString(DateTime.Now.ToString() + ": " + message + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
            }
        }

        private static bool EnsurePane()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (pane == null)
            {
                Guid guid = Guid.NewGuid();

                int hr = output.CreatePane(ref guid, name, 1, 1);
                System.Diagnostics.Debug.WriteLine($"CreatePane: 0x{hr:X8}");

                hr = output.GetPane(ref guid, out pane);
                System.Diagnostics.Debug.WriteLine($"GetPane: 0x{hr:X8}, pane == null: {pane == null}");
            }

            return pane != null;
        }

        public static void DumpSolution()
        {
            var dte = (DTE2)Package.GetGlobalService(typeof(DTE));

            Logger.Log($"Solution: {dte.Solution.FullName}");

            foreach (EnvDTE.Project project in dte.Solution.Projects)
            {
                DumpProject(project, 0);
            }
        }

        private static void DumpProject(EnvDTE.Project project, int indent)
        {
            Logger.Log($"{new string(' ', indent * 2)}Project: {project.Name}");
            Logger.Log($"{new string(' ', indent * 2)}Kind: {project.Kind}");
            Logger.Log($"{new string(' ', indent * 2)}FullName: {project.FullName}");

            if (project.ProjectItems != null)
            {
                foreach (EnvDTE.ProjectItem item in project.ProjectItems)
                {
                    DumpProjectItem(item, indent + 1);
                }
            }

            // Некоторые типы проектов (например solution folders)
            // содержат вложенные проекты.
            if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
            {
                foreach (EnvDTE.ProjectItem item in project.ProjectItems)
                {
                    if (item.SubProject != null)
                    {
                        DumpProject(item.SubProject, indent + 1);
                    }
                }
            }
        }

        private static void DumpProjectItem(EnvDTE.ProjectItem item, int indent)
        {
            Logger.Log($"{new string(' ', indent * 2)}{item.Name}");

            if (item.ProjectItems != null)
            {
                foreach (EnvDTE.ProjectItem child in item.ProjectItems)
                {
                    DumpProjectItem(child, indent + 1);
                }
            }
        }
    }
}