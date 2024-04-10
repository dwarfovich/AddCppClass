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
        public string className { get; set; } = "";
        public FilenameStyle filenameStyle { get; set; } = FilenameStyle.CamelCase;
        public string headerExtension { get; set; } = "";
        public string implementationExtension { get { return ".cpp"; } }
        public string headerFilename { get; set; } = "";
        public string implementationFilename { get; set; } = "";
        public string headerSubfolder { get; set; } = "";
        public string implementationSubfolder { get; set; } = "";
        public bool useSingleSubfolder { get; set; } = true;
        public bool hasImplementationFile { get; set; } = true;
        public bool createFilters { get; set; } = true;
        public int recentHeaderSubfoldersCount { get; set; } = 10;
        public string[] recentHeaderSubfolders { get; set; } = [];
        public int recentImplementationSubfoldersCount { get; set; } = 10;
        public string[] recentImplementationSubfolders { get; set; } = [];
        public bool autoSaveSettings { get; set; } = true;
        //public bool includePrecompiledHeader { get; set; } = false;
        //public string precompiledHeader { get; set; } = ".pch";
        public Settings() { }
    }
}
