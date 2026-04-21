@echo off
setlocal
set "ROOT=%~dp0.."
set "EXE=%ROOT%\publish\Ordir.exe"
if exist "%EXE%" (
  start "" "%EXE%"
  exit /b 0
)
set "DLL=%ROOT%\publish\Ordir.dll"
if exist "%DLL%" (
  cd /d "%ROOT%\publish"
  if errorlevel 1 exit /b 1
  dotnet Ordir.dll
  exit /b 0
)
echo No build found. Run:  scripts\build-self-contained.bat
pause
exit /b 1
