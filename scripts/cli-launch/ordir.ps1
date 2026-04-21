#Requires -Version 5.1

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

# Set console to use TrueType font for better Unicode support
try {
    $null = [System.Console]::InputEncoding = [System.Text.Encoding]::UTF8
    $null = [System.Console]::OutputEncoding = [System.Text.Encoding]::UTF8
} catch {
}

# ============================================================================
# GLOBAL VARIABLES
# ============================================================================
$script:SessionPath = $null
$script:SessionListPath = $null

# ============================================================================
# MENU DISPLAY FUNCTIONS
# ============================================================================

function Show-MainMenu {
    # Call the batch file to display the menu with proper encoding and colors
    $batPath = Join-Path $PSScriptRoot "ShowMenu.bat"
    & $batPath
}

# ============================================================================
# MANUAL MENU FUNCTION
# ============================================================================



function Show-ManualMenu {
    # Call the batch file to display the manual menu with proper encoding and colors
    $batPath = Join-Path $PSScriptRoot "ShowManualMenu.bat"
    & $batPath
}

# ============================================================================
# HELPER FUNCTIONS
# ============================================================================

function Get-Confirmation {
    param([string]$Message)
    
    Write-Host ""
    Write-Host $Message -ForegroundColor Magenta
    Write-Host ""
    Write-Host "Y - Yes" -ForegroundColor White
    Write-Host "S - Skip" -ForegroundColor White
    Write-Host "M - Back to menu" -ForegroundColor White
    Write-Host ""
    
    do {
        $response = Read-Host "Enter Y, S, or M"
        $response = $response.Trim().ToUpper()
        
        if ($response -eq 'Y') {
            return 'Y'
        }
        elseif ($response -eq 'S') {
            return 'S'
        }
        elseif ($response -eq 'M') {
            return 'M'
        }
        else {
            Write-Host "Invalid input. Please type Y (Yes), S (Skip), or M (Menu)." -ForegroundColor White
        }
    } while ($true)
}

function Get-ValidPath {
    param(
        [string]$Prompt,
        [bool]$MustExist = $true,
        [bool]$RequiresFilename = $false
    )
    
    while ($true) {
        $path = Read-Host $Prompt
        
        if ([string]::IsNullOrWhiteSpace($path)) {
            Write-Host "Path cannot be empty. Please try again." -ForegroundColor Red
            continue
        }
        
        if ($RequiresFilename) {
            $filename = Split-Path -Leaf $path
            if ([string]::IsNullOrWhiteSpace($filename) -or $filename -notmatch '\.[a-zA-Z0-9]+$') {
                Write-Host "ERROR: The path must include a filename at the end (e.g., C:\Folder\file.txt)" -ForegroundColor Red
                continue
            }
        }
        
        if ($MustExist) {
            if (Test-Path $path) {
                return $path
            } else {
                Write-Host "ERROR: Path does not exist. Please enter a valid path." -ForegroundColor Red
            }
        } else {
            # For output paths, create directory if it doesn't exist
            $parentDir = Split-Path -Parent $path
            if ($parentDir -and -not (Test-Path $parentDir)) {
                try {
                    New-Item -ItemType Directory -Path $parentDir -Force | Out-Null
                } catch {
                    Write-Host "ERROR: Cannot create directory. Please check the path." -ForegroundColor Red
                    continue
                }
            }
            return $path
        }
    }
}

function Get-ValidPathWithMenu {
    param(
        [string]$Prompt,
        [bool]$MustExist = $true,
        [bool]$RequiresFilename = $false
    )
    
    Write-Host ""
    Write-Host "M - Back to menu" -ForegroundColor White
    Write-Host ""
    
    while ($true) {
        $path = Read-Host $Prompt
        
        # Check for menu command
        $pathUpper = $path.Trim().ToUpper()
        if ($pathUpper -eq 'M') {
            return 'MENU'
        }
        
        if ([string]::IsNullOrWhiteSpace($path)) {
            Write-Host "Path cannot be empty. Please try again." -ForegroundColor Red
            continue
        }
        
        if ($RequiresFilename) {
            $filename = Split-Path -Leaf $path
            if ([string]::IsNullOrWhiteSpace($filename) -or $filename -notmatch '\.[a-zA-Z0-9]+$') {
                Write-Host "ERROR: The path must include a filename at the end (e.g., C:\Folder\file.txt)" -ForegroundColor Red
                continue
            }
        }
        
        if ($MustExist) {
            if (Test-Path $path) {
                return $path
            } else {
                Write-Host "ERROR: Path does not exist. Please enter a valid path." -ForegroundColor Red
            }
        } else {
            # For output paths, create directory if it doesn't exist
            $parentDir = Split-Path -Parent $path
            if ($parentDir -and -not (Test-Path $parentDir)) {
                try {
                    New-Item -ItemType Directory -Path $parentDir -Force | Out-Null
                } catch {
                    Write-Host "ERROR: Cannot create directory. Please check the path." -ForegroundColor Red
                    continue
                }
            }
            return $path
        }
    }
}

function Get-ValidPathWithOptions {
    param(
        [string]$Prompt,
        [bool]$MustExist = $true,
        [bool]$RequiresFilename = $false
    )
    
    Write-Host ""
    Write-Host "S - Skip" -ForegroundColor White
    Write-Host "M - Back to menu" -ForegroundColor White
    Write-Host ""
    
    while ($true) {
        $path = Read-Host $Prompt
        
        # Check for special commands
        $pathUpper = $path.Trim().ToUpper()
        if ($pathUpper -eq 'S') {
            return 'SKIP'
        }
        elseif ($pathUpper -eq 'M') {
            return 'MENU'
        }
        
        if ([string]::IsNullOrWhiteSpace($path)) {
            Write-Host "Path cannot be empty. Please try again." -ForegroundColor Red
            continue
        }
        
        if ($RequiresFilename) {
            $filename = Split-Path -Leaf $path
            if ([string]::IsNullOrWhiteSpace($filename) -or $filename -notmatch '\.[a-zA-Z0-9]+$') {
                Write-Host "ERROR: The path must include a filename at the end (e.g., C:\Folder\file.txt)" -ForegroundColor Red
                continue
            }
        }
        
        if ($MustExist) {
            if (Test-Path $path) {
                return $path
            } else {
                Write-Host "ERROR: Path does not exist. Please enter a valid path." -ForegroundColor Red
            }
        } else {
            # For output paths, create directory if it doesn't exist
            $parentDir = Split-Path -Parent $path
            if ($parentDir -and -not (Test-Path $parentDir)) {
                try {
                    New-Item -ItemType Directory -Path $parentDir -Force | Out-Null
                } catch {
                    Write-Host "ERROR: Cannot create directory. Please check the path." -ForegroundColor Red
                    continue
                }
            }
            return $path
        }
    }
}

function Get-ValidPathWithManualMenu {
    param(
        [string]$Prompt,
        [bool]$MustExist = $true,
        [bool]$RequiresFilename = $false,
        [string]$Example = $null
    )

    Write-Host ""
    Write-Host "MM - Back to manual menu" -ForegroundColor White
    Write-Host ""

    if ($Example) {
        Write-Host $Example
        Write-Host ""
    }
    
    while ($true) {
        $path = Read-Host $Prompt
        
        # Check for manual menu command
        $pathUpper = $path.Trim().ToUpper()
        if ($pathUpper -eq 'MM') {
            return 'MANUAL_MENU'
        }
        
        if ([string]::IsNullOrWhiteSpace($path)) {
            Write-Host "Path cannot be empty. Please try again." -ForegroundColor Red
            continue
        }
        
        if ($RequiresFilename) {
            $filename = Split-Path -Leaf $path
            if ([string]::IsNullOrWhiteSpace($filename) -or $filename -notmatch '\.[a-zA-Z0-9]+$') {
                Write-Host "ERROR: The path must include a filename at the end (e.g., C:\Folder\file.txt)" -ForegroundColor Red
                continue
            }
        }
        
        if ($MustExist) {
            if (Test-Path $path) {
                return $path
            } else {
                Write-Host "ERROR: Path does not exist. Please enter a valid path." -ForegroundColor Red
            }
        } else {
            # For output paths, create directory if it doesn't exist
            $parentDir = Split-Path -Parent $path
            if ($parentDir -and -not (Test-Path $parentDir)) {
                try {
                    New-Item -ItemType Directory -Path $parentDir -Force | Out-Null
                } catch {
                    Write-Host "ERROR: Cannot create directory. Please check the path." -ForegroundColor Red
                    continue
                }
            }
            return $path
        }
    }
}

function Get-NextAvailableFilename {
    param([string]$directory, [string]$baseName)

    $nameWithoutExtension = [System.IO.Path]::GetFileNameWithoutExtension($baseName)
    $extension = [System.IO.Path]::GetExtension($baseName)

    $counter = 2
    $newPath = Join-Path $directory $baseName

    while (Test-Path $newPath) {
        $newName = "$nameWithoutExtension`_$counter$extension"
        $newPath = Join-Path $directory $newName
        $counter++
    }

    return $newPath
}

function Show-OperationComplete {
    Write-Host ""
    Write-Host "Operation Complete!!" -ForegroundColor Green
    Write-Host ""
    Write-Host "To return to the menu:" -ForegroundColor White
    Write-Host "[" -NoNewline -ForegroundColor White
    Write-Host "menu" -NoNewline -ForegroundColor Green
    Write-Host "]" -ForegroundColor White
    Write-Host ""
    Write-Host "To return to the manual menu:" -ForegroundColor White
    Write-Host "[" -NoNewline -ForegroundColor White
    Write-Host "manual" -NoNewline -ForegroundColor Green
    Write-Host "]" -ForegroundColor White
    Write-Host ""
}

function Show-OperationCompleteWithInfo {
    Write-Host ""
    Write-Host "Operation Complete!!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Now you have to sort your folders by comments, see how by using this command:" -ForegroundColor White
    Write-Host "[" -NoNewline -ForegroundColor White
    Write-Host "info" -NoNewline -ForegroundColor Green
    Write-Host "]" -ForegroundColor White
    Write-Host ""
    Write-Host "To return to the menu:" -ForegroundColor White
    Write-Host "[" -NoNewline -ForegroundColor White
    Write-Host "menu" -NoNewline -ForegroundColor Green
    Write-Host "]" -ForegroundColor White
    Write-Host ""
    Write-Host "To return to the manual menu:" -ForegroundColor White
    Write-Host "[" -NoNewline -ForegroundColor White
    Write-Host "manual" -NoNewline -ForegroundColor Green
    Write-Host "]" -ForegroundColor White
    Write-Host ""
}

# ============================================================================
# MANUAL MODE COMMANDS - CATEGORY 1: CREATE DESKTOP.INI's
# ============================================================================

function Invoke-Cat1Op1 {
    Write-Host ""
    Write-Host "CREATE OPTION 1 ~ CREATE DESKTOP.INI INSIDE ALL FOLDERS INSIDE A FOLDER (PER-PARENT NUMBERING)" -ForegroundColor Magenta
    Write-Host ""
    
    $root = Get-ValidPathWithManualMenu "Enter the path of the folder where ALL folders inside it will receive desktop.ini files"
    if ($root -eq 'MANUAL_MENU') { Show-ManualMenu; return }
    
    $parentPaths = @($root) + (Get-ChildItem -Path $root -Directory -Recurse | Select-Object -ExpandProperty FullName)
    $totalCreated = 0
    
    foreach ($parent in $parentPaths) {
        $children = Get-ChildItem -Path $parent -Directory | Sort-Object Name
        if (-not $children) { continue }
        
        Write-Host "`nProcessing parent folder: $parent" -ForegroundColor Magenta
        $counter = 1
        
        foreach ($child in $children) {
            $iniPath = Join-Path $child.FullName 'desktop.ini'
            $ini = @"
[ViewState]
Mode=
Vid=
FolderType=Generic

[.ShellClassInfo]
ConfirmFileOp=0
InfoTip=#$counter
"@
            
            Set-Content -Path $iniPath -Value $ini -Encoding Unicode
            attrib +s "$($child.FullName)"
            attrib -h "$iniPath"
            Write-Host "Created desktop.ini → $($child.FullName)   [InfoTip #$counter]" -ForegroundColor Green
            $counter++
            $totalCreated++
        }
    }
    
    Write-Host "`nCompleted. $totalCreated desktop.ini files created across all subfolders." -ForegroundColor Green
    Write-Host ""

    Show-OperationComplete
}

function Invoke-Cat1Op2 {
    Write-Host ""
    Write-Host "CREATE OPTION 2 ~ CREATE DESKTOP.INI INSIDE FIRST FOREFRONT SUBFOLDERS INSIDE A FOLDER" -ForegroundColor Magenta
    Write-Host ""
    
    $root = Get-ValidPathWithManualMenu "Enter the path of the folder that contains the first forefront subfolders to receive desktop.ini files"
    if ($root -eq 'MANUAL_MENU') { Show-ManualMenu; return }
    
    $counter = 1
    $folders = Get-ChildItem -Path $root -Directory | Sort-Object Name
    $created = 0
    
    foreach ($folder in $folders) {
        $iniPath = Join-Path $folder.FullName 'desktop.ini'
        $ini = @"
[ViewState]
Mode=
Vid=
FolderType=Generic

[.ShellClassInfo]
ConfirmFileOp=0
InfoTip=#$counter
"@
        Set-Content -Path $iniPath -Value $ini -Encoding Unicode
        attrib +s "$($folder.FullName)"
        attrib -h "$iniPath"
        Write-Host "Created desktop.ini → $($folder.FullName)   [InfoTip #$counter]" -ForegroundColor Green
        $counter++
        $created++
    }
    
    Write-Host "`nCompleted. $created desktop.ini files created in first-level subfolders." -ForegroundColor Green
    Write-Host ""

    Show-OperationComplete
}

function Invoke-Cat1Op3 {
    Write-Host ""
    Write-Host "CREATE OPTION 3 ~ CREATE DESKTOP.INI INSIDE JUST ONE FOLDER" -ForegroundColor Magenta
    Write-Host ""
    
    $folder = Get-ValidPathWithManualMenu "Enter the folder path where you want to create a desktop.ini file"
    if ($folder -eq 'MANUAL_MENU') { Show-ManualMenu; return }
    $number = Read-Host "Enter the InfoTip number (default = 1)"
    if ([string]::IsNullOrWhiteSpace($number)) { $number = 1 }
    
    $path = Join-Path $folder 'desktop.ini'
    
    @"
[ViewState]
Mode=
Vid=
FolderType=Generic

[.ShellClassInfo]
ConfirmFileOp=0
InfoTip=#$number
"@ | Set-Content -Path $path -Encoding Unicode
    
    attrib +s (Split-Path $path)
    attrib -h $path
    
    Write-Host "`nCreated desktop.ini → $folder   [InfoTip #$number]" -ForegroundColor Green
    Write-Host "Completed. Desktop.ini successfully created in the specified folder." -ForegroundColor Green
    Write-Host ""

    Show-OperationComplete
}

# ============================================================================
# MANUAL MODE COMMANDS - CATEGORY 2: MAKE SYSTEM FOLDERS
# ============================================================================

function Invoke-Cat2Op1 {
    Write-Host ""
    Write-Host "SYSTEM FOLDER OPTION 1 ~ MAKE ALL FOLDERS INSIDE A FOLDER SYSTEM FOLDERS" -ForegroundColor Magenta
    Write-Host ""
    
    $root = Get-ValidPathWithManualMenu "Enter the path of the folder where ALL folders inside it will be made into system folders"
    if ($root -eq 'MANUAL_MENU') { Show-ManualMenu; return }
    
    $folders = Get-ChildItem -Path $root -Directory -Recurse
    $changed = 0
    
    foreach ($folder in $folders) {
        $iniPath = Join-Path $folder.FullName 'desktop.ini'
        if (Test-Path $iniPath) {
            attrib +s "$($folder.FullName)"
            Write-Host "Marked as system folder → $($folder.FullName)" -ForegroundColor Green
            $changed++
        } else {
            Write-Host "Skipped (no desktop.ini) → $($folder.FullName)" -ForegroundColor Magenta
        }
    }
    
    Write-Host "`nCompleted. $changed subfolders marked as system folders." -ForegroundColor Green
    Write-Host ""

    Show-OperationComplete
}

function Invoke-Cat2Op2 {
    Write-Host ""
    Write-Host "SYSTEM FOLDER OPTION 2 ~ MAKE FIRST FOREFRONT SUBFOLDERS INSIDE A FOLDER SYSTEM FOLDERS" -ForegroundColor Magenta
    Write-Host ""
    
    $root = Get-ValidPathWithManualMenu "Enter the path of the folder that contains the first forefront subfolders that will be made into system folders"
    if ($root -eq 'MANUAL_MENU') { Show-ManualMenu; return }
    
    $folders = Get-ChildItem -Path $root -Directory
    $changed = 0
    
    foreach ($folder in $folders) {
        $iniPath = Join-Path $folder.FullName 'desktop.ini'
        if (Test-Path $iniPath) {
            attrib +s "$($folder.FullName)"
            Write-Host "Marked as system folder → $($folder.FullName)" -ForegroundColor Green
            $changed++
        } else {
            Write-Host "Skipped (no desktop.ini) → $($folder.FullName)" -ForegroundColor Magenta
        }
    }
    
    Write-Host "`nCompleted. $changed folders marked as system folders." -ForegroundColor Green
    Write-Host ""

    Show-OperationComplete
}

function Invoke-Cat2Op3 {
    Write-Host ""
    Write-Host "SYSTEM FOLDER OPTION 3 ~ MAKE JUST ONE FOLDER A SYSTEM FOLDER" -ForegroundColor Magenta
    Write-Host ""
    
    $folder = Get-ValidPathWithManualMenu "Enter the path of the folder you want to make a system folder"
    if ($folder -eq 'MANUAL_MENU') { Show-ManualMenu; return }
    $iniPath = Join-Path $folder 'desktop.ini'
    
    if (Test-Path $iniPath) {
        attrib +s "$folder"
        Write-Host "Marked as system folder → $folder" -ForegroundColor Green
    } else {
        Write-Host "Skipped (no desktop.ini found) → $folder" -ForegroundColor Magenta
    }
    
    Write-Host "`nCompleted. Action finished for the selected folder." -ForegroundColor Green
    Write-Host ""

    Show-OperationComplete
}

# ============================================================================
# MANUAL MODE COMMANDS - CATEGORY 3: CREATE LISTS
# ============================================================================

function Get-InfoTip {
    param([string]$folderPath)
    $ini = Join-Path $folderPath 'desktop.ini'
    if (Test-Path $ini) {
        $text = Get-Content $ini -Raw -ErrorAction SilentlyContinue
        if ($text -match '(?m)^\s*InfoTip\s*=\s*(.+?)\s*$') {
            return ($Matches[1].Trim())
        }
    }
    return ''
}

function Invoke-Cat3Op1 {
    # CREATE LIST OF ALL DESKTOP.INI FILES INSIDE ALL FOLDERS IN A FOLDER
# (Depth-first order: each folder printed immediately before its children)

$root = Get-ValidPathWithManualMenu "Enter the path of the folder where ALL folders inside it will be scanned and made into a list"
if ($root -eq 'MANUAL_MENU') { Show-ManualMenu; return }

$outDir = Get-ValidPathWithManualMenu "Enter the folder path where the list should be created"
if ($outDir -eq 'MANUAL_MENU') { Show-ManualMenu; return }

# Ensure output directory exists
if (-not (Test-Path $outDir)) { 
    New-Item -ItemType Directory -Path $outDir -Force | Out-Null 
}

# Build output filename automatically
$folderName = Split-Path $root -Leaf
$baseName = "List_For_ALL_Folders_Inside_$folderName.txt"
$outPath  = Join-Path $outDir $baseName
if (Test-Path $outPath) {
    $outPath = Get-NextAvailableFilename $outDir $baseName
}

function Get-InfoTip {
    param([string]$folderPath)
    $ini = Join-Path $folderPath 'desktop.ini'
    if (Test-Path $ini) {
        $text = Get-Content $ini -Raw -ErrorAction SilentlyContinue
        if ($text -match '(?m)^\s*InfoTip\s*=\s*(.+?)\s*$') {
            return ($Matches[1].Trim())
        }
    }
    return ''
}

$lines = New-Object System.Collections.Generic.List[string]
$totalSections = 0
$totalEntries = 0

function Process-Folder {
    param([string]$parentPath)

    $children = Get-ChildItem -Path $parentPath -Directory | Sort-Object Name
    if ($children.Count -gt 0) {
        $folderName = Split-Path $parentPath -Leaf
        $lines.Add("$folderName Folder")
        $lines.Add($parentPath + '\')

        foreach ($child in $children) {
            $tip = Get-InfoTip $child.FullName
            if (-not $tip) { $tip = '' }
            $lines.Add("$($child.Name) $tip")
            Write-Host "Added → $($child.Name) $tip" -ForegroundColor Green
            $global:totalEntries++
        }

        $lines.Add('')
        $lines.Add('')
        $global:totalSections++
        Write-Host "Section logged: $folderName Folder ($($children.Count) entries)" -ForegroundColor Cyan

        # 🔁 Recurse into each child to list its own subfolders next
        foreach ($child in $children) {
            Process-Folder $child.FullName
        }
    }
}

# Start recursive listing from the root
Process-Folder $root

Set-Content -Path $outPath -Value $lines -Encoding UTF8

Write-Host "`nList created successfully." -ForegroundColor Green
Write-Host "$totalSections sections, $totalEntries folders total." -ForegroundColor Green
Write-Host "Saved to: $outPath" -ForegroundColor White
Write-Host ""

Show-OperationComplete
}

function Invoke-Cat3Op2 {
    Write-Host ""
    Write-Host "LIST OPTION 2 ~ CREATE A LIST OF ALL DESKTOP.INI'S INSIDE FIRST FOREFRONT SUBFOLDERS INSIDE A FOLDER" -ForegroundColor Magenta
    Write-Host ""
    
    $root = Get-ValidPathWithManualMenu "Enter the path of the folder to scan for its first forefront subfolders"
    if ($root -eq 'MANUAL_MENU') { Show-ManualMenu; return }
    $outDir = Get-ValidPathWithManualMenu "Enter the path where you want the list to be created"
    if ($outDir -eq 'MANUAL_MENU') { Show-ManualMenu; return }
    
    if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }
    
    $folderName = Split-Path $root -Leaf
    $baseName = "List_For_First_Subfolders_Inside_$folderName.txt"
    $outPath = Join-Path $outDir $baseName
    if (Test-Path $outPath) {
        $outPath = Get-NextAvailableFilename $outDir $baseName
    }
    
    $lines = New-Object System.Collections.Generic.List[string]
    
    $lines.Add("$folderName Folder")
    $lines.Add($root + '\')
    
    $folders = Get-ChildItem -Path $root -Directory | Sort-Object Name
    $count = 0
    
    foreach ($f in $folders) {
        $tip = Get-InfoTip $f.FullName
        if (-not $tip) { $tip = '' }
        $lines.Add("$($f.Name) $tip")
        Write-Host "Added → $($f.Name) $tip" -ForegroundColor Green
        $count++
    }
    
    Set-Content -Path $outPath -Value $lines -Encoding UTF8
    Write-Host "`nList created with $count entries." -ForegroundColor Green
    Write-Host "Saved to: $outPath" -ForegroundColor White
    Write-Host ""

    Show-OperationComplete
}

# ============================================================================
# MANUAL MODE COMMANDS - CATEGORY 4: APPLY LISTS
# ============================================================================

function Invoke-Cat4Op1 {
    Write-Host ""
    Write-Host "APPLY LIST OPTION 1 ~ APPLY A LIST FOR ALL FOLDERS INSIDE A FOLDER" -ForegroundColor Magenta
    Write-Host ""

    $listPath = Get-ValidPathWithManualMenu "Enter the FULL path of the list file to apply: " -Example "Example: C:\Desktop\List_For_ALL_Folders_Inside_TESTER.txt"
    if ($listPath -eq 'MANUAL_MENU') { Show-ManualMenu; return }

    if (-not (Test-Path $listPath)) {
        Write-Host "ERROR: List file not found." -ForegroundColor Red
        Write-Host ""
        return
    }
    
    $lines = Get-Content -Path $listPath -Raw -Encoding UTF8
    $lines = $lines -split "`r?`n"
    
    $currentParent = $null
    $updated = 0
    $skipped = 0
    
    foreach ($line in $lines) {
        $trim = $line.Trim()
        
        if ($trim -match '^[A-Za-z]:\\') {
            $currentParent = $trim.TrimEnd('\')
            continue
        }
        
        if (-not $trim -or $trim -match 'Folder$') { continue }
        
        if ($trim -match '^(.+?)\s+#(\d+)$' -and $currentParent) {
            $folderName = $matches[1].Trim()
            $infoTip = "#$($matches[2])"
            $targetFolder = Join-Path $currentParent $folderName
            $iniPath = Join-Path $targetFolder 'desktop.ini'
            
            if (Test-Path $targetFolder) {
                if (Test-Path $iniPath) {
                    $ini = Get-Content $iniPath -Raw -ErrorAction SilentlyContinue
                    
                    if ($ini -match '(?m)^\s*InfoTip\s*=') {
                        $new = [regex]::Replace($ini, '(?m)^\s*InfoTip\s*=.*$', "InfoTip=$infoTip")
                    }
                    elseif ($ini -match '(?m)^\s*\[\.ShellClassInfo\]\s*$') {
                        $new = [regex]::Replace($ini, '(?m)^\s*\[\.ShellClassInfo\]\s*$', "[.ShellClassInfo]`r`nInfoTip=$infoTip")
                    }
                    else {
                        $new = $ini.TrimEnd() + "`r`n`r`n[.ShellClassInfo]`r`nInfoTip=$infoTip"
                    }
                    
                    Set-Content -Path $iniPath -Value $new -Encoding Unicode
                    Write-Host "Updated → $folderName ($infoTip)" -ForegroundColor Green
                    $updated++
                } else {
                    Write-Host "Skipped → $folderName   [No desktop.ini]" -ForegroundColor Magenta
                    $skipped++
                }
            } else {
                Write-Host "Skipped → $folderName   [Folder not found]" -ForegroundColor Magenta
                $skipped++
            }
        }
    }

    Write-Host "`n$updated folders updated, $skipped skipped." -ForegroundColor Green
    Write-Host ""

    Show-OperationCompleteWithInfo
}

function Invoke-Cat4Op2 {
    Write-Host ""
    Write-Host "APPLY LIST OPTION 2 ~ APPLY A LIST FOR FIRST FOREFRONT SUBFOLDERS INSIDE A FOLDER" -ForegroundColor Magenta
    Write-Host ""

    $listPath = Get-ValidPathWithManualMenu "Enter the FULL path of the list file to apply: " -Example "Example: C:\Desktop\List_For_First_Subfolders_Inside_TESTER.txt"
    if ($listPath -eq 'MANUAL_MENU') { Show-ManualMenu; return }
    
    if (-not (Test-Path $listPath)) {
        Write-Host "ERROR: List file not found." -ForegroundColor Red
        Write-Host ""
        return
    }
    
    $lines = Get-Content -Path $listPath -Raw -Encoding UTF8
    $lines = $lines -split "`r?`n"
    
    $currentParent = $null
    $updated = 0
    $skipped = 0
    
    foreach ($line in $lines) {
        $trim = $line.Trim()
        
        if ($trim -match '^[A-Za-z]:\\') {
            $currentParent = $trim.TrimEnd('\')
            continue
        }
        
        if (-not $trim -or $trim -match 'Folder$') { continue }
        
        if ($trim -match '^(.+?)\s+#(\d+)$' -and $currentParent) {
            $folderName = $matches[1].Trim()
            $newTip = $matches[2]
            
            $folderPath = Join-Path $currentParent $folderName
            if (Test-Path $folderPath) {
                $iniPath = Join-Path $folderPath 'desktop.ini'
                if (Test-Path $iniPath) {
                    $content = Get-Content -Path $iniPath -Raw -Encoding Unicode
                    $content = $content -replace 'InfoTip=#\d+', "InfoTip=#$newTip"
                    Set-Content -Path $iniPath -Value $content -Encoding Unicode
                    Write-Host "Updated → $folderPath   [InfoTip #$newTip]" -ForegroundColor Green
                    $updated++
                } else {
                    Write-Host "Skipped → $folderPath   [No desktop.ini]" -ForegroundColor Magenta
                    $skipped++
                }
            } else {
                Write-Host "Skipped → $folderPath   [Folder not found]" -ForegroundColor Magenta
                $skipped++
            }
        }
    }
    
    Write-Host "`n$updated folders updated, $skipped skipped." -ForegroundColor Green
    Write-Host ""

    Show-OperationCompleteWithInfo
}

# ============================================================================
# MANUAL MODE COMMANDS - CATEGORY 5: HIDE DESKTOP.INI's
# ============================================================================

function Invoke-Cat5Op1 {
    Write-Host ""
    Write-Host "HIDE ALL DESKTOP.INI FILES INSIDE ALL FOLDERS IN A FOLDER" -ForegroundColor Magenta
    Write-Host ""
    
    $root = Get-ValidPathWithManualMenu "Enter the path of the folder where ALL folders' desktop.ini files will be hidden"
    if ($root -eq 'MANUAL_MENU') { Show-ManualMenu; return }
    
    $files = Get-ChildItem -Path $root -Filter 'desktop.ini' -Recurse -File -Force -ErrorAction SilentlyContinue
    $count = 0

    foreach ($file in $files) {
        attrib +s +h `"$($file.FullName)`"
        Write-Host "Hidden → $($file.FullName)" -ForegroundColor Green
        $count++
    }
    
    Write-Host "`nCompleted. $count desktop.ini files hidden." -ForegroundColor Green
    Write-Host ""

    Show-OperationComplete
}

function Invoke-Cat5Op2 {
    Write-Host ""
    Write-Host "HIDE ALL DESKTOP.INI'S INSIDE FIRST FOREFRONT SUBFOLDERS OF A FOLDER" -ForegroundColor Magenta
    Write-Host ""
    
    $root = Get-ValidPathWithManualMenu "Enter the path of the folder where the first forefront subfolders' desktop.ini files will be hidden"
    if ($root -eq 'MANUAL_MENU') { Show-ManualMenu; return }
    
    $subfolders = Get-ChildItem -Path $root -Directory -Force -ErrorAction SilentlyContinue
    $count = 0

    foreach ($folder in $subfolders) {
        $iniPath = Join-Path $folder.FullName 'desktop.ini'
        if (Test-Path $iniPath) {
            attrib +s +h `"$iniPath`"
            Write-Host "Hidden → $iniPath" -ForegroundColor Green
            $count++
        }
    }
    
    Write-Host "`nCompleted. $count desktop.ini files hidden." -ForegroundColor Green
    Write-Host ""

    Show-OperationComplete
}

function Invoke-Cat5Op3 {
    Write-Host ""
    Write-Host "HIDE A DESKTOP.INI INSIDE A FOLDER" -ForegroundColor Magenta
    Write-Host ""
    
    $folder = Get-ValidPathWithManualMenu "Enter the path of the folder containing the desktop.ini to hide"
    if ($folder -eq 'MANUAL_MENU') { Show-ManualMenu; return }
    $ini = Join-Path $folder 'desktop.ini'
    
    if (Test-Path $ini) {
        attrib +s +h `"$ini`"
        Write-Host "Hidden → $ini" -ForegroundColor Green
    } else {
        Write-Host "No desktop.ini found in: $folder" -ForegroundColor Magenta
    }
    Write-Host ""

    Show-OperationComplete
}

# ============================================================================
# MANUAL MODE COMMANDS - CATEGORY 6: UNHIDE DESKTOP.INI's
# ============================================================================

function Invoke-Cat6Op1 {
    Write-Host ""
    Write-Host "UNHIDE ALL DESKTOP.INI FILES INSIDE ALL FOLDERS IN A FOLDER" -ForegroundColor Magenta
    Write-Host ""
    
    $root = Get-ValidPathWithManualMenu "Enter the path of the folder where ALL folders' desktop.ini files will be unhidden"
    if ($root -eq 'MANUAL_MENU') { Show-ManualMenu; return }

    $files = Get-ChildItem -Path $root -Filter 'desktop.ini' -Recurse -File -Force -ErrorAction SilentlyContinue
    $count = 0
    
    foreach ($file in $files) {
        attrib -s -h $file.FullName
        Write-Host "Unhidden → $($file.FullName)" -ForegroundColor Green
        $count++
    }
    
    Write-Host "`nCompleted. $count desktop.ini files unhidden." -ForegroundColor Green
    Write-Host ""

    Show-OperationComplete
}

function Invoke-Cat6Op2 {
    Write-Host ""
    Write-Host "UNHIDE ALL DESKTOP.INI'S INSIDE FIRST FOREFRONT SUBFOLDERS OF A FOLDER" -ForegroundColor Magenta
    Write-Host ""
    
    $root = Get-ValidPathWithManualMenu "Enter the path of the folder where the first forefront subfolders' desktop.ini files will be unhidden"
    if ($root -eq 'MANUAL_MENU') { Show-ManualMenu; return }
    
    $subfolders = Get-ChildItem -Path $root -Directory -Force -ErrorAction SilentlyContinue
    $count = 0

    foreach ($folder in $subfolders) {
        $iniPath = Join-Path $folder.FullName 'desktop.ini'
        if (Test-Path $iniPath) {
            attrib -s -h `"$iniPath`"
            Write-Host "Unhidden → $iniPath" -ForegroundColor Green
            $count++
        }
    }
    
    Write-Host "`nCompleted. $count desktop.ini files unhidden." -ForegroundColor Green
    Write-Host ""

    Show-OperationComplete
}

function Invoke-Cat6Op3 {
    Write-Host ""
    Write-Host "UNHIDE A DESKTOP.INI INSIDE A FOLDER" -ForegroundColor Magenta
    Write-Host ""
    
    $folder = Get-ValidPathWithManualMenu "Enter the path of the folder containing the desktop.ini to unhide"
    if ($folder -eq 'MANUAL_MENU') { Show-ManualMenu; return }
    $ini = Join-Path $folder 'desktop.ini'
    
    if (Test-Path $ini) {
        attrib -s -h `"$ini`"
        Write-Host "Unhidden → $ini" -ForegroundColor Green
    } else {
        Write-Host "No desktop.ini found in: $folder" -ForegroundColor Magenta
    }
    Write-Host ""

    Show-OperationComplete
}

# ============================================================================
# MANUAL MODE COMMANDS - CATEGORY 7: DELETE DESKTOP.INI's
# ============================================================================

function Invoke-Cat7Op1 {
    Write-Host ""
    Write-Host "DELETE ALL DESKTOP.INI FILES INSIDE ALL FOLDERS IN A FOLDER" -ForegroundColor Magenta
    Write-Host ""
    
    $root = Get-ValidPathWithManualMenu "Enter the path of the folder where ALL folders' desktop.ini files will be deleted"
    if ($root -eq 'MANUAL_MENU') { Show-ManualMenu; return }
    
    $deleted = 0
    $files = Get-ChildItem -Path $root -Filter 'desktop.ini' -Recurse -Force -ErrorAction SilentlyContinue
    
    foreach ($file in $files) {
        Remove-Item -Path $file.FullName -Force -ErrorAction SilentlyContinue
        Write-Host "Deleted → $($file.FullName)" -ForegroundColor Green
        $deleted++
    }
    
    Write-Host "`nCompleted. $deleted desktop.ini files deleted." -ForegroundColor Green
    Write-Host ""

    Show-OperationComplete
}

function Invoke-Cat7Op2 {
    Write-Host ""
    Write-Host "DELETE ALL DESKTOP.INI FILES INSIDE FIRST FOREFRONT SUBFOLDERS OF A FOLDER" -ForegroundColor Magenta
    Write-Host ""
    
    $root = Get-ValidPathWithManualMenu "Enter the path of the folder where the first forefront subfolders' desktop.ini files will be deleted"
    if ($root -eq 'MANUAL_MENU') { Show-ManualMenu; return }
    
    $deleted = 0
    Get-ChildItem -Path $root -Directory -Force | ForEach-Object {
        $iniPath = Join-Path $_.FullName 'desktop.ini'
        if (Test-Path $iniPath) {
            Remove-Item -Path $iniPath -Force -ErrorAction SilentlyContinue
            Write-Host "Deleted → $iniPath" -ForegroundColor Green
            $deleted++
        }
    }
    
    Write-Host "`nCompleted. $deleted desktop.ini files deleted." -ForegroundColor Green
    Write-Host ""

    Show-OperationComplete
}

# ============================================================================
# NON-MANUAL MODE COMMANDS - AUTOMATED WORKFLOWS
# ============================================================================

function Invoke-Op1 {
    Write-Host ""
    Write-Host "AUTOMATED WORKFLOW: All Folders Inside a Folder" -ForegroundColor Magenta
    Write-Host ""
    
    # Step 1: Get folder path
    $script:SessionPath = Get-ValidPathWithMenu "Enter the path of the folder where ALL folders inside it will be organized"
    if ($script:SessionPath -eq 'MENU') { Show-MainMenu; return }
    
    # Step 2: Create Desktop.ini's
    $response = Get-Confirmation "Ready to create the Desktop.ini's?"
    if ($response -eq 'M') { Show-MainMenu; return }
    
    if ($response -eq 'Y') {
        $parentPaths = @($script:SessionPath) + (Get-ChildItem -Path $script:SessionPath -Directory -Recurse | Select-Object -ExpandProperty FullName)
        $totalCreated = 0
        
        foreach ($parent in $parentPaths) {
            $children = Get-ChildItem -Path $parent -Directory | Sort-Object Name
            if (-not $children) { continue }
            
            $counter = 1
            foreach ($child in $children) {
                $iniPath = Join-Path $child.FullName 'desktop.ini'
                $ini = @"
[ViewState]
Mode=
Vid=
FolderType=Generic

[.ShellClassInfo]
ConfirmFileOp=0
InfoTip=#$counter
"@
                
                Set-Content -Path $iniPath -Value $ini -Encoding Unicode
                attrib +s "$($child.FullName)"
                attrib -h "$iniPath"
                Write-Host "Created desktop.ini → $($child.FullName)   [InfoTip #$counter]" -ForegroundColor Green
                $counter++
                $totalCreated++
            }
        }
        Write-Host "`n$totalCreated desktop.ini files created." -ForegroundColor Green
    } else {
        Write-Host "`nSkipped creating desktop.ini files." -ForegroundColor Magenta
    }
    
    # Step 3: Make System Folders
    $response = Get-Confirmation "Ready to make them System Folders?"
    if ($response -eq 'M') { Show-MainMenu; return }
    
    if ($response -eq 'Y') {
        $folders = Get-ChildItem -Path $script:SessionPath -Directory -Recurse
        $changed = 0
        
        foreach ($folder in $folders) {
            $iniPath = Join-Path $folder.FullName 'desktop.ini'
            if (Test-Path $iniPath) {
                attrib +s "$($folder.FullName)"
                Write-Host "Marked as system folder → $($folder.FullName)" -ForegroundColor Green
                $changed++
            }
        }
        Write-Host "`n$changed folders marked as system folders." -ForegroundColor Green
    } else {
        Write-Host "`nSkipped making system folders." -ForegroundColor Magenta
    }
    
    # Step 4: Get list output path
    Write-Host ""
    Write-Host "Ready to create the list?" -ForegroundColor Magenta
    Write-Host ""
    $outDir = Get-ValidPathWithOptions "Enter the path where the list should be created"
    if ($outDir -eq 'MENU') { Show-MainMenu; return }
    if ($outDir -eq 'SKIP') { 
        Write-Host "`nSkipped creating list." -ForegroundColor Magenta
    } else {
        if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }
    
    # Step 5: Create list
    $folderName = Split-Path $script:SessionPath -Leaf
    $baseName = "List_For_ALL_Folders_Inside_$folderName.txt"
    $script:SessionListPath = Join-Path $outDir $baseName
    if (Test-Path $script:SessionListPath) {
        $script:SessionListPath = Get-NextAvailableFilename $outDir $baseName
    }
    
    $lines = New-Object System.Collections.Generic.List[string]
    $totalSections = 0
    $totalEntries = 0
    
    function Process-Folder {
        param([string]$parentPath)
        
        $children = Get-ChildItem -Path $parentPath -Directory | Sort-Object Name
        if ($children.Count -gt 0) {
            $folderName = Split-Path $parentPath -Leaf
            $lines.Add("$folderName Folder")
            $lines.Add($parentPath + '\')
            
            foreach ($child in $children) {
                $tip = Get-InfoTip $child.FullName
                if (-not $tip) { $tip = '' }
                $lines.Add("$($child.Name) $tip")
                Write-Host "Added → $($child.Name) $tip" -ForegroundColor Green
                $global:totalEntries++
            }
            
            $lines.Add('')
            $lines.Add('')
            $global:totalSections++
            Write-Host "Section logged: $folderName Folder ($($children.Count) entries)" -ForegroundColor Cyan
            
            # 🔁 Recurse into each child to list its own subfolders next
            foreach ($child in $children) {
                Process-Folder $child.FullName
            }
        }
    }
    
    # Start recursive listing from the root
    Process-Folder $script:SessionPath
    
    Set-Content -Path $script:SessionListPath -Value $lines -Encoding UTF8
    Write-Host "`nList created successfully." -ForegroundColor Green
    Write-Host "$totalSections sections, $totalEntries folders total." -ForegroundColor Green
    Write-Host "Saved to: $script:SessionListPath" -ForegroundColor White

    # Automatically open in Notepad
    $notepadProcess = Start-Process notepad.exe -ArgumentList $script:SessionListPath -PassThru

    # Step 6: Wait for user to finish editing
    Write-Host "`nEdit the list, save it, and close Notepad to continue." -ForegroundColor Magenta
    Write-Host "" -ForegroundColor Magenta
    Write-Host "Example of how to reorder in the list:" -ForegroundColor Magenta
    Write-Host "" -ForegroundColor Magenta
    Write-Host "February #1                     February #2" -ForegroundColor Magenta
    Write-Host "January #2       ------>        January #1" -ForegroundColor Magenta
    Write-Host "March #3                        March #3" -ForegroundColor Magenta
    Write-Host "" -ForegroundColor Magenta
    Write-Host "Press any key when you're done editing..." -ForegroundColor Magenta

    # Wait for either notepad to close OR user to press a key
    $continue = $false

    while (-not $continue) {
        # Check if notepad has closed
        if ($notepadProcess.HasExited) {
            $continue = $true
            break
        }

        # Check if a key was pressed (non-blocking)
        if ($Host.UI.RawUI.KeyAvailable) {
            $key = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
            $continue = $true
            break
        }

        # Brief pause before checking again
        Start-Sleep -Milliseconds 500
    }

    # Step 7: Confirm after notepad closes
    $response = Get-Confirmation "Ready to apply your changes from the list?"
    if ($response -eq 'M') { Show-MainMenu; return }

        if ($response -eq 'Y') {
            # Process the changes after Notepad closes
            $lines = Get-Content -Path $script:SessionListPath -Raw -Encoding UTF8
            $lines = $lines -split "`r?`n"

            $currentParent = $null
            $updated = 0
            $skipped = 0

            foreach ($line in $lines) {
                $trim = $line.Trim()

                if ($trim -match '^[A-Za-z]:\\') {
                    $currentParent = $trim.TrimEnd('\')
                    continue
                }

                if (-not $trim -or $trim -match 'Folder$') { continue }

                if ($trim -match '^(.+?)\s+#(\d+)$' -and $currentParent) {
                    $folderName = $matches[1].Trim()
                    $infoTip = "#$($matches[2])"
                    $targetFolder = Join-Path $currentParent $folderName
                    $iniPath = Join-Path $targetFolder 'desktop.ini'

                    if (Test-Path $targetFolder) {
                        if (Test-Path $iniPath) {
                            $ini = Get-Content $iniPath -Raw -ErrorAction SilentlyContinue

                            if ($ini -match '(?m)^\s*InfoTip\s*=') {
                                $new = [regex]::Replace($ini, '(?m)^\s*InfoTip\s*=.*$', "InfoTip=$infoTip")
                            }
                            elseif ($ini -match '(?m)^\s*\[\.ShellClassInfo\]\s*$') {
                                $new = [regex]::Replace($ini, '(?m)^\s*\[\.ShellClassInfo\]\s*$', "[.ShellClassInfo]`r`nInfoTip=$infoTip")
                            }
                            else {
                                $new = $ini.TrimEnd() + "`r`n`r`n[.ShellClassInfo]`r`nInfoTip=$infoTip"
                            }

                            Set-Content -Path $iniPath -Value $new -Encoding Unicode
                            Write-Host "Updated → $folderName ($infoTip)" -ForegroundColor Green
                            $updated++
                        } else {
                            Write-Host "Skipped → $folderName   [No desktop.ini]" -ForegroundColor Magenta
                            $skipped++
                        }
                    } else {
                        Write-Host "Skipped → $folderName   [Folder not found]" -ForegroundColor Magenta
                        $skipped++
                    }
                }
            }

            Write-Host "`n$updated folders updated, $skipped skipped." -ForegroundColor Green
        } else {
            Write-Host "`nSkipped applying changes from list." -ForegroundColor Magenta
        }
    }
    
    # Step 8: Hide Desktop.ini's
    $response = Get-Confirmation "Ready to hide the Desktop.ini's?"
    if ($response -eq 'M') { Show-MainMenu; return }
    
    if ($response -eq 'Y') {
        $files = Get-ChildItem -Path $script:SessionPath -Filter 'desktop.ini' -Recurse -File -Force -ErrorAction SilentlyContinue
        $hiddenCount = 0

        foreach ($file in $files) {
            attrib +s +h `"$($file.FullName)`"
            Write-Host "Hidden → $($file.FullName)" -ForegroundColor Green
            $hiddenCount++
        }
        
        Write-Host "`n$hiddenCount desktop.ini files hidden." -ForegroundColor Green
    } else {
        Write-Host "`nSkipped hiding desktop.ini files." -ForegroundColor Magenta
    }
    
    Show-OperationComplete
}

function Invoke-Op2 {
    Write-Host ""
    Write-Host "AUTOMATED WORKFLOW: All First Forefront Subfolders Inside a Folder" -ForegroundColor Magenta
    Write-Host ""
    
    # Step 1: Get folder path
    $script:SessionPath = Get-ValidPathWithMenu "Enter the path of the folder that contains the first forefront subfolders to be organized"
    if ($script:SessionPath -eq 'MENU') { Show-MainMenu; return }
    
    # Step 2: Create Desktop.ini's
    $response = Get-Confirmation "Ready to create the Desktop.ini's?"
    if ($response -eq 'M') { Show-MainMenu; return }
    
    if ($response -eq 'Y') {
        $counter = 1
        $folders = Get-ChildItem -Path $script:SessionPath -Directory | Sort-Object Name
        $created = 0
        
        foreach ($folder in $folders) {
            $iniPath = Join-Path $folder.FullName 'desktop.ini'
            $ini = @"
[ViewState]
Mode=
Vid=
FolderType=Generic

[.ShellClassInfo]
ConfirmFileOp=0
InfoTip=#$counter
"@
            Set-Content -Path $iniPath -Value $ini -Encoding Unicode
            attrib +s "$($folder.FullName)"
            attrib -h "$iniPath"
            Write-Host "Created desktop.ini → $($folder.FullName)   [InfoTip #$counter]" -ForegroundColor Green
            $counter++
            $created++
        }
        Write-Host "`n$created desktop.ini files created." -ForegroundColor Green
    } else {
        Write-Host "`nSkipped creating desktop.ini files." -ForegroundColor Magenta
    }
    
    # Step 3: Make System Folders
    $response = Get-Confirmation "Ready to make them System Folders?"
    if ($response -eq 'M') { Show-MainMenu; return }
    
    if ($response -eq 'Y') {
        $folders = Get-ChildItem -Path $script:SessionPath -Directory | Sort-Object Name
        $changed = 0
        foreach ($folder in $folders) {
            $iniPath = Join-Path $folder.FullName 'desktop.ini'
            if (Test-Path $iniPath) {
                attrib +s "$($folder.FullName)"
                Write-Host "Marked as system folder → $($folder.FullName)" -ForegroundColor Green
                $changed++
            }
        }
        Write-Host "`n$changed folders marked as system folders." -ForegroundColor Green
    } else {
        Write-Host "`nSkipped making system folders." -ForegroundColor Magenta
    }
    
    # Step 4: Get list output path
    Write-Host ""
    Write-Host "Ready to create the list?" -ForegroundColor Magenta
    Write-Host ""
    $outDir = Get-ValidPathWithOptions "Enter the path where the list should be created"
    if ($outDir -eq 'MENU') { Show-MainMenu; return }
    if ($outDir -eq 'SKIP') { 
        Write-Host "`nSkipped creating list." -ForegroundColor Magenta
    } else {
        if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }
    
    # Step 5: Create list
    $folderName = Split-Path $script:SessionPath -Leaf
    $baseName = "List_For_First_Subfolders_Inside_$folderName.txt"
    $script:SessionListPath = Join-Path $outDir $baseName
    if (Test-Path $script:SessionListPath) {
        $script:SessionListPath = Get-NextAvailableFilename $outDir $baseName
    }
    
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("$folderName Folder")
    $lines.Add($script:SessionPath + '\')

    $folders = Get-ChildItem -Path $script:SessionPath -Directory | Sort-Object Name
    $count = 0
    foreach ($f in $folders) {
        $tip = Get-InfoTip $f.FullName
        if (-not $tip) { $tip = '' }
        $lines.Add("$($f.Name) $tip")
        Write-Host "Added → $($f.Name) $tip" -ForegroundColor Green
        $count++
    }
    
    Set-Content -Path $script:SessionListPath -Value $lines -Encoding UTF8
    Write-Host "`nList created with $count entries." -ForegroundColor Green
    Write-Host "Saved to: $script:SessionListPath" -ForegroundColor White

    # Automatically open in Notepad
    $notepadProcess = Start-Process notepad.exe -ArgumentList $script:SessionListPath -PassThru

    # Step 6: Wait for user to finish editing
    Write-Host "`nEdit the list, save it, and close Notepad to continue." -ForegroundColor Magenta
    Write-Host "" -ForegroundColor Magenta
    Write-Host "Example of how to reorder in the list:" -ForegroundColor Magenta
    Write-Host "" -ForegroundColor Magenta
    Write-Host "February #1                     February #2" -ForegroundColor Magenta
    Write-Host "January #2       ------>        January #1" -ForegroundColor Magenta
    Write-Host "March #3                        March #3" -ForegroundColor Magenta
    Write-Host "" -ForegroundColor Magenta
    Write-Host "Press any key when you're done editing..." -ForegroundColor Magenta

    # Wait for either notepad to close OR user to press a key
    $continue = $false

    while (-not $continue) {
        # Check if notepad has closed
        if ($notepadProcess.HasExited) {
            $continue = $true
            break
        }

        # Check if a key was pressed (non-blocking)
        if ($Host.UI.RawUI.KeyAvailable) {
            $key = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
            $continue = $true
            break
        }

        # Brief pause before checking again
        Start-Sleep -Milliseconds 500
    }

    # Step 7: Confirm after notepad closes
    $response = Get-Confirmation "Ready to apply your changes from the list?"
    if ($response -eq 'M') { Show-MainMenu; return }

        if ($response -eq 'Y') {
            # Process the changes after Notepad closes
            $lines = Get-Content -Path $script:SessionListPath -Raw -Encoding UTF8
            $lines = $lines -split "`r?`n"

            $currentParent = $null
            $updated = 0
            $skipped = 0

            foreach ($line in $lines) {
                $trim = $line.Trim()

                if ($trim -match '^[A-Za-z]:\\') {
                    $currentParent = $trim.TrimEnd('\')
                    continue
                }

                if (-not $trim -or $trim -match 'Folder$') { continue }

                if ($trim -match '^(.+?)\s+#(\d+)$' -and $currentParent) {
                    $folderName = $matches[1].Trim()
                    $infoTip = "#$($matches[2])"
                    $targetFolder = Join-Path $currentParent $folderName
                    $iniPath = Join-Path $targetFolder 'desktop.ini'

                    if (Test-Path $targetFolder) {
                        if (Test-Path $iniPath) {
                            $ini = Get-Content $iniPath -Raw -ErrorAction SilentlyContinue

                            if ($ini -match '(?m)^\s*InfoTip\s*=') {
                                $new = [regex]::Replace($ini, '(?m)^\s*InfoTip\s*=.*$', "InfoTip=$infoTip")
                            }
                            elseif ($ini -match '(?m)^\s*\[\.ShellClassInfo\]\s*$') {
                                $new = [regex]::Replace($ini, '(?m)^\s*\[\.ShellClassInfo\]\s*$', "[.ShellClassInfo]`r`nInfoTip=$infoTip")
                            }
                            else {
                                $new = $ini.TrimEnd() + "`r`n`r`n[.ShellClassInfo]`r`nInfoTip=$infoTip"
                            }

                            Set-Content -Path $iniPath -Value $new -Encoding Unicode
                            Write-Host "Updated → $folderName ($infoTip)" -ForegroundColor Green
                            $updated++
                        } else {
                            Write-Host "Skipped → $folderName   [No desktop.ini]" -ForegroundColor Magenta
                            $skipped++
                        }
                    } else {
                        Write-Host "Skipped → $folderName   [Folder not found]" -ForegroundColor Magenta
                        $skipped++
                    }
                }
            }

            Write-Host "`n$updated folders updated, $skipped skipped." -ForegroundColor Green
        } else {
            Write-Host "`nSkipped applying changes from list." -ForegroundColor Magenta
        }
    }
    
    # Step 8: Hide Desktop.ini's
    $response = Get-Confirmation "Ready to hide the Desktop.ini's?"
    if ($response -eq 'M') { Show-MainMenu; return }
    
    if ($response -eq 'Y') {
        $subfolders = Get-ChildItem -Path $script:SessionPath -Directory -ErrorAction SilentlyContinue
        $hiddenCount = 0
        
        foreach ($folder in $subfolders) {
            $iniPath = Join-Path $folder.FullName 'desktop.ini'
            if (Test-Path $iniPath) {
                attrib +s +h "$iniPath"
                Write-Host "Hidden → $iniPath" -ForegroundColor Green
                $hiddenCount++
            }
        }
        
        Write-Host "`n$hiddenCount desktop.ini files hidden." -ForegroundColor Green
    } else {
        Write-Host "`nSkipped hiding desktop.ini files." -ForegroundColor Magenta
    }
    
    Show-OperationComplete
}

function Invoke-Op3 {
    Write-Host ""
    Write-Host "AUTOMATED WORKFLOW: In a single folder" -ForegroundColor Magenta
    Write-Host ""
    
    # Step 1: Get folder path
    $folder = Get-ValidPathWithMenu "Enter the folder path where you want to create a desktop.ini file"
    if ($folder -eq 'MENU') { Show-MainMenu; return }
    $number = Read-Host "Enter the order number you'd like for this folder"
    if ([string]::IsNullOrWhiteSpace($number)) { $number = 1 }
    
    # Step 2: Create Desktop.ini
    $response = Get-Confirmation "Ready to create the Desktop.ini?"
    if ($response -eq 'M') { Show-MainMenu; return }
    
    if ($response -eq 'Y') {
        $path = Join-Path $folder 'desktop.ini'
        
        @"
[ViewState]
Mode=
Vid=
FolderType=Generic

[.ShellClassInfo]
ConfirmFileOp=0
InfoTip=#$number
"@ | Set-Content -Path $path -Encoding Unicode
        
        attrib +s (Split-Path $path)
        attrib -h $path
        
        Write-Host "`nCreated desktop.ini → $folder   [InfoTip #$number]" -ForegroundColor Green
    } else {
        Write-Host "`nSkipped creating desktop.ini." -ForegroundColor Magenta
    }
    
    # Step 3: Make System Folder
    $response = Get-Confirmation "Ready to make it a System Folder?"
    if ($response -eq 'M') { Show-MainMenu; return }
    
    if ($response -eq 'Y') {
        $iniPath = Join-Path $folder 'desktop.ini'
        if (Test-Path $iniPath) {
            attrib +s "$folder"
            Write-Host "Marked as system folder → $folder" -ForegroundColor Green
        }
    } else {
        Write-Host "`nSkipped making system folder." -ForegroundColor Magenta
    }
    
    # Step 4: Hide Desktop.ini
    $response = Get-Confirmation "Ready to hide the Desktop.ini?"
    if ($response -eq 'M') { Show-MainMenu; return }
    
    if ($response -eq 'Y') {
        $ini = Join-Path $folder 'desktop.ini'
        if (Test-Path $ini) {
            attrib +s +h "$ini"
            Write-Host "Hidden → $ini" -ForegroundColor Green
        }
    } else {
        Write-Host "`nSkipped hiding desktop.ini." -ForegroundColor Magenta
    }
    
    Show-OperationComplete
}

function Invoke-Del1 {
    Write-Host ""
    Write-Host "DELETE WORKFLOW: All Desktop.ini's in All Folders" -ForegroundColor Magenta
    Write-Host ""

    $root = Get-ValidPathWithMenu "Enter the path of the folder where ALL folders' desktop.ini files will be deleted"
    if ($root -eq 'MENU') { Show-MainMenu; return }

    $response = Get-Confirmation "Ready to delete the Desktop.ini files?"
    if ($response -eq 'M') { Show-MainMenu; return }

    if ($response -eq 'Y') {
        $deleted = 0
        $files = Get-ChildItem -Path $root -Filter 'desktop.ini' -Recurse -Force -ErrorAction SilentlyContinue

        foreach ($file in $files) {
            Remove-Item -Path $file.FullName -Force -ErrorAction SilentlyContinue
            Write-Host "Deleted → $($file.FullName)" -ForegroundColor Green
            $deleted++
        }

        Write-Host "`nCompleted. $deleted desktop.ini files deleted." -ForegroundColor Green
    } else {
        Write-Host "`nSkipped deleting desktop.ini files." -ForegroundColor Magenta
    }

    Write-Host ""
    Write-Host "Operation Complete!!" -ForegroundColor Green
    Write-Host ""
    Write-Host "To return to the menu:" -ForegroundColor White
    Write-Host "[" -NoNewline -ForegroundColor White
    Write-Host "menu" -NoNewline -ForegroundColor Green
    Write-Host "]" -ForegroundColor White
    Write-Host ""
}

function Invoke-Del2 {
    Write-Host ""
    Write-Host "DELETE WORKFLOW: All Desktop.ini's in First Forefront Subfolders" -ForegroundColor Magenta
    Write-Host ""

    $root = Get-ValidPathWithMenu "Enter the path of the folder where the first forefront subfolders' desktop.ini files will be deleted"
    if ($root -eq 'MENU') { Show-MainMenu; return }

    $response = Get-Confirmation "Ready to delete the Desktop.ini files?"
    if ($response -eq 'M') { Show-MainMenu; return }

    if ($response -eq 'Y') {
        $deleted = 0
        Get-ChildItem -Path $root -Directory | ForEach-Object {
            $iniPath = Join-Path $_.FullName 'desktop.ini'
            if (Test-Path $iniPath) {
                Remove-Item -Path $iniPath -Force -ErrorAction SilentlyContinue
                Write-Host "Deleted → $iniPath" -ForegroundColor Green
                $deleted++
            }
        }

        Write-Host "`nCompleted. $deleted desktop.ini files deleted." -ForegroundColor Green
    } else {
        Write-Host "`nSkipped deleting desktop.ini files." -ForegroundColor Magenta
    }

    Write-Host ""
    Write-Host "Operation Complete!!" -ForegroundColor Green
    Write-Host ""
    Write-Host "To return to the menu:" -ForegroundColor White
    Write-Host "[" -NoNewline -ForegroundColor White
    Write-Host "menu" -NoNewline -ForegroundColor Green
    Write-Host "]" -ForegroundColor White
    Write-Host ""
}

# ============================================================================
# MAIN COMMAND LOOP
# ============================================================================

function Start-OrganizeFolders {
    # Menu is already displayed by the batch file
    # Just start the command loop
    
    while ($true) {
        Write-Host ""
        $command = Read-Host "Enter command"
        $command = $command.Trim()
        
        switch ($command) {
            "menu" { Show-MainMenu }
            "manual menu" { Show-ManualMenu }
            "manual mode" { Show-ManualMenu }
            "manual" { Show-ManualMenu }
            "manualmenu" { Show-ManualMenu }
            "manualmode" { Show-ManualMenu }
            
            # Non-Manual Mode
            "op1" { Invoke-Op1 }
            "op2" { Invoke-Op2 }
            "op3" { Invoke-Op3 }
            "del1" { Invoke-Del1 }
            "del2" { Invoke-Del2 }
            
            # Manual Mode - Category 1
            "cat1 op1" { Invoke-Cat1Op1 }
            "cat1op1" { Invoke-Cat1Op1 }
            "cat1 op2" { Invoke-Cat1Op2 }
            "cat1op2" { Invoke-Cat1Op2 }
            "cat1 op3" { Invoke-Cat1Op3 }
            "cat1op3" { Invoke-Cat1Op3 }
            
            # Manual Mode - Category 2
            "cat2 op1" { Invoke-Cat2Op1 }
            "cat2op1" { Invoke-Cat2Op1 }
            "cat2 op2" { Invoke-Cat2Op2 }
            "cat2op2" { Invoke-Cat2Op2 }
            "cat2 op3" { Invoke-Cat2Op3 }
            "cat2op3" { Invoke-Cat2Op3 }
            
            # Manual Mode - Category 3
            "cat3 op1" { Invoke-Cat3Op1 }
            "cat3op1" { Invoke-Cat3Op1 }
            "cat3 op2" { Invoke-Cat3Op2 }
            "cat3op2" { Invoke-Cat3Op2 }
            
            # Manual Mode - Category 4
            "cat4 op1" { Invoke-Cat4Op1 }
            "cat4op1" { Invoke-Cat4Op1 }
            "cat4 op2" { Invoke-Cat4Op2 }
            "cat4op2" { Invoke-Cat4Op2 }
            
            # Manual Mode - Category 5
            "cat5 op1" { Invoke-Cat5Op1 }
            "cat5op1" { Invoke-Cat5Op1 }
            "cat5 op2" { Invoke-Cat5Op2 }
            "cat5op2" { Invoke-Cat5Op2 }
            "cat5 op3" { Invoke-Cat5Op3 }
            "cat5op3" { Invoke-Cat5Op3 }
            
            # Manual Mode - Category 6
            "cat6 op1" { Invoke-Cat6Op1 }
            "cat6op1" { Invoke-Cat6Op1 }
            "cat6 op2" { Invoke-Cat6Op2 }
            "cat6op2" { Invoke-Cat6Op2 }
            "cat6 op3" { Invoke-Cat6Op3 }
            "cat6op3" { Invoke-Cat6Op3 }
            
            # Manual Mode - Category 7
            "cat7 op1" { Invoke-Cat7Op1 }
            "cat7op1" { Invoke-Cat7Op1 }
            "cat7 op2" { Invoke-Cat7Op2 }
            "cat7op2" { Invoke-Cat7Op2 }
            
            "info" {
                # Call the batch file to display the info with proper encoding and colors
                $batPath = Join-Path $PSScriptRoot "ShowInfo.bat"
                & $batPath
            }
            
            "exit" { 
                Write-Host "`nThank you for using OrganizeFolders THE RIGHT WAY! <3" -ForegroundColor White
                Write-Host ""
                Write-Host "Exiting in " -NoNewline -ForegroundColor White
                Write-Host "4 " -NoNewline -ForegroundColor Red
                Start-Sleep -Seconds 1
                Write-Host "3 " -NoNewline -ForegroundColor Red
                Start-Sleep -Seconds 1
                Write-Host "2 " -NoNewline -ForegroundColor Red
                Start-Sleep -Seconds 1
                Write-Host "1" -ForegroundColor Red
                Start-Sleep -Seconds 1
                Stop-Process -Id $PID
            }
            "quit" { 
                Write-Host "`nThank you for using OrganizeFolders THE RIGHT WAY! <3" -ForegroundColor White
                Write-Host ""
                Write-Host "Exiting in " -NoNewline -ForegroundColor White
                Write-Host "4 " -NoNewline -ForegroundColor Red
                Start-Sleep -Seconds 1
                Write-Host "3 " -NoNewline -ForegroundColor Red
                Start-Sleep -Seconds 1
                Write-Host "2 " -NoNewline -ForegroundColor Red
                Start-Sleep -Seconds 1
                Write-Host "1" -ForegroundColor Red
                Start-Sleep -Seconds 1
                Stop-Process -Id $PID
            }
            
            default { 
                Write-Host "Unknown command: $command" -ForegroundColor Red
                Write-Host "Type " -NoNewline -ForegroundColor White
                Write-Host "[" -NoNewline -ForegroundColor White
                Write-Host "menu" -NoNewline -ForegroundColor Green
                Write-Host "]" -NoNewline -ForegroundColor White
                Write-Host " to see available options" -ForegroundColor White
            }
        }
    }
}

Start-OrganizeFolders

