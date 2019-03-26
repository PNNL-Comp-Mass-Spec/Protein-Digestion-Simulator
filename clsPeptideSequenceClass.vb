Option Strict On

Imports System.Runtime.InteropServices

''' <summary>
''' This class can be used to calculate the mass of an amino acid sequence (peptide or protein)
''' The protein must be in one letter abbreviation format
''' </summary>
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

    ''' <summary>
    ''' Cysteine treatment constants
    ''' </summary>
    Public Enum CysTreatmentModeConstants
        Untreated = 0
        Iodoacetamide = 1       ' +57.0215 (alkylated)
        IodoaceticAcid = 2      ' +58.0055
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
        ' ReSharper disable once NotAccessedField.Local
        Public PrecedingResidue As String      ' If the peptide sequence is part of a protein, the user can record the final residue of the previous peptide sequence here
        ' ReSharper disable once NotAccessedField.Local
        Public FollowingResidue As String      ' If the peptide sequence is part of a protein, the user can record the first residue of the next peptide sequence here
    End Structure
#End Region

#Region "Shared Classwide Variables"
    ' Variables shared across all instances of this class

    Private Shared mSharedArraysInitialized As Boolean

    ''' <summary>
    ''' One letter symbols and corresponding mass for each
    ''' </summary>
    Private Shared AminoAcidMasses As Dictionary(Of Char, Double)

    ''' <summary>
    ''' One letter symbols and corresponding three letter symbol for each
    ''' </summary>
    Private Shared AminoAcidSymbols As Dictionary(Of Char, String)

    ''' <summary>
    ''' One letter element symbols and corresponding mass for each
    ''' </summary>
    Private Shared ElementMasses As Dictionary(Of Char, Double)

    Private Shared mCurrentElementMode As ElementModeConstants

    ''' <summary>
    ''' Mass of hydrogen
    ''' </summary>
    Private Shared mHydrogenMass As Double

    ''' <summary>
    ''' Charge carrier mass: hydrogen minus one electron
    ''' </summary>
    Private Shared mChargeCarrierMass As Double

    ''' <summary>
    ''' Mass value to add for each Cys residue when CysTreatmentMode is Iodoacetamide
    ''' </summary>
    ''' <remarks>Auto-updated when mCurrentElementMode is updated</remarks>
    Private Shared mIodoacetamideMass As Double

    ''' <summary>
    ''' Mass value to add for each Cys residue when CysTreatmentMode is IodoaceticAcid
    ''' </summary>
    ''' <remarks>Auto-updated when mCurrentElementMode is updated</remarks>
    Private Shared mIodoaceticAcidMass As Double
#End Region

#Region "Classwide Variables"
    Private mResidues As String

    ''' <summary>
    ''' Formula on the N-Terminus
    ''' </summary>
    Private mNTerminus As udtTerminusType

    ''' <summary>
    ''' Formula on the C-Terminus
    ''' </summary>
    Private mCTerminus As udtTerminusType

    Private mTotalMass As Double
    Private mTotalMassElementMode As ElementModeConstants

    Private mDelayUpdateResidueMass As Boolean
#End Region

#Region "Processing Options Interface Functions"

    ''' <summary>
    ''' Charge carrier mass
    ''' </summary>
    ''' <returns></returns>
    Public Shared ReadOnly Property ChargeCarrierMass As Double
        Get
            Return mChargeCarrierMass
        End Get
    End Property

    ''' <summary>
    ''' Cysteine treatment mode
    ''' </summary>
    ''' <returns></returns>
    Public Property CysTreatmentMode As CysTreatmentModeConstants

    ''' <summary>
    ''' Element mode
    ''' </summary>
    ''' <returns></returns>
    Public Property ElementMode As ElementModeConstants
        Get
            Return mCurrentElementMode
        End Get
        Set
            mCurrentElementMode = Value
            InitializeSharedData()
            UpdateSequenceMass()
        End Set
    End Property

    ''' <summary>
    ''' Sequence mass
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Mass As Double
        Get
            If mTotalMassElementMode <> mCurrentElementMode Then UpdateSequenceMass()
            Return mTotalMass
        End Get
    End Property

#End Region

    ''' <summary>
    ''' Adds new entry to AminoAcidMasses and AminoAcidSymbols
    ''' </summary>
    ''' <param name="symbolOneLetter"></param>
    ''' <param name="symbolThreeLetter"></param>
    ''' <param name="formula">
    ''' Can only contain C, H, N, O, S, or P, with each element optionally having an integer after it
    ''' Cannot contain any parentheses or other advanced formula features
    ''' </param>
    Private Sub AddAminoAcidStatEntry(symbolOneLetter As Char, symbolThreeLetter As String, formula As String)

        Dim aminoAcidMass As Double

        If AminoAcidMasses.ContainsKey(symbolOneLetter) Then
            AminoAcidMasses.Remove(symbolOneLetter)
        End If

        aminoAcidMass = ComputeFormulaWeightCHNOSP(formula)
        AminoAcidMasses.Add(symbolOneLetter, aminoAcidMass)

        ' Uncomment this to create a file named "AminoAcidMasses.txt" containing the amino acid symbols and masses
        ''Dim writer As New System.IO.StreamWriter("AminoAcidMasses.txt", True)
        ''writer.WriteLine(symbolOneLetter & vbTab & mass.ToString("0.000000"))
        ''writer.Close()

        AminoAcidSymbols.Add(symbolOneLetter, symbolThreeLetter)

    End Sub

    ''' <summary>
    ''' Remove prefix and suffix residues
    ''' </summary>
    ''' <param name="sequence">Sequence to examine</param>
    ''' <param name="prefix">Output: prefix residue</param>
    ''' <param name="suffix">Output: suffix residue</param>
    ''' <returns></returns>
    ''' <remarks>This function is only applicable for sequences in one-letter notation</remarks>
    Private Function CheckForAndRemovePrefixAndSuffixResidues(
        sequence As String,
        <Out> Optional ByRef prefix As String = "",
        <Out> Optional ByRef suffix As String = "") As String

        prefix = String.Empty
        suffix = String.Empty

        ' First look if sequence is in the form A.BCDEFG.Z or -.BCDEFG.Z or A.BCDEFG.-
        ' If so, then need to strip out the preceding A and Z residues since they aren't really part of the sequence
        If sequence.Length > 1 And sequence.IndexOf(".", StringComparison.Ordinal) >= 0 Then
            If sequence.Chars(1) = "." AndAlso sequence.Length > 2 Then
                prefix = sequence.Chars(0)
                sequence = sequence.Substring(2)
            End If

            If sequence.Length > 2 AndAlso sequence.Chars(sequence.Length - 2) = "." Then
                prefix = sequence.Chars(sequence.Length - 1)
                sequence = sequence.Substring(0, sequence.Length - 2)
            End If

            ' Also check for starting with a . or ending with a .
            If sequence.Length > 0 AndAlso sequence.Chars(0) = "." Then
                sequence = sequence.Substring(1)
            End If

            If sequence.Length > 0 AndAlso sequence.Chars(sequence.Length - 1) = "." Then
                sequence = sequence.Substring(0, sequence.Length - 1)
            End If
        End If

        Return sequence
    End Function

    ''' <summary>
    ''' Checks a sequence to see if it matches the cleavage rule
    ''' Both the prefix residue and the residue at the end of the sequence are tested against ruleResidues and exceptionResidues
    ''' </summary>
    ''' <param name="sequence"></param>
    ''' <param name="ruleResidues">residues that specify the cleavage rule</param>
    ''' <param name="exceptionResidues"></param>
    ''' <param name="reversedCleavageDirection">When True, cleave on the N-terminal side of ruleResidues</param>
    ''' <param name="allowPartialCleavage"></param>
    ''' <param name="separationChar"></param>
    ''' <param name="terminiiSymbol"></param>
    ''' <param name="ignoreCase"></param>
    ''' <param name="ruleMatchCount">Output: the number of ends that matched the rule (0, 1, or 2); terminii are counted as rule matches</param>
    ''' <returns>True if a valid match, False if not a match</returns>
    ''' <remarks>Returns True if sequence doesn't contain any periods, and thus, can't be examined</remarks>
    Public Function CheckSequenceAgainstCleavageRule(
        sequence As String,
        ruleResidues As String,
        exceptionResidues As String,
        reversedCleavageDirection As Boolean,
        allowPartialCleavage As Boolean,
        Optional separationChar As String = ".",
        Optional terminiiSymbol As Char = TERMINII_SYMBOL,
        Optional ignoreCase As Boolean = True,
        <Out> Optional ByRef ruleMatchCount As Integer = 0) As Boolean

        ' ReSharper disable CommentTypo

        ' For example, if ruleResidues = "KR", exceptionResidues = "P", and reversedCleavageDirection = False
        ' Then if sequence = "R.AEQDDLANYGPGNGVLPSAGSSISMEK.L" then matchesCleavageRule = True
        ' However, if sequence = "R.IGASGEHIFIIGVDK.P" then matchesCleavageRule = False since suffix = "P"
        ' Finally, if sequence = "R.IGASGEHIFIIGVDKPNR.Q" then matchesCleavageRule = True since K is ignored, but the final R.Q is valid

        ' ReSharper restore CommentTypo

        Dim sequenceStart As Char, sequenceEnd As Char
        Dim prefix As Char
        Dim suffix As Char
        Dim matchesCleavageRule As Boolean, skipThisEnd As Boolean
        Dim testResidue As Char
        Dim exceptionResidue As Char

        Dim periodIndex1 As Integer
        Dim periodIndex2 As Integer

        Dim endToCheck As Integer
        Dim terminusCount As Integer

        ' Need to reset this to zero since passed ByRef
        ruleMatchCount = 0
        terminusCount = 0

        ' First, make sure the sequence is in the form A.BCDEFG.H or A.BCDEFG or BCDEFG.H
        ' If it isn't, then we can't check it (we'll return true)

        If ruleResidues Is Nothing OrElse ruleResidues.Length = 0 Then
            ' No rules
            Return True
        End If

        periodIndex1 = sequence.IndexOf(separationChar, StringComparison.Ordinal)
        If periodIndex1 < 0 Then
            ' No periods, can't check
            Console.WriteLine("CheckSequenceAgainstCleavageRule called with a sequence that doesn't contain prefix or suffix separation characters; unable to process: " & sequence)
            Return True
        Else
            periodIndex2 = sequence.IndexOf(separationChar, periodIndex1 + 1, StringComparison.Ordinal)
        End If

        If ignoreCase Then
            sequence = sequence.ToUpper
        End If

        ' Find the prefix residue and starting residue
        '
        If periodIndex1 >= 1 Then
            prefix = sequence.Chars(periodIndex1 - 1)
            sequenceStart = sequence.Chars(periodIndex1 + 1)
        Else
            ' periodIndex1 must be 0; assume we're at the protein terminus
            prefix = terminiiSymbol
            sequenceStart = sequence.Chars(periodIndex1 + 1)
        End If

        If periodIndex2 > periodIndex1 + 1 And periodIndex2 <= sequence.Length - 2 Then
            suffix = sequence.Chars(periodIndex2 - 1)
            sequenceStart = sequence.Chars(periodIndex2 + 1)
        ElseIf periodIndex1 > 1 Then
            sequenceEnd = sequence.Chars(sequence.Length - 1)
        End If

        If ruleResidues = terminiiSymbol Then
            ' Peptide database rules
            ' See if prefix and suffix are both "" or are both terminiiSymbol
            If (prefix = terminiiSymbol AndAlso suffix = terminiiSymbol) OrElse
               (prefix = Nothing AndAlso suffix = Nothing) Then
                ruleMatchCount = 2
                matchesCleavageRule = True
            Else
                matchesCleavageRule = False
            End If
        Else
            If ignoreCase Then
                ruleResidues = ruleResidues.ToUpper
            End If

            ' Test both prefix and sequenceEnd against ruleResidues
            ' Make sure suffix does not match exceptionResidues
            For endToCheck = 0 To 1
                skipThisEnd = False
                If endToCheck = 0 Then
                    ' N terminus
                    If prefix = terminiiSymbol Then
                        terminusCount += 1
                        skipThisEnd = True
                    Else
                        If reversedCleavageDirection Then
                            testResidue = sequenceStart
                            exceptionResidue = prefix
                        Else
                            testResidue = prefix
                            exceptionResidue = sequenceStart
                        End If
                    End If
                Else
                    ' C terminus
                    If suffix = terminiiSymbol Then
                        terminusCount += 1
                        skipThisEnd = True
                    Else
                        If reversedCleavageDirection Then
                            testResidue = suffix
                            exceptionResidue = sequenceEnd
                        Else
                            testResidue = sequenceEnd
                            exceptionResidue = suffix
                        End If
                    End If
                End If

                If Not skipThisEnd Then
                    If CheckSequenceAgainstCleavageRuleMatchTestResidue(testResidue, ruleResidues) Then
                        ' Match found
                        ' See if exceptionResidue matches any of the exception residues
                        If Not CheckSequenceAgainstCleavageRuleMatchTestResidue(exceptionResidue, exceptionResidues) Then
                            ruleMatchCount += 1
                        End If
                    End If
                End If

            Next endToCheck

            If terminusCount = 2 Then
                ' Both ends of the peptide are terminii; label as fully matching the rules and set RuleMatchCount to 2
                matchesCleavageRule = True
                ruleMatchCount = 2
            ElseIf terminusCount = 1 Then
                ' One end was a terminus; can either be fully cleaved or non-cleaved; never partially cleaved
                If ruleMatchCount >= 1 Then
                    matchesCleavageRule = True
                    ruleMatchCount = 2
                End If
            Else
                If ruleMatchCount = 2 Then
                    matchesCleavageRule = True
                ElseIf ruleMatchCount >= 1 AndAlso allowPartialCleavage Then
                    matchesCleavageRule = True
                End If
            End If

        End If

        Return matchesCleavageRule

    End Function

    Private Function CheckSequenceAgainstCleavageRuleMatchTestResidue(testResidue As Char, ruleResidues As String) As Boolean
        ' Checks to see if testResidue matches one of the residues in ruleResidues
        ' Used to test by Rule Residues and Exception Residues

        If ruleResidues.IndexOf(testResidue) >= 0 Then
            Return True
        Else
            Return False
        End If

    End Function

    ''' <summary>
    ''' Very simple mass computation utility; only considers elements C, H, N, O, S, and P
    ''' </summary>
    ''' <param name="formula">
    ''' Can only contain C, H, N, O, S, or P, with each element optionally having an integer after it
    ''' Cannot contain any parentheses or other advanced formula features
    ''' </param>
    ''' <returns>Formula mass, or 0 if any unknown symbols are encountered</returns>
    Private Function ComputeFormulaWeightCHNOSP(formula As String) As Double

        Dim formulaMass As Double = 0
        Dim index As Integer
        Dim lastElementIndex As Integer = -1
        Dim multiplier As String = String.Empty

        For index = 0 To formula.Length - 1
            If Char.IsNumber(formula.Chars(index)) Then
                multiplier &= formula.Chars(index)
            Else
                If lastElementIndex >= 0 Then
                    formulaMass += ComputeFormulaWeightLookupMass(formula.Chars(lastElementIndex), multiplier)
                    multiplier = String.Empty
                End If
                lastElementIndex = index
            End If
        Next

        ' Handle the final element
        If lastElementIndex >= 0 Then
            formulaMass += ComputeFormulaWeightLookupMass(formula.Chars(lastElementIndex), multiplier)
        End If

        Return formulaMass

    End Function

    Private Function ComputeFormulaWeightLookupMass(symbol As Char, multiplier As String) As Double
        Dim multiplierVal As Integer

        If multiplier.Length > 0 Then
            Try
                multiplierVal = Integer.Parse(multiplier)
            Catch ex As Exception
                ' Error converting to integer
                multiplierVal = 1
            End Try
        Else
            multiplierVal = 1
        End If

        Try
            Return ElementMasses(symbol) * multiplierVal
        Catch ex As Exception
            ' Symbol not found, or has invalid mass
            Return 0
        End Try

    End Function

    ''' <summary>
    ''' Converts a sequence from 1 letter to 3 letter codes or vice versa
    ''' </summary>
    ''' <param name="sequence"></param>
    ''' <param name="oneLetterTo3Letter"></param>
    ''' <param name="addSpaceEvery10Residues"></param>
    ''' <param name="separateResiduesWithDash"></param>
    ''' <returns></returns>
    Public Function ConvertAminoAcidSequenceSymbols(
        sequence As String,
        oneLetterTo3Letter As Boolean,
        Optional addSpaceEvery10Residues As Boolean = False,
        Optional separateResiduesWithDash As Boolean = False) As String

        Dim peptide As New PeptideSequenceClass
        Dim prefix As String = String.Empty
        Dim suffix As String = String.Empty
        Dim sequenceOut As String

        Try
            If oneLetterTo3Letter Then
                sequence = CheckForAndRemovePrefixAndSuffixResidues(sequence, prefix, suffix)
            End If
            peptide.SetSequence(sequence, NTerminusGroupConstants.None, CTerminusGroupConstants.None, Not oneLetterTo3Letter)

            sequenceOut = peptide.GetSequence(oneLetterTo3Letter, addSpaceEvery10Residues, separateResiduesWithDash)

            If oneLetterTo3Letter AndAlso (prefix.Length > 0 Or suffix.Length > 0) Then
                peptide.SetSequence(prefix, NTerminusGroupConstants.None, CTerminusGroupConstants.None, False)
                sequenceOut = peptide.GetSequence(True, False, False) & "." & sequenceOut

                peptide.SetSequence(suffix, NTerminusGroupConstants.None, CTerminusGroupConstants.None, False)
                sequenceOut &= "." & peptide.GetSequence(True, False, False)
            End If

        Catch ex As Exception
            sequenceOut = String.Empty
        End Try

        Return sequenceOut

    End Function

    ''' <summary>
    ''' Convert between 1 letter and 3 letter symbol
    ''' </summary>
    ''' <param name="symbolToParse">Amino acid symbol to parse</param>
    ''' <param name="oneLetterTo3Letter">
    ''' When true, converting 1 letter codes to 3 letter codes
    ''' Otherwise, converting 3 letter codes to 1 letter codes
    ''' </param>
    ''' <returns>Converted amino acid symbol if a valid amino acid symbol, otherwise an empty string</returns>
    Private Function GetAminoAcidSymbolConversion(symbolToParse As String, oneLetterTo3Letter As Boolean) As String

        Dim symbol As Char

        If oneLetterTo3Letter Then
            symbol = symbolToFind.Substring(0, 1).ToUpper.Chars(0)
            If AminoAcidSymbols.ContainsKey(symbol) Then
                Return AminoAcidSymbols(symbol)
            Else
                Return String.Empty
            End If
        Else
            Dim myEnumerator As IDictionaryEnumerator = AminoAcidSymbols.GetEnumerator()
            symbolToFind = symbolToFind.ToUpper
            Do While myEnumerator.MoveNext()
                If CType(myEnumerator.Value, String).ToUpper = symbolToFind Then
                    Return CType(myEnumerator.Key, String)
                End If
            Loop

            ' If we get here, then the value wasn't found
            Return String.Empty
        End If

    End Function

    ''' <summary>
    ''' Returns the given residue in the current sequence
    ''' </summary>
    ''' <param name="residueNumber"></param>
    ''' <param name="use3LetterCode"></param>
    ''' <returns></returns>
    Public Function GetResidue(residueNumber As Integer, Optional use3LetterCode As Boolean = False) As String

        If Not mResidues Is Nothing AndAlso (residueNumber > 0 And residueNumber <= mResidues.Length) Then
            If use3LetterCode Then
                Return GetAminoAcidSymbolConversion(mResidues.Chars(residueNumber - 1), True)
            Else
                Return mResidues.Chars(residueNumber - 1)
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

    ''' <summary>
    ''' Returns the number of occurrences of the given residue in the loaded sequence, or -1 if an error
    ''' </summary>
    ''' <param name="residueSymbol"></param>
    ''' <param name="use3LetterCode"></param>
    ''' <returns></returns>
    Public Function GetResidueCountSpecificResidue(residueSymbol As String, Optional use3LetterCode As Boolean = False) As Integer

        Dim searchResidue1Letter As Char
        Dim residueCount As Integer
        Dim residueIndex As Integer

        If mResidues Is Nothing OrElse mResidues.Length = 0 Then Return 0

        Try
            If use3LetterCode Then
                searchResidue1Letter = GetAminoAcidSymbolConversion(residueSymbol, False).Chars(0)
            Else
                searchResidue1Letter = residueSymbol.Chars(0)
            End If
        Catch ex As Exception
            Return -1
        End Try

        residueIndex = -1
        residueCount = 0
        Do
            residueIndex = mResidues.IndexOf(searchResidue1Letter, residueIndex + 1)
            If residueIndex >= 0 Then
                residueCount += 1
            Else
                Exit Do
            End If
        Loop

        Return residueCount
    End Function

    Public Function GetSequence(
        Optional use3LetterCode As Boolean = False,
        Optional addSpaceEvery10Residues As Boolean = False,
        Optional separateResiduesWithDash As Boolean = False,
        Optional includeNAndCTerminii As Boolean = False) As String

        ' Construct a text sequence using Residues() and the N and C Terminus info

        Dim sequence As String, symbol3Letter As String
        Dim dashAdd As String = String.Empty
        Dim index As Integer
        Dim lastIndex As Integer

        If mResidues Is Nothing Then Return String.Empty

        If Not use3LetterCode And Not addSpaceEvery10Residues And Not separateResiduesWithDash Then
            ' Simply return the sequence, possibly with the N and C terminii
            sequence = mResidues
        Else

            If separateResiduesWithDash Then dashAdd = "-" Else dashAdd = String.Empty

            sequence = String.Empty
            lastIndex = mResidues.Length - 1
            For index = 0 To lastIndex

                If use3LetterCode Then
                    symbol3Letter = GetAminoAcidSymbolConversion(mResidues.Chars(index), True)
                    If symbol3Letter = "" Then symbol3Letter = UNKNOWN_SYMBOL_THREE_LETTERS
                    sequence &= symbol3Letter
                Else
                    sequence &= mResidues.Chars(index)
                End If

                If index < lastIndex Then
                    If addSpaceEvery10Residues Then
                        If (index + 1) Mod 10 = 0 Then
                            sequence &= " "
                        Else
                            sequence &= dashAdd
                        End If
                    Else
                        sequence &= dashAdd
                    End If
                End If

            Next index

        End If

        If includeNAndCTerminii Then
            sequence = mNTerminus.Formula & dashAdd & sequence & dashAdd & mCTerminus.Formula
        End If

        Return sequence
    End Function

    Public Function GetSequenceMass() As Double
        Return Mass
    End Function

    ''' <summary>
    ''' Get the tryptic name for the peptide, within the context of the protein residues
    ''' </summary>
    ''' <param name="proteinResidues"></param>
    ''' <param name="peptideResidues"></param>
    ''' <param name="returnResidueStart">Output: residue in the protein where the peptide starts</param>
    ''' <param name="returnResidueEnd">Output: residue in the protein where the peptide ends</param>
    ''' <param name="icr2LSCompatible"></param>
    ''' <param name="ruleResidues"></param>
    ''' <param name="exceptionResidues"></param>
    ''' <param name="reversedCleavageDirection"></param>
    ''' <param name="terminiiSymbol"></param>
    ''' <param name="ignoreCase"></param>
    ''' <param name="proteinSearchStartLoc"></param>
    ''' <returns></returns>
    Public Function GetTrypticName(
        proteinResidues As String,
        peptideResidues As String,
        <Out> Optional ByRef returnResidueStart As Integer = 0,
        <Out> Optional ByRef returnResidueEnd As Integer = 0,
        Optional icr2LSCompatible As Boolean = False,
        Optional ruleResidues As String = TRYPTIC_RULE_RESIDUES,
        Optional exceptionResidues As String = TRYPTIC_EXCEPTION_RESIDUES,
        Optional reversedCleavageDirection As Boolean = False,
        Optional terminiiSymbol As Char = TERMINII_SYMBOL,
        Optional ignoreCase As Boolean = True,
        Optional proteinSearchStartLoc As Integer = 1) As String

        ' Examines peptideResidues to see where they exist in proteinResidues
        ' Constructs a name string based on their position and based on whether the fragment is truly tryptic
        ' In addition, returns the position of the first and last residue in returnResidueStart and returnResidueEnd
        ' The tryptic name in the following format
        ' t1  indicates tryptic peptide 1
        ' t2 represents tryptic peptide 2, etc.
        ' t1.2  indicates tryptic peptide 1, plus one more tryptic peptide, i.e. t1 and t2
        ' t5.2  indicates tryptic peptide 5, plus one more tryptic peptide, i.e. t5 and t6
        ' t5.3  indicates tryptic peptide 5, plus two more tryptic peptides, i.e. t5, t6, and t7
        ' 40.52  means that the residues are not tryptic, and simply range from residue 40 to 52
        ' If the peptide residues are not present in proteinResidues, returns ""
        ' Since a peptide can occur multiple times in a protein, one can set proteinSearchStartLoc to a value larger than 1 to ignore previous hits

        ' If icr2LSCompatible is True, then the values returned when a peptide is not tryptic are modified to
        ' range from the starting residue, to the ending residue +1
        ' returnResidueEnd is always equal to the position of the final residue, regardless of icr2LSCompatible

        ' ReSharper disable CommentTypo
        ' For example, if proteinResidues = "IGKANR"
        ' Then when peptideResidues = "IGK", the TrypticName is t1
        ' Then when peptideResidues = "ANR", the TrypticName is t2
        ' Then when peptideResidues = "IGKANR", the TrypticName is t1.2
        ' Then when peptideResidues = "IG", the TrypticName is 1.2
        ' Then when peptideResidues = "KANR", the TrypticName is 3.6
        ' Then when peptideResidues = "NR", the TrypticName is 5.6

        ' However, if icr2LSCompatible = True, then the last three are changed to:
        ' Then when peptideResidues = "IG", the TrypticName is 1.3
        ' Then when peptideResidues = "KANR", the TrypticName is 3.7
        ' Then when peptideResidues = "NR", the TrypticName is 5.7

        ' ReSharper restore CommentTypo

        Dim startLoc As Integer, endLoc As Integer        ' Residue numbers, ranging from 1 to length of the protein
        Dim trypticName As String
        Dim prefix As String, suffix As String
        Dim residueFollowingSearchResidues As String
        Dim matchesCleavageRule As Boolean

        Dim trypticResidueNumber As Integer
        Dim ruleResidueMatchCount As Integer
        Dim ruleResidueLoc As Integer
        Dim proteinResiduesBeforeStartLoc As String
        Dim peptideResiduesLength As Integer

        If ignoreCase Then
            proteinResidues = proteinResidues.ToUpper()
            peptideResidues = peptideResidues.ToUpper()
        End If

        If proteinSearchStartLoc <= 1 Then
            startLoc = proteinResidues.IndexOf(peptideResidues, StringComparison.Ordinal) + 1
        Else
            startLoc = proteinResidues.Substring(proteinSearchStartLoc + 1).IndexOf(peptideResidues, StringComparison.Ordinal) + 1
            If startLoc > 0 Then
                startLoc = startLoc + proteinSearchStartLoc - 1
            End If
        End If

        peptideResiduesLength = peptideResidues.Length

        If startLoc > 0 And proteinResidues.Length > 0 And peptideResiduesLength > 0 Then
            endLoc = startLoc + peptideResiduesLength - 1

            ' Determine if the residue is tryptic
            ' Use CheckSequenceAgainstCleavageRule() for this
            If startLoc > 1 Then
                prefix = proteinResidues.Chars(startLoc - 2)
            Else
                prefix = terminiiSymbol
            End If

            If endLoc = proteinResidues.Length Then
                suffix = terminiiSymbol
            Else
                suffix = proteinResidues.Chars(endLoc)
            End If

            matchesCleavageRule = CheckSequenceAgainstCleavageRule(prefix & "." & peptideResidues & "." & suffix, ruleResidues, exceptionResidues, reversedCleavageDirection, False, "."c, terminiiSymbol, ignoreCase)

            If matchesCleavageRule Then
                ' Construct trypticName

                ' Determine which tryptic residue peptideResidues is
                If startLoc = 1 Then
                    trypticResidueNumber = 1
                Else
                    trypticResidueNumber = 0
                    proteinResiduesBeforeStartLoc = proteinResidues.Substring(0, startLoc - 1)
                    residueFollowingSearchResidues = peptideResidues.Chars(0)

                    ruleResidueLoc = 0
                    Do
                        ruleResidueLoc = GetTrypticNameFindNextCleavageLoc(proteinResiduesBeforeStartLoc, residueFollowingSearchResidues, ruleResidueLoc + 1, ruleResidues, exceptionResidues, reversedCleavageDirection, terminiiSymbol)
                        If ruleResidueLoc > 0 Then
                            trypticResidueNumber += 1
                        End If
                    Loop While ruleResidueLoc > 0 And ruleResidueLoc + 1 < startLoc
                    trypticResidueNumber += 1
                End If

                ' Determine number of K or R residues in peptideResidues
                ' Ignore K or R residues followed by Proline
                ruleResidueMatchCount = 0
                ruleResidueLoc = 0
                Do
                    ruleResidueLoc = GetTrypticNameFindNextCleavageLoc(peptideResidues, suffix, ruleResidueLoc + 1, ruleResidues, exceptionResidues, reversedCleavageDirection, terminiiSymbol)
                    If ruleResidueLoc > 0 Then
                        ruleResidueMatchCount += 1
                    End If
                Loop While ruleResidueLoc > 0 And ruleResidueLoc < peptideResiduesLength

                trypticName = "t" & trypticResidueNumber.ToString()
                If ruleResidueMatchCount > 1 Then
                    trypticName &= "." & Trim(Str((ruleResidueMatchCount)))
                End If
            Else
                If icr2LSCompatible Then
                    trypticName = startLoc.ToString() & "." & (endLoc + 1).ToString()
                Else
                    trypticName = startLoc.ToString() & "." & endLoc.ToString()
                End If
            End If

            returnResidueStart = startLoc
            returnResidueEnd = endLoc
            Return trypticName
        Else
            ' Residues not found
            returnResidueStart = 0
            returnResidueEnd = 0
            Return String.Empty
        End If

    End Function

    ''' <summary>
    ''' Examines peptideResidues to see where they exist in proteinResidues
    ''' Looks for all possible matches, returning them as a comma separated list
    ''' </summary>
    ''' <param name="proteinResidues"></param>
    ''' <param name="peptideResidues"></param>
    ''' <param name="returnMatchCount">Output: number of times peptideResidues is found in proteinResidues</param>
    ''' <param name="returnResidueStart">Output: residue in the protein where the peptide starts</param>
    ''' <param name="returnResidueEnd">Output: residue in the protein where the peptide ends</param>
    ''' <param name="icr2LSCompatible"></param>
    ''' <param name="ruleResidues"></param>
    ''' <param name="exceptionResidues"></param>
    ''' <param name="reversedCleavageDirection"></param>
    ''' <param name="terminiiSymbol"></param>
    ''' <param name="ignoreCase"></param>
    ''' <param name="proteinSearchStartLoc"></param>
    ''' <param name="listDelimiter"></param>
    ''' <returns>Comma separated list of tryptic names</returns>
    ''' <remarks>See GetTrypticName for additional information</remarks>
    Public Function GetTrypticNameMultipleMatches(
        proteinResidues As String,
        peptideResidues As String,
        <Out> Optional ByRef returnMatchCount As Integer = 1,
        <Out> Optional ByRef returnResidueStart As Integer = 0,
        <Out> Optional ByRef returnResidueEnd As Integer = 0,
        Optional icr2LSCompatible As Boolean = False,
        Optional ruleResidues As String = TRYPTIC_RULE_RESIDUES,
        Optional exceptionResidues As String = TRYPTIC_EXCEPTION_RESIDUES,
        Optional reversedCleavageDirection As Boolean = False,
        Optional terminiiSymbol As Char = TERMINII_SYMBOL,
        Optional ignoreCase As Boolean = True,
        Optional proteinSearchStartLoc As Integer = 1,
        Optional listDelimiter As String = ", ") As String

        Dim nameList As String
        Dim currentName As String
        Dim currentSearchLoc As Integer
        Dim currentResidueStart As Integer, currentResidueEnd As Integer

        currentSearchLoc = proteinSearchStartLoc
        returnMatchCount = 0
        returnResidueStart = 0
        returnResidueEnd = 0
        nameList = String.Empty

        Do
            currentName = GetTrypticName(proteinResidues, peptideResidues, currentResidueStart, currentResidueEnd, icr2LSCompatible, ruleResidues, exceptionResidues, reversedCleavageDirection, terminiiSymbol, ignoreCase, currentSearchLoc)

            If currentName.Length > 0 Then
                If nameList.Length > 0 Then
                    nameList &= listDelimiter
                End If
                nameList &= currentName
                currentSearchLoc = currentResidueEnd + 1
                returnMatchCount += 1

                If returnMatchCount = 1 Then
                    returnResidueStart = currentResidueStart
                End If
                returnResidueEnd = currentResidueEnd

                If currentSearchLoc > Len(proteinResidues) Then Exit Do
            Else
                Exit Do
            End If
        Loop

        Return nameList

    End Function

    Private Function GetTrypticNameFindNextCleavageLoc(
        searchResidues As String,
        residueFollowingSearchResidues As String,
        startChar As Integer,
        Optional ruleResidues As String = TRYPTIC_RULE_RESIDUES,
        Optional exceptionResidues As String = TRYPTIC_EXCEPTION_RESIDUES,
        Optional reversedCleavageDirection As Boolean = False,
        Optional terminiiSymbol As Char = TERMINII_SYMBOL) As Integer

        ' Finds the location of the next searchChar in searchResidues (K or R by default)
        ' Assumes searchResidues are already upper case
        ' Examines the residue following the matched residue
        '   If it matches one of the characters in exceptionResidues, then the match is not counted
        ' Note that residueFollowingSearchResidues is necessary in case the potential cleavage residue is the final residue in searchResidues
        ' We need to know the next residue to determine if it matches an exception residue

        ' ReSharper disable CommentTypo

        ' For example, if searchResidues =      "IGASGEHIFIIGVDKPNR"
        '  and the protein it is part of is: TNSANFRIGASGEHIFIIGVDKPNRQPDS
        '  and searchChars = "KR while exceptionResidues  = "P"
        ' Then the K in IGASGEHIFIIGVDKPNR is ignored because the following residue is P,
        '  while the R in IGASGEHIFIIGVDKPNR is OK because residueFollowingSearchResidues is Q
        ' It is the calling function's responsibility to assign the correct residue to residueFollowingSearchResidues
        ' If no match is found, but residueFollowingSearchResidues is "-", then the cleavage location returned is Len(searchResidues) + 1

        ' ReSharper restore CommentTypo

        Dim charIndexInSearchChars As Integer
        Dim charLoc As Integer, minCharLoc As Integer
        Dim exceptionSuffixResidueCount As Integer
        Dim exceptionResidueToCheck As Char
        Dim exceptionCharLocInSearchResidues As Integer, charLocViaRecursiveSearch As Integer
        Dim newStartCharLoc As Integer

        Dim matchFound As Boolean
        Dim recursiveCheck As Boolean

        If exceptionResidues Is Nothing Then
            exceptionSuffixResidueCount = 0
        Else
            exceptionSuffixResidueCount = exceptionResidues.Length
        End If

        minCharLoc = -1
        For charIndexInSearchChars = 0 To ruleResidues.Length - 1
            matchFound = False
            If reversedCleavageDirection Then
                ' Cleave before the matched residue
                ' Note that the CharLoc value we're storing is the location just before the cleavage point
                charLoc = searchResidues.IndexOf(ruleResidues.Chars(charIndexInSearchChars), startChar)
            Else
                charLoc = searchResidues.IndexOf(ruleResidues.Chars(charIndexInSearchChars), startChar - 1) + 1
            End If

            If charLoc >= 1 And charLoc >= startChar Then matchFound = True

            If matchFound Then
                If exceptionSuffixResidueCount > 0 Then
                    If reversedCleavageDirection Then
                        ' Make sure the residue before the matched residue does not match exceptionResidues
                        ' We already subtracted 1 from charLoc so charLoc is already the correct character number
                        ' Note that the above logic assures that charLoc is > 0
                        exceptionCharLocInSearchResidues = charLoc
                        exceptionResidueToCheck = searchResidues.Chars(exceptionCharLocInSearchResidues - 1)
                    Else
                        ' Make sure suffixResidue does not match exceptionResidues
                        If charLoc < searchResidues.Length Then
                            exceptionCharLocInSearchResidues = charLoc + 1
                            exceptionResidueToCheck = searchResidues.Chars(exceptionCharLocInSearchResidues - 1)
                        Else
                            ' Matched the last residue in searchResidues
                            exceptionCharLocInSearchResidues = searchResidues.Length + 1
                            exceptionResidueToCheck = residueFollowingSearchResidues.Chars(0)
                        End If
                    End If

                    If exceptionResidues.IndexOf(exceptionResidueToCheck) >= 0 Then
                        ' Exception char matched; can't count this as the cleavage point
                        matchFound = False
                        recursiveCheck = False
                        If reversedCleavageDirection Then
                            If charLoc + 1 < searchResidues.Length Then
                                recursiveCheck = True
                                newStartCharLoc = charLoc + 2
                            End If
                        Else
                            If exceptionCharLocInSearchResidues < searchResidues.Length Then
                                recursiveCheck = True
                                newStartCharLoc = exceptionCharLocInSearchResidues
                            End If
                        End If

                        If recursiveCheck Then
                            ' Recursively call this function to find the next cleavage position, using an updated startChar position
                            charLocViaRecursiveSearch = GetTrypticNameFindNextCleavageLoc(searchResidues, residueFollowingSearchResidues, newStartCharLoc, ruleResidues, exceptionResidues, reversedCleavageDirection, terminiiSymbol)

                            If charLocViaRecursiveSearch > 0 Then
                                ' Found a residue further along that is a valid cleavage point
                                charLoc = charLocViaRecursiveSearch
                                If charLoc >= 1 And charLoc >= startChar Then matchFound = True
                            Else
                                charLoc = 0
                            End If
                        Else
                            charLoc = 0
                        End If
                    End If
                End If
            End If

            If matchFound Then
                If minCharLoc < 0 Then
                    minCharLoc = charLoc
                Else
                    If charLoc < minCharLoc Then
                        minCharLoc = charLoc
                    End If
                End If
            End If
        Next charIndexInSearchChars

        If minCharLoc < 0 And residueFollowingSearchResidues = terminiiSymbol Then
            minCharLoc = searchResidues.Length + 1
        End If

        If minCharLoc < 0 Then
            Return 0
        Else
            Return minCharLoc
        End If

    End Function

    Public Function GetTrypticPeptideNext(
        proteinResidues As String,
        searchStartLoc As Integer,
        <Out> Optional ByRef returnResidueStart As Integer = 0,
        <Out> Optional ByRef returnResidueEnd As Integer = 0) As String

        Return GetTrypticPeptideNext(proteinResidues, searchStartLoc, returnResidueStart, returnResidueEnd, TRYPTIC_RULE_RESIDUES, TRYPTIC_EXCEPTION_RESIDUES, False)
    End Function

    ''' <summary>
    ''' Finds the next tryptic peptide in proteinResidues, starting the search as searchStartLoc
    ''' </summary>
    ''' <param name="proteinResidues"></param>
    ''' <param name="searchStartLoc"></param>
    ''' <param name="returnResidueStart">Output: residue in the protein where the peptide starts</param>
    ''' <param name="returnResidueEnd">Output: residue in the protein where the peptide ends</param>
    ''' <param name="ruleResidues"></param>
    ''' <param name="exceptionResidues"></param>
    ''' <param name="reversedCleavageDirection"></param>
    ''' <param name="terminiiSymbol"></param>
    ''' <returns>The next tryptic peptide in proteinResidues</returns>
    ''' <remarks>
    ''' Useful when obtaining all of the tryptic peptides for a protein, since this function will operate
    ''' much faster than repeatedly calling GetTrypticPeptideByFragmentNumber()
    ''' </remarks>
    Public Function GetTrypticPeptideNext(
        proteinResidues As String,
        searchStartLoc As Integer,
        <Out> ByRef returnResidueStart As Integer,
        <Out> ByRef returnResidueEnd As Integer,
        ruleResidues As String,
        exceptionResidues As String,
        reversedCleavageDirection As Boolean,
        Optional terminiiSymbol As Char = TERMINII_SYMBOL) As String

        Dim ruleResidueLoc As Integer
        Dim proteinResiduesLength As Integer

        If searchStartLoc < 1 Then searchStartLoc = 1

        If proteinResidues Is Nothing Then
            proteinResiduesLength = 0
        Else
            proteinResiduesLength = proteinResidues.Length
        End If

        If searchStartLoc > proteinResiduesLength Then
            Return String.Empty
        End If

        ruleResidueLoc = GetTrypticNameFindNextCleavageLoc(proteinResidues, terminiiSymbol, searchStartLoc, ruleResidues, exceptionResidues, reversedCleavageDirection, terminiiSymbol)
        If ruleResidueLoc > 0 Then
            returnResidueStart = searchStartLoc
            If ruleResidueLoc > proteinResiduesLength Then
                returnResidueEnd = proteinResiduesLength
            Else
                returnResidueEnd = ruleResidueLoc
            End If
            Return proteinResidues.Substring(returnResidueStart - 1, returnResidueEnd - returnResidueStart + 1)
        Else
            returnResidueStart = 1
            returnResidueEnd = proteinResiduesLength
            Return proteinResidues
        End If

    End Function

    ''' <summary>
    ''' Finds the desired tryptic peptide from proteinResidues
    ''' </summary>
    ''' <param name="proteinResidues"></param>
    ''' <param name="desiredPeptideNumber"></param>
    ''' <param name="returnResidueStart">Output: residue in the protein where the peptide starts</param>
    ''' <param name="returnResidueEnd">Output: residue in the protein where the peptide ends</param>
    ''' <param name="ruleResidues"></param>
    ''' <param name="exceptionResidues"></param>
    ''' <param name="reversedCleavageDirection"></param>
    ''' <param name="terminiiSymbol"></param>
    ''' <param name="ignoreCase"></param>
    ''' <returns></returns>
    Public Function GetTrypticPeptideByFragmentNumber(
        proteinResidues As String,
        desiredPeptideNumber As Integer,
        <Out> Optional ByRef returnResidueStart As Integer = 0,
        <Out> Optional ByRef returnResidueEnd As Integer = 0,
        Optional ruleResidues As String = TRYPTIC_RULE_RESIDUES,
        Optional exceptionResidues As String = TRYPTIC_EXCEPTION_RESIDUES,
        Optional reversedCleavageDirection As Boolean = False,
        Optional terminiiSymbol As Char = TERMINII_SYMBOL,
        Optional ignoreCase As Boolean = True) As String

        ' ReSharper disable CommentTypo

        ' Returns the desired tryptic peptide from proteinResidues
        ' For example, if proteinResidues = "IGKANRMTFGL" then
        '  when desiredPeptideNumber = 1, returns "IGK"
        '  when desiredPeptideNumber = 2, returns "ANR"
        '  when desiredPeptideNumber = 3, returns "MTFGL"

        ' ReSharper restore CommentTypo

        ' Optionally, returns the position of the start and end residues
        '  using returnResidueStart and returnResidueEnd


        Dim startLoc As Integer, ruleResidueLoc As Integer
        Dim prevStartLoc As Integer
        Dim proteinResiduesLength As Integer
        Dim currentTrypticPeptideNumber As Integer

        Dim matchingFragment As String

        If desiredPeptideNumber < 1 Then
            returnResidueStart = 0
            returnResidueEnd = 0
            Return String.Empty
        End If

        If ignoreCase Then
            proteinResidues = proteinResidues.ToUpper
        End If
        proteinResiduesLength = proteinResidues.Length

        startLoc = 1
        currentTrypticPeptideNumber = 0
        Do
            ruleResidueLoc = GetTrypticNameFindNextCleavageLoc(proteinResidues, terminiiSymbol, startLoc, ruleResidues, exceptionResidues, reversedCleavageDirection, terminiiSymbol)
            If ruleResidueLoc > 0 Then
                currentTrypticPeptideNumber += 1
                prevStartLoc = startLoc
                startLoc = ruleResidueLoc + 1

                If prevStartLoc > proteinResiduesLength Then
                    ' User requested a peptide number that doesn't exist
                    returnResidueStart = 0
                    returnResidueEnd = 0
                    Return String.Empty
                End If
            Else
                ' I don't think I'll ever reach this code
                Console.WriteLine("Unexpected code point reached in GetTrypticPeptideByFragmentNumber")
                Exit Do
            End If
        Loop While currentTrypticPeptideNumber < desiredPeptideNumber

        If currentTrypticPeptideNumber > 0 And prevStartLoc > 0 Then
            If prevStartLoc > proteinResidues.Length Then
                ' User requested a peptide number that is too high
                returnResidueStart = 0
                returnResidueEnd = 0
                matchingFragment = String.Empty
            Else
                ' Match found, find the extent of this peptide
                returnResidueStart = prevStartLoc
                If ruleResidueLoc > proteinResiduesLength Then
                    returnResidueEnd = proteinResiduesLength
                Else
                    returnResidueEnd = ruleResidueLoc
                End If
                matchingFragment = proteinResidues.Substring(prevStartLoc - 1, ruleResidueLoc - prevStartLoc + 1)
            End If
        Else
            returnResidueStart = 1
            returnResidueEnd = proteinResiduesLength
            matchingFragment = proteinResidues
        End If

        Return matchingFragment

    End Function

    ''' <summary>
    ''' Removing the leading H, if present
    ''' </summary>
    ''' <param name="workingSequence"></param>
    ''' <remarks>This is only applicable for sequences in 3 letter notation</remarks>
    Private Sub RemoveLeadingH(ByRef workingSequence As String)

        Dim oneLetterSymbol As String

        If workingSequence.Substring(0, 1).ToUpper = "H" AndAlso workingSequence.Length >= 4 Then
            ' If next character is not a character, then remove the H and the next character
            If Not Char.IsLetter(workingSequence.Chars(1)) Then
                workingSequence = workingSequence.Substring(2)
            Else
                ' Otherwise, see if next three characters are letters
                If Char.IsLetter(workingSequence.Chars(1)) AndAlso
                    Char.IsLetter(workingSequence.Chars(2)) AndAlso
                    Char.IsLetter(workingSequence.Chars(3)) Then
                    ' Formula starts with 4 characters and the first is H, see if the first 3 characters are a valid amino acid code

                    oneLetterSymbol = GetAminoAcidSymbolConversion(workingSequence.Substring(1, 3), False)

                    If oneLetterSymbol.Length = 0 Then
                        ' Doesn't start with a valid amino acid 3 letter abbreviation, so remove the initial H
                        workingSequence = workingSequence.Substring(1)
                    End If
                End If
            End If
        End If

    End Sub

    ''' <summary>
    ''' Removing the trailing OH, if present
    ''' </summary>
    ''' <param name="workingSequence">Amino acids, in 3 letter notation</param>
    ''' <remarks>This is only applicable for sequences in 3 letter notation</remarks>
    Private Sub RemoveTrailingOH(ByRef workingSequence As String)

        Dim oneLetterSymbol As String
        Dim stringLength As Integer

        stringLength = workingSequence.Length
        If workingSequence.Substring(stringLength - 2, 2).ToUpper = "OH" And stringLength >= 5 Then
            ' If previous character is not a character, then remove the OH (and the character preceding)
            If Not Char.IsLetter(workingSequence.Chars(stringLength - 3)) Then
                workingSequence = workingSequence.Substring(0, stringLength - 3)
            Else
                ' Otherwise, see if previous three characters are letters
                If Char.IsLetter(workingSequence.Chars(stringLength - 2)) AndAlso
                    Char.IsLetter(workingSequence.Chars(stringLength - 3)) AndAlso
                    Char.IsLetter(workingSequence.Chars(stringLength - 4)) Then
                    ' Formula ends with 3 characters and the last two are OH, see if the last 3 characters are a valid amino acid code

                    oneLetterSymbol = GetAminoAcidSymbolConversion(workingSequence.Substring(stringLength - 4, 3), False)

                    If oneLetterSymbol.Length = 0 Then
                        ' Doesn't end with a valid amino acid 3 letter abbreviation, so remove the trailing OH
                        workingSequence = workingSequence.Substring(0, stringLength - 2)
                    End If
                End If
            End If
        End If

    End Sub

    ''' <summary>
    ''' Define the C-terminus using an empirical formula
    ''' </summary>
    ''' <param name="formula">Can only contain C, H, N, O, S, or P, and cannot contain any parentheses or other advanced formula features</param>
    ''' <param name="followingResidue"></param>
    ''' <param name="use3LetterCode"></param>
    ''' <returns>0 if success; 1 if error</returns>
    ''' <remarks>
    ''' Typical C terminus groups
    ''' Free Acid = OH
    ''' Amide = NH2
    ''' </remarks>
    Private Function SetCTerminus(formula As String, Optional followingResidue As String = "", Optional use3LetterCode As Boolean = False) As Integer

        Dim returnVal As Integer

        mCTerminus.Formula = formula
        mCTerminus.Mass = ComputeFormulaWeightCHNOSP(mCTerminus.Formula)
        mCTerminus.MassElementMode = mCurrentElementMode
        If mCTerminus.Mass < 0 Then
            mCTerminus.Mass = 0
            returnVal = 1
        Else
            returnVal = 0
        End If
        mCTerminus.PrecedingResidue = String.Empty
        If use3LetterCode AndAlso followingResidue.Length > 0 Then
            mCTerminus.FollowingResidue = GetAminoAcidSymbolConversion(followingResidue, False)
        Else
            mCTerminus.FollowingResidue = followingResidue
        End If

        If Not mDelayUpdateResidueMass Then UpdateSequenceMass()
        Return returnVal

    End Function

    ''' <summary>
    ''' Define the C-terminus using an enum in CTerminusGroupConstants
    ''' </summary>
    ''' <param name="eCTerminusGroup"></param>
    ''' <param name="followingResidue"></param>
    ''' <param name="use3LetterCode"></param>
    ''' <returns>0 if success; 1 if error</returns>
    Public Function SetCTerminusGroup(eCTerminusGroup As CTerminusGroupConstants, Optional followingResidue As String = "", Optional use3LetterCode As Boolean = False) As Integer

        Dim errorCode As Integer

        Select Case eCTerminusGroup
            Case CTerminusGroupConstants.Hydroxyl : errorCode = SetCTerminus("OH", followingResidue, use3LetterCode)
            Case CTerminusGroupConstants.Amide : errorCode = SetCTerminus("NH2", followingResidue, use3LetterCode)
            Case CTerminusGroupConstants.None : errorCode = SetCTerminus(String.Empty, followingResidue, use3LetterCode)
            Case Else : errorCode = 1
        End Select

        Return errorCode

    End Function

    ''' <summary>
    ''' Define the N-terminus using an empirical formula
    ''' </summary>
    ''' <param name="formula">Can only contain C, H, N, O, S, or P, and cannot contain any parentheses or other advanced formula features</param>
    ''' <param name="precedingResidue"></param>
    ''' <param name="use3LetterCode"></param>
    ''' <returns>0 if success; 1 if error</returns>
    ''' <remarks>
    ''' Typical N terminus groups
    ''' Hydrogen = H
    ''' Acetyl = C2OH3
    ''' PyroGlu = C5O2NH6
    ''' Carbamyl = CONH2
    ''' PTC = C7H6NS
    ''' </remarks>
    Private Function SetNTerminus(formula As String, Optional precedingResidue As String = "", Optional use3LetterCode As Boolean = False) As Integer

        Dim returnVal As Integer

        mNTerminus.Formula = formula
        mNTerminus.Mass = ComputeFormulaWeightCHNOSP(mNTerminus.Formula)
        mNTerminus.MassElementMode = mCurrentElementMode
        If mNTerminus.Mass < 0 Then
            mNTerminus.Mass = 0
            returnVal = 1
        Else
            returnVal = 0
        End If
        mNTerminus.PrecedingResidue = String.Empty
        If use3LetterCode AndAlso precedingResidue.Length > 0 Then
            mNTerminus.PrecedingResidue = GetAminoAcidSymbolConversion(precedingResidue, False)
        Else
            mNTerminus.PrecedingResidue = precedingResidue
        End If
        mNTerminus.FollowingResidue = String.Empty

        If Not mDelayUpdateResidueMass Then UpdateSequenceMass()
        Return returnVal

    End Function

    ''' <summary>
    ''' Define the N-terminus using an enum in NTerminusGroupConstants
    ''' </summary>
    ''' <param name="eNTerminusGroup"></param>
    ''' <param name="precedingResidue"></param>
    ''' <param name="use3LetterCode"></param>
    ''' <returns>0 if success; 1 if error</returns>
    Public Function SetNTerminusGroup(eNTerminusGroup As NTerminusGroupConstants, Optional precedingResidue As String = "", Optional use3LetterCode As Boolean = False) As Integer

        Dim errorCode As Integer

        Select Case eNTerminusGroup
            Case NTerminusGroupConstants.Hydrogen : errorCode = SetNTerminus("H", precedingResidue, use3LetterCode)
            Case NTerminusGroupConstants.HydrogenPlusProton : errorCode = SetNTerminus("HH", precedingResidue, use3LetterCode)
            Case NTerminusGroupConstants.Acetyl : errorCode = SetNTerminus("C2OH3", precedingResidue, use3LetterCode)
            Case NTerminusGroupConstants.PyroGlu : errorCode = SetNTerminus("C5O2NH6", precedingResidue, use3LetterCode)
                ' ReSharper disable once StringLiteralTypo
            Case NTerminusGroupConstants.Carbamyl : errorCode = SetNTerminus("CONH2", precedingResidue, use3LetterCode)
            Case NTerminusGroupConstants.PTC : errorCode = SetNTerminus("C7H6NS", precedingResidue, use3LetterCode)
            Case NTerminusGroupConstants.None : errorCode = SetNTerminus("", precedingResidue, use3LetterCode)
            Case Else : errorCode = 1
        End Select

        Return errorCode

    End Function

    ''' <summary>
    ''' Define the peptide sequence
    ''' </summary>
    ''' <param name="sequence">Peptide or protein amino acid symbols</param>
    ''' <param name="eNTerminus">N-terminus type</param>
    ''' <param name="eCTerminus">C-terminus type</param>
    ''' <param name="is3LetterCode">When true, sequence uses 3 letter amino acid symbols</param>
    ''' <param name="oneLetterCheckForPrefixAndSuffixResidues">
    ''' When true, if the sequence has prefix and suffix letters, they are removed
    ''' For example: R.ABCDEFGK.R will be stored as ABCDEFGK
    ''' </param>
    ''' <param name="threeLetterCheckForPrefixHandSuffixOH">
    ''' When true, if the sequence starts with H or OH, remove those letters
    ''' For example: Arg.AlaCysAspPheGlyLys.Arg will be stored as AlaCysAspPheGlyLys
    ''' </param>
    ''' <returns>
    ''' 0 if success or 1 if an error
    ''' Will return 0 if the sequence is blank or if it contains no valid residues
    ''' </returns>
    ''' <remarks>Calls UpdateSequenceMass</remarks>
    Public Overridable Function SetSequence(
      sequence As String,
      Optional eNTerminus As NTerminusGroupConstants = NTerminusGroupConstants.Hydrogen,
      Optional eCTerminus As CTerminusGroupConstants = CTerminusGroupConstants.Hydroxyl,
      Optional is3LetterCode As Boolean = False,
      Optional oneLetterCheckForPrefixAndSuffixResidues As Boolean = True,
      Optional threeLetterCheckForPrefixHandSuffixOH As Boolean = True) As Integer

        Dim sequenceStrLength As Integer, index As Integer
        Dim oneLetterSymbol As String

        mResidues = String.Empty

        sequence = sequence.Trim
        sequenceStrLength = sequence.Length
        If sequenceStrLength = 0 Then
            UpdateSequenceMass()
            Return 0
        End If

        If Not is3LetterCode Then
            ' Sequence is 1 letter codes

            If oneLetterCheckForPrefixAndSuffixResidues Then
                sequence = CheckForAndRemovePrefixAndSuffixResidues(sequence)
                sequenceStrLength = sequence.Length
            End If

            ' Now parse the string of 1 letter characters
            For index = 0 To sequenceStrLength - 1
                If Char.IsLetter(sequence.Chars(index)) Then
                    ' Character found
                    mResidues &= sequence.Chars(index)
                Else
                    ' Ignore anything else
                End If
            Next index

        Else
            ' Sequence is 3 letter codes
            If bln3LetterCheckForPrefixHandSuffixOH Then
                ' Look for a leading H or trailing OH, provided those don't match any of the amino acids
                RemoveLeadingH(sequence)
                RemoveTrailingOH(sequence)

                ' Recompute sequence length
                sequenceStrLength = sequence.Length
            End If

            index = 0
            Do While index <= sequenceStrLength - 3
                If Char.IsLetter(sequence.Chars(index)) Then
                    If Char.IsLetter(sequence.Chars(index + 1)) And
                       Char.IsLetter(sequence.Chars(index + 2)) Then


                        oneLetterSymbol = GetAminoAcidSymbolConversion(sequence.Substring(index, 3), False)

                        If oneLetterSymbol.Length = 0 Then
                            ' 3 letter symbol not found
                            ' Add anyway, but mark as X
                            oneLetterSymbol = UNKNOWN_SYMBOL_ONE_LETTER
                        End If

                        mResidues &= oneLetterSymbol
                        index += 3
                    Else
                        ' First letter is a character, but next two are not; ignore it
                        index += 1
                    End If
                Else
                    ' Ignore anything else
                    index += 1
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

        Return 0

    End Function

    ''' <summary>
    ''' Updates the sequence without performing any error checking
    ''' Does not look for or remove prefix or suffix letters
    ''' </summary>
    ''' <param name="sequenceNoPrefixOrSuffix"></param>
    ''' <remarks>Calls UpdateSequenceMass</remarks>
    Public Overridable Sub SetSequenceOneLetterCharactersOnly(sequenceNoPrefixOrSuffix As String)
        mResidues = sequenceNoPrefixOrSuffix
        UpdateSequenceMass()
    End Sub

    ''' <summary>
    ''' Computes mass for each residue in mResidues
    ''' Only process one letter amino acid abbreviations
    ''' </summary>
    Private Sub UpdateSequenceMass()

        Dim runningTotal As Double
        Dim protonatedNTerminus As Boolean

        Dim index As Integer

        If mDelayUpdateResidueMass Then Exit Sub

        If mResidues.Length = 0 Then
            runningTotal = 0
        Else
            ' The N-terminus ions are the basis for the running total
            ValidateTerminusMasses()

            runningTotal = mNTerminus.Mass
            If mNTerminus.Formula.ToUpper = "HH" Then
                ' ntgNTerminusGroupConstants.HydrogenPlusProton; since we add back in the proton below when computing the fragment masses,
                '  we need to subtract it out here
                ' However, we need to subtract out mHydrogenMass, and not mChargeCarrierMass since the current
                '  formula's mass was computed using two hydrogen atoms, and not one hydrogen and one charge carrier
                protonatedNTerminus = True
                runningTotal = runningTotal - mHydrogenMass
            End If

            For index = 0 To mResidues.Length - 1
                Try
                    Dim oneLetterSymbol = mResidues.Chars(index)

                    runningTotal += AminoAcidMasses(oneLetterSymbol)

                    If oneLetterSymbol = "C"c AndAlso CysTreatmentMode <> CysTreatmentModeConstants.Untreated Then
                        Select Case CysTreatmentMode
                            Case CysTreatmentModeConstants.Iodoacetamide
                                runningTotal += mIodoacetamideMass

                            Case CysTreatmentModeConstants.IodoaceticAcid
                                runningTotal += mIodoaceticAcidMass
                        End Select

                    End If

                Catch ex As Exception
                    ' Skip this residue
                    Console.WriteLine("Error parsing Residue symbols in UpdateSequenceMass; " & ex.Message)
                End Try
            Next index

            runningTotal += mCTerminus.Mass
            If protonatedNTerminus Then
                runningTotal += mChargeCarrierMass
            End If
        End If

        mTotalMassElementMode = mCurrentElementMode
        mTotalMass = runningTotal

    End Sub

    Private Sub UpdateStandardMasses()
        Const DEFAULT_CHARGE_CARRIER_MASS_AVG = 1.00739
        Const DEFAULT_CHARGE_CARRIER_MASS_MONOISOTOPIC = 1.00727649

        If mCurrentElementMode = ElementModeConstants.AverageMass Then
            mChargeCarrierMass = DEFAULT_CHARGE_CARRIER_MASS_AVG
        Else
            mChargeCarrierMass = DEFAULT_CHARGE_CARRIER_MASS_MONOISOTOPIC
        End If

        ' Update Hydrogen mass
        mHydrogenMass = ComputeFormulaWeightCHNOSP("H")

        mIodoacetamideMass = ComputeFormulaWeightCHNOSP("C2H3NO")
        mIodoaceticAcidMass = ComputeFormulaWeightCHNOSP("C2H2O2")

    End Sub

    Private Sub ValidateTerminusMasses()
        If mNTerminus.MassElementMode <> mCurrentElementMode Then
            mNTerminus.Mass = ComputeFormulaWeightCHNOSP(mNTerminus.Formula)
            mNTerminus.MassElementMode = mCurrentElementMode
        End If

        If mCTerminus.MassElementMode <> mCurrentElementMode Then
            mCTerminus.Mass = ComputeFormulaWeightCHNOSP(mCTerminus.Formula)
            mCTerminus.MassElementMode = mCurrentElementMode
        End If
    End Sub

    Private Sub InitializeSharedData()

        If ElementMasses Is Nothing Then
            ElementMasses = New Dictionary(Of Char, Double)
        Else
            ElementMasses.Clear()
        End If

        If mCurrentElementMode = ElementModeConstants.IsotopicMass Then
            ' Isotopic masses
            ElementMasses.Add("C"c, 12.0)
            ElementMasses.Add("H"c, 1.0078246)
            ElementMasses.Add("N"c, 14.003074)
            ElementMasses.Add("O"c, 15.994915)
            ElementMasses.Add("S"c, 31.972072)
            ElementMasses.Add("P"c, 30.973763)
        Else
            ' Average masses
            ElementMasses.Add("C"c, 12.0107)
            ElementMasses.Add("H"c, 1.00794)
            ElementMasses.Add("N"c, 14.00674)
            ElementMasses.Add("O"c, 15.9994)
            ElementMasses.Add("S"c, 32.066)
            ElementMasses.Add("P"c, 30.973761)
        End If

        If AminoAcidMasses Is Nothing Then
            AminoAcidMasses = New Dictionary(Of Char, Double)
        Else
            AminoAcidMasses.Clear()
        End If

        If AminoAcidSymbols Is Nothing Then
            AminoAcidSymbols = New Dictionary(Of Char, String)
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
