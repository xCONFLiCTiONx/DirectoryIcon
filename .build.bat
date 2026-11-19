cd /d %~dp0
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /win32manifest:app.manifest /win32icon:icon.ico /out:DirectoryIconTool.exe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll DirectoryIconTool.cs

pause