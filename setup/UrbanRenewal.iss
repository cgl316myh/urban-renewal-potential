; 城市更新潜力评价与验证系统 — Inno Setup 7 安装脚本
; 源文件：src\UrbanRenewal.Host\bin\Debug
; 编译：ISCC.exe setup\UrbanRenewal.iss

#define MyAppName "城市更新潜力评价与验证系统"
#define MyAppNameEn "UrbanRenewal"
#define MyAppVersion "1.2.0"
#define MyAppPublisher "UrbanRenewal"
#define MyAppExeName "UrbanRenewal.Host.exe"
#define MyAppId "{{A7C3E9B1-4F2D-4A8E-9C1B-UrbanRenewal01}"

; 相对本 .iss 的路径（当前使用 Debug 输出）
#define SourceRoot "..\src\UrbanRenewal.Host\bin\Debug"

[Setup]
AppId={#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf32}\{#MyAppNameEn}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=output
OutputBaseFilename=UrbanRenewal_Setup_{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x86 x64compatible
; 使用 {autopf32}，在 64 位系统上也装到 Program Files (x86)
MinVersion=6.1sp1
SetupLogging=yes
SetupIconFile=UrbanRenewal.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
VersionInfoVersion={#MyAppVersion}
VersionInfoProductName={#MyAppName}
InfoBeforeFile=Prerequisite.txt
LicenseFile=
DisableDirPage=no
AllowNoIcons=yes

[Languages]
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; 主程序与依赖（排除调试符号、文档 XML、多语言卫星程序集）
Source: "{#SourceRoot}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; \
  Excludes: "*.pdb,*.xml,de\*,es\*,ja\*,ru\*"
; 上面排除了全部 xml，再加回业务配置
Source: "{#SourceRoot}\plugins.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceRoot}\Config\*"; DestDir: "{app}\Config"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#SourceRoot}\*.dll.config"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "{#SourceRoot}\*.exe.config"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
; 应用 Logo
Source: "UrbanRenewal.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\UrbanRenewal.ico"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\UrbanRenewal.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup(): Boolean;
begin
  Result := True;
  // ArcGIS / .NET 由 InfoBefore 提示；此处不强制检测注册表以免误拦 Engine Runtime
end;
