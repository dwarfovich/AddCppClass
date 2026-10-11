global using Task = System.Threading.Tasks.Task;
using AddCppClass;
using Community.VisualStudio.Toolkit;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Experimentation;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.VCProjectEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace Dwarfovich.AddCppClass
{
    [Command(PackageIds.AddCppClassCommand)]
    internal sealed class AddCppClassCommand
    {
        public static readonly Guid CommandSet = new Guid(PackageGuids.AddCppClassString);

        private static readonly string extensionConfigFile = "AddCppClass.config.json";

        public static AddCppClassCommand Instance
        {
            get;
            private set;
        }

        private AddCppClassCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            var commandId = new CommandID(PackageGuids.AddCppClass, PackageIds.AddCppClassCommand);
            var menuItem = new OleMenuCommand(Execute, commandId);
            menuItem.BeforeQueryStatus += new EventHandler(OnBeforeQueryStatus);
            commandService.AddCommand(menuItem);
        }
        private static string SettingsPath(DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var activeProject = Utils.Solution.CurrentProject();
            if (activeProject is null)
            {
                return "";
            }
            else
            {
                return Path.Combine(new FileInfo(activeProject.FullName).DirectoryName, extensionConfigFile);
            }
        }

        private static Settings GetSettings()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string settingsPath = SettingsPath(AddCppClassPackage.dte);
            Settings settings = new Settings();
            try
            {
                if (File.Exists(settingsPath))
                {
                    string jsonString = File.ReadAllText(settingsPath);
                    JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
                    jsonSettings.ContractResolver = new Utils.ListClearingContractResolver();
                    settings = JsonConvert.DeserializeObject<Settings>(jsonString, jsonSettings);
                    settings.filterToFolder ??= new Dictionary<string, string>();
                }
            }
            catch (Exception ex)
            {
                _ = VS.MessageBox.Show("Error occured while loading AddCppClass settings", "Error message: " + ex.Message, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
                return new Settings();
            }

            return settings;
        }

        private void SaveSettingsToFile(Settings settings)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string settingsPath = SettingsPath(AddCppClassPackage.dte);
            if (String.IsNullOrEmpty(settingsPath))
            {
                _ = VS.MessageBox.Show("Error occured while saving AddCppClass settings", "Couldn't get settings file path. Settings will not be saved.", OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
            }
            else
            {
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(settingsPath, json);
            }
        }

        private void ShowSettingErrorMessage(SettingError settingError)
        {
            string message = "The setting \"" + settingError.settingName + "\" has one or more incorrect values in config file: " + Environment.NewLine;
            foreach (var value in settingError.invalidValues)
            {
                message += '\"' + value + '\"' + Environment.NewLine;
            }
            message += "These values will be ignored.";
            VS.MessageBox.Show("Warning", message, OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            VCProject vcProject = Utils.Solution.CurrentProject().Object as VCProject;
            if (vcProject == null)
            {
                throw new Exception("Failed to convert EnvDTE.Project to VCProject");
            }

            Logger.Log("Hello, Execute()");
            string m = "";
            var filters = vcProject.Filters as System.Collections.IEnumerable;
            foreach (var filter in filters.Cast<VCFilter>())
            {
                m += filter.CanonicalName + "\n";// + filter.Filters + "\n";
            }
            //VS.MessageBox.Show("Warning", m, OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK);

            //string filter = @"filter1\\filter2";
            //VCFilter f = vcProject.AddFilter(filter) as VCFilter;

            //f.AddFile(filePath);

            //return;

            var settings = GetSettings();
            var errors = ClassFacilities.ConformSettings(ref settings);
            foreach (var error in errors)
            {
                ShowSettingErrorMessage(error);
            }

            var monitorSelection = Package.GetGlobalService(typeof(SVsShellMonitorSelection))
    as IVsMonitorSelection;

            monitorSelection.GetCurrentSelection(
                out IntPtr hierarchyPtr,
                out uint itemid,
                out IVsMultiItemSelect multiSelect,
                out IntPtr selectionContainer);

            var hierarchy = Marshal.GetObjectForIUnknown(hierarchyPtr) as IVsHierarchy;
            //Logger.Log($"-------");
            //Logger.Log($"itemid = {itemid}");
            //Logger.Log($"hierarchy = {hierarchy}");
            hierarchy.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_Name, out object name);
            hierarchy.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_SaveName, out object saveName);
            hierarchy.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_ExtObject, out object extObject);

            //Logger.Log($"Name: {name}");
            //Logger.Log($"SaveName: {saveName}");
            //Logger.Log($"ExtObject: {extObject?.GetType().FullName}");

            if (hierarchy == null || itemid == VSConstants.VSITEMID_NIL)
                return;


            //if (extObject is EnvDTE.ProjectItem projectItem)
            //{
            //    Logger.Log($"ProjectItem: {projectItem.Name}");
            //    var filter = projectItem?.Object as VCFilter;

            //    if (filter != null)
            //    {
            //        Logger.Log($"Filter: {filter.Name}");
            //    }
            //}

            var dialog = new AddCppClassDialog(settings);
            dialog.HasMinimizeButton = false;
            dialog.HasMaximizeButton = false;
            bool result = (bool)dialog.ShowModal();
            if (result && dialog.ShouldSaveSettingsToFile())
            {
                SaveSettingsToFile(dialog.settings);
            }
        }
        private void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var command = sender as OleMenuCommand;
            var project = Utils.Solution.CurrentProject();
            if (command == null || project == null)
            {
                return;
            }

            command.Visible = (project.CodeModel.Language == CodeModelLanguageConstants.vsCMLanguageMC)
                           || (project.CodeModel.Language == CodeModelLanguageConstants.vsCMLanguageVC);
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new AddCppClassCommand(package, commandService);
        }
    }
}
