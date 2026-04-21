@echo off
setlocal

set "ROOT=%~dp0.."
cd /d "%ROOT%"
if errorlevel 1 exit /b 1

call scripts\build-self-contained.bat
if errorlevel 1 exit /b 1

if exist "dist\ordir-portable" rmdir /s /q "dist\ordir-portable"
if not exist "dist" mkdir dist

mkdir "dist\ordir-portable"
if errorlevel 1 exit /b 1

robocopy "publish" "dist\ordir-portable" /MIR >nul
if errorlevel 8 exit /b 1

mkdir "dist\ordir-portable\scripts\cli-launch" >nul 2>nul
robocopy "scripts\cli-launch" "dist\ordir-portable\scripts\cli-launch" /E >nul
if errorlevel 8 exit /b 1

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "if (Test-Path 'dist\ordir-portable-win-x64.zip') { Remove-Item 'dist\ordir-portable-win-x64.zip' -Force }; Compress-Archive -Path 'dist\ordir-portable\*' -DestinationPath 'dist\ordir-portable-win-x64.zip' -CompressionLevel Optimal"
if errorlevel 1 exit /b 1

echo.
echo Done. Portable folder: dist\ordir-portable
echo Done. Portable zip:    dist\ordir-portable-win-x64.zip
endlocal
