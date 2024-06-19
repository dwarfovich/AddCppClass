using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using AddCppClass;
using EnvDTE;

namespace Dwarfovich.AddCppClass
{
    public partial class AddCppClassDialog : DialogWindow
    {
        public Settings settings { get; private set; } = new();

        private ClassFacilities classFacilities = new();
        private ClassSettingsErrorsCollection errors = new();
        private readonly string title = "Add C++ class";
        private readonly string defaultclassName = "MyClass";
        private readonly string errorMessageBeginning = "Error message: ";

        private bool headerFilterValidated = false;
        private bool implementationFilterValidated = false;
        private bool updatingFilterCheckBoxes = false;

        public AddCppClassDialog()
        {
            InitializeGui();
        }

        public AddCppClassDialog(Settings settings)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            InitializeGui();
            LoadSettings(settings);
        }

        private void InitializeGui()
        {
            InitializeComponent();
            HeaderFilter.IsEnabledChanged += new(HeaderFilterEnabledChanged);
            ImplementationFilter.IsEnabledChanged += new(ImplementationFilterEnabledChanged);
            Title = title;
            classNameTextBox.Text = defaultclassName;
        }

        private void LoadSettings(Settings settings)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.settings = settings;

            switch (settings.filenameStyle)
            {
                case FilenameStyle.CamelCase: CamelCaseNameStyle.IsChecked = true; break;
                case FilenameStyle.SnakeCase: SnakeCaseNameStyle.IsChecked = true; break;
                case FilenameStyle.LowerCase: LowerCaseNameStyle.IsChecked = true; break;
                default: CamelCaseNameStyle.IsChecked = true; break;
            }

            PopulateComboBox(NamespaceCombo, settings.recentNamespaces, settings.maxRecentNamespaces);
            if (ClassFacilities.ClassExists(NamespaceCombo.Text, classNameTextBox.Text))
            {
                AddError(ErrorType.ClassExists);
            }
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
            if (settings.includeGuardStyle == IncludeGuard.PragmaOnce)
            {
                PragmaOnceGuardStyle.IsChecked = true;
            }
            else
            {
                IfndefGuardStyle.IsChecked = true;
            }

            PopulateComboBox(HeaderFilter, settings.recentHeaderFilters, settings.maxRecentHeaderFilters);
            PopulateComboBox(ImplementationFilter, settings.recentImplementationFilters, settings.maxRecentImplementationFilters);
            CreateFiltersCheckBox.IsChecked = settings.createFilters;
            UseSubfolderAsFilterCheckBox.IsChecked = settings.useSubfoldersAsFilters;
            UseSingleFilterCheckBox.IsChecked = settings.useSingleFilter;

            AutoSaveSettingsCheckBox.IsChecked = settings.autoSaveSettings;
        }

        private void SaveSettings()
        {
            settings.className = classNameTextBox.Text;
            settings.SetNamespace(NamespaceCombo.Text);

            settings.useSingleSubfolder = (bool)UseSingleSubfolderCheckBox.IsChecked;
            settings.SetHeaderSubfolder(ClassFacilities.ConformSubfolder(HeaderSubfolderCombo.Text));
            settings.SetImplementationSubfolder(ClassFacilities.ConformSubfolder(ImplementationSubfolderCombo.Text));
            settings.headerFilename = HeaderFilename.Text;
            settings.hasImplementationFile = (bool)HasImplementationFileCheckBox.IsChecked;
            settings.implementationFilename = ImplementationFilename.Text;
            settings.createFilters = (bool)CreateFiltersCheckBox.IsChecked;
            settings.includePrecompiledHeader = (bool)IncludePrecompiledHeaderCheckBox.IsChecked;
            if (settings.includePrecompiledHeader)
            {
                settings.precompiledHeader = ClassFacilities.ConformPrecompiledHeaderPath(PrecompiledHeader.Text);
            }
            else
            {
                settings.precompiledHeader = "";
            }
            SaveIncludeGuardStyleSettings();

            settings.createFilters = (bool)CreateFiltersCheckBox.IsChecked;
            settings.useSubfoldersAsFilters = (bool)UseSubfolderAsFilterCheckBox.IsChecked;
            settings.useSingleFilter = (bool)UseSingleFilterCheckBox.IsChecked;
            settings.SetHeaderFilter(HeaderFilter.Text);
            settings.SetImplementationFilter(ImplementationFilter.Text);

            settings.autoSaveSettings = (bool)AutoSaveSettingsCheckBox.IsChecked;
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
            else if (spareValue != null)
            {
                comboBox.Items.Add(spareValue);
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

        private void SaveFilenameStyleSettings(RadioButton button)
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

        private void SaveIncludeGuardStyleSettings()
        {
            if ((bool)PragmaOnceGuardStyle.IsChecked)
            {
                settings.includeGuardStyle = IncludeGuard.PragmaOnce;
            }
            else
            {
                settings.includeGuardStyle = IncludeGuard.Ifndef;
            }
        }

        private void FilenameStyleRadioButtonChecked(object sender, EventArgs e)
        {
            RadioButton button = sender as RadioButton;
            if (button != null && (bool)button.IsChecked)
            {
                SaveFilenameStyleSettings(button);
                (settings.headerFilename, settings.implementationFilename) = classFacilities.GenerateFilenames(settings);
                UpdateFilenameTextBoxes();
            }
        }

        private void RemoveError(ErrorType type)
        {
            bool hasOtherErrors = errors.RemoveError(type);
            if (AddClassButton != null)
            {
                AddClassButton.IsEnabled = !hasOtherErrors;
                SetErrorMessage(errors.MessageForLastError());
            }
        }

        private void SetErrorMessage(string message)
        {
            if (ErrorMessage != null)
            {
                ErrorMessage.Content = errorMessageBeginning + message;
            }
        }
        private void AddError(ErrorType type)
        {
            bool added = errors.AddError(type);
            if (added)
            {
                SetErrorMessage(errors.MessageForLastError());
                if (AddClassButton != null)
                {
                    AddClassButton.IsEnabled = false;
                }
            }
        }
        private void ClassNameChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            TextBox textBox = sender as TextBox;
            if (textBox == null)
            {
                throw new InvalidCastException("Sender should be a TextBox");
            }

            if (textBox != null)
            {
                if (ClassFacilities.IsValidClassName(textBox.Text))
                {
                    settings.className = textBox.Text;
                    (settings.headerFilename, settings.implementationFilename) = classFacilities.GenerateFilenames(settings);
                    UpdateFilenameTextBoxes();
                    RemoveError(ErrorType.InvalidClassName);
                    if (ClassFacilities.ClassExists(NamespaceCombo.Text, classNameTextBox.Text))
                    {
                        AddError(ErrorType.ClassExists);
                    }
                    else
                    {
                        RemoveError(ErrorType.ClassExists);
                    }
                }
                else
                {
                    AddError(ErrorType.InvalidClassName);
                }
            }
        }

        private void NamespaceChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ComboBox comboBox = sender as ComboBox;
            if (comboBox == null)
            {
                throw new InvalidCastException("Sender should be a ComboBox");
            }
            TextBox textBox = (TextBox)comboBox.Template.FindName("PART_EditableTextBox", comboBox);
            if (textBox != null)
            {
                if (ClassFacilities.IsValidNamespace(textBox.Text))
                {
                    RemoveError(ErrorType.InvalidNamespace);
                    if (ClassFacilities.ClassExists(textBox.Text, classNameTextBox.Text))
                    {
                        AddError(ErrorType.ClassExists);
                    }
                    else
                    {
                        RemoveError(ErrorType.ClassExists);
                    }
                }
                else
                {
                    AddError(ErrorType.InvalidNamespace);
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

            if (ClassFacilities.IsValidSubfolder(comboBox.Text))
            {
                RemoveError(ErrorType.InvalidHeaderSubfolder);
                if ((bool)UseSingleSubfolderCheckBox.IsChecked)
                {
                    ImplementationSubfolderCombo.Text = comboBox.Text;
                }
            }
            else
            {
                AddError(ErrorType.InvalidHeaderSubfolder);
            }

            UpdateFilters();
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
                RemoveError(ErrorType.InvalidImplementationSubfolder);
            }
            else
            {
                AddError(ErrorType.InvalidImplementationSubfolder);
            }

            if ((bool)CreateFiltersCheckBox.IsChecked && (bool)UseSubfolderAsFilterCheckBox.IsChecked)
            {
                if (!(bool)UseSingleFilterCheckBox.IsChecked)
                {
                    ImplementationFilter.Text = comboBox.Text;
                }
            }
            UpdateImplementationFilter();
        }

        private void HeaderExtensionChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox is null)
            {
                return;
            }

            if (ClassFacilities.IsValidExtension(comboBox.Text))
            {
                RemoveError(ErrorType.InvalidHeaderExtension);
                settings.SetHeaderExtension(comboBox.Text);
                (settings.headerFilename, settings.implementationFilename) = classFacilities.GenerateFilenamesForChangedExtension(settings);
                UpdateFilenameTextBoxes();
            }
            else
            {
                AddError(ErrorType.InvalidHeaderExtension);
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
                RemoveError(ErrorType.InvalidHeaderFilename);
            }
            else
            {
                AddError(ErrorType.InvalidHeaderFilename);
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
                RemoveError(ErrorType.InvalidImplementationFilename);
            }
            else
            {
                AddError(ErrorType.InvalidImplementationFilename);
            }
        }

        private void PrecompiledHeaderFilenameChangedEventHandler(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                if (ClassFacilities.IsValidPrecompiledHeaderPath(textBox.Text))
                {
                    RemoveError(ErrorType.InvalidPrecompiledHeader);
                }
                else
                {
                    AddError(ErrorType.InvalidPrecompiledHeader);
                }
            }
        }
        private bool CheckFiles()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            EnvDTE.Project project = Utils.Solution.CurrentProject();

            string projectPath = new FileInfo(project.FullName).DirectoryName;
            string headerPath = Path.Combine(projectPath, HeaderSubfolderCombo.Text, HeaderFilename.Text);
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
                string precompiledHeaderPath = Path.Combine(projectPath, PrecompiledHeader.Text);
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

            bool classExists = ClassFacilities.ClassExists(NamespaceCombo.Text, classNameTextBox.Text);
            if (classExists)
            {
                VS.MessageBox.Show("Error", "Class already exists", OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
                return;
            }

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

        public bool ShouldSaveSettingsToFile()
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

            ImplementationSubfolderCombo.IsEnabled = !(bool)checkBox.IsChecked;
            SelectImplementationSubfolderButton.IsEnabled = !(bool)checkBox.IsChecked;
            if ((bool)checkBox.IsChecked)
            {
                RemoveError(ErrorType.InvalidHeaderSubfolder);
                ImplementationSubfolderCombo.Text = HeaderSubfolderCombo.Text;
            }
            else
            {
                if (ClassFacilities.IsValidSubfolder(ImplementationSubfolderCombo.Text))
                {
                    RemoveError(ErrorType.InvalidImplementationSubfolder);
                }
                else
                {
                    AddError(ErrorType.InvalidImplementationSubfolder);
                }
            }
        }

        private void HasImplementationFileCheckChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox is null)
            {
                return;
            }

            ImplementationFilename.IsEnabled = (bool)checkBox.IsChecked;
            ImplementationSubfolderCombo.IsEnabled = (bool)checkBox.IsChecked;

            if ((bool)checkBox.IsChecked)
            {
                if (!ClassFacilities.IsValidFilename(ImplementationFilename.Text))
                {
                    AddError(ErrorType.InvalidImplementationFilename);
                }
                if (!ClassFacilities.IsValidSubfolder(ImplementationSubfolderCombo.Text))
                {
                    AddError(ErrorType.InvalidImplementationSubfolder);
                }
            }
            else
            {
                RemoveError(ErrorType.InvalidImplementationFilename);
                RemoveError(ErrorType.InvalidImplementationSubfolder);
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
                    RemoveError(ErrorType.InvalidPrecompiledHeader);
                }
                else
                {
                    AddError(ErrorType.InvalidPrecompiledHeader);
                }
            }
            else
            {
                PrecompiledHeader.IsEnabled = false;
                RemoveError(ErrorType.InvalidPrecompiledHeader);
            }
        }

        private void HeaderFilterEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            headerFilterValidated = false;
            UpdateFilters();
        }

        private void ImplementationFilterEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            implementationFilterValidated = false;
            UpdateImplementationFilter();
        }
        private void OnAnyFilterCheckBoxChanged(object sender, EventArgs e)
        {
            if (updatingFilterCheckBoxes)
            {
                return;
            }
            updatingFilterCheckBoxes = true;
            bool createFilters = (bool)CreateFiltersCheckBox.IsChecked;
            UseSubfolderAsFilterCheckBox.IsEnabled = createFilters;
            UseSingleFilterCheckBox.IsEnabled = createFilters;
            HeaderFilter.IsEnabled = createFilters && !(bool)UseSubfolderAsFilterCheckBox.IsChecked;
            ImplementationFilter.IsEnabled = createFilters && !(bool)UseSubfolderAsFilterCheckBox.IsChecked
                                                       && !(bool)UseSingleFilterCheckBox.IsChecked;
            if (createFilters)
            {
                UpdateFilters();
            }
            else
            {
                RemoveError(ErrorType.InvalidHeaderFilter);
                RemoveError(ErrorType.InvalidImplementationFilter);
            }
            updatingFilterCheckBoxes = false;
        }

        private void ValidateImplementationFilterIfNeeded()
        {
            if (implementationFilterValidated)
            {
                return;
            }

            if (ClassFacilities.IsValidFilter(ImplementationFilter.Text))
            {
                RemoveError(ErrorType.InvalidImplementationFilter);
            }
            else
            {
                AddError(ErrorType.InvalidImplementationFilter);
            }
            implementationFilterValidated = true;
        }
        private void ValidateHeaderFilterIfNeeded()
        {
            if (headerFilterValidated)
            {
                return;
            }
            if (ClassFacilities.IsValidFilter(HeaderFilter.Text))
            {
                RemoveError(ErrorType.InvalidHeaderFilter);
            }
            else
            {
                AddError(ErrorType.InvalidHeaderFilter);
            }
            headerFilterValidated = true;
        }
        private void UpdateImplementationFilter()
        {
            if (!(bool)CreateFiltersCheckBox.IsChecked)
            {
                return;
            }
            if ((bool)UseSingleFilterCheckBox.IsChecked)
            {
                ImplementationFilter.Text = HeaderFilter.Text;
            }
            else if ((bool)UseSubfolderAsFilterCheckBox.IsChecked)
            {
                ImplementationFilter.Text = ImplementationSubfolderCombo.Text;
            }
            ValidateImplementationFilterIfNeeded();
        }
        private void UpdateFilters()
        {
            if (!(bool)CreateFiltersCheckBox.IsChecked)
            {
                return;
            }
            if ((bool)UseSubfolderAsFilterCheckBox.IsChecked)
            {
                HeaderFilter.Text = HeaderSubfolderCombo.Text;
            }
            ValidateHeaderFilterIfNeeded();
            UpdateImplementationFilter();
        }

        private void HeaderFilterChangedEventHandler(object sender, EventArgs e)
        {
            headerFilterValidated = false;
            ValidateHeaderFilterIfNeeded();
            UpdateImplementationFilter();
        }

        private void ImplementationFilterChangedEventHandler(object sender, EventArgs e)
        {
            implementationFilterValidated = false;
            ValidateImplementationFilterIfNeeded();
        }

        private void SelectSubfolderButtonClicked(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Button button = sender as Button;
            if (button == null)
            {
                return;
            }

            string path = Utils.Solution.CurrentProjectPath();
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                return;
            }

            if (dialog.FileName.StartsWith(path))
            {
                path = dialog.FileName.Substring(path.Length);
            }
            else
            {
                VS.MessageBox.Show("Warning", "Couldn't determine subfolder. Please ensure you are not leaving project folder", OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK);
                return;
            }
            bool isHeader = (button.Name == "SelectHeaderSubfolderButton");
            if (isHeader)
            {
                HeaderSubfolderCombo.Text = ClassFacilities.ConformSubfolder(path);
            }
            else
            {
                ImplementationSubfolderCombo.Text = ClassFacilities.ConformSubfolder(path);
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
