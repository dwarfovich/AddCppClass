﻿using AddCppClass;
using Community.VisualStudio.Toolkit;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;
using System.IO;
using System.Runtime;
using System.Windows.Shapes;

namespace Dwarfovich.AddCppClass
{
    [Command(PackageIds.AddCppClassCommand)]
    internal sealed class AddCppClassCommand : BaseCommand<AddCppClassCommand>
    {
        private static string SettingsPath(DTE2 dte)
        {
            var activeProject = Utils.Solution.CurrentProject(AddCppClassPackage.dte);
            if (activeProject is null)
            {
                if (dte.Solution is null)
                {
                    return "";
                }
                else
                {
                    return Path.Combine(new FileInfo(dte.Solution.FullName).DirectoryName, "AddCppClass.config.json");
                }
            }
            else
            {
                return Path.Combine(new FileInfo(activeProject.FullName).DirectoryName, "AddCppClass.config.json");
            }
        }

        private static ExtensionSettings GetExtensionSettings()
        {
            string settingsPath = SettingsPath(AddCppClassPackage.dte);
            ExtensionSettings settings = new();
            try
            {
                if (File.Exists(settingsPath))
                {
                    string jsonString = File.ReadAllText(settingsPath);
                    settings = JsonConvert.DeserializeObject<ExtensionSettings>(jsonString);
                }
            }
            catch
            {
                // TODO: handle error.
            }

            return settings;
        }

        private void SaveSettings(ExtensionSettings settings)
        {
            string settingsPath = SettingsPath(AddCppClassPackage.dte);
            if(String.IsNullOrEmpty(settingsPath))
            {
                // TODO: Handle error.
            }
            string json = JsonConvert.SerializeObject(settings);
            File.WriteAllText(settingsPath, json);
        }
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var settings = GetExtensionSettings();
            var dialog = new AddCppClassDialog(settings);
            dialog.HasMinimizeButton = false;
            dialog.HasMaximizeButton = false;
            bool result = (bool)dialog.ShowModal();
            if (result && dialog.ShouldSaveSettings())
            {
                SaveSettings(dialog.ExtensionSettings());
            }
            //await VS.MessageBox.ShowWarningAsync("AddCppClass", "Button clicked");
        }
    }
}
