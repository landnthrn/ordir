<p align="center">
  <img src="https://github.com/user-attachments/assets/8b8f5455-5b3e-4728-a479-83774db6cf12" height="365"/>
  <img src="https://github.com/user-attachments/assets/4bf72cf2-57a7-4f67-a9b2-1b5b66853376" height="365"/>
</p>

# Ordir

A handy Windows app for quickly organizing File Explorer folders in any order you please. Say goodbye to the gut-wrenching mess of folder name ordering by A–Z or numbers. Adds the ability for custom folder thumbnails as well.

Your configuration transfers wherever you move an organized folder, even to other drives. Save a custom setup or for bulk operations export lists, feed them to an AI to organize, import a revised list, and apply!

---

## How it works

Ordir uses a fairly unknown method via hidden `desktop.ini` files, infotips, and sorting by **Comments** in Explorer—think of it as giving folders metadata and sorting by it.

#### Input Process:

1. Load a target folder  
2. Order folders to your desire
         

#### Apply Process:  

1. Creates `desktop.ini(s)` in each folder  
2. Inserts infotip(s) (order number) into desktop.ini(s)  
3. Makes into system folder(s)  
4. Hides `desktop.ini(s)`  

---

# Install

For full install options info see docs/install-options-guide.md#build

### ➤ Installer 

Download the installer `.exe` from **Releases** and run it. Installs to `Program Files` 

Check `Install 'ordir' command to user PATH`, so you can launch from any folder with [quick launch](#quick-launch-from-any-folder).


### ➤ Portable version

Download the portable `.zip` from **[Releases](https://github.com/landnthrn/ordir/releases/tag/2.0.0)**. #

Extract anywhere and run `Ordir.exe`


### ➤ Build from source

#### Requirements:

• Windows 10+
• .NET 8 SDK
• Inno Setup 6 (only if building installer)

#### Quick build:

    dotnet build Ordir.sln -c Release

#### Full instructions:
docs/install-options-guide.md#build

---

## Quick launch from any folder

#### You can launch Ordir from any folder inside File Explorer:
- In the folder you want to launch from
- Clear the address bar, type `cmd /k ordir`, press Enter  

If you used the installer and enabled PATH integration, you can do this automatically. 

#### For portable setup or building, manually add the `cli-launch` folder to your user PATH:
- In this repo or your install folder, open `scripts\cli-launch` (under the app root)   
- Copy that folder’s path  
- Windows search > **Environment variables** > under User Variables select **Path** > **Edit** > **New** > paste the path > OK  

---

## How to see changes

#### In File Explorer of target folder:

- Right-click empty space → **Sort by** → **More…** (on Windows 11, use **Show more options** if needed)  
- Check **Comments**  
- Right-click empty space → **Sort by** → **Comments**  
- Right-click → **Refresh**  

Sometimes it takes some play to Explorer to refresh properly.

#### If you don't see changes:
- Open Task Manager  
- Right-click **Windows Explorer** → **Restart**  

---

## How to set custom thumbnails

Once folders have been applied, you may notice file and gear icons as thumbnails while viewing folders by large icons.
Luckily this setup gives folders the ability to add custom thumbnails from image files. 

- Right-click folder > **Properties** > **Customize** > **Choose File...**
- Pick an image > **Apply** -> **OK**

## Disclaimer

Sometimes Windows may partically reset some thumbnail cache. 
It's recommended to apply the following cache tweak before setting custom thumbnails or sorting by comments.

- [Stop Windows From Deleting Thumbnail Cache (Win 10/11)](https://winaero.com/windows-10-deleting-thumbnail-cache/)
- *(If you want this also)*  
  [How to Increase Number of Folder Views to Remember (Win 10/11)](https://winaero.com/change-number-of-folder-views-to-remember-in-windows-10/)

After applying a tweak, restart **Windows Explorer** from Task Manager.

### Strong Tip:
To avoid set image paths breaking, only set thumbnails to image files that you don't ever plan on moving, or renaming.   
You could make a thumbnail bin just for this to make it easy.

---

## Found this useful?<img src="https://media.tenor.com/23NitOvEEkMAAAAj/optical-illusion-rotating-head.gif" width="30"><br>

If Ordir helps your workflow, supporting me helps for future updates and more desktop tools, or just leave a star on repo :)

[![Follow Me <3](https://img.shields.io/badge/Follow%20Me%20%3C3-000000?style=for-the-badge&logo=github&logoColor=white)](https://github.com/landnthrn)   
[![Find More of my Creations on GitHub](https://img.shields.io/badge/Find%20More%20of%20my%20Creations%20on%20GitHub-311A82?style=for-the-badge&logo=github&logoColor=white)](https://github.com/landnthrn?tab=repositories)  
[![Gists: landnthrn](https://img.shields.io/badge/Gists-311A82?style=for-the-badge&logo=github&logoColor=white)](https://gist.github.com/landnthrn)  
[![Discord: landn.thrn](https://img.shields.io/badge/Discord-311A82?style=for-the-badge&logo=discord&logoColor=white)](https://discord.com/users/831735011588964392)  
[![Buy Me a Coffee](https://img.shields.io/badge/Buy%20Me%20a%20Coffee-311A82?style=for-the-badge&logo=buymeacoffee&logoColor=white)](https://buymeacoffee.com/landn.thrn/extras)  
[![PayPal](https://img.shields.io/badge/PayPal-311A82?style=for-the-badge&logo=paypal&logoColor=white)](https://www.paypal.com/donate/?hosted_button_id=K4PLHFVBH7X8C)

