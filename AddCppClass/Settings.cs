using Newtonsoft.Json;

namespace Dwarfovich.AddCppClass
{
    public enum FilenameStyle
    {
        CamelCase,
        SnakeCase,
        LowerCase
    };
    public class Settings
    {
        [JsonIgnore]
        public string className { get; set; } = "";
        public FilenameStyle filenameStyle { get; set; } = FilenameStyle.CamelCase;
        public string[] recentHeaderExtensions { get; set; } = [".h", ".hpp"];
        [JsonIgnore]
        public string implementationExtension { get { return ".cpp"; } }
        [JsonIgnore]
        public string headerFilename { get; set; } = "";
        [JsonIgnore]
        public string implementationFilename { get; set; } = "";
        [JsonIgnore]
        public string headerSubfolder { get; set; } = "";
        [JsonIgnore]
        public string implementationSubfolder { get; set; } = "";
        public bool useSingleSubfolder { get; set; } = true;
        public bool hasImplementationFile { get; set; } = true;
        public bool createFilters { get; set; } = true;
        public int recentHeaderSubfoldersCount { get; set; } = 10;
        public string[] recentHeaderSubfolders { get; set; } = [];
        public int recentImplementationSubfoldersCount { get; set; } = 10;
        public string[] recentImplementationSubfolders { get; set; } = [];
        public bool autoSaveSettings { get; set; } = true;
        public bool includePrecompiledHeader { get; set; } = false;
        public string precompiledHeader { get; set; } = ".pch";
        public Settings() { }
    }
}
