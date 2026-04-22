@echo off
setlocal EnableExtensions
set "ROOT=%~dp0.."
cd /d "%ROOT%"
if errorlevel 1 exit /b 1

rem Publishing directly to a folder under the repo can hit MSB3094 when the repo path
rem contains an apostrophe (e.g. C:\Users\Somebody's PC\repo). Stage to a neutral path first.
set "STAGE=%ProgramData%\OrdirPublishStaging"
echo Publishing self-contained (staging to "%STAGE%" then mirroring to publish\) ...
if exist "%STAGE%" rmdir /s /q "%STAGE%"
mkdir "%STAGE%"
if errorlevel 1 exit /b 1

dotnet publish "src\Ordir.csproj" -c Release -r win-x64 --self-contained true -o "%STAGE%"
if errorlevel 1 (
  rmdir /s /q "%STAGE%" 2>nul
  exit /b 1
)

if not exist "publish" mkdir "publish"
robocopy "%STAGE%" "publish" /MIR >nul
if errorlevel 8 (
  rmdir /s /q "%STAGE%" 2>nul
  exit /b 1
)

rmdir /s /q "%STAGE%" 2>nul

echo.
echo Published to publish\ - quick test:
echo   publish\Ordir.exe
echo   Alternatively: scripts\Run-Published.bat
endlocal
