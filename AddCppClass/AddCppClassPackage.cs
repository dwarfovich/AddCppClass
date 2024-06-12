global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
using Dwarfovich;
using Dwarfovich.AddCppClass;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;
using System.Threading;

namespace AddCppClass
{
    [ProvideAutoLoad(UIContextGuids.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.AddCppClassString)]
    public sealed class AddCppClassPackage : AsyncPackage
    {
        public static DTE2 dte;
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            await AddCppClassCommand.InitializeAsync(this);
            dte = await GetServiceAsync(typeof(DTE)) as DTE2;
            Assumes.Present(dte);

            Logger.Initialize(this, Vsix.Name);
            Logger.Log("Hello, logger");
        }
    }
}