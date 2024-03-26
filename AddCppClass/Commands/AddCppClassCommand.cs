using EnvDTE;
using Microsoft.VisualStudio.Threading;

namespace Dwarfovich.AddCppClass
{
    [Command(PackageIds.AddCppClassCommand)]
    internal sealed class AddCppClassCommand : BaseCommand<AddCppClassCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            //await VS.MessageBox.ShowWarningAsync("AddCppClass", "Button clicked");
            
            var dialog = new AddCppClassDialog();
            dialog.HasMinimizeButton = false;
            dialog.HasMaximizeButton = false;
            bool result = (bool)dialog.ShowModal();
        }
    }
}
