using Microsoft.VisualStudio.PlatformUI;
using System.Windows.Controls;

namespace Dwarfovich.AddCppClass
{
    public partial class AddCppClassDialog : DialogWindow
    {
        public AddCppClassDialog()
        {
            InitializeComponent();
        }

        private void classNameChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            //MessageBox.Show("Changed");
            //System.Windows.Forms.MessageBox.Show("test");77
            VS.MessageBox.ShowWarningAsync("AddCppClass", "Button clicked");
        }
    }
}
