
# Protein Digestion Simulator

The Protein Digestion Simulator is a utility for processing [FASTA files](https://en.wikipedia.org/wiki/FASTA_format)
including converting to/from tab delimited text.  It also supports FASTA file validation.

### Continuous Integration

The latest version of the application is available on the [AppVeyor CI server](https://ci.appveyor.com/project/PNNLCompMassSpec/protein-digestion-simulator/build/artifacts)

[![Build status](https://ci.appveyor.com/api/projects/status/j6kerul55ql8cd54?svg=true)](https://ci.appveyor.com/project/PNNLCompMassSpec/protein-digestion-simulator)

## Features

The Protein Digestion Simulator program can be used to read a text file containing 
protein or peptide sequences (fasta format or delimited text) then output the 
data to a tab-delimited file.  It can optionally digest the input sequences 
using trypsin or partial trpysin rules, and can add the predicted normalized 
elution time (NET) values for the peptides.  It can also validate a fasta file, 
testing it against a set of rules that identify common formatting errors.

As an alternative to digesting the peptides, one can read in a fasta file and
create a new fasta file with all of the protein sequences reversed or even
randomized.  This new file can be the equivalent length of the original file,
or can include just a subset of the original file.

The third key feature of this software is the ability to calculate the number 
of uniquely identifiable peptides within the input file (digested or undigested),
using only mass, or both mass and NET, with appropriate tolerances.  Note that the 
NET values are based on a model trained using data from Dr. Richard D. Smith's
laboratory at Pacific Northwest National Lab (Richland, WA) and are thus specific
to the reversed phase (C18) capillary chromatography system being used (see 
Petritis, Kangas, Ferfuson, Anderson, Pasa-Tolic, Lipton, Auberry, Strittmatter, 
Shen, Zhao, and Smith. 2003. Analytical Chemistry, 75 (5), 1039-1048).

## Installation

* Download Protein-Digestion-Simulator.zip from [AppVeyor](https://ci.appveyor.com/project/PNNLCompMassSpec/protein-digestion-simulator/build/artifacts)
* Extract the files
* Run ProteinDigestionSimulator.exe

## Console Switches

The Protein Digestion Simluator can be run in batch mode from the Windows command line.  Syntax:

```
ProteinDigestionSimulator.exe 
  /I:SourceFastaOrTextFile [/F] [/D] [/M] [/AD:AlternateDelimeter] 
  [/O:OutputFolderPath] [/P:ParameterFilePath] [/S:[MaxLevel]] 
  [/A:AlternateOutputFolderPath] [/R] [/Q]
```

The input file path can contain the wildcard character * and should point to a fasta file or tab-delimited text file.

Use /F to indicate that the input file is a fasta file.  If /F is not used, then the format will be assumed to be fasta 
only if the file contains .fasta in the name

Use /D to indicate that an in-silico digestion of the proteins should be performed.  Digestion options must be specified in the Parameter file.

Use /M to indicate that protein mass should be computed.

Use /AD to specify a delimiter other than the Tab character (not applicable for fasta files).

The output folder path is optional.  If omitted, the output files will be created in the same folder as the input file.

The parameter file path is optional.  If included, it should point to a valid XML parameter file.

Use /S to process all valid files in the input folder and subfolders. Include a number after /S (like /S:2) to limit the level of subfolders to examine.
* When using /S, you can redirect the output of the results using /A.
* When using /S, you can use /R to re-create the input folder hierarchy in the alternate output folder (if defined).

The optional /Q switch will suppress all error messages.

## Contacts

Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) \
E-mail: matthew.monroe@pnnl.gov or matt@alchemistmatt.com\
Website: http://panomics.pnnl.gov/ or http://omics.pnl.gov or http://www.sysbio.org/resources/staff/

## License

The Protein Digestion Simulator is licensed under the Apache License, Version 2.0; 
you may not use this file except in compliance with the License.  You may obtain 
a copy of the License at https://opensource.org/licenses/Apache-2.0

The NET Prediction DLL is licensed under the Reciprocal Public License v1.5; 
for details see file PNNL_NETPrediction_License.pdf\
You may obtain a copy of the License at https://opensource.org/licenses/rpl1.5.txt

All publications that result from the use of this software should include 
the following acknowledgment statement:
> Portions of this research were supported by the W.R. Wiley Environmental 
> Molecular Science Laboratory, a national scientific user facility sponsored 
> by the U.S. Department of Energy's Office of Biological and Environmental 
> Research and located at PNNL.  PNNL is operated by Battelle Memorial Institute 
> for the U.S. Department of Energy under contract DE-AC05-76RL0 1830.

Notice: This computer software was prepared by Battelle Memorial Institute, 
hereinafter the Contractor, under Contract No. DE-AC05-76RL0 1830 with the 
Department of Energy (DOE).  All rights in the computer software are reserved 
by DOE on behalf of the United States Government and the Contractor as 
provided in the Contract.  NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAKES ANY 
WARRANTY, EXPRESS OR IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THIS 
SOFTWARE.  This notice including this sentence must appear on any copies of 
this computer software.
