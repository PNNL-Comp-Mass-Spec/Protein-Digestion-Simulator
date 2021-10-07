# Methods for in silico tryptic digestion, formula calculation, monoisotopic mass
# calculation and b and y fragment ion simulation in R.
#
# Written by Tom Taverner for Pacific Northwest National Lab.
# 10-26-2010

# Methods for in silico digestion, 

# Required packages
install.packages("seqinr")
library(seqinr)
library(reshape)

  # lookup table for amino acid formulae
my.aa <- array(c(6, 6, 6, 5, 9, 4, 11, 5, 6, 6, 3, 4, 4, 3, 5, 5, 2, 5, 3, 9, 11, 11, 12, 9, 9, 7, 10, 9, 12, 7, 5, 6, 5, 5, 7, 8, 3, 7, 5, 9, 1, 1, 2, 1, 1, 1, 2, 1, 4, 3, 1, 2, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 2, 3, 1, 3, 2, 1, 1, 2, 2, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0), c(20L, 5L))
dimnames(my.aa) <- list(c("I", "L", "K", "M", "F", "T", "W", "V", "R", "H", "A", "N", "D", "C", "E", "Q", "G", "P", "S", "Y"), c("C", "H", "N", "O", "S"))

  # lookup table for monoisotopic masses
my.fw = c(C = 12.0, H = 1.00782503207, N = 14.0030740048, O = 15.99491461956, S = 31.97207100)


  # get.formula: given a string of 1-letter amino acid codes, returns the formula of the peptide
  # arguments: x, peptide sequence, 
  # plusFormula - default corresponds to H2O for peptides
    # b ions: plusFormula = c(C=0,H=0,N=0,O=0,S=0)
    # y ions: default is OK
  # charge, charge to place on sequence
get.formula <- function(x, charge=1, plusFormula=c(C=0, H=2, N=0, O=1, S=0)){
  f1 <- factor(strsplit(x, "")[[1]], levels=rownames(my.aa))
  colSums(rbind(model.matrix(~f1-1) %*% my.aa, plusFormula, c(C=0, H=charge, N=0, O=0, S=0)))
}

  # get.monoisotopic.mass: given a formula output from get.formula, get the monoisotopic mass
get.monoisotopic.mass <- function(ff){
  stopifnot(identical(names(ff), names(my.fw)))
  drop(ff %*% my.fw)
}


  # apply.protease: virtual protease digestion function
  # arguments:
  # prots: list containing protein sequences
  # missed: number of missed cleavages
  # include.re and exclude.re: regular expressions corresponding to protease cleavage rules
  #   the cleavage sites are the include.re matches that aren't in exclude.re
  # note: only supports cleavage on C-terminal side
  # output: the same list of prots, with $peptide containing 
  #   (1) list of peptides 
  #   (2) position of cleavage 
  #   (3) number of missed cleavages
apply.protease <- function(prots, missed=1, include.re = "[KR]", exclude.re = "([KR]$)|([KR][P])"){
 for(jj in seq(length=length(prots))){
   prot <- prots[[jj]][[1]]
   pos <- setdiff(gregexpr(include.re, prot)[[1]], gregexpr(exclude.re, prot)[[1]])

   pos <- c(0, c(pos), nchar(prot))
   n <- length(pos)+1
   nmis <- missed+1
   pid <- cbind(rep(1:n, each=nmis), rep(1:nmis, n))
   zz <- cbind(pos[pid[,1]], pos[pid[,1]+pid[,2]])
   ok.idx <- !rowSums(is.na(zz))
   zz <- zz[ok.idx,,drop=F]
     # this is where we'd extend the method with PTM's, etc
   suppressWarnings(prots[[jj]]$peps <- substr(rep(prot[[1]], nrow(zz)), zz[,1]+1, zz[,2]))

   attr(prots[[jj]]$peps, "start") <- pos[pid[ok.idx,1]]
   attr(prots[[jj]]$peps, "missed") <- pid[ok.idx,2]-1
  }   
  return(prots)
}


  # For a given protein from the apply.protease() output
  # create a Protein Prospector style data frame 
  # Input: a single list item from apply.protease() output
  # Output: a data frame containing sequence, start, missed cleavages, charge, monoisotopic m/z
create.peptides.table <- function(digested.peps, minMZ=800, maxMZ=4000, maxCharge=1){
 thePeps <- digested.peps$peps
 theFormulas <- sapply(thePeps, get.formula)
 theCharge <- rep(1:maxCharge, each=length(thePeps))

 mono.mz <- c()
 for(z in 1:maxCharge)
   mono.mz <- c(mono.mz, c(t(theFormulas)%*%my.fw + (z-1)*my.fw[["H"]])/z)

 peps.and.masses <- data.frame(MassToCharge = mono.mz, 
   Start = attr(thePeps, "start"), MissedCleavages = attr(thePeps, "missed"), 
   Charge = theCharge, Sequence=thePeps)

 # ensure we have reasonable numbers of charges - don't allow more charge than K + R + 1
 maxAllowedCharge <- sapply(gregexpr("[KR]", thePeps), length)+1
 peps.and.masses <- peps.and.masses[peps.and.masses$Charge <= maxAllowedCharge,,drop=FALSE]

 peps.and.masses <- peps.and.masses[minMZ< peps.and.masses$MassToCharge 
   & peps.and.masses$MassToCharge < maxMZ,,drop=FALSE]
 peps.and.masses <- peps.and.masses[order(peps.and.masses$MassToCharge),,drop=FALSE]


 return(peps.and.masses)
}

 # create b and y ions for a given peptide sequence
 # input: peptide sequence
 # output: list containing b.ions and y.ions
 # both are named vectors containing monoisotopic singly charged masses of ions
create.ions <- function(peptide){
  ncp <- nchar(peptide)
  b.sequences <- c(mapply(substr, rep(peptide, ncp), 1, 1:ncp))
  names(b.sequences) <- paste("b", 1:length(b.sequences), sep="")

  b.ions <- sapply(b.sequences, 
   function(x) get.monoisotopic.mass(get.formula(x, plusFormula = c(C=0,H=0,N=0,O=0,S=0))))
  b.ions[1] <- NA # there's no such thing as a b1 ion ;-)
  b.ions[length(b.ions)] <- NA


  y.sequences <- c(mapply(substr, rep(peptide, ncp), ncp:1, ncp))
  names(y.sequences) <- paste("y", 1:length(y.sequences), sep="")

  y.ions <- sapply(y.sequences, 
   function(x) get.monoisotopic.mass(get.formula(x, plusFormula = c(C=0,H=2,N=0,O=1,S=0))))
  y.ions[length(y.ions)] <- NA

  return(list(b.ions=b.ions, y.ions=y.ions))
}



#
# Demonstration code
#

library(seqinr)
  # read in the Shew fasta file here
my.fasta <- read.fasta(file.choose(), as.string=TRUE, forceDNAtolower=FALSE)

 # take the first 5
prots <- my.fasta[1:5]

 # apply simulated tryptic digestion to get a list of all peptides given 1 missed
digested <- apply.protease(prots, missed=1)
 
 # create tables including monoisotopic masses, etc
peptide.tables <- lapply(digested, create.peptides.table, minMZ=800, maxMZ=4000, maxCharge=3)

library(RGtk2Extras)
dfedit(peptide.tables$SO_0001)
# compare to prospector
http://prospector.ucsf.edu/prospector/cgi-bin/msform.cgi?form=msdigest

# enter in MTKIAILVGTTLGSSEYIADEMQAQLTPLGHEVHTFLHPTLDELKPYPLWILVSSTHGAGDLPDNLQPFCKELLL
NTPDLTQVKFALCAIGDSSYDTFCQGPEKLIEALEYSGAKAVVDKIQIDVQQDPVPEDPALAWLAQWQDQI
# notice we predict some peptides their algorithm doesn't...

 # bring peptides from first 5 proteins together
library(reshape)
mm <- melt(peptide.tables, measure.vars="MassToCharge")
all.peptides <- cast(mm)
colnames(all.peptides)[colnames(all.peptides)=="L1"] <- "Protein"
all.peptides <- all.peptides[order(all.peptides$MassToCharge),,drop=FALSE]

 # take a look
dfedit(all.peptides)

# generate some b and y ions
my.fragments <- create.ions("LIEALEYSGAKAVVDK")
data.frame(b.type=names(my.fragments$b.ions), b.mass=my.fragments$b.ions, y.type=rev(names(my.fragments$y.ions)), y.mass=rev(my.fragments$y.ions))

# compare to prospector
http://prospector.ucsf.edu/prospector/cgi-bin/mssearch.cgi?search_name=msproduct&output_type=HTML&report_title=MS-Product&version=5.6.2&instrument_name=ESI-Q-TOF&use_instrument_ion_types=1&parent_mass_convert=monoisotopic&user_aa_composition=C2%20H3%20N1%20O1&max_charge=2&s=1&sequence=LIEALEYSGAKAVVDK&

