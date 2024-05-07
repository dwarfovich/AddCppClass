using System.Collections.Generic;
using System.Windows.Input;
using EnvDTE80;

namespace Dwarfovich.AddCppClass.Utils
{
    public static class ListExtensions
    {
        public static void MoveItemAtIndexToFront<T>(this List<T> list, int index)
        {
            T item = list[index];
            for (int i = index; i > 0; --i)
            {
                list[i] = list[i - 1];
            }
            list[0] = item;
        }
    }
    public static class ClassUtils
    {
        public static string[] TokenizeNamespace(string ns)
        {
            if (String.IsNullOrEmpty(ns))
            {
                return [];
            }
            else
            {
                return ns.Split(new[] { "::" }, StringSplitOptions.None);
            }

        }
    }
    public static class Keyboard
    {
        public static bool NumlockActive()
        {
            return System.Windows.Input.Keyboard.IsKeyToggled(Key.NumLock);
        }
    }

    public static class Solution
    {
        public static EnvDTE.Project CurrentProject(DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Array projects = dte.ActiveSolutionProjects as Array;
            if (projects != null && projects.Length != 0)
            {
                return projects.GetValue(0) as EnvDTE.Project;
            }
            else
            {
                return null;
            }
        }
    }
}
