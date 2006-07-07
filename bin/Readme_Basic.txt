Protein Digestion Simulator Basic

The Protein Digestion Simulator Basic can be used to read a text file containing 
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
NET values are computed using a hydrophobicity model developed by Oleg Krokhin 
et. al. (see Krokhin, Craig, Spicer, Ens, Standing, Beavis, and Wilkins. 2004.
Molecular & Cellular Proteomics, 3 (9), 908-919).

Double click the ProteinDigestionSimulatorBasic_Installer.msi file to install.
The program shortcut can be found at Start Menu -> Programs -> PAST Toolkit -> Protein Digestion Simulator Basic

-------------------------------------------------------------------------------
Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
Copyright 2005, Battelle Memorial Institute.  All Rights Reserved.

E-mail: matthew.monroe@pnl.gov or matt@alchemistmatt.com
Website: http://ncrr.pnl.gov/ or http://www.sysbio.org/resources/staff/
-------------------------------------------------------------------------------

Licensed under the Apache License, Version 2.0; you may not use this file except 
in compliance with the License.  You may obtain a copy of the License at 
http://www.apache.org/licenses/LICENSE-2.0

All publications that result from the use of this software should include 
the following acknowledgment statement:
 Portions of this research were supported by the W.R. Wiley Environmental 
 Molecular Science Laboratory, a national scientific user facility sponsored 
 by the U.S. Department of Energy's Office of Biological and Environmental 
 Research and located at PNNL.  PNNL is operated by Battelle Memorial Institute 
 for the U.S. Department of Energy under contract DE-AC05-76RL0 1830.

Notice: This computer software was prepared by Battelle Memorial Institute, 
hereinafter the Contractor, under Contract No. DE-AC05-76RL0 1830 with the 
Department of Energy (DOE).  All rights in the computer software are reserved 
by DOE on behalf of the United States Government and the Contractor as 
provided in the Contract.  NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAKES ANY 
WARRANTY, EXPRESS OR IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THIS 
SOFTWARE.  This notice including this sentence must appear on any copies of 
this computer software.
