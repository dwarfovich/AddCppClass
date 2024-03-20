using Microsoft.VisualStudio.PlatformUI;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dwarfovich.AddCppClass
{
    public partial class AddCppClassDialog : DialogWindow
    {
        private ClassSettings settings;
        private ClassGenerator generator;
        public AddCppClassDialog()
        {
            settings = new ClassSettings();
            generator = new ClassGenerator();
            InitializeComponent();
            ClassNameTextBox.PreviewKeyDown += KeyDownPreviewHandler;

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
        private void KeyDownPreviewHandler(object sender, KeyEventArgs args)
        {
            if ((args.Key >= Key.D0 && args.Key <= Key.D9) || (args.Key >= Key.A && args.Key <= Key.Z) || args.Key == Key.OemMinus)
            {
                args.Handled = false;
            }
            else
            {
                args.Handled = true;
            }
        }
    }
}
