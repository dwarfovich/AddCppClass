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
    public class ClassNameValidator : ValidationRule
    {
        public Type ValidationType { get; set; }
        internal readonly Regex regex;

        public ClassNameValidator()
        {
            regex = new Regex("^[_a-zA-Z][_a-zA-Z0-9]*$");
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string text = Convert.ToString(value);
            if (regex.IsMatch(text)) { return new ValidationResult(true, ""); }
            else { return new ValidationResult(false, "Wrong symbol"); }
        }
    }
}
