using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DirectoryIconTool
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string initialFolder = "";
            if (args != null && args.Length > 0 && Directory.Exists(args[0]))
            {
                initialFolder = args[0];
            }

            Application.Run(new MainForm(initialFolder));
        }
    }

    public class MainForm : Form
    {
        private TextBox txtFolderPath;
        private Button btnBrowseFolder;
        private TextBox txtIconPath;
        private Button btnBrowseIcon;
        private Button btnApply;
        private Button btnRestartExplorer;
        private Label lblStatus;
        private int selectedIconIndex = 0;

        // Checkboxes
        private CheckBox chkShowHidden;
        private CheckBox chkShowSystem;
        private CheckBox chkSetHidden;
        private CheckBox chkSetSystem;
        private CheckBox chkSetReadOnly;

        // Folder Checkboxes
        private CheckBox chkFolderReadOnly;
        private CheckBox chkFolderSystem;
        private CheckBox chkFolderHidden;

        private bool _isLoading = false;

        public MainForm(string initialFolder)
        {
            _isLoading = true;
            InitializeComponent();
            txtFolderPath.Text = initialFolder;
            LoadExplorerSettings();
            if (!string.IsNullOrEmpty(initialFolder))
            {
                LoadFolderAttributes(initialFolder);
            }
            _isLoading = false;
        }

        private void InitializeComponent()
        {
            this.Text = "Windows Directory Icon Tool (Admin)";
            this.Size = new Size(520, 580);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9F);
            this.BackColor = Color.White;

            Label lblFolder = new Label() { Text = "Step 1: Select Folder", Left = 20, Top = 20, Width = 150, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtFolderPath = new TextBox() { Left = 20, Top = 45, Width = 350, BackColor = Color.White };
            txtFolderPath.TextChanged += (s, e) => {
                if (!_isLoading && Directory.Exists(txtFolderPath.Text)) LoadFolderAttributes(txtFolderPath.Text);
            };
            btnBrowseFolder = new Button() { Text = "Browse...", Left = 380, Top = 43, Width = 80, FlatStyle = FlatStyle.System };
            btnBrowseFolder.Click += BtnBrowseFolder_Click;

            Label lblIcon = new Label() { Text = "Step 2: Select Icon Source (.ico, .dll, .exe)", Left = 20, Top = 85, Width = 300, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtIconPath = new TextBox() { Left = 20, Top = 110, Width = 350, BackColor = Color.White };
            btnBrowseIcon = new Button() { Text = "Browse...", Left = 380, Top = 108, Width = 80, FlatStyle = FlatStyle.System };
            btnBrowseIcon.Click += BtnBrowseIcon_Click;

            // System Attributes Group (Explorer Settings)
            Label lblSysAttr = new Label() { Text = "System Attributes (Explorer Settings)", Left = 20, Top = 150, Width = 460, Font = new Font("Segoe UI", 9F, FontStyle.Bold), BackColor = Color.FromArgb(240, 240, 240), Padding = new Padding(2) };
            chkShowHidden = new CheckBox() { Text = "Show Hidden Files", Left = 20, Top = 175, Width = 200 };
            chkShowSystem = new CheckBox() { Text = "Show System Files", Left = 20, Top = 200, Width = 200 };

            chkShowHidden.Click += (s, e) => SaveExplorerSettings();
            chkShowSystem.Click += (s, e) => SaveExplorerSettings();

            // Folder Attributes Group
            Label lblFolderAttr = new Label() { Text = "Folder Attributes (Target Directory)", Left = 20, Top = 235, Width = 220, Font = new Font("Segoe UI", 9F, FontStyle.Bold), BackColor = Color.FromArgb(240, 240, 240), Padding = new Padding(2) };
            chkFolderHidden = new CheckBox() { Text = "Hidden", Left = 20, Top = 260, Width = 100 };
            chkFolderSystem = new CheckBox() { Text = "System", Left = 20, Top = 285, Width = 100 };
            chkFolderReadOnly = new CheckBox() { Text = "Read-only", Left = 20, Top = 310, Width = 100 };

            chkFolderHidden.Click += (s, e) => ApplyFolderAttributesImmediately();
            chkFolderSystem.Click += (s, e) => ApplyFolderAttributesImmediately();
            chkFolderReadOnly.Click += (s, e) => ApplyFolderAttributesImmediately();

            // File Attributes Group (desktop.ini)
            Label lblFileAttr = new Label() { Text = "File Attributes (desktop.ini)", Left = 260, Top = 235, Width = 220, Font = new Font("Segoe UI", 9F, FontStyle.Bold), BackColor = Color.FromArgb(240, 240, 240), Padding = new Padding(2) };
            chkSetHidden = new CheckBox() { Text = "Set as Hidden", Left = 260, Top = 260, Width = 200 };
            chkSetSystem = new CheckBox() { Text = "Set as System", Left = 260, Top = 285, Width = 200 };
            chkSetReadOnly = new CheckBox() { Text = "Set as Read-only", Left = 260, Top = 310, Width = 200 };

            chkSetHidden.Click += (s, e) => ApplyIniAttributesImmediately();
            chkSetSystem.Click += (s, e) => ApplyIniAttributesImmediately();
            chkSetReadOnly.Click += (s, e) => ApplyIniAttributesImmediately();

            btnApply = new Button() { Text = "Write desktop.ini and Refresh Icon", Left = 20, Top = 350, Width = 460, Height = 45, FlatStyle = FlatStyle.System, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            btnApply.Click += BtnApply_Click;

            btnRestartExplorer = new Button() { Text = "Restart Windows Explorer (Force Refresh)", Left = 20, Top = 405, Width = 460, Height = 30, FlatStyle = FlatStyle.Flat };
            btnRestartExplorer.FlatAppearance.BorderSize = 1;
            btnRestartExplorer.Click += BtnRestartExplorer_Click;

            lblStatus = new Label() { Text = "Ready", Left = 20, Top = 450, Width = 460, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.Gray };

            this.Controls.Add(lblFolder);
            this.Controls.Add(txtFolderPath);
            this.Controls.Add(btnBrowseFolder);
            this.Controls.Add(lblIcon);
            this.Controls.Add(txtIconPath);
            this.Controls.Add(btnBrowseIcon);

            this.Controls.Add(lblSysAttr);
            this.Controls.Add(chkShowHidden);
            this.Controls.Add(chkShowSystem);

            this.Controls.Add(lblFolderAttr);
            this.Controls.Add(chkFolderHidden);
            this.Controls.Add(chkFolderSystem);
            this.Controls.Add(chkFolderReadOnly);

            this.Controls.Add(lblFileAttr);
            this.Controls.Add(chkSetHidden);
            this.Controls.Add(chkSetSystem);
            this.Controls.Add(chkSetReadOnly);

            this.Controls.Add(btnApply);
            this.Controls.Add(btnRestartExplorer);
            this.Controls.Add(lblStatus);
        }

        private void LoadFolderAttributes(string folderPath)
        {
            bool oldLoading = _isLoading;
            _isLoading = true;
            try
            {
                if (Directory.Exists(folderPath))
                {
                    FileAttributes fAttr = File.GetAttributes(folderPath);
                    chkFolderHidden.Checked = (fAttr & FileAttributes.Hidden) == FileAttributes.Hidden;
                    chkFolderSystem.Checked = (fAttr & FileAttributes.System) == FileAttributes.System;
                    chkFolderReadOnly.Checked = (fAttr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
                }

                string iniPath = Path.Combine(folderPath, "desktop.ini");
                if (File.Exists(iniPath))
                {
                    FileAttributes attr = File.GetAttributes(iniPath);
                    chkSetHidden.Checked = (attr & FileAttributes.Hidden) == FileAttributes.Hidden;
                    chkSetSystem.Checked = (attr & FileAttributes.System) == FileAttributes.System;
                    chkSetReadOnly.Checked = (attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;

                    ParseExistingIni(iniPath);
                }
                else
                {
                    chkSetHidden.Checked = true;
                    chkSetSystem.Checked = true;
                    chkSetReadOnly.Checked = false;
                }
            }
            catch { }
            finally { _isLoading = oldLoading; }
        }

        private void ParseExistingIni(string iniPath)
        {
            try
            {
                string[] lines = File.ReadAllLines(iniPath);
                foreach (string line in lines)
                {
                    if (line.StartsWith("IconResource=", StringComparison.OrdinalIgnoreCase))
                    {
                        string res = line.Substring(13).Trim();
                        txtIconPath.Text = res;

                        if (res.Contains(","))
                        {
                            int lastComma = res.LastIndexOf(',');
                            int.TryParse(res.Substring(lastComma + 1), out selectedIconIndex);
                        }
                        break;
                    }
                }
            }
            catch { }
        }

        private void ApplyFolderAttributesImmediately()
        {
            if (_isLoading) return;

            string folderPath = txtFolderPath.Text;
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath)) return;

            try
            {
                FileAttributes attr = File.GetAttributes(folderPath);

                if (chkFolderHidden.Checked) attr |= FileAttributes.Hidden; else attr &= ~FileAttributes.Hidden;
                if (chkFolderSystem.Checked) attr |= FileAttributes.System; else attr &= ~FileAttributes.System;
                if (chkFolderReadOnly.Checked) attr |= FileAttributes.ReadOnly; else attr &= ~FileAttributes.ReadOnly;

                File.SetAttributes(folderPath, attr);

                // Robust refresh from xToolsMenu logic
                IntPtr pathPtr = Marshal.StringToHGlobalUni(folderPath);
                try {
                    SHChangeNotify(SHCNE_ATTRIBUTES, SHCNF_PATH, pathPtr, IntPtr.Zero);
                    SHChangeNotify(SHCNE_UPDATEITEM, SHCNF_PATH, pathPtr, IntPtr.Zero);

                    string parent = Path.GetDirectoryName(folderPath);
                    if (!string.IsNullOrEmpty(parent)) {
                        IntPtr parentPtr = Marshal.StringToHGlobalUni(parent);
                        try {
                            SHChangeNotify(SHCNE_UPDATEDIR, SHCNF_PATH, parentPtr, IntPtr.Zero);
                        } finally {
                            Marshal.FreeHGlobal(parentPtr);
                        }
                    }
                } finally {
                    Marshal.FreeHGlobal(pathPtr);
                }

                lblStatus.Text = "Folder attributes updated.";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Attr Update Error: " + ex.Message;
            }
        }

        private void ApplyIniAttributesImmediately()
        {
            if (_isLoading) return;
            string folderPath = txtFolderPath.Text;
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath)) return;
            string iniPath = Path.Combine(folderPath, "desktop.ini");
            if (!File.Exists(iniPath)) return;

            try
            {
                FileAttributes attr = FileAttributes.Normal;
                if (chkSetHidden.Checked) attr |= FileAttributes.Hidden;
                if (chkSetSystem.Checked) attr |= FileAttributes.System;
                if (chkSetReadOnly.Checked) attr |= FileAttributes.ReadOnly;

                File.SetAttributes(iniPath, attr);

                IntPtr pathPtr = Marshal.StringToHGlobalUni(iniPath);
                try {
                    SHChangeNotify(SHCNE_ATTRIBUTES, SHCNF_PATH, pathPtr, IntPtr.Zero);
                    SHChangeNotify(SHCNE_UPDATEITEM, SHCNF_PATH, pathPtr, IntPtr.Zero);
                } finally {
                    Marshal.FreeHGlobal(pathPtr);
                }
                lblStatus.Text = "desktop.ini attributes updated.";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Ini Attr Error: " + ex.Message;
            }
        }

        private void LoadExplorerSettings()
        {
            bool oldLoading = _isLoading;
            _isLoading = true;
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"))
                {
                    if (key != null)
                    {
                        object hidden = key.GetValue("Hidden");
                        object superHidden = key.GetValue("ShowSuperHidden");

                        if (hidden != null) chkShowHidden.Checked = (int)hidden == 1;
                        if (superHidden != null) chkShowSystem.Checked = (int)superHidden == 1;
                    }
                }
            }
            catch { }
            finally { _isLoading = oldLoading; }
        }

        private void SaveExplorerSettings()
        {
            if (_isLoading) return;
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true))
                {
                    if (key != null)
                    {
                        key.SetValue("Hidden", chkShowHidden.Checked ? 1 : 2, RegistryValueKind.DWord);
                        key.SetValue("ShowSuperHidden", chkShowSystem.Checked ? 1 : 0, RegistryValueKind.DWord);

                        // Broadast "ShellState" change
                        IntPtr result;
                        SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, "ShellState", SMTO_ABORTIFHUNG, 5000, out result);

                        SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
                        lblStatus.Text = "Explorer settings updated.";
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Registry Error: " + ex.Message;
            }
        }

        private void BtnRestartExplorer_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("This will close and restart Windows Explorer. Any open folder windows will be closed. Continue?", "Restart Explorer", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName("explorer");
                    foreach (var p in processes)
                    {
                        p.Kill();
                        p.WaitForExit();
                    }
                    System.Diagnostics.Process.Start("explorer.exe");
                    lblStatus.Text = "Explorer restarted.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error restarting explorer: " + ex.Message);
                }
            }
        }

        private void BtnBrowseFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtFolderPath.Text = fbd.SelectedPath;
                }
            }
        }

        private void BtnBrowseIcon_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Icon Files|*.ico;*.dll;*.exe|All Files|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string ext = Path.GetExtension(ofd.FileName).ToLower();
                    if (ext == ".dll" || ext == ".exe")
                    {
                        ShowIconPicker(ofd.FileName);
                    }
                    else
                    {
                        txtIconPath.Text = ofd.FileName;
                        selectedIconIndex = 0;
                    }
                }
            }
        }

        private void ShowIconPicker(string filePath)
        {
            using (IconPickerForm picker = new IconPickerForm(filePath))
            {
                if (picker.ShowDialog() == DialogResult.OK)
                {
                    selectedIconIndex = picker.SelectedIndex;
                    txtIconPath.Text = string.Format("{0},{1}", filePath, selectedIconIndex);
                    lblStatus.Text = string.Format("Selected icon index: {0}", selectedIconIndex);
                }
            }
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFolderPath.Text) || string.IsNullOrEmpty(txtIconPath.Text))
            {
                MessageBox.Show("Please select both a folder and an icon.");
                return;
            }

            string folderPath = txtFolderPath.Text;
            string fullIconPath = txtIconPath.Text;

            try
            {
                string iconPath = fullIconPath;
                int index = selectedIconIndex;

                if (fullIconPath.Contains(","))
                {
                    int lastComma = fullIconPath.LastIndexOf(',');
                    string partBefore = fullIconPath.Substring(0, lastComma);
                    string partAfter = fullIconPath.Substring(lastComma + 1);

                    int parsedIndex;
                    if (int.TryParse(partAfter, out parsedIndex))
                    {
                        iconPath = partBefore;
                        index = parsedIndex;
                    }
                }

                if (!Directory.Exists(folderPath))
                {
                    MessageBox.Show("Target folder does not exist.");
                    return;
                }

                lblStatus.Text = "Checking Known Folders...";
                KnownFolderRegistry.ApplyToKnownFolderIfMatch(folderPath, iconPath, index);

                string iniPath = Path.Combine(folderPath, "desktop.ini");
                lblStatus.Text = "Preparing desktop.ini...";

                if (File.Exists(iniPath))
                {
                    File.SetAttributes(iniPath, FileAttributes.Normal);
                }

                string[] lines = {
                    "[.ShellClassInfo]",
                    string.Format("IconResource={0},{1}", iconPath, index),
                    "IconIndex=" + index
                };

                File.WriteAllLines(iniPath, lines);

                FileAttributes iniAttrs = FileAttributes.Normal;
                if (chkSetHidden.Checked) iniAttrs |= FileAttributes.Hidden;
                if (chkSetSystem.Checked) iniAttrs |= FileAttributes.System;
                if (chkSetReadOnly.Checked) iniAttrs |= FileAttributes.ReadOnly;
                File.SetAttributes(iniPath, iniAttrs);

                lblStatus.Text = "Applying folder attributes...";

                FileAttributes folderAttrs = File.GetAttributes(folderPath);

                // desktop.ini requires ReadOnly OR System on the folder to be parsed
                if (!chkFolderReadOnly.Checked && !chkFolderSystem.Checked)
                {
                    folderAttrs |= FileAttributes.ReadOnly;
                }

                File.SetAttributes(folderPath, folderAttrs);

                lblStatus.Text = "Notifying Windows Shell...";

                // Final targeted notify
                IntPtr pathPtr = Marshal.StringToHGlobalUni(folderPath);
                try {
                    SHChangeNotify(SHCNE_UPDATEITEM, SHCNF_PATH, pathPtr, IntPtr.Zero);
                } finally {
                    Marshal.FreeHGlobal(pathPtr);
                }

                MessageBox.Show("Icon applied successfully!", "Success");
                lblStatus.Text = "Applied successfully.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message + "\n\nMake sure to run as Administrator.");
                lblStatus.Text = "Failed.";
            }
        }

        private const int SHCNE_ATTRIBUTES = 0x00000800;
        private const int SHCNE_UPDATEITEM = 0x00002000;
        private const int SHCNE_ASSOCCHANGED = 0x08000000;
        private const int SHCNE_UPDATEDIR = 0x00001000;
        private const int SHCNF_IDLIST = 0x0000;
        private const int SHCNF_PATH = 0x0005;
        private const uint WM_SETTINGCHANGE = 0x001A;
        private const uint SMTO_ABORTIFHUNG = 0x0002;
        private static readonly IntPtr HWND_BROADCAST = new IntPtr(0xffff);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);
    }

    public static class KnownFolderRegistry
    {
        private const string FolderDescriptionsPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FolderDescriptions";

        // Comprehensive list of common Known Folder GUIDs
        private static readonly string[] KnownFolderGuids = {
            "{1777F761-68AD-4D8A-87BD-30B759FA33DD}", // Favorites
            "{4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4}", // Saved Games
            "{FDD39AD0-B39B-4249-939E-0291D7430030}", // Documents
            "{374DE290-123F-4565-9164-39C4925E467B}", // Downloads
            "{33E28130-4E1E-4676-835A-98395C3BC3BB}", // Pictures
            "{4BD8D571-6D19-48D3-BE97-422220080E43}", // Music
            "{18989B1D-99B5-455B-841C-AB7C74E4DDFC}", // Videos
            "{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}", // Desktop
            "{56784D92-A512-4DF5-8C0D-673977E1F010}", // Contacts
            "{7D1D3A04-DEBB-4115-95C0-2F7364091478}", // Links
            "{BF503916-0182-4467-AC95-0315736DBA7C}", // Saved Searches
            "{6D809377-6AF0-444B-8957-A3773F02200E}", // Program Files
            "{7C5A6935-D392-4E14-87A4-01E3150567C7}", // Program Files (x86)
            "{F38BF404-1D43-42F2-9305-67DE0B28FC23}", // Windows
            "{D65231B0-B2F1-4857-A4CE-A8E7C6EA7D27}"  // System32
        };

        public static void ApplyToKnownFolderIfMatch(string folderPath, string iconPath, int index)
        {
            string targetPath = folderPath.TrimEnd('\\');

            foreach (string guidStr in KnownFolderGuids)
            {
                string knownPath = GetKnownFolderPath(new Guid(guidStr));
                if (!string.IsNullOrEmpty(knownPath) &&
                    string.Equals(targetPath, knownPath.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
                {
                    SetKnownFolderIcon(guidStr, iconPath, index);
                    // Continue loop in case folder represents multiple GUIDs (rare but possible)
                }
            }
        }

        private static void SetKnownFolderIcon(string guid, string iconPath, int index)
        {
            try
            {
                using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    string subKeyPath = string.Format(@"{0}\{1}", FolderDescriptionsPath, guid);
                    using (RegistryKey key = hklm.OpenSubKey(subKeyPath, true))
                    {
                        if (key != null)
                        {
                            string iconValue = string.Format("{0},{1}", iconPath, index);
                            key.SetValue("Icon", iconValue, RegistryValueKind.ExpandString);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Registry Update Failed: " + ex.Message);
            }
        }

        [DllImport("shell32.dll")]
        private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pszPath);

        private static string GetKnownFolderPath(Guid guid)
        {
            IntPtr pathPtr;
            int result = SHGetKnownFolderPath(guid, 0, IntPtr.Zero, out pathPtr);
            if (result == 0)
            {
                string path = Marshal.PtrToStringUni(pathPtr);
                Marshal.FreeCoTaskMem(pathPtr);
                return path;
            }
            return string.Empty;
        }
    }

    public class IconPickerForm : Form
    {
        private int _selectedIndex = 0;
        public int SelectedIndex { get { return _selectedIndex; } private set { _selectedIndex = value; } }
        private ListView listView;
        private string filePath;

        public IconPickerForm(string filePath)
        {
            this.filePath = filePath;
            InitializeComponent();
            LoadIcons();
        }

        private void InitializeComponent()
        {
            this.Text = "Select Icon from " + Path.GetFileName(filePath);
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9F);
            this.BackColor = Color.White;

            listView = new ListView();
            listView.Dock = DockStyle.Fill;
            listView.View = View.LargeIcon;
            listView.MultiSelect = false;
            listView.BackColor = Color.White;
            listView.BorderStyle = BorderStyle.None;
            listView.DoubleClick += (s, e) => { ConfirmSelection(); };

            Panel bottomPanel = new Panel() { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(10) };
            Button btnOk = new Button() { Text = "Select Icon", Dock = DockStyle.Right, Width = 120, FlatStyle = FlatStyle.System, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnOk.Click += (s, e) => { ConfirmSelection(); };

            Button btnCancel = new Button() { Text = "Cancel", Dock = DockStyle.Left, Width = 100, FlatStyle = FlatStyle.System };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            bottomPanel.Controls.Add(btnOk);
            bottomPanel.Controls.Add(btnCancel);

            this.Controls.Add(listView);
            this.Controls.Add(bottomPanel);
        }

        private void LoadIcons()
        {
            ImageList imageList = new ImageList();
            imageList.ImageSize = new Size(32, 32);
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            listView.LargeImageList = imageList;

            int iconCount = ShellIconHelper.GetIconCount(filePath);
            int limit = Math.Min(iconCount, 500);

            for (int i = 0; i < limit; i++)
            {
                IntPtr hIcon = ShellIconHelper.ExtractIcon(filePath, i);
                if (hIcon != IntPtr.Zero)
                {
                    using (Icon icon = Icon.FromHandle(hIcon))
                    {
                        imageList.Images.Add(icon.ToBitmap());
                        ListViewItem item = new ListViewItem(i.ToString(), i);
                        listView.Items.Add(item);
                    }
                    ShellIconHelper.DestroyIcon(hIcon);
                }
            }
        }

        private void ConfirmSelection()
        {
            if (listView.SelectedItems.Count > 0)
            {
                SelectedIndex = listView.SelectedItems[0].Index;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please select an icon.");
            }
        }
    }

    public static class ShellIconHelper
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern int ExtractIconEx(string lpszFile, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, int nIcons);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyIcon(IntPtr hIcon);

        public static int GetIconCount(string filePath)
        {
            return ExtractIconEx(filePath, -1, null, null, 0);
        }

        public static IntPtr ExtractIcon(string filePath, int index)
        {
            IntPtr[] largeIcons = new IntPtr[1];
            ExtractIconEx(filePath, index, largeIcons, null, 1);
            return largeIcons[0];
        }
    }
}
