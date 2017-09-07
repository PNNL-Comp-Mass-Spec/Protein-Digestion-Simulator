; This is an Inno Setup configuration file
; http://www.jrsoftware.org/isinfo.php

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
Source: ..\bin\ProteinFileReader.dll                  ; DestDir: {app}
Source: ..\bin\SharedVBNetRoutines.dll                ; DestDir: {app}
Source: ..\bin\ValidateFastaFile.dll                  ; DestDir: {app}
Source: ..\bin\QC_Standards_2004-01-21.fasta          ; DestDir: {app}
Source: ..\bin\ProteinDigestionSimulatorOptions.xml   ; DestDir: {app}
Source: ..\bin\Elute1052.wts                          ; DestDir: {app}
Source: ..\bin\SCX1052.wts                            ; DestDir: {app}

Source: ..\README.md                                  ; DestDir: {app}
Source: ..\RevisionHistory.txt                        ; DestDir: {app}
Source: ..\PNNL_NETPrediction_License.pdf             ; DestDir: {app}
Source: Images\delete_16x.ico                         ; DestDir: {app}

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
AppPublisherURL=http://omics.pnl.gov/software
AppSupportURL=http://omics.pnl.gov/software
AppUpdatesURL=http://omics.pnl.gov/software
DefaultDirName={pf}\ProteinDigestionSimulator
DefaultGroupName=PAST Toolkit
AppCopyright=© PNNL
;LicenseFile=.\License.rtf
PrivilegesRequired=poweruser
OutputBaseFilename=ProteinDigestionSimulator_Installer
VersionInfoVersion={#ApplicationVersion}
VersionInfoCompany=PNNL
VersionInfoDescription=Protein Digestion Simulator
VersionInfoCopyright=PNNL
DisableFinishedPage=true
ShowLanguageDialog=no
ChangesAssociations=false
EnableDirDoesntExistWarning=false
AlwaysShowDirOnReadyPage=true
UninstallDisplayIcon={app}\delete_16x.ico
ShowTasksTreeLines=true
OutputDir=.\Output

[Registry]
;Root: HKCR; Subkey: MyAppFile; ValueType: string; ValueName: ; ValueDataMyApp File; Flags: uninsdeletekey
;Root: HKCR; Subkey: MyAppSetting\DefaultIcon; ValueType: string; ValueData: {app}\wand.ico,0; Flags: uninsdeletevalue

[UninstallDelete]
Name: {app}; Type: filesandordirs
