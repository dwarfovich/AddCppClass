using Microsoft.VisualStudio.PlatformUI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dwarfovich.AddCppClass
{
    public partial class AddCppClassDialog : DialogWindow
    {
        private ClassSettings settings;
        private ClassGenerator generator;
        private bool shiftEnabled = false;
        public AddCppClassDialog()
        {
            settings = new ClassSettings();
            generator = new ClassGenerator();
            InitializeComponent();
            ClassNameTextBox.PreviewKeyDown += KeyDownPreviewHandler;
            ClassNameTextBox.PreviewKeyUp += KeyUpPreviewHandler;
            ClassNameTextBox.KeyDown += KeyDownHandler;
            // TODO: Change to false.
            AddClassButton.IsEnabled = true;
        }

        private void UpdateFilenameTextBoxes()
        {
            if (HeaderFilename is not null)
            {
                HeaderFilename.Text = generator.headerFilename;
            }
            if (ImplementationFilename is not null)
            {
                ImplementationFilename.Text = generator.implementationFilename;
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
                    generator.GenerateClassData(settings);
                }
                else
                {
                    UpdateHeaderExtensionSettings(button);
                    generator.GenerateFilenames(settings);
                }
                UpdateFilenameTextBoxes();
            }
        }
        private void ClassNameChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                settings.ClassName = textBox.Text.Substring(textBox.Text.LastIndexOf(':') + 1);
                generator.GenerateClassData(settings);
                UpdateFilenameTextBoxes();
                // TODO: uncomment.
                //AddClassButton.IsEnabled = !String.IsNullOrEmpty(settings.ClassName);
            }
        }
        private void KeyDownPreviewHandler(object sender, KeyEventArgs e)
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
        private bool IsLetter(Key key)
        {
            return key >= Key.A && key <= Key.Z;
        }
        private bool IsNavigationOrEditKey(Key key)
        {
            return key == Key.Delete
                || key == Key.Back
                || key == Key.Left
                || key == Key.Up
                || key == Key.Right
                || key == Key.Down
                || key == Key.Home
                || key == Key.End;
        }
        private void KeyDownHandler(object sender, KeyEventArgs e)
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

            if(IsDigit(e.Key)) {
                e.Handled = caretPos == 0 || textBox.Text[caretPos - 1] == ':';
                return;
            }

            if (IsUnderline(e.Key)
                || IsLetter(e.Key)
                || IsNavigationOrEditKey(e.Key))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }
        private void KeyUpPreviewHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                shiftEnabled = false;
            }
        }

        private void AddClassButtonClicked(object sender, RoutedEventArgs e)
        {
            ClassAdder.AddClass(generator, SubFolderCombo.Text);
        }
    }
}
