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
echo  %MAGENTA% █▀▄▀█ ─█▀▀█ ░█▄─ █ ░█─░█ ─█▀▀█░ █─── 　  █▀▄▀█ ░█▀▀▀█ ░█▀▀▄  █▀▀▀%RESET%
echo  %MAGENTA%░█░█ █ ░█▄▄█  █░█ █  █─ █  █▄▄█ ░█─── 　 ░█ █ █  █──░█ ░█─░█ ░█▀▀▀%RESET%
echo  %MAGENTA% █──░█ ░█─ █░ █──▀█ ─▀▄▄▀ ░█─ █ ░█▄▄█ 　  █──░█  █▄▄▄█  █▄▄▀ ░█▄▄▄%RESET%
echo.
echo.
echo  %BG_BRIGHTMAGENTA% CATEGORY 1 %RESET%
echo. 
echo                         CREATE DESKTOP.INI'S IN:
echo  %WHITE%[%RESET%%BRIGHTGREEN%cat1 op1%RESET%%WHITE%]%RESET% %BG_MAGENTA% Option 1: %RESET% %BRIGHTMAGENTA%All Folders Inside a Folder%RESET%
echo.
echo  %WHITE%[%RESET%%BRIGHTGREEN%cat1 op2%RESET%%WHITE%]%RESET% %BG_MAGENTA% Option 2: %RESET% %BRIGHTMAGENTA%First Forefront Subfolders Inside a Folder%RESET%
echo.
echo  %WHITE%[%RESET%%BRIGHTGREEN%cat1 op3%RESET%%WHITE%]%RESET% %BG_MAGENTA% Option 3: %RESET% %BRIGHTMAGENTA%A single folder%RESET%
echo.
echo.
echo  %BG_BRIGHTMAGENTA% CATEGORY 2 %RESET%
echo. 
echo                         MAKE SYSTEM FOLDERS FOR:
echo  %WHITE%[%RESET%%BRIGHTGREEN%cat2 op1%RESET%%WHITE%]%RESET% %BG_MAGENTA% Option 1: %RESET% %BRIGHTMAGENTA%All Folders Inside a Folder%RESET%
echo.
echo  %WHITE%[%RESET%%BRIGHTGREEN%cat2 op2%RESET%%WHITE%]%RESET% %BG_MAGENTA% Option 2: %RESET% %BRIGHTMAGENTA%First Forefront Subfolders Inside a Folder%RESET%
echo.
echo  %WHITE%[%RESET%%BRIGHTGREEN%cat2 op3%RESET%%WHITE%]%RESET% %BG_MAGENTA% Option 3: %RESET% %BRIGHTMAGENTA%A single folder%RESET%
echo.
echo.
echo  %BG_BRIGHTMAGENTA% CATEGORY 3 %RESET%
echo. 
echo                         CREATE A LIST FOR:
echo  %WHITE%[%RESET%%BRIGHTGREEN%cat3 op1%RESET%%WHITE%]%RESET% %BG_MAGENTA% Option 1: %RESET% %BRIGHTMAGENTA%All Folders Inside a Folder%RESET%
echo.
echo  %WHITE%[%RESET%%BRIGHTGREEN%cat3 op2%RESET%%WHITE%]%RESET% %BG_MAGENTA% Option 2: %RESET% %BRIGHTMAGENTA%First Forefront Subfolders Inside a Folder%RESET%
echo.
echo.
echo  %BG_BRIGHTMAGENTA% CATEGORY 4 %RESET%
echo.
echo                         APPLY A LIST FOR:
echo  %WHITE%[%RESET%%BRIGHTGREEN%cat4 op1%RESET%%WHITE%]%RESET% %BG_MAGENTA% Option 1: %RESET% %BRIGHTMAGENTA%All Folders Inside a Folder%RESET%
echo.
echo  %WHITE%[%RESET%%BRIGHTGREEN%cat4 op2%RESET%%WHITE%]%RESET% %BG_MAGENTA% Option 1: %RESET% %BRIGHTMAGENTA%First Forefront Subfolders Inside a Folder%RESET%
echo.
echo.
echo  %BG_BRIGHTMAGENTA% CATEGORY 5 %RESET%
echo. 
echo                         HIDE DESKTOP.INI'S IN:
echo  %WHITE%[%RESET%%BRIGHTGREEN%cat5 op1%RESET%%WHITE%]%RESET% %BG_MAGENTA% Option 1: %RESET% %BRIGHTMAGENTA%All Folders Inside a Folder%RESET%
echo.
echo  %WHITE%[%RESET%%BRIGHTGREEN%cat5 op2%RESET%%WHITE%]%RESET% %BG_MAGENTA% Option 2: %RESET% %BRIGHTMAGENTA%First Forefront Subfolders Inside a Folder%RESET%
echo.
echo  %WHITE%[%RESET%%BRIGHTGREEN%cat5 op3%RESET%%WHITE%]%RESET% %BG_MAGENTA% Option 3: %RESET% %BRIGHTMAGENTA%A single folder%RESET%
echo.
echo.
echo  %BG_BRIGHTMAGENTA% CATEGORY 6 %RESET%
echo. 
echo                         UNHIDE DESKTOP.INI'S IN:
echo  %WHITE%[%RESET%%BRIGHTGREEN%cat6 op1%RESET%%WHITE%]%RESET% %BG_MAGENTA% Option 1: %RESET% %BRIGHTMAGENTA%All Folders Inside a Folder%RESET%
echo.
echo  %WHITE%[%RESET%%BRIGHTGREEN%cat6 op2%RESET%%WHITE%]%RESET% %BG_MAGENTA% Option 2: %RESET% %BRIGHTMAGENTA%First Forefront Subfolders Inside a Folder%RESET%
echo.
echo  %WHITE%[%RESET%%BRIGHTGREEN%cat6 op3%RESET%%WHITE%]%RESET% %BG_MAGENTA% Option 3: %RESET% %BRIGHTMAGENTA%A single folder%RESET%
echo.
echo.
echo  %BG_RED% CATEGORY 7 %RESET%
echo. 
echo                         DELETE DESKTOP.INI'S IN:
echo  %WHITE%[%RESET%%RED%cat7 op1%RESET%%WHITE%]%RESET% %BG_RED% Option 1: %RESET% %BRIGHTMAGENTA%All Folders Inside a Folder%RESET%
echo.
echo  %WHITE%[%RESET%%RED%cat7 op2%RESET%%WHITE%]%RESET% %BG_RED% Option 2: %RESET% %BRIGHTMAGENTA%First Forefront Subfolders Inside a Folder%RESET%
echo.
