﻿using Dwarfovich.AddCppClass.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Windows;

namespace Dwarfovich.AddCppClass
{
    public enum FilenameStyle
    {
        CamelCase,
        SnakeCase,
        LowerCase
    };

    public enum IncludeGuard
    {
        PragmaOnce,
        Ifndef
    };

    public class Settings
    {
        public static readonly Regex defaultClassNameRegex = new(@"^[a-zA-Z_][a-zA-Z_\d]*$");
        public static readonly Regex defaultNamespaceRegex = new(@"^(([a-zA-Z_][a-zA-Z_\d]*::)*)([a-zA-Z_][a-zA-Z_\d]*)+$");
        public static readonly Regex defaultFileNameRegex = new(@"^([a-zA-Z_\-\d]*\.)*[a-zA-Z_\-\d]+$");
        public static readonly Regex defaultHeaderExtensionRegex = new(@"^(\.?)([a-zA-Z_\d]+\.)*([a-zA-Z_\d]+)$");
        public static readonly Regex defaultSubfolderRegex = new(@"^([\\/]?)([a-zA-Z\-_\d]+[\\/]?)+$");

        [JsonProperty]
        public static Regex classNameRegex { get; set; } = defaultClassNameRegex;
        public bool ShouldSerializeclassNamespaceRegex() { return classNameRegex != defaultClassNameRegex; }
        [JsonProperty]
        public static Regex namespaceRegex { get; set; } = defaultNamespaceRegex;
        public bool ShouldSerializenamespaceRegex() { return namespaceRegex != defaultNamespaceRegex; }
        [JsonProperty]
        public static Regex fileNameRegex { get; set; } = defaultFileNameRegex;
        public bool ShouldSerializefileNameRegex() { return fileNameRegex != defaultFileNameRegex; }
        [JsonProperty]
        public static Regex fileExtensionRegex { get; set; } = defaultHeaderExtensionRegex;
        public bool ShouldSerializefileExtensionRegex() { return fileExtensionRegex != defaultHeaderExtensionRegex; }
        [JsonProperty]
        public static Regex subfolderRegex { get; set; } = defaultSubfolderRegex;
        public bool ShouldSerializesubfolderRegex() { return subfolderRegex != defaultSubfolderRegex; }

        public string className { get; set; } = "";
        public int maxRecentNamespaces = 10;
        public List<string> recentNamespaces { get; set; } = new List<string> { };
        [JsonConverter(typeof(StringEnumConverter))]
        public FilenameStyle filenameStyle { get; set; } = FilenameStyle.CamelCase;
        public int maxRecentHeaderExtensions = 10;
        [JsonProperty]
        public List<string> recentHeaderExtensions { get; set; } = new List<string> { ".h", ".hpp" };
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
        //[JsonProperty]
        public List<string> recentHeaderSubfolders { get; set; } = new List<string> { };
        public int maxRecentImplementationSubfolders { get; set; } = 10;
        //[JsonProperty]
        public List<string> recentImplementationSubfolders { get; set; } = new List<string> { };
        public bool autoSaveSettings { get; set; } = true;
        public bool includePrecompiledHeader { get; set; } = false;
        public string precompiledHeader { get; set; } = "pch.h";
        [JsonConverter(typeof(StringEnumConverter))]
        public IncludeGuard includeGuardStyle { get; set; } = IncludeGuard.PragmaOnce;
        public Settings() { }
        public Settings(string className, FilenameStyle style, string headerExtension)
        {
            this.className = className;
            filenameStyle = style;
            AddMostRecentHeaderExtension(headerExtension);
        }

        public string RecentNamespace()
        {
            return recentNamespaces.FirstOrValue("");
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
            //mostRecentNamespaceTokenized = ClassUtils.TokenizeNamespace(ns);
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
