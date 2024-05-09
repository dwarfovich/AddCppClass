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

        private static void PopulateHeaderFile(FileStream fileStream, Settings settings)
        {
            using (var writer = new StreamWriter(fileStream))
            {
                writer.WriteLine("#pragma once" + Environment.NewLine);
                foreach (string ns in settings.mostRecentNamespaceTokenized)
                {
                    writer.WriteLine("namespace " + ns + " {");
                }
                writer.Write(Environment.NewLine);
                writer.WriteLine("class " + settings.className + " {");
                writer.WriteLine("public:");
                writer.WriteLine("private:");
                writer.WriteLine("};");
                if(settings.mostRecentNamespaceTokenized.Length > 0)
                {
                    writer.Write(Environment.NewLine);
                }
                for (int i = settings.mostRecentNamespaceTokenized.Count() - 1; i >= 0; --i)
                {
                    writer.WriteLine("} // namespace " + settings.mostRecentNamespaceTokenized[i]);
                }
            }
        }

        private static void PopulateImplementationFile(FileStream fileStream, Settings settings)
        {
            using (var writer = new StreamWriter(fileStream))
            {
                if (settings.includePrecompiledHeader)
                {
                    writer.WriteLine("#include \"" + settings.precompiledHeader + '\"');
                }
                if (String.IsNullOrEmpty(settings.RecentHeaderSubfolder()))
                {
                    writer.WriteLine("#include \"" + settings.headerFilename + '\"');
                }
                else
                {
                    writer.WriteLine("#include \"" + settings.RecentHeaderSubfolder() + '/' + settings.headerFilename + '\"');
                }
            }
        }

        public static void CreateHeaderFile(EnvDTE.Project project, Settings settings, string projectPath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string path = Path.Combine(projectPath, settings.RecentHeaderSubfolder());
            Directory.CreateDirectory(path);
            path = Path.Combine(path, settings.headerFilename);
            var fileStream = File.Create(path);
            PopulateHeaderFile(fileStream, settings);
            fileStream.Close();
            ProjectItems projectItems = project.ProjectItems;
            projectItems.AddFromFile(path);
        }

        public static void CreateImplementationFile(EnvDTE.Project project, Settings settings, string projectPath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string implementationPath = "";
            if (settings.useSingleSubfolder)
            {
                implementationPath = Path.Combine(projectPath, settings.RecentHeaderSubfolder(), settings.implementationFilename);
            }
            else
            {
                implementationPath = Path.Combine(projectPath, settings.RecentImplementationSubfolder(), settings.implementationFilename);
            }
            var fileStream = File.Create(implementationPath);
            PopulateImplementationFile(fileStream, settings);
            fileStream.Close();
            ProjectItems projectItems = project.ProjectItems;
            projectItems.AddFromFile(implementationPath);
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
        private static void AddFilter(XDocument doc, XNamespace ns, string path)
        {
            XElement filterItemGroup = GetFilterItemGroup(doc, ns);
            string[] filterTokens = path.Split('\\');
            if(filterTokens.Length == 0 || String.IsNullOrEmpty(filterTokens.First())) {
                return;
            }

            string filterSubPath = "";
            for (int i = 0; i < filterTokens.Length; ++i, filterSubPath += '\\')
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

        private static void ReplaceFileFilter(XDocument doc, XNamespace ns, string fileName, string newFilterPath, string clElement)
        {
            var itemGroupElements = doc.Root.Elements(ns + "ItemGroup");
            var filePath = Path.Combine(newFilterPath, fileName);
            foreach (var fileFilter in itemGroupElements)
            {
              fileFilter.Descendants(ns + clElement).Where(el => (string)el.Attribute("Include") == filePath).Remove();
            }
            XElement newItemGroup = new XElement(ns + "ItemGroup");
            XElement clCompileElement = new XElement(ns + clElement, new XAttribute("Include", filePath));
            newItemGroup.Add(clCompileElement);
            XElement fileFilterElement = new XElement(ns + "Filter");
            fileFilterElement.Add(newFilterPath);
            clCompileElement.Add(fileFilterElement);
            doc.Root.Add(newItemGroup);
        }
        private static void ReplaceHeaderFileFilter(XDocument doc, XNamespace ns, Settings settings, string newFilterPath)
        {
            ReplaceFileFilter(doc, ns, settings.headerFilename, newFilterPath, "ClInclude");
        }
        private static void ReplaceImplementationFileFilter(XDocument doc, XNamespace ns, Settings settings, string newFilterPath)
        {
            ReplaceFileFilter(doc, ns, settings.implementationFilename, newFilterPath, "ClCompile");
        }

        private static void CreateFilters(EnvDTE.Project project, Settings settings)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string filterFilePath = project.FullName + ".filters";
            var doc = OpenFilterXmlDocument(filterFilePath);
            var ns = doc.Root.GetDefaultNamespace();
            AddFilter(doc, ns, settings.RecentImplementationSubfolder());
            AddFilter(doc, ns, settings.RecentHeaderSubfolder());
            ReplaceHeaderFileFilter(doc, ns, settings, settings.RecentHeaderSubfolder());
            if (settings.hasImplementationFile)
            {
                ReplaceImplementationFileFilter(doc, ns, settings, settings.RecentImplementationSubfolder());
            }
            doc.Save(filterFilePath);
        }

        public static void AddClass(Settings settings)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            EnvDTE.Project project = Utils.Solution.CurrentProject(dte);

            string projectPath = new FileInfo(project.FullName).DirectoryName;
            CreateHeaderFile(project, settings, projectPath);
            if (settings.hasImplementationFile)
            {
                CreateImplementationFile(project, settings, projectPath);
            }
            project.DTE.ExecuteCommand("File.SaveAll");

            if (settings.createFilters)
            {
                CreateFilters(project, settings);
            }

            project.DTE.ExecuteCommand("Project.UnloadProject");
            project.DTE.ExecuteCommand("Project.ReloadProject");
        }
    }
}
