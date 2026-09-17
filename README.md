<img src="icon.ico"  width="64" align="left" style="margin-right: 20px; border-radius: 10px;">

# Windows Directory Icon Tool

A professional C# GUI utility designed to easily customize folder icons on Windows. It handles standard directories as well as "stubborn" system folders (like Favorites, Downloads, etc.) by combining `desktop.ini` customization with Windows Registry overrides.

This can also be used as an extension to [xToolsMenu](https://github.com/xCONFLiCTiONx/xToolsMenu).

## Key Features

- **Icon Extraction:** Built-in picker to browse and select icons embedded within `.dll` or `.exe` files.
- **Known Folder Support:** Automatically detects system folders and applies Registry fixes in `HKLM` to ensure icons are honored by the shell.
- **Live Attribute Editing:** - Toggle Explorer settings (**Show Hidden/System Files**) with instant global refresh.
- Modify folder and `desktop.ini` attributes (**Hidden/System/Read-only**) in real-time.
- **Shell Integration:** Supports command-line arguments (`DirectoryIconTool.exe "%1"`) for easy integration into the Windows context menu.
- **Administrator Required:** Automatically requests elevation to modify protected system folders and registry keys.

## Usage

1. **Select Folder:** Pick the directory you want to customize.
2. **Select Icon:** Choose an `.ico` file or a library (`shell32.dll`, `imageres.dll`) to pick from embedded icons.
3. **Apply:** Click "Write desktop.ini and Refresh Icon".
4. **Instant Refresh:** Use the checkboxes to toggle visibility or folder attributes; Explorer will update immediately.

## Compilation

The tool is written in C# and targets .NET 4.0+ for maximum compatibility without external dependencies.

### Requirements

- Windows 10/11
- .NET Framework 4.0 or higher (Standard on Windows)

### Build Command

You can compile this tool using the standard C# compiler (`csc.exe`) included with Windows. Run the following command from the project root:

```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /win32manifest:app.manifest /win32icon:icon.ico /out:DirectoryIconTool.exe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll DirectoryIconTool.cs
```

### Files

- `DirectoryIconTool.cs`: Main source code.
- `app.manifest`: Application manifest for Administrator privileges.
- `icon.ico`: The icon used for the application executable.