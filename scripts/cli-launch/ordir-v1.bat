@echo off
REM Launch Ordir TUI Tool
REM by landn.thrn

REM Set console to UTF-8 encoding for proper Unicode display
chcp 65001 >nul

REM Set window size (columns x lines) and buffer for scrolling
mode con: cols=120 lines=250
powershell -NoProfile -Command "$h=$Host.UI.RawUI;$b=$h.BufferSize;$b.Width=120;$b.Height=9999;$h.BufferSize=$b"

REM Get the directory where this batch file is located
set "SCRIPT_DIR=%~dp0"

REM Display the initial menu by calling ShowMenu.bat
call "%SCRIPT_DIR%ShowMenu.bat"

REM Launch PowerShell with UTF-8 encoding and the Ordir script (no logo)
powershell.exe -NoLogo -NoExit -ExecutionPolicy Bypass -File "%SCRIPT_DIR%ordir.ps1"
