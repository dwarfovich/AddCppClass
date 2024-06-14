using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using AddCppClass;
using Community.VisualStudio.Toolkit;

namespace Dwarfovich.AddCppClass
{
    public partial class AddCppClassDialog : DialogWindow
    {
        public Settings settings { get; private set; } = new();

        private ClassFacilities generator = new();
        private ClassSettingsErrorsCollection errors = new();
        private bool shiftEnabled = false;
        private readonly string title = "Add C++ class";
        private readonly string defaultclassName = "MyClass";
        private readonly string errorMessageBeginning = "Error message: ";

        public AddCppClassDialog()
        {
            InitializeGui();

            errors.Clear();
        }

        public AddCppClassDialog(Settings settings)
        {
            InitializeGui();

            LoadSettings(settings);
            errors.Clear();
        }

        private void InitializeGui()
        {
            InitializeComponent();
            Title = title;
            classNameTextBox.Text = defaultclassName;
        }

        private void LoadSettings(Settings settings)
        {
            this.settings = settings;

            switch (settings.filenameStyle)
            {
                case FilenameStyle.CamelCase: CamelCaseNameStyle.IsChecked = true; break;
                case FilenameStyle.SnakeCase: SnakeCaseNameStyle.IsChecked = true; break;
                case FilenameStyle.LowerCase: LowerCaseNameStyle.IsChecked = true; break;
                default: CamelCaseNameStyle.IsChecked = true; break;
            }

            PopulateComboBox(NamespaceCombo, settings.recentNamespaces, settings.maxRecentNamespaces);
            PopulateComboBox(HeaderExtensionCombo, settings.recentHeaderExtensions, settings.maxRecentHeaderExtensions, ".h");

            CreateFiltersCheckBox.IsChecked = settings.createFilters;
            HasImplementationFileCheckBox.IsChecked = settings.hasImplementationFile;

            PopulateComboBox(HeaderSubfolderCombo, settings.recentHeaderSubfolders, settings.maxRecentHeaderSubfolders);
            PopulateComboBox(ImplementationSubfolderCombo, settings.recentImplementationSubfolders, settings.maxRecentImplementationSubfolders);
            UseSingleSubfolderCheckBox.IsChecked = settings.useSingleSubfolder;
            if (settings.useSingleSubfolder)
            {
                ImplementationSubfolderCombo.SelectedItem = HeaderSubfolderCombo.Text;
                ImplementationSubfolderCombo.IsEnabled = false;
            }

            IncludePrecompiledHeaderCheckBox.IsChecked = settings.includePrecompiledHeader;
            PrecompiledHeader.Text = settings.precompiledHeader;
            if (!settings.includePrecompiledHeader)
            {
                PrecompiledHeader.IsEnabled = false;
            }

            AutosaveSettingsCheckBox.IsChecked = settings.autoSaveSettings;
        }

        private void PopulateComboBox(ComboBox comboBox, List<string> values, int maxItems = 0, string spareValue = null)
        {
            comboBox.Items.Clear();
            if (values != null && values.Count > 0)
            {
                for (int i = 0; i < Math.Min(values.Count, maxItems); ++i)
                {
                    comboBox.Items.Add(values[i]);
                }
            }
            else
            {
                if (spareValue != null)
                {
                    comboBox.Items.Add(spareValue);
                }
            }
            comboBox.SelectedIndex = 0;
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
                UpdateFilenameTextBoxes();
            }
        }

        private void RemoveError(Object source)
        {
            bool hasOtherErrors = errors.RemoveError(source);
            if (AddClassButton != null)
            {
                AddClassButton.IsEnabled = !hasOtherErrors;
                SetErrorMessage(errors.NextMessage());
            }
        }

        private void SetErrorMessage(string message)
        {
            if (ErrorMessage != null)
            {
                ErrorMessage.Content = errorMessageBeginning + message;
            }
        }
        private void AddError(Object source, string message)
        {
            errors.AddError(source, message);
            SetErrorMessage(message);
            if (AddClassButton != null)
            {
                AddClassButton.IsEnabled = false;
            }

        }
        private void classNameChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null)
            {
                throw new InvalidCastException("Sender should be a TextBox");
            }

            if (textBox != null)
            {
                if (ClassFacilities.IsValidClassName(textBox.Text))
                {
                    settings.className = textBox.Text.Substring(textBox.Text.LastIndexOf(':') + 1);
                    (settings.headerFilename, settings.implementationFilename) = generator.GenerateFilenames(settings);
                    UpdateFilenameTextBoxes();
                    RemoveError(textBox);
                }
                else
                {
                    AddError(textBox, "Class name is invalid.");
                }
            }
        }

        private void classNameComboSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            if (combo == null)
            {
                return;
            }
        }

        private void HeaderSubfolderChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox is null)
            {
                return;
            }

            if (ClassFacilities.IsValidSubfolder(comboBox.Text))
            {
                RemoveError(comboBox);
                if (settings.useSingleSubfolder)
                {
                    ImplementationSubfolderCombo.Text = comboBox.Text;
                }
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

            if (ClassFacilities.IsValidSubfolder(comboBox.Text))
            {
                RemoveError(comboBox);
            }
            else
            {
                AddError(comboBox, "Implementation subfolder is invalid");
            }
        }

        private void HeaderExtensionChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox is null)
            {
                return;
            }

            if (ClassFacilities.IsValidHeaderExtension(comboBox.Text))
            {
                RemoveError(comboBox);
                settings.AddMostRecentHeaderExtension(comboBox.Text);
                (settings.headerFilename, settings.implementationFilename) = generator.GenerateFilenamesForChangedExtension(settings);
                UpdateFilenameTextBoxes();
            }
            else
            {
                AddError(comboBox, "Header extension is invalid");
            }
        }
        private void HeaderFilenameChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            TextBox textBox = sender as TextBox;
            if (textBox is null)
            {
                return;
            }

            if (ClassFacilities.IsValidFilename(textBox.Text))
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

            if (ClassFacilities.IsValidFilename(textBox.Text))
            {
                RemoveError(textBox);
            }
            else
            {
                AddError(textBox, "Implementation file name is invalid");
            }
        }

        private void SaveSettings()
        {
            settings.className = classNameTextBox.Text;
            settings.AddMostRecentNamespace(NamespaceCombo.Text);

            settings.useSingleSubfolder = (bool)UseSingleSubfolderCheckBox.IsChecked;
            string conformedSubfolder = ClassFacilities.ConformSubfolder(HeaderSubfolderCombo.Text);
            settings.AddMostRecentHeaderSubfolder(conformedSubfolder);
            if (!settings.useSingleSubfolder)
            {
                settings.AddMostRecentImplementationSubfolder(ClassFacilities.ConformSubfolder(ImplementationSubfolderCombo.Text));
            }
            settings.headerFilename = HeaderFilename.Text;
            settings.hasImplementationFile = (bool)HasImplementationFileCheckBox.IsChecked;
            settings.implementationFilename = ImplementationFilename.Text;
            settings.createFilters = (bool)CreateFiltersCheckBox.IsChecked;
            settings.includePrecompiledHeader = (bool)IncludePrecompiledHeaderCheckBox.IsChecked;
            if (settings.includePrecompiledHeader) { 
                settings.precompiledHeader = ClassFacilities.ConformPrecompiledHeaderPath(PrecompiledHeader.Text);
            }
            else {
                settings.precompiledHeader = "";
            }

            settings.autoSaveSettings = (bool)AutosaveSettingsCheckBox.IsChecked;
        }

        private bool CheckFiles()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            EnvDTE.Project project = Utils.Solution.CurrentProject(AddCppClassPackage.dte);

            string projectPath = new FileInfo(project.FullName).DirectoryName;
            string headerPath = System.IO.Path.Combine(projectPath, HeaderSubfolderCombo.Text, HeaderFilename.Text);
            string message = "";
            if (File.Exists(headerPath))
            {
                message += "Header file already exists.";
            }
            string implementationPath = System.IO.Path.Combine(projectPath, ImplementationSubfolderCombo.Text, ImplementationFilename.Text);
            if (File.Exists(implementationPath))
            {
                message += "\nImplementation file already exists.";
            }
            bool filesOk = true;
            if (message != "")
            {
                message += "\nDo you want to overwrite file/files?";
                filesOk = VS.MessageBox.Show("Warning", message, OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_YESNO) == VSConstants.MessageBoxResult.IDYES;
            }
            if (!filesOk)
            {
                return false;
            }

            if ((bool)IncludePrecompiledHeaderCheckBox.IsChecked)
            {
                string precompiledHeaderPath = System.IO.Path.Combine(projectPath, PrecompiledHeader.Text);
                if (!File.Exists(precompiledHeaderPath))
                {
                    message = "Precompiled header at path \"" + precompiledHeaderPath + "\" doesn't exist. Proceed anyway?";
                    return VS.MessageBox.Show("Warning", message, OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_YESNO) == VSConstants.MessageBoxResult.IDYES;
                }
            }

            return true;
        }

        private void AddClassButtonClicked(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            bool result = CheckFiles();
            if (!result)
            {
                return;
            }

            SaveSettings();
            try
            {
                ClassAdder.AddClass(settings);
            }
            catch (Exception ex)
            {
                var errorMessage = "Oops! An error occured while adding class, error message: " + ex.Message + ". Please fix the error and commit it to AddCppClass project on GitHub)";
                bool closeDialog = VS.MessageBox.Show("Error", errorMessage, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OKCANCEL) == VSConstants.MessageBoxResult.IDCANCEL;
                if (closeDialog)
                {
                    DialogResult = false;
                    Close();
                }
                else
                {
                    return;
                }
            }
            DialogResult = true;
            Close();
        }

        public bool ShouldSaveSettings()
        {
            return settings.autoSaveSettings;
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
                ImplementationSubfolderCombo.Text = HeaderSubfolderCombo.Text;
            }
            else
            {
                if (ClassFacilities.IsValidSubfolder(ImplementationSubfolderCombo.Text))
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
                if (ClassFacilities.IsValidSubfolder(HeaderSubfolderCombo.Text))
                {
                    RemoveError(HeaderSubfolderCombo);
                }
                else
                {
                    AddError(HeaderSubfolderCombo, "Header subfolder is invalid");
                }
                if (ClassFacilities.IsValidSubfolder(ImplementationSubfolderCombo.Text))
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

            if (previousPos < 0 || text[previousPos] != '\\' && text[previousPos] != '.')
            {
                if (nextPos == -1 || (text[nextPos] != '\\'))
                {
                    return true;
                }
            }

            return false;
        }

        private void HasImplementationFileCheckChanged(object sender, EventArgs e)
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
                if (!ClassFacilities.IsValidFilename(ImplementationFilename.Text))
                {
                    AddError(ImplementationFilename, "Implementation file name is invalid");
                }
                if (!ClassFacilities.IsValidSubfolder(ImplementationSubfolderCombo.Text))
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
                if (ClassFacilities.IsValidPrecompiledHeaderPath(PrecompiledHeader.Text))
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
                if (ClassFacilities.IsValidPrecompiledHeaderPath(textBox.Text))
                {
                    RemoveError(sender);
                }
                else
                {
                    AddError(sender, "Precompiled header file name is invalid");
                }
            }
        }

        private void ShowInfoMessage(string caption, string text)
        {
            _ = VS.MessageBox.Show(caption, text, OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK);
        }

        private void UseSingleFolderInfoButtonDown(object sender, EventArgs e)
        {
            ShowInfoMessage("Use single subfolder", "If this checkbox is checked implementation file would be created in the same folder as header file");
        }

        private void CreateFiltersInfoButtonDown(object sender, EventArgs e)
        {
            ShowInfoMessage("Create filters", "If this checkbox is checked filters with corresponding files would be added to the project. For example, file with path \"MyFolder/header.h\" will added to filter \"MyFolder\".");
        }

        private void HeaderExtensionInfoButtonDown(object sender, EventArgs e)
        {
            ShowInfoMessage("Header extension", "A file extension for the header. Should be started with the dot '.'.");
        }
    }
}
