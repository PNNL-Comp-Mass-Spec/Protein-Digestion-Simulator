Option Strict On
'
' This class can be used to perform an in-silico digest of an amino acid sequence
' Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in November 2003
' Copyright 2005, Battelle Memorial Institute.  All Rights Reserved.

' Ported by VB.NET by Matthew Monroe in October 2004
'
' Utilizes the PeptideInfoClass class
'
' Last Modified July 30, 2007
'
Public Class clsInSilicoDigest

    Public Sub New()
        mShowMessages = True
        mPeptideSequence = New PeptideSequenceClass
        mPeptideSequence.ElementMode = PeptideSequenceClass.ElementModeConstants.IsotopicMass

        InitializeCleavageRules()
        InitializepICalculator()
    End Sub

#Region "Constants and Enums"

    ' Note: Good list of enzymes is at http://ca.expasy.org/tools/peptidecutter/peptidecutter_enzymes.html
    Private Const CleavageRuleCount As Integer = 18
    Public Enum CleavageRuleConstants
        NoRule = 0
        ConventionalTrypsin = 1
        EricPartialTrypsin = 2
        TrypsinPlusFVLEY = 3
        KROneEnd = 4
        TerminiiOnly = 5
        Chymotrypsin = 6
        ChymotrypsinAndTrypsin = 7
        GluC = 8
        CyanBr = 9                      ' Aka CNBr
        LysC = 10
        GluC_EOnly = 11
        ArgC = 12
        AspN = 13
        ProteinaseK = 14
        PepsinA = 15
        PepsinB = 16
        PepsinC = 17
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

    Private Structure udtCleavageRuleList
        Public RuleCount As Integer
        Public Rules() As udtCleavageRulesType         ' 0-based array
    End Structure

#End Region

#Region "Classwide Variables"

    Private mCleavageRules As udtCleavageRuleList
    Private mShowMessages As Boolean

    ' General purpose object for computing mass and calling cleavage and digestion functions
    Private mPeptideSequence As PeptideSequenceClass

    Private mpICalculator As clspICalculation

#End Region

#Region "Processing Options Interface Functions"
    Public ReadOnly Property CleaveageRuleCount() As Integer
        Get
            Return mCleavageRules.RuleCount
        End Get
    End Property

    Public Property ShowMessages() As Boolean
        Get
            Return mShowMessages
        End Get
        Set(ByVal Value As Boolean)
            mShowMessages = Value
        End Set
    End Property

    Public Property ElementMassMode() As PeptideSequenceClass.ElementModeConstants
        Get
            If mPeptideSequence Is Nothing Then
                Return PeptideSequenceClass.ElementModeConstants.IsotopicMass
            Else
                Return mPeptideSequence.ElementMode
            End If
        End Get
        Set(ByVal Value As PeptideSequenceClass.ElementModeConstants)
            If mPeptideSequence Is Nothing Then
                mPeptideSequence = New PeptideSequenceClass
            End If
            mPeptideSequence.ElementMode = Value
        End Set
    End Property
#End Region

    Public Function CheckSequenceAgainstCleavageRule(ByVal strSequence As String, ByVal eRuleID As CleavageRuleConstants, Optional ByRef intRuleMatchCount As Integer = 0) As Boolean
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
                blnRuleMatch = mPeptideSequence.CheckSequenceAgainstCleavageRule( _
                                                            strSequence, _
                                                            mCleavageRules.Rules(eRuleID).CleavageResidues, _
                                                            mCleavageRules.Rules(eRuleID).ExceptionResidues, _
                                                            mCleavageRules.Rules(eRuleID).ReversedCleavageDirection, _
                                                            mCleavageRules.Rules(eRuleID).AllowPartialCleavage, _
                                                            , , , intRuleMatchCount)
            End If
        Else
            ' No rule selected; assume True
            intRuleMatchCount = 2
            blnRuleMatch = True
        End If

        Return blnRuleMatch

    End Function

    Public Function ComputeSequenceMass(ByVal strSequence As String, Optional ByVal blnIncludeXResiduesInMass As Boolean = True) As Double
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

    Public Function CountTrypticsInSequence(ByVal strSequence As String) As Integer
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

    Public Function DigestSequence(ByVal strProteinSequence As String, ByRef objPeptideFragments() As PeptideInfoClass, ByVal objDigestionOptions As DigestionOptionsClass, ByVal blnFilterByIsoelectricPoint As Boolean, Optional ByVal strProteinName As String = "", Optional ByRef objProgressForm As ProgressFormNET.frmProgress = Nothing) As Integer
        ' Digests strProteinSequence using the sequence rule given by objDigestionOptions.CleavageRuleID
        ' If blnRemoveDuplicateSequences = True, then only returns the first occurrence of each unique sequence
        '
        ' Returns the number of fragments
        ' Returns the fragment info in objPeptideFragments()

        Const MAX_FRAGMENT_COUNT_PRE_RESERVE As Integer = 100000
        Dim htFragmentsUniqueList As New System.Collections.Specialized.StringDictionary

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
        Dim blnUsingExistingProgressForm As Boolean

        Try

            intProteinSequenceLength = strProteinSequence.Length

            If intProteinSequenceLength = 0 Then
                ReDim objPeptideFragments(0)
                objPeptideFragments(0) = New PeptideInfoClass
                DigestSequence = 0
                Exit Function
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

            ReDim objPeptideFragments(intFragmentCountTotal - 1)
            objPeptideFragments(0) = New PeptideInfoClass
            objPeptideFragments(0).AutoComputeNET = False

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

            If objDigestionOptions.CleavageRuleID = CleavageRuleConstants.KROneEnd Then
                If objProgressForm Is Nothing Then
                    objProgressForm = New ProgressFormNET.frmProgress
                    objProgressForm.MoveToBottomCenter()
                    objProgressForm.InitializeProgressForm("Digesting protein " & strProteinName, 0, intTrypticFragCacheCount * 2, False, False)
                    blnUsingExistingProgressForm = False
                Else
                    blnUsingExistingProgressForm = True
                    objProgressForm.InitializeSubtask("Digesting protein " & strProteinName, 0, intTrypticFragCacheCount * 2)
                End If
                objProgressForm.Visible = True
                Windows.Forms.Application.DoEvents()
            End If

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

                            If strPeptideSequence.Length >= objDigestionOptions.MinFragmentResidueCount Then
                                PossiblyAddPeptide(strPeptideSequence, intTrypticIndex, intIndex, intResidueStartLoc, intResidueEndLoc, strProteinSequence, intProteinSequenceLength, htFragmentsUniqueList, objPeptideFragments, intFragmentCountTotal, objDigestionOptions, blnFilterByIsoelectricPoint)
                            End If
                        Next intResidueLength
                    Else
                        ' Normal cleavage rule
                        intResidueEndLoc = intTrypticFragEndLocs(intTrypticIndex + intIndex)

                        strPeptideSequence = strPeptideSequence & strTrypticFragCache(intTrypticIndex + intIndex)
                        If strPeptideSequence.Length >= objDigestionOptions.MinFragmentResidueCount Then
                            PossiblyAddPeptide(strPeptideSequence, intTrypticIndex, intIndex, intResidueStartLoc, intResidueEndLoc, strProteinSequence, intProteinSequenceLength, htFragmentsUniqueList, objPeptideFragments, intFragmentCountTotal, objDigestionOptions, blnFilterByIsoelectricPoint)
                        End If
                    End If

                    strPeptideSequenceBase = strPeptideSequenceBase & strTrypticFragCache(intTrypticIndex + intIndex)
                Next intIndex

                If objDigestionOptions.CleavageRuleID = CleavageRuleConstants.KROneEnd Then
                    If blnUsingExistingProgressForm Then
                        objProgressForm.UpdateSubtaskProgressBar(intTrypticIndex)
                    Else
                        objProgressForm.UpdateProgressBar(intTrypticIndex)
                    End If
                    Windows.Forms.Application.DoEvents()
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
                                blnPeptideAdded = PossiblyAddPeptide(strPeptideSequence, intTrypticIndex, intIndex, intResidueStartLoc, intResidueEndLoc, strProteinSequence, intProteinSequenceLength, htFragmentsUniqueList, objPeptideFragments, intFragmentCountTotal, objDigestionOptions, blnFilterByIsoelectricPoint)
                            End If

                        Next intResidueLength

                        strPeptideSequenceBase = strTrypticFragCache(intTrypticIndex - intIndex) & strPeptideSequenceBase
                    Next intIndex

                    If blnUsingExistingProgressForm Then
                        objProgressForm.UpdateSubtaskProgressBar(intTrypticFragCacheCount * 2 - intTrypticIndex)
                    Else
                        objProgressForm.UpdateProgressBar(intTrypticFragCacheCount * 2 - intTrypticIndex)
                    End If
                    Windows.Forms.Application.DoEvents()

                Next intTrypticIndex

            End If


            If intFragmentCountTotal < objPeptideFragments.Length And intFragmentCountTotal > 0 Then
                ReDim Preserve objPeptideFragments(intFragmentCountTotal - 1)
            End If

            Return intFragmentCountTotal
        Catch ex As Exception
            ReportError("DigestSequence", ex)
            Return intFragmentCountTotal
        Finally
            If objDigestionOptions.CleavageRuleID = CleavageRuleConstants.KROneEnd AndAlso Not objProgressForm Is Nothing Then
                If Not blnUsingExistingProgressForm Then
                    objProgressForm.HideForm()
                    objProgressForm.Close()
                    objProgressForm = Nothing
                End If
            End If
        End Try

    End Function

    Public Function GetCleavageAllowPartialCleavage(ByVal eRuleID As CleavageRuleConstants) As Boolean
        If eRuleID >= 0 And eRuleID < mCleavageRules.RuleCount Then
            Return mCleavageRules.Rules(eRuleID).AllowPartialCleavage
        Else
            Return False
        End If
    End Function

    Public Function GetCleavageIsReversedDirection(ByVal eRuleID As CleavageRuleConstants) As Boolean
        If eRuleID >= 0 And eRuleID < mCleavageRules.RuleCount Then
            Return mCleavageRules.Rules(eRuleID).ReversedCleavageDirection
        Else
            Return False
        End If
    End Function

    Public Function GetCleavageExceptionSuffixResidues(ByVal eRuleID As CleavageRuleConstants) As String
        If eRuleID >= 0 And eRuleID < mCleavageRules.RuleCount Then
            Return mCleavageRules.Rules(eRuleID).ExceptionResidues
        Else
            Return String.Empty
        End If
    End Function

    Public Function GetCleaveageRuleName(ByVal eRuleID As CleavageRuleConstants) As String
        If eRuleID >= 0 And eRuleID < mCleavageRules.RuleCount Then
            Return mCleavageRules.Rules(eRuleID).Description
        Else
            Return String.Empty
        End If
    End Function

    Public Function GetCleaveageRuleResiduesDescription(ByVal eRuleID As CleavageRuleConstants) As String
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

    Public Function GetCleaveageRuleResiduesSymbols(ByVal eRuleID As CleavageRuleConstants) As String
        If eRuleID >= 0 And eRuleID < mCleavageRules.RuleCount Then
            Return mCleavageRules.Rules(eRuleID).CleavageResidues
        Else
            Return String.Empty
        End If
    End Function

    Public Function GetCleaveageRuleIDInParallax(ByVal eRuleID As CleavageRuleConstants) As Integer
        If eRuleID >= 0 And eRuleID < mCleavageRules.RuleCount Then
            Return mCleavageRules.Rules(eRuleID).RuleIDInParallax
        Else
            Return -1
        End If

    End Function

    Private Sub InitializeCleavageRules()

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
                .ReversedCleavageDirection = True
                .RuleIDInParallax = 0
            End With

            With .Rules(CleavageRuleConstants.PepsinB)
                .Description = "PepsinB"
                .CleavageResidues = "FLIWY"
                .ExceptionResidues = "PVAG"
                .ReversedCleavageDirection = True
                .RuleIDInParallax = 0
            End With

            With .Rules(CleavageRuleConstants.PepsinC)
                .Description = "PepsinC"
                .CleavageResidues = "FLWYA"
                .ExceptionResidues = "P"
                .ReversedCleavageDirection = True
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

    Public Sub InitializepICalculator(ByVal eHydrophobicityType As clspICalculation.eHydrophobicityTypeConstants, ByVal blnReportMaximumpI As Boolean, ByVal intSequenceWidthToExamineForMaximumpI As Integer)
        If mpICalculator Is Nothing Then
            mpICalculator = New clspICalculation
        End If

        With mpICalculator
            .HydrophobicityType = eHydrophobicityType
            .ReportMaximumpI = blnReportMaximumpI
            .SequenceWidthToExamineForMaximumpI = intSequenceWidthToExamineForMaximumpI
        End With
    End Sub

    Private Function PossiblyAddPeptide(ByVal strPeptideSequence As String, ByVal intTrypticIndex As Integer, ByVal intMissedCleavageCount As Integer, ByVal intResidueStartLoc As Integer, ByVal intResidueEndLoc As Integer, ByRef strProteinSequence As String, ByVal intProteinSequenceLength As Integer, ByRef htFragmentsUniqueList As System.Collections.Specialized.StringDictionary, ByRef objPeptideFragments() As PeptideInfoClass, ByRef intFragmentCountTotal As Integer, ByVal objDigestionOptions As DigestionOptionsClass, ByVal blnFilterByIsoelectricPoint As Boolean) As Boolean
        ' Note: strProteinSequence is passed ByRef for speed purposes since passing a reference of a large string is easier than passing it ByVal
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

        If blnAddFragment Then

            If objPeptideFragments(intFragmentCountTotal) Is Nothing Then
                objPeptideFragments(intFragmentCountTotal) = New PeptideInfoClass
                objPeptideFragments(intFragmentCountTotal).AutoComputeNET = False
            End If

            With objPeptideFragments(intFragmentCountTotal)
                .SequenceOneLetter = strPeptideSequence

                If .Mass < objDigestionOptions.MinFragmentMass OrElse _
                   .Mass > objDigestionOptions.MaxFragmentMass Then
                    blnAddFragment = False
                Else
                    ' Possibly compute the isoelectric point for the peptide
                    If blnFilterByIsoelectricPoint Then
                        sngIsoelectricPoint = mpICalculator.CalculateSequencepI(strPeptideSequence)
                    End If

                    If blnFilterByIsoelectricPoint AndAlso ( _
                       sngIsoelectricPoint < objDigestionOptions.MinIsoelectricPoint OrElse _
                       sngIsoelectricPoint > objDigestionOptions.MaxIsoelectricPoint) Then
                        blnAddFragment = False
                    Else
                        ' We can now compute the NET value for the peptide
                        .UpdateNET()

                        If objDigestionOptions.IncludePrefixAndSuffixResidues Then
                            If intResidueStartLoc <= 1 Then
                                strPrefix = PeptideInfoClass.PROTEIN_TERMINUS_SYMBOL
                            Else
                                strPrefix = strProteinSequence.Substring(intResidueStartLoc - 2, 1)
                            End If

                            If intResidueEndLoc >= intProteinSequenceLength Then
                                strSuffix = PeptideInfoClass.PROTEIN_TERMINUS_SYMBOL
                            Else
                                strSuffix = strProteinSequence.Substring(intResidueEndLoc, 1)
                            End If

                            .PrefixResidue = strPrefix
                            .SuffixResidue = strSuffix
                        Else
                            .PrefixResidue = PeptideInfoClass.PROTEIN_TERMINUS_SYMBOL
                            .SuffixResidue = PeptideInfoClass.PROTEIN_TERMINUS_SYMBOL
                        End If


                        If objDigestionOptions.CleavageRuleID = CleavageRuleConstants.ConventionalTrypsin Then
                            .PeptideName = "t" & (intTrypticIndex + 1).ToString & "." & (intMissedCleavageCount + 1).ToString
                        Else
                            .PeptideName = (intResidueStartLoc).ToString & "." & (intResidueEndLoc).ToString
                        End If
                    End If
                End If
            End With

            If blnAddFragment Then
                intFragmentCountTotal += 1
                If intFragmentCountTotal >= objPeptideFragments.Length Then
                    ReDim Preserve objPeptideFragments(objPeptideFragments.Length * 2 - 1)
                End If
            End If
        End If

        Return blnAddFragment
    End Function

    Private Sub ReportError(ByVal strFunctionName As String, ByVal ex As Exception)
        Try
            Dim strErrorMessage As String

            If Not strFunctionName Is Nothing AndAlso strFunctionName.Length > 0 Then
                strErrorMessage = "Error in " & strFunctionName & ": " & ex.Message
            Else
                strErrorMessage = "Error: " & ex.Message
            End If

            Console.WriteLine(strErrorMessage)

            If mShowMessages Then
                MsgBox(strErrorMessage, MsgBoxStyle.Exclamation Or MsgBoxStyle.OKOnly, "Error")
            Else
                Debug.Assert(False, strErrorMessage)
            End If

        Catch exNew As Exception
            ' Ignore errors here
        End Try
    End Sub

#Region "Peptide Info class"

    Public Class PeptideInfoClass
        Inherits PeptideSequenceClass

        ' Adds NET computation to the PeptideSequenceClass

        Public Sub New()

            If objNETPrediction Is Nothing Then
#If IncludePNNLNETRoutines Then
                objNETPrediction = New NETPrediction.ElutionTimePredictionKangas
#Else
                objNETPrediction = New NETPredictionBasic.ElutionTimePredictionKrokhin
#End If
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
#If IncludePNNLNETRoutines Then
        Private Shared objNETPrediction As NETPrediction.iPeptideElutionTime
#Else
        Private Shared objNETPrediction As NETPredictionBasic.iPeptideElutionTime
#End If


        Private mAutoComputeNET As Boolean      ' Set to False to skip computation of NET when Sequence changes; useful for speeding up things a little

        Private mPeptideName As String
        Private mNET As Single
        Private mPrefixResidue As String
        Private mSuffixResidue As String

#End Region

#Region "Processing Options Interface Functions"
        Public Property AutoComputeNET() As Boolean
            Get
                Return mAutoComputeNET
            End Get
            Set(ByVal Value As Boolean)
                mAutoComputeNET = Value
            End Set
        End Property

        Public Property PeptideName() As String
            Get
                Return mPeptideName
            End Get
            Set(ByVal Value As String)
                mPeptideName = Value
            End Set
        End Property

        Public ReadOnly Property NET() As Single
            Get
                Return mNET
            End Get
        End Property

        Public Property PrefixResidue() As String
            Get
                Return mPrefixResidue
            End Get
            Set(ByVal Value As String)
                If Not Value Is Nothing AndAlso Value.Length > 0 Then
                    mPrefixResidue = Value.Chars(0)
                Else
                    mPrefixResidue = PROTEIN_TERMINUS_SYMBOL
                End If
            End Set
        End Property

        Public Property SequenceOneLetter() As String
            Get
                Return MyBase.GetSequence(False)
            End Get
            Set(ByVal Value As String)
                MyClass.SetSequence(Value, PeptideSequenceClass.NTerminusGroupConstants.Hydrogen, PeptideSequenceClass.CTerminusGroupConstants.Hydroxyl, False)
            End Set
        End Property

        Public ReadOnly Property SequenceWithPrefixAndSuffix() As String
            Get
                Return mPrefixResidue & "." & MyClass.SequenceOneLetter & "." & mSuffixResidue
            End Get
        End Property

        Public Property SuffixResidue() As String
            Get
                Return mSuffixResidue
            End Get
            Set(ByVal Value As String)
                If Not Value Is Nothing AndAlso Value.Length > 0 Then
                    mSuffixResidue = Value.Chars(0)
                Else
                    mSuffixResidue = PROTEIN_TERMINUS_SYMBOL
                End If
            End Set
        End Property
#End Region

        Public Overrides Function SetSequence(ByVal strSequence As String, Optional ByVal eNTerminus As NTerminusGroupConstants = NTerminusGroupConstants.Hydrogen, Optional ByVal eCTerminus As CTerminusGroupConstants = CTerminusGroupConstants.Hydroxyl, Optional ByVal blnIs3LetterCode As Boolean = False, Optional ByVal bln1LetterCheckForPrefixAndSuffixResidues As Boolean = True, Optional ByVal bln3LetterCheckForPrefixHandSuffixOH As Boolean = True) As Integer
            Dim intReturn As Integer

            intReturn = MyBase.SetSequence(strSequence, eNTerminus, eCTerminus, blnIs3LetterCode, bln1LetterCheckForPrefixAndSuffixResidues, bln3LetterCheckForPrefixHandSuffixOH)
            If mAutoComputeNET Then UpdateNET()

            Return intReturn
        End Function

        Public Overrides Sub SetSequenceOneLetterCharactersOnly(ByVal strSequenceNoPrefixOrSuffix As String)
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


        Public Property AminoAcidResidueFilterChars() As Char()
            Get
                Return mAminoAcidResidueFilterChars
            End Get
            Set(ByVal Value As Char())
                mAminoAcidResidueFilterChars = Value
            End Set
        End Property

        Public Property MaxMissedCleavages() As Integer
            Get
                Return mMaxMissedCleavages
            End Get
            Set(ByVal Value As Integer)
                If Value < 0 Then Value = 0
                If Value > 500000 Then Value = 500000
                mMaxMissedCleavages = Value
            End Set
        End Property

        Public Property CleavageRuleID() As clsInSilicoDigest.CleavageRuleConstants
            Get
                Return mCleavageRuleID
            End Get
            Set(ByVal Value As clsInSilicoDigest.CleavageRuleConstants)
                mCleavageRuleID = Value
            End Set
        End Property

        Public Property MinFragmentResidueCount() As Integer
            Get
                Return mMinFragmentResidueCount
            End Get
            Set(ByVal Value As Integer)
                If Value < 1 Then Value = 1
                mMinFragmentResidueCount = Value
            End Set
        End Property

        Public Property MinFragmentMass() As Integer
            Get
                Return mMinFragmentMass
            End Get
            Set(ByVal Value As Integer)
                If Value < 0 Then Value = 0
                mMinFragmentMass = Value
            End Set
        End Property

        Public Property MaxFragmentMass() As Integer
            Get
                Return mMaxFragmentMass
            End Get
            Set(ByVal Value As Integer)
                If Value < 0 Then Value = 0
                mMaxFragmentMass = Value
            End Set
        End Property

        Public Property MinIsoelectricPoint() As Single
            Get
                Return mMinIsoelectricPoint
            End Get
            Set(ByVal Value As Single)
                mMinIsoelectricPoint = Value
            End Set
        End Property

        Public Property MaxIsoelectricPoint() As Single
            Get
                Return mMaxIsoelectricPoint
            End Get
            Set(ByVal Value As Single)
                mMaxIsoelectricPoint = Value
            End Set
        End Property

        Public Property RemoveDuplicateSequences() As Boolean
            Get
                Return mRemoveDuplicateSequences
            End Get
            Set(ByVal Value As Boolean)
                mRemoveDuplicateSequences = Value
            End Set
        End Property

        Public Property IncludePrefixAndSuffixResidues() As Boolean
            Get
                Return mIncludePrefixAndSuffixResidues
            End Get
            Set(ByVal Value As Boolean)
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

