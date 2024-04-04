using System;
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

namespace Dwarfovich.AddCppClass.Utils
{
    public static class Keyboard
    {
        public static bool NumlockActive()
        {
            return System.Windows.Input.Keyboard.IsKeyToggled(Key.NumLock);
        }
    }
}
