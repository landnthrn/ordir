@echo off
REM Launch Ordir TUI Tool
REM by landn.thrn

mode con: cols=120 lines=81 >nul 2>&1

REM Set console to UTF-8 encoding for proper Unicode display
chcp 65001 >nul

REM Tall scrollback buffer, then pin window to 81 lines again (buffer-first can grow the window on some hosts)
powershell -NoProfile -ExecutionPolicy Bypass -Command "try{$r=$Host.UI.RawUI;$tw=[Math]::Min(120,[int]$r.MaxWindowSize.Width);$th=[Math]::Min(81,[int]$r.MaxWindowSize.Height);$b=$r.BufferSize;if($b.Width -lt 120){$b.Width=120};if($b.Height -lt $th){$b.Height=$th};$r.BufferSize=$b;$w=$r.WindowSize;$w.Width=$tw;$w.Height=$th;$r.WindowSize=$w;$b=$r.BufferSize;$b.Height=9999;$r.BufferSize=$b;$w=$r.WindowSize;$w.Width=$tw;$w.Height=$th;$r.WindowSize=$w}catch{}"

REM Get the directory where this batch file is located
set "SCRIPT_DIR=%~dp0"

REM Display the initial menu by calling ShowMenu.bat
call "%SCRIPT_DIR%ShowMenu.bat"

REM Launch PowerShell with UTF-8 encoding and the Ordir script (no logo)
powershell.exe -NoLogo -NoExit -ExecutionPolicy Bypass -File "%SCRIPT_DIR%ordir.ps1"
