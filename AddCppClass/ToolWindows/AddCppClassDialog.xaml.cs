using Microsoft.VisualStudio.PlatformUI;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dwarfovich.AddCppClass
{
    public partial class AddCppClassDialog : DialogWindow
    {
        public Settings settings { get; private set; } = new();
        private ClassGenerator generator = new();
        private ClassSettingsErrorsCollection errors = new();
        private bool shiftEnabled = false;

        public AddCppClassDialog()
        {
            InitializeComponent();
            AssignKeyHandlers();

            errors.Clear();
            ClassNameTextBox.Text = "MyClass";
        }

        public AddCppClassDialog(Settings settings)
        {
            InitializeComponent();
            AssignKeyHandlers();

            LoadSettings(settings);
            errors.Clear();
            ClassNameTextBox.Text = "MyClass";
        }

        private void AssignKeyHandlers()
        {
            ClassNameTextBox.PreviewKeyDown += ClassNameKeyDownPreviewHandler;
            ClassNameTextBox.PreviewKeyUp += ClassNameKeyUpPreviewHandler;
            ClassNameTextBox.KeyDown += ClassNameKeyDownHandler;
            HeaderSubfolderCombo.PreviewKeyDown += SubfolderKeyDownPreviewHandler;
            HeaderSubfolderCombo.PreviewKeyUp += SubfolderKeyUpPreviewHandler;
            HeaderSubfolderCombo.KeyDown += SubfolderKeyDownHandler;
            ImplementationSubfolderCombo.PreviewKeyDown += SubfolderKeyDownPreviewHandler;
            ImplementationSubfolderCombo.PreviewKeyUp += SubfolderKeyUpPreviewHandler;
            ImplementationSubfolderCombo.KeyDown += SubfolderKeyDownHandler;
            HeaderFilename.PreviewKeyDown += FileKeyDownPreviewHandler;
            HeaderFilename.PreviewKeyUp += FileKeyUpPreviewHandler;
            HeaderFilename.KeyDown += FileKeyDownHandler;
            ImplementationFilename.PreviewKeyDown += FileKeyDownPreviewHandler;
            ImplementationFilename.PreviewKeyUp += FileKeyUpPreviewHandler;
            ImplementationFilename.KeyDown += FileKeyDownHandler;
            PrecompiledHeader.PreviewKeyDown += PrecompiledHeaderKeyDownPreviewHandler;
            PrecompiledHeader.PreviewKeyUp += PrecompiledHeaderKeyUpPreviewHandler;
            PrecompiledHeader.KeyDown += PrecompiledHeaderKeyDownHandler;
        }
        private void LoadSettings(Settings settings)
        {
            switch (settings.filenameStyle)
            {
                case FilenameStyle.CamelCase: CamelCaseNameStyle.IsChecked = true; break;
                case FilenameStyle.SnakeCase: SnakeCaseNameStyle.IsChecked = true; break;
                case FilenameStyle.LowerCase: LowerCaseNameStyle.IsChecked = true; break;
                default: CamelCaseNameStyle.IsChecked = true; break;
            }
            if (settings.headerExtension == ".h")
            {
                HeaderHStyle.IsChecked = true;
            }
            else
            {
                HeaderHppStyle.IsChecked = true;
            }
            UseSingleSubfolderCheckBox.IsChecked = settings.useSingleSubfolder;
            CreateFiltersCheckBox.IsChecked = settings.createFilters;
            HasCppFileCheckBox.IsChecked = settings.hasImplementationFile;
            for (int i = 0; i < Math.Min(settings.recentHeaderSubfoldersCount, settings.recentHeaderSubfolders.Length); ++i)
            {
                HeaderSubfolderCombo.Items.Add(settings.recentHeaderSubfolders[i]);
            }
            HeaderSubfolderCombo.SelectedIndex = 0;
            for (int i = 0; i < Math.Min(settings.recentImplementationSubfoldersCount, settings.recentImplementationSubfolders.Length); ++i)
            {
                ImplementationSubfolderCombo.Items.Add(settings.recentImplementationSubfolders[i]);
            }
            ImplementationSubfolderCombo.SelectedIndex = 0;
            AutosaveSettingsCheckBox.IsChecked = settings.autoSaveSettings;
        }
        private void UpdateFilenameTextBoxes()
        {
            if (HeaderFilename is not null)
            {
                HeaderFilename.Text = settings.headerFilename;
            }
            if (ImplementationFilename is not null)
            {
                ImplementationFilename.Text = settings.implementationFilename;
            }
        }

        private void UpdateFilenameStyleSettings(RadioButton button)
        {
            if (button.Content.ToString() == "CamelCase")
            {
                settings.filenameStyle = FilenameStyle.CamelCase;
            }
            else if (button.Content.ToString() == "snake__case")
            {
                settings.filenameStyle = FilenameStyle.SnakeCase;
            }
            else
            {
                settings.filenameStyle = FilenameStyle.LowerCase;
            }
        }

        private void UpdateHeaderExtensionSettings(RadioButton button)
        {
            settings.headerExtension = button.Content.ToString();
        }
        private void RadioButtonChecked(object sender, EventArgs e)
        {
            RadioButton button = sender as RadioButton;
            if (button != null && (bool)button.IsChecked)
            {
                if (button.GroupName == "filenameStyleGroup")
                {
                    UpdateFilenameStyleSettings(button);
                    (settings.headerFilename, settings.implementationFilename) = generator.GenerateFilenames(settings);
                }
                else
                {
                    UpdateHeaderExtensionSettings(button);
                    (settings.headerFilename, settings.implementationFilename) = generator.GenerateFilenamesForChangedExtension(settings);
                }
                UpdateFilenameTextBoxes();
            }
        }

        private bool IsColon(Key key)
        {
            return (key == Key.Oem1 || key == Key.OemSemicolon || key == Key.D6) && shiftEnabled;
        }
        private bool CanInsertNamespaceDelimiter(string text, int caretPos)
        {
            if (String.IsNullOrEmpty(text))
            {
                return true;
            }
            int previousPos = caretPos - 1;
            int nextPos = caretPos + 1 < text.Length ? caretPos + 1 : -1;

            if (previousPos < 0 || text[previousPos] != ':')
            {
                if (nextPos == -1 || (text[nextPos] != ':'))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsDigit(Key key)
        {
            return (key >= Key.D0 && key <= Key.D9 && !shiftEnabled)
                || (key >= Key.NumPad0 && key <= Key.NumPad9 && !Utils.Keyboard.NumlockActive());
        }

        private bool IsUnderline(Key key)
        {
            return key == Key.OemMinus && shiftEnabled;
        }
        private bool IsPathSeparator(Key key)
        {
            return key == Key.OemBackslash || key == Key.Divide || (key == Key.OemQuestion && !shiftEnabled) || key == Key.Oem5;
        }
        private bool IsPeriod(Key key)
        {
            return key == Key.OemPeriod;
        }
        private bool IsLetter(Key key)
        {
            return key >= Key.A && key <= Key.Z;
        }
        private bool IsNavigationKey(Key key)
        {
            return key == Key.Left
                || key == Key.Up
                || key == Key.Right
                || key == Key.Down
                || key == Key.Home
                || key == Key.End;
        }

        private void RemoveError(Object source)
        {
            bool hasOtherErrors = errors.RemoveError(source);
            if (AddClassButton != null)
            {
                AddClassButton.IsEnabled = !hasOtherErrors;
            }
        }

        private void AddError(Object source, string message)
        {
            errors.AddError(source, message);
            if (AddClassButton != null)
            {
                AddClassButton.IsEnabled = false;
            }
        }
        private void ClassNameChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                if (ClassGenerator.IsValidClassName(textBox.Text))
                {
                    settings.className = textBox.Text.Substring(textBox.Text.LastIndexOf(':') + 1);
                    (settings.headerFilename, settings.implementationFilename) = generator.GenerateFilenames(settings);
                    UpdateFilenameTextBoxes();
                    RemoveError(textBox);
                }
                else
                {
                    AddError(textBox, "Class name contains errors");
                }
            }
        }

        private void HeaderSubfolderChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox is null)
            {
                return;
            }

            if (ClassGenerator.IsValidSubfolder(comboBox.Text))
            {
                RemoveError(comboBox);
            }
            else
            {

                AddError(comboBox, "Header subfolder is invalid");
            }
        }
        private void ImplementationSubfolderChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox is null)
            {
                return;
            }

            if (ClassGenerator.IsValidSubfolder(comboBox.Text))
            {
                RemoveError(comboBox);
            }
            else
            {

                AddError(comboBox, "Implementation subfolder is invalid");
            }
        }

        private void HeaderFilenameChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            TextBox textBox = sender as TextBox;
            if (textBox is null)
            {
                return;
            }

            if (ClassGenerator.IsValidFilename(textBox.Text))
            {
                RemoveError(textBox);
            }
            else
            {

                AddError(textBox, "Header file name is invalid");
            }
        }
        private void ImplementationFilenameChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            TextBox textBox = sender as TextBox;
            if (textBox is null)
            {
                return;
            }

            if (ClassGenerator.IsValidFilename(textBox.Text))
            {
                RemoveError(textBox);
            }
            else
            {

                AddError(textBox, "Implementation file name is invalid");
            }
        }
        private void ClassNameKeyDownPreviewHandler(object sender, KeyEventArgs e)
        {
            Logger.Log(e.Key.ToString());
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                shiftEnabled = true;

            }
            else if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
            else if (e.Key == Key.Delete)
            {
                TextBox textBox = sender as TextBox;
                if (textBox == null)
                {
                    throw new InvalidCastException("Sender should be a TextBox");
                }

                var caretPos = textBox.CaretIndex;
                if (caretPos < textBox.Text.Length && textBox.Text[caretPos] == ':')
                {
                    if (caretPos + 1 < textBox.Text.Length - 1 && textBox.Text[caretPos + 1] == ':')
                    {
                        textBox.Text = textBox.Text.Remove(caretPos, 2);
                    }
                    else if (caretPos - 1 >= 0 && textBox.Text[caretPos - 1] == ':')
                    {
                        textBox.Text = textBox.Text.Remove(caretPos - 1, 2);
                        textBox.CaretIndex = caretPos - 1;
                    }
                    e.Handled = true;
                }
                else
                {
                    e.Handled = false;
                }
            }
            else if (e.Key == Key.Back)
            {
                TextBox textBox = sender as TextBox;
                if (textBox == null)
                {
                    throw new InvalidCastException("Sender should be a TextBox");
                }

                var caretPos = textBox.CaretIndex;
                if (caretPos < textBox.Text.Length && textBox.Text[caretPos] == ':')
                {
                    if (caretPos - 1 >= 0 && textBox.Text[caretPos - 1] == ':')
                    {
                        textBox.Text = textBox.Text.Remove(caretPos - 1, 2);
                        textBox.CaretIndex = caretPos - 1;
                        e.Handled = true;
                    }
                    else
                    {
                        e.Handled = false;
                    }
                }
                else if (caretPos == textBox.Text.Length || textBox.Text[caretPos] != ':')
                {
                    if (caretPos - 1 >= 0 && textBox.Text[caretPos - 1] == ':')
                    {
                        textBox.Text = textBox.Text.Remove(caretPos - 2, 2);
                        textBox.CaretIndex = caretPos - 2;
                        e.Handled = true;
                    }
                    else
                    {
                        e.Handled = false;
                    }
                }
                else
                {
                    e.Handled = false;
                }
            }
        }
        private void ClassNameKeyDownHandler(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null)
            {
                throw new InvalidCastException("Sender should be a TextBox");
            }

            var caretPos = textBox.CaretIndex;
            if (String.IsNullOrEmpty(textBox.Text) || caretPos == 0) // First symbol cann't be a digit.
            {
                if (IsDigit(e.Key))
                {
                    e.Handled = true;
                    return;
                }
            }

            if (IsColon(e.Key))
            {
                var canInsert = CanInsertNamespaceDelimiter(textBox.Text, caretPos);
                if (canInsert)
                {
                    textBox.Text = textBox.Text.Insert(caretPos, "::"); ;
                    textBox.CaretIndex = caretPos + 2;
                }
                e.Handled = true;
                return;
            }

            if (IsDigit(e.Key))
            {
                e.Handled = (caretPos == 0 || textBox.Text[caretPos - 1] == ':');
                return;
            }

            if (IsUnderline(e.Key)
                || IsLetter(e.Key)
                || IsNavigationKey(e.Key))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }
        private void ClassNameKeyUpPreviewHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                shiftEnabled = false;
            }
        }

        private void AddClassButtonClicked(object sender, RoutedEventArgs e)
        {
            settings.useSingleSubfolder = (bool)UseSingleSubfolderCheckBox.IsChecked;

            if (HeaderSubfolderCombo.Text.EndsWith("\\"))
            {
                HeaderSubfolderCombo.Text = HeaderSubfolderCombo.Text.Remove(HeaderSubfolderCombo.Text.Length - 1);
            }
            if (settings.useSingleSubfolder)
            {
                settings.implementationSubfolder = settings.headerSubfolder;
            }
            else
            {
                if (ImplementationSubfolderCombo.Text.EndsWith("\\"))
                {
                    ImplementationSubfolderCombo.Text = ImplementationSubfolderCombo.Text.Remove(ImplementationSubfolderCombo.Text.Length - 1);
                }
            }

            settings.hasImplementationFile = (bool)HasCppFileCheckBox.IsChecked;

            ClassAdder.AddClass(settings);
            Close();
        }

        public bool ShouldSaveSettings()
        {
            return (bool)AutosaveSettingsCheckBox.IsChecked;
        }
        public Settings Settings()
        {
            if ((bool)CamelCaseNameStyle.IsChecked)
            {
                settings.filenameStyle = FilenameStyle.CamelCase;
            }
            else if ((bool)SnakeCaseNameStyle.IsChecked)
            {
                settings.filenameStyle = FilenameStyle.SnakeCase;
            }
            else
            {
                settings.filenameStyle = FilenameStyle.LowerCase;
            }
            if ((bool)HeaderHStyle.IsChecked)
            {
                settings.headerExtension = ".h";
            }
            else
            {
                settings.headerExtension = ".hpp";
            }
            settings.useSingleSubfolder = (bool)UseSingleSubfolderCheckBox.IsChecked;
            settings.createFilters = (bool)CreateFiltersCheckBox.IsChecked;
            settings.hasImplementationFile = (bool)HasCppFileCheckBox.IsChecked;

            string[] subfolders = Array.Empty<String>();
            for (int i = 0; i < Math.Min(HeaderSubfolderCombo.Items.Count, settings.recentHeaderSubfoldersCount); i++)
            {
                subfolders.Append(HeaderSubfolderCombo.Items[i].ToString());
            }
            settings.recentHeaderSubfolders = subfolders;

            subfolders = Array.Empty<String>();

            for (int i = 0; i < Math.Min(ImplementationSubfolderCombo.Items.Count, settings.recentImplementationSubfoldersCount); i++)
            {
                subfolders.Append(ImplementationSubfolderCombo.Items[i].ToString());
            }
            settings.recentImplementationSubfolders = subfolders;

            settings.autoSaveSettings = (bool)AutosaveSettingsCheckBox.IsChecked;

            return settings;
        }

        private void UseSingleSubfolderCheckChanged(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox is null)
            {
                return;
            }

            settings.useSingleSubfolder = (bool)checkBox.IsChecked;
            ImplementationSubfolderCombo.IsEnabled = !(bool)checkBox.IsChecked;
            if ((bool)checkBox.IsChecked)
            {
                RemoveError(ImplementationSubfolderCombo);
            }
            else
            {
                if (ClassGenerator.IsValidSubfolder(ImplementationSubfolderCombo.Text))
                {
                    RemoveError(ImplementationSubfolderCombo);
                }
                else
                {
                    AddError(ImplementationSubfolderCombo, "Implementation subfolder is invalid");
                }
            }
        }

        private void CreateFiltersCheckChanged(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox is null)
            {
                return;
            }

            if ((bool)checkBox.IsChecked)
            {
                if (ClassGenerator.IsValidSubfolder(HeaderSubfolderCombo.Text))
                {
                    RemoveError(HeaderSubfolderCombo);
                }
                else
                {
                    AddError(HeaderSubfolderCombo, "Header subfolder is invalid");
                }
                if (ClassGenerator.IsValidSubfolder(ImplementationSubfolderCombo.Text))
                {
                    RemoveError(ImplementationSubfolderCombo);
                }
                else
                {
                    AddError(ImplementationSubfolderCombo, "Implementation subfolder is invalid");
                }
            }
            else
            {
                RemoveError(HeaderSubfolderCombo);
                RemoveError(ImplementationSubfolderCombo);
            }

            settings.createFilters = (bool)checkBox.IsChecked;
        }

        private bool CanInsertPathSeparator(string text, int caretPos)
        {
            if (String.IsNullOrEmpty(text))
            {
                return false;
            }
            int previousPos = caretPos - 1;
            int nextPos = caretPos + 1 < text.Length ? caretPos + 1 : -1;

            if (previousPos < 0 || text[previousPos] != '/')
            {
                if (nextPos == -1 || (text[nextPos] != '/'))
                {
                    return true;
                }
            }

            return false;
        }
        private void SubfolderKeyDownPreviewHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                shiftEnabled = true;

            }
            else if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void SubfolderKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (IsDigit(e.Key) || IsLetter(e.Key) || IsUnderline(e.Key) || IsPeriod(e.Key))
            {
                e.Handled = false;
            }
            else if (IsPathSeparator(e.Key))
            {
                ComboBox combo = sender as ComboBox;
                if (combo == null)
                {
                    throw new InvalidCastException("Sender should be a TextBox");
                }

                var textBox = (TextBox)combo.Template.FindName("PART_EditableTextBox", combo);
                if (textBox == null)
                {
                    throw new InvalidCastException("Couldn't get TextBox part of ComboBox");
                }

                var caretPos = textBox.CaretIndex;
                if (String.IsNullOrEmpty(textBox.Text) || caretPos == 0) // First symbol cann't be a path separator.
                {
                    if (IsPathSeparator(e.Key))
                    {
                        e.Handled = true;
                        return;
                    }
                }

                if (IsPathSeparator(e.Key))
                {
                    var canInsert = CanInsertPathSeparator(textBox.Text, caretPos);
                    if (canInsert)
                    {
                        textBox.Text = textBox.Text.Insert(caretPos, "\\"); ;
                        textBox.CaretIndex = caretPos + 1;
                    }
                    e.Handled = true;
                    return;
                }

            }
            else
            {
                e.Handled = true;
            }
        }
        private void SubfolderKeyUpPreviewHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                shiftEnabled = false;
            }
        }

        private void FileKeyDownPreviewHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                shiftEnabled = true;

            }
            else if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
        private void FileKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (IsDigit(e.Key) || IsLetter(e.Key) || IsUnderline(e.Key) || IsPeriod(e.Key))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }
        private void FileKeyUpPreviewHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                shiftEnabled = false;
            }
        }

        private void PrecompiledHeaderKeyDownPreviewHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                shiftEnabled = true;

            }
            else if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
        private void PrecompiledHeaderKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (IsDigit(e.Key) || IsLetter(e.Key) || IsUnderline(e.Key) || IsPeriod(e.Key) || IsPathSeparator(e.Key))
            {
                if (IsPathSeparator(e.Key))
                {
                    var textBox = sender as TextBox;
                    if (textBox == null)
                    {
                        throw new InvalidCastException("Couldn't get TextBox part of ComboBox");
                    }

                    var caretPos = textBox.CaretIndex;
                    var canInsert = CanInsertPathSeparator(textBox.Text, caretPos);
                    if (canInsert)
                    {
                        textBox.Text = textBox.Text.Insert(caretPos, "/"); ;
                        textBox.CaretIndex = caretPos + 1;
                    }
                    e.Handled = true;
                }
                else
                {
                    e.Handled = false;
                }
            }
            else
            {
                e.Handled = true;
            }
        }
        private void PrecompiledHeaderKeyUpPreviewHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                shiftEnabled = false;
            }
        }

        private void HasCppFileCheckChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox is null)
            {
                return;
            }

            settings.hasImplementationFile = (bool)checkBox.IsChecked;
            ImplementationFilename.IsEnabled = (bool)checkBox.IsChecked;
            ImplementationSubfolderCombo.IsEnabled = (bool)checkBox.IsChecked;

            if ((bool)checkBox.IsChecked)
            {
                if (!ClassGenerator.IsValidFilename(ImplementationFilename.Text))
                {
                    AddError(ImplementationFilename, "Implementation file name is invalid");
                }
                if (!ClassGenerator.IsValidSubfolder(ImplementationSubfolderCombo.Text))
                {
                    AddError(ImplementationSubfolderCombo, "Implementation subfolder is invalid");
                }
            }
            else
            {
                RemoveError(ImplementationFilename);
                RemoveError(ImplementationSubfolderCombo);
            }
        }

        private void IncludePrecompiledHeaderCheckChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox is null)
            {
                return;
            }

            if ((bool)checkBox.IsChecked)
            {
                PrecompiledHeader.IsEnabled = true;
                if (ClassGenerator.IsValidSubfolder(PrecompiledHeader.Text))
                {
                    RemoveError(PrecompiledHeader);
                }
                else
                {
                    AddError(PrecompiledHeader, "Precompiled header file name is invalid");
                }
            }
            else
            {
                PrecompiledHeader.IsEnabled = false;
                RemoveError(PrecompiledHeader);
            }
        }

        private void PrecompiledHeaderFilenameChangedEventHandler(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                if (ClassGenerator.IsValidSubfolder(textBox.Text))
                {
                    RemoveError(sender);
                }
                else
                {
                    AddError(sender, "Precompiled header file name is invalid");
                }
            }
        }
    }
}
