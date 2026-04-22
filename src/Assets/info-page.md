---

# Find this useful?

If Ordir helps your workflow, supporting me helps for future updates and more desktop tools, or [leave a star on GitHub](https://github.com/landnthrn/ordir) :) 

---

# About:

A handy Windows app for quickly organizing File Explorer folders in any order you please! Say goodbye to the gut-wrenching mess of foldername ordering by a-z or numbers.  Adds ability for custom folder thumbnails as well. 

Your configuration transfers wherever you move a organized folder, even to other drives. Save custom setups, or for bulk operations export lists, feed to AI to organize, import revised list, and apply!

---

# How it works?
Ordir uses a fairly unknown method via hidden `desktop.ini` files, infotips, and sorting by comments.
Think of it like giving folders metadata, and sorting by it.
		    
## Input:
|
└── Load a target folder
    |
    └── Order folders to your desire
                    
## Apply process:
|
└── Creates `desktop.ini(s)` in each folder
    |
    └── Inserts infotip(s) (order number) into desktop.ini(s)
        |
        └── Makes into system folder(s)
            |
            └── Hides `desktop.ini(s)`

---

# How to see changes:

## In File Explorer of Folder:

- Right click empty space > Sort by > More... *(If on Windows 11, use Show more options)*
- Check Comments > OK 
- Right click empty space > Sort by > Comments
- Right click > Refresh

Sometimes it takes some play to refresh properly in Explorer.

### If you don't see changes:
- Open Task Manager
- Right click the task Windows Explorer > Restart

---

# How to set custom thumbnails:

Once folders have been applied, you may notice file and gear icons as thumbnails while viewing folders by large icons.
Luckily this setup gives folders the ability to add custom thumbnails from image files. 

- Right click a folder > Properties > Customize tab > Choose File...
- Select any image you'd like *(most formats are supported)*  
- Hit Apply > OK

## Disclaimer

Sometimes Windows may partically reset some thumbnail cache. 
It's recommended to apply the following cache tweak before setting custom thumbnails or sorting by comments.

►  [Stop Windows From Deleting Thumbnail Cache (Win 10/11)](https://winaero.com/windows-10-deleting-thumbnail-cache/)

►  *(If you want this also)*
   [How to Increase Number of Folder Views to Remember (Win 10/11)](https://winaero.com/change-number-of-folder-views-to-remember-in-windows-10/)

After applying a tweak, restart **Windows Explorer** from Task Manager.

### Strong Tip:
To avoid set image paths breaking, only set thumbnails to image files that you don't ever plan on moving, or renaming.
You could make a thumbnail bin just for this to make it easy.

---

## Quick launch from any folder

You can quickly launch Ordir, from any folder in File Explorer, here's how:
- In the folder you want to launch from
- Clear the address bar, type `cmd /k ordir`, press Enter  

If you used the installer and checked `Install 'ordir' command to PATH`, you can do this automatically. 

### For portable setup or building, manually add the `cli-launch` folder path to your user PATH:
- In this repo or your install folder, open `ordir-main\scripts\cli-launch`  
- Copy that folder’s path  
- Windows search > Environment variables > under User Variables, select Path > Edit > New > paste the path > OK 

---