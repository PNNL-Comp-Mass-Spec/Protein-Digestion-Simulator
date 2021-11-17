namespace ProteinDigestionSimulator.Options
{
    // Ignore Spelling: al., Eisenberg, Engelman, Hydrophilicity, Hydrophobicity, Hopp, Kyte, Mant

    /// <summary>
    /// Enzyme cleavage rules
    /// </summary>
    /// <remarks>
    /// A good list of enzymes is at
    /// https://web.expasy.org/peptide_cutter/peptidecutter_enzymes.html and
    /// https://web.expasy.org/peptide_mass/peptide-mass-doc.html
    /// </remarks>
    public enum CleavageRuleConstants
    {
        NoRule = 0,
        ConventionalTrypsin = 1,
        TrypsinWithoutProlineException = 2,
        EricPartialTrypsin = 3,
        // ReSharper disable once IdentifierTypo
        TrypsinPlusFVLEY = 4,
        KROneEnd = 5,
        TerminiiOnly = 6,
        Chymotrypsin = 7,
        ChymotrypsinAndTrypsin = 8,
        GluC = 9,
        CyanBr = 10, // Aka CNBr
        LysC = 11,
        GluC_EOnly = 12,
        ArgC = 13,
        AspN = 14,
        // ReSharper disable once IdentifierTypo
        ProteinaseK = 15,
        PepsinA = 16,
        PepsinB = 17,
        PepsinC = 18,
        PepsinD = 19,
        AceticAcidD = 20,
        TrypsinPlusLysC = 21,
        Thermolysin = 22,
        TrypsinPlusThermolysin = 23
    }

    /// <summary>
    /// Fragment mass range mode constants
    /// </summary>
    public enum FragmentMassConstants
    {
        Monoisotopic = 0,
        MH = 1
    }

    /// <summary>
    /// Hydrophobicity values for each amino acid
    /// </summary>
    /// <remarks>
    /// Originally from ICR-2LS
    /// Values confirmed via various resources:
    /// Ref 1: http://resources.qiagenbioinformatics.com/manuals/clcgenomicsworkbench/650/Hydrophobicity_scales.html
    /// Ref 2: https://web.expasy.org/protscale/
    /// Ref 3: Manuscript by Mant and Hodges at https://www.ncbi.nlm.nih.gov/pmc/articles/PMC2792893/
    ///        Intrinsic Amino Acid Side-Chain Hydrophilicity/Hydrophobicity Coefficients Determined
    ///        by Reversed-Phase High-Performance Liquid Chromatography of Model Peptides
    /// </remarks>
    public enum HydrophobicityTypeConstants
    {
        /// <summary>
        /// Hopp and Woods, values available at references 1 and 2
        /// </summary>
        /// <remarks>
        /// See also https://pubmed.ncbi.nlm.nih.gov/6191210/
        /// </remarks>
        HW = 0,

        /// <summary>
        /// Kyte and Doolittle, values available at references 1 and 2
        /// </summary>
        /// <remarks>
        /// See also https://pubmed.ncbi.nlm.nih.gov/7108955/
        /// </remarks>
        KD = 1,

        /// <summary>
        /// Eisenberg, values available at references 1 and 2
        /// </summary>
        /// <remarks>
        /// See also https://pubmed.ncbi.nlm.nih.gov/6502707/
        /// </remarks>
        Eisenberg = 2,

        /// <summary>
        /// Engelman et. al., values available at reference 1
        /// </summary>
        /// <remarks>
        /// See also https://pubmed.ncbi.nlm.nih.gov/3521657/
        /// </remarks>
        GES = 3,

        /// <summary>
        /// Meek, pH 7.4; column 14 in table 3 of reference 3;
        /// </summary>
        /// <remarks>
        /// See also https://pubmed.ncbi.nlm.nih.gov/19795449/
        /// </remarks>
        MeekPH7p4 = 4,

        /// <summary>
        /// Meek, pH 2.1; column 3  in table 3 of reference 3
        /// </summary>
        /// <remarks>
        /// See also https://pubmed.ncbi.nlm.nih.gov/19795449/
        /// </remarks>
        MeekPH2p1 = 5
    }
}
