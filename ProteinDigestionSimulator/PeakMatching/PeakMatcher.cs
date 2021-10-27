using System;
using System.Collections.Generic;
using ProteinDigestionSimulator.Options;

// ReSharper disable UnusedMember.Global

namespace ProteinDigestionSimulator.PeakMatching
{
    public enum MessageTypeConstants
    {
        Normal = 0,
        ErrorMsg = 1,
        Warning = 2,
        Health = 3
    }

    public class PeakMatcher
    {
        // Ignore Spelling: Da, Sql, tol

        private bool mAbortProcessing;

        public event ProgressChangedEventHandler ProgressChanged;
        public event LogEventEventHandler LogEvent;

        private PeakMatchingOptions PeakMatchingOptions { get; }

        public string ProgressDescription { get; private set; }

        public float ProgressPct { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="peakMatchingOptions"></param>
        public PeakMatcher(PeakMatchingOptions peakMatchingOptions)
        {
            PeakMatchingOptions = peakMatchingOptions;
        }
        private void ComputeSLiCScores(
            FeatureInfo featureToIdentify,
            PMFeatureMatchResults featureMatchResults,
            List<PeakMatchingRawMatches> rawMatches,
            PMComparisonFeatureInfo comparisonFeatures,
            SearchThresholds searchThresholds,
            SearchThresholds.SearchTolerances computedTolerances)
        {
            int index;
            string message;

            // Compute the match scores (aka SLiC scores)

            var massStDevPPM = searchThresholds.SLiCScoreMassPPMStDev;
            if (massStDevPPM <= 0d)
            {
                massStDevPPM = 3d;
            }

            var massStDevAbs = searchThresholds.PPMToMass(massStDevPPM, featureToIdentify.Mass);
            if (massStDevAbs <= 0d)
            {
                message = "Assertion failed in ComputeSLiCScores; massStDevAbs is <= 0, which isn't allowed; will assume 0.003";
                Console.WriteLine(message);
                PostLogEntry(message, MessageTypeConstants.ErrorMsg);
                massStDevAbs = 0.003d;
            }

            // Compute the standardized squared distance and the numerator sum
            var numeratorSum = 0d;
            for (index = 0; index < rawMatches.Count; index++)
            {
                double netStDevCombined;
                if (searchThresholds.SLiCScoreUseAMTNETStDev)
                {
                    // The NET StDev is computed by combining the default NETStDev value with the Comparison Features' specific NETStDev
                    // The combining is done by "adding in quadrature", which means to square each number, add together, and take the square root
                    netStDevCombined = Math.Sqrt(Math.Pow(searchThresholds.SLiCScoreNETStDev, 2d) + Math.Pow(comparisonFeatures.GetNETStDevByRowIndex(rawMatches[index].MatchingIDIndex), 2d));
                }
                else
                {
                    // Simply use the default NETStDev value
                    netStDevCombined = searchThresholds.SLiCScoreNETStDev;
                }

                if (netStDevCombined <= 0d)
                {
                    message = "Assertion failed in ComputeSLiCScores; netStDevCombined is <= 0, which isn't allowed; will assume 0.025";
                    Console.WriteLine(message);
                    PostLogEntry(message, MessageTypeConstants.ErrorMsg);
                    netStDevCombined = 0.025d;
                }

                rawMatches[index].StandardizedSquaredDistance = Math.Pow(rawMatches[index].MassErr, 2d) / Math.Pow(massStDevAbs, 2d) +
                                                                Math.Pow(rawMatches[index].NETErr, 2d) / Math.Pow(netStDevCombined, 2d);

                rawMatches[index].SLiCScoreNumerator = 1d / (massStDevAbs * netStDevCombined) * Math.Exp(-rawMatches[index].StandardizedSquaredDistance / 2d);

                numeratorSum += rawMatches[index].SLiCScoreNumerator;
            }

            // Compute the match score for each match
            for (index = 0; index < rawMatches.Count; index++)
            {
                if (numeratorSum > 0d)
                {
                    rawMatches[index].SLiCScore = Math.Round(rawMatches[index].SLiCScoreNumerator / numeratorSum, 5);
                }
                else
                {
                    rawMatches[index].SLiCScore = 0d;
                }
            }

            if (rawMatches.Count > 1)
            {
                // Sort by SLiCScore descending
                rawMatches.Sort();
            }

            if (rawMatches.Count > 0)
            {
                // Compute the DelSLiC value
                // If there is only one match, the DelSLiC value is 1
                // If there is more than one match, the highest scoring match gets a DelSLiC value,
                // computed by subtracting the next lower scoring value from the highest scoring value; all
                // other matches get a DelSLiC score of 0
                // This allows one to quickly identify the features with a single match (DelSLiC = 1) or with a match
                // distinct from other matches (DelSLiC > threshold)

                if (rawMatches.Count > 1)
                {
                    rawMatches[0].DelSLiC = rawMatches[0].SLiCScore - rawMatches[1].SLiCScore;

                    for (index = 1; index < rawMatches.Count; index++)
                    {
                        rawMatches[index].DelSLiC = 0d;
                    }
                }
                else
                {
                    rawMatches[0].DelSLiC = 1d;
                }

                // Now filter the list using the tighter tolerances:
                // Since we're shrinking the array, we can copy in place
                //
                // When testing whether to keep the match or not, we're testing whether the match is in the ellipse bounded by MWTolAbsFinal and NETTolFinal
                // Note that these are half-widths of the ellipse
                var newMatches = new List<PeakMatchingRawMatches>();
                for (index = 0; index < rawMatches.Count; index++)
                {
                    if (TestPointInEllipse(rawMatches[index].NETErr, rawMatches[index].MassErr, computedTolerances.NETTolFinal, computedTolerances.MWTolAbsFinal))
                    {
                        newMatches.Add(rawMatches[index]);
                    }
                }

                rawMatches.Clear();
                rawMatches.AddRange(newMatches);

                // Add new match results to featureMatchResults
                // Record, at most, mMaxPeakMatchingResultsPerFeatureToSave entries
                for (index = 0; index < Math.Min(PeakMatchingOptions.MaxPeakMatchingResultsPerFeatureToSave, rawMatches.Count); index++)
                {
                    comparisonFeatures.GetFeatureInfoByRowIndex(rawMatches[index].MatchingIDIndex, out var comparisonFeatureInfo);
                    featureMatchResults.AddMatch(featureToIdentify.FeatureID, comparisonFeatureInfo.FeatureID,
                                                 rawMatches[index].SLiCScore, rawMatches[index].DelSLiC,
                                                 rawMatches[index].MassErr, rawMatches[index].NETErr,
                                                 rawMatches.Count);
                }
            }
        }

        internal static bool FillRangeSearchObject(SearchRange rangeSearch, PMComparisonFeatureInfo comparisonFeatures)
        {
            // Initialize the range searching class

            const int LOAD_BLOCK_SIZE = 50000;

            rangeSearch.ClearData();

            if (comparisonFeatures.Count == 0)
            {
                // No comparison features to search against
                return false;
            }

            rangeSearch.InitializeDataFillDouble(comparisonFeatures.Count);

            var index = 0;
            // for (index = 0; i < comparisonFeatures.Count; i++)
            //     rangeSearch.FillWithDataAddPoint(comparisonFeatures.GetMassByRowIndex(index));

            var comparisonFeatureCount = comparisonFeatures.Count;
            while (index < comparisonFeatureCount)
            {
                rangeSearch.FillWithDataAddBlock(comparisonFeatures.GetMassArrayByRowRange(index, index + LOAD_BLOCK_SIZE - 1));
                index += LOAD_BLOCK_SIZE;
            }

            return rangeSearch.FinalizeDataFill();
        }

        internal bool IdentifySequences(
            SearchThresholds searchThresholds,
            PMFeatureInfo featuresToIdentify,
            PMComparisonFeatureInfo comparisonFeatures,
            out PMFeatureMatchResults featureMatchResults,
            SearchRange rangeSearch)
        {
            // Returns True if success, False if the search is canceled
            // Will return true even if none of the features match any of the comparison features
            //
            // If rangeSearch is Nothing or if rangeSearch contains a different number of entries than comparisonFeatures,
            // then will auto-populate it; otherwise, assumes it is populated

            // Note that featureMatchResults will only contain info on the features in featuresToIdentify that matched entries in comparisonFeatures

            bool success;

            // if (mUseSqlServerForMatchResults)
            //     featureMatchResults = new PMFeatureMatchResults(mSqlServerConnectionString, mTableNameFeatureMatchResults);
            // else
            featureMatchResults = new PMFeatureMatchResults();

            if (rangeSearch.DataCount != comparisonFeatures.Count)
            {
                success = FillRangeSearchObject(rangeSearch, comparisonFeatures);
            }
            else
            {
                success = true;
            }

            if (!success)
            {
                return false;
            }

            try
            {
                var featureCount = featuresToIdentify.Count;

                UpdateProgress("Finding matching peptides for given search thresholds", 0f);
                mAbortProcessing = false;

                PostLogEntry("IdentifySequences starting, total feature count = " + featureCount, MessageTypeConstants.Normal);

                for (var featureIndex = 0; featureIndex < featureCount; featureIndex++)
                {
                    // Use rangeSearch to search for matches to each peptide in comparisonFeatures

                    if (featuresToIdentify.GetFeatureInfoByRowIndex(featureIndex, out var currentFeatureToIdentify))
                    {
                        // By Calling .GetComputedSearchTolerances() with a mass, the tolerances will be auto re-computed
                        var computedTolerances = searchThresholds.GetComputedSearchTolerances(currentFeatureToIdentify.Mass);

                        double netTol;
                        double massTol;
                        if (PeakMatchingOptions.UseMaxSearchDistanceMultiplierAndSLiCScore)
                        {
                            massTol = computedTolerances.MWTolAbsBroad;
                            netTol = computedTolerances.NETTolBroad;
                        }
                        else
                        {
                            massTol = computedTolerances.MWTolAbsFinal;
                            netTol = computedTolerances.NETTolFinal;
                        }

                        var matchInd1 = 0;
                        var matchInd2 = -1;

                        if (rangeSearch.FindValueRange(currentFeatureToIdentify.Mass, massTol, ref matchInd1, ref matchInd2))
                        {
                            // The following hold the matches using the broad search tolerances (if .UseMaxSearchDistanceMultiplierAndSLiCScore = True, otherwise, simply holds the matches)
                            // Pointers into comparisonFeatures; list of peptides that match within both mass and NET tolerance
                            var rawMatches = new List<PeakMatchingRawMatches>();

                            for (var matchIndex = matchInd1; matchIndex <= matchInd2; matchIndex++)
                            {
                                var comparisonFeaturesOriginalRowIndex = rangeSearch.GetOriginalIndex(matchIndex);

                                if (comparisonFeatures.GetFeatureInfoByRowIndex(comparisonFeaturesOriginalRowIndex, out var currentComparisonFeature))
                                {
                                    double netDiff = currentFeatureToIdentify.NET - currentComparisonFeature.NET;
                                    if (Math.Abs(netDiff) <= netTol)
                                    {
                                        bool storeMatch;
                                        if (PeakMatchingOptions.UseMaxSearchDistanceMultiplierAndSLiCScore)
                                        {
                                            // Store this match
                                            storeMatch = true;
                                        }
                                        // The match is within a rectangle defined by computedTolerances.MWTolAbsBroad and computedTolerances.NETTolBroad
                                        else if (PeakMatchingOptions.UseEllipseSearchRegion)
                                        {
                                            // Only keep the match if it's within the ellipse defined by the search tolerances
                                            // Note that the search tolerances we send to TestPointInEllipse should be half-widths (i.e. tolerance +- comparison value), not full widths
                                            storeMatch = TestPointInEllipse(netDiff, currentFeatureToIdentify.Mass - currentComparisonFeature.Mass, netTol, massTol);
                                        }
                                        else
                                        {
                                            storeMatch = true;
                                        }

                                        if (storeMatch)
                                        {
                                            var rawMatch = new PeakMatchingRawMatches(comparisonFeaturesOriginalRowIndex)
                                            {
                                                SLiCScore = -1,
                                                MassErr = currentFeatureToIdentify.Mass - currentComparisonFeature.Mass,
                                                NETErr = netDiff
                                            };

                                            rawMatches.Add(rawMatch);
                                        }
                                    }
                                }
                            }

                            if (rawMatches.Count > 0)
                            {
                                rawMatches.Capacity = rawMatches.Count;
                                // Store the FeatureIDIndex in featureMatchResults
                                // Compute the SLiC Scores and store the results
                                ComputeSLiCScores(currentFeatureToIdentify, featureMatchResults, rawMatches, comparisonFeatures, searchThresholds, computedTolerances);
                            }
                        }
                    }
                    else
                    {
                        var message = "Programming error in IdentifySequences: Feature not found in featuresToIdentify using feature index: " + featureIndex;
                        Console.WriteLine(message);
                        PostLogEntry(message, MessageTypeConstants.ErrorMsg);
                    }

                    if (featureIndex % 100 == 0)
                    {
                        UpdateProgress((float)(featureIndex / (double)featureCount * 100d));
                        if (mAbortProcessing)
                        {
                            break;
                        }
                    }

                    if (featureIndex % 10000 == 0 && featureIndex > 0)
                    {
                        PostLogEntry("IdentifySequences, featureIndex = " + featureIndex, MessageTypeConstants.Health);
                    }
                }

                UpdateProgress("IdentifySequences complete", 100f);
                PostLogEntry("IdentifySequences complete", MessageTypeConstants.Normal);

                success = !mAbortProcessing;
            }
            catch
            {
                success = false;
            }

            return success;
        }

        private void PostLogEntry(string message, MessageTypeConstants entryType)
        {
            LogEvent?.Invoke(message, entryType);
        }

        private bool TestPointInEllipse(double pointX, double pointY, double xTol, double yTol)
        {
            // The equation for the points along the edge of an ellipse is x^2/a^2 + y^2/b^2 = 1 where a and b are
            // the half-widths of the ellipse and x and y are the coordinates of each point on the ellipse's perimeter
            //
            // This function takes x, y, a, and b as inputs and computes the result of this equation
            // If the result is <= 1, the point at x,y is inside the ellipse

            try
            {
                return Math.Pow(pointX, 2d) / Math.Pow(xTol, 2d) + Math.Pow(pointY, 2d) / Math.Pow(yTol, 2d) <= 1d;
            }
            catch
            {
                // Error; return false
                return false;
            }
        }

        /// <summary>
        /// Update the progress
        /// </summary>
        /// <param name="progressPercent">Value between 0 and 100</param>
        private void UpdateProgress(float progressPercent)
        {
            ProgressPct = progressPercent;
            ProgressChanged?.Invoke(ProgressDescription, ProgressPct);
        }

        /// <summary>
        /// Update the progress
        /// </summary>
        /// <param name="description">Progress description</param>
        /// <param name="progressPercent">Value between 0 and 100</param>
        private void UpdateProgress(string description, float progressPercent)
        {
            ProgressDescription = description;
            ProgressPct = progressPercent;
            ProgressChanged?.Invoke(ProgressDescription, ProgressPct);
        }
    }
}