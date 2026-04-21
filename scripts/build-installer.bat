@echo off
setlocal EnableExtensions
set "ROOT=%~dp0.."
cd /d "%ROOT%"
if errorlevel 1 exit /b 1

if not exist "publish\Ordir.exe" (
  echo Publish output not found. Building self-contained publish first...
  call scripts\build-self-contained.bat
  if errorlevel 1 exit /b 1
)

rem Optional: set INNO_ISCC to the full path of ISCC.exe (User or System env), e.g. after a non-default install.
set "ISCC_EXE="
if defined INNO_ISCC if exist "%INNO_ISCC%" set "ISCC_EXE=%INNO_ISCC%"

if not defined ISCC_EXE if exist "%ProgramFiles%\Inno Setup 6\ISCC.exe" set "ISCC_EXE=%ProgramFiles%\Inno Setup 6\ISCC.exe"
if not defined ISCC_EXE if exist "%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe" set "ISCC_EXE=%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe"
if not defined ISCC_EXE if exist "%LocalAppData%\Programs\Inno Setup 6\ISCC.exe" set "ISCC_EXE=%LocalAppData%\Programs\Inno Setup 6\ISCC.exe"
if not defined ISCC_EXE if exist "C:\Program Files\Inno Setup 6\ISCC.exe" set "ISCC_EXE=C:\Program Files\Inno Setup 6\ISCC.exe"
if not defined ISCC_EXE if exist "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" set "ISCC_EXE=C:\Program Files (x86)\Inno Setup 6\ISCC.exe"

if not defined ISCC_EXE (
  echo ERROR: Inno Setup 6 compiler ^(ISCC.exe^) not found.
  echo Install from https://jrsoftware.org/isdl.php
  echo.
  echo Searched: INNO_ISCC env, "Program Files", "Program Files (x86)", and %%LocalAppData%%\Programs
  echo If ISCC.exe is elsewhere, set a User environment variable:
  echo   Name:  INNO_ISCC
  echo   Value: full path to ISCC.exe ^(e.g. D:\Tools\Inno Setup 6\ISCC.exe^)
  echo Then open a new terminal and run this script again.
  exit /b 1
)

if not exist "dist" mkdir dist

echo Using: "%ISCC_EXE%"
echo Building installer...
"%ISCC_EXE%" "installer\ordir-setup.iss"
if errorlevel 1 exit /b 1

echo.
echo Done. Installer: dist\ordir-setup.exe
endlocal
