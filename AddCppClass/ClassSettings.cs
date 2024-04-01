using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string ClassName { get; set; } = "";
        public FilenameStyle Style { get; set; } = FilenameStyle.CamelCase;
        public string HeaderExtension { get; set; } = "";
        public string ImplementationExtension { get { return ".cpp"; } }
        public string headerFilename { get; set; } = "";
        public string implementationFilename { get; set; } = "";
        public string headerSubfolder { get; set; } = "";
        public string implementationSubfolder { get; set; } = "";
        public bool useSingleSubfolder { get; set; } = true;
        public bool hasImplementationFile { get; set; } = true;
        public bool createFilters { get; set; } = true;

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
        
    }
}
