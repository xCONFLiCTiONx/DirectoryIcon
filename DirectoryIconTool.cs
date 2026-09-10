using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DirectoryIconTool
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
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

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Windows Directory Icon Tool";
            this.Size = new Size(500, 320);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9F);
            this.BackColor = Color.White;

            Label lblFolder = new Label() { Text = "Step 1: Select Folder", Left = 20, Top = 20, Width = 150, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtFolderPath = new TextBox() { Left = 20, Top = 45, Width = 350, ReadOnly = true, BackColor = Color.WhiteSmoke };
            btnBrowseFolder = new Button() { Text = "Browse...", Left = 380, Top = 43, Width = 80, FlatStyle = FlatStyle.System };
            btnBrowseFolder.Click += BtnBrowseFolder_Click;

            Label lblIcon = new Label() { Text = "Step 2: Select Icon Source (.ico, .dll, .exe)", Left = 20, Top = 85, Width = 300, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtIconPath = new TextBox() { Left = 20, Top = 110, Width = 350, BackColor = Color.White };
            btnBrowseIcon = new Button() { Text = "Browse...", Left = 380, Top = 108, Width = 80, FlatStyle = FlatStyle.System };
            btnBrowseIcon.Click += BtnBrowseIcon_Click;

            btnApply = new Button() { Text = "Set Icon and Refresh", Left = 20, Top = 160, Width = 440, Height = 45, FlatStyle = FlatStyle.System, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            btnApply.Click += BtnApply_Click;

            btnRestartExplorer = new Button() { Text = "Restart Windows Explorer (Force Refresh)", Left = 20, Top = 215, Width = 440, Height = 30, FlatStyle = FlatStyle.Flat };
            btnRestartExplorer.FlatAppearance.BorderSize = 1;
            btnRestartExplorer.Click += BtnRestartExplorer_Click;

            lblStatus = new Label() { Text = "Ready", Left = 20, Top = 250, Width = 440, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.Gray };

            this.Controls.Add(lblFolder);
            this.Controls.Add(txtFolderPath);
            this.Controls.Add(btnBrowseFolder);
            this.Controls.Add(lblIcon);
            this.Controls.Add(txtIconPath);
            this.Controls.Add(btnBrowseIcon);
            this.Controls.Add(btnApply);
            this.Controls.Add(btnRestartExplorer);
            this.Controls.Add(lblStatus);
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

            try
            {
                string folderPath = txtFolderPath.Text;
                string fullIconPath = txtIconPath.Text;
                string iconPath = fullIconPath;
                int index = selectedIconIndex;

                // Parse manual input like "path,index"
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

                string iniPath = Path.Combine(folderPath, "desktop.ini");

                // Remove attributes if exists to overwrite
                if (File.Exists(iniPath))
                {
                    File.SetAttributes(iniPath, FileAttributes.Normal);
                }

                string[] lines = {
                    "[.ShellClassInfo]",
                    string.Format("IconResource={0},{1}", iconPath, index),
                    "[ViewState]",
                    "Mode=",
                    "Vid=",
                    "FolderType=Generic"
                };

                File.WriteAllLines(iniPath, lines);

                // Set desktop.ini attributes: Hidden + System
                File.SetAttributes(iniPath, FileAttributes.Hidden | FileAttributes.System);

                // Set folder attribute: ReadOnly (Required for Windows to read desktop.ini for icons)
                DirectoryInfo di = new DirectoryInfo(folderPath);
                di.Attributes |= FileAttributes.ReadOnly;

                lblStatus.Text = "Applying changes and refreshing...";

                // Refresh shell
                ShellIconHelper.RefreshShell();

                MessageBox.Show("Icon applied successfully! Explorer has been notified of the change.");
                lblStatus.Text = "Done.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
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
            for (int i = 0; i < iconCount; i++)
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

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);

        private const int SHCNE_ASSOCCHANGED = 0x08000000;
        private const int SHCNF_IDLIST = 0x0000;

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

        public static void RefreshShell()
        {
            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
