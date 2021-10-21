using System;
using System.Collections.Generic;
using ProteinDigestionSimulator.Options;

namespace ProteinDigestionSimulator
{
    // -------------------------------------------------------------------------------
    // Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2005
    //
    // E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
    // Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics
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
    /// Ported to C# by Bryson Gibbons in 2021
    /// </summary>
    public class ComputePeptideProperties
    {
        // Ignore Spelling: al, Bryson, Eisenberg, Engleman, Hopp, hydrophilicity, hydrophobicity,
        // Ignore Spelling: isoelectric, Kyte, Mant, MaximumpI

        // Dissociation constants              // Alternate values
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
            // ReSharper disable once MemberCanBePrivate.Local
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
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

        private DigestionSimulatorOptions ProcessingOptions { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options"></param>
        public ComputePeptideProperties(DigestionSimulatorOptions options)
        {
            ProcessingOptions = options;

            mAminoAcids = new Dictionary<char, AA>();
            InitializeLocalVariables();
        }

        private double CalculateCharge(double pH, int numC, int numD, int numE, int numH, int numK, int numR, int numY)
        {
            var charge =
                CalculateNp(pH, Ck, numC) +
                CalculateNp(pH, Dk, numD) +
                CalculateNp(pH, Ek, numE) +
                CalculateNp(pH, Hk, numH) +
                CalculateNp(pH, Kk, numK) +
                CalculateNp(pH, Rk, numR) +
                CalculateNp(pH, Yk, numY) +
                CalculateNp(pH, NH2k, 1) +
                CalculateNp(pH, COOHk, 1);

            return charge - (numC + numD + numE + numY + 1);
        }

        private double CalculateHydrophobicity(string seq, HydrophobicityTypeConstants hydrophobicityMode)
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

                    runningSum += hydrophobicityMode switch
                    {
                        HydrophobicityTypeConstants.HW => aaInfo.HW,
                        HydrophobicityTypeConstants.KD => aaInfo.KD,
                        HydrophobicityTypeConstants.Eisenberg => aaInfo.Eisenberg,
                        HydrophobicityTypeConstants.GES => aaInfo.GES,
                        HydrophobicityTypeConstants.MeekPH7p4 => aaInfo.MeekPH7p4,
                        HydrophobicityTypeConstants.MeekPH2p1 => aaInfo.MeekPH2p1,
                        _ => throw new ArgumentOutOfRangeException(nameof(hydrophobicityMode), hydrophobicityMode, null),
                    };
                    residueCount++;
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
            if (string.IsNullOrEmpty(seq))
            {
                return 0;
            }

            try
            {
                var chargeState = 0;
                foreach (var c in seq)
                {
                    switch (char.ToUpper(c))
                    {
                        case 'C':
                            if (Ck > pH)
                            {
                                chargeState++;
                            }
                            break;

                        case 'D':
                            if (Dk > pH)
                            {
                                chargeState++;
                            }
                            break;

                        case 'E':
                            if (Ek > pH)
                            {
                                chargeState++;
                            }
                            break;

                        case 'H':
                            if (Hk > pH)
                            {
                                chargeState++;
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
                                chargeState++;
                            }
                            break;

                        case 'Y':
                            if (Yk > pH)
                            {
                                chargeState++;
                            }
                            break;
                    }
                }

                return chargeState >= 1 ? chargeState : 1;
            }
            catch
            {
                // Error occurred
                return 1;
            }
        }

        public float CalculateSequenceHydrophobicity(string seq)
        {
            if (string.IsNullOrEmpty(seq))
            {
                return 0f;
            }

            try
            {
                if (ProcessingOptions.ReportMaximumpI && seq.Length > ProcessingOptions.SequenceLengthToExamineForMaximumpI)
                {
                    var maxHydrophobicity = 0d;
                    for (var index = 0; index < seq.Length - ProcessingOptions.SequenceLengthToExamineForMaximumpI; index++)
                    {
                        var segmentHydrophobicity = CalculateHydrophobicity(seq.Substring(index, ProcessingOptions.SequenceLengthToExamineForMaximumpI), ProcessingOptions.HydrophobicityMode);
                        if (segmentHydrophobicity > maxHydrophobicity)
                        {
                            maxHydrophobicity = segmentHydrophobicity;
                        }
                    }

                    return (float)maxHydrophobicity;
                }

                var hydrophobicity = CalculateHydrophobicity(seq, ProcessingOptions.HydrophobicityMode);
                return (float)hydrophobicity;
            }
            catch
            {
                // Error occurred
                return 0f;
            }
        }

        public float CalculateSequencepI(string peptideSequence)
        {
            double pH;
            if (string.IsNullOrEmpty(peptideSequence))
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

                foreach (var oneLetterSymbol in peptideSequence)
                {
                    switch (char.ToUpper(oneLetterSymbol))
                    {
                        case 'C':
                            numC++;
                            break;
                        case 'D':
                            numD++;
                            break;
                        case 'E':
                            numE++;
                            break;
                        case 'H':
                            numH++;
                            break;
                        case 'K':
                            numK++;
                            break;
                        case 'R':
                            numR++;
                            break;
                        case 'Y':
                            numY++;
                            break;
                    }
                }

                pH = 1;
                double delta = 1;
                var charge = CalculateCharge(pH, numC, numD, numE, numH, numK, numR, numY) + 1;

                while (true)
                {
                    var alternateCharge = CalculateCharge(pH, numC, numD, numE, numH, numK, numR, numY);
                    if (Math.Abs(alternateCharge) <= Math.Abs(charge))
                    {
                        charge = alternateCharge;
                        pH += delta;
                    }
                    else
                    {
                        delta /= -10;
                        charge = alternateCharge;
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
