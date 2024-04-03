using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.PlatformUI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dwarfovich.AddCppClass
{
    public partial class AddCppClassDialog : DialogWindow
    {
        private ClassSettings settings = new ClassSettings();
        private ClassGenerator generator = new ClassGenerator();
        private bool shiftEnabled = false;
        public AddCppClassDialog()
        {
            InitializeComponent();
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
            AddClassButton.IsEnabled = false;
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
                settings.Style = FilenameStyle.CamelCase;
            }
            else if (button.Content.ToString() == "snake__case")
            {
                settings.Style = FilenameStyle.SnakeCase;
            }
            else
            {
                settings.Style = FilenameStyle.LowerCase;
            }
        }

        private void UpdateHeaderExtensionSettings(RadioButton button)
        {
            settings.HeaderExtension = button.Content.ToString();
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

        private void ClassNameChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                if(ClassGenerator.IsValidClassName(textBox.Text))
                {
                    settings.ClassName = textBox.Text.Substring(textBox.Text.LastIndexOf(':') + 1);
                    (settings.headerFilename, settings.implementationFilename) = generator.GenerateFilenames(settings);
                    UpdateFilenameTextBoxes();
                    AddClassButton.IsEnabled = true;
                }
                else
                {
                    AddClassButton.IsEnabled = false;
                }
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

            if (HeaderSubfolderCombo.Text.EndsWith("/"))
            {
                HeaderSubfolderCombo.Text = HeaderSubfolderCombo.Text.Remove(HeaderSubfolderCombo.Text.Length - 1);
            }
            settings.headerSubfolder = HeaderSubfolderCombo.Text.Replace('/', '\\');
            if (settings.useSingleSubfolder)
            {
                settings.implementationSubfolder = settings.headerSubfolder;
            }
            else
            {
                if (ImplementationSubfolderCombo.Text.EndsWith("/"))
                {
                    ImplementationSubfolderCombo.Text = ImplementationSubfolderCombo.Text.Remove(ImplementationSubfolderCombo.Text.Length - 1);
                }
                settings.implementationSubfolder = ImplementationSubfolderCombo.Text.Replace('/', '\\');
            }

            settings.hasImplementationFile = !(bool)DontCreateCppFileCheckBox.IsChecked;

            ClassAdder.AddClass(settings);
            Close();
        }
        private void UseSingleSubfolderCheckChanged(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox is null)
            {
                return;
            }

            ImplementationSubfolderCombo.IsEnabled = (bool)!checkBox.IsChecked;
        }

        private void CreateFiltersCheckChanged(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox is null)
            {
                return;
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
                        textBox.Text = textBox.Text.Insert(caretPos, "/"); ;
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
    }
}
