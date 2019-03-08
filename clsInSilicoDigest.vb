Option Strict On

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
        Public RuleIDInParallax As Integer             ' Unused in this software
    End Structure


#End Region

#Region "Classwide Variables"

    Private mCleavageRules As udtCleavageRuleList

    ' General purpose object for computing mass and calling cleavage and digestion functions
    Private mPeptideSequence As PeptideSequenceClass

    Private mpICalculator As clspICalculation

    Public Event ErrorEvent(strMessage As String)

    Public Event ProgressReset()
    Public Event ProgressChanged(taskDescription As String, percentComplete As Single)     ' PercentComplete ranges from 0 to 100, but can contain decimal percentage values
    Public Event ProgressComplete()

    Protected mProgressStepDescription As String
    Protected mProgressPercentComplete As Single        ' Ranges from 0 to 100, but can contain decimal percentage values

#End Region

#Region "Processing Options Interface Functions"
    Public ReadOnly Property CleaveageRuleCount As Integer
        Get
            Return mCleavageRules.RuleCount
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

    Public Function CheckSequenceAgainstCleavageRule(strSequence As String, eRuleID As CleavageRuleConstants, Optional ByRef intRuleMatchCount As Integer = 0) As Boolean
        ' Checks strSequence against the rule given by eRuleID
        ' See sub InitializeCleavageRules for a list of the rules
        ' Returns True if valid, False if invalid
        ' intRuleMatchCount returns 0, 1, or 2:  0 if neither end matches, 1 if one end matches, 2 if both ends match
        '
        ' In order to check for Exception residues, strSequence must be in the form "R.ABCDEFGK.L" so that the residue following the final residue of the fragment can be examined

        Dim blnRuleMatch As Boolean

        blnRuleMatch = False
        intRuleMatchCount = 0

        If eRuleID >= 0 And eRuleID < mCleavageRules.RuleCount Then
            If eRuleID = CleavageRuleConstants.NoRule Then
                ' No cleavage rule; no point in checking
                blnRuleMatch = True
                intRuleMatchCount = 2
            Else
                blnRuleMatch = mPeptideSequence.CheckSequenceAgainstCleavageRule(
                                                            strSequence,
                                                            mCleavageRules.Rules(eRuleID).CleavageResidues,
                                                            mCleavageRules.Rules(eRuleID).ExceptionResidues,
                                                            mCleavageRules.Rules(eRuleID).ReversedCleavageDirection,
                                                            mCleavageRules.Rules(eRuleID).AllowPartialCleavage,
                                                            , , , intRuleMatchCount)
            End If
        Else
            ' No rule selected; assume True
            intRuleMatchCount = 2
            blnRuleMatch = True
        End If

        Return blnRuleMatch

    End Function

    Public Function ComputeSequenceMass(strSequence As String, Optional blnIncludeXResiduesInMass As Boolean = True) As Double
        ' Note that strSequence must be in 1-letter notation, and will automatically be converted to uppercase

        Try
            With mPeptideSequence
                If blnIncludeXResiduesInMass Then
                    .SetSequence(strSequence.ToUpper)
                Else
                    ' Exclude X residues from strSequence when calling .SetSequence
                    .SetSequence(strSequence.ToUpper.Replace("X", ""))
                End If
                Return .Mass
            End With
        Catch ex As Exception
            ReportError("ComputeSequenceMass", ex)
            Return 0
        End Try

    End Function

    Public Function CountTrypticsInSequence(strSequence As String) As Integer
        Dim intTrypticCount As Integer
        Dim intStartSearchLoc As Integer, intReturnResidueStart As Integer, intReturnResidueEnd As Integer
        Dim strFragment As String

        Try

            intTrypticCount = 0
            intStartSearchLoc = 1

            If strSequence.Length > 0 Then
                Do
                    strFragment = mPeptideSequence.GetTrypticPeptideNext(strSequence, intStartSearchLoc, intReturnResidueStart, intReturnResidueEnd)
                    If strFragment.Length > 0 Then
                        intTrypticCount += 1
                        intStartSearchLoc = intReturnResidueEnd + 1
                    Else
                        Exit Do
                    End If
                Loop
            End If

            Return intTrypticCount

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

        Dim intTrypticFragmentCount As Integer, intFragmentCountTotal As Integer
        Dim intTrypticIndex As Integer
        Dim intSearchStartLoc As Integer
        Dim intResidueStartLoc As Integer, intResidueEndLoc As Integer
        Dim intResidueLength As Integer
        Dim intResidueLengthStart As Integer

        Dim intProteinSequenceLength As Integer

        Dim intIndex As Integer
        Dim intMaxMissedCleavagesStart As Integer

        Dim intTrypticFragCacheCount As Integer
        Dim strTrypticFragCache() As String                 ' 0-based array
        Dim intTrypticFragStartLocs() As Integer            ' 0-based array, parallel to strTypticFragmentCache()
        Dim intTrypticFragEndLocs() As Integer              ' 0-based array, parallel to strTypticFragmentCache()

        Dim strPeptideSequence As String, strPeptideSequenceBase As String
        Dim strRuleResidues As String, strExceptionSuffixResidues As String
        Dim blnReversedCleavageDirection As Boolean

        Dim blnPeptideAdded As Boolean

        peptideFragments = New List(Of PeptideInfoClass)()

        If String.IsNullOrWhiteSpace(proteinSequence) Then
            Return 0
        End If


            strRuleResidues = GetCleaveageRuleResiduesSymbols(objDigestionOptions.CleavageRuleID)
            strExceptionSuffixResidues = GetCleavageExceptionSuffixResidues(objDigestionOptions.CleavageRuleID)
            blnReversedCleavageDirection = GetCleavageIsReversedDirection(objDigestionOptions.CleavageRuleID)

            ' We initially count the number of tryptic peptides in the sequence (regardless of the cleavage rule)
            intTrypticFragmentCount = CountTrypticsInSequence(strProteinSequence)

            ' Increment intTrypticFragmentCount to account for missed cleavages
            ' This will be drastically low if using partial cleavage, but it is a starting point
            intFragmentCountTotal = 0

            intMaxMissedCleavagesStart = objDigestionOptions.MaxMissedCleavages
            If intMaxMissedCleavagesStart > strProteinSequence.Length Then
                intMaxMissedCleavagesStart = strProteinSequence.Length
            End If

            For intIndex = intMaxMissedCleavagesStart + 1 To 1 Step -1
                intFragmentCountTotal += intIndex * intTrypticFragmentCount
                If intFragmentCountTotal > MAX_FRAGMENT_COUNT_PRE_RESERVE Then
                    intFragmentCountTotal = MAX_FRAGMENT_COUNT_PRE_RESERVE
                    Exit For
                End If
            Next intIndex


            ReDim strTrypticFragCache(9)
            ReDim intTrypticFragEndLocs(9)
            ReDim intTrypticFragStartLocs(9)

            intFragmentCountTotal = 0
            intTrypticFragCacheCount = 0
            intSearchStartLoc = 1

            ' Using the GetTrypticPeptideNext function to retrieve the sequence for each tryptic peptide
            '   is faster than using the GetTrypticPeptideByFragmentNumber function
            ' Populate strTrypticFragCache()
            Do
                strPeptideSequence = mPeptideSequence.GetTrypticPeptideNext(strProteinSequence, intSearchStartLoc, intResidueStartLoc, intResidueEndLoc, strRuleResidues, strExceptionSuffixResidues, blnReversedCleavageDirection)
                If strPeptideSequence.Length > 0 Then

                    strTrypticFragCache(intTrypticFragCacheCount) = strPeptideSequence
                    intTrypticFragStartLocs(intTrypticFragCacheCount) = intResidueStartLoc
                    intTrypticFragEndLocs(intTrypticFragCacheCount) = intResidueEndLoc
                    intTrypticFragCacheCount += 1

                    If intTrypticFragCacheCount >= strTrypticFragCache.Length Then
                        ReDim Preserve strTrypticFragCache(strTrypticFragCache.Length + 10)
                        ReDim Preserve intTrypticFragEndLocs(strTrypticFragCache.Length - 1)
                        ReDim Preserve intTrypticFragStartLocs(strTrypticFragCache.Length - 1)
                    End If

                    intSearchStartLoc = intResidueEndLoc + 1
                Else
                    Exit Do
                End If
            Loop

            ResetProgress("Digesting protein " & strProteinName)

            For intTrypticIndex = 0 To intTrypticFragCacheCount - 1
                strPeptideSequenceBase = String.Empty
                strPeptideSequence = String.Empty
                intResidueStartLoc = intTrypticFragStartLocs(intTrypticIndex)

                For intIndex = 0 To objDigestionOptions.MaxMissedCleavages
                    If intTrypticIndex + intIndex >= intTrypticFragCacheCount Then
                        Exit For
                    End If

                    If objDigestionOptions.CleavageRuleID = CleavageRuleConstants.KROneEnd Then
                        ' Partially tryptic cleavage rule: Add all partially tryptic fragments
                        If intIndex = 0 Then
                            intResidueLengthStart = objDigestionOptions.MinFragmentResidueCount
                            If intResidueLengthStart < 1 Then intResidueLengthStart = 1
                        Else
                            intResidueLengthStart = 1
                        End If

                        For intResidueLength = intResidueLengthStart To strTrypticFragCache(intTrypticIndex + intIndex).Length
                            If intIndex > 0 Then
                                intResidueEndLoc = intTrypticFragEndLocs(intTrypticIndex + intIndex - 1) + intResidueLength
                            Else
                                intResidueEndLoc = intResidueStartLoc + intResidueLength - 1
                            End If

                            strPeptideSequence = strPeptideSequenceBase & strTrypticFragCache(intTrypticIndex + intIndex).Substring(0, intResidueLength)

                            If peptideSequence.Length >= digestionOptions.MinFragmentResidueCount Then
                                PossiblyAddPeptide(peptideSequence, trypticIndex, index, residueStartLoc, residueEndLoc, proteinSequence, proteinSequenceLength, fragmentsUniqueList, peptideFragments, digestionOptions, filterByIsoelectricPoint)
                            End If
                        Next intResidueLength
                    Else
                        ' Normal cleavage rule
                        intResidueEndLoc = intTrypticFragEndLocs(intTrypticIndex + intIndex)

                        strPeptideSequence = strPeptideSequence & strTrypticFragCache(intTrypticIndex + intIndex)
                        If strPeptideSequence.Length >= objDigestionOptions.MinFragmentResidueCount Then
                            PossiblyAddPeptide(peptideSequence, trypticIndex, index, residueStartLoc, residueEndLoc, proteinSequence, proteinSequenceLength, fragmentsUniqueList, peptideFragments, digestionOptions, filterByIsoelectricPoint)
                        End If
                    End If

                    strPeptideSequenceBase = strPeptideSequenceBase & strTrypticFragCache(intTrypticIndex + intIndex)
                Next intIndex

                If objDigestionOptions.CleavageRuleID = CleavageRuleConstants.KROneEnd Then
                    UpdateProgress(CSng(intTrypticIndex / (intTrypticFragCacheCount * 2) * 100.0))
                End If
            Next intTrypticIndex

            If objDigestionOptions.CleavageRuleID = CleavageRuleConstants.KROneEnd Then
                ' Partially tryptic cleavage rule: Add all partially tryptic fragments, working from the end toward the front
                For intTrypticIndex = intTrypticFragCacheCount - 1 To 0 Step -1
                    strPeptideSequenceBase = String.Empty
                    strPeptideSequence = String.Empty
                    intResidueEndLoc = intTrypticFragEndLocs(intTrypticIndex)

                    For intIndex = 0 To objDigestionOptions.MaxMissedCleavages
                        If intTrypticIndex - intIndex < 0 Then
                            Exit For
                        End If

                        If intIndex = 0 Then
                            intResidueLengthStart = objDigestionOptions.MinFragmentResidueCount
                        Else
                            intResidueLengthStart = 1
                        End If

                        ' We can limit the following for loop to the peptide length - 1 since those peptides using the full peptide will have already been added above
                        For intResidueLength = intResidueLengthStart To strTrypticFragCache(intTrypticIndex - intIndex).Length - 1
                            If intIndex > 0 Then
                                intResidueStartLoc = intTrypticFragStartLocs(intTrypticIndex - intIndex + 1) - intResidueLength
                            Else
                                intResidueStartLoc = intResidueEndLoc - (intResidueLength - 1)
                            End If

                            ' Grab characters from the end of strTrypticFragCache()
                            strPeptideSequence = strTrypticFragCache(intTrypticIndex - intIndex).Substring(strTrypticFragCache(intTrypticIndex - intIndex).Length - intResidueLength, intResidueLength) & strPeptideSequenceBase

                            If strPeptideSequence.Length >= objDigestionOptions.MinFragmentResidueCount Then
                                PossiblyAddPeptide(peptideSequence, trypticIndex, index, residueStartLoc, residueEndLoc, proteinSequence, proteinSequenceLength, fragmentsUniqueList, peptideFragments, digestionOptions, filterByIsoelectricPoint)
                            End If

                        Next intResidueLength

                        strPeptideSequenceBase = strTrypticFragCache(intTrypticIndex - intIndex) & strPeptideSequenceBase
                    Next intIndex

                    UpdateProgress(CSng((intTrypticFragCacheCount * 2 - intTrypticIndex) / (intTrypticFragCacheCount * 2) * 100))

                Next intTrypticIndex

            End If

            Return peptideFragments.Count

        Catch ex As Exception
            ReportError("DigestSequence", ex)
            Return peptideFragments.Count
        End Try

    End Function

    Public Function GetCleavageAllowPartialCleavage(eRuleID As CleavageRuleConstants) As Boolean
        If eRuleID >= 0 And eRuleID < mCleavageRules.RuleCount Then
            Return mCleavageRules.Rules(eRuleID).AllowPartialCleavage
        Else
            Return False
        End If
    End Function

    Public Function GetCleavageIsReversedDirection(eRuleID As CleavageRuleConstants) As Boolean
        If eRuleID >= 0 And eRuleID < mCleavageRules.RuleCount Then
            Return mCleavageRules.Rules(eRuleID).ReversedCleavageDirection
        Else
            Return False
        End If
    End Function

    Public Function GetCleavageExceptionSuffixResidues(eRuleID As CleavageRuleConstants) As String
        If eRuleID >= 0 And eRuleID < mCleavageRules.RuleCount Then
            Return mCleavageRules.Rules(eRuleID).ExceptionResidues
        Else
            Return String.Empty
        End If
    End Function

    Public Function GetCleaveageRuleName(eRuleID As CleavageRuleConstants) As String
        If eRuleID >= 0 And eRuleID < mCleavageRules.RuleCount Then
            Return mCleavageRules.Rules(eRuleID).Description
        Else
            Return String.Empty
        End If
    End Function

    Public Function GetCleaveageRuleResiduesDescription(eRuleID As CleavageRuleConstants) As String
        Dim strDescription As String

        If eRuleID >= 0 And eRuleID < mCleavageRules.RuleCount Then
            With mCleavageRules.Rules(eRuleID)
                If .ReversedCleavageDirection Then
                    strDescription = "Before " & .CleavageResidues
                    If .ExceptionResidues.Length > 0 Then
                        strDescription &= " not preceded by " & .ExceptionResidues
                    End If
                Else
                    strDescription = .CleavageResidues
                    If .ExceptionResidues.Length > 0 Then
                        strDescription &= " not " & .ExceptionResidues
                    End If
                End If
            End With

            Return strDescription
        Else
            Return String.Empty
        End If
    End Function

    Public Function GetCleaveageRuleResiduesSymbols(eRuleID As CleavageRuleConstants) As String
        If eRuleID >= 0 And eRuleID < mCleavageRules.RuleCount Then
            Return mCleavageRules.Rules(eRuleID).CleavageResidues
        Else
            Return String.Empty
        End If
    End Function

    Public Function GetCleaveageRuleIDInParallax(eRuleID As CleavageRuleConstants) As Integer
        If eRuleID >= 0 And eRuleID < mCleavageRules.RuleCount Then
            Return mCleavageRules.Rules(eRuleID).RuleIDInParallax
        Else
            Return -1
        End If

    End Function

    Private Sub InitializeCleavageRules()

        ' Useful site for cleavage rule info is https://web.expasy.org/peptide_mass/peptide-mass-doc.html

        With mCleavageRules
            .RuleCount = CleavageRuleCount
            ReDim .Rules(.RuleCount - 1)          ' 0-based array

            With .Rules(CleavageRuleConstants.NoRule)
                .Description = "No cleavage rule"
                .CleavageResidues = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
                .ExceptionResidues = String.Empty
                .ReversedCleavageDirection = False
                .RuleIDInParallax = 0
            End With

            With .Rules(CleavageRuleConstants.ConventionalTrypsin)
                .Description = "Fully Tryptic"
                .CleavageResidues = "KR"
                .ExceptionResidues = "P"
                .ReversedCleavageDirection = False
                .RuleIDInParallax = 10
            End With

            With .Rules(CleavageRuleConstants.TrypsinWithoutProlineException)
                .Description = "Fully Tryptic (no Proline Rule)"
                .CleavageResidues = "KR"
                .ExceptionResidues = String.Empty
                .ReversedCleavageDirection = False
                .RuleIDInParallax = 0
            End With

            With .Rules(CleavageRuleConstants.EricPartialTrypsin)
                .Description = "Eric's Partial Trypsin"
                .CleavageResidues = "KRFYVEL"
                .ExceptionResidues = String.Empty
                .ReversedCleavageDirection = False
                .AllowPartialCleavage = True                        ' Allows partial cleavage
                .RuleIDInParallax = 11
            End With

            With .Rules(CleavageRuleConstants.TrypsinPlusFVLEY)
                .Description = "Trypsin Plus FVLEY"
                .CleavageResidues = "KRFYVEL"
                .ExceptionResidues = ""
                .ReversedCleavageDirection = False
                .RuleIDInParallax = 12
            End With

            With .Rules(CleavageRuleConstants.KROneEnd)
                .Description = "Half (Partial) Trypsin "
                .CleavageResidues = "KR"
                .ExceptionResidues = "P"
                .ReversedCleavageDirection = False
                .AllowPartialCleavage = True
                .RuleIDInParallax = 13
            End With

            With .Rules(CleavageRuleConstants.TerminiiOnly)
                .Description = "Peptide Database; terminii only"
                .CleavageResidues = "-"
                .ExceptionResidues = String.Empty
                .ReversedCleavageDirection = False
                .RuleIDInParallax = 20
            End With

            With .Rules(CleavageRuleConstants.Chymotrypsin)
                .Description = "Chymotrypsin"
                .CleavageResidues = "FWYL"
                .ExceptionResidues = String.Empty
                .ReversedCleavageDirection = False
                .RuleIDInParallax = 30
            End With

            With .Rules(CleavageRuleConstants.ChymotrypsinAndTrypsin)
                .Description = "Chymotrypsin + Trypsin"
                .CleavageResidues = "FWYLKR"
                .ExceptionResidues = String.Empty
                .ReversedCleavageDirection = False
                .RuleIDInParallax = 31
            End With

            With .Rules(CleavageRuleConstants.GluC)
                .Description = "Glu-C"
                .CleavageResidues = "ED"
                .ExceptionResidues = String.Empty
                .ReversedCleavageDirection = False
                .RuleIDInParallax = 40
            End With

            With .Rules(CleavageRuleConstants.CyanBr)
                .Description = "CyanBr"
                .CleavageResidues = "M"
                .ExceptionResidues = String.Empty
                .ReversedCleavageDirection = False
                .RuleIDInParallax = 50
            End With

            With .Rules(CleavageRuleConstants.LysC)
                .Description = "Lys-C"
                .CleavageResidues = "K"
                .ExceptionResidues = String.Empty
                .ReversedCleavageDirection = False
                .RuleIDInParallax = 0
            End With

            With .Rules(CleavageRuleConstants.GluC_EOnly)
                .Description = "Glu-C, just Glu"
                .CleavageResidues = "E"
                .ExceptionResidues = String.Empty
                .ReversedCleavageDirection = False
                .RuleIDInParallax = 0
            End With

            With .Rules(CleavageRuleConstants.ArgC)
                .Description = "Arg-C"
                .CleavageResidues = "R"
                .ExceptionResidues = String.Empty
                .ReversedCleavageDirection = False
                .RuleIDInParallax = 0
            End With

            With .Rules(CleavageRuleConstants.AspN)
                .Description = "Asp-N"
                .CleavageResidues = "D"
                .ExceptionResidues = String.Empty
                .ReversedCleavageDirection = True
                .RuleIDInParallax = 0
            End With

            With .Rules(CleavageRuleConstants.ProteinaseK)
                .Description = "Proteinase K"
                .CleavageResidues = "AEFILTVWY"
                .ExceptionResidues = String.Empty
                .ReversedCleavageDirection = False
                .RuleIDInParallax = 0
            End With

            With .Rules(CleavageRuleConstants.PepsinA)
                .Description = "PepsinA"
                .CleavageResidues = "FLIWY"
                .ExceptionResidues = "P"
                .ReversedCleavageDirection = False
                .RuleIDInParallax = 0
            End With

            With .Rules(CleavageRuleConstants.PepsinB)
                .Description = "PepsinB"
                .CleavageResidues = "FLIWY"
                .ExceptionResidues = "PVAG"
                .ReversedCleavageDirection = False
                .RuleIDInParallax = 0
            End With

            With .Rules(CleavageRuleConstants.PepsinC)
                .Description = "PepsinC"
                .CleavageResidues = "FLWYA"
                .ExceptionResidues = "P"
                .ReversedCleavageDirection = False
                .RuleIDInParallax = 0
            End With

            With .Rules(CleavageRuleConstants.PepsinD)
                .Description = "PepsinD"
                .CleavageResidues = "FLWYAEQ"
                .ExceptionResidues = ""
                .ReversedCleavageDirection = False
                .RuleIDInParallax = 0
            End With

            With .Rules(CleavageRuleConstants.AceticAcidD)
                .Description = "Acetic Acid Hydrolysis"
                .CleavageResidues = "D"
                .ExceptionResidues = String.Empty
                .ReversedCleavageDirection = False
                .RuleIDInParallax = 0
            End With

        End With

    End Sub

    Public Sub InitializepICalculator()
        Me.InitializepICalculator(New clspICalculation)
    End Sub

    Public Sub InitializepICalculator(ByRef objpICalculator As clspICalculation)
        If Not mpICalculator Is Nothing Then
            If mpICalculator Is objpICalculator Then
                ' Classes are the same instance of the object; no need to update anything
                Exit Sub
            Else
                mpICalculator = Nothing
            End If
        End If

        mpICalculator = objpICalculator
    End Sub

    Public Sub InitializepICalculator(eHydrophobicityType As clspICalculation.eHydrophobicityTypeConstants, blnReportMaximumpI As Boolean, intSequenceWidthToExamineForMaximumpI As Integer)
        If mpICalculator Is Nothing Then
            mpICalculator = New clspICalculation
        End If

        With mpICalculator
            .HydrophobicityType = eHydrophobicityType
            .ReportMaximumpI = blnReportMaximumpI
            .SequenceWidthToExamineForMaximumpI = intSequenceWidthToExamineForMaximumpI
        End With
    End Sub

    Private Sub PossiblyAddPeptide(
        peptideSequence As String,
        trypticIndex As Integer,
        missedCleavageCount As Integer,
        residueStartLoc As Integer,
        residueEndLoc As Integer,
        ByRef proteinSequence As String,
        proteinSequenceLength As Integer,
        fragmentsUniqueList As Specialized.StringDictionary,
        peptideFragments As ICollection(Of PeptideInfoClass),
        digestionOptions As DigestionOptionsClass,
        filterByIsoelectricPoint As Boolean)
        '       It is not modified by this function

        Dim blnAddFragment As Boolean
        Dim strPrefix As String, strSuffix As String

        Dim sngIsoelectricPoint As Single

        blnAddFragment = True
        If objDigestionOptions.RemoveDuplicateSequences Then
            If htFragmentsUniqueList.ContainsKey(strPeptideSequence) Then
                blnAddFragment = False
            Else
                htFragmentsUniqueList.Add(strPeptideSequence, Nothing)
            End If
        End If

        If blnAddFragment AndAlso objDigestionOptions.AminoAcidResidueFilterChars.Length > 0 Then
            If strPeptideSequence.IndexOfAny(objDigestionOptions.AminoAcidResidueFilterChars) < 0 Then
                blnAddFragment = False
            End If
        End If

        If Not addFragment Then Return

        Dim peptideFragment = New PeptideInfoClass With {.AutoComputeNET = False}

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

    Private Sub ReportError(strFunctionName As String, ex As Exception)
        Try
            Dim strErrorMessage As String

            If Not strFunctionName Is Nothing AndAlso strFunctionName.Length > 0 Then
                strErrorMessage = "Error in " & strFunctionName & ": " & ex.Message
            Else
                strErrorMessage = "Error: " & ex.Message
            End If

            Console.WriteLine(strErrorMessage)

            RaiseEvent ErrorEvent(strErrorMessage)

        Catch exNew As Exception
            ' Ignore errors here
        End Try
    End Sub

    Protected Sub ResetProgress()
        RaiseEvent ProgressReset()
    End Sub

    Protected Sub ResetProgress(strProgressStepDescription As String)
        UpdateProgress(strProgressStepDescription, 0)
        RaiseEvent ProgressReset()
    End Sub

    Protected Sub UpdateProgress(strProgressStepDescription As String)
        UpdateProgress(strProgressStepDescription, mProgressPercentComplete)
    End Sub

    Protected Sub UpdateProgress(sngPercentComplete As Single)
        UpdateProgress(Me.ProgressStepDescription, sngPercentComplete)
    End Sub

    Protected Sub UpdateProgress(strProgressStepDescription As String, sngPercentComplete As Single)
        Dim blnDescriptionChanged As Boolean = False

        If strProgressStepDescription <> mProgressStepDescription Then
            blnDescriptionChanged = True
        End If

        mProgressStepDescription = String.Copy(strProgressStepDescription)
        If sngPercentComplete < 0 Then
            sngPercentComplete = 0
        ElseIf sngPercentComplete > 100 Then
            sngPercentComplete = 100
        End If
        mProgressPercentComplete = sngPercentComplete

        RaiseEvent ProgressChanged(Me.ProgressStepDescription, Me.ProgressPercentComplete)

    End Sub

#Region "Peptide Info class"

    Public Class PeptideInfoClass
        Inherits PeptideSequenceClass

        ' Adds NET computation to the PeptideSequenceClass

        Public Sub New()

            If objNETPrediction Is Nothing Then
                objNETPrediction = New NETPrediction.ElutionTimePredictionKangas
            End If

            ' Disable mAutoComputeNET for now so that the call to SetSequence() below doesn't auto-call UpdateNET
            mAutoComputeNET = False

            mPeptideName = String.Empty
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
        Private Shared objNETPrediction As NETPrediction.iPeptideElutionTime


        Private mAutoComputeNET As Boolean      ' Set to False to skip computation of NET when Sequence changes; useful for speeding up things a little

        Private mPeptideName As String
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
            Get
                Return mPeptideName
            End Get
            Set
                mPeptideName = Value
            End Set
        End Property

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
                Return MyBase.GetSequence(False)
            End Get
            Set
                MyClass.SetSequence(Value, PeptideSequenceClass.NTerminusGroupConstants.Hydrogen, PeptideSequenceClass.CTerminusGroupConstants.Hydroxyl, False)
            End Set
        End Property

        Public ReadOnly Property SequenceWithPrefixAndSuffix As String
            Get
                Return mPrefixResidue & "." & MyClass.SequenceOneLetter & "." & mSuffixResidue
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

        Public Overrides Function SetSequence(strSequence As String, Optional eNTerminus As NTerminusGroupConstants = NTerminusGroupConstants.Hydrogen, Optional eCTerminus As CTerminusGroupConstants = CTerminusGroupConstants.Hydroxyl, Optional blnIs3LetterCode As Boolean = False, Optional bln1LetterCheckForPrefixAndSuffixResidues As Boolean = True, Optional bln3LetterCheckForPrefixHandSuffixOH As Boolean = True) As Integer
            Dim intReturn As Integer

            intReturn = MyBase.SetSequence(strSequence, eNTerminus, eCTerminus, blnIs3LetterCode, bln1LetterCheckForPrefixAndSuffixResidues, bln3LetterCheckForPrefixHandSuffixOH)
            If mAutoComputeNET Then UpdateNET()

            Return intReturn
        End Function

        Public Overrides Sub SetSequenceOneLetterCharactersOnly(strSequenceNoPrefixOrSuffix As String)
            MyBase.SetSequenceOneLetterCharactersOnly(strSequenceNoPrefixOrSuffix)
            If mAutoComputeNET Then UpdateNET()
        End Sub

        Public Sub UpdateNET()
            Try
                mNET = objNETPrediction.GetElutionTime(MyBase.GetSequence(False))
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
            mCleavageRuleID = clsInSilicoDigest.CleavageRuleConstants.ConventionalTrypsin
            mMinFragmentResidueCount = 4

            mMinFragmentMass = 0
            mMaxFragmentMass = 6000

            mMinIsoelectricPoint = 0
            mMaxIsoelectricPoint = 100

            mRemoveDuplicateSequences = False
            mIncludePrefixAndSuffixResidues = False
            ReDim mAminoAcidResidueFilterChars(-1)
        End Sub

#Region "Classwide Variables"
        Private mMaxMissedCleavages As Integer
        Private mCleavageRuleID As clsInSilicoDigest.CleavageRuleConstants
        Private mMinFragmentResidueCount As Integer
        Private mMinFragmentMass As Integer
        Private mMaxFragmentMass As Integer

        Private mMinIsoelectricPoint As Single
        Private mMaxIsoelectricPoint As Single

        Private mRemoveDuplicateSequences As Boolean
        Private mIncludePrefixAndSuffixResidues As Boolean
        Private mAminoAcidResidueFilterChars As Char()
#End Region

#Region "Processing Options Interface Functions"


        Public Property AminoAcidResidueFilterChars As Char()
            Get
                Return mAminoAcidResidueFilterChars
            End Get
            Set
                mAminoAcidResidueFilterChars = Value
            End Set
        End Property

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

        Public Property CleavageRuleID As clsInSilicoDigest.CleavageRuleConstants
            Get
                Return mCleavageRuleID
            End Get
            Set
                mCleavageRuleID = Value
            End Set
        End Property

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
            Get
                Return mRemoveDuplicateSequences
            End Get
            Set
                mRemoveDuplicateSequences = Value
            End Set
        End Property

        Public Property IncludePrefixAndSuffixResidues As Boolean
            Get
                Return mIncludePrefixAndSuffixResidues
            End Get
            Set
                mIncludePrefixAndSuffixResidues = Value
            End Set
        End Property
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

