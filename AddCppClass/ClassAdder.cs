using AddCppClass;
using EnvDTE;
using EnvDTE80;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

using EnvDTE80;

using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.Shell.Interop;
using System.Web.UI.Design;
using System.Windows.Controls;

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
        public static void AddClass(ClassGenerator generator, string subFolder)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            EnvDTE.Project project = CurrentProject();
            string projectPath = new FileInfo(project.FullName).DirectoryName;
            string classPath = String.IsNullOrEmpty(subFolder) ? projectPath + '\\' : projectPath + '\\' + subFolder + '\\';
            Directory.CreateDirectory(classPath);

            ProjectItems projectItems = project.ProjectItems;
            //string path = folder + generator.headerFilename;
            //string headerPath = folder + generator.headerFilename;
            //projectItems.AddFromTemplate(classPath, generator.headerFilename);

            projectItems.AddFromFile(classPath + generator.headerFilename);
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
