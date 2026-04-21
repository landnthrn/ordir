#define MyAppName "Ordir"
#define MyAppVersion "2.0.0"
#define MyAppPublisher "landn.thrn"
#define MyAppExeName "Ordir.exe"
#define MyAppId "{{85F93D91-6C0F-4A2B-B4E2-E427A2B3D81C}}"

[Setup]
AppId={#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\Ordir
DefaultGroupName=Ordir
DisableProgramGroupPage=yes
OutputDir=..\dist
OutputBaseFilename=ordir-setup
SetupIconFile=..\src\Assets\ordir-logo-1.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
ChangesEnvironment=yes

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"; Flags: unchecked
Name: "addtopath"; Description: "Install 'ordir' command to PATH"; GroupDescription: "Terminal integration:"; Flags: unchecked

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\scripts\cli-launch\*"; DestDir: "{app}\scripts\cli-launch"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Ordir"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\Ordir"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch Ordir"; Flags: nowait postinstall skipifsilent

[Code]
const
  EnvKey = 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment';

function PathContains(const PathList, Item: string): Boolean;
begin
  Result := Pos(';' + UpperCase(Item) + ';', ';' + UpperCase(PathList) + ';') > 0;
end;

function UpdateReadyMemo(Space, NewLine, MemoUserInfoInfo, MemoDirInfo, MemoTypeInfo, MemoComponentsInfo, MemoGroupInfo, MemoTasksInfo: String): String;
var
  Tasks: String;
begin
  Tasks := MemoTasksInfo;
  { Blank line between task groups on Ready to Install page. }
  if Tasks <> '' then
    StringChangeEx(Tasks, NewLine + Space + Space + 'Terminal integration:', NewLine + NewLine + Space + Space + 'Terminal integration:', True);

  Result := '';
  if MemoUserInfoInfo <> '' then
    Result := Result + MemoUserInfoInfo + NewLine + NewLine;
  if MemoDirInfo <> '' then
    Result := Result + MemoDirInfo + NewLine + NewLine;
  if MemoTypeInfo <> '' then
    Result := Result + MemoTypeInfo + NewLine + NewLine;
  if MemoComponentsInfo <> '' then
    Result := Result + MemoComponentsInfo + NewLine + NewLine;
  if MemoGroupInfo <> '' then
    Result := Result + MemoGroupInfo + NewLine + NewLine;
  if Tasks <> '' then
    Result := Result + Tasks;
end;

procedure AddPathIfNeeded(const Item: string);
var
  CurrentPath: string;
begin
  if not RegQueryStringValue(HKLM, EnvKey, 'Path', CurrentPath) then
    CurrentPath := '';

  if PathContains(CurrentPath, Item) then
    exit;

  if (CurrentPath = '') then
    CurrentPath := Item
  else if CurrentPath[Length(CurrentPath)] = ';' then
    CurrentPath := CurrentPath + Item
  else
    CurrentPath := CurrentPath + ';' + Item;

  RegWriteExpandStringValue(HKLM, EnvKey, 'Path', CurrentPath);
  { WM_SETTINGCHANGE: [Setup] ChangesEnvironment=yes notifies Explorer at end of install/uninstall. }
end;

procedure RemovePathIfExists(const Item: string);
var
  CurrentPath, NewPath, Token: string;
  P: Integer;
begin
  if not RegQueryStringValue(HKLM, EnvKey, 'Path', CurrentPath) then
    exit;

  NewPath := '';
  while CurrentPath <> '' do
  begin
    P := Pos(';', CurrentPath);
    if P > 0 then
    begin
      Token := Copy(CurrentPath, 1, P - 1);
      Delete(CurrentPath, 1, P);
    end
    else
    begin
      Token := CurrentPath;
      CurrentPath := '';
    end;

    if (Trim(Token) <> '') and (CompareText(Trim(Token), Item) <> 0) then
    begin
      if NewPath = '' then
        NewPath := Trim(Token)
      else
        NewPath := NewPath + ';' + Trim(Token);
    end;
  end;

  RegWriteExpandStringValue(HKLM, EnvKey, 'Path', NewPath);
  { WM_SETTINGCHANGE: [Setup] ChangesEnvironment=yes notifies Explorer at end of install/uninstall. }
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    if WizardIsTaskSelected('addtopath') then
      AddPathIfNeeded(ExpandConstant('{app}\scripts\cli-launch'));
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
    RemovePathIfExists(ExpandConstant('{app}\scripts\cli-launch'));
end;
