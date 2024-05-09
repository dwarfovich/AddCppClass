using AddCppClass;
using EnvDTE;
using EnvDTE80;
using Newtonsoft.Json;
using System.IO;

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

        private static Settings GetSettings()
        {
            string settingsPath = SettingsPath(AddCppClassPackage.dte);
            Settings settings = new Settings();
            try
            {
                if (File.Exists(settingsPath))
                {
                    string jsonString = File.ReadAllText(settingsPath);
                    JsonSerializerSettings jsonSettings = new JsonSerializerSettings ();
                    jsonSettings.ContractResolver = new Utils.ListClearingContractResolver();
                    settings = JsonConvert.DeserializeObject<Settings>(jsonString, jsonSettings);
                }
            }
            catch
            {
                // TODO: handle error.
            }

            return settings;
        }

        private void SaveSettings(Settings settings)
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
            var settings = GetSettings();
            var dialog = new AddCppClassDialog(settings);
            dialog.HasMinimizeButton = false;
            dialog.HasMaximizeButton = false;
            bool result = (bool)dialog.ShowModal();
            if (result && dialog.ShouldSaveSettings())
            {
                SaveSettings(dialog.settings);
            }
            //await VS.MessageBox.ShowWarningAsync("AddCppClass", "Button clicked");
        }
    }
}
