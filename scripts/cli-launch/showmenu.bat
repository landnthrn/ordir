@echo off
chcp 65001 >nul

rem Define the escape character for ANSI colors
for /F %%a in ('echo prompt $E ^| cmd') do set "ESC=%%a"

rem ─ Normal Colors: 30–37 ─
set "BLACK=%ESC%[30m"
set "RED=%ESC%[31m"
set "GREEN=%ESC%[32m"
set "YELLOW=%ESC%[33m"
set "BLUE=%ESC%[34m"
set "MAGENTA=%ESC%[35m"
set "CYAN=%ESC%[36m"
set "WHITE=%ESC%[37m"

rem ─ Bright: 90–97 ─ 
set "BRIGHTBLACK=%ESC%[90m"
set "BRIGHTRED=%ESC%[91m"
set "BRIGHTGREEN=%ESC%[92m"
set "BRIGHTYELLOW=%ESC%[93m"
set "BRIGHTBLUE=%ESC%[94m"
set "BRIGHTMAGENTA=%ESC%[95m"
set "BRIGHTCYAN=%ESC%[96m"
set "BRIGHTWHITE=%ESC%[97m"

rem == Background Colors: 40–47 ==
set "BG_BLACK=%ESC%[40m"
set "BG_RED=%ESC%[41m"
set "BG_GREEN=%ESC%[42m"
set "BG_YELLOW=%ESC%[43m"
set "BG_BLUE=%ESC%[44m"
set "BG_MAGENTA=%ESC%[45m"
set "BG_CYAN=%ESC%[46m"
set "BG_WHITE=%ESC%[47m"

rem == Bright Backgrounds: 100–107 ==
set "BG_BRIGHTBLACK=%ESC%[100m"
set "BG_BRIGHTRED=%ESC%[101m"
set "BG_BRIGHTGREEN=%ESC%[102m"
set "BG_BRIGHTYELLOW=%ESC%[103m"
set "BG_BRIGHTBLUE=%ESC%[104m"
set "BG_BRIGHTMAGENTA=%ESC%[105m"
set "BG_BRIGHTCYAN=%ESC%[106m"
set "BG_BRIGHTWHITE=%ESC%[107m"

rem ==== Color shortcuts ====
set "RESET=%ESC%[0m"
set "BOLD=%ESC%[1m"
set "DIM=%ESC%[2m"

cls
echo.
echo %MAGENTA%   ─────░█████╗░██████╗░██████╗░██╗██████╗░──────%RESET%
echo %MAGENTA%       ─██╔══██╗██╔══██╗██╔══██╗██║██╔══██╗──%RESET%
echo %MAGENTA% ───────██║░░██║██████╔╝██║░░██║██║██████╔╝─%RESET%
echo %MAGENTA%     ───██║░░██║██╔══██╗██║░░██║██║██╔══██╗────%RESET%
echo %MAGENTA%   ─────╚█████╔╝██║░░██║██████╔╝██║██║░░██║──%RESET%
echo %MAGENTA%    ────░╚════╝░╚═╝░░╚═╝╚═════╝░╚═╝╚═╝░░╚═╝────────%RESET%
echo.
echo  %BRIGHTWHITE%█▄▄ █▄█   █░░ ▄▀█ █▄░█ █▀▄ █▄░█   ▀█▀ █ █ █▀█ █▄ █%RESET%
echo  %BRIGHTWHITE%█▄█ ░█░   █▄▄ █▀█ █ ▀█ █▄▀ █░▀█ ▄ ░█░ █▀█ █▀▄ █░▀█%RESET%
echo.
echo.
echo  %BG_BRIGHTMAGENTA% Found this useful? %RESET%
echo.
echo  %BRIGHTGREEN%Find more of my creations on Github:%RESET%
echo  %WHITE%https://github.com/landnthrn?tab=repositories%RESET%
echo.
echo  %BRIGHTGREEN%Support me on Buy Me a Coffee:%RESET%
echo  %WHITE%https://buymeacoffee.com/landn.thrn%RESET%
echo.
echo. 
echo  %BG_BRIGHTMAGENTA% NOTES: %RESET%
echo.
echo  %WHITE%Commands are highlighted like%RESET% %WHITE%[%RESET%%BRIGHTGREEN%this%RESET%%WHITE%]%RESET%
echo. 
echo  %WHITE%To read recommended useful info about this feature, use this command:%RESET%
echo  %WHITE%[%RESET%%BRIGHTGREEN%info%RESET%%WHITE%]%RESET%
echo.
echo  %BRIGHTMAGENTA%TO SEE CHANGES:%RESET%
echo  %WHITE%Sort by Comments see how with %WHITE%[%RESET%%BRIGHTGREEN%info%RESET%%WHITE%]%RESET% 
echo.
echo  %WHITE%Suggest using Right Click ^> Refresh in File Explorer%RESET%
echo  %WHITE%Or for full refresh open Task Manager right click on ^> File Explorer ^> Restart%RESET%
echo.
echo. 
echo  %BG_BRIGHTMAGENTA% ORGANIZE OPTIONS: %RESET%
echo.
echo  %WHITE%How would you like to organize your folders?%RESET%
echo. 
echo  %WHITE%[%RESET%%BRIGHTGREEN%op1%RESET%%WHITE%]%RESET% %BG_MAGENTA% Option 1: %RESET% %BRIGHTMAGENTA%All Folders Inside a Folder %RESET%
echo                    %BRIGHTGREEN%^└─%RESET% %WHITE%Create Desktop.ini's%RESET%
echo                         %BRIGHTGREEN%^└─%RESET% %WHITE%Make Into System Folders%RESET%
echo                              %BRIGHTGREEN%^└─%RESET% %WHITE%Create a List%RESET%
echo                                   %BRIGHTGREEN%^└─%RESET% %WHITE%You Edit The List in Notepad%RESET%
echo                                        %BRIGHTGREEN%^└─%RESET% %WHITE%Apply The Revised List%RESET%
echo                                             %BRIGHTGREEN%^└─%RESET% %WHITE%Hide Desktop.ini's%RESET%
echo.
echo  %WHITE%[%RESET%%BRIGHTGREEN%op2%RESET%%WHITE%]%RESET% %BG_MAGENTA% Option 2: %RESET% %BRIGHTMAGENTA%First Forefront Subfolders Inside a Folder%RESET%
echo                    %BRIGHTGREEN%^└─%RESET% %WHITE%Create Desktop.ini's%RESET%
echo                         %BRIGHTGREEN%^└─%RESET% %WHITE%Make Into System Folders%RESET%
echo                              %BRIGHTGREEN%^└─%RESET% %WHITE%Create a List%RESET%
echo                                   %BRIGHTGREEN%^└─%RESET% %WHITE%You Edit The List in Notepad%RESET%
echo                                        %BRIGHTGREEN%^└─%RESET% %WHITE%Apply The Revised List%RESET%
echo                                             %BRIGHTGREEN%^└─%RESET% %WHITE%Hide Desktop.ini's%RESET%
echo.
echo  %WHITE%[%RESET%%BRIGHTGREEN%op3%RESET%%WHITE%]%RESET% %BG_MAGENTA% Option 3: %RESET% %BRIGHTMAGENTA%In a single folder%RESET%
echo                    %BRIGHTGREEN%^└─%RESET% %WHITE%Create Desktop.ini%RESET%
echo                         %BRIGHTGREEN%^└─%RESET% %WHITE%Make Into System Folder%RESET%
echo                              %BRIGHTGREEN%^└─%RESET% %WHITE%Hide Desktop.ini%RESET%
echo.
echo.
echo  %BG_RED% DELETE OPTIONS: %RESET%
echo.
echo  %WHITE%How do you want to delete Desktop.ini's?%RESET%
echo.
echo  %WHITE%[%RESET%%RED%del1%RESET%%WHITE%]%RESET% %BG_RED% Option 1: %RESET% %BRIGHTMAGENTA%Delete Desktop.ini's in ALL Folders Inside a Folder%RESET%
echo.
echo  %WHITE%[%RESET%%RED%del2%RESET%%WHITE%]%RESET% %BG_RED% Option 2: %RESET% %BRIGHTMAGENTA%Delete Desktop.ini's in First Forefront Subfolders Inside a Folder%RESET%
echo.
echo.
echo  %WHITE%To see more commands and have a more manual procedure, use:%RESET%
echo  %WHITE%[%RESET%%BRIGHTGREEN%manual%RESET%%WHITE%]%RESET%
echo.
echo.
