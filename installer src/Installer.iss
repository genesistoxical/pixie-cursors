#define MyAppName "Pixie Cursors"
#define MyAppVersion "1.4.1"
#define MyAppPublisher "Génesis Toxical"
#define MyAppURL "https://genesistoxical.github.io/pixie-cursors/"
#define MyAppExeName "Pixie Cursors.exe"

[Setup]
AppId={{CF830ABE-EBC2-479C-9E76-2CE49DFF8676}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
VersionInfoVersion=1.4.1.0
AppPublisher={#MyAppPublisher}
AppCopyright={#MyAppPublisher} © 2025
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
LicenseFile=Pixie Cursors.txt
OutputDir=Output
OutputBaseFilename=Pixie-Cursors-Installer
SetupIconFile=..\src\PixieCursors\Resources\Icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern      
WizardSmallImageFile=Wizard Small Image.bmp
WizardImageFile=Wizard Image File.bmp
;DisableWelcomePage=no
;WizardSizePercent=114,100
WizardSizePercent=110,100 
DisableProgramGroupPage=yes
UninstallDisplayIcon={uninstallexe}
UsedUserAreasWarning=no                                    

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}";

[Files]
Source: "..\src\PixieCursors\bin\Release\*"; DestDir: "{app}"; Excludes: "*.ini"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\src\PixieCursors\bin\Release\Config.ini"; DestDir: "{userappdata}\{#MyAppName}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Dirs]
Name: "C:\Windows\Cursors\- Pixie Cursors -"

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

;[Messages]
;english.WelcomeLabel1=[name] Setup Wizard
;english.WelcomeLabel2=This will install [name/ver] on your computer.
;spanish.WelcomeLabel1=Asistente de Instalación de [name]
;spanish.WelcomeLabel2=Este programa instalará [name/ver] en su sistema.
; NOTA: El apartado de Messages solo se utilizará en caso de que la bienvenida esté habilitada