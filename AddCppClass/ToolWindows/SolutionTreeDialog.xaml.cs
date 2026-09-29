using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using System.Xml.Linq;
using EnvDTE;
namespace Dwarfovich.AddCppClass
{
    public partial class SolutionTreeDialog : DialogWindow
    {
        public SolutionTreeDialog()
        {

            InitializeComponent();

            Logger.Log("Hello from SolutionTreeDialog!");

            var solution = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));

            if (solution == null)
            {
                Logger.Log("IVsSolution not found");
                return;
            }

            int hr = solution.GetProjectEnum(
                (uint)__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION,
                Guid.Empty,
                out IEnumHierarchies enumHierarchies);

            if (ErrorHandler.Failed(hr))
            {
                Logger.Log($"GetProjectEnum failed: 0x{hr:X8}");
                return;
            }

            var hierarchies = new IVsHierarchy[1];

            while (enumHierarchies.Next(1, hierarchies, out uint fetched) == VSConstants.S_OK
                   && fetched == 1)
            {
                var hierarchy = hierarchies[0];

                hr = hierarchy.GetProperty(
                    VSConstants.VSITEMID_ROOT,
                    (int)__VSHPROPID.VSHPROPID_ExtObject,
                    out object extObject);

                if (ErrorHandler.Failed(hr))
                    continue;

                var project = extObject as EnvDTE.Project;

                if (project == null)
                    continue;

                Logger.Log($"Project: {project.Name}");

                PrintProjectItems(project.ProjectItems);
            }
        }

        private void PrintHierarchy(IVsHierarchy hierarchy, uint itemId, int level)
        {
            if (hierarchy == null)
            {
                Logger.Log("hierarchy == null");
                return;
            }
        }

        private void PrintProjectItems(EnvDTE.ProjectItems items, int level = 0)
        {
            if (items == null)
                return;

            foreach (EnvDTE.ProjectItem item in items)
            {
                string indent = new string(' ', level * 2);

                string path = "<no path>";

                try
                {
                    if (item.FileCount > 0)
                        path = item.FileNames[1];
                }
                catch (Exception ex)
                {
                    path = $"<error: {ex.Message}>";
                }

                Logger.Log($"{indent}{item.Name} | {path}");

                try
                {
                    PrintProjectItems(item.ProjectItems, level + 1);
                }
                catch (Exception ex)
                {
                    Logger.Log($"{indent}  <children error: {ex.Message}>");
                }
            }
        }
    }
}
