using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json.Serialization;
using AddCppClass;

namespace Dwarfovich.AddCppClass.Utils
{
    public class ListClearingContractResolver : DefaultContractResolver
    {
        protected override JsonArrayContract CreateArrayContract(Type objectType)
        {
            var contract = base.CreateArrayContract(objectType);
            if (objectType.IsGenericType && (objectType.GetGenericTypeDefinition() == typeof(List<>)))
            {
                contract.OnDeserializingCallbacks.Add((obj, streamingContext) =>
                {
                    IList list = (IList)obj;
                    if (list != null)
                    {
                        list.Clear();
                    }
                });
            }

            return contract;
        }
    }
    public static class ListExtensions
    {
        public static T FirstOrValue<T>(this List<T> list, T value)
        {
            if (list.Count > 0)
            {
                return list.First();
            }
            else
            {
                return value;
            }
        }
        public static void SetFrontValue<T>(this List<T> list, T value, int maxListCount)
        {
            var index = list.IndexOf(value);
            if (index == -1)
            {
                list.Insert(0, value);
                if (list.Count > maxListCount)
                {
                    list.RemoveRange(maxListCount, list.Count - maxListCount);
                }
            }
            else
            {
                list.MoveItemAtIndexToFront(index);
            }
        }
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

    public static class Path
    {
        public static string ToWindowsStylePath(string path)
        {
            return path.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);
        }

        public static string ToLinuxStylePath(string path)
        {
            return path.Replace(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
        }
    }

    public static class Solution
    {
        public static EnvDTE.Project CurrentProject()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Array projects = AddCppClassPackage.dte.ActiveSolutionProjects as Array;
            if (projects != null && projects.Length != 0)
            {
                return projects.GetValue(0) as EnvDTE.Project;
            }
            else
            {
                return null;
            }
        }

        public static string CurrentProjectPath()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            EnvDTE.Project project = CurrentProject();
            if (project == null)
            {
                return "";
            }

            try
            {
                return new FileInfo(project.FullName).DirectoryName;
            }
            catch
            {
                return "";
            }
        }
    }
}
