﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Readability", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses are correct", Scope = "member", Target = "~M:ProteinDigestionSimulator.InSilicoDigest.DigestSequence(System.String,System.Collections.Generic.List{ProteinDigestionSimulator.PeptideSequenceWithNET}@,System.Boolean,System.String)~System.Int32")]
[assembly: SuppressMessage("Readability", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses are correct", Scope = "member", Target = "~M:ProteinDigestionSimulator.PeptideSequence.CheckSequenceAgainstCleavageRule(System.String,ProteinDigestionSimulator.CleavageRule,System.Int32@,System.String,System.Char,System.Boolean)~System.Boolean")]
[assembly: SuppressMessage("Readability", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses are correct", Scope = "member", Target = "~M:ProteinDigestionSimulator.PeptideSequence.ConvertAminoAcidSequenceSymbols(System.String,System.Boolean,System.Boolean,System.Boolean)~System.String")]
[assembly: SuppressMessage("Readability", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses not needed", Scope = "member", Target = "~M:ProteinDigestionSimulator.DigestionSimulator.GenerateUniquenessStats(System.String,System.String,System.String)~System.Boolean")]
[assembly: SuppressMessage("Readability", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses not needed", Scope = "member", Target = "~M:ProteinDigestionSimulator.DigestionSimulator.InitializeBinnedStats(ProteinDigestionSimulator.DigestionSimulator.BinnedPeptideCountStats,System.Int32)")]
[assembly: SuppressMessage("Readability", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses not needed", Scope = "member", Target = "~M:ProteinDigestionSimulator.DigestionSimulator.LoadPeptidesFromDelimitedFile(System.String)~System.Boolean")]
[assembly: SuppressMessage("Readability", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses not needed", Scope = "member", Target = "~M:ProteinDigestionSimulator.PeakMatching.PeakMatcher.ComputeSLiCScores(ProteinDigestionSimulator.PeakMatching.FeatureInfo,ProteinDigestionSimulator.PeakMatching.PMFeatureMatchResults,System.Collections.Generic.List{ProteinDigestionSimulator.PeakMatching.PeakMatchingRawMatches},ProteinDigestionSimulator.PeakMatching.PMComparisonFeatureInfo,ProteinDigestionSimulator.PeakMatching.SearchThresholds,ProteinDigestionSimulator.PeakMatching.SearchThresholds.SearchTolerances)")]
[assembly: SuppressMessage("Readability", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses not needed", Scope = "member", Target = "~M:ProteinDigestionSimulator.PeakMatching.PeakMatcher.TestPointInEllipse(System.Double,System.Double,System.Double,System.Double)~System.Boolean")]
[assembly: SuppressMessage("Readability", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses not needed", Scope = "member", Target = "~M:ProteinDigestionSimulator.ProteinFileParser.ParseProteinFile(System.String,System.String,System.String)~System.Boolean")]
[assembly: SuppressMessage("Simplification", "RCS1179:Unnecessary assignment.", Justification = "Leave as-is for readability", Scope = "member", Target = "~M:ProteinDigestionSimulator.ProteinFileParser.GetErrorMessage~System.String")]
[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Allowed name", Scope = "member", Target = "~P:ProteinDigestionSimulator.ProteinFileParser.ProteinInfo.pI")]
