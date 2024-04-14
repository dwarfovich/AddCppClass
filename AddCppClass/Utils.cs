﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System;
using System.Windows.Forms;
using System.Windows.Input;
using EnvDTE;
using EnvDTE80;

namespace Dwarfovich.AddCppClass.Utils
{
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