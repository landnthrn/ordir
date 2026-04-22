# Ordir

A handy Windows app for quickly organizing File Explorer folders in any order you please. Say goodbye to the gut-wrenching mess of folder name ordering by A–Z or numbers. It also adds custom folder thumbnails.

Your configuration transfers wherever you move an organized folder, even to other drives. Save custom setups, or for bulk operations export lists, feed them to an AI to organize, import a revised list, and apply.

Ordir uses a fairly unknown method via hidden `desktop.ini` files, infotips, and sorting by **Comments** in Explorer—think of it as giving folders metadata and sorting by it.

---

## How it works

**Input**

- Load a target folder  
- Order folders however you want  

**Apply process**

- Creates `desktop.ini` file(s) in each folder  
- Inserts infotip(s) (order number) into those `desktop.ini` file(s)  
- Makes folders into system folder(s)  
- Hides `desktop.ini` file(s)  

---

## How to see changes

**In File Explorer for that folder**

- Right-click empty space → **Sort by** → **More…** (on Windows 11, use **Show more options** if needed)  
- Check **Comments** → **OK**  
- Right-click empty space → **Sort by** → **Comments**  
- Right-click → **Refresh**  

Sometimes it takes some play to refresh properly in Explorer.

**If you do not see changes**

- Open Task Manager  
- Right-click **Windows Explorer** → **Restart**  

---

## How to set custom thumbnails

Once folders have been applied, you may notice file and gear icons as thumbnails while viewing folders by large icons. This setup also lets you add custom thumbnails from image files.

- Right-click a folder → **Properties** → **Customize** → **Choose File…**  
- Select an image (most formats are supported) → **Apply** → **OK**  

**Strong tip:** Point thumbnails at image files you do not plan to move or rename, or the folder thumbnail path can break—you could use a dedicated “thumbnail bin” folder to keep paths stable.

### Disclaimer

Sometimes Windows may partially reset thumbnail cache. Do the fix below **before** setting custom folder thumbnails/icons or sorting by comments.

## Recommendation

Install the `.reg` from [Winaero Tweaker](https://winaero.com/) (well-regarded for years). You can use the article download for just the registry file, or install Tweaker and browse their other Windows tweaks.

- [Stop Windows From Deleting Thumbnail Cache.reg (Win 10/11)](https://winaero.com/windows-10-deleting-thumbnail-cache/)  
- (Optional) [How to increase number of folder views to remember (Win 10/11)](https://winaero.com/change-number-of-folder-views-to-remember-in-windows-10/)  

Do **not** install the “restore defaults” `.reg` unless you intend to uninstall the tweak.

After installing the `.reg`, open Task Manager → **Windows Explorer** → right-click → **Restart**.

You may notice File Explorer’s window size and sort options reset once; that is why applying this before thumbnails and **Comments** sorting is strongly suggested.

---

## Quick launch from any folder (`ordir`)

You can launch Ordir from any folder you want inside File Explorer:

- In the folder you want to organize, clear the address bar  
- Type `cmd /k ordir`, press Enter  

If you checked **Install to path** on the installer, you can do this automatically.

If you are building or using portable, set up quick launch manually:

- Navigate to `Ordir\scripts\cli-launch` (under the app root, or the same path in this repo)  
- Copy that folder’s path  
- Windows search → **Environment variables** → open **Environment variables** again if System Properties appears first  
- Under **User** variables, select **Path** → **Edit** → **New** → paste the `cli-launch` path (or a directory of shims you use) → OK out  
- In `cli-launch`, edit `ordir.bat` and set the path to `Ordir.exe` to match where your build or install lives  

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
