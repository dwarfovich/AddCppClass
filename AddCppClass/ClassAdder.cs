using AddCppClass;
using EnvDTE;
using EnvDTE80;
using System.IO;
using System.Linq;
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

        private static XDocument CreateFilterXmlDocument()
        {
            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
            var root = new XElement(XNamespace.Get("http://schemas.microsoft.com/developer/msbuild/2003") + "Project");
            root.Add(new XAttribute("ToolsVersion", "4.0"));
            doc.Add(root);

            return doc;
        }
        private static XDocument OpenFilterXmlDocument(string filterFilePath)
        {
            Logger.Log("OpenFilterXmlDocument: " + filterFilePath);
            if (File.Exists(filterFilePath))
            {
                return XDocument.Load(filterFilePath);
            }
            else
            {
                return CreateFilterXmlDocument();
            }
        }
        public static void CreateHeaderFile(EnvDTE.Project project, ClassGenerator generator, string projectPath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string path = Path.Combine(projectPath, generator.headerSubfolder);
            Directory.CreateDirectory(path);
            path = Path.Combine(path, generator.headerFilename);
            File.Create(path);
            ProjectItems projectItems = project.ProjectItems;
            projectItems.AddFromFile(path);
        }

        public static void CreateImplementationFile(EnvDTE.Project project, ClassGenerator generator, string projectPath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string cppPath = "";
            if (generator.useSingleSubfolder)
            {
                cppPath = Path.Combine(projectPath, generator.headerSubfolder, generator.implementationFilename);
            }
            else
            {
                cppPath = Path.Combine(projectPath, generator.implementationSubfolder, generator.implementationFilename);
            }
            File.Create(cppPath);
            ProjectItems projectItems = project.ProjectItems;
            projectItems.AddFromFile(cppPath);
        }

        private static XElement GetFilterItemGroup(XDocument doc, XNamespace ns)
        {
            foreach (var element in doc.Root.Elements(ns + "ItemGroup"))
            {
                if (element.Elements(ns + "Filter").Any())
                {
                    return element;
                }
            }

            var filterItemGroup = new XElement(ns + "ItemGroup");
            doc.Root.Add(filterItemGroup);
            return filterItemGroup;
        }
        public static void AddFilter(XDocument doc, XNamespace ns, ClassGenerator generator, string path)
        {
            XElement filterItemGroup = GetFilterItemGroup(doc, ns);
            string[] filterTokens = path.Split('\\');

            string filterSubPath = "";
            for (int i = 0; i < filterTokens.Length; ++i, filterSubPath += '/')
            {
                filterSubPath += filterTokens[i];
                var element = filterItemGroup.Descendants(ns + "Filter").Where(el => (string)el.Attribute("Include") == filterSubPath);
                if (element is null || !element.Any())
                {
                    XElement filterElement = new XElement(ns + "Filter", new XAttribute("Include", filterSubPath));
                    XElement identifierElement = new XElement(ns + "UniqueIdentifier");
                    identifierElement.Add(Guid.NewGuid().ToString("B"));
                    filterElement.Add(identifierElement);
                    filterItemGroup.Add(filterElement);
                }
            }
        }
        public static void ReplaceHeaderFileFilter(XDocument doc, XNamespace ns, ClassGenerator generator, string newFilterPath)
        {
            var itemGroupElements = doc.Root.Elements(ns + "ItemGroup");
            string filePath = Path.Combine(newFilterPath, generator.headerFilename);
            foreach (var fileFilter in itemGroupElements) {
                var clElement = fileFilter.Descendants(ns + "ClInclude").Where(el => (string)el.Attribute("Include") == filePath);
                clElement.Descendants(ns + "Filter").Remove();
            }
            XElement newItemGroup = new XElement(ns + "ItemGroup");
            XElement clCompileElement = new XElement(ns + "ClInclude", new XAttribute("Include", filePath));
            newItemGroup.Add(clCompileElement);
            XElement fileFilterElement = new XElement(ns + "Filter");
            fileFilterElement.Add(newFilterPath);
            clCompileElement.Add(fileFilterElement);
            doc.Root.Add(newItemGroup);
        }
        public static void AddClass(ClassGenerator generator)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            EnvDTE.Project project = CurrentProject();

            string filterFilePath = project.FullName + ".filters";
            var doc = OpenFilterXmlDocument(filterFilePath);
            var ns = doc.Root.GetDefaultNamespace();

            string projectPath = new FileInfo(project.FullName).DirectoryName;
            CreateHeaderFile(project, generator, projectPath);
            if (generator.hasImplementationFile)
            {
                CreateImplementationFile(project, generator, projectPath);
            }
            project.DTE.ExecuteCommand("File.SaveAll");

            if (generator.createFilters)
            {
                AddFilter(doc, ns, generator, generator.implementationSubfolder);
                AddFilter(doc, ns, generator, generator.headerSubfolder);
                ReplaceHeaderFileFilter(doc, ns, generator, generator.headerSubfolder);
                if (generator.hasImplementationFile)
                {
                    //ReplaceFileFilter(doc, ns, generator, generator.implementationSubfolder);
                }
            }

            doc.Save(filterFilePath);
            project.DTE.ExecuteCommand("Project.UnloadProject");
            project.DTE.ExecuteCommand("Project.ReloadProject");
        }
    }
}
