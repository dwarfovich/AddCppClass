using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace Dwarfovich.AddCppClass
{
    public enum FilenameStyle
    {
        CamelCase,
        SnakeCase,
        LowerCase
    }

    public class ClassSettings
    {
        public ClassSettings()
        {
            ClassName = "";
            Style = FilenameStyle.CamelCase;
            HeaderExtension = "";
        }
        public ClassSettings(string className, FilenameStyle style, string headerExtension)
        {
            ClassName = className;
            Style = style;
            HeaderExtension = headerExtension;
        }
        public string ClassName { get; set; }
        public FilenameStyle Style { get; set; }
        public string HeaderExtension { get; set; }
        public string ImplementationExtension { get { return ".cpp"; } }
    }
    public class ClassGenerator
    {
        public string headerFilename { get; private set; } = "";
        public string implementationFilename { get; private set; } = "";
        public string filename { get; private set; } = "";
        public string headerSubfolder { get; set; } = "";
        public string implementationSubfolder { get; set; } = "";
        public bool useSingleSubfolder { get; set; } = true;
        public bool hasImplementationFile { get; set; } = true;
        public bool createFilters { get; set; } = true;

        public ClassGenerator()
        {
        }
        public void GenerateClassData(ClassSettings classSettings)
        {
            filename = GenerateFilename(classSettings);
            GenerateFilenames(classSettings);
        }

        public void GenerateFilenames(ClassSettings classSettings)
        {
            headerFilename = filename + classSettings.HeaderExtension;
            implementationFilename = filename + classSettings.ImplementationExtension;
        }
        public string GenerateCamelCaseFilename(ClassSettings classSettings)
        {
            return classSettings.ClassName;
        }

        public string GenerateSnakeCaseFilename(ClassSettings classSettings)
        {
            Func<char, char, bool> ShouldInsertUnderline = (char previousChar, char nextChar) =>
            {
                return (Char.IsLower(previousChar) && Char.IsUpper(nextChar))
                || (Char.IsDigit(previousChar) && !Char.IsDigit(nextChar));
            };

            string filename = "";
            char previousChar = '\0';
            foreach (char c in classSettings.ClassName)
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
        public string GenerateLowerCaseFilename(ClassSettings classSettings)
        {
            return classSettings.ClassName.ToLower();
        }
        public string GenerateFilename(ClassSettings classSettings)
        {
            switch (classSettings.Style)
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
    }
}
