@echo off
setlocal EnableExtensions
REM Use ASCII only in this file so CMD (cp437) does not show garbled punctuation.

title Ordir - dev

set "ROOT=%~dp0.."
echo.
echo ========================================
echo  Ordir - run-dev.bat
echo ========================================
echo.

cd /d "%ROOT%"
if errorlevel 1 (
  echo [ERROR] Could not change directory to repo root:
  echo         %ROOT%
  echo.
  goto :EndPause
)

echo [OK] Repo root:
echo     %CD%
echo.

where dotnet >nul 2>&1
if errorlevel 1 (
  echo [ERROR] dotnet was not found on your PATH.
  echo         Install the .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0
  echo         Then reopen this window or sign out/in so PATH updates.
  echo.
  goto :EndPause
)

echo [..] Checking for an installed .NET SDK ^(required: SDK, not runtime-only^)...
dotnet --list-sdks 2>nul | findstr "." >nul
if errorlevel 1 (
  echo.
  echo [ERROR] No .NET SDK found. "dotnet.exe" is on PATH but it cannot run "dotnet run".
  echo.
  echo This usually means you installed only a ".NET Runtime" / host bundle, not the SDK.
  echo.
  echo Fix:
  echo   1. Open https://dotnet.microsoft.com/download/dotnet/8.0
  echo   2. Under ".NET 8", download and install ".NET SDK" ^(x64^) for Windows.
  echo   3. Close this window, open a NEW Command Prompt, run this script again.
  echo.
  echo Optional check in a new CMD window:  dotnet --list-sdks
  echo   You should see a line like:  8.0.xxx [C:\Program Files\dotnet\sdk\...]
  echo.
  goto :EndPause
)

echo [OK] SDK^(s^) installed:
dotnet --list-sdks
echo.

echo [OK] dotnet version string:
dotnet --version
if errorlevel 1 (
  echo [ERROR] dotnet --version failed unexpectedly.
  echo.
  goto :EndPause
)
echo.

if not exist "src\Ordir.csproj" (
  echo [ERROR] Project file not found:
  echo         %CD%\src\Ordir.csproj
  echo         Run this script from the repo ^(keep scripts\ inside the project^).
  echo.
  goto :EndPause
)

echo [..] Restoring / building / starting WPF window...
echo     Close the GUI when done - this window will show the exit code after.
echo     If you do not see the GUI, check taskbar / Alt+Tab / behind other windows.
echo.

dotnet run --project "src\Ordir.csproj"
set "RC=%ERRORLEVEL%"

echo.
echo ========================================
if "%RC%"=="0" (
  echo  dotnet finished with exit code 0 ^(normal^).
) else (
  echo  dotnet finished with exit code %RC% ^(build or runtime error^).
  echo  Scroll up for MSBuild or runtime errors.
)
echo ========================================
echo.

:EndPause
pause

endlocal
