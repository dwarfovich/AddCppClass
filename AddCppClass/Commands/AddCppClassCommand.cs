﻿global using Task = System.Threading.Tasks.Task;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;
using Newtonsoft.Json;
using AddCppClass;
using System.ComponentModel.Design;

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

            var activeProject = Utils.Solution.CurrentProject(AddCppClassPackage.dte);
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

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var settings = GetSettings();
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
            var project = Utils.Solution.CurrentProject(AddCppClassPackage.dte);
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
