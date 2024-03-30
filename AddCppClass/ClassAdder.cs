using AddCppClass;
using EnvDTE;
using EnvDTE80;
using System.IO;
using System.Xml.Linq;

namespace Dwarfovich.AddCppClass
{
    public class ClassAdder
    {
        private static readonly DTE2 dte = AddCppClassPackage.dte;

        private static EnvDTE.Project CurrentProject()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Array projects = dte.ActiveSolutionProjects as Array;
            if (projects != null && projects.Length != 0)
            {
                return projects.GetValue(0) as EnvDTE.Project;
            }
            else
            {
                throw new InvalidOperationException("No selected project in solution");
            }
        }
        private static void AddFileToProject(EnvDTE.Project project, FileInfo file, string itemType = null)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ProjectItems projectItems = project.ProjectItems;
            projectItems.AddFromTemplate(file.FullName, file.Name);
        }

        private static XDocument CreateFilterXmlDocument()
        {
            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
            var root = new XElement(XNamespace.Get("http://schemas.microsoft.com/developer/msbuild/2003") + "Project");
            root.Add(new XAttribute("ToolsVersion", "4.0"));
            doc.Add(root);

            return doc;
        }
        private static XDocument OpenFilterXmlDocument(string filterfilePath)
        {
            if (File.Exists(filterfilePath)) {
                return new XDocument(filterfilePath);
            }
            else
            {
                return CreateFilterXmlDocument();
            }
        }
        public static void AddClass(ClassGenerator generator, string subFolder)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            EnvDTE.Project project = CurrentProject();
            string projectPath = new FileInfo(project.FullName).DirectoryName;
            string classPath = String.IsNullOrEmpty(subFolder) ? projectPath + '\\' : projectPath + '\\' + subFolder + '\\';
            Directory.CreateDirectory(classPath);


            string filterFilePath = project.FullName + ".filters";
            Logger.Log("filterFilePath = " + filterFilePath);
            var doc = OpenFilterXmlDocument(filterFilePath);

            doc.Save(filterFilePath);

            project.DTE.ExecuteCommand("Project.ReloadProject");
        }
        public static void CreateHeaderFile(ClassGenerator generator, string folder)
        {
            string path = folder + generator.headerFilename;
            if(File.Exists(path))
            {
                throw new InvalidOperationException("Header file for path " + path + " already exists");
            }
        }
        public static void CreateImplementationFile(ClassGenerator generator, string folder)
        {

        }
    }
}
