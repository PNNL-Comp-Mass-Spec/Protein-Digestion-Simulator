
xcopy Output\ProteinDigestionSimulator_Installer.exe \\proto-2\Software\ProteinDigestionSimulator /Y /D

xcopy ..\ProteinDigestionSimulator\Bin\ProteinDigestionSimulator.exe  \\proto-2\Software\ProteinDigestionSimulator\Exe_Only /Y /D
xcopy ..\ProteinDigestionSimulator\Bin\ProteinDigestionSimulator.pdb  \\proto-2\Software\ProteinDigestionSimulator\Exe_Only /Y /D

xcopy ..\ProteinDigestionSimulator\Bin\*.dll                          \\proto-2\Software\ProteinDigestionSimulator\Exe_Only /Y /D

xcopy ..\Readme.md                          \\proto-2\Software\ProteinDigestionSimulator\Exe_Only /Y /D
xcopy ..\RevisionHistory.txt                \\proto-2\Software\ProteinDigestionSimulator\Exe_Only /Y /D

pause
