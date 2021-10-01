using System;
using System.Collections.Generic;

namespace ProteinDigestionSimulator
{
    // -------------------------------------------------------------------------------
    // Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2005
    //
    // E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
    // Website: https://omics.pnl.gov/ or https://www.pnnl.gov/sysbio/ or https://panomics.pnnl.gov/
    // -------------------------------------------------------------------------------
    //
    // Licensed under the 2-Clause BSD License; you may not use this file except
    // in compliance with the License.  You may obtain a copy of the License at
    // https://opensource.org/licenses/BSD-2-Clause
    //
    // Copyright 2018 Battelle Memorial Institute

    /// <summary>
    /// This class will compute the pI (isoelectric point) and hydrophobicity for a peptide or protein sequence
    /// Code originally written by Gordon Anderson for the application ICR-2LS
    /// Ported to VB.NET by Matthew Monroe in August 2005
    /// </summary>
    public class ComputePeptideProperties
    {
        // Ignore Spelling: MaximumpI, hydrophobicity, hydrophilicity
        // Ignore Spelling: Mant, Hopp, Kyte, Eisenberg, Engleman, al

        public ComputePeptideProperties()
        {
            mAminoAcids = new Dictionary<char, AA>();
            InitializeLocalVariables();
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
        public enum HydrophobicityTypeConstants : int
        {
            HW = 0,                  // Hopp and Woods, values available at references 1 and 2
            KD = 1,                  // Kyte and Doolittle, values available t references 1 and 2
            Eisenberg = 2,           // Eisenberg, values available t references 1 and 2
            GES = 3,                 // Engleman et. al., values available at reference 1
            MeekPH7p4 = 4,           // Meek, pH 7.4; column 14 in table 3 of reference 3
            MeekPH2p1 = 5           // Meek, pH 2.1; column 3  in table 3 of reference 3
        }

        // Dissociation constants                ' Alternate values
        private const double Ck = 9.3d;        // 8.3
        private const double Dk = 4.5d;        // 3.91
        private const double Ek = 4.6d;        // 4.25
        private const double Hk = 6.2d;        // 6.5
        private const double Kk = 10.4d;       // 10.79
        private const double Rk = 12d;         // 12.5
        private const double Yk = 9.7d;        // 10.95
        private const double NH2k = 7.3d;      // 8.56
        private const double COOHk = 3.9d;     // 3.56

        private readonly struct AA
        {
            /// <summary>
            /// One letter abbreviation for the amino acid
            /// </summary>
            // ReSharper disable once NotAccessedField.Local
            public string Symbol { get; }
            public double HW { get; }
            public double KD { get; }
            public double Eisenberg { get; }
            public double GES { get; }
            public double MeekPH7p4 { get; }
            public double MeekPH2p1 { get; }

            public AA(string symbol, double hw, double kd, double eisenberg, double ges, double meekPH7p4, double meekPH2p1)
            {
                Symbol = symbol;
                HW = hw;
                KD = kd;
                Eisenberg = eisenberg;
                GES = ges;
                MeekPH7p4 = meekPH7p4;
                MeekPH2p1 = meekPH2p1;
            }
        }

        private readonly Dictionary<char, AA> mAminoAcids;

        /// <summary>
        /// Hydrophobicity type
        /// </summary>
        public HydrophobicityTypeConstants HydrophobicityType { get; set; }

        /// <summary>
        /// When true, examine the protein residues in chunks of SequenceWidthToExamineForMaximumpI,
        /// compute the pI for each chunk, then report the largest pI
        /// </summary>
        public bool ReportMaximumpI { get; set; }

        /// <summary>
        /// Number of residues to use for computation of pI when ReportMaximumpI is true
        /// </summary>
        public int SequenceWidthToExamineForMaximumpI { get; set; }

        private double CalculateCharge(double pH, int numC, int numD, int numE, int numH, int numK, int numR, int numY)
        {
            var value = 0d;
            value += CalculateNp(pH, Ck, numC);
            value += CalculateNp(pH, Dk, numD);
            value += CalculateNp(pH, Ek, numE);
            value += CalculateNp(pH, Hk, numH);
            value += CalculateNp(pH, Kk, numK);
            value += CalculateNp(pH, Rk, numR);
            value += CalculateNp(pH, Yk, numY);
            value += CalculateNp(pH, NH2k, 1);
            value += CalculateNp(pH, COOHk, 1);
            value -= numC + numD + numE + numY + 1;
            return value;
        }

        private double CalculateHydrophobicity(string seq, HydrophobicityTypeConstants HT)
        {
            var runningSum = 0d;
            var residueCount = 0;

            foreach (var c in seq)
            {
                var residue = char.ToUpper(c);

                try
                {
                    if (!mAminoAcids.TryGetValue(residue, out var aaInfo))
                    {
                        continue;
                    }

                    switch (HT)
                    {
                        case HydrophobicityTypeConstants.HW:
                            runningSum += aaInfo.HW;
                            break;
                        case HydrophobicityTypeConstants.KD:
                            runningSum += aaInfo.KD;
                            break;
                        case HydrophobicityTypeConstants.Eisenberg:
                            runningSum += aaInfo.Eisenberg;
                            break;
                        case HydrophobicityTypeConstants.GES:
                            runningSum += aaInfo.GES;
                            break;
                        case HydrophobicityTypeConstants.MeekPH7p4:
                            runningSum += aaInfo.MeekPH7p4;
                            break;
                        case HydrophobicityTypeConstants.MeekPH2p1:
                            runningSum += aaInfo.MeekPH2p1;
                            break;
                    }

                    residueCount += 1;
                }
                catch
                {
                    // Residue is not present so ignore it
                }
            }

            if (residueCount > 0)
            {
                return runningSum / residueCount;
            }

            return 0d;
        }

        private double CalculateNp(double pH, double k, int n)
        {
            return n * (Math.Pow(10d, -pH) / (Math.Pow(10d, -pH) + Math.Pow(10d, -k)));
        }

        // ReSharper disable once UnusedMember.Global
        public int CalculateSequenceChargeState(string seq, double pH)
        {
            int chargeState;
            if (string.IsNullOrEmpty(seq))
            {
                return 0;
            }

            try
            {
                chargeState = 0;
                foreach (var c in seq)
                {
                    switch (char.ToUpper(c))
                    {
                        case 'C':
                            if (Ck > pH)
                            {
                                chargeState += 1;
                            }

                            break;
                        case 'D':
                            if (Dk > pH)
                            {
                                chargeState += 1;
                            }

                            break;
                        case 'E':
                            if (Ek > pH)
                            {
                                chargeState += 1;
                            }

                            break;
                        case 'H':
                            if (Hk > pH)
                            {
                                chargeState += 1;
                            }

                            break;
                        case 'K':
                            if (Kk > pH)
                            {
                                chargeState += 1 + 1;
                            }

                            break;
                        case 'R':
                            if (Rk > pH)
                            {
                                chargeState += 1;
                            }

                            break;
                        case 'Y':
                            if (Yk > pH)
                            {
                                chargeState += 1;
                            }

                            break;
                    }
                }

                if (chargeState == 0)
                {
                    chargeState = 1;
                }
            }
            catch
            {
                // Error occurred
                chargeState = 1;
            }

            return chargeState;
        }

        public float CalculateSequenceHydrophobicity(string seq)
        {
            if (string.IsNullOrEmpty(seq))
            {
                return 0f;
            }

            try
            {
                if (ReportMaximumpI && seq.Length > SequenceWidthToExamineForMaximumpI)
                {
                    var maxHydrophobicity = 0d;
                    for (var index = 0; index < seq.Length - SequenceWidthToExamineForMaximumpI; index++)
                    {
                        var segmentHydrophobicity = CalculateHydrophobicity(seq.Substring(index, SequenceWidthToExamineForMaximumpI), HydrophobicityType);
                        if (segmentHydrophobicity > maxHydrophobicity)
                        {
                            maxHydrophobicity = segmentHydrophobicity;
                        }
                    }

                    return (float)maxHydrophobicity;
                }

                var hydrophobicity = CalculateHydrophobicity(seq, HydrophobicityType);
                return (float)hydrophobicity;
            }
            catch
            {
                // Error occurred
                return 0f;
            }
        }

        public float CalculateSequencepI(string seq)
        {
            double pH;
            if (string.IsNullOrEmpty(seq))
            {
                return 0f;
            }

            try
            {
                var numC = 0;
                var numD = 0;
                var numE = 0;
                var numH = 0;
                var numK = 0;
                var numR = 0;
                var numY = 0;
                foreach (var c in seq)
                {
                    switch (char.ToUpper(c))
                    {
                        case 'C':
                            numC += 1;
                            break;
                        case 'D':
                            numD += 1;
                            break;
                        case 'E':
                            numE += 1;
                            break;
                        case 'H':
                            numH += 1;
                            break;
                        case 'K':
                            numK += 1;
                            break;
                        case 'R':
                            numR += 1;
                            break;
                        case 'Y':
                            numY += 1;
                            break;
                    }
                }

                pH = 1d;
                var delta = 1d;
                var Value = CalculateCharge(pH, numC, numD, numE, numH, numK, numR, numY) + 1d;
                while (true)
                {
                    var value1 = CalculateCharge(pH, numC, numD, numE, numH, numK, numR, numY);
                    if (Math.Abs(value1) <= Math.Abs(Value))
                    {
                        Value = value1;
                        pH += delta;
                    }
                    else
                    {
                        delta /= -10;
                        Value = value1;
                        pH += delta;
                        if (Math.Abs(delta) < 0.01d)
                        {
                            break;
                        }
                    }
                }
            }
            catch
            {
                // Error occurred
                pH = 0d;
            }

            return (float)pH;
        }

        private void AddAminoAcid(char oneLetterSymbol, double hw, double kd, double eisenberg, double ges, double meekPH7p4, double meekPH2p1)
        {
            mAminoAcids.Add(oneLetterSymbol, new AA(oneLetterSymbol.ToString(), hw, kd, eisenberg, ges, meekPH7p4, meekPH2p1));
        }

        private void InitializeLocalVariables()
        {
            HydrophobicityType = HydrophobicityTypeConstants.HW;
            ReportMaximumpI = false;
            SequenceWidthToExamineForMaximumpI = 10;

            LoadAminoAcids();
        }

        private void LoadAminoAcids()
        {
            mAminoAcids.Clear();

            AddAminoAcid('A', -0.5d, 1.8d, 0.25d, -1.6d, 0.5d, -0.1d);
            AddAminoAcid('C', -1, 2.5d, 0.04d, -2, -6.8d, -2.2d);
            AddAminoAcid('D', 3d, -3.5d, -0.72d, 9.2d, -8.2d, -2.8d);
            AddAminoAcid('E', 3d, -3.5d, -0.62d, 8.2d, -16.9d, -7.5d);
            AddAminoAcid('F', -2.5d, 2.8d, 0.61d, -3.7d, 13.2d, 13.9d);
            AddAminoAcid('G', 0d, -0.4d, 0.16d, -1, 0d, -0.5d);
            AddAminoAcid('H', -0.5d, -3.2d, -0.4d, 3d, -3.5d, 0.8d);
            AddAminoAcid('I', -1.8d, 4.5d, 0.73d, -3.1d, 13.9d, 11.8d);
            AddAminoAcid('K', 3d, -3.9d, -1.1d, 8.8d, 0.1d, -3.2d);
            AddAminoAcid('L', -1.8d, 3.8d, 0.53d, -2.8d, 8.8d, 10d);
            AddAminoAcid('M', -1.3d, 1.9d, 0.26d, -3.4d, 4.8d, 7.1d);
            AddAminoAcid('N', 0.2d, -3.5d, -0.64d, 4.8d, 0.8d, -1.6d);
            AddAminoAcid('P', 0d, -1.6d, -0.07d, 0.2d, 6.1d, 8d);
            AddAminoAcid('Q', 0.2d, -3.5d, -0.85d, -4.1d, -4.8d, -2.5d);
            AddAminoAcid('R', 3d, -4.5d, -1.8d, 12.3d, 0.8d, -4.5d);
            AddAminoAcid('S', 0.3d, -0.8d, -0.26d, -0.6d, 1.2d, -3.7d);
            AddAminoAcid('T', -0.4d, -0.7d, -0.18d, -1.2d, 2.7d, 1.5d);
            AddAminoAcid('V', -1.5d, 4.2d, 0.54d, -2.6d, 2.1d, 3.3d);
            AddAminoAcid('W', -3.4d, -0.9d, 0.37d, -1.9d, 14.9d, 18.1d);
            AddAminoAcid('Y', -2.3d, -1.3d, 0.02d, 0.7d, 6.1d, 8.2d);
        }
    }
}
