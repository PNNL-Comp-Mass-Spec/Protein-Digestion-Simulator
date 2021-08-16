
xcopy Output\ProteinDigestionSimulator_Installer.exe \\floyd\software\ProteinDigestionSimulator /Y /D

xcopy ..\ProteinDigestionSimulator\Bin\ProteinDigestionSimulator.exe  \\floyd\software\ProteinDigestionSimulator\Exe_Only /Y /D
xcopy ..\ProteinDigestionSimulator\Bin\ProteinDigestionSimulator.pdb  \\floyd\software\ProteinDigestionSimulator\Exe_Only /Y /D

xcopy ..\ProteinDigestionSimulator\Bin\*.dll                          \\floyd\software\ProteinDigestionSimulator\Exe_Only /Y /D

xcopy ..\Readme.md                          \\floyd\software\ProteinDigestionSimulator\Exe_Only /Y /D
xcopy ..\RevisionHistory.txt                \\floyd\software\ProteinDigestionSimulator\Exe_Only /Y /D

pause
