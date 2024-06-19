global using Task = System.Threading.Tasks.Task;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;
using Newtonsoft.Json;
using AddCppClass;
using System.ComponentModel.Design;
using Community.VisualStudio.Toolkit;
using System.Collections.Generic;

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
                string json = JsonConvert.SerializeObject(settings);
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

            var dte2 = AddCppClassPackage.dte as DTE2;
            Array projects = AddCppClassPackage.dte.ActiveSolutionProjects as Array;
            var project = projects.GetValue(0) as EnvDTE.Project;

            var props = project.Properties;
            string str = "";
            string str2 = "";
            foreach (Property p in props)
            {
                str += p.Name + "\n";
                //str2 += p.Value.ToString() + "\n";
                if(p.Name == "Filters")
                {
                    p.Value = "Hello";
                }
            }


            return;


            var settings = GetSettings();
            var errors = ClassFacilities.ConformSettings(ref settings);
            foreach (var error in errors)
            {
                ShowSettingErrorMessage(error);
            }

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
