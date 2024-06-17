using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dwarfovich.AddCppClass
{
    public enum ErrorType
    {
        InvalidClassName,
        ClassExists,
        InvalidNamespace,
        InvalidPrecompiledHeader,
        InvalidHeaderFilename,
        InvalidImplementationFilename,
        InvalidHeaderExtension,
        InvalidHeaderSubfolder,
        InvalidImplementationSubfolder
    };

    public class ClassSettingsErrorsCollection
    {

        private Dictionary<ErrorType, string> errorMessages = new(){
            {ErrorType.InvalidClassName, "Invalid class name"},
            {ErrorType.ClassExists, "Class already exists"},
            {ErrorType.InvalidNamespace, "Invalid namespace"},
            {ErrorType.InvalidPrecompiledHeader, "Invalid precompiled header"},
            {ErrorType.InvalidHeaderFilename, "Invalid header filename"},
            {ErrorType.InvalidImplementationFilename, "Invalid implementation filename"},
            {ErrorType.InvalidHeaderExtension, "Invalid header extension"},
            {ErrorType.InvalidHeaderSubfolder, "Invalid header subfolder"},
            {ErrorType.InvalidImplementationSubfolder, "Invalid implementation subfolder"}
            };

        private List<ErrorType> errors = new();

        public void Clear()
        {
            errors.Clear();
        }
        public bool AddError(ErrorType type)
        {
            if (!errors.Contains(type))
            {
                errors.Add(type);
                return true;
            }

            return false;
        }

        public bool RemoveError(ErrorType type)
        {
            errors.Remove(type);
            return errors.Count != 0;
        }

        public string MessageForLastError()
        {
            if (errors.Count == 0)
            {
                return "";
            }
            else
            {
                return errorMessages[errors.Last()];
            }
        }
    }
}
