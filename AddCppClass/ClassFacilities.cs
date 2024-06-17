using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using AddCppClass;

namespace Dwarfovich.AddCppClass
{
    public class SettingError
    {
        public string settingName = "";
        public List<string> invalidValues = new();
    }
    public class ClassFacilities
    {
        private string filename = "";

        public (string header, string implementation) GenerateFilenamesForChangedExtension(Settings classSettings)
        {
            return (filename + classSettings.RecentHeaderExtension(), filename + classSettings.implementationExtension);
        }

        public (string header, string implementation) GenerateFilenames(Settings classSettings)
        {
            filename = GenerateFilename(classSettings);

            return (filename + classSettings.RecentHeaderExtension(), filename + classSettings.implementationExtension);
        }
        public string GenerateCamelCaseFilename(Settings classSettings)
        {
            return classSettings.className;
        }

        public string GenerateSnakeCaseFilename(Settings classSettings)
        {
            Func<char, char, bool> ShouldInsertUnderline = (char previousChar, char nextChar) =>
            {
                return (Char.IsLower(previousChar) && Char.IsUpper(nextChar))
                || (Char.IsDigit(previousChar) && !Char.IsDigit(nextChar));
            };

            string filename = "";
            char previousChar = '\0';
            foreach (char c in classSettings.className)
            {
                if (ShouldInsertUnderline(previousChar, c))
                {
                    filename += '_';
                }
                filename += Char.ToLower(c);
                previousChar = c;
            }
            return filename;
        }
        public string GenerateLowerCaseFilename(Settings classSettings)
        {
            return classSettings.className.ToLower();
        }
        public string GenerateFilename(Settings classSettings)
        {
            switch (classSettings.filenameStyle)
            {
                case FilenameStyle.CamelCase:
                    return GenerateCamelCaseFilename(classSettings);
                case FilenameStyle.SnakeCase:
                    return GenerateSnakeCaseFilename(classSettings);
                case FilenameStyle.LowerCase:
                    return GenerateLowerCaseFilename(classSettings);
                default: throw new NotImplementedException();
            }

        }

        public static string[] TokenizeNamespace(string ns)
        {
            if (String.IsNullOrEmpty(ns))
            {
                return [];
            }
            else
            {
                return ns.Split(new[] { "::" }, StringSplitOptions.None);
            }
        }

        static public bool ClassExists(string ns, string className)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            EnvDTE.Project project = Utils.Solution.CurrentProject(AddCppClassPackage.dte);
            string fullName = string.IsNullOrEmpty(ns) ? className :  ns + "::" + className;
            var ce = project.CodeModel.CodeTypeFromFullName(fullName);

            return ce != null;
        }
        static private void ConformStringList(ref Settings settings, string settingName, Func<string, bool> validateFunction, ref List<SettingError> errors)
        {
            SettingError settingErrors = new();
            settingErrors.settingName = settingName;
            var list = settings.GetType().GetProperty(settingName).GetValue(settings) as List<string>;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (!validateFunction(list[i]))
                {
                    settingErrors.invalidValues.Add(list[i]);
                    list.RemoveAt(i);
                }
            }

            settings.GetType().GetProperty(settingName).SetValue(settings, list);
            if (settingErrors.invalidValues.Count > 0)
            {
                errors.Add(settingErrors);
            }
        }

        static private void ConformPrecompiledHeaderPath(ref Settings settings, string settingName, Func<string, bool> validateFunction, ref List<SettingError> errors)
        {
            SettingError settingErrors = new();
            settingErrors.settingName = settingName;
            var path = settings.GetType().GetProperty(settingName).GetValue(settings) as string;
            if (!validateFunction(path))
            {
                settingErrors.invalidValues.Add(path);
                settings.GetType().GetProperty(settingName).SetValue(settings, "pch.h");
            }
            if (settingErrors.invalidValues.Count > 0 && settings.includePrecompiledHeader)
            {
                errors.Add(settingErrors);
            }
        }

        static public List<SettingError> ConformSettings(ref Settings settings)
        {
            List<SettingError> errors = new();

            ConformStringList(ref settings, nameof(Settings.recentNamespaces), IsValidNamespace, ref errors);
            ConformStringList(ref settings, nameof(Settings.recentHeaderExtensions), IsValidHeaderExtension, ref errors);
            ConformStringList(ref settings, nameof(Settings.recentHeaderSubfolders), IsValidSubfolder, ref errors);
            ConformStringList(ref settings, nameof(Settings.recentImplementationSubfolders), IsValidSubfolder, ref errors);
            ConformPrecompiledHeaderPath(ref settings, nameof(Settings.precompiledHeader), IsValidPrecompiledHeaderPath, ref errors);

            return errors;
        }
        public static bool IsLatinLetter(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }
        public static bool IsValidClassName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return Settings.classNameRegex.IsMatch(name);
        }

        public static bool IsValidNamespace(string ns)
        {
            if (string.IsNullOrEmpty(ns))
            {
                return true;
            }

            return Settings.namespaceRegex.IsMatch(ns);
        }

        public static string ConformSubfolder(string subfolder)
        {
            if (String.IsNullOrEmpty(subfolder))
            {
                return "";
            }

            if (subfolder.EndsWith("\\") || subfolder.EndsWith("/"))
            {
                subfolder = subfolder.Remove(subfolder.Length - 1);
            }
            if (subfolder.StartsWith("\\") || subfolder.StartsWith("/"))
            {
                subfolder = subfolder.Remove(0, 1);
            }

            subfolder = subfolder.Replace("/", "\\");

            return subfolder;
        }

        public static bool IsValidSubfolder(string subfolder)
        {
            if (String.IsNullOrEmpty(subfolder))
            {
                return true;
            }

            if (!Settings.subfolderRegex.IsMatch(subfolder))
            {
                return false;
            }

            try
            {
                string testPath = Path.Combine(Environment.CurrentDirectory, subfolder);
                string fullPath = Path.GetFullPath(subfolder);
                return fullPath == testPath;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidFilename(string filename)
        {
            return Settings.fileNameRegex.IsMatch(filename);
        }

        public static string ConformPrecompiledHeaderPath(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return "";
            }

            if (path.StartsWith("\\") || path.StartsWith("/"))
            {
                path = path.Remove(0, 1);
            }

            path = path.Replace("\\", "/");
            var pos = path.LastIndexOf('/');
            if (pos == -1)
            {
                return path;
            }
            else
            {
                return ConformSubfolder(path.Substring(0, pos)) + '/' + path.Substring(pos + 1);
            }
        }

        public static bool IsValidPrecompiledHeaderPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return true;
            }

            var separatorPos = path.LastIndexOf(Path.DirectorySeparatorChar);
            var slashPos = path.LastIndexOf('/');
            var pos = Math.Max(separatorPos, slashPos);
            if (pos == -1)
            {
                return IsValidFilename(path);
            }
            else
            {
                return IsValidSubfolder(ConformSubfolder(path.Substring(0, pos)))
                    && IsValidFilename(path.Substring(pos + 1));
            }
        }
        public static bool IsValidHeaderExtension(string extension)
        {
            return Settings.fileExtensionRegex.IsMatch(extension);
        }
    }
}
