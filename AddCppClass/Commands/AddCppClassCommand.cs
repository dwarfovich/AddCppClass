global using Task = System.Threading.Tasks.Task;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;
using Newtonsoft.Json;
using AddCppClass;

namespace Dwarfovich.AddCppClass
{
    [Command(PackageIds.AddCppClassCommand)]
    internal sealed class AddCppClassCommand : BaseCommand<AddCppClassCommand>
    {
        private static readonly string extensionConfigFile = "AddCppClass.config.json";
        private static string SettingsPath(DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var activeProject = Utils.Solution.CurrentProject(AddCppClassPackage.dte);
            if (activeProject is null)
            {
                if (dte.Solution is null)
                {
                    return "";
                }
                else
                {
                    return Path.Combine(new FileInfo(dte.Solution.FullName).DirectoryName, extensionConfigFile);
                }
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

        private void SaveSettings(Settings settings)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string settingsPath = SettingsPath(AddCppClassPackage.dte);
            if(String.IsNullOrEmpty(settingsPath))
            {
                _ = VS.MessageBox.Show("Error occured while saving AddCppClass settings", "Couldn't get settings file path. Settings will not be saved.", OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
            }
            else
            {
                string json = JsonConvert.SerializeObject(settings);
                File.WriteAllText(settingsPath, json);
            }
        }
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var settings = GetSettings();
            var dialog = new AddCppClassDialog(settings);
            dialog.HasMinimizeButton = false;
            dialog.HasMaximizeButton = false;
            bool result = (bool)dialog.ShowModal();
            if (result && dialog.ShouldSaveSettings())
            {
                SaveSettings(dialog.settings);
            }
        }
    }
}
