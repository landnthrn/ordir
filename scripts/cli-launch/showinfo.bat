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
echo.
echo %BRIGHTMAGENTA%  █─ █ ░█▀▀▀  █───  █▀▀█ ░█▀▀▀ ░█─░█  █─── 　 ▀█▀ ░█▄─░█ ░█▀▀▀  █▀▀▀█%RESET%
echo %BRIGHTMAGENTA% ░█▀▀█ ░█▀▀▀ ░█─── ░█▄▄█  █▀▀▀ ░█─░█ ░█─── 　 ░█─ ░█ █ █ ░█▀▀▀ ░█──░█%RESET%
echo %BRIGHTMAGENTA% ░█─░█  █▄▄▄ ░█▄▄█ ░█───  █─── ─▀▄▄▀ ░█▄▄█ 　 ▄█▄  █──▀█  █─── ░█▄▄▄█%RESET%
echo.
echo  %BG_BRIGHTMAGENTA% ^* SUGGESTED TO READ ALL THE INFO ON THIS PAGE ^* %RESET%
echo.
echo  %WHITE%— There is a TESTER Folder included in this pack if you'd like to test the process with that :)%RESET%
echo.
echo  %WHITE%— When editing lists, you can use decimal numbers to squeeze in items%RESET% 
echo    %WHITE%without having to adjust all the ones that come after it (very useful)%RESET%
echo. 
echo  %WHITE%— If you want to save presets of lists you make just rename the list file to whatever you'd like%RESET%
echo    %WHITE%as long as it's not the defaulted name%RESET%
echo.
echo  %WHITE%— If you're viewing your folders as large icons you'll notice that%RESET%
echo    %WHITE%Desktop.ini's gear icon sometimes shows as thumbnail even after hiding them%RESET%
echo    %WHITE%Suggest to customize the folder thumbnail/icon%RESET%
echo. 
echo.
echo  %BG_BRIGHTMAGENTA% IMPORTANT RECOMMENDATION: %RESET%
echo.
echo  %WHITE%Some users have experienced their custom set folder thumbnails/icons being reset by Windows randomly.%RESET%
echo.
echo  %WHITE%There is a quick fix for this, do it before setting custom folder thumbnail/icons or sorting by comments.%RESET%  
echo.
echo  %WHITE%Download this .reg file by Winaero Tweaker%RESET%
echo  %WHITE%Winaero Tweaker is a very well loved ^& praised software for many years%RESET%
echo  %WHITE%You could use the direct download links below for just the .reg file, or you could download the software for yourself%RESET% 
echo  %WHITE%and see all the sorts of Windows Tweaks they offer with it%RESET%
echo.
echo  %BRIGHTMAGENTA%Stop Windows From Deleting Thumbnail Cache.reg (Win 10/11)%RESET%
echo  %WHITE%https://winaero.com/windows-10-deleting-thumbnail-cache/%RESET%
echo.
echo  %BRIGHTMAGENTA%(If you want also) How to Increase Number of Folder Views to Remember (Win 10/11)%RESET%
echo  %WHITE%https://winaero.com/change-number-of-folder-views-to-remember-in-windows-10/%RESET%
echo.
echo  %WHITE%Don't install the restore defaults .reg, that's for uninstalling the tweak%RESET%
echo.
echo  %WHITE%Once you have installed the .reg open Task manager ^> Windows Explorer ^> Right Click ^> Restart%RESET%
echo  %WHITE%You may notice your custom window size for File Explorer is different and your sort by options are default,%RESET% 
echo  %WHITE%this only happens once. That's why it's strongly suggested to do prior setting thumbnails ^& sorting by comments.%RESET%
echo.
echo  %BRIGHTGREEN%EXTRA TIP:
echo  %WHITE%Set thumbnails to image files that you don't ever plan on moving, or renaming.%RESET%
echo  %WHITE%Otherwise it will break the path location thats set for the folders thumbnail,
echo  %WHITE%you could make a thumbnail bin just for this to make it easy.%RESET%
echo.
echo.
echo  %BRIGHTGREEN%How to set custom folder thumbnails/icons%RESET%
echo. 
echo  %WHITE%Right click a folder ^> Properties ^> Customize ^> Choose File ^> Choose a Image File%RESET%
echo.
echo.
echo  %BRIGHTGREEN%How to sort by the order you applied: (See changes)%RESET%
echo.
echo  %WHITE%— Inside of your newly organized folders %RESET%
echo  %WHITE%— At the top of File Explorer click View ^> Details %RESET%
echo  %WHITE%— Now you will be able to see the column of sort options like Name, Date, Type %RESET%
echo  %WHITE%— Select 'More...' or if Windows 11 use 'Show more options' %RESET%
echo  %WHITE%— Find ^& enable Comments, then select OK%RESET%
echo  %WHITE%— Then Sort By ^> Comments%RESET%
echo  %WHITE%— Right click a empty space inside the folder ^> Refresh%RESET%
echo  %WHITE%  If you don't see the changes go to Task Manager ^> File Explorer ^> Right click ^> Restart%RESET%
echo.
echo  %WHITE%Now your folders will be sorted in the order you've customized :)%RESET%
echo.
echo  %WHITE%Unfortunately there was no way to automate this for you,%RESET%
echo  %WHITE%so you will have to do this individually for each folder.%RESET%
echo. 
echo.
echo  %BG_BRIGHTMAGENTA% BONUS TIP %RESET%
echo.
echo  %BRIGHTGREEN%Want to launch this feature easier?%RESET%
echo  %WHITE%(if you use shims, then you know what to do, use your shim directory)%RESET%
echo.
echo  %WHITE%- Copy the files 'ordir-v1.bat', 'ordir.ps1', ShowMenu.bat, ShowManualMenu.bat, and ShowInfo.bat%RESET%
echo  %WHITE%- Go to this path  'C:\Windows'%RESET%
echo  %WHITE%- Create a folder called 'Scripts'%RESET%
echo  %WHITE%- Create a folder inside 'Scripts' called `Ordir-v1`%RESET%
echo  %WHITE%- Paste the files you copied in there%RESET%
echo  %WHITE%- Copy the folder path `C:\Windows\Scripts`%RESET%
echo  %WHITE%- Windows search for 'Environment Variables'%RESET%
echo  %WHITE%- Once System Properties is open select 'Environment Variables'%RESET%
echo  %WHITE%- Under your 'User Variables' find 'Path' select it and click 'Edit' then 'New'%RESET%
echo  %WHITE%- Paste the path 'C:\Windows\Scripts\Ordir-v1'%RESET%
echo  %WHITE%- Press Ok a few times until Environment Variables is closed%RESET%
echo.
echo  %WHITE%Now you will be able to launch this feature just by opening a regular Command Prompt%RESET%
echo  %WHITE%and typing 'ordir-v1'%RESET%
echo.
echo  %BRIGHTWHITE%To return to the menu:%RESET%
echo  %WHITE%[%BRIGHTGREEN%menu%RESET%%WHITE%]%RESET%
echo.
