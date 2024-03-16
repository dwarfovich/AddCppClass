using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public string headerFileName { get; private set; }
        public string implementationFileName { get; private set; }

        public void GenerateClassData(ClassSettings classSettings)
        {
            var filename = GenerateFilename(classSettings);
            headerFileName = filename + classSettings.HeaderExtension;
            implementationFileName = filename + classSettings.ImplementationExtension;
        }
        public string GenerateCamelCaseFilename(ClassSettings classSettings)
        {
            return classSettings.ClassName;
        }
        public string GenerateSnakeCaseFilename(ClassSettings classSettings)
        {
            string filename = "";
            char previousChar = '\0';
            foreach (char c in classSettings.ClassName)
            {
                if (previousChar == 0)
                {
                    filename += Char.ToLower(c);
                }
                else if (Char.IsLower(previousChar))
                {
                    if (Char.IsUpper(c))
                    {
                        filename += '_';
                    }
                    filename += Char.ToLower(c);
                }
                else if (Char.IsUpper(previousChar))
                {
                    filename += Char.ToLower(c);
                }
                else if (previousChar == '_')
                {
                    filename += Char.ToLower(c);
                }
                else if (Char.IsDigit(previousChar))
                {
                    if (!char.IsDigit(c))
                    {
                        filename += '_';
                    }
                    filename += Char.ToLower(c);
                }
                previousChar = c;
            }
            return filename;
        }
        public string GenerateFilename(ClassSettings classSettings)
        {
            switch (classSettings.Style)
            {
                case FilenameStyle.CamelCase:
                    return GenerateCamelCaseFilename(classSettings);
                case FilenameStyle.SnakeCase:
                    return GenerateSnakeCaseFilename(classSettings);
                default: throw new NotImplementedException();
            }

        }
    }
}
