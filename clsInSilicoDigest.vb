Option Strict On

Imports System.Runtime.InteropServices

''' <summary>
''' This class can be used to perform an in-silico digest of an amino acid sequence
''' Utilizes the PeptideInfoClass class
''' </summary>
Public Class clsInSilicoDigest

    Public Sub New()
        mPeptideSequence = New PeptideSequenceClass
        mPeptideSequence.ElementMode = PeptideSequenceClass.ElementModeConstants.IsotopicMass

        InitializeCleavageRules()
        InitializepICalculator()
    End Sub

#Region "Constants and Enums"

    ' Note: Good list of enzymes is at https://web.expasy.org/peptide_cutter/peptidecutter_enzymes.html
    '                              and https://web.expasy.org/peptide_mass/peptide-mass-doc.html

    Public Enum CleavageRuleConstants
        NoRule = 0
        ConventionalTrypsin = 1
        TrypsinWithoutProlineException = 2
        EricPartialTrypsin = 3
        ' ReSharper disable once IdentifierTypo
        TrypsinPlusFVLEY = 4
        KROneEnd = 5
        TerminiiOnly = 6
        Chymotrypsin = 7
        ChymotrypsinAndTrypsin = 8
        GluC = 9
        CyanBr = 10                     ' Aka CNBr
        LysC = 11
        GluC_EOnly = 12
        ArgC = 13
        AspN = 14
        ' ReSharper disable once IdentifierTypo
        ProteinaseK = 15
        PepsinA = 16
        PepsinB = 17
        PepsinC = 18
        PepsinD = 19
        AceticAcidD = 20
    End Enum

#End Region

#Region "Structures"

    Private Structure udtCleavageRulesType
        Public Description As String
        Public CleavageResidues As String
        Public ExceptionResidues As String

        ' If ReversedCleavageDirection= False, then cleave after the CleavageResidues, unless followed by the ExceptionResidues, e.g. Trypsin, CNBr, GluC
        ' If ReversedCleavageDirection= True, then cleave before the CleavageResidues, unless preceded by the ExceptionResidues, e.g. Asp-N
        Public ReversedCleavageDirection As Boolean

        Public AllowPartialCleavage As Boolean
    End Structure


#End Region

#Region "Classwide Variables"

    Private ReadOnly mCleavageRules As Dictionary(Of CleavageRuleConstants, udtCleavageRulesType) = New Dictionary(Of CleavageRuleConstants, udtCleavageRulesType)

    ' General purpose object for computing mass and calling cleavage and digestion functions
    Private mPeptideSequence As PeptideSequenceClass

    Private mpICalculator As clspICalculation

    Public Event ErrorEvent(message As String)

    Public Event ProgressReset()
    Public Event ProgressChanged(taskDescription As String, percentComplete As Single)     ' PercentComplete ranges from 0 to 100, but can contain decimal percentage values
    Public Event ProgressComplete()

    Protected mProgressStepDescription As String
    Protected mProgressPercentComplete As Single        ' Ranges from 0 to 100, but can contain decimal percentage values

#End Region

#Region "Processing Options Interface Functions"
    Public ReadOnly Property CleavageRuleCount As Integer
        Get
            Return mCleavageRules.Count
        End Get
    End Property

    Public Property ElementMassMode As PeptideSequenceClass.ElementModeConstants
        Get
            If mPeptideSequence Is Nothing Then
                Return PeptideSequenceClass.ElementModeConstants.IsotopicMass
            Else
                Return mPeptideSequence.ElementMode
            End If
        End Get
        Set
            If mPeptideSequence Is Nothing Then
                mPeptideSequence = New PeptideSequenceClass
            End If
            mPeptideSequence.ElementMode = Value
        End Set
    End Property

    Public Overridable ReadOnly Property ProgressStepDescription As String
        Get
            Return mProgressStepDescription
        End Get
    End Property

    ' ProgressPercentComplete ranges from 0 to 100, but can contain decimal percentage values
    Public ReadOnly Property ProgressPercentComplete As Single
        Get
            Return CType(Math.Round(mProgressPercentComplete, 2), Single)
        End Get
    End Property

#End Region

    Private Sub AddCleavageRule(
        ruleId As CleavageRuleConstants,
        description As String,
        cleavageResidues As String,
        exceptionResidues As String,
        reversedCleavageDirection As Boolean,
        Optional allowPartialCleavage As Boolean = False)

        Dim cleavageRule = New udtCleavageRulesType() With {
                .Description = description,
                .CleavageResidues = cleavageResidues,
                .ExceptionResidues = exceptionResidues,
                .ReversedCleavageDirection = reversedCleavageDirection,
                .AllowPartialCleavage = allowPartialCleavage
                }

        mCleavageRules.Add(ruleId, cleavageRule)
    End Sub

    Public Function CheckSequenceAgainstCleavageRule(sequence As String, ruleId As CleavageRuleConstants, Optional ByRef ruleMatchCount As Integer = 0) As Boolean
        ' Checks sequence against the rule given by ruleId
        ' See sub InitializeCleavageRules for a list of the rules
        ' Returns True if valid, False if invalid
        ' ruleMatchCount returns 0, 1, or 2:  0 if neither end matches, 1 if one end matches, 2 if both ends match
        '
        ' In order to check for Exception residues, sequence must be in the form "R.ABCDEFGK.L" so that the residue following the final residue of the fragment can be examined

        Dim ruleMatch As Boolean

        ruleMatchCount = 0

        Dim cleavageRule As udtCleavageRulesType = Nothing
        If mCleavageRules.TryGetValue(ruleId, cleavageRule) Then
            If ruleId = CleavageRuleConstants.NoRule Then
                ' No cleavage rule; no point in checking
                ruleMatch = True
                ruleMatchCount = 2
            Else
                ruleMatch = mPeptideSequence.CheckSequenceAgainstCleavageRule(
                                                            sequence,
                                                            cleavageRule.CleavageResidues,
                                                            cleavageRule.ExceptionResidues,
                                                            cleavageRule.ReversedCleavageDirection,
                                                            cleavageRule.AllowPartialCleavage,
                                                            , , , ruleMatchCount)
            End If
        Else
            ' No rule selected; assume True
            ruleMatchCount = 2
            ruleMatch = True
        End If

        Return ruleMatch

    End Function

    Public Function ComputeSequenceMass(sequence As String, Optional includeXResiduesInMass As Boolean = True) As Double
        ' Note that sequence must be in 1-letter notation, and will automatically be converted to uppercase

        Try
            If includeXResiduesInMass Then
                mPeptideSequence.SetSequence(sequence.ToUpper())
            Else
                ' Exclude X residues from sequence when calling .SetSequence
                mPeptideSequence.SetSequence(sequence.ToUpper().Replace("X", ""))
            End If
            Return mPeptideSequence.Mass
        Catch ex As Exception
            ReportError("ComputeSequenceMass", ex)
            Return 0
        End Try

    End Function

    Public Function CountTrypticsInSequence(sequence As String) As Integer

        Try

            Dim trypticCount = 0
            Dim startSearchLoc = 1

            If sequence.Length > 0 Then
                Do
                    Dim returnResidueStart As Integer, returnResidueEnd As Integer

                    Dim fragment = mPeptideSequence.GetTrypticPeptideNext(sequence, startSearchLoc, returnResidueStart, returnResidueEnd)
                    If fragment.Length > 0 Then
                        trypticCount += 1
                        startSearchLoc = returnResidueEnd + 1
                    Else
                        Exit Do
                    End If
                Loop
            End If

            Return trypticCount

        Catch ex As Exception
            ReportError("CountTrypticsInSequence", ex)
            Return 0
        End Try

    End Function

    Public Function DigestSequence(proteinSequence As String,
                                   <Out> ByRef peptideFragments As List(Of PeptideInfoClass),
                                   digestionOptions As DigestionOptionsClass,
                                   filterByIsoelectricPoint As Boolean) As Integer

        Return DigestSequence(proteinSequence, peptideFragments, digestionOptions, filterByIsoelectricPoint, "")

    End Function

    ''' <summary>
    ''' Digests proteinSequence using the sequence rule given by digestionOptions.CleavageRuleID
    ''' If removeDuplicateSequences = True, only returns the first occurrence of each unique sequence
    ''' </summary>
    ''' <param name="proteinSequence"></param>
    ''' <param name="peptideFragments"></param>
    ''' <param name="digestionOptions"></param>
    ''' <param name="filterByIsoelectricPoint"></param>
    ''' <param name="proteinName"></param>
    ''' <returns>The number of peptides in peptideFragments</returns>
    Public Function DigestSequence(proteinSequence As String,
                                   <Out> ByRef peptideFragments As List(Of PeptideInfoClass),
                                   digestionOptions As DigestionOptionsClass,
                                   filterByIsoelectricPoint As Boolean,
                                   proteinName As String) As Integer

        Dim fragmentsUniqueList As New SortedSet(Of String)

        Dim trypticFragCache() As String                      ' 0-based array
        Dim trypticFragStartLocations() As Integer            ' 0-based array, parallel to trypticFragmentCache()
        Dim trypticFragEndLocations() As Integer              ' 0-based array, parallel to trypticFragmentCache()

        peptideFragments = New List(Of PeptideInfoClass)()

        If String.IsNullOrWhiteSpace(proteinSequence) Then
            Return 0
        End If

        Dim proteinSequenceLength As Integer = proteinSequence.Length

        Try

            Dim ruleResidues = GetCleavageRuleResiduesSymbols(digestionOptions.CleavageRuleID)
            Dim exceptionSuffixResidues = GetCleavageExceptionSuffixResidues(digestionOptions.CleavageRuleID)
            Dim reversedCleavageDirection = GetCleavageIsReversedDirection(digestionOptions.CleavageRuleID)

            ' We initially count the number of tryptic peptides in the sequence (regardless of the cleavage rule)
            ' ReSharper disable once UnusedVariable
            Dim trypticFragmentCount = CountTrypticsInSequence(proteinSequence)

            ReDim trypticFragCache(9)
            ReDim trypticFragEndLocations(9)
            ReDim trypticFragStartLocations(9)

            Dim trypticFragCacheCount = 0
            Dim searchStartLoc = 1

            ' Populate trypticFragCache()
            '
            ' Using the GetTrypticPeptideNext function to retrieve the sequence for each tryptic peptide
            '   is faster than using the GetTrypticPeptideByFragmentNumber function
            Do
                Dim residueStartLoc As Integer
                Dim residueEndLoc As Integer

                Dim peptideSequence = mPeptideSequence.GetTrypticPeptideNext(proteinSequence, searchStartLoc, residueStartLoc, residueEndLoc, ruleResidues, exceptionSuffixResidues, reversedCleavageDirection)
                If peptideSequence.Length > 0 Then

                    trypticFragCache(trypticFragCacheCount) = peptideSequence
                    trypticFragStartLocations(trypticFragCacheCount) = residueStartLoc
                    trypticFragEndLocations(trypticFragCacheCount) = residueEndLoc
                    trypticFragCacheCount += 1

                    If trypticFragCacheCount >= trypticFragCache.Length Then
                        ReDim Preserve trypticFragCache(trypticFragCache.Length + 10)
                        ReDim Preserve trypticFragEndLocations(trypticFragCache.Length - 1)
                        ReDim Preserve trypticFragStartLocations(trypticFragCache.Length - 1)
                    End If

                    searchStartLoc = residueEndLoc + 1
                Else
                    Exit Do
                End If
            Loop

            ResetProgress("Digesting protein " & proteinName)

            For trypticIndex = 0 To trypticFragCacheCount - 1
                Dim peptideSequenceBase = String.Empty
                Dim peptideSequence = String.Empty
                Dim residueStartLoc = trypticFragStartLocations(trypticIndex)
                Dim residueEndLoc As Integer

                For index = 0 To digestionOptions.MaxMissedCleavages
                    If trypticIndex + index >= trypticFragCacheCount Then
                        Exit For
                    End If

                    If digestionOptions.CleavageRuleID = CleavageRuleConstants.KROneEnd Then
                        ' Partially tryptic cleavage rule: Add all partially tryptic fragments
                        Dim residueLengthStart As Integer
                        If index = 0 Then
                            residueLengthStart = digestionOptions.MinFragmentResidueCount
                            If residueLengthStart < 1 Then residueLengthStart = 1
                        Else
                            residueLengthStart = 1
                        End If

                        For residueLength = residueLengthStart To trypticFragCache(trypticIndex + index).Length

                            If index > 0 Then
                                residueEndLoc = trypticFragEndLocations(trypticIndex + index - 1) + residueLength
                            Else
                                residueEndLoc = residueStartLoc + residueLength - 1
                            End If

                            peptideSequence = peptideSequenceBase & trypticFragCache(trypticIndex + index).Substring(0, residueLength)

                            If peptideSequence.Length >= digestionOptions.MinFragmentResidueCount Then
                                PossiblyAddPeptide(peptideSequence, trypticIndex, index, residueStartLoc, residueEndLoc, proteinSequence, proteinSequenceLength, fragmentsUniqueList, peptideFragments, digestionOptions, filterByIsoelectricPoint)
                            End If
                        Next residueLength
                    Else
                        ' Normal cleavage rule
                        residueEndLoc = trypticFragEndLocations(trypticIndex + index)

                        peptideSequence = peptideSequence & trypticFragCache(trypticIndex + index)
                        If peptideSequence.Length >= digestionOptions.MinFragmentResidueCount Then
                            PossiblyAddPeptide(peptideSequence, trypticIndex, index, residueStartLoc, residueEndLoc, proteinSequence, proteinSequenceLength, fragmentsUniqueList, peptideFragments, digestionOptions, filterByIsoelectricPoint)
                        End If
                    End If

                    peptideSequenceBase = peptideSequenceBase & trypticFragCache(trypticIndex + index)
                Next index

                If digestionOptions.CleavageRuleID = CleavageRuleConstants.KROneEnd Then
                    UpdateProgress(CSng(trypticIndex / (trypticFragCacheCount * 2) * 100.0))
                End If
            Next trypticIndex

            If digestionOptions.CleavageRuleID = CleavageRuleConstants.KROneEnd Then
                ' Partially tryptic cleavage rule: Add all partially tryptic fragments, working from the end toward the front
                For trypticIndex = trypticFragCacheCount - 1 To 0 Step -1
                    Dim peptideSequenceBase = String.Empty

                    Dim residueEndLoc = trypticFragEndLocations(trypticIndex)

                    For index = 0 To digestionOptions.MaxMissedCleavages
                        If trypticIndex - index < 0 Then
                            Exit For
                        End If

                        Dim residueLengthStart As Integer

                        If index = 0 Then
                            residueLengthStart = digestionOptions.MinFragmentResidueCount
                        Else
                            residueLengthStart = 1
                        End If

                        ' We can limit the following for loop to the peptide length - 1 since those peptides using the full peptide will have already been added above
                        For residueLength = residueLengthStart To trypticFragCache(trypticIndex - index).Length - 1
                            Dim residueStartLoc As Integer

                            If index > 0 Then
                                residueStartLoc = trypticFragStartLocations(trypticIndex - index + 1) - residueLength
                            Else
                                residueStartLoc = residueEndLoc - (residueLength - 1)
                            End If

                            ' Grab characters from the end of trypticFragCache()
                            Dim peptideSequence = trypticFragCache(trypticIndex - index).Substring(trypticFragCache(trypticIndex - index).Length - residueLength, residueLength) & peptideSequenceBase

                            If peptideSequence.Length >= digestionOptions.MinFragmentResidueCount Then
                                PossiblyAddPeptide(peptideSequence, trypticIndex, index, residueStartLoc, residueEndLoc, proteinSequence, proteinSequenceLength, fragmentsUniqueList, peptideFragments, digestionOptions, filterByIsoelectricPoint)
                            End If

                        Next residueLength

                        peptideSequenceBase = trypticFragCache(trypticIndex - index) & peptideSequenceBase
                    Next index

                    UpdateProgress(CSng((trypticFragCacheCount * 2 - trypticIndex) / (trypticFragCacheCount * 2) * 100))

                Next trypticIndex

            End If

            Return peptideFragments.Count

        Catch ex As Exception
            ReportError("DigestSequence", ex)
            Return peptideFragments.Count
        End Try

    End Function

    Public Function GetCleavageAllowPartialCleavage(ruleId As CleavageRuleConstants) As Boolean
        Dim cleavageRule As udtCleavageRulesType = Nothing
        If mCleavageRules.TryGetValue(ruleId, cleavageRule) Then
            Return cleavageRule.AllowPartialCleavage
        Else
            Return False
        End If
    End Function

    Public Function GetCleavageIsReversedDirection(ruleId As CleavageRuleConstants) As Boolean
        Dim cleavageRule As udtCleavageRulesType = Nothing
        If mCleavageRules.TryGetValue(ruleId, cleavageRule) Then
            Return cleavageRule.ReversedCleavageDirection
        Else
            Return False
        End If
    End Function

    Public Function GetCleavageExceptionSuffixResidues(ruleId As CleavageRuleConstants) As String
        Dim cleavageRule As udtCleavageRulesType = Nothing
        If mCleavageRules.TryGetValue(ruleId, cleavageRule) Then
            Return cleavageRule.ExceptionResidues
        Else
            Return String.Empty
        End If
    End Function

    Public Function GetCleavageRuleName(ruleId As CleavageRuleConstants) As String
        Dim cleavageRule As udtCleavageRulesType = Nothing
        If mCleavageRules.TryGetValue(ruleId, cleavageRule) Then
            Return cleavageRule.Description
        Else
            Return String.Empty
        End If
    End Function

    Public Function GetCleavageRuleResiduesDescription(ruleId As CleavageRuleConstants) As String
        Dim description As String

        Dim cleavageRule As udtCleavageRulesType = Nothing
        If mCleavageRules.TryGetValue(ruleId, cleavageRule) Then
            If cleavageRule.ReversedCleavageDirection Then
                description = "Before " & cleavageRule.CleavageResidues
                If cleavageRule.ExceptionResidues.Length > 0 Then
                    description &= " not preceded by " & cleavageRule.ExceptionResidues
                End If
            Else
                description = cleavageRule.CleavageResidues
                If cleavageRule.ExceptionResidues.Length > 0 Then
                    description &= " not " & cleavageRule.ExceptionResidues
                End If
            End If

            Return description
        Else
            Return String.Empty
        End If
    End Function

    Public Function GetCleavageRuleResiduesSymbols(ruleId As CleavageRuleConstants) As String
        Dim cleavageRule As udtCleavageRulesType = Nothing
        If mCleavageRules.TryGetValue(ruleId, cleavageRule) Then
            Return cleavageRule.CleavageResidues
        Else
            Return String.Empty
        End If
    End Function

    Private Sub InitializeCleavageRules()

        ' Useful site for cleavage rule info is https://web.expasy.org/peptide_mass/peptide-mass-doc.html

        mCleavageRules.Clear()

        ' ReSharper disable StringLiteralTypo
        AddCleavageRule(CleavageRuleConstants.NoRule,
                        "No cleavage rule",
                        "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                        String.Empty,
                        False)


        AddCleavageRule(CleavageRuleConstants.ConventionalTrypsin,
            "Fully Tryptic",
            "KR",
            "P",
            False)

        AddCleavageRule(CleavageRuleConstants.TrypsinWithoutProlineException,
            "Fully Tryptic (no Proline Rule)",
            "KR",
            String.Empty,
            False)

        ' Allows partial cleavage
        AddCleavageRule(CleavageRuleConstants.EricPartialTrypsin,
            "Eric's Partial Trypsin",
            "KRFYVEL",
            String.Empty,
            False,
            True)

        AddCleavageRule(CleavageRuleConstants.TrypsinPlusFVLEY,
            "Trypsin Plus FVLEY",
            "KRFYVEL",
            "",
            False)

        ' Allows partial cleavage
        AddCleavageRule(CleavageRuleConstants.KROneEnd,
            "Half (Partial) Trypsin ",
            "KR",
            "P",
            False,
            True)

        AddCleavageRule(CleavageRuleConstants.TerminiiOnly,
            "Peptide Database; terminii only",
            "-",
            String.Empty,
            False)

        AddCleavageRule(CleavageRuleConstants.Chymotrypsin,
            "Chymotrypsin",
            "FWYL",
            String.Empty,
            False)

        AddCleavageRule(CleavageRuleConstants.ChymotrypsinAndTrypsin,
            "Chymotrypsin + Trypsin",
            "FWYLKR",
            String.Empty,
            False)

        AddCleavageRule(CleavageRuleConstants.GluC,
            "Glu-C",
            "ED",
            String.Empty,
            False)

        AddCleavageRule(CleavageRuleConstants.CyanBr,
            "CyanBr",
            "M",
            String.Empty,
            False)

        AddCleavageRule(CleavageRuleConstants.LysC,
            "Lys-C",
            "K",
            String.Empty,
            False)

        AddCleavageRule(CleavageRuleConstants.GluC_EOnly,
            "Glu-C, just Glu",
            "E",
            String.Empty,
            False)

        AddCleavageRule(CleavageRuleConstants.ArgC,
            "Arg-C",
            "R",
            String.Empty,
            False)

        AddCleavageRule(CleavageRuleConstants.AspN,
            "Asp-N",
            "D",
            String.Empty,
            True)

        AddCleavageRule(CleavageRuleConstants.ProteinaseK,
            "Proteinase K",
            "AEFILTVWY",
            String.Empty,
            False)

        AddCleavageRule(CleavageRuleConstants.PepsinA,
            "PepsinA",
            "FLIWY",
            "P",
            False)

        AddCleavageRule(CleavageRuleConstants.PepsinB,
            "PepsinB",
            "FLIWY",
            "PVAG",
            False)

        AddCleavageRule(CleavageRuleConstants.PepsinC,
            "PepsinC",
            "FLWYA",
            "P",
            False)

        AddCleavageRule(CleavageRuleConstants.PepsinD,
            "PepsinD",
            "FLWYAEQ",
            "",
            False)

        AddCleavageRule(CleavageRuleConstants.AceticAcidD,
            "Acetic Acid Hydrolysis",
            "D",
            String.Empty,
            False)

        ' ReSharper restore StringLiteralTypo

    End Sub

    Public Sub InitializepICalculator()
        Me.InitializepICalculator(New clspICalculation)
    End Sub

    Public Sub InitializepICalculator(ByRef pICalculator As clspICalculation)
        If Not mpICalculator Is Nothing Then
            If mpICalculator Is pICalculator Then
                ' Classes are the same instance of the object; no need to update anything
                Exit Sub
            Else
                mpICalculator = Nothing
            End If
        End If

        mpICalculator = pICalculator
    End Sub

    Public Sub InitializepICalculator(
        eHydrophobicityType As clspICalculation.eHydrophobicityTypeConstants,
        reportMaximumpI As Boolean,
        sequenceWidthToExamineForMaximumpI As Integer)

        If mpICalculator Is Nothing Then
            mpICalculator = New clspICalculation()
        End If

        mpICalculator.HydrophobicityType = eHydrophobicityType
        mpICalculator.ReportMaximumpI = reportMaximumpI
        mpICalculator.SequenceWidthToExamineForMaximumpI = sequenceWidthToExamineForMaximumpI
    End Sub

    Private Sub PossiblyAddPeptide(
        peptideSequence As String,
        trypticIndex As Integer,
        missedCleavageCount As Integer,
        residueStartLoc As Integer,
        residueEndLoc As Integer,
        ByRef proteinSequence As String,
        proteinSequenceLength As Integer,
        fragmentsUniqueList As ISet(Of String),
        peptideFragments As ICollection(Of PeptideInfoClass),
        digestionOptions As DigestionOptionsClass,
        filterByIsoelectricPoint As Boolean)

        ' Note: proteinSequence is passed ByRef for speed purposes since passing a reference of a large string is easier than passing it ByVal
        '       It is not modified by this function

        Dim addFragment As Boolean
        Dim prefix As String, suffix As String

        Dim isoelectricPoint As Single

        addFragment = True
        If digestionOptions.RemoveDuplicateSequences Then
            If fragmentsUniqueList.Contains(peptideSequence) Then
                addFragment = False
            Else
                fragmentsUniqueList.Add(peptideSequence)
            End If
        End If

        If addFragment AndAlso digestionOptions.AminoAcidResidueFilterChars.Length > 0 Then
            If peptideSequence.IndexOfAny(digestionOptions.AminoAcidResidueFilterChars) < 0 Then
                addFragment = False
            End If
        End If

        If Not addFragment Then Return

        Dim peptideFragment = New PeptideInfoClass With {
            .AutoComputeNET = False,
            .CysTreatmentMode = digestionOptions.CysTreatmentMode
        }

        peptideFragment.SequenceOneLetter = peptideSequence

        If peptideFragment.Mass < digestionOptions.MinFragmentMass OrElse
           peptideFragment.Mass > digestionOptions.MaxFragmentMass Then
            Return
        End If

        ' Possibly compute the isoelectric point for the peptide
        If filterByIsoelectricPoint Then
            isoelectricPoint = mpICalculator.CalculateSequencepI(peptideSequence)
        End If

        If filterByIsoelectricPoint AndAlso (
                   isoelectricPoint < digestionOptions.MinIsoelectricPoint OrElse
                   isoelectricPoint > digestionOptions.MaxIsoelectricPoint) Then
            Return
        End If

        ' We can now compute the NET value for the peptide
        peptideFragment.UpdateNET()

        If digestionOptions.IncludePrefixAndSuffixResidues Then
            If residueStartLoc <= 1 Then
                prefix = PeptideInfoClass.PROTEIN_TERMINUS_SYMBOL
            Else
                prefix = proteinSequence.Substring(residueStartLoc - 2, 1)
            End If

            If residueEndLoc >= proteinSequenceLength Then
                suffix = PeptideInfoClass.PROTEIN_TERMINUS_SYMBOL
            Else
                suffix = proteinSequence.Substring(residueEndLoc, 1)
            End If

            peptideFragment.PrefixResidue = prefix
            peptideFragment.SuffixResidue = suffix
        Else
            peptideFragment.PrefixResidue = PeptideInfoClass.PROTEIN_TERMINUS_SYMBOL
            peptideFragment.SuffixResidue = PeptideInfoClass.PROTEIN_TERMINUS_SYMBOL
        End If


        If digestionOptions.CleavageRuleID = CleavageRuleConstants.ConventionalTrypsin OrElse
               digestionOptions.CleavageRuleID = CleavageRuleConstants.TrypsinWithoutProlineException Then
            peptideFragment.PeptideName = "t" & (trypticIndex + 1).ToString() & "." & (missedCleavageCount + 1).ToString()
        Else
            peptideFragment.PeptideName = residueStartLoc.ToString() & "." & residueEndLoc.ToString()
        End If

        If addFragment Then
            peptideFragments.Add(peptideFragment)
        End If
    End Sub

    Private Sub ReportError(functionName As String, ex As Exception)
        Try
            Dim errorMessage As String

            If Not functionName Is Nothing AndAlso functionName.Length > 0 Then
                errorMessage = "Error in " & functionName & ": " & ex.Message
            Else
                errorMessage = "Error: " & ex.Message
            End If

            Console.WriteLine(errorMessage)

            RaiseEvent ErrorEvent(errorMessage)

        Catch exNew As Exception
            ' Ignore errors here
        End Try
    End Sub

    Protected Sub ResetProgress()
        RaiseEvent ProgressReset()
    End Sub

    Protected Sub ResetProgress(description As String)
        UpdateProgress(description, 0)
        RaiseEvent ProgressReset()
    End Sub

    Protected Sub UpdateProgress(description As String)
        UpdateProgress(description, mProgressPercentComplete)
    End Sub

    Protected Sub UpdateProgress(percentComplete As Single)
        UpdateProgress(Me.ProgressStepDescription, percentComplete)
    End Sub

    Protected Sub UpdateProgress(description As String, percentComplete As Single)

        mProgressStepDescription = String.Copy(description)
        If percentComplete < 0 Then
            percentComplete = 0
        ElseIf percentComplete > 100 Then
            percentComplete = 100
        End If
        mProgressPercentComplete = percentComplete

        RaiseEvent ProgressChanged(description, ProgressPercentComplete)

    End Sub

#Region "Peptide Info class"

    Public Class PeptideInfoClass
        Inherits PeptideSequenceClass

        ' Adds NET computation to the PeptideSequenceClass

        Public Sub New()

            If NETPredictor Is Nothing Then
                NETPredictor = New NETPrediction.ElutionTimePredictionKangas
            End If

            ' Disable mAutoComputeNET for now so that the call to SetSequence() below doesn't auto-call UpdateNET
            mAutoComputeNET = False

            PeptideName = String.Empty
            SetSequence(String.Empty)
            mNET = 0
            mPrefixResidue = PROTEIN_TERMINUS_SYMBOL
            mSuffixResidue = PROTEIN_TERMINUS_SYMBOL

            ' Re-enable mAutoComputeNET
            mAutoComputeNET = True
        End Sub

#Region "Constants and Enums"
        Public Const PROTEIN_TERMINUS_SYMBOL As String = "-"
#End Region

#Region "Structures"
#End Region

#Region "Classwide Variables"

        ' The following is declared Shared so that it is only initialized once per program execution
        ' All objects of type PeptideInfoClass will use the same instance of this object
        Private Shared NETPredictor As NETPrediction.iPeptideElutionTime


        Private mAutoComputeNET As Boolean      ' Set to False to skip computation of NET when Sequence changes; useful for speeding up things a little

        Private mNET As Single
        Private mPrefixResidue As String
        Private mSuffixResidue As String

#End Region

#Region "Processing Options Interface Functions"
        Public Property AutoComputeNET As Boolean
            Get
                Return mAutoComputeNET
            End Get
            Set
                mAutoComputeNET = Value
            End Set
        End Property

        Public Property PeptideName As String

        Public ReadOnly Property NET As Single
            Get
                Return mNET
            End Get
        End Property

        Public Property PrefixResidue As String
            Get
                Return mPrefixResidue
            End Get
            Set
                If Not Value Is Nothing AndAlso Value.Length > 0 Then
                    mPrefixResidue = Value.Chars(0)
                Else
                    mPrefixResidue = PROTEIN_TERMINUS_SYMBOL
                End If
            End Set
        End Property

        Public Property SequenceOneLetter As String
            Get
                Return GetSequence(False)
            End Get
            Set
                SetSequence(Value, NTerminusGroupConstants.Hydrogen, CTerminusGroupConstants.Hydroxyl, False)
            End Set
        End Property

        Public ReadOnly Property SequenceWithPrefixAndSuffix As String
            Get
                Return mPrefixResidue & "." & SequenceOneLetter & "." & mSuffixResidue
            End Get
        End Property

        Public Property SuffixResidue As String
            Get
                Return mSuffixResidue
            End Get
            Set
                If Not Value Is Nothing AndAlso Value.Length > 0 Then
                    mSuffixResidue = Value.Chars(0)
                Else
                    mSuffixResidue = PROTEIN_TERMINUS_SYMBOL
                End If
            End Set
        End Property
#End Region

        Public Overrides Function SetSequence(sequence As String, Optional eNTerminus As NTerminusGroupConstants = NTerminusGroupConstants.Hydrogen, Optional eCTerminus As CTerminusGroupConstants = CTerminusGroupConstants.Hydroxyl, Optional is3LetterCode As Boolean = False, Optional bln1LetterCheckForPrefixAndSuffixResidues As Boolean = True, Optional bln3LetterCheckForPrefixHandSuffixOH As Boolean = True) As Integer
            Dim returnVal As Integer

            returnVal = MyBase.SetSequence(sequence, eNTerminus, eCTerminus, is3LetterCode, bln1LetterCheckForPrefixAndSuffixResidues, bln3LetterCheckForPrefixHandSuffixOH)
            If mAutoComputeNET Then UpdateNET()

            Return returnVal
        End Function

        Public Overrides Sub SetSequenceOneLetterCharactersOnly(sequenceNoPrefixOrSuffix As String)
            MyBase.SetSequenceOneLetterCharactersOnly(sequenceNoPrefixOrSuffix)
            If mAutoComputeNET Then UpdateNET()
        End Sub

        Public Sub UpdateNET()
            Try
                mNET = NETPredictor.GetElutionTime(GetSequence(False))
            Catch ex As Exception
                mNET = 0
            End Try
        End Sub

    End Class
#End Region

#Region "Digestion options class"

    Public Class DigestionOptionsClass

        Public Sub New()
            mMaxMissedCleavages = 0
            CleavageRuleID = CleavageRuleConstants.ConventionalTrypsin
            mMinFragmentResidueCount = 4

            CysTreatmentMode = PeptideSequenceClass.CysTreatmentModeConstants.Untreated

            mMinFragmentMass = 0
            mMaxFragmentMass = 6000

            mMinIsoelectricPoint = 0
            mMaxIsoelectricPoint = 100

            RemoveDuplicateSequences = False
            IncludePrefixAndSuffixResidues = False
            ReDim AminoAcidResidueFilterChars(-1)
        End Sub

#Region "Classwide Variables"
        Private mMaxMissedCleavages As Integer
        Private mMinFragmentResidueCount As Integer
        Private mMinFragmentMass As Integer
        Private mMaxFragmentMass As Integer

        Private mMinIsoelectricPoint As Single
        Private mMaxIsoelectricPoint As Single

#End Region

#Region "Processing Options Interface Functions"

        Public Property AminoAcidResidueFilterChars As Char()

        Public Property MaxMissedCleavages As Integer
            Get
                Return mMaxMissedCleavages
            End Get
            Set
                If Value < 0 Then Value = 0
                If Value > 500000 Then Value = 500000
                mMaxMissedCleavages = Value
            End Set
        End Property

        Public Property CleavageRuleID As CleavageRuleConstants

        Public Property CysTreatmentMode As PeptideSequenceClass.CysTreatmentModeConstants

        Public Property MinFragmentResidueCount As Integer
            Get
                Return mMinFragmentResidueCount
            End Get
            Set
                If Value < 1 Then Value = 1
                mMinFragmentResidueCount = Value
            End Set
        End Property

        Public Property MinFragmentMass As Integer
            Get
                Return mMinFragmentMass
            End Get
            Set
                If Value < 0 Then Value = 0
                mMinFragmentMass = Value
            End Set
        End Property

        Public Property MaxFragmentMass As Integer
            Get
                Return mMaxFragmentMass
            End Get
            Set
                If Value < 0 Then Value = 0
                mMaxFragmentMass = Value
            End Set
        End Property

        Public Property MinIsoelectricPoint As Single
            Get
                Return mMinIsoelectricPoint
            End Get
            Set
                mMinIsoelectricPoint = Value
            End Set
        End Property

        Public Property MaxIsoelectricPoint As Single
            Get
                Return mMaxIsoelectricPoint
            End Get
            Set
                mMaxIsoelectricPoint = Value
            End Set
        End Property

        Public Property RemoveDuplicateSequences As Boolean

        Public Property IncludePrefixAndSuffixResidues As Boolean

#End Region

        Public Sub ValidateOptions()
            If mMaxFragmentMass < mMinFragmentMass Then
                mMaxFragmentMass = mMinFragmentMass
            End If

            If mMaxIsoelectricPoint < mMinIsoelectricPoint Then
                mMaxIsoelectricPoint = mMinIsoelectricPoint
            End If
        End Sub

    End Class
#End Region

End Class

