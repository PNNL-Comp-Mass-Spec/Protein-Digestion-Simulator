@echo off

set ExePath=ProteinDigestionSimulator.exe

if exist %ExePath% goto DoWork
if exist ..\%ExePath% set ExePath=..\%ExePath% && goto DoWork
if exist ..\bin\%ExePath% set ExePath=..\bin\%ExePath% && goto DoWork
if exist ..\ProteinDigestionSimulator\bin\%ExePath% set ExePath=..\ProteinDigestionSimulator\bin\%ExePath% && goto DoWork

echo Executable not found: %ExePath%
goto Done

:DoWork
echo.
echo Procesing with %ExePath%
echo.

%ExePath% JunkTest.fasta

%ExePath% QC_Standards_2004-01-21.fasta /Digest

%ExePath% QC_Standards_2004-01-21.fasta /Hash

%ExePath% QC_Standards_2004-01-21_digested_peptides.txt /DelimitedFileFormat:SequenceOnly /Hash /Mass:False /InputFileHasHeader:False

%ExePath% TestProteins.csv

:Done

pause
