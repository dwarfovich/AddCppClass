using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dwarfovich.AddCppClass
{
    public class ClassSettingsErrorsCollection
    {
        private Dictionary<Object, string> errors = new ();

        public void Clear()
        {
            errors.Clear();
        }
        public void AddError(Object source, string message)
        {
            errors[source] = message;
        }

        public bool RemoveError(Object source)
        {
            errors.Remove(source);
            return errors.Count != 0;
        }

        public string NextMessage()
        {
            if (errors.Count == 0)
            {
                return "";
            }
            else
            {
                return errors.First().Value;
            }
        }
    }
}
