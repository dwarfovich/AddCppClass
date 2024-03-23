using Microsoft;
using Microsoft.VisualStudio.Shell.Interop;

namespace Dwarfovich
{
    internal static class Logger
    {
        private static string name;
        private static IVsOutputWindowPane pane;
        private static IVsOutputWindow output;

        public static void Initialize(IServiceProvider provider, string name)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            output = (IVsOutputWindow)provider.GetService(typeof(SVsOutputWindow));
            Assumes.Present(output);
            Logger.name = name;
        }

        public static void Log(object message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                if (EnsurePane())
                {
                    pane.OutputString(DateTime.Now.ToString() + ": " + message + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
            }
        }

        private static bool EnsurePane()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (pane == null)
            {
                Guid guid = Guid.NewGuid();
                output.CreatePane(ref guid, name, 1, 1);
                output.GetPane(ref guid, out pane);
            }

            return pane != null;
        }
    }
}