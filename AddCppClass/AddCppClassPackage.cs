global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using Dwarfovich;
using Dwarfovich.AddCppClass;
using System.Runtime.InteropServices;
using System.Threading;

namespace AddCppClass
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.AddCppClassString)]
    public sealed class AddCppClassPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.RegisterCommandsAsync();
            Logger.Initialize(this, Vsix.Name);
            Logger.Log("efdefef");
        }
    }
}