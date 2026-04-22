# Install or Build Ordir

Single reference for **installing** (installer, portable zip, publish folders) and **building** from this repo.

**Where to run commands:** open a terminal in the **`ordir-main`** folder (the directory that contains `Ordir.sln` and the `scripts\` folder). Paths below are relative to that folder.

This file lives at **`docs/install-options-guide.md`** in the repository. It is not embedded in the app; the in-app **Info** tab text is **`src/Assets/info-page.md`** (see **`docs/info-page-guide.md`**).

---

## Prerequisites

- **Windows** — Ordir targets Windows 10 build **17763** and later.
- **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** — required for `dotnet build` / `dotnet publish` (includes Desktop / WPF workload).
- **[Inno Setup 6](https://jrsoftware.org/isdl.php)** — only if you run **`scripts\build-installer.bat`**.

Quick compile (no publish / no installer):

```powershell
cd ordir-main
dotnet build Ordir.sln -c Release
```

Binaries land under **`src\bin\Release\…\`**. For a folder you can zip or ship, use **`publish\`** via the scripts in the next sections.

---

## Installer

1. From **`ordir-main`**, run:

```bat
scripts\build-installer.bat
```

If **`publish\Ordir.exe`** is missing, this script runs **`scripts\build-self-contained.bat`** first.

2. Output:

```text
dist\ordir-setup.exe
```

3. Default install location: **`C:\Program Files\Ordir\`**.
4. The wizard can add optional **PATH** integration for the **`ordir`** command.

---

## Portable zip

1. From **`ordir-main`**, run:

```bat
scripts\build-portable.bat
```

This runs a self-contained **`dotnet publish`** (via **`build-self-contained.bat`**), fills **`publish\`**, copies **`scripts\cli-launch`** into **`dist\ordir-portable\`**, and creates **`dist\ordir-portable-win-x64.zip`**.

2. Share **`dist\ordir-portable-win-x64.zip`**, or zip **`dist\ordir-portable\`** yourself.
3. Extract and run **`Ordir.exe`** from the extracted folder.
4. Optional: add **`scripts\cli-launch`** (from the extracted tree or repo) to your user **PATH** so **`cmd /k ordir`** works from any folder.

---

## Self-contained publish folder (manual zip / copy)

```bat
scripts\build-self-contained.bat
```

Output:

```text
publish\
```

Run **`publish\Ordir.exe`**. This script stages publish to **`%ProgramData%\OrdirPublishStaging`** first, then mirrors into **`publish\`**, which avoids MSBuild issues when the repo path contains an apostrophe.

---

## Framework-dependent publish (smaller)

```bat
scripts\build-framework-dependent.bat
```

Output:

```text
publish-fd\
```

Target PCs need the **[.NET 8 Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/8.0)** installed.

---

## Build from source manually (advanced)

If you prefer not to use **`build-self-contained.bat`**, the equivalent idea is:

```bat
dotnet restore
dotnet build Ordir.sln -c Release
dotnet publish src\Ordir.csproj -c Release -r win-x64 --self-contained true -o publish
```

If **`dotnet publish -o publish`** fails when the repo lives under a path with **`'`** in the name, use **`scripts\build-self-contained.bat`** instead (it uses a staging folder under **`%ProgramData%`**).

---

## Build

Summary table (same layout as the main **README**):

| Output | Command | Where it lands |
|--------|---------|----------------|
| **Self-contained publish** | `scripts\build-self-contained.bat` | **`ordir-main\publish\`** (`Ordir.exe` + runtime) |
| **Portable folder + zip** | `scripts\build-portable.bat` | **`ordir-main\dist\ordir-portable\`** and **`ordir-main\dist\ordir-portable-win-x64.zip`** |
| **Installer** | `scripts\build-installer.bat` | **`ordir-main\dist\ordir-setup.exe`** |
| **Framework-dependent** | `scripts\build-framework-dependent.bat` | **`ordir-main\publish-fd\`** |

**Portable release artifact:** attach **`dist\ordir-portable-win-x64.zip`** next to the installer, or zip **`dist\ordir-portable\`** after **`build-portable.bat`**. Both contain the self-contained app; the portable script also copies **`scripts\cli-launch`** for PATH / **`ordir`** usage.

---

## Full release build (recommended order)

From **`ordir-main`**, in order:

1. **`dotnet build Ordir.sln -c Release`** — refreshes **`src\bin\Release\…\Ordir.exe`** from the latest source.
2. **`scripts\build-portable.bat`** — fills **`publish\`**, **`dist\ordir-portable\`**, and **`dist\ordir-portable-win-x64.zip`**.
3. **`scripts\build-installer.bat`** — builds **`dist\ordir-setup.exe`** from the current **`publish\`** tree (requires Inno Setup 6).

```powershell
cd ordir-main
dotnet build Ordir.sln -c Release
scripts\build-portable.bat
scripts\build-installer.bat
```

If **`publish\`** still looks wrong after edits, run **`scripts\build-portable.bat`** again so **`publish\`** is regenerated before the installer step.

---

## `cli-launch` batch files (Unicode)

**`scripts\cli-launch\ordir.bat`** (banner / box-drawing): keep the file as **UTF-8 with BOM** and run **`chcp 65001`** before **`echo`** art so **cmd.exe** does not garble characters. UTF-8 *without* BOM often breaks after an editor save. Use **Save with Encoding → UTF-8 with BOM**, or rely on the repo **`.editorconfig`** for **`scripts\cli-launch\*.bat`**. Build scripts only **copy** this folder; they do not rewrite it.

---

## For contributors

- **Source:** **`src/`** — .NET 8 WPF (C#), project **`src\Ordir.csproj`**
- **Solution:** **`Ordir.sln`**
- **Quick dev run:** **`scripts\run-dev.bat`** or **`dotnet run --project src\Ordir.csproj`**
- **Info tab** copy: **`src\Assets\info-page.md`** (embedded at build as **`Assets/info-page.md`**). Markup subset: **`docs\info-page-guide.md`**.
