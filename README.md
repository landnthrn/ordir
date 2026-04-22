<p align="center">
  <img src="https://github.com/user-attachments/assets/8b8f5455-5b3e-4728-a479-83774db6cf12" height="365"/>
  <img src="https://github.com/user-attachments/assets/4bf72cf2-57a7-4f67-a9b2-1b5b66853376" height="365"/>
</p>

# Ordir

A handy Windows app for quickly organizing File Explorer folders in any order you please. Say goodbye to the gut-wrenching mess of folder name ordering by A–Z or numbers.

Your configuration transfers wherever you move an organized folder, even to other drives. Save a custom setup as lists, or for bulk operations export lists, feed them to an AI to organize, import a revised list, and apply.

Ordir uses a fairly unknown method via hidden `desktop.ini` files, infotips, and sorting by **Comments** in Explorer—think of it as giving folders metadata and sorting by it.

---

## How it works

**Input**

- Load a target folder  
- Order folders however you want  

**Apply process**

- Creates hidden `desktop.ini` file(s) in each folder  
- Inserts infotip(s) (order number) into those `desktop.ini` file(s)  
- Makes folders into system folder(s)  
- Hides `desktop.ini` file(s)  

---

## How to see changes

**In File Explorer for that folder**

- Right-click empty space → **Sort by** → **More…** (on Windows 11, use **Show more options** if needed)  
- Turn on **Comments**  
- Right-click empty space → **Sort by** → **Comments**  
- Right-click → **Refresh**  

Sometimes it takes a bit of refreshing for Explorer to catch up.

**If you do not see changes**

- Open Task Manager  
- Right-click **Windows Explorer** → **Restart**  

---

## Quick launch from any folder (`ordir`)

You can launch Ordir from the folder you are organizing inside File Explorer:

- Clear the address bar, type `cmd /k ordir`, press Enter  

If you used the installer and enabled PATH integration, that is set up for you. For portable installs or your own builds, add the `cli-launch` folder to your user **PATH**:

- In this repo or your install folder, open `scripts\cli-launch` (under the app root)  
- Copy that folder’s path  
- Windows search → **Environment variables** → **User** variables → **Path** → **Edit** → **New** → paste the path → OK out  

---

## How to set custom thumbnails

While viewing organized folders by large icons, you may notice file/gear icons as thumbnails. This setup also allows custom folder thumbnails from image files.

- Right-click folder -> **Properties** -> **Customize** -> **Choose File...**
- Pick an image and click **Apply** -> **OK**

## Disclaimer

Windows can sometimes partially reset thumbnail cache. It is recommended to apply the cache tweak before relying on custom thumbnails and sorting by comments.

Recommended:
- [Stop Windows From Deleting Thumbnail Cache (Win 10/11)](https://winaero.com/windows-10-deleting-thumbnail-cache/)
- [How to Increase Number of Folder Views to Remember (Win 10/11)](https://winaero.com/change-number-of-folder-views-to-remember-in-windows-10/)

After applying a tweak, restart **Windows Explorer** from Task Manager.

**Extra tip:** Use image files for thumbnails that you do not move or rename.

---

## Find this useful?

If Ordir helps your workflow, support helps future updates and more desktop tools—or leave a star on GitHub.

- GitHub: https://github.com/landnthrn  
- Buy Me a Coffee: https://buymeacoffee.com/landn.thrn  
- Discord: https://discord.com/users/831735011588964392  

---

## Requirements (for building)

- **Windows** (the app targets Windows 10 build 17763+ / Win10+)  
- **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** (includes the Desktop / WPF workload for `dotnet build` / `dotnet publish`)  
- **[Inno Setup 6](https://jrsoftware.org/isdl.php)** (optional; only if you run `scripts\build-installer.bat`)  

Quick compile (no installer):

```powershell
cd ordir-main
dotnet build Ordir.sln -c Release
```

Output DLL is under `src\bin\Release\…\`. For a runnable folder you normally publish (see below).

---

## Build and release outputs

From the `ordir-main` folder (repository layout: `quick-gui-dir-order\ordir-main\`):

| Output | Command | Where it lands |
|--------|---------|----------------|
| **Self-contained publish** (folder you can zip or ship) | `scripts\build-self-contained.bat` | `ordir-main\publish\` (contains `Ordir.exe` and all runtime files) |
| **Portable folder + zip** (same bits as publish, plus `scripts\cli-launch` copied in) | `scripts\build-portable.bat` | `ordir-main\dist\ordir-portable\` **and** `ordir-main\dist\ordir-portable-win-x64.zip` |
| **Installer** | `scripts\build-installer.bat` (runs publish then Inno) | `ordir-main\dist\ordir-setup.exe` |
| **Framework-dependent** (smaller; needs [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) on the PC) | `scripts\build-framework-dependent.bat` | `ordir-main\publish-fd\` |

**What to attach for a “portable release” next to the installer:** use **`dist\ordir-portable-win-x64.zip`**, or zip **`dist\ordir-portable\`** yourself after `build-portable.bat`. Both are the self-contained app; the batch also mirrors `scripts\cli-launch` into that folder for PATH / `ordir` usage.

See **INSTALL.md** for end-user install options (installer, portable zip, copy `publish`, framework-dependent).

---

## For contributors and builders

- **Source:** `src/` — .NET 8 WPF (C#), project `src\Ordir.csproj`  
- **Solution:** `Ordir.sln`  
- **Quick dev run:** `scripts\run-dev.bat` or `dotnet run --project src\Ordir.csproj`  
- **Info tab (About) text** source is **`creation-zone-files\About-section.txt`**. During build it is linked into the app as an embedded WPF `Resource` (`Assets/About-section.txt`). It is **not** copied as a loose file next to `Ordir.exe` in publish/installer outputs.

### Full release build (fresh `bin\Release`, portable zip, installer)

Run these **in order** from the `ordir-main` folder:

1. **`dotnet build Ordir.sln -c Release`** — refreshes **`src\bin\Release\…\Ordir.exe`** (and DLLs next to it) from the latest source.  
2. **`scripts\build-portable.bat`** — self-contained **`dotnet publish`**, fills **`publish\`**, then **`dist\ordir-portable\`** and **`dist\ordir-portable-win-x64.zip`**.  
3. **`scripts\build-installer.bat`** — compiles **`installer\ordir-setup.iss`** into **`dist\ordir-setup.exe`** (packages the current **`publish\`** tree).

```powershell
cd ordir-main
dotnet build Ordir.sln -c Release
scripts\build-portable.bat
scripts\build-installer.bat
```

Use whatever Git branch you prefer; for releases, building from **`main`** (or your default release branch) after merging is the usual workflow so tags and binaries match the branch users expect.
