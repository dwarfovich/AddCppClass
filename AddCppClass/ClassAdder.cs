using AddCppClass;
using Community.VisualStudio.Toolkit;
using Dwarfovich.AddCppClass.Utils;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.VCProjectEngine;
using System.Collections.Generic;
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
            //ProjectItems projectItems = project.ProjectItems;
            //projectItems.AddFromFile(path);

            return path;
        }

        public static string CreateImplementationFile(EnvDTE.Project project, Settings settings, string projectPath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string path = settings.useSingleSubfolder ? System.IO.Path.Combine(projectPath, settings.HeaderSubfolder())
                                                      : System.IO.Path.Combine(projectPath, settings.ImplementationSubfolder());
            Directory.CreateDirectory(path);
            path = System.IO.Path.Combine(path, settings.implementationFilename);
            var fileStream = File.Create(path);
            PopulateImplementationFile(fileStream, settings);
            fileStream.Close();
            //ProjectItems projectItems = project.ProjectItems;
            //projectItems.AddFromFile(path);

            return path;
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

        public static VCFilter EnsureFilter(EnvDTE.Project project, Settings settings)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var vcProject = project.Object as VCProject;
            var filters = vcProject.Filters as System.Collections.IEnumerable;

            foreach (VCFilter filter in filters)
            {
                if (filter.Name == "Filter3")
                {
                    return filter.AddFilter(@"new filter") as VCFilter;
                }
            }
            return null;
        }

        public static (string, string) CreateClassFiles(EnvDTE.Project project, Settings settings)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string projectPath = new FileInfo(project.FullName).DirectoryName;
            string headerPath = CreateHeaderFile(project, settings, projectPath);
            string cppPath = "";
            if (settings.hasImplementationFile)
            {
                cppPath = CreateImplementationFile(project, settings, projectPath);
            }

            return (headerPath, cppPath);
        }

        public static void AddFileToFilter(EnvDTE.Project project, string filePath, string filter)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            VCProject vcProject = project.Object as VCProject;
            if (vcProject == null)
            {
                throw new Exception("Failed to convert EnvDTE.Project to VCProject");
            }

            filter = "Filter1";
            var tokens = ClassFacilities.TokenizeFilter(filter);
            if (tokens.Length == 0)
            {
                return;
            }
            //VCFilter targetFilter;
            //var filters = vcProject.Filters as System.Collections.IEnumerable;
            //for (int i = 0; i < tokens.Length; i++)
            //{
            //    targetFilter = filters.Cast<VCFilter>().FirstOrDefault(x => x.Name == tokens[i]);
            //    if (targetFilter == null)
            //    {
            //        break;
            //    }
            //}

            //VCProject p = Utils.Solution.CurrentProject().Object as VCProject;
            //string str = @"`~!@#$%^&*()<>,.{}[]:""";
            //var filters = p.Filters as System.Collections.IEnumerable;
            //string m = "";
            //foreach (VCFilter f in filters.Cast<VCFilter>())
            //{
            //    if (f.Name == str)
            //    {
            //        //VS.MessageBox.Show("Warning", "Found! " + str, OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK);
            //    }
            //    m += f.CanonicalName + "\n";
            //}
            //VS.MessageBox.Show("Warning", m, OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK);
            return;
        }
        public static void AddClass(Settings settings)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            var project = Utils.Solution.CurrentProject();
            (var headerPath, var cppPath) = CreateClassFiles(project, settings);
            var exceptions = new List<Exception>();
            try
            {
                AddFileToFilter(project, headerPath, settings.HeaderFilter());
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
            finally
            {
                try
                {
                    AddFileToFilter(project, cppPath, settings.ImplementationFilter());
                }
                catch (Exception ex2)
                {
                    exceptions.Add(ex2);
                }
                finally
                {
                    if (exceptions.Count > 0)
                    {
                        throw new AggregateException(exceptions);
                    }
                }
            }

            //project.DTE.ExecuteCommand("File.SaveAll");

            //if (settings.createFilters)
            //{
            //    CreateFilters(project, settings);
            //}

            //project.DTE.ExecuteCommand("Project.UnloadProject");
            //project.DTE.ExecuteCommand("Project.ReloadProject");
            //project.DTE.ItemOperations.OpenFile(headerPath);
        }
    }
}
