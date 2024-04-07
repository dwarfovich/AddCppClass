using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dwarfovich.AddCppClass
{
    public class ExtensionSettings
    {
        public FilenameStyle filenameStyle { get; set; } = FilenameStyle.CamelCase;
        public string headerExtension { get; set; } = ".h";
        public bool useSingleSubfolder { get; set; } = true;
        public bool createFilters {  get; set; } = true;
        public bool dontCreateImplementationFile { get; set; } = false;
        public int recentHeaderSubfoldersCount {  get; set; } = 10;
        public string[] recentHeaderSubfolders { get; set; } = [];
        public int recentImplementationSubfoldersCount { get; set; } = 10;
        public string[] recentImplementationSubfolders { get; set; } = [];
        public bool autoSaveSettings { get; set; } = true;

        public ExtensionSettings() { }
    }
}
