using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Security;
using System;

namespace Dwarfovich.AddCppClass
{
    public class ClassFacilities
    {
        private string filename = "";
        private static readonly Regex namespaceRegex = new(@"^(::)?([a-zA-Z_][a-zA-Z_\d]*::)*([a-zA-Z_][a-zA-Z_\d]*)$");
        private static readonly Regex fileNameRegex = new(@"([a-zA-Z_\-\d]*.)*([a-zA-Z_\-\d])$");
        private static readonly Regex fileExtensionRegex = new(@"^(\.?)([a-zA-Z_\d]+\.)*([a-zA-Z_\d]+)$");

        public ClassFacilities()
        {
        }
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

            if (!IsLatinLetter(name.First()) && name.First() != '_')
            {
                return false;
            }

            for (int i = 1; i < name.Length; i++)
            {
                if (!IsLatinLetter(name[i]) && !Char.IsDigit(name[i]) && name[i] != '_')
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidNamespace(string ns)
        {
            if (ns is null)
            {
                return false;
            }

            if (ns == "::")
            {
                return false;
            }

            if (ns.Length == 0)
            {
                return true;
            }

            return namespaceRegex.IsMatch(ns);
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
            if (String.IsNullOrWhiteSpace(subfolder))
            {
                return false;
            }

            if (subfolder.Any(Char.IsWhiteSpace))
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
            return fileNameRegex.IsMatch(filename);
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
            if (path.Any(Char.IsWhiteSpace))
            {
                return false;
            }

            if (path== string.Empty)
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
            return fileExtensionRegex.IsMatch(extension);
        }
    }
}
