using AddCppClass;
using Community.VisualStudio.Toolkit;
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
        private static XDocument OpenFilterXmlDocument(string filterFilePath)
        {
            Logger.Log("OpenFilterXmlDocument: " + filterFilePath);
            if (File.Exists(filterFilePath))
            {
                return new XDocument(filterFilePath);
            }
            else
            {
                return CreateFilterXmlDocument();
            }
        }
        public static void CreateHeaderFile(ClassGenerator generator, string projectPath)
        {
            string path = Path.Combine(projectPath, generator.headerSubfolder);
            Directory.CreateDirectory(path);
            path = Path.Combine(path, generator.headerFilename);
            File.Create(path);
        }

        public static void CreateImplementationFile(ClassGenerator generator, string projectPath)
        {
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
        }

        private static XElement GetFilterItemGroup(XDocument doc, XNamespace ns)
        {
            foreach (var element in doc.Elements(ns + "ItemGroup"))
            {
                if (element.Elements(ns + "Filter").Any())
                {
                    return element;
                }
            }

            var filterItemGroup = new XElement("ItemGroup");
            doc.Root.Add(filterItemGroup);
            return filterItemGroup;
        }
        public static void AddFilterForHeader(XDocument doc, ClassGenerator generator)
        {
            var ns = doc.Root.GetDefaultNamespace();
            XElement itemGroup = GetFilterItemGroup(doc, ns);

            string filterFullPath = generator.headerSubfolder;
            string[] filterTokens = filterFullPath.Split('\\');

            string filterSubPath = "";
            for (int i = 0; i < filterTokens.Length; ++i) {
                filterSubPath += filterTokens[i];
                var element = itemGroup.Descendants(ns + "Filter").Where(el => (string)el.Attribute("Include") == filterSubPath);
                if (element is null || !element.Any())
                {
                    XElement filterElement = new XElement(ns + "Filter", new XAttribute("Include", filterSubPath));
                    XElement identifierElement = new XElement(ns + "UniqueIdentifier");
                    identifierElement.Add(Guid.NewGuid().ToString("B"));
                    filterElement.Add(identifierElement);
                    itemGroup.Add(filterElement);
                }
                filterSubPath += '\\';
            }

            XElement fileItemGroupElement = new XElement(ns + "ItemGroup");
            XElement ciCompileElement = new XElement(ns + "ClCompile", new XAttribute("Include", generator.implementationFilename));
            fileItemGroupElement.Add(ciCompileElement);
            XElement fileFilterElement = new XElement(ns + "Filter");
            fileFilterElement.Add(filterFullPath);
            ciCompileElement.Add(fileFilterElement);
            doc.Root.Add(fileItemGroupElement);
        }
        public static void AddFilterForCpp(XDocument doc, ClassGenerator generator)
        {
            var ns = doc.Root.GetDefaultNamespace();
            XElement itemGroup = GetFilterItemGroup(doc, ns);

            string filterFullPath = generator.headerSubfolder;
            string[] filterTokens = filterFullPath.Split('\\');

            string filterSubPath = "";
            for (int i = 0; i < filterTokens.Length; ++i)
            {
                filterSubPath += filterTokens[i];
                var element = itemGroup.Descendants(ns + "Filter").Where(el => (string)el.Attribute("Include") == filterSubPath);
                if (element is null || !element.Any())
                {
                    XElement filterElement = new XElement(ns + "Filter", new XAttribute("Include", filterSubPath));
                    XElement identifierElement = new XElement(ns + "UniqueIdentifier");
                    identifierElement.Add(Guid.NewGuid().ToString("B"));
                    filterElement.Add(identifierElement);
                    itemGroup.Add(filterElement);
                }
                filterSubPath += '\\';
            }

            XElement fileItemGroupElement = new XElement(ns + "ItemGroup");
            XElement ciCompileElement = new XElement(ns + "ClCompile", new XAttribute("Include", generator.headerFilename));
            fileItemGroupElement.Add(ciCompileElement);
            XElement fileFilterElement = new XElement(ns + "Filter");
            fileFilterElement.Add(filterFullPath);
            ciCompileElement.Add(fileFilterElement);
            doc.Root.Add(fileItemGroupElement);
        }
        public static void AddClass(ClassGenerator generator)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            EnvDTE.Project project = CurrentProject();

            string filterFilePath = project.Name + ".vcxproj.filters";
            var doc = OpenFilterXmlDocument(filterFilePath);

            string projectPath = new FileInfo(project.FullName).DirectoryName;
            CreateHeaderFile(generator, projectPath);
            AddFilterForHeader(doc, generator);
            if (generator.hasImplementationFile)
            {
                CreateImplementationFile(generator, projectPath);
                AddFilterForCpp(doc, generator);
            }

            doc.Save(filterFilePath);
            //project.DTE.ExecuteCommand("Project.ReloadProject");
            project.DTE.ExecuteCommand("Project.UnloadProject");
            project.DTE.ExecuteCommand("Project.ReloadProject");
        }
    }
}
