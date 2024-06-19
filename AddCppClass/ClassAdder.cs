using AddCppClass;
using Dwarfovich.AddCppClass.Utils;
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
            if (File.Exists(filterFilePath))
            {
                return XDocument.Load(filterFilePath);
            }
            else
            {
                return CreateFilterXmlDocument();
            }
        }

        private static string GenerateIfndefGuardFilenameToken(Settings settings)
        {
            return settings.headerFilename.ToUpper().Replace('.', '_');
        }
        private static void PopulateHeaderFile(FileStream fileStream, Settings settings)
        {
            using (var writer = new StreamWriter(fileStream))
            {
                string ifndefGuardFilenameToken = "";
                if (settings.includeGuardStyle == IncludeGuard.PragmaOnce)
                {
                    writer.WriteLine("#pragma once");
                }
                else
                {
                    ifndefGuardFilenameToken = GenerateIfndefGuardFilenameToken(settings);
                    writer.WriteLine("#ifndef " + ifndefGuardFilenameToken);
                    writer.WriteLine("#define " + ifndefGuardFilenameToken);
                }
                writer.Write(Environment.NewLine);

                string[] tokens = ClassFacilities.TokenizeNamespace(settings.Namespace());
                foreach (string token in tokens)
                {
                    writer.WriteLine("namespace " + token + " {");
                }
                if (tokens.Length > 0)
                {
                    writer.Write(Environment.NewLine);
                }

                writer.WriteLine("class " + settings.className + " {");
                writer.WriteLine("public:");
                writer.WriteLine("private:");
                writer.WriteLine("};");

                if (tokens.Length > 0)
                {
                    writer.Write(Environment.NewLine);
                }
                for (int i = tokens.Count() - 1; i >= 0; --i)
                {
                    writer.WriteLine("} // namespace " + tokens[i]);
                }
                if (settings.includeGuardStyle == IncludeGuard.Ifndef)
                {
                    writer.Write(Environment.NewLine);
                    writer.WriteLine("#endif // " + ifndefGuardFilenameToken);
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
                if (String.IsNullOrEmpty(settings.HeaderSubfolder()))
                {
                    writer.WriteLine("#include \"" + settings.headerFilename + '\"');
                }
                else
                {
                    writer.WriteLine("#include \"" + settings.HeaderSubfolder() + '/' + settings.headerFilename + '\"');
                }
            }
        }

        public static string CreateHeaderFile(EnvDTE.Project project, Settings settings, string projectPath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string path = System.IO.Path.Combine(projectPath, settings.HeaderSubfolder());
            Directory.CreateDirectory(path);
            path = System.IO.Path.Combine(path, settings.headerFilename);
            var fileStream = File.Create(path);
            PopulateHeaderFile(fileStream, settings);
            fileStream.Close();
            ProjectItems projectItems = project.ProjectItems;
            projectItems.AddFromFile(path);

            return path;
        }

        public static void CreateImplementationFile(EnvDTE.Project project, Settings settings, string projectPath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string path = settings.useSingleSubfolder ? System.IO.Path.Combine(projectPath, settings.HeaderSubfolder())
                                                      : System.IO.Path.Combine(projectPath, settings.ImplementationSubfolder());
            Directory.CreateDirectory(path);
            path = System.IO.Path.Combine(path, settings.implementationFilename);
            var fileStream = File.Create(path);
            PopulateImplementationFile(fileStream, settings);
            fileStream.Close();
            ProjectItems projectItems = project.ProjectItems;
            projectItems.AddFromFile(path);
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
        private static void AddFilterPathItem(XDocument doc, XNamespace ns, string path)
        {
            XElement filterItemGroup = GetFilterItemGroup(doc, ns);
            string[] filterTokens = path.Split('\\');
            if (filterTokens.Length == 0 || String.IsNullOrEmpty(filterTokens.First()))
            {
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
            var filePath = System.IO.Path.Combine(newFilterPath, fileName);
            XElement itemGroupWithClElement = null;
            foreach (var itemGroupElement in itemGroupElements)
            {
                var descendants = itemGroupElement.Descendants(ns + clElement);
                if (descendants.Any())
                {
                    if (itemGroupWithClElement == null)
                    {
                        itemGroupWithClElement = itemGroupElement;
                    }
                }
                foreach (var descendant in descendants)
                {
                    if ((string)descendant.Attribute("Include") == filePath)
                    {
                        descendant.Remove();
                        break;
                    }
                }
            }
            XElement itemGroup;
            if (itemGroupWithClElement == null)
            {
                itemGroup = new XElement(ns + "ItemGroup");
                doc.Root.Add(itemGroup);
            }
            else
            {
                itemGroup = itemGroupWithClElement;
            }
            XElement clCompileElement = new XElement(ns + clElement, new XAttribute("Include", filePath));
            itemGroup.Add(clCompileElement);
            XElement fileFilterElement = new XElement(ns + "Filter");
            fileFilterElement.Add(newFilterPath);
            clCompileElement.Add(fileFilterElement);
        }
        private static void ReplaceFilterPathForHeaderFile(XDocument doc, XNamespace ns, Settings settings, string newFilterPath)
        {
            ReplaceFileFilter(doc, ns, settings.headerFilename, newFilterPath, "ClInclude");
        }
        private static void ReplaceFilterPathForImplementationFile(XDocument doc, XNamespace ns, Settings settings, string newFilterPath)
        {
            ReplaceFileFilter(doc, ns, settings.implementationFilename, newFilterPath, "ClCompile");
        }

        private static void CreateFilters(EnvDTE.Project project, Settings settings)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string filterFilePath = project.FullName + ".filters";
            var doc = OpenFilterXmlDocument(filterFilePath);
            var ns = doc.Root.GetDefaultNamespace();
            string headerSubfolder = Utils.Path.ToWindowsStylePath(settings.HeaderSubfolder());
            if (!string.IsNullOrEmpty(headerSubfolder))
            {
                AddFilterPathItem(doc, ns, headerSubfolder);
                ReplaceFilterPathForHeaderFile(doc, ns, settings, headerSubfolder);
            }

            if (settings.hasImplementationFile)
            {
                string implementationSubfolder = settings.useSingleSubfolder
                                           ? headerSubfolder
                                           : Utils.Path.ToWindowsStylePath(settings.ImplementationSubfolder());
                if (!string.IsNullOrEmpty(implementationSubfolder))
                {
                    if (!settings.useSingleSubfolder)
                    {
                        AddFilterPathItem(doc, ns, implementationSubfolder);
                    }
                    ReplaceFilterPathForImplementationFile(doc, ns, settings, implementationSubfolder);
                }
            }

            doc.Save(filterFilePath);
        }

        public static void AddClass(Settings settings)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            EnvDTE.Project project = Utils.Solution.CurrentProject(dte);

            string projectPath = new FileInfo(project.FullName).DirectoryName;
            string headerPath = CreateHeaderFile(project, settings, projectPath);
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
            project.DTE.ItemOperations.OpenFile(headerPath);
        }
    }
}
