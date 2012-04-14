@echo on
Del NETPredictionUtility_Source*.zip
Del ProteinFileReader_Source*.zip
Del SharedVBNetRoutines_Source*.zip
Del SmoothProgressBar_Source*.zip
Del ValidateFastaFile_Source*.zip

@echo off
echo;

Copy "F:\My Documents\Projects\DataMining\NET_Prediction_Utility\NETPredictionUtility_SourceCode\*Source*.zip" .
Copy "F:\My Documents\Projects\DataMining\ProteinFileReaderDLL\ProteinFileReader_SourceCode\*.zip" .
Copy "F:\My Documents\Projects\DataMining\SharedVBNetRoutines\SharedVBNetRoutines_SourceCode\*.zip" .
Copy "F:\My Documents\Projects\DataMining\SmoothProgressBarDLL\SmoothProgressBar_SourceCode\*.zip" .
Copy "F:\My Documents\Projects\DataMining\Validate_Fasta_File\ValidateFastaFile_SourceCode\*Source*.zip" .
