@echo off
rem Rebuild XingceTimer.exe with the C# compiler built into Windows.
rem No downloads or installations are required.
cd /d "%~dp0"
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /nologo /target:winexe /platform:anycpu /codepage:65001 /out:XingceTimer.exe /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll Program.cs Stage.cs MainForm.cs EditDialog.cs RunForm.cs
if errorlevel 1 (
  echo BUILD FAILED
) else (
  echo BUILD OK: XingceTimer.exe
)
pause
