﻿using System.Collections.Generic;
using System.Text;

namespace ProteinDigestionSimulator
{
    public class CleavageRule
    {
        /// <summary>
        /// Cleavage rule description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Residues to cleave after (or before if ReversedCleavageDirection is true)
        /// Will not cleave after the residue if it is followed by any of the ExceptionResidues (or preceded by if ReversedCleavageDirection is true)
        /// </summary>
        public string CleavageResidues { get; }

        /// <summary>
        /// Adjacent residue(s) that prevent cleavage
        /// </summary>
        public string ExceptionResidues { get; }

        /// <summary>
        /// When false, cleave after the CleavageResidues, unless followed by the ExceptionResidues, e.g. Trypsin, CNBr, GluC
        /// When true, cleave before the CleavageResidues, unless preceded by the ExceptionResidues, e.g. Asp-N
        /// </summary>
        public bool ReversedCleavageDirection { get; }

        /// <summary>
        /// When true, allow for either end of a peptide to match the cleavage rules
        /// When false, both ends must match the cleavage rules
        /// </summary>
        public bool AllowPartialCleavage { get; }

        /// <summary>
        /// Additional cleavage rules to also consider when checking for cleavage points in a peptide
        /// </summary>
        public List<CleavageRule> AdditionalCleavageRules { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ruleDescription"></param>
        /// <param name="cleavageResidueList"></param>
        /// <param name="exceptionResidueList"></param>
        /// <param name="reversedCleavage"></param>
        /// <param name="allowPartial"></param>
        /// <param name="additionalRules"></param>
        public CleavageRule(
            string ruleDescription,
            string cleavageResidueList,
            string exceptionResidueList,
            bool reversedCleavage,
            bool allowPartial = false,
            IReadOnlyCollection<CleavageRule> additionalRules = null)
        {
            Description = ruleDescription;
            CleavageResidues = cleavageResidueList;
            ExceptionResidues = exceptionResidueList;
            ReversedCleavageDirection = reversedCleavage;
            AllowPartialCleavage = allowPartial;

            AdditionalCleavageRules = new List<CleavageRule>();

            if (additionalRules == null || additionalRules.Count == 0)
            {
                return;
            }

            AdditionalCleavageRules.AddRange(additionalRules);
        }

        /// <summary>
        /// Obtain a string describing the cleavage residues and exception residues
        /// </summary>
        public string GetDetailedRuleDescription(bool includeForwardCleavageDirectionWord = false)
        {
            var detailedDescription = new StringBuilder();

            if (ReversedCleavageDirection)
            {
                detailedDescription.AppendFormat("before {0}", CleavageResidues);

                if (ExceptionResidues.Length > 0)
                {
                    detailedDescription.AppendFormat(" not preceded by {0}", ExceptionResidues);
                }
            }
            else
            {
                if (includeForwardCleavageDirectionWord)
                {
                    detailedDescription.Append("after ");
                }

                detailedDescription.Append(CleavageResidues);

                if (ExceptionResidues.Length > 0)
                {
                    detailedDescription.AppendFormat(" not {0}", ExceptionResidues);
                }
            }

            foreach (var additionalRule in AdditionalCleavageRules)
            {
                detailedDescription.AppendFormat("; or {0}", additionalRule.GetDetailedRuleDescription(true));
            }

            return detailedDescription.ToString();
        }

        public override string ToString()
        {
            return Description;
        }
    }
}
