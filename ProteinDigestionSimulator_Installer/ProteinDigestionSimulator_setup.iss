; This is an Inno Setup configuration file
; https://jrsoftware.org/isinfo.php

#define ApplicationVersion GetFileVersion('..\bin\ProteinDigestionSimulator.exe')

[CustomMessages]
AppName=Protein Digestion Simulator

[Messages]
WelcomeLabel2=This will install [name/ver] on your computer.
; Example with multiple lines:
; WelcomeLabel2=Welcome message%n%nAdditional sentence

[Files]
Source: ..\bin\ProteinDigestionSimulator.exe          ; DestDir: {app}
Source: ..\bin\ProteinDigestionSimulator.pdb          ; DestDir: {app}
Source: ..\bin\FlexibleFileSortUtility.dll            ; DestDir: {app}
Source: ..\bin\NETPrediction.dll                      ; DestDir: {app}
Source: ..\bin\PRISM.dll                              ; DestDir: {app}
Source: ..\bin\PRISMWin.dll                           ; DestDir: {app}
Source: ..\bin\ProteinFileReader.dll                  ; DestDir: {app}
Source: ..\bin\ValidateFastaFile.dll                  ; DestDir: {app}
Source: ..\bin\QC_Standards_2004-01-21.fasta          ; DestDir: {app}
Source: ..\bin\QC_Standards_2004-01-21.txt                            ; DestDir: {app}
Source: ..\bin\QC_Standards_2004-01-21_digested_Mass400to6000.txt     ; DestDir: {app}

Source: ..\bin\ProteinDigestionSimulatorOptions.xml   ; DestDir: {app}
Source: ..\bin\Elute1052.wts                          ; DestDir: {app}
Source: ..\bin\SCX1052.wts                            ; DestDir: {app}

Source: ..\License.txt                                ; DestDir: {app}
Source: ..\Disclaimer.txt                             ; DestDir: {app}
Source: ..\Readme.md                                  ; DestDir: {app}
Source: ..\RevisionHistory.txt                        ; DestDir: {app}
Source: ..\PNNL_NETPrediction_License.pdf             ; DestDir: {app}
Source: Images\delete_16x.ico                         ; DestDir: {app}

Source: ..\Docs\ProteinDigestionSimulatorOptions.xml     ; DestDir: {app}\Docs

[Dirs]
Name: {commonappdata}\ProteinDigestionSimulator; Flags: uninsalwaysuninstall

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked
; Name: quicklaunchicon; Description: {cm:CreateQuickLaunchIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked

[Icons]
Name: {commondesktop}\Protein Digestion Simulator; Filename: {app}\ProteinDigestionSimulator.exe; Tasks: desktopicon; Comment: ProteinDigestionSimulator
Name: {group}\Protein Digestion Simulator; Filename: {app}\ProteinDigestionSimulator.exe; Comment: Protein Digestion Simulator

[Setup]
AppName=Protein Digestion Simulator
AppVersion={#ApplicationVersion}
;AppVerName=ProteinDigestionSimulator
AppID=ProteinDigestionSimulatorId
AppPublisher=Pacific Northwest National Laboratory
AppPublisherURL=https://omics.pnl.gov/software
AppSupportURL=https://omics.pnl.gov/software
AppUpdatesURL=https://github.com/PNNL-Comp-Mass-Spec/Protein-Digestion-Simulator
ArchitecturesAllowed=x64 x86
ArchitecturesInstallIn64BitMode=x64
DefaultDirName={autopf}\ProteinDigestionSimulator
DefaultGroupName=PAST Toolkit
AppCopyright=© PNNL
;LicenseFile=.\License.rtf
PrivilegesRequired=admin
OutputBaseFilename=ProteinDigestionSimulator_Installer
VersionInfoVersion={#ApplicationVersion}
VersionInfoCompany=PNNL
VersionInfoDescription=Protein Digestion Simulator
VersionInfoCopyright=PNNL
DisableFinishedPage=yes
DisableWelcomePage=no
ShowLanguageDialog=no
ChangesAssociations=no
WizardStyle=modern
EnableDirDoesntExistWarning=no
AlwaysShowDirOnReadyPage=yes
UninstallDisplayIcon={app}\delete_16x.ico
ShowTasksTreeLines=yes
OutputDir=.\Output

[Registry]
;Root: HKCR; Subkey: MyAppFile; ValueType: string; ValueName: ; ValueDataMyApp File; Flags: uninsdeletekey
;Root: HKCR; Subkey: MyAppSetting\DefaultIcon; ValueType: string; ValueData: {app}\wand.ico,0; Flags: uninsdeletevalue

[UninstallDelete]
Name: {app}; Type: filesandordirs
