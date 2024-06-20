using Dwarfovich.AddCppClass.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

    public class StringListSetting
    {
        public int MaxValues { get; set; } = 10;
        public List<string> Values { get; set; } = new List<string> { };
        [JsonIgnore]
        public Func<string, bool> ValidateFunction { get; set; }
        [JsonIgnore]
        public Func<string, string> ConformFunction { get; set; }

        public void SetFrontValue(string value)
        {
            Values.SetFrontValue(value, MaxValues);
        }
        public SettingError Validate()
        {
            Values.ShrinkToSize(MaxValues);
            SettingError errors = new();
            for (int i = Values.Count - 1; i >= 0; i--)
            {
                if (ValidateFunction != null && !ValidateFunction(Values[i]))
                {
                    errors.invalidValues.Add(Values[i]);
                    Values.RemoveAt(i);
                }
                else if (ConformFunction != null)
                {
                    Values[i] = ConformFunction(Values[i]);
                }
            }

            return errors;
        }
    }
    public class Settings
    {
        public StringListSetting _namespaces = new() { ValidateFunction = ClassFacilities.IsValidNamespace };
        public StringListSetting _precompiledHeaders = new() { ValidateFunction = ClassFacilities.IsValidPrecompiledHeaderPath };
        public StringListSetting _headerExtensions = new() { ValidateFunction = ClassFacilities.IsValidExtension, Values = { defaultHeaderExtension, defaultAltHeaderExtension } };
        public StringListSetting _implementationExtensions = new() { ValidateFunction = ClassFacilities.IsValidExtension, Values = { defaultCppExtension } };
        public StringListSetting _headerSubfolders = new() { ValidateFunction = ClassFacilities.IsValidSubfolder, ConformFunction = ClassFacilities.ConformSubfolder };
        public StringListSetting _implementationSubfolders = new() { ValidateFunction = ClassFacilities.IsValidSubfolder, ConformFunction = ClassFacilities.ConformSubfolder };
        public StringListSetting _headerFilters = new();
        public StringListSetting _implementationFilters = new();
        public StringListSetting _dedicatedHeaderSubfolder = new() { ValidateFunction = ClassFacilities.IsValidSubfolder, ConformFunction = ClassFacilities.ConformSubfolder };
        public StringListSetting _dedicatedCppSubfolder = new() { ValidateFunction = ClassFacilities.IsValidSubfolder, ConformFunction = ClassFacilities.ConformSubfolder };

        public static readonly string defaultHeaderExtension = ".h";
        public static readonly string defaultAltHeaderExtension = ".hpp";
        public static readonly string defaultCppExtension = ".cpp";

        public static Regex defaultClassNameRegex { get; } = new(@"^[a-zA-Z_][a-zA-Z_\d]*$");
        public static Regex defaultNamespaceRegex { get; } = new(@"^(([a-zA-Z_][a-zA-Z_\d]*::)*)([a-zA-Z_][a-zA-Z_\d]*)+$");
        public static Regex defaultFileNameRegex { get; } = new(@"^([a-zA-Z_\-\d]*\.)*[a-zA-Z_\-\d]+$");
        public static Regex defaultFileExtensionRegex { get; } = new(@"^(\.?)([a-zA-Z_\d]+\.)*([a-zA-Z_\d]+)$");
        public static Regex defaultSubfolderRegex { get; } = new(@"^([a-zA-Z\-_\d]+)([\\/][a-zA-Z\-_\d]+)*$");
        public static Regex defaultfilterSplitRegex { get; } = new(@"/(?!(/|\z))");

        public Settings() { }
        public Settings(string className, FilenameStyle style, string headerExtension)
        {
            this.className = className;
            filenameStyle = style;
            SetHeaderExtension(headerExtension);
        }

        [JsonProperty]
        public static Regex classNameRegex { get; set; } = defaultClassNameRegex;
        public bool ShouldSerializeclassNameRegex() { return classNameRegex != defaultClassNameRegex; }
        [JsonProperty]
        public static Regex namespaceRegex { get; set; } = defaultNamespaceRegex;
        public bool ShouldSerializenamespaceRegex() { return namespaceRegex != defaultNamespaceRegex; }
        [JsonProperty]
        public static Regex fileNameRegex { get; set; } = defaultFileNameRegex;
        public bool ShouldSerializefileNameRegex() { return fileNameRegex != defaultFileNameRegex; }
        [JsonProperty]
        public static Regex fileExtensionRegex { get; set; } = defaultFileExtensionRegex;
        public bool ShouldSerializefileExtensionRegex() { return fileExtensionRegex != defaultFileExtensionRegex; }
        [JsonProperty]
        public static Regex subfolderRegex { get; set; } = defaultSubfolderRegex;
        public bool ShouldSerializesubfolderRegex() { return subfolderRegex != defaultSubfolderRegex; }
        [JsonProperty]
        public static Regex filterSplitRegex { get; set; } = defaultfilterSplitRegex;
        public bool ShouldSerializefilterRegex() { return filterSplitRegex != defaultfilterSplitRegex; }

        [JsonIgnore]
        public string className { get; set; } = "";
        public int maxRecentNamespaces { get; set; } = 10;
        public List<string> recentNamespaces { get; set; } = new List<string> { };
        [JsonConverter(typeof(StringEnumConverter))]
        public FilenameStyle filenameStyle { get; set; } = FilenameStyle.CamelCase;
        public int maxRecentHeaderExtensions { get; set; } = 10;
        public List<string> recentHeaderExtensions { get; set; } = new List<string>
        { defaultHeaderExtension, defaultAltHeaderExtension };
        [JsonIgnore]
        public string implementationExtension { get { return ".cpp"; } }
        [JsonIgnore]
        public string headerFilename { get; set; } = "";
        [JsonIgnore]
        public string implementationFilename { get; set; } = "";
        public bool hasImplementationFile { get; set; } = true;
        public bool useSingleSubfolder { get; set; } = true;
        public int maxRecentHeaderSubfolders { get; set; } = 10;
        public List<string> recentHeaderSubfolders { get; set; } = new List<string> { };
        public int maxRecentImplementationSubfolders { get; set; } = 10;
        public List<string> recentImplementationSubfolders { get; set; } = new List<string> { };
        public bool autoSaveSettings { get; set; } = true;
        public bool includePrecompiledHeader { get; set; } = false;
        public string precompiledHeader { get; set; } = "pch.h";
        [JsonConverter(typeof(StringEnumConverter))]
        public IncludeGuard includeGuardStyle { get; set; } = IncludeGuard.PragmaOnce;
        public bool createFilters { get; set; } = true;
        public bool useSubfoldersAsFilters { get; set; } = true;
        public bool useSingleFilter { get; set; } = true;
        public int maxRecentHeaderFilters { get; set; } = 10;
        public List<string> recentHeaderFilters { get; set; } = new List<string> { };
        public int maxRecentImplementationFilters { get; set; } = 10;
        public List<string> recentImplementationFilters { get; set; } = new List<string> { };
        public bool storeValuesUnconditiaonally { get; set; } = false;
        public bool ShouldSerializestoreValuesUnconditiaonally() { return storeValuesUnconditiaonally = true; }
        public string Namespace()
        {
            return recentNamespaces.FirstOrValue("");
        }
        public void SetNamespace(string ns)
        {
            recentNamespaces.SetFrontValue(ns, maxRecentNamespaces);
        }
        public string HeaderExtension()
        {
            return recentHeaderExtensions.FirstOrValue(defaultHeaderExtension);
        }
        public void SetHeaderExtension(string extension)
        {
            recentHeaderExtensions.SetFrontValue(extension, maxRecentHeaderExtensions);
        }
        public string HeaderSubfolder()
        {
            return recentHeaderSubfolders.FirstOrValue("");
        }
        public void SetHeaderSubfolder(string subfolder)
        {
            recentHeaderSubfolders.SetFrontValue(subfolder, maxRecentHeaderSubfolders);
        }
        public string ImplementationSubfolder()
        {
            if(storeValuesUnconditiaonally || !useSingleSubfolder)
            {
                return recentImplementationSubfolders.FirstOrValue("");
            }
            else
            {
                return recentHeaderSubfolders.FirstOrValue("");
            }
        }
        public void SetImplementationSubfolder(string subfolder)
        {
            if (storeValuesUnconditiaonally || !useSingleSubfolder)
            {
                recentImplementationSubfolders.SetFrontValue(subfolder, maxRecentImplementationSubfolders);
            }
        }
        public string HeaderFilter()
        {
            if (createFilters)
            {
                if (storeValuesUnconditiaonally)
                {
                    return recentHeaderFilters.FirstOrValue("");
                }
                else
                {
                    return (useSubfoldersAsFilters ? HeaderSubfolder() : recentHeaderFilters.FirstOrValue(""));
                }
            }

            return "";
        }
        public void SetHeaderFilter(string filter)
        {
            if (storeValuesUnconditiaonally || (createFilters && !useSubfoldersAsFilters))
            {
                recentHeaderFilters.SetFrontValue(filter, maxRecentHeaderFilters);
            }
        }
        public string ImplementationFilter()
        {
            if (createFilters)
            {
                if (storeValuesUnconditiaonally)
                {
                    return recentImplementationFilters.FirstOrValue("");
                }
                else
                {
                    if (useSubfoldersAsFilters)
                    {
                        return useSingleFilter ? HeaderSubfolder() : ImplementationSubfolder();
                    }
                    else
                    {
                        return useSingleFilter ? HeaderFilter() : recentImplementationFilters.FirstOrValue("");
                    }
                }
            }

            return "";
        }
        private bool ShouldStoreImplementationFilter()
        {
            return storeValuesUnconditiaonally || (createFilters && !useSubfoldersAsFilters && !useSingleFilter);
        }
        public void SetImplementationFilter(string filter)
        {
            if (ShouldStoreImplementationFilter())
            {
                recentImplementationFilters.SetFrontValue(filter, maxRecentImplementationFilters);
            }
        }
    }
}
