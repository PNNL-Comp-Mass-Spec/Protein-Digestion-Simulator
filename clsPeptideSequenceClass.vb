Option Strict On
'
' This class can be used to calculate the mass of an amino acid sequence (peptide or protein)
' The protein must be in one letter abbreviation format
'
' Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in October 2004
' Copyright 2005, Battelle Memorial Institute.  All Rights Reserved.
'
' Last Modified April 13, 2007
'

Public Class PeptideSequenceClass

    Public Sub New()
        If Not mSharedArraysInitialized Then
            mCurrentElementMode = ElementModeConstants.IsotopicMass
            InitializeSharedData()
            mSharedArraysInitialized = True
        End If

        mResidues = String.Empty
    End Sub

#Region "Constants and Enums"
    Private Const UNKNOWN_SYMBOL_ONE_LETTER As Char = "X"c
    Private Const UNKNOWN_SYMBOL_THREE_LETTERS As String = "Xxx"

    Private Const TERMINII_SYMBOL As Char = "-"c
    Private Const TRYPTIC_RULE_RESIDUES As String = "KR"
    Private Const TRYPTIC_EXCEPTION_RESIDUES As String = "P"

    Public Enum CTerminusGroupConstants
        Hydroxyl = 0
        Amide = 1
        None = 2
    End Enum

    Public Enum NTerminusGroupConstants
        Hydrogen = 0
        HydrogenPlusProton = 1
        Acetyl = 2
        PyroGlu = 3
        Carbamyl = 4
        PTC = 5
        None = 6
    End Enum

    Public Enum ElementModeConstants
        AverageMass = 0
        IsotopicMass = 1
    End Enum
#End Region

#Region "Structures"
    Private Structure udtTerminusType
        Public Formula As String
        Public Mass As Double
        Public MassElementMode As ElementModeConstants
        Public PrecedingResidue As String      ' If the peptide sequence is part of a protein, the user can record the final residue of the previous peptide sequence here
        Public FollowingResidue As String      ' If the peptide sequence is part of a protein, the user can record the first residue of the next peptide sequence here
    End Structure
#End Region

#Region "Shared Classwide Variables"
    ' Variables shared across all instances of this class
    Private Shared mSharedArraysInitialized As Boolean
    Private Shared AminoAcidMasses As Hashtable     ' Hash of one letter symbols and corresponding mass for each
    Private Shared AminoAcidSymbols As Hashtable    ' Hash of one letter symbols and corresponding three letter symbol for each

    Private Shared ElementMasses As Hashtable       ' Hash of one letter element symbols and corresponding mass for each

    Private Shared mCurrentElementMode As ElementModeConstants
    Private Shared mHydrogenMass As Double           ' Mass of hydrogen
    Private Shared mChargeCarrierMass As Double      ' H minus one electron
#End Region

#Region "Classwide Variables"
    Private mResidues As String

    Private mNTerminus As udtTerminusType       ' Formula on the N-Terminus
    Private mCTerminus As udtTerminusType       ' Formula on the C-Terminus

    Private mTotalMass As Double
    Private mTotalMassElementMode As ElementModeConstants

    Private mDelayUpdateResidueMass As Boolean
#End Region

#Region "Processing Options Interface Functions"

    Public Property ElementMode() As ElementModeConstants
        Get
            Return mCurrentElementMode
        End Get
        Set(ByVal Value As ElementModeConstants)
            mCurrentElementMode = Value
            InitializeSharedData()
            UpdateSequenceMass()
        End Set
    End Property

    Public ReadOnly Property Mass() As Double
        Get
            If mTotalMassElementMode <> mCurrentElementMode Then UpdateSequenceMass()
            Return mTotalMass
        End Get
    End Property
#End Region

    Private Sub AddAminoAcidStatEntry(ByVal strSymbolOneLetter As Char, ByVal strSymbolThreeLetter As String, ByVal strFormula As String)
        ' Adds new entry to AminoAcidMasses and AminoAcidSymbols
        ' Note: strFormula can only contain C, H, N, O, S, or P, and cannot contain any parentheses or other advanced formula features

        Dim dblMass As Double

        If AminoAcidMasses.ContainsKey(strSymbolOneLetter) Then
            AminoAcidMasses.Remove(strSymbolOneLetter)
        End If

        dblMass = ComputeFormulaWeightCHNOSP(strFormula)
        AminoAcidMasses.Add(strSymbolOneLetter, dblMass)

        ' Uncomment this to create a file named "AminoAcidMasses.txt" containing the amino acid symbols and masses
        ''Dim srOutFile As New System.IO.StreamWriter("AminoAcidMasses.txt", True)
        ''srOutFile.WriteLine(strSymbolOneLetter & vbTab & dblMass.ToString("0.000000"))
        ''srOutFile.Close()

        AminoAcidSymbols.Add(strSymbolOneLetter, strSymbolThreeLetter)

    End Sub

    Private Function CheckForAndRemovePrefixAndSuffixResidues(ByVal strSequence As String, Optional ByRef strPrefix As String = "", Optional ByRef strSuffix As String = "") As String
        ' This function is only applicable for sequences in one-letter notation

        strPrefix = String.Empty
        strSuffix = String.Empty

        ' First look if sequence is in the form A.BCDEFG.Z or -.BCDEFG.Z or A.BCDEFG.-
        ' If so, then need to strip out the preceding A and Z residues since they aren't really part of the sequence
        If strSequence.Length > 1 And strSequence.IndexOf(".") >= 0 Then
            If strSequence.Chars(1) = "." AndAlso strSequence.Length > 2 Then
                strPrefix = strSequence.Chars(0)
                strSequence = strSequence.Substring(2)
            End If

            If strSequence.Length > 2 AndAlso strSequence.Chars(strSequence.Length - 2) = "." Then
                strPrefix = strSequence.Chars(strSequence.Length - 1)
                strSequence = strSequence.Substring(0, strSequence.Length - 2)
            End If

            ' Also check for starting with a . or ending with a .
            If strSequence.Length > 0 AndAlso strSequence.Chars(0) = "." Then
                strSequence = strSequence.Substring(1)
            End If

            If strSequence.Length > 0 AndAlso strSequence.Chars(strSequence.Length - 1) = "." Then
                strSequence = strSequence.Substring(0, strSequence.Length - 1)
            End If
        End If

        Return strSequence
    End Function

    ' Set blnReversedCleavageDirection to True to cleave on the N-terminal side of  strRuleResidues
    Public Function CheckSequenceAgainstCleavageRule( _
                                        ByVal strSequence As String, _
                                        ByVal strRuleResidues As String, _
                                        ByVal strExceptionResidues As String, _
                                        ByVal blnReversedCleavageDirection As Boolean, _
                                        ByVal blnAllowPartialCleavage As Boolean, _
                                        Optional ByVal strSeparationChar As String = ".", _
                                        Optional ByVal chTerminiiSymbol As Char = TERMINII_SYMBOL, _
                                        Optional ByVal blnIgnoreCase As Boolean = True, _
                                        Optional ByRef intRuleMatchCount As Integer = 0) As Boolean

        ' Checks strSequence to see if it matches the cleavage rule
        ' Returns True if valid, False if invalid
        ' Returns True if doesn't contain any periods, and thus, can't be examined
        ' The ByRef variable intRuleMatchCount can be used to retrieve the number of ends that matched the rule (0, 1, or 2); terminii are counted as rule matches

        ' The residues in strRuleResidues specify the cleavage rule
        ' Both the prefix residue and the residue at the end of the sequence are tested against strRuleResidues and strExceptionResidues
        ' If blnReversedCleavageDirection is True, then the residue testing order is reversed

        ' For example, if strRuleResidues = "KR", strExceptionResidues = "P", and blnReversedCleavageDirection = False
        ' Then if strSequence = "R.AEQDDLANYGPGNGVLPSAGSSISMEK.L" then blnMatchesCleavageRule = True
        ' However, if strSequence = "R.IGASGEHIFIIGVDK.P" then blnMatchesCleavageRule = False since chSuffix = "P"
        ' Finally, if strSequence = "R.IGASGEHIFIIGVDKPNR.Q" then blnMatchesCleavageRule = True since K is ignored, but the final R.Q is valid

        Dim chSequenceStart As Char, chSequenceEnd As Char
        Dim chPrefix As Char
        Dim chSuffix As Char
        Dim blnMatchesCleavageRule As Boolean, blnSkipThisEnd As Boolean
        Dim chTestResidue As Char
        Dim chExceptionResidue As Char

        Dim intPeriodLoc1 As Integer
        Dim intPeriodLoc2 As Integer

        Dim intEndToCheck As Integer
        Dim intTerminusCount As Integer

        ' Need to reset this to zero since passed ByRef
        intRuleMatchCount = 0
        intTerminusCount = 0

        ' First, make sure the sequence is in the form A.BCDEFG.H or A.BCDEFG or BCDEFG.H
        ' If it isn't, then we can't check it (we'll return true)

        If strRuleResidues Is Nothing OrElse strRuleResidues.Length = 0 Then
            ' No rules
            Return True
        End If

        intPeriodLoc1 = strSequence.IndexOf(strSeparationChar)
        If intPeriodLoc1 < 0 Then
            ' No periods, can't check
            Debug.Assert(False, "CheckSequenceAgainstCleavageRule called with a sequence that doesn't contain prefix or suffix separation characters")
            Return True
        Else
            intPeriodLoc2 = strSequence.IndexOf(strSeparationChar, intPeriodLoc1 + 1)
        End If

        If blnIgnoreCase Then
            strSequence = strSequence.ToUpper
        End If

        ' Find the prefix residue and starting residue
        ' 
        If intPeriodLoc1 >= 1 Then
            chPrefix = strSequence.Chars(intPeriodLoc1 - 1)
            chSequenceStart = strSequence.Chars(intPeriodLoc1 + 1)
        Else
            ' intPeriodLoc1 must be 0; assume we're at the protein terminus
            chPrefix = chTerminiiSymbol
            chSequenceStart = strSequence.Chars(intPeriodLoc1 + 1)
        End If

        If intPeriodLoc2 > intPeriodLoc1 + 1 And intPeriodLoc2 <= strSequence.Length - 2 Then
            chSuffix = strSequence.Chars(intPeriodLoc2 - 1)
            chSequenceStart = strSequence.Chars(intPeriodLoc2 + 1)
        ElseIf intPeriodLoc1 > 1 Then
            chSequenceEnd = strSequence.Chars(strSequence.Length - 1)
        End If

        If strRuleResidues = chTerminiiSymbol Then
            ' Peptide database rules
            ' See if prefix and suffix are both "" or are both chTerminiiSymbol
            If (chPrefix = chTerminiiSymbol AndAlso chSuffix = chTerminiiSymbol) OrElse _
               (chPrefix = Nothing AndAlso chSuffix = Nothing) Then
                intRuleMatchCount = 2
                blnMatchesCleavageRule = True
            Else
                blnMatchesCleavageRule = False
            End If
        Else
            If blnIgnoreCase Then
                strRuleResidues = strRuleResidues.ToUpper
            End If

            ' Test both chPrefix and chSequenceEnd against strRuleResidues
            ' Make sure chSuffix does not match strExceptionResidues
            For intEndToCheck = 0 To 1
                blnSkipThisEnd = False
                If intEndToCheck = 0 Then
                    ' N terminus
                    If chPrefix = chTerminiiSymbol Then
                        intTerminusCount += 1
                        blnSkipThisEnd = True
                    Else
                        If blnReversedCleavageDirection Then
                            chTestResidue = chSequenceStart
                            chExceptionResidue = chPrefix
                        Else
                            chTestResidue = chPrefix
                            chExceptionResidue = chSequenceStart
                        End If
                    End If
                Else
                    ' C terminus
                    If chSuffix = chTerminiiSymbol Then
                        intTerminusCount += 1
                        blnSkipThisEnd = True
                    Else
                        If blnReversedCleavageDirection Then
                            chTestResidue = chSuffix
                            chExceptionResidue = chSequenceEnd
                        Else
                            chTestResidue = chSequenceEnd
                            chExceptionResidue = chSuffix
                        End If
                    End If
                End If

                If Not blnSkipThisEnd Then
                    If CheckSequenceAgainstCleavageRuleMatchTestResidue(chTestResidue, strRuleResidues) Then
                        ' Match found
                        ' See if chExceptionResidue matches any of the exception residues
                        If Not CheckSequenceAgainstCleavageRuleMatchTestResidue(chExceptionResidue, strExceptionResidues) Then
                            intRuleMatchCount += 1
                        End If
                    End If
                End If

            Next intEndToCheck

            If intTerminusCount = 2 Then
                ' Both ends of the peptide are terminii; label as fully matching the rules and set RuleMatchCount to 2
                blnMatchesCleavageRule = True
                intRuleMatchCount = 2
            ElseIf intTerminusCount = 1 Then
                ' One end was a terminus; can either be fully cleaved or non-cleaved; never partially cleavaged
                If intRuleMatchCount >= 1 Then
                    blnMatchesCleavageRule = True
                    intRuleMatchCount = 2
                End If
            Else
                If intRuleMatchCount = 2 Then
                    blnMatchesCleavageRule = True
                ElseIf intRuleMatchCount >= 1 AndAlso blnAllowPartialCleavage Then
                    blnMatchesCleavageRule = True
                End If
            End If

        End If

        Return blnMatchesCleavageRule

    End Function

    Private Function CheckSequenceAgainstCleavageRuleMatchTestResidue(ByVal strTestResidue As Char, ByVal strRuleResidues As String) As Boolean
        ' Checks to see if strTestResidue matches one of the residues in strRuleResidues
        ' Used to test by Rule Residues and Exception Residues

        If strRuleResidues.IndexOf(strTestResidue) >= 0 Then
            Return True
        Else
            Return False
        End If

    End Function

    Private Function ComputeFormulaWeightCHNOSP(ByVal strFormula As String) As Double
        ' Very simple mass computation utility; only considers elements C, H, N, O, S, and P
        ' Does not handle parentheses or any other advanced formula features
        ' Returns 0 if any unknown symbols are encountered

        Dim dblMass As Double = 0
        Dim intIndex As Integer
        Dim intLastElementIndex As Integer = -1
        Dim strMultiplier As String = String.Empty

        For intIndex = 0 To strFormula.Length - 1
            If Char.IsNumber(strFormula.Chars(intIndex)) Then
                strMultiplier &= strFormula.Chars(intIndex)
            Else
                If intLastElementIndex >= 0 Then
                    dblMass += ComputeFormulaWeightLookupMass(strFormula.Chars(intLastElementIndex), strMultiplier)
                    strMultiplier = String.Empty
                End If
                intLastElementIndex = intIndex
            End If
        Next

        ' Handle the final element
        If intLastElementIndex >= 0 Then
            dblMass += ComputeFormulaWeightLookupMass(strFormula.Chars(intLastElementIndex), strMultiplier)
        End If

        Return dblMass

    End Function

    Private Function ComputeFormulaWeightLookupMass(ByVal chSymbol As Char, ByVal strMultiplier As String) As Double
        Dim intMultiplier As Integer

        If strMultiplier.Length > 0 Then
            Try
                intMultiplier = Integer.Parse(strMultiplier)
            Catch ex As Exception
                ' Error converting to integer
                intMultiplier = 1
            End Try
        Else
            intMultiplier = 1
        End If

        Try
            Return CType(ElementMasses(chSymbol), Double) * intMultiplier
        Catch ex As Exception
            ' Symbol not found, or has invalid mass
            Return 0
        End Try

    End Function

    Public Function ConvertAminoAcidSequenceSymbols(ByVal strSequence As String, ByVal bln1LetterTo3Letter As Boolean, Optional ByVal blnAddSpaceEvery10Residues As Boolean = False, Optional ByVal blnSeparateResiduesWithDash As Boolean = False) As String
        ' Converts a sequence from 1 letter to 3 letter codes or vice versa

        Dim objPeptide As New PeptideSequenceClass
        Dim strPrefix As String, strSuffix As String
        Dim strSequenceOut As String

        Try
            If bln1LetterTo3Letter Then
                strSequence = CheckForAndRemovePrefixAndSuffixResidues(strSequence, strPrefix, strSuffix)
            End If
            objPeptide.SetSequence(strSequence, NTerminusGroupConstants.None, CTerminusGroupConstants.None, Not bln1LetterTo3Letter)

            strSequenceOut = objPeptide.GetSequence(bln1LetterTo3Letter, blnAddSpaceEvery10Residues, blnSeparateResiduesWithDash)

            If bln1LetterTo3Letter AndAlso (strPrefix.Length > 0 Or strSuffix.Length > 0) Then
                objPeptide.SetSequence(strPrefix, NTerminusGroupConstants.None, CTerminusGroupConstants.None, False)
                strSequenceOut = objPeptide.GetSequence(True, False, False) & "." & strSequenceOut

                objPeptide.SetSequence(strSuffix, NTerminusGroupConstants.None, CTerminusGroupConstants.None, False)
                strSequenceOut &= "." & objPeptide.GetSequence(True, False, False)
            End If

        Catch ex As Exception
            strSequenceOut = String.Empty
        End Try

        Return strSequenceOut

    End Function

    Private Function GetAminoAcidSymbolConversion(ByVal strSymbolToFind As String, ByVal bln1LetterTo3Letter As Boolean) As String
        ' If bln1LetterTo3Letter = True, then converting 1 letter codes to 3 letter codes
        ' Returns the symbol, if found
        ' Otherwise, returns ""

        Dim intIndex As Integer
        Dim strSymbol As Char

        If bln1LetterTo3Letter Then
            strSymbol = strSymbolToFind.Substring(0, 1).ToUpper.Chars(0)
            If AminoAcidSymbols.ContainsKey(strSymbol) Then
                Return CType(AminoAcidSymbols(strSymbol), String)
            Else
                Return String.Empty
            End If
        Else
            Dim myEnumerator As IDictionaryEnumerator = AminoAcidSymbols.GetEnumerator()
            strSymbolToFind = strSymbolToFind.ToUpper
            Do While myEnumerator.MoveNext()
                If CType(myEnumerator.Value, String).ToUpper = strSymbolToFind Then
                    Return CType(myEnumerator.Key, String)
                End If
            Loop

            ' If we get here, then the value wasn't found
            Return String.Empty
        End If

    End Function

    Public Function GetResidue(ByVal intResidueNumber As Integer, Optional ByVal blnUse3LetterCode As Boolean = False) As String
        ' Returns the given residue in the current sequence

        If Not mResidues Is Nothing AndAlso (intResidueNumber > 0 And intResidueNumber <= mResidues.Length) Then
            If blnUse3LetterCode Then
                Return GetAminoAcidSymbolConversion(mResidues.Chars(intResidueNumber - 1), True)
            Else
                Return mResidues.Chars(intResidueNumber - 1)
            End If
        Else
            Return String.Empty
        End If

    End Function

    Public Function GetResidueCount() As Integer
        If mResidues Is Nothing Then
            Return 0
        Else
            Return mResidues.Length
        End If
    End Function

    Public Function GetResidueCountSpecificResidue(ByVal strResidueSymbol As String, Optional ByVal blnUse3LetterCode As Boolean = False) As Integer
        ' Returns the number of occurrences of the given residue in the loaded sequence
        ' Returns -1 if an error

        Dim strSearchResidue1Letter As Char
        Dim intResidueCount As Integer
        Dim intResidueIndex As Integer

        If mResidues Is Nothing OrElse mResidues.Length = 0 Then Return 0

        Try
            If blnUse3LetterCode Then
                strSearchResidue1Letter = GetAminoAcidSymbolConversion(strResidueSymbol, False).Chars(0)
            Else
                strSearchResidue1Letter = strResidueSymbol.Chars(0)
            End If
        Catch ex As Exception
            Return -1
        End Try

        intResidueIndex = -1
        intResidueCount = 0
        Do
            intResidueIndex = mResidues.IndexOf(strSearchResidue1Letter, intResidueIndex + 1)
            If intResidueIndex >= 0 Then
                intResidueCount += 1
            Else
                Exit Do
            End If
        Loop

        GetResidueCountSpecificResidue = intResidueCount
    End Function

    Public Function GetSequence(Optional ByVal blnUse3LetterCode As Boolean = False, Optional ByVal blnAddSpaceEvery10Residues As Boolean = False, Optional ByVal blnSeparateResiduesWithDash As Boolean = False, Optional ByVal blnIncludeNandCTerminii As Boolean = False) As String
        ' Construct a text sequence using Residues() and the N and C Terminus info

        Dim strSequence As String, strSymbol3Letter As String
        Dim strDashAdd As String
        Dim intIndex As Integer
        Dim intLastIndex As Integer

        If mResidues Is Nothing Then Return String.Empty

        If Not blnUse3LetterCode And Not blnAddSpaceEvery10Residues And Not blnSeparateResiduesWithDash Then
            ' Simply return the sequence, possibly with the N and C terminii
            strSequence = mResidues
        Else

            If blnSeparateResiduesWithDash Then strDashAdd = "-" Else strDashAdd = String.Empty

            strSequence = String.Empty
            intLastIndex = mResidues.Length - 1
            For intIndex = 0 To intLastIndex

                If blnUse3LetterCode Then
                    strSymbol3Letter = GetAminoAcidSymbolConversion(mResidues.Chars(intIndex), True)
                    If strSymbol3Letter = "" Then strSymbol3Letter = UNKNOWN_SYMBOL_THREE_LETTERS
                    strSequence &= strSymbol3Letter
                Else
                    strSequence &= mResidues.Chars(intIndex)
                End If

                If intIndex < intLastIndex Then
                    If blnAddSpaceEvery10Residues Then
                        If (intIndex + 1) Mod 10 = 0 Then
                            strSequence &= " "
                        Else
                            strSequence &= strDashAdd
                        End If
                    Else
                        strSequence &= strDashAdd
                    End If
                End If

            Next intIndex

        End If

        If blnIncludeNandCTerminii Then
            strSequence = mNTerminus.Formula & strDashAdd & strSequence & strDashAdd & mCTerminus.Formula
        End If

        GetSequence = strSequence
    End Function

    Public Function GetSequenceMass() As Double
        Return Me.Mass
    End Function

    Public Function GetTrypticName(ByVal strProteinResidues As String, ByVal strPeptideResidues As String, Optional ByRef intReturnResidueStart As Integer = 0, Optional ByRef intReturnResidueEnd As Integer = 0, Optional ByVal blnICR2LSCompatible As Boolean = False, Optional ByVal strRuleResidues As String = TRYPTIC_RULE_RESIDUES, Optional ByVal strExceptionResidues As String = TRYPTIC_EXCEPTION_RESIDUES, Optional ByVal blnReversedCleavageDirection As Boolean = False, Optional ByVal chTerminiiSymbol As Char = TERMINII_SYMBOL, Optional ByVal blnIgnoreCase As Boolean = True, Optional ByVal intProteinSearchStartLoc As Integer = 1) As String
        ' Examines strPeptideResidues to see where they exist in strProteinResidues
        ' Constructs a name string based on their position and based on whether the fragment is truly tryptic
        ' In addition, returns the position of the first and last residue in intReturnResidueStart and intReturnResidueEnd
        ' The tryptic name in the following format
        ' t1  indicates tryptic peptide 1
        ' t2 represents tryptic peptide 2, etc.
        ' t1.2  indicates tryptic peptide 1, plus one more tryptic peptide, i.e. t1 and t2
        ' t5.2  indicates tryptic peptide 5, plus one more tryptic peptide, i.e. t5 and t6
        ' t5.3  indicates tryptic peptide 5, plus two more tryptic peptides, i.e. t5, t6, and t7
        ' 40.52  means that the residues are not tryptic, and simply range from residue 40 to 52
        ' If the peptide residues are not present in strProteinResidues, then returns ""
        ' Since a peptide can occur multiple times in a protein, one can set intProteinSearchStartLoc to a value larger than 1 to ignore previous hits

        ' If blnICR2LSCompatible is True, then the values returned when a peptide is not tryptic are modified to
        ' range from the starting residue, to the ending residue +1
        ' intReturnResidueEnd is always equal to the position of the final residue, regardless of blnICR2LSCompatible

        ' For example, if strProteinResidues = "IGKANR"
        ' Then when strPeptideResidues = "IGK", the TrypticName is t1
        ' Then when strPeptideResidues = "ANR", the TrypticName is t2
        ' Then when strPeptideResidues = "IGKANR", the TrypticName is t1.2
        ' Then when strPeptideResidues = "IG", the TrypticName is 1.2
        ' Then when strPeptideResidues = "KANR", the TrypticName is 3.6
        ' Then when strPeptideResidues = "NR", the TrypticName is 5.6

        ' However, if blnICR2LSCompatible = True, then the last three are changed to:
        ' Then when strPeptideResidues = "IG", the TrypticName is 1.3
        ' Then when strPeptideResidues = "KANR", the TrypticName is 3.7
        ' Then when strPeptideResidues = "NR", the TrypticName is 5.7

        Dim intStartLoc As Integer, intEndLoc As Integer        ' Residue numbers, ranging from 1 to length of the protein
        Dim strTrypticName As String
        Dim strPrefix As String, strSuffix As String
        Dim strResidueFollowingSearchResidues As String
        Dim blnMatchesCleavageRule As Boolean

        Dim intTrypticResidueNumber As Integer
        Dim intRuleResidueMatchCount As Integer
        Dim intRuleResidueLoc As Integer
        Dim strProteinResiduesBeforeStartLoc As String
        Dim intPeptideResiduesLength As Integer

        If blnIgnoreCase Then
            strProteinResidues = strProteinResidues.ToUpper
            strPeptideResidues = strPeptideResidues.ToUpper
        End If

        If intProteinSearchStartLoc <= 1 Then
            intStartLoc = strProteinResidues.IndexOf(strPeptideResidues) + 1
        Else
            intStartLoc = strProteinResidues.Substring(intProteinSearchStartLoc + 1).IndexOf(strPeptideResidues) + 1
            If intStartLoc > 0 Then
                intStartLoc = intStartLoc + intProteinSearchStartLoc - 1
            End If
        End If

        intPeptideResiduesLength = strPeptideResidues.Length

        If intStartLoc > 0 And strProteinResidues.Length > 0 And intPeptideResiduesLength > 0 Then
            intEndLoc = intStartLoc + intPeptideResiduesLength - 1

            ' Determine if the residue is tryptic
            ' Use CheckSequenceAgainstCleavageRule() for this
            If intStartLoc > 1 Then
                strPrefix = strProteinResidues.Chars(intStartLoc - 2)
            Else
                strPrefix = chTerminiiSymbol
            End If

            If intEndLoc = strProteinResidues.Length Then
                strSuffix = chTerminiiSymbol
            Else
                strSuffix = strProteinResidues.Chars(intEndLoc)
            End If

            blnMatchesCleavageRule = CheckSequenceAgainstCleavageRule(strPrefix & "." & strPeptideResidues & "." & strSuffix, strRuleResidues, strExceptionResidues, blnReversedCleavageDirection, False, "."c, chTerminiiSymbol, blnIgnoreCase)

            If blnMatchesCleavageRule Then
                ' Construct strTrypticName

                ' Determine which tryptic residue strPeptideResidues is
                If intStartLoc = 1 Then
                    intTrypticResidueNumber = 1
                Else
                    intTrypticResidueNumber = 0
                    strProteinResiduesBeforeStartLoc = strProteinResidues.Substring(0, intStartLoc - 1)
                    strResidueFollowingSearchResidues = strPeptideResidues.Chars(0)
                    intTrypticResidueNumber = 0
                    intRuleResidueLoc = 0
                    Do
                        intRuleResidueLoc = GetTrypticNameFindNextCleavageLoc(strProteinResiduesBeforeStartLoc, strResidueFollowingSearchResidues, intRuleResidueLoc + 1, strRuleResidues, strExceptionResidues, blnReversedCleavageDirection, chTerminiiSymbol)
                        If intRuleResidueLoc > 0 Then
                            intTrypticResidueNumber += 1
                        End If
                    Loop While intRuleResidueLoc > 0 And intRuleResidueLoc + 1 < intStartLoc
                    intTrypticResidueNumber += 1
                End If

                ' Determine number of K or R residues in strPeptideResidues
                ' Ignore K or R residues followed by Proline
                intRuleResidueMatchCount = 0
                intRuleResidueLoc = 0
                Do
                    intRuleResidueLoc = GetTrypticNameFindNextCleavageLoc(strPeptideResidues, strSuffix, intRuleResidueLoc + 1, strRuleResidues, strExceptionResidues, blnReversedCleavageDirection, chTerminiiSymbol)
                    If intRuleResidueLoc > 0 Then
                        intRuleResidueMatchCount += 1
                    End If
                Loop While intRuleResidueLoc > 0 And intRuleResidueLoc < intPeptideResiduesLength

                strTrypticName = "t" & intTrypticResidueNumber.ToString
                If intRuleResidueMatchCount > 1 Then
                    strTrypticName &= "." & Trim(Str((intRuleResidueMatchCount)))
                End If
            Else
                If blnICR2LSCompatible Then
                    strTrypticName = intStartLoc.ToString & "." & (intEndLoc + 1).ToString
                Else
                    strTrypticName = intStartLoc.ToString & "." & intEndLoc.ToString
                End If
            End If

            intReturnResidueStart = intStartLoc
            intReturnResidueEnd = intEndLoc
            Return strTrypticName
        Else
            ' Residues not found
            intReturnResidueStart = 0
            intReturnResidueEnd = 0
            Return String.Empty
        End If

    End Function

    Public Function GetTrypticNameMultipleMatches(ByVal strProteinResidues As String, ByVal strPeptideResidues As String, Optional ByRef intReturnMatchCount As Integer = 1, Optional ByRef intReturnResidueStart As Integer = 0, Optional ByRef intReturnResidueEnd As Integer = 0, Optional ByVal blnICR2LSCompatible As Boolean = False, Optional ByVal strRuleResidues As String = TRYPTIC_RULE_RESIDUES, Optional ByVal strExceptionResidues As String = TRYPTIC_EXCEPTION_RESIDUES, Optional ByVal blnReversedCleavageDirection As Boolean = False, Optional ByVal chTerminiiSymbol As Char = TERMINII_SYMBOL, Optional ByVal blnIgnoreCase As Boolean = True, Optional ByVal intProteinSearchStartLoc As Integer = 1, Optional ByVal strListDelimeter As String = ", ") As String
        ' Examines strPeptideResidues to see where they exist in strProteinResidues
        ' Looks for all possible matches, returning them as a comma separated list
        ' Returns the number of matches in intReturnMatchCount
        ' intReturnResidueStart contains the residue number of the start of the first match
        ' intReturnResidueEnd contains the residue number of the end of the last match

        ' See GetTrypticName for additional information

        Dim strNameList As String
        Dim strCurrentName As String
        Dim intCurrentSearchLoc As Integer
        Dim intCurrentResidueStart As Integer, intCurrentResidueEnd As Integer

        intCurrentSearchLoc = intProteinSearchStartLoc
        intReturnMatchCount = 0
        strNameList = String.Empty
        Do
            strCurrentName = GetTrypticName(strProteinResidues, strPeptideResidues, intCurrentResidueStart, intCurrentResidueEnd, blnICR2LSCompatible, strRuleResidues, strExceptionResidues, blnReversedCleavageDirection, chTerminiiSymbol, blnIgnoreCase, intCurrentSearchLoc)

            If strCurrentName.Length > 0 Then
                If strNameList.Length > 0 Then
                    strNameList &= strListDelimeter
                End If
                strNameList &= strCurrentName
                intCurrentSearchLoc = intCurrentResidueEnd + 1
                intReturnMatchCount += 1

                If intReturnMatchCount = 1 Then
                    intReturnResidueStart = intCurrentResidueStart
                End If
                intReturnResidueEnd = intCurrentResidueEnd

                If intCurrentSearchLoc > Len(strProteinResidues) Then Exit Do
            Else
                Exit Do
            End If
        Loop

        Return strNameList

    End Function

    Private Function GetTrypticNameFindNextCleavageLoc(ByVal strSearchResidues As String, ByVal strResidueFollowingSearchResidues As String, ByVal intStartChar As Integer, Optional ByVal strRuleResidues As String = TRYPTIC_RULE_RESIDUES, Optional ByVal strExceptionResidues As String = TRYPTIC_EXCEPTION_RESIDUES, Optional ByVal blnReversedCleavageDirection As Boolean = False, Optional ByVal chTerminiiSymbol As Char = TERMINII_SYMBOL) As Integer
        ' Finds the location of the next strSearchChar in strSearchResidues (K or R by default)
        ' Assumes strSearchResidues are already upper case
        ' Examines the residue following the matched residue
        '   If it matches one of the characters in strExceptionResidues, then the match is not counted
        ' Note that strResidueFollowingSearchResidues is necessary in case the potential cleavage residue is the final residue in strSearchResidues
        ' We need to know the next residue to determine if it matches an exception residue
        ' For example, if strSearchResidues =      "IGASGEHIFIIGVDKPNR"
        '  and the protein it is part of is: TNSANFRIGASGEHIFIIGVDKPNRQPDS
        '  and strSearchChars = "KR while strExceptionResidues  = "P"
        ' Then the K in IGASGEHIFIIGVDKPNR is ignored because the following residue is P,
        '  while the R in IGASGEHIFIIGVDKPNR is OK because strResidueFollowingSearchResidues is Q
        ' It is the calling function's responsibility to assign the correct residue to strResidueFollowingSearchResidues
        ' If no match is found, but strResidueFollowingSearchResidues is "-", then the cleavage location returned is Len(strSearchResidues) + 1

        Dim intCharIndexInSearchChars As Integer
        Dim intCharLoc As Integer, intMinCharLoc As Integer
        Dim intExceptionSuffixResidueCount As Integer
        Dim chExceptionResidueToCheck As Char
        Dim intExceptionCharLocInSearchResidues As Integer, intCharLocViaRecursiveSearch As Integer
        Dim intNewStartCharLoc As Integer

        Dim blnMatchFound As Boolean
        Dim blnRecursiveCheck As Boolean

        If strExceptionResidues Is Nothing Then
            intExceptionSuffixResidueCount = 0
        Else
            intExceptionSuffixResidueCount = strExceptionResidues.Length
        End If

        intMinCharLoc = -1
        For intCharIndexInSearchChars = 0 To strRuleResidues.Length - 1
            blnMatchFound = False
            If blnReversedCleavageDirection Then
                ' Cleave before the matched residue
                ' Note that the CharLoc value we're storing is the location just before the cleavage point
                intCharLoc = strSearchResidues.IndexOf(strRuleResidues.Chars(intCharIndexInSearchChars), intStartChar)
            Else
                intCharLoc = strSearchResidues.IndexOf(strRuleResidues.Chars(intCharIndexInSearchChars), intStartChar - 1) + 1
            End If

            If intCharLoc >= 1 And intCharLoc >= intStartChar Then blnMatchFound = True

            If blnMatchFound Then
                If intExceptionSuffixResidueCount > 0 Then
                    If blnReversedCleavageDirection Then
                        ' Make sure the residue before the matched residue does not match strExceptionResidues
                        ' We already subtracted 1 from intCharLoc so intCharLoc is already the correct character number
                        ' Note that the above logic assures that intCharLoc is > 0
                        intExceptionCharLocInSearchResidues = intCharLoc
                        chExceptionResidueToCheck = strSearchResidues.Chars(intExceptionCharLocInSearchResidues - 1)
                    Else
                        ' Make sure strSuffixResidue does not match strExceptionResidues
                        If intCharLoc < strSearchResidues.Length Then
                            intExceptionCharLocInSearchResidues = intCharLoc + 1
                            chExceptionResidueToCheck = strSearchResidues.Chars(intExceptionCharLocInSearchResidues - 1)
                        Else
                            ' Matched the last residue in strSearchResidues
                            intExceptionCharLocInSearchResidues = strSearchResidues.Length + 1
                            chExceptionResidueToCheck = strResidueFollowingSearchResidues.Chars(0)
                        End If
                    End If

                    If strExceptionResidues.IndexOf(chExceptionResidueToCheck) >= 0 Then
                        ' Exception char matched; can't count this as the cleavage point
                        blnMatchFound = False
                        blnRecursiveCheck = False
                        If blnReversedCleavageDirection Then
                            If intCharLoc + 1 < strSearchResidues.Length Then
                                blnRecursiveCheck = True
                                intNewStartCharLoc = intCharLoc + 2
                            End If
                        Else
                            If intExceptionCharLocInSearchResidues < strSearchResidues.Length Then
                                blnRecursiveCheck = True
                                intNewStartCharLoc = intExceptionCharLocInSearchResidues
                            End If
                        End If

                        If blnRecursiveCheck Then
                            ' Recursively call this function to find the next cleavage position, using an updated intStartChar position
                            intCharLocViaRecursiveSearch = GetTrypticNameFindNextCleavageLoc(strSearchResidues, strResidueFollowingSearchResidues, intNewStartCharLoc, strRuleResidues, strExceptionResidues, blnReversedCleavageDirection, chTerminiiSymbol)

                            If intCharLocViaRecursiveSearch > 0 Then
                                ' Found a residue further along that is a valid cleavage point
                                intCharLoc = intCharLocViaRecursiveSearch
                                If intCharLoc >= 1 And intCharLoc >= intStartChar Then blnMatchFound = True
                            Else
                                intCharLoc = 0
                            End If
                        Else
                            intCharLoc = 0
                        End If
                    End If
                End If
            End If

            If blnMatchFound Then
                If intMinCharLoc < 0 Then
                    intMinCharLoc = intCharLoc
                Else
                    If intCharLoc < intMinCharLoc Then
                        intMinCharLoc = intCharLoc
                    End If
                End If
            End If
        Next intCharIndexInSearchChars

        If intMinCharLoc < 0 And strResidueFollowingSearchResidues = chTerminiiSymbol Then
            intMinCharLoc = strSearchResidues.Length + 1
        End If

        If intMinCharLoc < 0 Then
            Return 0
        Else
            Return intMinCharLoc
        End If

    End Function

    Public Function GetTrypticPeptideNext(ByVal strProteinResidues As String, ByVal intSearchStartLoc As Integer, Optional ByRef intReturnResidueStart As Integer = 0, Optional ByRef intReturnResidueEnd As Integer = 0) As String
        Return GetTrypticPeptideNext(strProteinResidues, intSearchStartLoc, intReturnResidueStart, intReturnResidueEnd, TRYPTIC_RULE_RESIDUES, TRYPTIC_EXCEPTION_RESIDUES, False)
    End Function

    Public Function GetTrypticPeptideNext(ByVal strProteinResidues As String, ByVal intSearchStartLoc As Integer, ByRef intReturnResidueStart As Integer, ByRef intReturnResidueEnd As Integer, ByVal strRuleResidues As String, ByVal strExceptionResidues As String, ByVal blnReversedCleavageDirection As Boolean, Optional ByVal chTerminiiSymbol As Char = TERMINII_SYMBOL) As String
        ' Returns the next tryptic peptide in strProteinResidues, starting the search as intSearchStartLoc
        ' Useful when obtaining all of the tryptic peptides for a protein, since this function will operate
        '  much faster than repeatedly calling GetTrypticPeptideByFragmentNumber()

        ' Returns the position of the start and end residues using intReturnResidueStart and intReturnResidueEnd

        Dim intRuleResidueLoc As Integer
        Dim intProteinResiduesLength As Integer

        If intSearchStartLoc < 1 Then intSearchStartLoc = 1

        If strProteinResidues Is Nothing Then
            intProteinResiduesLength = 0
        Else
            intProteinResiduesLength = strProteinResidues.Length
        End If

        If intSearchStartLoc > intProteinResiduesLength Then
            Return String.Empty
        End If

        intRuleResidueLoc = GetTrypticNameFindNextCleavageLoc(strProteinResidues, chTerminiiSymbol, intSearchStartLoc, strRuleResidues, strExceptionResidues, blnReversedCleavageDirection, chTerminiiSymbol)
        If intRuleResidueLoc > 0 Then
            intReturnResidueStart = intSearchStartLoc
            If intRuleResidueLoc > intProteinResiduesLength Then
                intReturnResidueEnd = intProteinResiduesLength
            Else
                intReturnResidueEnd = intRuleResidueLoc
            End If
            Return strProteinResidues.Substring(intReturnResidueStart - 1, intReturnResidueEnd - intReturnResidueStart + 1)
        Else
            intReturnResidueStart = 1
            intReturnResidueEnd = intProteinResiduesLength
            Return strProteinResidues
        End If

    End Function

    Public Function GetTrypticPeptideByFragmentNumber(ByVal strProteinResidues As String, ByVal intDesiredPeptideNumber As Integer, Optional ByRef intReturnResidueStart As Integer = 0, Optional ByRef intReturnResidueEnd As Integer = 0, Optional ByVal strRuleResidues As String = TRYPTIC_RULE_RESIDUES, Optional ByVal strExceptionResidues As String = TRYPTIC_EXCEPTION_RESIDUES, Optional ByVal blnReversedCleavageDirection As Boolean = False, Optional ByVal chTerminiiSymbol As Char = TERMINII_SYMBOL, Optional ByVal blnIgnoreCase As Boolean = True) As String
        ' Returns the desired tryptic peptide from strProteinResidues
        ' For example, if strProteinResidues = "IGKANRMTFGL" then
        '  when intDesiredPeptideNumber = 1, returns "IGK"
        '  when intDesiredPeptideNumber = 2, returns "ANR"
        '  when intDesiredPeptideNumber = 3, returns "MTFGL"

        ' Optionally, returns the position of the start and end residues
        '  using intReturnResidueStart and intReturnResidueEnd


        Dim intStartLoc As Integer, intRuleResidueLoc As Integer
        Dim intPrevStartLoc As Integer
        Dim intProteinResiduesLength As Integer
        Dim intCurrentTrypticPeptideNumber As Integer

        Dim strMatchingFragment As String

        If intDesiredPeptideNumber < 1 Then
            Return String.Empty
        End If

        If blnIgnoreCase Then
            strProteinResidues = strProteinResidues.ToUpper
        End If
        intProteinResiduesLength = strProteinResidues.Length

        intStartLoc = 1
        intRuleResidueLoc = 0
        intCurrentTrypticPeptideNumber = 0
        Do
            intRuleResidueLoc = GetTrypticNameFindNextCleavageLoc(strProteinResidues, chTerminiiSymbol, intStartLoc, strRuleResidues, strExceptionResidues, blnReversedCleavageDirection, chTerminiiSymbol)
            If intRuleResidueLoc > 0 Then
                intCurrentTrypticPeptideNumber += 1
                intPrevStartLoc = intStartLoc
                intStartLoc = intRuleResidueLoc + 1

                If intPrevStartLoc > intProteinResiduesLength Then
                    ' User requested a peptide number that doesn't exist
                    Return String.Empty
                End If
            Else
                ' I don't think I'll ever reach this code
                Debug.Assert(False, "Unexpected code point reached in GetTrypticPeptideByFragmentNumber")
                Exit Do
            End If
        Loop While intCurrentTrypticPeptideNumber < intDesiredPeptideNumber

        strMatchingFragment = String.Empty
        If intCurrentTrypticPeptideNumber > 0 And intPrevStartLoc > 0 Then
            If intPrevStartLoc > strProteinResidues.Length Then
                ' User requested a peptide number that is too high
                intReturnResidueStart = 0
                intReturnResidueEnd = 0
                strMatchingFragment = String.Empty
            Else
                ' Match found, find the extent of this peptide
                intReturnResidueStart = intPrevStartLoc
                If intRuleResidueLoc > intProteinResiduesLength Then
                    intReturnResidueEnd = intProteinResiduesLength
                Else
                    intReturnResidueEnd = intRuleResidueLoc
                End If
                strMatchingFragment = strProteinResidues.Substring(intPrevStartLoc - 1, intRuleResidueLoc - intPrevStartLoc + 1)
            End If
        Else
            intReturnResidueStart = 1
            intReturnResidueEnd = intProteinResiduesLength
            strMatchingFragment = strProteinResidues
        End If

        GetTrypticPeptideByFragmentNumber = strMatchingFragment

    End Function

    Private Function RemoveLeadingH(ByRef strWorkingSequence As String) As Boolean
        ' Returns True if a leading H is removed
        ' This is only applicable for sequences in 3 letter notation

        Dim str1LetterSymbol As String
        Dim blnHRemoved As Boolean

        blnHRemoved = False
        If strWorkingSequence.Substring(0, 1).ToUpper = "H" AndAlso strWorkingSequence.Length >= 4 Then
            ' If next character is not a character, then remove the H and the next character
            If Not Char.IsLetter(strWorkingSequence.Chars(1)) Then
                strWorkingSequence = strWorkingSequence.Substring(2)
                blnHRemoved = True
            Else
                ' Otherwise, see if next three characters are letters
                If Char.IsLetter(strWorkingSequence.Chars(1)) AndAlso _
                    Char.IsLetter(strWorkingSequence.Chars(2)) AndAlso _
                    Char.IsLetter(strWorkingSequence.Chars(3)) Then
                    ' Formula starts with 4 characters and the first is H, see if the first 3 characters are a valid amino acid code

                    str1LetterSymbol = GetAminoAcidSymbolConversion(strWorkingSequence.Substring(1, 3), False)

                    If str1LetterSymbol.Length = 0 Then
                        ' Doesn't start with a valid amino acid 3 letter abbreviation, so remove the initial H
                        strWorkingSequence = strWorkingSequence.Substring(1)
                        blnHRemoved = True
                    End If
                End If
            End If
        End If

        RemoveLeadingH = blnHRemoved
    End Function

    Private Function RemoveTrailingOH(ByRef strWorkingSequence As String) As Boolean
        ' Returns True if a trailing OH is removed
        ' This is only applicable for sequences in 3 letter notation

        Dim str1LetterSymbol As String
        Dim blnOHRemoved As Boolean
        Dim intStringLength As Integer

        blnOHRemoved = False
        intStringLength = strWorkingSequence.Length
        If strWorkingSequence.Substring(intStringLength - 2, 2).ToUpper = "OH" And intStringLength >= 5 Then
            ' If previous character is not a character, then remove the OH (and the character preceding)
            If Not Char.IsLetter(strWorkingSequence.Chars(intStringLength - 3)) Then
                strWorkingSequence = strWorkingSequence.Substring(0, intStringLength - 3)
                blnOHRemoved = True
            Else
                ' Otherwise, see if previous three characters are letters
                If Char.IsLetter(strWorkingSequence.Chars(intStringLength - 2)) AndAlso _
                    Char.IsLetter(strWorkingSequence.Chars(intStringLength - 3)) AndAlso _
                    Char.IsLetter(strWorkingSequence.Chars(intStringLength - 4)) Then
                    ' Formula ends with 3 characters and the last two are OH, see if the last 3 characters are a valid amino acid code

                    str1LetterSymbol = GetAminoAcidSymbolConversion(strWorkingSequence.Substring(intStringLength - 4, 3), False)

                    If str1LetterSymbol.Length = 0 Then
                        ' Doesn't end with a valid amino acid 3 letter abbreviation, so remove the trailing OH
                        strWorkingSequence = strWorkingSequence.Substring(0, intStringLength - 2)
                        blnOHRemoved = True
                    End If
                End If
            End If
        End If

        RemoveTrailingOH = blnOHRemoved

    End Function

    Private Function SetCTerminus(ByVal strFormula As String, Optional ByVal strFollowingResidue As String = "", Optional ByVal blnUse3LetterCode As Boolean = False) As Integer
        ' Note: strFormula can only contain C, H, N, O, S, or P, and cannot contain any parentheses or other advanced formula features
        ' Returns 0 if success; 1 if error

        ' Typical N terminus mods
        ' Free Acid = OH
        ' Amide = NH2

        Dim intReturn As Integer

        With mCTerminus
            .Formula = strFormula
            .Mass = ComputeFormulaWeightCHNOSP(.Formula)
            .MassElementMode = mCurrentElementMode
            If .Mass < 0 Then
                .Mass = 0
                intReturn = 1
            Else
                intReturn = 0
            End If
            .PrecedingResidue = String.Empty
            If blnUse3LetterCode AndAlso strFollowingResidue.Length > 0 Then
                .FollowingResidue = GetAminoAcidSymbolConversion(strFollowingResidue, False)
            Else
                .FollowingResidue = strFollowingResidue
            End If
        End With

        If Not mDelayUpdateResidueMass Then UpdateSequenceMass()
        Return intReturn

    End Function

    Public Function SetCTerminusGroup(ByVal eCTerminusGroup As CTerminusGroupConstants, Optional ByVal strFollowingResidue As String = "", Optional ByVal blnUse3LetterCode As Boolean = False) As Integer
        ' Returns 0 if success; 1 if error
        Dim intError As Integer

        intError = 0
        Select Case eCTerminusGroup
            Case CTerminusGroupConstants.Hydroxyl : intError = SetCTerminus("OH", strFollowingResidue, blnUse3LetterCode)
            Case CTerminusGroupConstants.Amide : intError = SetCTerminus("NH2", strFollowingResidue, blnUse3LetterCode)
            Case CTerminusGroupConstants.None : intError = SetCTerminus(String.Empty, strFollowingResidue, blnUse3LetterCode)
            Case Else : intError = 1
        End Select

        Return intError

    End Function

    Private Function SetNTerminus(ByVal strFormula As String, Optional ByVal strPrecedingResidue As String = "", Optional ByVal blnUse3LetterCode As Boolean = False) As Integer
        ' Note: strFormula can only contain C, H, N, O, S, or P, and cannot contain any parentheses or other advanced formula features
        ' Returns 0 if success; 1 if error

        ' Typical N terminus mods
        ' Hydrogen = H
        ' Acetyl = C2OH3
        ' PyroGlu = C5O2NH6
        ' Carbamyl = CONH2
        ' PTC = C7H6NS

        Dim intReturn As Integer

        With mNTerminus
            .Formula = strFormula
            .Mass = ComputeFormulaWeightCHNOSP(.Formula)
            .MassElementMode = mCurrentElementMode
            If .Mass < 0 Then
                .Mass = 0
                intReturn = 1
            Else
                intReturn = 0
            End If
            .PrecedingResidue = String.Empty
            If blnUse3LetterCode AndAlso strPrecedingResidue.Length > 0 Then
                .PrecedingResidue = GetAminoAcidSymbolConversion(strPrecedingResidue, False)
            Else
                .PrecedingResidue = strPrecedingResidue
            End If
            .FollowingResidue = String.Empty
        End With

        If Not mDelayUpdateResidueMass Then UpdateSequenceMass()
        Return intReturn

    End Function

    Public Function SetNTerminusGroup(ByVal eNTerminusGroup As NTerminusGroupConstants, Optional ByVal strPrecedingResidue As String = "", Optional ByVal blnUse3LetterCode As Boolean = False) As Integer
        ' Returns 0 if success; 1 if error
        Dim intError As Integer

        intError = 0
        Select Case eNTerminusGroup
            Case NTerminusGroupConstants.Hydrogen : intError = SetNTerminus("H", strPrecedingResidue, blnUse3LetterCode)
            Case NTerminusGroupConstants.HydrogenPlusProton : intError = SetNTerminus("HH", strPrecedingResidue, blnUse3LetterCode)
            Case NTerminusGroupConstants.Acetyl : intError = SetNTerminus("C2OH3", strPrecedingResidue, blnUse3LetterCode)
            Case NTerminusGroupConstants.PyroGlu : intError = SetNTerminus("C5O2NH6", strPrecedingResidue, blnUse3LetterCode)
            Case NTerminusGroupConstants.Carbamyl : intError = SetNTerminus("CONH2", strPrecedingResidue, blnUse3LetterCode)
            Case NTerminusGroupConstants.PTC : intError = SetNTerminus("C7H6NS", strPrecedingResidue, blnUse3LetterCode)
            Case NTerminusGroupConstants.None : intError = SetNTerminus("", strPrecedingResidue, blnUse3LetterCode)
            Case Else : intError = 1
        End Select

        Return intError

    End Function

    Public Overridable Function SetSequence(ByVal strSequence As String, Optional ByVal eNTerminus As NTerminusGroupConstants = NTerminusGroupConstants.Hydrogen, Optional ByVal eCTerminus As CTerminusGroupConstants = CTerminusGroupConstants.Hydroxyl, Optional ByVal blnIs3LetterCode As Boolean = False, Optional ByVal bln1LetterCheckForPrefixAndSuffixResidues As Boolean = True, Optional ByVal bln3LetterCheckForPrefixHandSuffixOH As Boolean = True) As Integer
        ' If blnIs3LetterCode = false, then look for sequence of the form: R.ABCDEF.R
        ' If found, remove the leading and ending residues since these aren't for this peptide
        ' Returns 0 if success or 1 if an error
        ' Will return 0 even in strSequence is blank or if it contains no valid residues

        Dim intSequenceStrLength As Integer, intIndex As Integer
        Dim str1LetterSymbol As String

        mResidues = String.Empty

        strSequence = strSequence.Trim
        intSequenceStrLength = strSequence.Length
        If intSequenceStrLength = 0 Then
            UpdateSequenceMass()
            Return 0
        End If

        If Not blnIs3LetterCode Then
            ' Sequence is 1 letter codes

            If bln1LetterCheckForPrefixAndSuffixResidues Then
                strSequence = CheckForAndRemovePrefixAndSuffixResidues(strSequence)
                intSequenceStrLength = strSequence.Length
            End If

            ' Now parse the string of 1 letter characters
            For intIndex = 0 To intSequenceStrLength - 1
                If Char.IsLetter(strSequence.Chars(intIndex)) Then
                    ' Character found
                    mResidues &= strSequence.Chars(intIndex)
                Else
                    ' Ignore anything else
                End If
            Next intIndex

        Else
            ' Sequence is 3 letter codes
            If bln3LetterCheckForPrefixHandSuffixOH Then
                ' Look for a leading H or trailing OH, provided those don't match any of the amino acids
                RemoveLeadingH(strSequence)
                RemoveTrailingOH(strSequence)

                ' Recompute sequence length
                intSequenceStrLength = strSequence.Length
            End If

            intIndex = 0
            Do While intIndex <= intSequenceStrLength - 3
                If Char.IsLetter(strSequence.Chars(intIndex)) Then
                    If Char.IsLetter(strSequence.Chars(intIndex + 1)) And _
                       Char.IsLetter(strSequence.Chars(intIndex + 2)) Then


                        str1LetterSymbol = GetAminoAcidSymbolConversion(strSequence.Substring(intIndex, 3), False)

                        If str1LetterSymbol.Length = 0 Then
                            ' 3 letter symbol not found
                            ' Add anyway, but mark as X
                            str1LetterSymbol = UNKNOWN_SYMBOL_ONE_LETTER
                        End If

                        mResidues &= str1LetterSymbol
                        intIndex += 3
                    Else
                        ' First letter is a character, but next two are not; ignore it
                        intIndex += 1
                    End If
                Else
                    ' Ignore anything else
                    intIndex += 1
                End If
            Loop
        End If

        ' By calling SetNTerminus and SetCTerminus, the UpdateSequenceMass() Sub will also be called
        ' We don't want to compute the mass yet
        mDelayUpdateResidueMass = True
        SetNTerminusGroup(eNTerminus)
        SetCTerminusGroup(eCTerminus)

        mDelayUpdateResidueMass = False
        UpdateSequenceMass()

        SetSequence = 0

    End Function

    Public Overridable Sub SetSequenceOneLetterCharactersOnly(ByVal strSequenceNoPrefixOrSuffix As String)
        ' Updates mResidues without performing any error checking
        mResidues = strSequenceNoPrefixOrSuffix
        UpdateSequenceMass()
    End Sub

    Private Sub UpdateSequenceMass()
        ' Computes mass for each residue in mResidues
        ' Only process one letter amino acid abbreviations

        Dim dblRunningTotal As Double
        Dim blnProtonatedNTerminus As Boolean

        Dim intIndex As Integer

        If mDelayUpdateResidueMass Then Exit Sub

        If mResidues.Length = 0 Then
            dblRunningTotal = 0
        Else
            ' The N-terminus ions are the basis for the running total
            ValidateTerminusMasses()
            dblRunningTotal = mNTerminus.Mass
            If mNTerminus.Formula.ToUpper = "HH" Then
                ' ntgNTerminusGroupConstants.HydrogenPlusProton; since we add back in the proton below when computing the fragment masses,
                '  we need to subtract it out here
                ' However, we need to subtract out mHydrogenMass, and not mChargeCarrierMass since the current
                '  formula's mass was computed using two hydrogens, and not one hydrogen and one charge carrier
                blnProtonatedNTerminus = True
                dblRunningTotal = dblRunningTotal - mHydrogenMass
            End If

            For intIndex = 0 To mResidues.Length - 1
                Try
                    dblRunningTotal += CType(AminoAcidMasses(mResidues.Chars(intIndex)), Double)
                Catch ex As Exception
                    ' Skip this residue
                    Console.WriteLine("Error parsing Residue symbols in UpdateSequenceMass; " & ex.Message)
                End Try
            Next intIndex

            dblRunningTotal += mCTerminus.Mass
            If blnProtonatedNTerminus Then
                dblRunningTotal += mChargeCarrierMass
            End If
        End If

        mTotalMassElementMode = mCurrentElementMode
        mTotalMass = dblRunningTotal

    End Sub

    Private Sub UpdateStandardMasses()
        Const DEFAULT_CHARGE_CARRIER_MASS_AVG As Double = 1.00739
        Const DEFAULT_CHARGE_CARRIER_MASS_MONOISO As Double = 1.00727649

        If mCurrentElementMode = ElementModeConstants.AverageMass Then
            mChargeCarrierMass = DEFAULT_CHARGE_CARRIER_MASS_AVG
        Else
            mChargeCarrierMass = DEFAULT_CHARGE_CARRIER_MASS_MONOISO
        End If

        ' Update Hydrogen mass
        mHydrogenMass = ComputeFormulaWeightCHNOSP("H")

    End Sub

    Private Sub ValidateTerminusMasses()
        With mNTerminus
            If .MassElementMode <> mCurrentElementMode Then
                .Mass = ComputeFormulaWeightCHNOSP(.Formula)
                .MassElementMode = mCurrentElementMode
            End If
        End With

        With mCTerminus
            If .MassElementMode <> mCurrentElementMode Then
                .Mass = ComputeFormulaWeightCHNOSP(.Formula)
                .MassElementMode = mCurrentElementMode
            End If
        End With
    End Sub

    Private Sub InitializeSharedData()

        If ElementMasses Is Nothing Then
            ElementMasses = New Hashtable
        Else
            ElementMasses.Clear()
        End If

        With ElementMasses
            If mCurrentElementMode = ElementModeConstants.IsotopicMass Then
                ' Isotopic masses
                .Add("C"c, 12.0)
                .Add("H"c, 1.0078246)
                .Add("N"c, 14.003074)
                .Add("O"c, 15.994915)
                .Add("S"c, 31.972072)
                .Add("P"c, 30.973763)
            Else
                ' Average masses
                .Add("C"c, 12.0107)
                .Add("H"c, 1.00794)
                .Add("N"c, 14.00674)
                .Add("O"c, 15.9994)
                .Add("S"c, 32.066)
                .Add("P"c, 30.973761)
            End If
        End With

        If AminoAcidMasses Is Nothing Then
            AminoAcidMasses = New Hashtable
        Else
            AminoAcidMasses.Clear()
        End If

        If AminoAcidSymbols Is Nothing Then
            AminoAcidSymbols = New Hashtable
        Else
            AminoAcidSymbols.Clear()
        End If

        AddAminoAcidStatEntry("A"c, "Ala", "C3H5NO")
        AddAminoAcidStatEntry("B"c, "Bbb", "C4H6N2O2")       ' N or D
        AddAminoAcidStatEntry("C"c, "Cys", "C3H5NOS")
        AddAminoAcidStatEntry("D"c, "Asp", "C4H5NO3")
        AddAminoAcidStatEntry("E"c, "Glu", "C5H7NO3")
        AddAminoAcidStatEntry("F"c, "Phe", "C9H9NO")
        AddAminoAcidStatEntry("G"c, "Gly", "C2H3NO")
        AddAminoAcidStatEntry("H"c, "His", "C6H7N3O")
        AddAminoAcidStatEntry("I"c, "Ile", "C6H11NO")
        AddAminoAcidStatEntry("J"c, "Jjj", "C6H11NO")        ' Unknown; use mass of Ile/Leu
        AddAminoAcidStatEntry("K"c, "Lys", "C6H12N2O")
        AddAminoAcidStatEntry("L"c, "Leu", "C6H11NO")
        AddAminoAcidStatEntry("M"c, "Met", "C5H9NOS")
        AddAminoAcidStatEntry("N"c, "Asn", "C4H6N2O2")
        AddAminoAcidStatEntry("O"c, "Orn", "C5H10N2O")
        AddAminoAcidStatEntry("P"c, "Pro", "C5H7NO")
        AddAminoAcidStatEntry("Q"c, "Gln", "C5H8N2O2")
        AddAminoAcidStatEntry("R"c, "Arg", "C6H12N4O")
        AddAminoAcidStatEntry("S"c, "Ser", "C3H5NO2")
        AddAminoAcidStatEntry("T"c, "Thr", "C4H7NO2")
        AddAminoAcidStatEntry("U"c, "Gla", "C6H7NO5")
        AddAminoAcidStatEntry("V"c, "Val", "C5H9NO")
        AddAminoAcidStatEntry("W"c, "Trp", "C11H10N2O")
        AddAminoAcidStatEntry(UNKNOWN_SYMBOL_ONE_LETTER, UNKNOWN_SYMBOL_THREE_LETTERS, "C6H11NO")       ' Unknown; use mass of Ile/Leu
        AddAminoAcidStatEntry("Y"c, "Tyr", "C9H9NO2")
        AddAminoAcidStatEntry("Z"c, "Zzz", "C5H8N2O2")       ' Q or E (note that these are 0.984 Da apart)

        UpdateStandardMasses()

    End Sub

End Class
