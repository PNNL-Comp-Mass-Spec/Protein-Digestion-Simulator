
# Protein Digestion Simulator

The Protein Digestion Simulator is a utility for processing [FASTA files](https://en.wikipedia.org/wiki/FASTA_format)
including converting to/from tab delimited text.  It also supports FASTA file validation.

## Downloads

Download a .zip file with the installer from:\
https://github.com/PNNL-Comp-Mass-Spec/Protein-Digestion-Simulator/releases

The release page also includes a .zip file with ProteinDigestionSimulator.exe

### Continuous Integration

The latest version of the application may be available on the [AppVeyor CI server](https://ci.appveyor.com/project/PNNLCompMassSpec/protein-digestion-simulator/build/artifacts),
but builds are deleted after 6 months. \
Otherwise, see the [GitHub release page](https://github.com/PNNL-Comp-Mass-Spec/Protein-Digestion-Simulator/releases)

[![Build status](https://ci.appveyor.com/api/projects/status/j6kerul55ql8cd54?svg=true)](https://ci.appveyor.com/project/PNNLCompMassSpec/protein-digestion-simulator)

## Features

The Protein Digestion Simulator program can be used to read a text file containing
protein or peptide sequences (FASTA format or delimited text) then output the
data to a tab-delimited file.  It can optionally digest the input sequences
using trypsin, partial trpysin, or various other enzymes, and can add the predicted
normalized elution time (NET) values for the peptides.  It can also validate a FASTA file,
testing it against a set of rules that identify common formatting errors.

As an alternative to digesting the peptides, the software can read in a FASTA file and
create a new FASTA file with all of the protein sequences reversed or even
randomized.  This new file can be the equivalent length of the original file,
or can include just a subset of the original file.

The third key feature of this software is the ability to calculate the number
of uniquely identifiable peptides within the input file (digested or undigested),
using only mass, or both mass and NET, with appropriate tolerances.  Note that the
NET values are based on a model trained using data from Dr. Richard D. Smith's
laboratory at Pacific Northwest National Lab (Richland, WA) and are thus specific
to the reversed phase (C18) capillary chromatography system being used.

For more information on the Kangas/Petritis retention time prediction
algorithm, please see:

> K. Petritis, L.J. Kangas, B. Yan, M.E. Monroe, E.F. Strittmatter, W.J. Qian,
> J.N. Adkins, R.J. Moore, Y. Xu, M.S. Lipton, D.G. Camp, R.D. Smith.
> "Improved peptide elution time prediction for reversed-phase liquid
> chromatography-MS by incorporating peptide sequence information",
> Analytical Chemistry, 78, (14), 5026-5039 (2006). \
> [PMID: 16841926](https://www.ncbi.nlm.nih.gov/pubmed/16841926)

### Enzymes

The Protein Digestion Simulator supports the following enzymes:

| Enzyme Name                     |  Cleavage Residues | Exception Residues | Comments                                                 |
|---------------------------------|--------------------|--------------------|----------------------------------------------------------|
| Fully Tryptic                   | KR                 | P                  | Cleave after K or R, but not if K or R is followed by P  |
| Fully Tryptic (no Proline Rule) | KR                 |                    | Cleave after K or R, even if followed by P               |
| Half (Partial) Trypsin          | KR                 | P                  |                                                          |
| Trypsin Plus FVLEY              | KRFYVEL            |                    |                                                          |
| Trypsin plus Lys-C              | KR                 | P for R            | Either after R (no P) or after K (P allowed)             |
| Trypsin plus Thermolysin        | KRLFVIAM           | P for R            |                                                          |
| Acetic Acid Hydrolysis          | D                  |                    |                                                          |
| Arg-C                           | R                  |                    |                                                          |
| Asp-N                           | D                  |                    | Cleaves N-terminal to D                                  |
| Chymotrypsin                    | FWYL               |                    |                                                          |
| Chymotrypsin plus Trypsin       | FWYLKR             |                    |                                                          |
| CyanBr                          | M                  |                    |                                                          |
| Glu-C                           | ED                 |                    |                                                          |
| Glu-C, just Glu                 | E                  |                    |                                                          |
| Lys-C                           | K                  |                    |                                                          |
| PepsinA                         | FLIWY              | P                  |                                                          |
| PepsinB                         | FLIWY              | PVAG               |                                                          |
| PepsinC                         | FLWYA              | P                  |                                                          |
| PepsinD                         | FLWYAEQ            |                    |                                                          |
| Proteinase K                    | AEFILTVWY          |                    |                                                          |
| Thermolysin                     | LFVIAM             |                    |                                                          |
| No cleavage rule                | Cleave after any residue |              |                                                          |
| Peptide Database                | Do not cleavage after any residue |     | Use to extract peptides in a given mass range            |

### Parameter File Cleavage Rule Name

When using a parameter file to set the enzyme to use for digestion (via parameter `DigestionEnzyme`),
use the name shown in the second column of the following table:
* As an alternative to enzyme name, you can use the Enzyme ID

| Human-readable Enzyme Name      | Enzyme Name For Parameter File        | Enzyme ID  |
|---------------------------------|---------------------------------------|------------|
| Fully Tryptic                   | ConventionalTrypsin                   | 1          |
| Fully Tryptic (no Proline Rule) | TrypsinWithoutProlineException        | 2          |
| Half (Partial) Trypsin          | KROneEnd                              | 5          |
| Trypsin plus FVLEY              | TrypsinPlusFVLEY                      | 4          |
| Trypsin plus Lys-C              | TrypsinPlusLysC                       | 21         |
| Trypsin plus Thermolysin        | TrypsinPlusThermolysin                | 23         |
| Acetic Acid Hydrolysis          | AceticAcidD                           | 20         |
| Arg-C                           | ArgC                                  | 13         |
| Asp-N                           | AspN                                  | 14         |
| Chymotrypsin                    | Chymotrypsin                          | 7          |
| Chymotrypsin plus Trypsin       | ChymotrypsinAndTrypsin                | 8          |
| CyanBr                          | CyanBr                                | 10         |
| Glu-C                           | GluC                                  | 9          |
| Glu-C, just Glu                 | GluC_EOnly                            | 12         |
| Lys-C                           | LysC                                  | 11         |
| PepsinA                         | PepsinA                               | 16         |
| PepsinB                         | PepsinB                               | 17         |
| PepsinC                         | PepsinC                               | 18         |
| PepsinD                         | PepsinD                               | 19         |
| Proteinase K                    | ProteinaseK                           | 15         |
| Thermolysin                     | Thermolysin                           | 22         |
| No cleavage rule                | NoRule                                | 0          |
| Peptide Database                | TerminiiOnly                          | 6          |

For example, include this line in a Key=Value parameter file to select partial trypsin
```ini
DigestionEnzyme=KROneEnd
```

## Installation

* Download Protein-Digestion-Simulator.zip from [AppVeyor](https://ci.appveyor.com/project/PNNLCompMassSpec/protein-digestion-simulator/build/artifacts)
  * Alternatively, download from the [GitHub release page](https://github.com/PNNL-Comp-Mass-Spec/Protein-Digestion-Simulator/releases)
* Extract the files
* Run ProteinDigestionSimulator.exe to start the GUI

## Console Switches

The Protein Digestion Simulator is typically used as a GUI application, but it also can be run from the Windows console, 
or on Linux using [Mono](https://www.mono-project.com/).  

Syntax:
```
ProteinDigestionSimulator.exe
  /I:SourceFastaOrTextFile [/F] [/D] [/M] [/AD:AlternateDelimiter]
  [/O:OutputDirectoryPath] [/P:ParameterFilePath] [/S:[MaxLevel]]
  [/A:AlternateOutputDirectoryPath] [/R]
```

The input file path can contain the wildcard character * and should point to a FASTA file or tab-delimited text file
* Gzipped FASTA files are also supported, e.g. E_coli_K12_UniProt.fasta.gz

Use `/F` to indicate that the input file is a FASTA file
* If `/F` is not used, the format will be assumed to be FASTA only if the filename ends with .fasta or .fasta.gz

Use `/D` to indicate that an in-silico digestion of the proteins should be performed
* Digestion options must be specified in the parameter file

Use `/M` to indicate that protein mass should be computed

Use `/AD `to specify a delimiter other than the Tab character (not applicable for FASTA files)

The output directory path is optional
* If omitted, the output files will be created in the same director as the input file

The parameter file path is optional
* If included, it should point to a valid Name=Value parameter file

Use `/S` to process all valid files in the input director and subdirectories. Include a number after `/S` (like `/S:2`) to limit the level of subdirectories to examine.
* When using `/S`, you can redirect the output of the results using `/A`
* When using `/S`, you can use `/R` to re-create the input director hierarchy in the alternate output director (if defined)

Use `/L` or `/Log` to log messages to a file
* By default, log files will include the current date, e.g. `ProteinDigestionSimulator_log_2021-10-20.txt`
* You can define the name of the log file using `/L:LogFileName.txt`

The processing options can be specified in a parameter file using `/ParamFile:Options.conf` or `/Conf:Options.conf` or `/P:Options.conf`
* Define options using the format `ArgumentName=Value`
* Lines starting with `#` or `;` will be treated as comments
* Additional arguments on the command line can supplement or override the arguments in the parameter file

Use `/CreateParamFile` to create an example parameter file
* By default, the example parameter file content is shown at the console
* To create a file named Options.conf, use `/CreateParamFile:Options.conf`

## Contacts

Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) \
E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov\
Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics

## License

The Protein Digestion Simulator is licensed under the 2-Clause BSD License;
you may not use this program except in compliance with the License.  You may obtain
a copy of the License at https://opensource.org/licenses/BSD-2-Clause

Copyright 2021 Battelle Memorial Institute

The NET Prediction DLL is licensed under the Reciprocal Public License v1.5;
for details see file PNNL_NETPrediction_License.pdf\
You may obtain a copy of the License at https://opensource.org/licenses/rpl1.5.txt
