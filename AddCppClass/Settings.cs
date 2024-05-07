using Dwarfovich.AddCppClass.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

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
        public int recentNamespacesCount = 10;
        public List<string> recentNamespaces { get; set; } = new List<string> { };

        private string _lastUsedNamespace { get; set; } = "";

        public string lastUsedNamespace {
            get { return _lastUsedNamespace; }
            set {
                _lastUsedNamespace = value;
                lastUsedNamespaceTokenized = ClassUtils.TokenizeNamespace(value);
                AddNamespaceToRecents(value);
            }
        }
        [JsonIgnore]
        public string[] lastUsedNamespaceTokenized {  get; private set; } = [];
        public FilenameStyle filenameStyle { get; set; } = FilenameStyle.CamelCase;
        public List<string> recentHeaderExtensions { get; set; } = new List<string> { ".h", ".hpp" };
        public string lastUsedHeaderExtension = "";
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
        public List<string> recentHeaderSubfolders { get; set; } = new List<string> { };
        public int recentImplementationSubfoldersCount { get; set; } = 10;
        public List<string> recentImplementationSubfolders { get; set; } = new List<string> { };
        public bool autoSaveSettings { get; set; } = true;
        public bool includePrecompiledHeader { get; set; } = false;
        public string precompiledHeader { get; set; } = ".pch";
        public Settings() { }

        public string HeaderExtension()
        {
            if (!string.IsNullOrEmpty(lastUsedHeaderExtension))
            {
                return lastUsedHeaderExtension;
            }

            if(recentHeaderExtensions.Count > 0)
            {
                return recentHeaderExtensions.First();
            }
            else
            {
                return ".h";
            }
        }

        private void AddNamespaceToRecents(string ns)
        {
            var index = recentNamespaces.IndexOf(ns);
            if (index == -1)
            {
                recentNamespaces.Prepend(ns);
                if(recentNamespaces.Count > recentNamespacesCount)
                {
                    recentNamespaces.RemoveAt(recentNamespaces.Count - 1);
                }
            }
            else
            {
                recentNamespaces.MoveItemAtIndexToFront(index);
            }
        }
    }
}
