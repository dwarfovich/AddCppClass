using Microsoft.VisualStudio.PlatformUI;
using System.Windows.Controls;

namespace Dwarfovich.AddCppClass
{
    public partial class AddCppClassDialog : DialogWindow
    {
        private ClassSettings settings;
        private ClassGenerator generator;
        public AddCppClassDialog()
        {
            InitializeComponent();
            settings = new ClassSettings();
            generator = new ClassGenerator();
        }

        private void classNameChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                settings.ClassName = textBox.Text;
                generator.GenerateClassData(settings);
                HeaderFilename.Text = generator.headerFileName;
            }
        }
    }
}
