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
        private readonly string defaultClassName = "MyClass";

        private bool headerFilterValidated = false;
        private bool cppFilterValidated = false;
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
            CppFilter.IsEnabledChanged += new(CppFilterEnabledChanged);
            Title = title;
            classNameTextBox.Text = defaultClassName;
        }

        private void LoadSettings(Settings settings)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.settings = settings;

            switch (settings.filenameStyle)
            {
                case FilenameStyle.PascalCase: PascalCaseNameStyle.IsChecked = true; break;
                case FilenameStyle.SnakeCase: SnakeCaseNameStyle.IsChecked = true; break;
                case FilenameStyle.LowerCase: LowerCaseNameStyle.IsChecked = true; break;
                case FilenameStyle.Custom: CustomNameStyle.IsChecked = true; break;
                default: PascalCaseNameStyle.IsChecked = true; break;
            }

            PopulateComboBox(NamespaceCombo, settings.recentNamespaces, settings.maxRecentNamespaces);
            if (ClassFacilities.ClassExists(NamespaceCombo.Text, classNameTextBox.Text))
            {
                AddError(ErrorType.ClassExists);
            }
            PopulateComboBox(HeaderExtensionCombo, settings.recentHeaderExtensions, settings.maxRecentHeaderExtensions, ".h");

            CreateFiltersCheckBox.IsChecked = settings.createFilters;
            CreateCppFileCheckBox.IsChecked = settings.createCppFile;

            PopulateComboBox(HeaderSubfolderCombo, settings.recentHeaderSubfolders, settings.maxRecentHeaderSubfolders);
            PopulateComboBox(CppSubfolderCombo, settings.recentCppSubfolders, settings.maxRecentCppSubfolders);
            UseSingleSubfolderCheckBox.IsChecked = settings.useSingleSubfolder;
            if (settings.useSingleSubfolder)
            {
                CppSubfolderCombo.SelectedItem = HeaderSubfolderCombo.Text;
                CppSubfolderCombo.IsEnabled = false;
            }

            IncludePrecompiledHeaderCheckBox.IsChecked = settings.includePrecompiledHeader;

            PrecompiledHeader.Text = settings.precompiledHeader;
            if (!settings.includePrecompiledHeader)
            {
                PrecompiledHeader.IsEnabled = false;
            }

            if (settings.includeGuardStyle == IncludeGuardStyle.PragmaOnce)
            {
                PragmaOnceGuardStyle.IsChecked = true;
            }
            else if (settings.includeGuardStyle == IncludeGuardStyle.Ifndef)
            {
                IfndefGuardStyle.IsChecked = true;
            }
            else if (settings.includeGuardStyle == IncludeGuardStyle.IfndefGuid)
            {
                IfndefGuidGuardStyle.IsChecked = true;
            }
            else
            {
                NoneGuardStyle.IsChecked = true;
            }

            PopulateComboBox(HeaderFilter, settings.recentHeaderFilters, settings.maxRecentHeaderFilters);
            PopulateComboBox(CppFilter, settings.recentCppFilters, settings.maxRecentCppFilters);
            CreateFiltersCheckBox.IsChecked = settings.createFilters;
            UseSubfolderAsFilterCheckBox.IsChecked = settings.useSubfoldersAsFilters;
            UseSingleFilterCheckBox.IsChecked = settings.useSingleFilter;

            UseHeaderRoot.IsChecked = settings.useHeaderRoot;
            UseCppRoot.IsChecked = settings.useCppRoot;
            HeaderRoot.IsEnabled = (bool)UseHeaderRoot.IsChecked;
            CppRoot.IsEnabled = (bool)UseCppRoot.IsChecked;
            HeaderRoot.Text = settings.headerRoot;
            CppRoot.Text = settings.cppRoot;

            AutoSaveSettingsCheckBox.IsChecked = settings.autoSaveSettings;
        }

        private void SaveSettings()
        {
            settings.className = classNameTextBox.Text;
            settings.SetNamespace(NamespaceCombo.Text);

            settings.useSingleSubfolder = (bool)UseSingleSubfolderCheckBox.IsChecked;
            settings.SetHeaderSubfolder(ClassFacilities.ConformSubfolder(HeaderSubfolderCombo.Text));
            settings.SetCppSubfolder(ClassFacilities.ConformSubfolder(CppSubfolderCombo.Text));
            settings.headerFilename = HeaderFilename.Text;
            settings.createCppFile = (bool)CreateCppFileCheckBox.IsChecked;
            settings.cppFilename = CppFilename.Text;
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
            settings.SetCppFilter(CppFilter.Text);

            string headerFilter = HeaderFilter.Text;
            var filterTokens = headerFilter.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string headerFolder = ClassFacilities.ConformSubfolder(HeaderSubfolderCombo.Text);
            var folderTokens = headerFolder.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (filterTokens.Length > 0)
            {
                string currentFilter = filterTokens[0];
                string currentFolder = "";

                settings.filterToFolder[currentFilter] = currentFolder;

                for (int i = 1; i < filterTokens.Length; ++i)
                {
                    currentFilter += "/" + filterTokens[i];

                    int folderIndex = i - 1;

                    if (folderIndex < folderTokens.Length)
                    {
                        if (string.IsNullOrEmpty(currentFolder))
                            currentFolder = folderTokens[folderIndex];
                        else
                            currentFolder += "/" + folderTokens[folderIndex];
                    }

                    settings.filterToFolder[currentFilter] = currentFolder;
                }
            }

            settings.useHeaderRoot = (bool)UseHeaderRoot.IsChecked;
            settings.useCppRoot = (bool)UseCppRoot.IsChecked;
            if (settings.useHeaderRoot)
            {
                settings.headerRoot = HeaderRoot.Text; // TODO: conform folder
            }
            if (settings.useCppRoot)
            {
                settings.cppRoot = CppRoot.Text; // TODO: conform folder
            }
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
            if (CppFilename is not null)
            {
                CppFilename.Text = settings.cppFilename;
            }
        }

        private void SaveFilenameStyleSettings(RadioButton button)
        {
            if (button.Content.ToString() == "PascalCase")
            {
                settings.filenameStyle = FilenameStyle.PascalCase;
            }
            else if (button.Content.ToString() == "snake__case")
            {
                settings.filenameStyle = FilenameStyle.SnakeCase;
            }
            else if (button.Content.ToString() == "lowercase")
            {
                settings.filenameStyle = FilenameStyle.LowerCase;
            }
            else
            {
                settings.filenameStyle = FilenameStyle.Custom;
            }
        }

        private void SaveIncludeGuardStyleSettings()
        {
            if ((bool)PragmaOnceGuardStyle.IsChecked)
            {
                settings.includeGuardStyle = IncludeGuardStyle.PragmaOnce;
            }
            else if ((bool)IfndefGuardStyle.IsChecked)
            {
                settings.includeGuardStyle = IncludeGuardStyle.Ifndef;
            }
            else if ((bool)IfndefGuidGuardStyle.IsChecked)
            {
                settings.includeGuardStyle = IncludeGuardStyle.IfndefGuid;
            }
            else if ((bool)NoneGuardStyle.IsChecked)
            {
                settings.includeGuardStyle = IncludeGuardStyle.None;
            }
        }

        private void FilenameStyleRadioButtonChecked(object sender, EventArgs e)
        {
            RadioButton button = sender as RadioButton;
            if (button != null && (bool)button.IsChecked)
            {
                SaveFilenameStyleSettings(button);
                (settings.headerFilename, settings.cppFilename) = classFacilities.GenerateFilenames(settings);
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
                ErrorMessage.Text = message;
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

            if (ClassFacilities.IsValidClassName(textBox.Text))
            {
                settings.className = textBox.Text;
                if (settings.filenameStyle != FilenameStyle.Custom)
                {
                    (settings.headerFilename, settings.cppFilename) = classFacilities.GenerateFilenames(settings);
                    UpdateFilenameTextBoxes();
                }
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
                    CppSubfolderCombo.Text = comboBox.Text;
                }
            }
            else
            {
                AddError(ErrorType.InvalidHeaderSubfolder);
            }

            UpdateFilters();
        }

        private void CppSubfolderChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox is null)
            {
                return;
            }

            if (ClassFacilities.IsValidSubfolder(comboBox.Text))
            {
                RemoveError(ErrorType.InvalidCppSubfolder);
            }
            else
            {
                AddError(ErrorType.InvalidCppSubfolder);
            }

            if ((bool)CreateFiltersCheckBox.IsChecked && (bool)UseSubfolderAsFilterCheckBox.IsChecked)
            {
                if (!(bool)UseSingleFilterCheckBox.IsChecked)
                {
                    CppFilter.Text = comboBox.Text;
                }
            }
            UpdateCppFilter();
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
                (settings.headerFilename, settings.cppFilename) = classFacilities.GenerateFilenamesForChangedExtension(settings);
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
                if (settings.filenameStyle == FilenameStyle.Custom)
                {
                    settings.headerFilename = textBox.Text;
                }
                RemoveError(ErrorType.InvalidHeaderFilename);
            }
            else
            {
                AddError(ErrorType.InvalidHeaderFilename);
            }
        }

        private void CppFilenameChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            TextBox textBox = sender as TextBox;
            if (textBox is null)
            {
                return;
            }

            if (ClassFacilities.IsValidFilename(textBox.Text))
            {
                if (settings.filenameStyle == FilenameStyle.Custom)
                {
                    settings.cppFilename = textBox.Text;
                }
                RemoveError(ErrorType.InvalidCppFilename);
            }
            else
            {
                AddError(ErrorType.InvalidCppFilename);
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
            string cppPath = System.IO.Path.Combine(projectPath, CppSubfolderCombo.Text, CppFilename.Text);
            if (File.Exists(cppPath))
            {
                message += "\nSource file already exists.";
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
                var errorMessage = "Oops! An error occured while adding class, error message: " + ex.Message + ". Please fix the error and commit to AddCppClass project on GitHub)";
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

            CppSubfolderCombo.IsEnabled = !(bool)checkBox.IsChecked;
            SelectCppSubfolderButton.IsEnabled = !(bool)checkBox.IsChecked;
            if ((bool)checkBox.IsChecked)
            {
                RemoveError(ErrorType.InvalidHeaderSubfolder);
                CppSubfolderCombo.Text = HeaderSubfolderCombo.Text;
            }
            else
            {
                if (ClassFacilities.IsValidSubfolder(CppSubfolderCombo.Text))
                {
                    RemoveError(ErrorType.InvalidCppSubfolder);
                }
                else
                {
                    AddError(ErrorType.InvalidCppSubfolder);
                }
            }
        }

        private void CreateCppFileCheckChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox is null)
            {
                return;
            }

            CppFilename.IsEnabled = (bool)checkBox.IsChecked;
            CppSubfolderCombo.IsEnabled = (bool)checkBox.IsChecked;

            if ((bool)checkBox.IsChecked)
            {
                if (!ClassFacilities.IsValidFilename(CppFilename.Text))
                {
                    AddError(ErrorType.InvalidCppFilename);
                }
                if (!ClassFacilities.IsValidSubfolder(CppSubfolderCombo.Text))
                {
                    AddError(ErrorType.InvalidCppSubfolder);
                }
            }
            else
            {
                RemoveError(ErrorType.InvalidCppFilename);
                RemoveError(ErrorType.InvalidCppSubfolder);
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

        private void CppFilterEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            cppFilterValidated = false;
            UpdateCppFilter();
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
            CppFilter.IsEnabled = createFilters && !(bool)UseSubfolderAsFilterCheckBox.IsChecked
                                                       && !(bool)UseSingleFilterCheckBox.IsChecked;
            if (createFilters)
            {
                UpdateFilters();
            }
            else
            {
                RemoveError(ErrorType.InvalidHeaderFilter);
                RemoveError(ErrorType.InvalidCppFilter);
            }
            updatingFilterCheckBoxes = false;
        }

        private void OnUseHeaderRootCheckBoxChecked(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox is null)
            {
                throw new ArgumentNullException(nameof(checkBox));
            }

            HeaderRoot.IsEnabled = (bool)checkBox.IsChecked;
        }

        private void OnUseCppRootCheckBoxChecked(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox is null)
            {
                throw new ArgumentNullException(nameof(checkBox));
            }

            CppRoot.IsEnabled = (bool)checkBox.IsChecked;
        }

        private void HeaderRootChanged(object sender, EventArgs e)
        {
            //CheckBox checkBox = sender as CheckBox;
            //if (checkBox is null)
            //{
            //    throw new ArgumentNullException(nameof(checkBox));
            //}

            //DedicatedHeaderFolder.IsEnabled = checkBox.IsEnabled;
        }

        private void CppRootChanged(object sender, EventArgs e)
        {
            //CheckBox checkBox = sender as CheckBox;
            //if (checkBox is null)
            //{
            //    throw new ArgumentNullException(nameof(checkBox));
            //}

            //DedicatedHeaderFolder.IsEnabled = checkBox.IsEnabled;
        }

        private void ValidateCppFilterIfNeeded()
        {
            if (cppFilterValidated)
            {
                return;
            }

            //if (ClassFacilities.IsValidFilter(CppFilter.Text))
            //{
            //    RemoveError(ErrorType.InvalidCppFilter);
            //}
            //else
            //{
            //    AddError(ErrorType.InvalidCppFilter);
            //}
            cppFilterValidated = true;
        }
        private void ValidateHeaderFilterIfNeeded()
        {
            if (headerFilterValidated)
            {
                return;
            }
            //if (ClassFacilities.IsValidFilter(HeaderFilter.Text))
            //{
            //    RemoveError(ErrorType.InvalidHeaderFilter);
            //}
            //else
            //{
            //    AddError(ErrorType.InvalidHeaderFilter);
            //}
            headerFilterValidated = true;
        }
        private void UpdateCppFilter()
        {
            if (!(bool)CreateFiltersCheckBox.IsChecked)
            {
                return;
            }
            if ((bool)UseSingleFilterCheckBox.IsChecked)
            {
                CppFilter.Text = HeaderFilter.Text;
            }
            else if ((bool)UseSubfolderAsFilterCheckBox.IsChecked)
            {
                CppFilter.Text = CppSubfolderCombo.Text;
            }
            ValidateCppFilterIfNeeded();
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
            UpdateCppFilter();
        }

        private void HeaderFilterChangedEventHandler(object sender, EventArgs e)
        {
            headerFilterValidated = false;
            ValidateHeaderFilterIfNeeded();
            UpdateCppFilter();
        }

        private void CppFilterChangedEventHandler(object sender, EventArgs e)
        {
            cppFilterValidated = false;
            ValidateCppFilterIfNeeded();
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
                CppSubfolderCombo.Text = ClassFacilities.ConformSubfolder(path);
            }
        }
        private void ShowInfoMessage(string caption, string text)
        {
            _ = VS.MessageBox.Show(caption, text, OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK);
        }

        private void UseSingleFolderInfoButtonDown(object sender, EventArgs e)
        {
            ShowInfoMessage("Use single subfolder", "If this checkbox is checked source file would be created in the same folder as header file");
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
