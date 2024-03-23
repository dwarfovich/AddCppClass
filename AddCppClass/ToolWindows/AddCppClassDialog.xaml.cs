using Microsoft.VisualStudio.PlatformUI;
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
                settings.ClassName = textBox.Text;
                generator.GenerateClassData(settings);
                UpdateFilenameTextBoxes();
            }
        }
        private void KeyDownPreviewHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                shiftEnabled = true;
         
            } else if(e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
        private void KeyDownHandler(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                if (String.IsNullOrEmpty(textBox.Text)) // First symbol cann't be a digit.
                {
                    e.Handled = (e.Key >= Key.D0 && e.Key <= Key.D9) || e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9;
                    return;
                }
            }
            
            if ((e.Key >= Key.D0 && e.Key <= Key.D9 && !shiftEnabled)
                || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                || (e.Key == Key.OemMinus && shiftEnabled)
                || (e.Key >= Key.A && e.Key <= Key.Z)
                || e.Key == Key.Delete
                || e.Key == Key.Back
                || e.Key == Key.Left
                || e.Key == Key.Up
                || e.Key == Key.Right
                || e.Key == Key.Down
                || e.Key == Key.Home
                || e.Key == Key.End)
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
    }
}
