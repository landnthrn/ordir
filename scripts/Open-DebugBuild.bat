@echo off
setlocal
set "ROOT=%~dp0.."
set "EXE=%ROOT%\src\bin\Debug\net8.0-windows10.0.17763.0\Ordir.exe"

if exist "%EXE%" (
  start "" "%EXE%"
  exit /b 0
)

echo No Debug build found at:
echo   %EXE%
echo.
echo Run scripts\run-dev.bat once ^(or: dotnet build^), then try this again.
pause
exit /b 1
