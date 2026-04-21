@echo off
setlocal
set "ROOT=%~dp0.."
cd /d "%ROOT%"
if errorlevel 1 exit /b 1

echo Publishing framework-dependent to "publish-fd" (smaller; needs .NET 8 Desktop Runtime on the PC) ...
dotnet publish src\Ordir.csproj -c Release -r win-x64 --self-contained false -o publish-fd
if errorlevel 1 exit /b 1

echo.
echo Done. Install Desktop Runtime from https://dotnet.microsoft.com/download/dotnet/8.0
echo Then run:   publish-fd\Ordir.exe
endlocal
