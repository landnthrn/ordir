# Install Ordir

This project supports common distribution paths: installer, portable zip, or manual run from a publish folder.

---

## Option A - Installer (recommended)

1. Build installer:

```bat
scripts\build-installer.bat
```

2. Share/run:

```text
dist\ordir-setup.exe
```

3. Default install location is `C:\Program Files\Ordir\`.
4. Setup offers optional PATH integration for the `ordir` command.

---

## Option B - Portable zip

1. Build portable artifacts:

```bat
scripts\build-portable.bat
```

2. Share:

```text
dist\ordir-portable-win-x64.zip
```

3. Extract and run `Ordir.exe` from the extracted folder.
4. Optional: add `scripts\cli-launch` to PATH for the `ordir` command.

---

## Option C - Manual self-contained publish

Build a standalone publish folder with .NET runtime included:

```bat
scripts\build-self-contained.bat
```

Output:

```text
publish\
```

Run `publish\Ordir.exe`.

---

## Option D - Framework-dependent publish (smaller)

Build:

```bat
scripts\build-framework-dependent.bat
```

Output:

```text
publish-fd\
```

Requires installing [.NET 8 Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/8.0) on the target machine.

---

## Build from source manually

Requirements:
- Windows
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

From the repository root:

```bat
dotnet restore
dotnet build Ordir.sln -c Release
dotnet publish src\Ordir.csproj -c Release -r win-x64 --self-contained true -o publish
```

---
