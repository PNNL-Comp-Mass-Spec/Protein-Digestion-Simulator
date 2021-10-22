@echo off

set ExePath=ProteinDigestionSimulator.exe

if exist %ExePath% goto FindParamFile
if exist ..\%ExePath% set ExePath=..\%ExePath% && goto FindParamFile
if exist ..\bin\%ExePath% set ExePath=..\bin\%ExePath% && goto FindParamFile
if exist ..\ProteinDigestionSimulator\bin\%ExePath% set ExePath=..\ProteinDigestionSimulator\bin\%ExePath% && goto FindParamFile

echo Executable not found: %ExePath%
goto Done

:FindParamFile

set ParamFilePath=ProteinDigestionSimulatorOptions.conf
if exist %ParamFilePath% goto DoWork
if exist ..\%ParamFilePath% set ParamFilePath=..\%ParamFilePath% && goto DoWork
if exist ..\Documentation\%ParamFilePath% set ParamFilePath=..\Documentation\%ParamFilePath% && goto DoWork

echo Parameter file not found: %ParamFilePath%

:DoWork
echo.
echo Processing with %ExePath%
echo.

rem %ExePath% JunkTest.fasta

echo %ExePath% JunkTest.fasta /P:%ParamFilePath%

rem %ExePath% JunkTest.fasta /P:%ParamFilePath%
goto Done

%ExePath% QC_Standards_2004-01-21.fasta /Digest

%ExePath% QC_Standards_2004-01-21.fasta /Hash

%ExePath% QC_Standards_2004-01-21_digested_peptides.txt /DelimitedFileFormat:SequenceOnly /Hash /Mass:False /InputFileHasHeader:False

%ExePath% QC_Standards_2004-01-21.fasta ..\ProteinDigestionSimulatorOptions.conf

%ExePath% TestProteins.csv

:Done

pause
