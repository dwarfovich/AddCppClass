using System.Text.RegularExpressions;
using System.IO;
using Dwarfovich.AddCppClass;

namespace Dwarfovich.AddCppClass
{
    public class ClassGenerator
    {
        private string filename = "";
        private static readonly Regex fileNameRegex = new(@"^(::)?([a-zA-Z_][a-zA-Z\d_]*::)*([a-zA-Z_][a-zA-Z\d_]*)$");
        public ClassGenerator()
        {
        }
        public (string header, string implementation) GenerateFilenamesForChangedExtension(Settings classSettings)
        {
            return (filename + classSettings.headerExtension, filename + classSettings.implementationExtension);
        }

        public (string header, string implementation) GenerateFilenames(Settings classSettings)
        {
            filename = GenerateFilename(classSettings);

            return (filename + classSettings.headerExtension, filename + classSettings.implementationExtension);
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
        public static bool IsValidClassName(string name)
        {
            return fileNameRegex.IsMatch(name);
        }

        public static bool IsValidSubfolder(string subfolder)
        {
            if (String.IsNullOrEmpty(subfolder))
            {
                return true;
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
            return filename.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }
    }
}
