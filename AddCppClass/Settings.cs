using Dwarfovich.AddCppClass.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;

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
        public int maxRecentNamespaces = 10;
        public List<string> recentNamespaces { get; private set; }  = new List<string> { };
        [JsonIgnore]
        private string[] _mostRecentNamespaceTokenized = [];
        [JsonIgnore]
        public string[] mostRecentNamespaceTokenized
        {
            get
            {
                if (_mostRecentNamespaceTokenized == null || _mostRecentNamespaceTokenized.Length == 0)
                {
                    if (recentNamespaces == null || recentNamespaces.Count == 0)
                    {
                        return [];
                    }
                    else
                    {
                        _mostRecentNamespaceTokenized = ClassUtils.TokenizeNamespace(recentNamespaces.First());
                    }
                }

                return _mostRecentNamespaceTokenized;
            }
            private set
            {
                _mostRecentNamespaceTokenized = value;
            }
        }
        public FilenameStyle filenameStyle { get; set; } = FilenameStyle.CamelCase;
        public int maxRecentHeaderExtensions = 10;
        [JsonProperty]
        public List<string> recentHeaderExtensions = new List<string> { ".h", ".hpp" };
        [JsonIgnore]
        public string implementationExtension { get { return ".cpp"; } }
        [JsonIgnore]
        public string headerFilename { get; set; } = "";
        [JsonIgnore]
        public string implementationFilename { get; set; } = "";
        public bool useSingleSubfolder { get; set; } = true;
        public bool hasImplementationFile { get; set; } = true;
        public bool createFilters { get; set; } = true;
        public int maxRecentHeaderSubfolders { get; set; } = 10;
        [JsonProperty]
        public List<string> recentHeaderSubfolders = new List<string> {};
        public int maxRecentImplementationSubfolders { get; set; } = 10;
        [JsonProperty]
        public List<string> recentImplementationSubfolders = new List<string> { };
        public bool autoSaveSettings { get; set; } = true;
        public bool includePrecompiledHeader { get; set; } = false;
        public string precompiledHeader { get; set; } = "pch.h";
        public Settings() { }
        public Settings(string className, FilenameStyle style, string headerExtension) {
            this.className = className;
            filenameStyle = style;
            AddMostRecentHeaderExtension(headerExtension);
        }

        public string RecentHeaderExtension()
        {
            return recentHeaderExtensions.FirstOrValue(".h");
        }
        public string RecentHeaderSubfolder()
        {
            return recentHeaderSubfolders.FirstOrValue("");
        }
        public string RecentImplementationSubfolder()
        {
            return recentImplementationSubfolders.FirstOrValue("");
        }
        public void AddMostRecentNamespace(string ns)
        {
            recentNamespaces.AddFrontValue(ns, maxRecentNamespaces);
            mostRecentNamespaceTokenized = ClassUtils.TokenizeNamespace(ns);
        }

        public void AddMostRecentHeaderExtension(string extension)
        {
            recentHeaderExtensions.AddFrontValue(extension, maxRecentHeaderExtensions);
        }
        public void AddMostRecentHeaderSubfolder(string subfolder)
        {
            recentHeaderSubfolders.AddFrontValue(subfolder, maxRecentHeaderSubfolders);
        }
        public void AddMostRecentImplementationSubfolder(string subfolder)
        {
            recentImplementationSubfolders.AddFrontValue(subfolder, maxRecentImplementationSubfolders);
        }
    }
}
