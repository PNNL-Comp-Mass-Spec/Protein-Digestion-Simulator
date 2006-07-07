Option Strict On
'
' This class can be used to calculate the mass of an amino acid sequence (peptide or protein)
' The protein can be in one or three letter abbreviation format
' Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in October 2004
' Copyright 2005, Battelle Memorial Institute.  All Rights Reserved.
'
' The class is a port of the MWPeptideClass used in the molecular weight calculator
'
' THE CLASS IS UNFINISHED; I DECIDED TO CREATE A MUCH LEANER CLASS THAT ONLY HANDLES ONE LETTER AMINO ACID SYMBOLS
' RATHER THAN FINISH THIS ONE
'
' Last Modified October 2, 2004
'

Public Class PeptideSequenceClass_INCOMPLETE

    Private Const MAX_MODIFICATIONS As Short = 6             ' Maximum number of modifications for a single residue
    Private Const UNKNOWN_SYMBOL As String = "Xxx"
    Private Const UNKNOWN_SYMBOL_ONE_LETTER As String = "X"
    Private Const ION_TYPE_COUNT As Short = 3

    Private Const TERMINII_SYMBOL As Char = "-"c
    Private Const TRYPTIC_RULE_RESIDUES As String = "KR"
    Private Const TRYPTIC_EXCEPTION_RESIDUES As String = "P"

    Private Const SHOULDER_ION_PREFIX As String = "Shoulder-"

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

    Public Enum IonTypeConstants
        AIon = 0
        BIon = 1
        YIon = 2
    End Enum

    Public Enum ElementModeConstants
        AverageMass = 1
        IsotopicMass = 2
    End Enum

    Private Structure udtModificationSymbolType
        Public Symbol As String                        ' Symbol used for modification in formula; may be 1 or more characters; for example: + ++ * ** etc.
        Public ModificationMass As Double              ' Normally positive, but could be negative
        Public IndicatesPhosphorylation As Boolean     ' When true, then this symbol means a residue is phosphorylated
        Public Comment As String
    End Structure

    Private Structure udtResidueType
        Public Symbol As String                 ' 3 letter symbol
        Public Mass As Double                   ' The mass of the residue alone (excluding any modification)
        Public MassWithMods As Double           ' The mass of the residue, including phosphorylation or any modification
        Public IonMass() As Double              ' 0-based array, length ION_TYPE_COUNT; the masses that the a, b, and y ions ending/starting with this residue will produce in the mass spectrum (includes H+)
        Public Phosphorylated As Boolean        ' Technically, only Ser, Thr, or Tyr residues can be phosphorylated (H3PO4), but if the user phosphorylates other residues, we'll allow that
        Public ModificationIDCount As Integer
        Public ModificationIDs() As Integer        ' 0-based array, length MAX_MODIFICATIONS
    End Structure

    Private Structure udtTerminusType
        Public Formula As String
        Public Mass As Double
        Public PrecedingResidue As udtResidueType      ' If the peptide sequence is part of a protein, the user can record the final residue of the previous peptide sequence here
        Public FollowingResidue As udtResidueType      ' If the peptide sequence is part of a protein, the user can record the first residue of the next peptide sequence here
    End Structure

    Public Structure udtAbbrevStatsType
        Public Symbol As String            ' 3 letter symbol for the amino acid residue
        Public Formula As String
        Public Mass As Double              ' Computed mass for quick reference
        Public Charge As Single
        Public OneLetterSymbol As String   ' Only used for amino acids
        Public Comment As String           ' Description of the abbreviation
        Public InvalidSymbolOrFormula As Boolean
    End Structure

    Public Structure udtFragmentionSpectrumIntensitiesType
        Public IonType() As Double              ' 0-based array, length ION_TYPE_COUNT
        Public BYIonShoulder As Double          ' If > 0 then shoulder ions will be created by B and Y ions
        Public NeutralLoss As Double
    End Structure

    Public Structure udtIonTypeOptionsType
        Public ShowIon As Boolean
        Public NeutralLossWater As Boolean
        Public NeutralLossAmmonia As Boolean
        Public NeutralLossPhosphate As Boolean
    End Structure

    Public Structure udtFragmentationSpectrumOptionsType
        Public IntensityOptions As udtFragmentionSpectrumIntensitiesType
        Public IonTypeOptions() As udtIonTypeOptionsType                   ' 0-based array, length ION_TYPE_COUNT
        Public DoubleChargeIonsShow As Boolean
        Public DoubleChargeIonsThreshold As Single
    End Structure

    Public Structure udtFragmentationSpectrumDataType
        Public Mass As Double
        Public Intensity As Double
        Public Symbol As String                        ' The symbol, with the residue number (e.g. y1, y2, b3-H2O, Shoulder-y1, etc.)
        Public SymbolGeneric As String                 ' The symbol, without the residue number (e.g. a, b, y, b++, Shoulder-y, etc.)
        Public SourceResidueNumber As Integer             ' The residue number that resulted in this mass
        Public SourceResidueSymbol3Letter As String    ' The residue symbol that resulted in this mass
        Public Charge As Integer
        Public IonType As IonTypeConstants
        Public IsShoulderIon As Boolean                ' B and Y ions can have Shoulder ions at +-1
    End Structure

    ' Variables shared across all instances of this class
    Private Shared mSharedArraysInitialized As Boolean
    Private Shared AminoAcidStats() As udtAbbrevStatsType       ' Amino acid symbols and masses; 0-based array
    Private Shared htOneLetterSymbolMap As Hashtable            ' Maps one letter symbol to entry in AminoAcidStats
    Private Shared htThreeLetterSymbolMap As Hashtable          ' Maps three letter symbol to entry in AminoAcidStats

    Private Shared mWaterLossSymbol As String          ' -H2O
    Private Shared mAmmoniaLossSymbol As String        ' -NH3
    Private Shared mPhosphoLossSymbol As String        ' -H3PO4

    Private Shared mCurrentElementMode As ElementModeConstants
    Private Shared mHOHMass As Double
    Private Shared mNH3Mass As Double
    Private Shared mH3PO4Mass As Double
    Private Shared mPhosphorylationMass As Double    ' H3PO4 minus HOH = 79.9663326
    Private Shared mHydrogenMass As Double           ' Mass of hydrogen
    Private Shared mChargeCarrierMass As Double      ' H minus one electron

    Private Shared mImmoniumMassDifference As Double ' CO minus H = 26.9871

    Private Shared mHistidineFW As Double            ' 110
    Private Shared mPhenylalanineFW As Double        ' 120
    Private Shared mTyrosineFW As Double             ' 136


    ' Note: A peptide goes from N to C, eg. HGlyLeuTyrOH has N-Terminus = H and C-Terminus = OH
    ' Residue 1 would be Gly, Residue 2 would be Leu, Residue 3 would be Tyr
    Private Residues() As udtResidueType        ' 0-based array
    Private ResidueCount As Integer

    ' ModificationSymbols() holds a list of the potential modification symbols and the mass of each modification
    ' Modification symbols can be 1 or more letters long
    Private ModificationSymbols() As udtModificationSymbolType      ' 0-based array
    Private ModificationSymbolCount As Integer

    Private mNTerminus As udtTerminusType       ' Formula on the N-Terminus
    Private mCTerminus As udtTerminusType       ' Formula on the C-Terminus
    Private mTotalMass As Double

    Private mFragSpectrumOptions As udtFragmentationSpectrumOptionsType

    Private mDelayUpdateResidueMass As Boolean

    Public Shared Property WaterLossSymbol() As String
        Get
            Return mWaterLossSymbol
        End Get
        Set(ByVal Value As String)
            If Not Value Is Nothing AndAlso Value.Length > 0 Then mWaterLossSymbol = Value
        End Set
    End Property

    Public Shared Property PhosphoLossSymbol() As String
        Get
            Return mPhosphoLossSymbol
        End Get
        Set(ByVal Value As String)
            If Not Value Is Nothing AndAlso Value.Length > 0 Then mPhosphoLossSymbol = Value
        End Set
    End Property

    Public Shared Property AmmoniaLossSymbol() As String
        Get
            Return mAmmoniaLossSymbol
        End Get
        Set(ByVal Value As String)
            If Not Value Is Nothing AndAlso Value.Length > 0 Then mAmmoniaLossSymbol = Value
        End Set
    End Property

    Private Sub AddAminoAcidStatEntry(ByVal intAbbrevIndex As Integer, ByVal strSymbol As String, ByVal strFormula As String, ByVal sngCharge As Single, ByVal strOneLetter As String, ByVal strComment As String, Optional ByVal blnInvalidSymbolOrFormula As Boolean = False)
        ' Note: strFormula can only contain C, H, N, O, S, or P, and cannot contain any parentheses or other advanced formula features

        With AminoAcidStats(intAbbrevIndex)
            .InvalidSymbolOrFormula = blnInvalidSymbolOrFormula
            .Symbol = strSymbol
            .Formula = strFormula
            .Mass = ComputeFormulaWeightCHNOSP(strFormula)
            If .Mass < 0 Then
                ' Error occurred computing mass for abbreviation
                .Mass = 0
                .InvalidSymbolOrFormula = True
            End If
            .Charge = sngCharge
            .OneLetterSymbol = strOneLetter.ToUpper
            .Comment = strComment

            If .Symbol.Length > 0 Then
                htThreeLetterSymbolMap.Add(.Symbol, intAbbrevIndex)
            End If

            If .OneLetterSymbol.Length > 0 Then
                htOneLetterSymbolMap.Add(.OneLetterSymbol, intAbbrevIndex)
            End If
        End With
    End Sub

    Private Sub AppendDataToFragSpectrum(ByRef intIonCount As Integer, ByRef FragSpectrumWork() As udtFragmentationSpectrumDataType, ByVal sngMass As Single, ByVal sngIntensity As Single, ByVal strIonSymbol As String, ByVal strIonSymbolGeneric As String, ByVal intSourceResidue As Integer, ByVal strSourceResidueSymbol3Letter As String, ByVal intCharge As Integer, ByVal eIonType As IonTypeConstants, ByVal blnIsShoulderIon As Boolean)
        On Error GoTo AppendDataPointErrorHandler

        If intIonCount >= FragSpectrumWork.Length Then
            ' This shouldn't happen
            Debug.Assert(False, "intIounCount was out of bounds, this is unexpected")
            ReDim Preserve FragSpectrumWork(FragSpectrumWork.Length + 10)
        End If

        With FragSpectrumWork(intIonCount)
            .Mass = sngMass
            .Intensity = sngIntensity
            .Symbol = strIonSymbol
            .SymbolGeneric = strIonSymbolGeneric
            .SourceResidueNumber = intSourceResidue
            .SourceResidueSymbol3Letter = strSourceResidueSymbol3Letter
            .Charge = intCharge
            .IonType = eIonType
            .IsShoulderIon = blnIsShoulderIon
        End With
        intIonCount += 1
        Exit Sub

AppendDataPointErrorHandler:
        Debug.WriteLine(Err.Description)
        Debug.Assert(False, Err.Description)
    End Sub

    Private Function CheckForModifications(ByVal strPartialSequence As String, ByVal intResidueIndex As Integer, Optional ByVal blnAddMissingModificationSymbols As Boolean = False) As Integer
        ' Looks at strPartialSequence to see if it contains 1 or more modifications
        ' If any modification symbols are found, the modification is recorded in .ModificationIDs()
        ' If all or part of the modification symbol is not found in ModificationSymbols(), then a new entry
        '  is added to ModificationSymbols()
        ' Returns the total length of all modifications found

        Dim intCompareIndex As Integer, intSequenceStrLength As Integer
        Dim strModSymbolGroup As String
        Dim intModificationID As Integer, intModSymbolLengthTotal As Integer
        Dim strTestChar As String
        Dim intIndex As Integer
        Dim blnMatchFound As Boolean

        intSequenceStrLength = strPartialSequence.Length

        ' Find the entire group of potential modification symbols
        strModSymbolGroup = ""
        intCompareIndex = 0
        Do While intCompareIndex < intSequenceStrLength
            strTestChar = strPartialSequence.Substring(intCompareIndex, 1)
            If IsModSymbol(strTestChar) Then
                strModSymbolGroup &= strTestChar
            Else
                Exit Do
            End If
            intCompareIndex += 1
        Loop

        intModSymbolLengthTotal = strModSymbolGroup.Length
        Do While strModSymbolGroup.Length > 0
            ' Step through strModSymbolGroup to see if all of it or parts of it match any of the defined
            '  modification symbols

            blnMatchFound = False
            For intIndex = strModSymbolGroup.Length - 1 To 0 Step -1
                ' See if the modification is already defined
                intModificationID = GetModificationSymbolID(strModSymbolGroup.Substring(1, intIndex + 1))
                If intModificationID >= 0 Then
                    blnMatchFound = True
                    Exit For
                End If
            Next intIndex

            If Not blnMatchFound Then
                If blnAddMissingModificationSymbols Then
                    ' Add strModSymbolGroup as a new modification, using a mass of 0 since we don't know the modification mass
                    SetModificationSymbol(strModSymbolGroup, 0, False, "")
                    blnMatchFound = True
                Else
                    ' Ignore the modification
                    strModSymbolGroup = "0"
                End If
                strModSymbolGroup = ""
            End If

            If blnMatchFound Then
                ' Record the modification for this residue
                With Residues(intResidueIndex)
                    If .ModificationIDCount < MAX_MODIFICATIONS Then
                        .ModificationIDCount = .ModificationIDCount + 1
                        .ModificationIDs(.ModificationIDCount) = intModificationID
                        If ModificationSymbols(intModificationID).IndicatesPhosphorylation Then
                            .Phosphorylated = True
                        End If
                    End If
                End With

                If intIndex < Len(strModSymbolGroup) Then
                    ' Remove the matched portion from strModSymbolGroup and test again
                    strModSymbolGroup = Mid(strModSymbolGroup, intIndex + 1)
                Else
                    strModSymbolGroup = ""
                End If
            End If
        Loop

        CheckForModifications = intModSymbolLengthTotal

    End Function

    Private Function ComputeFormulaWeightCHNOSP(ByVal strFormula As String) As Double
        ' Very simple mass computation utility; only considers elements C, H, N, O, S, and P
        ' Does not handle parentheses or any other advanced formula features
        ' Returns 0 if any unknown symbols are encountered

        Dim dblMassC As Double
        Dim dblMassH As Double
        Dim dblMassN As Double
        Dim dblMassO As Double
        Dim dblMassS As Double
        Dim dblMassP As Double

        Dim dblMass As Double = 0
        Dim intIndex As Integer
        Dim intLastElementIndex As Integer = 0
        Dim strMultiplier As String = String.Empty
        Dim intMultiplier As Integer

        If mElementMassMode = ElementMassModeConstants.Isotopic Then
            ' Isotopic masses
            dblMassC = 12.0
            dblMassH = 1.0078246
            dblMassN = 14.003074
            dblMassO = 15.994915
            dblMassS = 31.972072
            dblMassP = 30.973763
        Else
            ' Average masses
            dblMassC = 12.0107
            dblMassH = 1.00794
            dblMassN = 14.00674
            dblMassO = 15.9994
            dblMassS = 32.066
            dblMassP = 30.973761
        End If

        For intIndex = 0 To strFormula.Length - 1
            If Char.IsNumber(strFormula.Chars(intIndex)) Then
                strMultiplier &= strFormula.Chars(intIndex)
            Else
                If intLastElementIndex > 0 Then
                    If strMultiplier.Length > 0 Then
                        intMultiplier = Integer.Parse(strMultiplier)
                    Else
                        intMultiplier = 1
                    End If
                    Select Case strFormula.Chars(intLastElementIndex)
                        Case "C"c : dblMass += dblMassC * intMultiplier
                        Case "H"c : dblMass += dblMassH * intMultiplier
                        Case "N"c : dblMass += dblMassN * intMultiplier
                        Case "O"c : dblMass += dblMassO * intMultiplier
                        Case "S"c : dblMass += dblMassS * intMultiplier
                        Case "P"c : dblMass += dblMassP * intMultiplier
                        Case Else
                            Return 0
                    End Select

                    strMultiplier = String.Empty
                End If
                intLastElementIndex = intIndex
            End If
        Next

        ' Handle the final element
        If intLastElementIndex > 0 Then
            If strMultiplier.Length > 0 Then
                intMultiplier = Integer.Parse(strMultiplier)
            Else
                intMultiplier = 1
            End If
            Select Case strFormula.Chars(intLastElementIndex)
                Case "C"c : dblMass += dblMassC * intMultiplier
                Case "H"c : dblMass += dblMassH * intMultiplier
                Case "N"c : dblMass += dblMassN * intMultiplier
                Case "O"c : dblMass += dblMassO * intMultiplier
                Case "S"c : dblMass += dblMassS * intMultiplier
                Case "P"c : dblMass += dblMassP * intMultiplier
                Case Else
                    Return 0
            End Select
        End If

        Return dblMass

    End Function

    Private Function ComputeMaxIonsPerResidue() As Integer
        ' Estimate the total ions per residue that will be created
        ' This number will nearly always be much higher than the number of ions that will actually
        '  be stored for a given sequence, since not all will be doubly charged, and not all will show
        '  all of the potential neutral losses

        Dim eIonIndex As Short, intIonCount As Integer

        intIonCount = 0
        With mFragSpectrumOptions
            For eIonIndex = 0 To ION_TYPE_COUNT - 1
                If .IonTypeOptions(eIonIndex).ShowIon Then
                    intIonCount = intIonCount + 1

                    If Math.Abs(.IntensityOptions.BYIonShoulder) > 0 Then
                        If eIonIndex = IonTypeConstants.BIon Or eIonIndex = IonTypeConstants.YIon Then
                            intIonCount = intIonCount + 2
                        End If
                    End If

                    If .IonTypeOptions(eIonIndex).NeutralLossAmmonia Then intIonCount = intIonCount + 1
                    If .IonTypeOptions(eIonIndex).NeutralLossPhosphate Then intIonCount = intIonCount + 1
                    If .IonTypeOptions(eIonIndex).NeutralLossWater Then intIonCount = intIonCount + 1
                End If
            Next eIonIndex

            ' Double Charge ions could be created for all ions, so simply double intIonCount
            If .DoubleChargeIonsShow Then
                intIonCount = intIonCount * 2
            End If

        End With

        ComputeMaxIonsPerResidue = intIonCount

    End Function

    Private Function FillResidueStructureUsingSymbol(ByVal strSymbol As String, Optional ByVal blnUse3LetterCode As Boolean = True) As udtResidueType
        ' Returns a variable of type udtResidueType containing strSymbol as the residue symbol
        ' If strSymbol is a valid amino acid type, then also updates udtResidue with the default information

        Dim strSymbol3Letter As String
        Dim udtResidue As udtResidueType

        If strSymbol.Length > 0 Then
            If blnUse3LetterCode Then
                strSymbol3Letter = strSymbol
            Else
                strSymbol3Letter = GetAminoAcidSymbolConversionInternal(strSymbol, True)
                If strSymbol3Letter = String.Empty Then strSymbol3Letter = strSymbol
            End If
        End If

        With udtResidue
            .Symbol = strSymbol3Letter
            .ModificationIDCount = 0
            .Phosphorylated = False
            .Mass = LookupResidueMassThreeLetter(strSymbol3Letter)
            .MassWithMods = .Mass
        End With

        FillResidueStructureUsingSymbol = udtResidue

    End Function

    Public Function GetAminoAcidSymbolConversionInternal(ByVal strSymbolToFind As String, ByVal bln1LetterTo3Letter As Boolean) As String
        ' If bln1LetterTo3Letter = True, then converting 1 letter codes to 3 letter codes
        ' Returns the symbol, if found
        ' Otherwise, returns ""
        Dim intIndex As Integer, strReturnSymbol As String, strCompareSymbol As String

        strReturnSymbol = String.Empty
        ' Use AminoAcidStats() array to lookup code
        For intIndex = 0 To AminoAcidStats.Length - 1
            If bln1LetterTo3Letter Then
                strCompareSymbol = AminoAcidStats(intIndex).OneLetterSymbol
            Else
                strCompareSymbol = AminoAcidStats(intIndex).Symbol
            End If

            If strCompareSymbol.ToUpper = strSymbolToFind.ToUpper Then
                If bln1LetterTo3Letter Then
                    strReturnSymbol = AminoAcidStats(intIndex).Symbol
                Else
                    strReturnSymbol = AminoAcidStats(intIndex).OneLetterSymbol
                End If
                Exit For
            End If
        Next intIndex

        GetAminoAcidSymbolConversionInternal = strReturnSymbol

    End Function

    Public Function GetElementMode() As ElementModeConstants
        Return mCurrentElementMode
    End Function

    Public Function GetFragmentationMasses(ByRef udtFragSpectrum() As udtFragmentationSpectrumDataType) As Integer
        ' Returns the number of ions in FragSpectrumWork()

        Dim intResidueIndex As Integer, intChargeIndex As Integer, intShoulderIndex As Integer
        Dim eIonType As IonTypeConstants
        Dim intIndex As Integer
        Dim intPredictedIonCount As Integer, intIonCount As Integer
        Dim sngIonIntensities(ION_TYPE_COUNT) As Single
        Dim sngIonShoulderIntensity As Single, sngNeutralLossIntensity As Single
        Dim blnShowDoublecharge As Boolean, sngDoubleChargeThreshold As Single
        Dim sngBaseMass As Single, sngConvolutedMass As Single, sngObservedMass As Single
        Dim strResidues As String
        Dim blnPhosphorylated As Boolean
        Dim sngIntensity As Single
        Dim strIonSymbol As String, strIonSymbolGeneric As String
        Dim FragSpectrumWork() As udtFragmentationSpectrumDataType
        Dim PointerArray() As Integer

        If ResidueCount = 0 Then
            ' No residues
            GetFragmentationMasses = 0
            Exit Function
        End If

        ' Copy some of the values from mFragSpectrumOptions to local variables to make things easier to read
        With mFragSpectrumOptions
            For eIonType = 0 To ION_TYPE_COUNT - 1
                sngIonIntensities(eIonType) = .IntensityOptions.IonType(eIonType)
            Next eIonType
            sngIonShoulderIntensity = .IntensityOptions.BYIonShoulder
            sngNeutralLossIntensity = .IntensityOptions.NeutralLoss

            blnShowDoublecharge = .DoubleChargeIonsShow
            sngDoubleChargeThreshold = .DoubleChargeIonsThreshold
        End With

        ' Populate sngIonMassesZeroBased() and sngIonIntensitiesZeroBased()
        ' Put ion descriptions in strIonSymbolsZeroBased
        intPredictedIonCount = GetFragmentationSpectrumRequiredDataPoints()

        If intPredictedIonCount = 0 Then intPredictedIonCount = ResidueCount
        ReDim FragSpectrumWork(intPredictedIonCount)

        ' Need to update the residue masses in case the modifications have changed
        UpdateResidueMasses()

        intIonCount = 0
        For intResidueIndex = 1 To ResidueCount
            With Residues(intResidueIndex)

                For eIonType = 0 To ION_TYPE_COUNT - 1
                    If mFragSpectrumOptions.IonTypeOptions(eIonType).ShowIon Then
                        If (intResidueIndex = 1 Or intResidueIndex = ResidueCount) And (eIonType = IonTypeConstants.AIon Or eIonType = IonTypeConstants.BIon) Then
                            ' Don't include a or b ions in the output masses
                        Else

                            ' Ion is used
                            sngBaseMass = .IonMass(eIonType)     ' Already in the H+ state
                            sngIntensity = sngIonIntensities(eIonType)

                            ' Get the list of residues preceding or following this residue
                            ' Note that the residue symbols are separated by a space to avoid accidental matching by the InStr() functions below
                            strResidues = GetInternalResidues(intResidueIndex, eIonType, blnPhosphorylated)

                            For intChargeIndex = 1 To 2
                                If intChargeIndex = 1 Or (intChargeIndex = 2 And blnShowDoublecharge) Then
                                    If intChargeIndex = 1 Then
                                        sngConvolutedMass = sngBaseMass
                                    Else
                                        ' Compute mass at higher charge
                                        sngConvolutedMass = ConvoluteMassInternal(sngBaseMass, 1, intChargeIndex, mChargeCarrierMass)
                                    End If

                                    If intChargeIndex > 1 And sngBaseMass < sngDoubleChargeThreshold Then
                                        ' BaseMass is below threshold, do not add to Predicted Spectrum
                                    Else
                                        ' Add ion to Predicted Spectrum

                                        ' Y Ions are numbered in decreasing order: y5, y4, y3, y2, y1
                                        ' A and B ions are numbered in increasing order: a1, a2, etc.  or b1, b2, etc.
                                        strIonSymbolGeneric = LookupIonTypeString(eIonType)
                                        If eIonType = IonTypeConstants.YIon Then
                                            strIonSymbol = strIonSymbolGeneric & Trim(Str(ResidueCount - intResidueIndex + 1))
                                        Else
                                            strIonSymbol = strIonSymbolGeneric & Trim(Str(intResidueIndex))
                                        End If

                                        If intChargeIndex = 2 Then
                                            strIonSymbol = strIonSymbol & "++"
                                            strIonSymbolGeneric = strIonSymbolGeneric & "++"
                                        End If

                                        AppendDataToFragSpectrum(intIonCount, FragSpectrumWork(), sngConvolutedMass, sngIntensity, strIonSymbol, strIonSymbolGeneric, intResidueIndex, .Symbol, intChargeIndex, eIonType, False)

                                        ' Add shoulder ions to PredictedSpectrum()
                                        '   if a B or Y ion and the shoulder intensity is > 0
                                        ' Need to use Abs() here since user can define negative theoretical intensities (which allows for plotting a spectrum inverted)
                                        If Abs(sngIonShoulderIntensity) > 0 And (eIonType = IonTypeConstants.BIon Or eIonType = IonTypeConstants.YIon) Then
                                            For intShoulderIndex = -1 To 1 Step 2
                                                sngObservedMass = sngConvolutedMass + intShoulderIndex * (1 / intChargeIndex)
                                                AppendDataToFragSpectrum(intIonCount, FragSpectrumWork(), sngObservedMass, sngIonShoulderIntensity, SHOULDER_ION_PREFIX & strIonSymbol, SHOULDER_ION_PREFIX & strIonSymbolGeneric, intResidueIndex, .Symbol, intChargeIndex, eIonType, True)
                                            Next intShoulderIndex
                                        End If

                                        ' Apply neutral loss modifications
                                        If mFragSpectrumOptions.IonTypeOptions(eIonType).NeutralLossWater Then
                                            ' Loss of water only affects Ser, Thr, Asp, or Glu (S, T, E, or D)
                                            ' See if the residues up to this point contain any of these residues
                                            If InStr(strResidues, "Ser") Or InStr(strResidues, "Thr") Or InStr(strResidues, "Glu") Or InStr(strResidues, "Asp") Then
                                                sngObservedMass = sngConvolutedMass - (mHOHMass / intChargeIndex)
                                                AppendDataToFragSpectrum(intIonCount, FragSpectrumWork(), sngObservedMass, sngNeutralLossIntensity, strIonSymbol & mWaterLossSymbol, strIonSymbolGeneric & mWaterLossSymbol, intResidueIndex, .Symbol, intChargeIndex, eIonType, False)
                                            End If
                                        End If

                                        If mFragSpectrumOptions.IonTypeOptions(eIonType).NeutralLossAmmonia Then
                                            ' Loss of Ammonia only affects Arg, Lys, Gln, or Asn (R, K, Q, or N)
                                            ' See if the residues up to this point contain any of these residues
                                            If InStr(strResidues, "Arg") Or InStr(strResidues, "Lys") Or InStr(strResidues, "Gln") Or InStr(strResidues, "Asn") Then
                                                sngObservedMass = sngConvolutedMass - (mNH3Mass / intChargeIndex)
                                                AppendDataToFragSpectrum(intIonCount, FragSpectrumWork(), sngObservedMass, sngNeutralLossIntensity, strIonSymbol & mAmmoniaLossSymbol, strIonSymbolGeneric & mAmmoniaLossSymbol, intResidueIndex, .Symbol, intChargeIndex, eIonType, False)
                                            End If
                                        End If

                                        If mFragSpectrumOptions.IonTypeOptions(eIonType).NeutralLossPhosphate Then
                                            ' Loss of phosphate only affects phosphorylated residues
                                            ' Technically, only Ser, Thr, or Tyr (S, T, or Y) can be phosphorylated, but if the user marks other residues as phosphorylated, we'll allow that
                                            ' See if the residues up to this point contain phosphorylated residues
                                            If blnPhosphorylated Then
                                                sngObservedMass = sngConvolutedMass - (mH3PO4Mass / intChargeIndex)
                                                AppendDataToFragSpectrum(intIonCount, FragSpectrumWork(), sngObservedMass, sngNeutralLossIntensity, strIonSymbol & mPhosphoLossSymbol, strIonSymbolGeneric & mPhosphoLossSymbol, intResidueIndex, .Symbol, intChargeIndex, eIonType, False)
                                            End If
                                        End If

                                    End If
                                End If
                            Next intChargeIndex
                        End If
                    End If
                Next eIonType
            End With
        Next intResidueIndex

        ' Sort arrays by mass (using a pointer array to synchronize the arrays)
        ReDim PointerArray(intIonCount)

        For intIndex = 0 To intIonCount - 1
            PointerArray(intIndex) = intIndex
        Next intIndex

        ShellSortFragSpectrum(FragSpectrumWork(), PointerArray(), 0, intIonCount - 1)

        ' Copy the data from FragSpectrumWork() to udtFragSpectrum()

        ReDim udtFragSpectrum(intIonCount)

        For intIndex = 0 To intIonCount - 1
            udtFragSpectrum(intIndex) = FragSpectrumWork(PointerArray(intIndex))
        Next intIndex

        ' Return the actual number of ions computed
        GetFragmentationMasses = intIonCount

    End Function

    Public Function GetFragmentationSpectrumRequiredDataPoints() As Integer
        ' Determines the total number of data points that will be required for a theoretical fragmentation spectrum

        GetFragmentationSpectrumRequiredDataPoints = ResidueCount * ComputeMaxIonsPerResidue()

    End Function

    Public Function GetFragmentationSpectrumOptions() As udtFragmentationSpectrumOptionsType

        On Error GoTo GetFragmentationSpectrumOptionsErrorHandler

        GetFragmentationSpectrumOptions = mFragSpectrumOptions

        Exit Function

GetFragmentationSpectrumOptionsErrorHandler:
        GeneralErrorHandler("MWPeptideClass.GetFragmentationSpectrumOptions", Err.Number)

    End Function

    Public Function GetPeptideMass() As Double
        ' Returns the mass of the entire peptide

        ' Update the residue masses in order to update mTotalMass
        UpdateResidueMasses()

        GetPeptideMass = mTotalMass
    End Function

Private Function GetInternalResidues(intCurrentResidueIndex as Integer, eIonType As IonTypeConstants, Optional ByRef blnPhosphorylated As Boolean) As String
        ' Determines the residues preceding or following the given residue (up to and including the current residue)
        ' If eIonType is a or b ions, then returns residues from the N terminus
        ' If eIonType is y ion, then returns residues from the C terminus
        ' Also, set blnPhosphorylated to true if any of the residues is Ser, Thr, or Tyr and is phosphorylated
        '
        ' Note that the residue symbols are separated by a space to avoid accidental matching by the InStr() function

        Dim strInternalResidues As String
        Dim intResidueIndex As Integer

        strInternalResidues = ""
        blnPhosphorylated = False
        If eIonType = IonTypeConstants.YIon Then
            For intResidueIndex = intCurrentResidueIndex To ResidueCount
                With Residues(intResidueIndex)
                    strInternalResidues = strInternalResidues & .Symbol & " "
                    If .Phosphorylated Then blnPhosphorylated = True
                End With
            Next intResidueIndex
        Else
            For intResidueIndex = 1 To intCurrentResidueIndex
                With Residues(intResidueIndex)
                    strInternalResidues = strInternalResidues & .Symbol & " "
                    If .Phosphorylated Then blnPhosphorylated = True
                End With
            Next intResidueIndex
        End If

        GetInternalResidues = strInternalResidues

    End Function

    Public Function GetModificationSymbol(ByVal intModificationID As Integer, ByRef strModSymbol As String, ByRef dblModificationMass As Double, ByRef blnIndicatesPhosphorylation As Boolean, ByRef strComment As String) As Integer
        ' Returns information on the modification with intModificationID
        ' Returns 0 if success, 1 if failure

        If intModificationID >= 1 And intModificationID <= ModificationSymbolCount Then
            With ModificationSymbols(intModificationID)
                strModSymbol = .Symbol
                dblModificationMass = .ModificationMass
                blnIndicatesPhosphorylation = .IndicatesPhosphorylation
                strComment = .Comment
            End With
            GetModificationSymbol = 0
        Else
            strModSymbol = ""
            dblModificationMass = 0
            blnIndicatesPhosphorylation = False
            strComment = ""
            GetModificationSymbol = 1
        End If

    End Function

    Public Function GetModificationSymbolCount() As Integer
        ' Returns the number of modifications defined

        GetModificationSymbolCount = ModificationSymbolCount
    End Function

    Public Function GetModificationSymbolID(ByVal strModSymbol As String) As Integer
        ' Returns the ID for a given modification
        ' Returns -1 if not found, the ID if found

        Dim intIndex As Integer, intModificationIDMatch As Integer

        For intIndex = 0 To ModificationSymbolCount - 1
            If ModificationSymbols(intIndex).Symbol = strModSymbol Then
                intModificationIDMatch = intIndex
                Exit For
            End If
        Next intIndex

        GetModificationSymbolID = intModificationIDMatch

    End Function

    Public Function GetResidue(ByVal intResidueNumber As Integer, ByRef strSymbol As String, ByRef dblMass As Double, ByRef blnIsModified As Boolean, ByRef intModificationCount As Integer) As Integer
        ' Returns 0 if success, 1 if failure
        If intResidueNumber >= 1 And intResidueNumber <= ResidueCount Then
            With Residues(intResidueNumber)
                strSymbol = .Symbol
                dblMass = .Mass
                blnIsModified = (.ModificationIDCount > 0)
                intModificationCount = .ModificationIDCount
            End With
            GetResidue = 0
        Else
            GetResidue = 1
        End If
    End Function

    Public Function GetResidueCount() As Integer
        GetResidueCount = ResidueCount
    End Function

    Public Function GetResidueCountSpecificResidue(ByVal strResidueSymbol As String, Optional ByVal blnUse3LetterCode As Boolean = True) As Integer
        ' Returns the number of occurrences of the given residue in the loaded sequence

        Dim strSearchResidue3Letter As String
        Dim intResidueCount As Integer
        Dim intResidueIndex As Integer

        If blnUse3LetterCode Then
            strSearchResidue3Letter = strResidueSymbol
        Else
            strSearchResidue3Letter = GetAminoAcidSymbolConversionInternal(strResidueSymbol, True)
        End If

        intResidueCount = 0
        For intResidueIndex = 0 To ResidueCount - 1
            If Residues(intResidueIndex).Symbol = strSearchResidue3Letter Then
                intResidueCount = intResidueCount + 1
            End If

        Next intResidueIndex

        GetResidueCountSpecificResidue = intResidueCount
    End Function

    Public Function GetResidueModificationIDs(ByVal intResidueNumber As Integer, ByVal intModificationIDsOneBased() As Integer) As Integer
        ' Returns the number of Modifications
        ' ReDims intModificationIDsOneBased() to hold the values

        Dim intIndex As Integer

        If intResidueNumber >= 1 And intResidueNumber <= ResidueCount Then

            With Residues(intResidueNumber)

                ' Need to use this in case the calling program is sending an array with fixed dimensions
                On Error Resume Next
                ReDim intModificationIDsOneBased(.ModificationIDCount)

                For intIndex = 1 To .ModificationIDCount
                    intModificationIDsOneBased(intIndex) = .ModificationIDs(intIndex)
                Next intIndex

                GetResidueModificationIDs = .ModificationIDCount
            End With
        Else
            GetResidueModificationIDs = 0
        End If

    End Function

    Public Function GetResidueSymbolOnly(ByVal intResidueNumber As Integer, Optional ByVal blnUse3LetterCode As Boolean = True) As String
        ' Returns the symbol at the given residue number, or "" if an invalid residue number

        Dim strSymbol As String

        If intResidueNumber >= 1 And intResidueNumber <= ResidueCount Then
            With Residues(intResidueNumber)
                strSymbol = .Symbol
            End With
            If Not blnUse3LetterCode Then strSymbol = GetAminoAcidSymbolConversionInternal(strSymbol, False)
        Else
            strSymbol = ""
        End If

        GetResidueSymbolOnly = strSymbol

    End Function

    Public Function GetSequence(Optional ByVal blnUse3LetterCode As Boolean = True, Optional ByVal blnAddSpaceEvery10Residues As Boolean = False, Optional ByVal blnSeparateResiduesWithDash As Boolean = False, Optional ByVal blnIncludeNandCTerminii As Boolean = False, Optional ByVal blnIncludeModificationSymbols As Boolean = True) As String
        ' Construct a text sequence using Residues() and the N and C Terminus info

        Dim strSequence As String, strSymbol3Letter As String, strSymbol1Letter As String
        Dim strDashAdd As String
        Dim strModSymbol As String, strModSymbolComment As String
        Dim blnIndicatesPhosphorylation As Boolean
        Dim dblModMass As Double
        Dim intIndex As Integer, intModIndex As Integer
        Dim intError As Integer

        If blnSeparateResiduesWithDash Then strDashAdd = "-" Else strDashAdd = ""

        strSequence = ""
        For intIndex = 1 To ResidueCount
            With Residues(intIndex)
                strSymbol3Letter = .Symbol
                If blnUse3LetterCode Then
                    strSequence = strSequence & strSymbol3Letter
                Else
                    strSymbol1Letter = GetAminoAcidSymbolConversionInternal(strSymbol3Letter, False)
                    If strSymbol1Letter = "" Then strSymbol1Letter = UNKNOWN_SYMBOL_ONE_LETTER
                    strSequence = strSequence & strSymbol1Letter
                End If

                If blnIncludeModificationSymbols Then
                    For intModIndex = 1 To .ModificationIDCount
                        intError = GetModificationSymbol(.ModificationIDs(intModIndex), strModSymbol, dblModMass, blnIndicatesPhosphorylation, strModSymbolComment)
                        If intError = 0 Then
                            strSequence = strSequence & strModSymbol
                        Else
                            Debug.Assert(False)
                        End If
                    Next intModIndex
                End If

            End With

            If intIndex <> ResidueCount Then
                If blnAddSpaceEvery10Residues Then
                    If intIndex Mod 10 = 0 Then
                        strSequence = strSequence & " "
                    Else
                        strSequence = strSequence & strDashAdd
                    End If
                Else
                    strSequence = strSequence & strDashAdd
                End If
            End If

        Next intIndex

        If blnIncludeNandCTerminii Then
            strSequence = mNTerminus.Formula & strDashAdd & strSequence & strDashAdd & mCTerminus.Formula
        End If

        GetSequence = strSequence
    End Function

    Public Function GetTrypticName(ByVal strProteinResidues As String, ByVal strPeptideResidues As String, Optional ByRef intReturnResidueStart as Integer = 0, Optional ByRef intReturnResidueEnd as Integer = 0, Optional ByVal blnICR2LSCompatible As Boolean, Optional ByVal strRuleResidues As String = TRYPTIC_RULE_RESIDUES, Optional ByVal strExceptionResidues As String = TRYPTIC_EXCEPTION_RESIDUES, Optional ByVal strTerminiiSymbol As String = TERMINII_SYMBOL, Optional ByVal blnIgnoreCase As Boolean = True, Optional ByVal intProteinSearchStartLoc as Integer = 1) As String
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

        Dim intStartLoc As Integer, intEndLoc As Integer
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
            strProteinResidues = UCase(strProteinResidues)
            strPeptideResidues = UCase(strPeptideResidues)
        End If

        If intProteinSearchStartLoc <= 1 Then
            intStartLoc = InStr(strProteinResidues, strPeptideResidues)
        Else
            intStartLoc = InStr(Mid(strProteinResidues, intProteinSearchStartLoc), strPeptideResidues)
            If intStartLoc > 0 Then
                intStartLoc = intStartLoc + intProteinSearchStartLoc - 1
            End If
        End If

        intPeptideResiduesLength = Len(strPeptideResidues)

        If intStartLoc > 0 And Len(strProteinResidues) > 0 And intPeptideResiduesLength > 0 Then
            intEndLoc = intStartLoc + intPeptideResiduesLength - 1

            ' Determine if the residue is tryptic
            ' Use CheckSequenceAgainstCleavageRule() for this
            If intStartLoc > 1 Then
                strPrefix = Mid(strProteinResidues, intStartLoc - 1, 1)
            Else
                strPrefix = strTerminiiSymbol
            End If

            If intEndLoc = Len(strProteinResidues) Then
                strSuffix = strTerminiiSymbol
            Else
                strSuffix = Mid(strProteinResidues, intEndLoc + 1, 1)
            End If

            blnMatchesCleavageRule = CheckSequenceAgainstCleavageRule(strPrefix & "." & strPeptideResidues & "." & strSuffix, strRuleResidues, strExceptionResidues, False, ".", strTerminiiSymbol, blnIgnoreCase)

            If blnMatchesCleavageRule Then
                ' Construct strTrypticName

                ' Determine which tryptic residue strPeptideResidues is
                If intStartLoc = 1 Then
                    intTrypticResidueNumber = 1
                Else
                    intTrypticResidueNumber = 0
                    strProteinResiduesBeforeStartLoc = Left(strProteinResidues, intStartLoc - 1)
                    strResidueFollowingSearchResidues = Left(strPeptideResidues, 1)
                    intTrypticResidueNumber = 0
                    intRuleResidueLoc = 0
                    Do
                        intRuleResidueLoc = GetTrypticNameFindNextCleavageLoc(strProteinResiduesBeforeStartLoc, strResidueFollowingSearchResidues, intRuleResidueLoc + 1, strRuleResidues, strExceptionResidues, strTerminiiSymbol)
                        If intRuleResidueLoc > 0 Then
                            intTrypticResidueNumber = intTrypticResidueNumber + 1
                        End If
                    Loop While intRuleResidueLoc > 0 And intRuleResidueLoc + 1 < intStartLoc
                    intTrypticResidueNumber = intTrypticResidueNumber + 1
                End If

                ' Determine number of K or R residues in strPeptideResidues
                ' Ignore K or R residues followed by Proline
                intRuleResidueMatchCount = 0
                intRuleResidueLoc = 0
                Do
                    intRuleResidueLoc = GetTrypticNameFindNextCleavageLoc(strPeptideResidues, strSuffix, intRuleResidueLoc + 1, strRuleResidues, strExceptionResidues, strTerminiiSymbol)
                    If intRuleResidueLoc > 0 Then
                        intRuleResidueMatchCount = intRuleResidueMatchCount + 1
                    End If
                Loop While intRuleResidueLoc > 0 And intRuleResidueLoc < intPeptideResiduesLength

                strTrypticName = "t" & Trim(Str(intTrypticResidueNumber))
                If intRuleResidueMatchCount > 1 Then
                    strTrypticName = strTrypticName & "." & Trim(Str((intRuleResidueMatchCount)))
                End If
            Else
                If blnICR2LSCompatible Then
                    strTrypticName = Trim(Str(intStartLoc)) & "." & Trim(Str(intEndLoc + 1))
                Else
                    strTrypticName = Trim(Str(intStartLoc)) & "." & Trim(Str(intEndLoc))
                End If
            End If

            intReturnResidueStart = intStartLoc
            intReturnResidueEnd = intEndLoc
            GetTrypticName = strTrypticName
        Else
            ' Residues not found
            intReturnResidueStart = 0
            intReturnResidueEnd = 0
            GetTrypticName = ""
        End If

    End Function

Public Function GetTrypticNameMultipleMatches(ByVal strProteinResidues As String, ByVal strPeptideResidues As String, Optional ByRef intReturnMatchCount as Integer, Optional ByRef intReturnResidueStart as Integer = 0, Optional ByRef intReturnResidueEnd as Integer = 0, Optional ByVal blnICR2LSCompatible As Boolean, Optional ByVal strRuleResidues As String = TRYPTIC_RULE_RESIDUES, Optional ByVal strExceptionResidues As String = TRYPTIC_EXCEPTION_RESIDUES, Optional ByVal strTerminiiSymbol As String = TERMINII_SYMBOL, Optional ByVal blnIgnoreCase As Boolean = True, Optional ByVal intProteinSearchStartLoc as Integer = 1, Optional ByVal strListDelimeter As String = ", ") As String
        ' Examines strPeptideResidues to see where they exist in strProteinResidues
        ' Looks for all possible matches, returning them as a comma separated list
        ' Returns the number of matches in intReturnMatchCount
        ' intReturnResidueStart contains the residue number of the start of the first match
        ' intReturnResidueEnd contains the residue number of the end of the last match

        ' See GetTrypticName for additional information

        Dim strNameList As String, strCurrentName As String
        Dim intCurrentSearchLoc As Integer
        Dim intCurrentResidueStart As Integer, intCurrentResidueEnd As Integer

        intCurrentSearchLoc = intProteinSearchStartLoc
        intReturnMatchCount = 0
        Do
            strCurrentName = GetTrypticName(strProteinResidues, strPeptideResidues, intCurrentResidueStart, intCurrentResidueEnd, blnICR2LSCompatible, strRuleResidues, strExceptionResidues, strTerminiiSymbol, blnIgnoreCase, intCurrentSearchLoc)

            If Len(strCurrentName) > 0 Then
                If Len(strNameList) > 0 Then
                    strNameList = strNameList & strListDelimeter
                End If
                strNameList = strNameList & strCurrentName
                intCurrentSearchLoc = intCurrentResidueEnd + 1
                intReturnMatchCount = intReturnMatchCount + 1

                If intReturnMatchCount = 1 Then
                    intReturnResidueStart = intCurrentResidueStart
                End If
                intReturnResidueEnd = intCurrentResidueEnd

                If intCurrentSearchLoc > Len(strProteinResidues) Then Exit Do
            Else
                Exit Do
            End If
        Loop

        GetTrypticNameMultipleMatches = strNameList

    End Function

    Private Function GetTrypticNameFindNextCleavageLoc(ByVal strSearchResidues As String, ByVal strResidueFollowingSearchResidues As String, ByVal intStartChar As Integer, Optional ByVal strSearchChars As String = TRYPTIC_RULE_RESIDUES, Optional ByVal strExceptionSuffixResidues As String = TRYPTIC_EXCEPTION_RESIDUES, Optional ByVal strTerminiiSymbol As String = TERMINII_SYMBOL) As Integer
        ' Finds the location of the next strSearchChar in strSearchResidues (K or R by default)
        ' Assumes strSearchResidues are already upper case
        ' Examines the residue following the matched residue
        '   If it matches one of the characters in strExceptionSuffixResidues, then the match is not counted
        ' Note that strResidueFollowingSearchResidues is necessary in case the potential cleavage residue is the final residue in strSearchResidues
        ' We need to know the next residue to determine if it matches an exception residue
        ' For example, if strSearchResidues =      "IGASGEHIFIIGVDKPNR"
        '  and the protein it is part of is: TNSANFRIGASGEHIFIIGVDKPNRQPDS
        '  and strSearchChars = "KR while strExceptionSuffixResidues  = "P"
        ' Then the K in IGASGEHIFIIGVDKPNR is ignored because the following residue is P,
        '  while the R in IGASGEHIFIIGVDKPNR is OK because strResidueFollowingSearchResidues is Q
        ' It is the calling function's responsibility to assign the correct residue to strResidueFollowingSearchResidues
        ' If no match is found, but strResidueFollowingSearchResidues is "-", then the cleavage location returned is Len(strSearchResidues) + 1

        Dim intCharLocInSearchChars As Integer
        Dim intCharLoc As Integer, intMinCharLoc As Integer
        Dim intExceptionSuffixResidueCount As Integer
        Dim intCharLocInExceptionChars As Integer
        Dim strResidueFollowingCleavageResidue As String
        Dim intExceptionCharLocInSearchResidues As Integer, intCharLocViaRecursiveSearch As Integer

        intExceptionSuffixResidueCount = Len(strExceptionSuffixResidues)

        intMinCharLoc = -1
        For intCharLocInSearchChars = 1 To Len(strSearchChars)
            intCharLoc = InStr(Mid(strSearchResidues, intStartChar), Mid(strSearchChars, intCharLocInSearchChars, 1))

            If intCharLoc > 0 Then
                intCharLoc = intCharLoc + intStartChar - 1

                If intExceptionSuffixResidueCount > 0 Then
                    ' Make sure strSuffixResidue does not match strExceptionSuffixResidues
                    If intCharLoc < Len(strSearchResidues) Then
                        intExceptionCharLocInSearchResidues = intCharLoc + 1
                        strResidueFollowingCleavageResidue = Mid(strSearchResidues, intExceptionCharLocInSearchResidues, 1)
                    Else
                        ' Matched the last residue in strSearchResidues
                        intExceptionCharLocInSearchResidues = Len(strSearchResidues) + 1
                        strResidueFollowingCleavageResidue = strResidueFollowingSearchResidues
                    End If

                    For intCharLocInExceptionChars = 1 To intExceptionSuffixResidueCount
                        If strResidueFollowingCleavageResidue = Mid(strExceptionSuffixResidues, intCharLocInExceptionChars, 1) Then
                            ' Exception char is the following character; can't count this as the cleavage point

                            If intExceptionCharLocInSearchResidues < Len(strSearchResidues) Then
                                ' Recursively call this function to find the next cleavage position, using an updated intStartChar position
                                intCharLocViaRecursiveSearch = GetTrypticNameFindNextCleavageLoc(strSearchResidues, strResidueFollowingSearchResidues, intExceptionCharLocInSearchResidues, strSearchChars, strExceptionSuffixResidues, strTerminiiSymbol)

                                If intCharLocViaRecursiveSearch > 0 Then
                                    ' Found a residue further along that is a valid cleavage point
                                    intCharLoc = intCharLocViaRecursiveSearch
                                Else
                                    intCharLoc = 0
                                End If
                            Else
                                intCharLoc = 0
                            End If
                            Exit For
                        End If
                    Next intCharLocInExceptionChars
                End If
            End If

            If intCharLoc > 0 Then
                If intMinCharLoc < 0 Then
                    intMinCharLoc = intCharLoc
                Else
                    If intCharLoc < intMinCharLoc Then
                        intMinCharLoc = intCharLoc
                    End If
                End If
            End If
        Next intCharLocInSearchChars

        If intMinCharLoc < 0 And strResidueFollowingSearchResidues = strTerminiiSymbol Then
            intMinCharLoc = Len(strSearchResidues) + 1
        End If

        If intMinCharLoc < 0 Then
            GetTrypticNameFindNextCleavageLoc = 0
        Else
            GetTrypticNameFindNextCleavageLoc = intMinCharLoc
        End If

    End Function

Public Function GetTrypticPeptideNext(ByVal strProteinResidues As String, ByVal intSearchStartLoc as Integer, Optional ByRef intReturnResidueStart as Integer, Optional ByRef intReturnResidueEnd as Integer, Optional ByVal strRuleResidues As String = TRYPTIC_RULE_RESIDUES, Optional ByVal strExceptionResidues As String = TRYPTIC_EXCEPTION_RESIDUES, Optional ByVal strTerminiiSymbol As String = TERMINII_SYMBOL) As String
        ' Returns the next tryptic peptide in strProteinResidues, starting the search as intSearchStartLoc
        ' Useful when obtaining all of the tryptic peptides for a protein, since this function will operate
        '  much faster than repeatedly calling GetTrypticPeptideByFragmentNumber()

        ' Returns the position of the start and end residues using intReturnResidueStart and intReturnResidueEnd

        Dim intRuleResidueLoc As Integer
        Dim intProteinResiduesLength As Integer

        If intSearchStartLoc < 1 Then intSearchStartLoc = 1

        intProteinResiduesLength = Len(strProteinResidues)
        If intSearchStartLoc > intProteinResiduesLength Then
            GetTrypticPeptideNext = ""
            Exit Function
        End If

        intRuleResidueLoc = GetTrypticNameFindNextCleavageLoc(strProteinResidues, strTerminiiSymbol, intSearchStartLoc, strRuleResidues, strExceptionResidues, strTerminiiSymbol)
        If intRuleResidueLoc > 0 Then
            intReturnResidueStart = intSearchStartLoc
            If intRuleResidueLoc > intProteinResiduesLength Then
                intReturnResidueEnd = intProteinResiduesLength
            Else
                intReturnResidueEnd = intRuleResidueLoc
            End If
            GetTrypticPeptideNext = Mid(strProteinResidues, intReturnResidueStart, intRuleResidueLoc - intReturnResidueStart + 1)
        Else
            intReturnResidueStart = 1
            intReturnResidueEnd = intProteinResiduesLength
            GetTrypticPeptideNext = strProteinResidues
        End If

    End Function

Public Function GetTrypticPeptideByFragmentNumber(ByVal strProteinResidues As String, ByVal intDesiredPeptideNumber As Integer, Optional ByRef intReturnResidueStart as Integer, Optional ByRef intReturnResidueEnd as Integer, Optional ByVal strRuleResidues As String = TRYPTIC_RULE_RESIDUES, Optional ByVal strExceptionResidues As String = TRYPTIC_EXCEPTION_RESIDUES, Optional ByVal strTerminiiSymbol As String = TERMINII_SYMBOL, Optional ByVal blnIgnoreCase As Boolean = True) As String
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
            GetTrypticPeptideByFragmentNumber = ""
            Exit Function
        End If

        If blnIgnoreCase Then
            strProteinResidues = UCase(strProteinResidues)
        End If
        intProteinResiduesLength = Len(strProteinResidues)

        intStartLoc = 1
        intRuleResidueLoc = 0
        intCurrentTrypticPeptideNumber = 0
        Do
            intRuleResidueLoc = GetTrypticNameFindNextCleavageLoc(strProteinResidues, strTerminiiSymbol, intStartLoc, strRuleResidues, strExceptionResidues, strTerminiiSymbol)
            If intRuleResidueLoc > 0 Then
                intCurrentTrypticPeptideNumber = intCurrentTrypticPeptideNumber + 1
                intPrevStartLoc = intStartLoc
                intStartLoc = intRuleResidueLoc + 1

                If intPrevStartLoc > intProteinResiduesLength Then
                    ' User requested a peptide number that doesn't exist
                    GetTrypticPeptideByFragmentNumber = ""
                    Exit Function
                End If
            Else
                ' I don't think I'll ever reach this code
                Debug.Assert(False)
                Exit Do
            End If
        Loop While intCurrentTrypticPeptideNumber < intDesiredPeptideNumber

        strMatchingFragment = ""
        If intCurrentTrypticPeptideNumber > 0 And intPrevStartLoc > 0 Then
            If intPrevStartLoc > Len(strProteinResidues) Then
                ' User requested a peptide number that is too high
                intReturnResidueStart = 0
                intReturnResidueEnd = 0
                strMatchingFragment = ""
            Else
                ' Match found, find the extent of this peptide
                intReturnResidueStart = intPrevStartLoc
                If intRuleResidueLoc > intProteinResiduesLength Then
                    intReturnResidueEnd = intProteinResiduesLength
                Else
                    intReturnResidueEnd = intRuleResidueLoc
                End If
                strMatchingFragment = Mid(strProteinResidues, intPrevStartLoc, intRuleResidueLoc - intPrevStartLoc + 1)
            End If
        Else
            intReturnResidueStart = 1
            intReturnResidueEnd = intProteinResiduesLength
            strMatchingFragment = strProteinResidues
        End If

        GetTrypticPeptideByFragmentNumber = strMatchingFragment

    End Function

Public Function CheckSequenceAgainstCleavageRule(ByVal strSequence As String, ByVal strRuleResidues As String, ByVal strExceptionSuffixResidues As String, ByVal blnAllowPartialCleavage As Boolean, Optional ByVal strSeparationChar As String = ".", Optional ByVal strTerminiiSymbol As String = TERMINII_SYMBOL, Optional ByVal blnIgnoreCase As Boolean = True, Optional ByRef intRuleMatchCount As Integer) As Boolean
        ' Checks strSequence to see if it matches the cleavage rule
        ' Returns True if valid, False if invalid
        ' Returns True if doesn't contain any periods, and thus, can't be examined
        ' The ByRef variable intRuleMatchCount can be used to retrieve the number of ends that matched the rule (0, 1, or 2)

        ' The residues in strRuleResidues specify the cleavage rule
        ' The peptide must end in one of the residues, or in -
        ' The preceding residue must be one of the residues or be -
        ' EXCEPTION: if blnAllowPartialCleavage = True then the rules need only apply to one end
        ' Finally, the suffix residue cannot match any of the residues in strExceptionSuffixResidues

        ' For example, if strRuleResidues = "KR" and strExceptionSuffixResidues = "P"
        ' Then if strSequence = "R.AEQDDLANYGPGNGVLPSAGSSISMEK.L" then blnMatchesCleavageRule = True
        ' However, if strSequence = "R.IGASGEHIFIIGVDK.P" then blnMatchesCleavageRule = False since strSuffix = "P"
        ' Finally, if strSequence = "R.IGASGEHIFIIGVDKPNR.Q" then blnMatchesCleavageRule = True since K is ignored, but the final R.Q is valid

        Dim strSequenceStart As String, strSequenceEnd As String
        Dim strPrefix As String, strSuffix As String
        Dim blnMatchesCleavageRule As Boolean, blnSkipThisEnd As Boolean, blnPossiblySkipEnd As Boolean
        Dim intTerminiiCount As Integer
        Dim strTestResidue As String
        Dim intEndToCheck As Integer

        ' Need to reset this to zero since passed ByRef
        intRuleMatchCount = 0

        ' First, make sure the sequence is in the form A.BCDEFG.H or A.BCDEFG or BCDEFG.H
        ' If it isn't, then we can't check it (we'll return true)

        If Len(strRuleResidues) = 0 Then
            ' No rules
            CheckSequenceAgainstCleavageRule = True
            Exit Function
        End If

        If InStr(strSequence, strSeparationChar) = 0 Then
            ' No periods, can't check
            Debug.Assert(False)
            CheckSequenceAgainstCleavageRule = True
            Exit Function
        End If

        If blnIgnoreCase Then
            strSequence = UCase(strSequence)
        End If

        ' Find the prefix residue and starting residue
        If Mid(strSequence, 2, 1) = strSeparationChar Then
            strPrefix = Left(strSequence, 1)
            strSequenceStart = Mid(strSequence, 3, 1)
        Else
            strSequenceStart = Left(strSequence, 1)
        End If

        ' Find the suffix residue and the ending residue
        If Mid(strSequence, Len(strSequence) - 1, 1) = strSeparationChar Then
            strSuffix = Right(strSequence, 1)
            strSequenceEnd = Mid(strSequence, Len(strSequence) - 2, 1)
        Else
            strSequenceEnd = Right(strSequence, 1)
        End If

        If strRuleResidues = strTerminiiSymbol Then
            ' Peptide database rules
            ' See if prefix and suffix are "" or are strTerminiiSymbol
            If (strPrefix = strTerminiiSymbol And strSuffix = strTerminiiSymbol) Or _
               (strPrefix = "" And strSuffix = "") Then
                blnMatchesCleavageRule = True
            Else
                blnMatchesCleavageRule = False
            End If
        Else
            If blnIgnoreCase Then
                strRuleResidues = UCase(strRuleResidues)
            End If

            ' Test each character in strRuleResidues against both strPrefix and strSequenceEnd
            ' Make sure strSuffix does not match strExceptionSuffixResidues
            For intEndToCheck = 0 To 1
                blnSkipThisEnd = False
                If intEndToCheck = 0 Then
                    strTestResidue = strPrefix
                    If strPrefix = strTerminiiSymbol Then
                        intTerminiiCount = intTerminiiCount + 1
                        blnSkipThisEnd = True
                    Else
                        ' See if strSequenceStart matches one of the exception residues
                        ' If it does, make sure strPrefix does not match one of the rule residues
                        blnPossiblySkipEnd = False
                        If CheckSequenceAgainstCleavageRuleMatchTestResidue(strSequenceStart, strExceptionSuffixResidues) Then
                            ' Match found; need to examine strPrefix
                            blnPossiblySkipEnd = True
                        End If

                        If blnPossiblySkipEnd Then
                            ' Make sure strPrefix does not match one of the rule residues
                            If CheckSequenceAgainstCleavageRuleMatchTestResidue(strPrefix, strRuleResidues) Then
                                ' Match found; thus does not match cleavage rule
                                blnSkipThisEnd = True
                            End If
                        End If
                    End If
                Else
                    strTestResidue = strSequenceEnd
                    If strSuffix = strTerminiiSymbol Then
                        intTerminiiCount = intTerminiiCount + 1
                        blnSkipThisEnd = True
                    Else
                        ' Make sure strSuffix does not match strExceptionSuffixResidues
                        If CheckSequenceAgainstCleavageRuleMatchTestResidue(strSuffix, strExceptionSuffixResidues) Then
                            ' Match found; thus does not match cleavage rule
                            blnSkipThisEnd = True
                        End If
                    End If
                End If

                If Not blnSkipThisEnd Then
                    If CheckSequenceAgainstCleavageRuleMatchTestResidue(strTestResidue, strRuleResidues) Then
                        intRuleMatchCount = intRuleMatchCount + 1
                    End If
                End If
            Next intEndToCheck

            If intRuleMatchCount > 0 Then
                If intRuleMatchCount = 2 Then
                    blnMatchesCleavageRule = True
                ElseIf intRuleMatchCount >= 1 And intTerminiiCount >= 1 Then
                    blnMatchesCleavageRule = True
                    ' Bump up intRuleMatchCount to 2 since having at least one terminus and
                    ' and one rule match essentially means two rule matches
                    ' E.g., for RuleResidues = "KR", and sequence = "-.ABCDER.-", this is fully tryptic,
                    '  so we should set intRuleMatchCount to 2
                    intRuleMatchCount = 2
                ElseIf intRuleMatchCount >= 1 And blnAllowPartialCleavage Then
                    blnMatchesCleavageRule = True
                End If
            ElseIf intTerminiiCount = 2 Then
                blnMatchesCleavageRule = True
                ' Both ends are terminii, bump up intRuleMatchCount to 2
                intRuleMatchCount = 2
            End If
        End If

        CheckSequenceAgainstCleavageRule = blnMatchesCleavageRule

    End Function

    Private Function CheckSequenceAgainstCleavageRuleMatchTestResidue(ByVal strTestResidue As String, ByVal strRuleResidues As String) As Boolean
        ' Checks to see if strTestResidue matches one of the residues in strRuleResidues
        ' Used to test by Rule Residues and Exception Residues

        Dim intCharLocInRuleResidues As Integer
        Dim strCompareResidue As String
        Dim blnMatchFound As Boolean

        For intCharLocInRuleResidues = 1 To Len(strRuleResidues)
            strCompareResidue = Trim(Mid(strRuleResidues, intCharLocInRuleResidues, 1))
            If Len(strCompareResidue) > 0 Then
                If strTestResidue = strCompareResidue Then
                    blnMatchFound = True
                    Exit For
                End If
            End If
        Next intCharLocInRuleResidues

        CheckSequenceAgainstCleavageRuleMatchTestResidue = blnMatchFound

    End Function

    Public Function ComputeImmoniumMass(ByVal dblResidueMass As Double) As Double
        ComputeImmoniumMass = dblResidueMass - mImmoniumMassDifference
    End Function

    Public Function IsModSymbol(ByVal strTestChar As String) As Boolean
        ' Returns True if the first letter of strTestChar is a ModSymbol
        ' Invalid Mod Symbols are letters, numbers, ., -, space, (, or )
        ' Valid Mod Symbols are ! # $ % & ' * + ? ^ _ ` ~

        Dim strFirstChar As Char
        Dim blnIsModSymbol As Boolean

        blnIsModSymbol = False
        If Not strTestChar Is Nothing AndAlso strTestChar.Length > 0 Then
            strFirstChar = strTestChar.Chars(0)

            Select Case Asc(strFirstChar)
                Case 34           ' " is not allowed
                    blnIsModSymbol = False
                Case 40 To 41     ' ( and ) are not allowed
                    blnIsModSymbol = False
                Case 44 To 62     ' . and - and , and / and numbers and : and ; and < and = and > are not allowed
                    blnIsModSymbol = False
                Case 33 To 43, 63 To 64, 94 To 96, 126
                    blnIsModSymbol = True
                Case Else
                    blnIsModSymbol = False
            End Select
        Else
            blnIsModSymbol = False
        End If

        IsModSymbol = blnIsModSymbol

    End Function

    Public Function LookupIonTypeString(ByVal eIonType As IonTypeConstants) As String

        Select Case eIonType
            Case IonTypeConstants.AIon : Return "a"
            Case IonTypeConstants.BIon : Return "b"
            Case IonTypeConstants.YIon : Return "y"
            Case Else : Return ""
        End Select

    End Function

    Private Function LookupResidueMassOneLetter(ByVal strSymbolOneLetter As String) As Double

    End Function

    Public Function RemoveAllResidues() As Integer
        ' Removes all the residues
        ' Returns 0 on success, 1 on failure

        ReserveMemoryForResidues(50, False)
        ResidueCount = 0
        mTotalMass = 0

        RemoveAllResidues = 0
    End Function

    Public Function RemoveAllModificationSymbols() As Integer
        ' Removes all possible Modification Symbols
        ' Returns 0 on success, 1 on failure
        ' Removing all modifications will invalidate any modifications present in a sequence

        ReserveMemoryForModifications(10, False)
        ModificationSymbolCount = 0

        RemoveAllModificationSymbols = 0
    End Function

    Private Function RemoveLeadingH(ByRef strWorkingSequence As String) As Boolean
        ' Returns True if a leading H is removed
        Dim intAbbrevID As Integer
        Dim blnHRemoved As Boolean

        blnHRemoved = False
        If UCase(Left(strWorkingSequence, 1)) = "H" And Len(strWorkingSequence) >= 4 Then
            ' If next character is not a character, then remove the H
            If Not IsCharacter(Mid(strWorkingSequence, 2, 1)) Then
                strWorkingSequence = Mid(strWorkingSequence, 3)
                blnHRemoved = True
            Else
                ' Otherwise, see if next three characters are letters
                If IsCharacter(Mid(strWorkingSequence, 2, 1)) And _
                   IsCharacter(Mid(strWorkingSequence, 3, 1)) And _
                   IsCharacter(Mid(strWorkingSequence, 4, 1)) Then
                    ' Formula starts with 4 characters and the first is H, see if the first 3 characters are a valid amino acid code
                    intAbbrevID = GetAbbreviationIDInternal(Left(strWorkingSequence, 3), True)

                    If intAbbrevID <= 0 Then
                        ' Doesn't start with a valid amino acid 3 letter abbreviation, so remove the initial H
                        strWorkingSequence = Mid(strWorkingSequence, 2)
                        blnHRemoved = True
                    End If
                End If
            End If
        End If

        RemoveLeadingH = blnHRemoved
    End Function

    Private Function RemoveTrailingOH(ByRef strWorkingSequence As String) As Boolean
        ' Returns True if a trailing OH is removed
        Dim intAbbrevID As Integer
        Dim blnOHRemoved As Boolean
        Dim intStringLength As Integer

        blnOHRemoved = False
        intStringLength = Len(strWorkingSequence)
        If UCase(Right(strWorkingSequence, 2)) = "OH" And intStringLength >= 5 Then
            ' If previous character is not a character, then remove the OH
            If Not IsCharacter(Mid(strWorkingSequence, intStringLength - 2, 1)) Then
                strWorkingSequence = Left(strWorkingSequence, intStringLength - 3)
                blnOHRemoved = True
            Else
                ' Otherwise, see if previous three characters are letters
                If IsCharacter(Mid(strWorkingSequence, intStringLength - 2, 1)) Then
                    ' Formula ends with 3 characters and the last two are OH, see if the last 3 characters are a valid amino acid code
                    intAbbrevID = GetAbbreviationIDInternal(Mid(strWorkingSequence, intStringLength - 2, 3), True)

                    If intAbbrevID <= 0 Then
                        ' Doesn't end with a valid amino acid 3 letter abbreviation, so remove the trailing OH
                        strWorkingSequence = Left(strWorkingSequence, intStringLength - 2)
                        blnOHRemoved = True
                    End If
                End If
            End If
        End If

        RemoveTrailingOH = blnOHRemoved

    End Function

    Public Function RemoveModification(ByVal strModSymbol As String) As Integer
        ' Returns 0 if found and removed; 1 if error

        Dim intIndex As Integer
        Dim blnRemoved As Boolean

        For intIndex = 1 To ModificationSymbolCount
            If ModificationSymbols(intIndex).Symbol = strModSymbol Then
                RemoveModificationByID(intIndex)
                blnRemoved = True
            End If
        Next intIndex

        If blnRemoved Then
            RemoveModification = 0
        Else
            RemoveModification = 1
        End If
    End Function

    Public Function RemoveModificationByID(ByVal intModificationID As Integer) As Integer
        ' Returns 0 if found and removed; 1 if error

        Dim intIndex As Integer
        Dim blnRemoved As Boolean

        If intModificationID >= 1 And intModificationID <= ModificationSymbolCount Then
            For intIndex = intModificationID To ModificationSymbolCount - 1
                ModificationSymbols(intIndex) = ModificationSymbols(intIndex + 1)
            Next intIndex
            ModificationSymbolCount = ModificationSymbolCount - 1
            blnRemoved = True
        Else
            blnRemoved = False
        End If

        If blnRemoved Then
            RemoveModificationByID = 0
        Else
            RemoveModificationByID = 1
        End If

    End Function

    Public Function RemoveResidue(ByVal intResidueNumber As Integer) As Integer
        ' Returns 0 if found and removed; 1 if error

        Dim intIndex As Integer

        If intResidueNumber >= 1 And intResidueNumber <= ResidueCount Then
            For intIndex = intResidueNumber To ResidueCount - 1
                Residues(intIndex) = Residues(intIndex + 1)
            Next intIndex
            ResidueCount = ResidueCount - 1
            RemoveResidue = 0
        Else
            RemoveResidue = 1
        End If

    End Function

    Private Sub ReserveMemoryForResidues(ByVal intNewResidueCount As Integer, ByVal blnPreserveContents As Boolean)
        ' Only reserves the memory if necessary
        ' Thus, do not use this sub to clear Residues()

        If intNewResidueCount >= Residues.Length Then
            If blnPreserveContents Then
                ReDim Preserve Residues(Residues.Length + 50)
            Else
                ReDim Residues(50)
            End If
        End If
    End Sub

    Private Sub ReserveMemoryForModifications(ByVal intNewModificationCount As Integer, ByVal blnPreserveContents As Boolean)

        If intNewModificationCount >= ModificationSymbols.Length Then
            If blnPreserveContents Then
                ReDim Preserve ModificationSymbols(ModificationSymbols.Length + 10)
            Else
                ReDim ModificationSymbols(10)
            End If
        End If
    End Sub

    Private Function SetCTerminus(ByVal strFormula As String, Optional ByVal strFollowingResidue As String = "", Optional ByVal blnUse3LetterCode As Boolean = True) As Integer
        ' Note: strFormula can only contain C, H, N, O, S, or P, and cannot contain any parentheses or other advanced formula features
        ' Returns 0 if success; 1 if error

        ' Typical N terminus mods
        ' Free Acid = OH
        ' Amide = NH2

        With mCTerminus
            .Formula = strFormula
            .Mass = ComputeFormulaWeightCHNOSP(.Formula)
            If .Mass < 0 Then
                .Mass = 0
                SetCTerminus = 1
            Else
                SetCTerminus = 0
            End If
            .PrecedingResidue = FillResidueStructureUsingSymbol("")
            .FollowingResidue = FillResidueStructureUsingSymbol(strFollowingResidue, blnUse3LetterCode)
        End With

        UpdateResidueMasses()
    End Function

    Public Function SetCTerminusGroup(ByVal eCTerminusGroup As ctgCTerminusGroupConstants, Optional ByVal strFollowingResidue As String = "", Optional ByVal blnUse3LetterCode As Boolean = True) As Integer
        ' Returns 0 if success; 1 if error
        Dim intError As Integer

        intError = 0
        Select Case eCTerminusGroup
            Case CTerminusGroupConstants.Hydroxyl : intError = SetCTerminus("OH", strFollowingResidue, blnUse3LetterCode)
            Case CTerminusGroupConstants.Amide : intError = SetCTerminus("NH2", strFollowingResidue, blnUse3LetterCode)
            Case CTerminusGroupConstants.None : intError = SetCTerminus("", strFollowingResidue, blnUse3LetterCode)
            Case Else : intError = 1
        End Select

        SetCTerminusGroup = intError

    End Function

    Public Sub SetDefaultModificationSymbols()

        On Error GoTo SetDefaultModificationSymbolsErrorHandler

        RemoveAllModificationSymbols()

        ' Add the symbol for phosphorylation
        SetModificationSymbol("*", mPhosphorylationMass, True, "Phosphorylation [HPO3]")

        ' Define the other default modifications
        ' Valid Mod Symbols are ! # $ % & ' * + ? ^ _ ` ~

        SetModificationSymbol("+", 14.01565, False, "Methylation [CH2]")
        SetModificationSymbol("@", 15.99492, False, "Oxidation [O]")
        SetModificationSymbol("!", 57.02146, False, "Carbamidomethylation [C2H3NO]")
        SetModificationSymbol("&", 58.00548, False, "Carboxymethylation [CH2CO2]")
        SetModificationSymbol("#", 71.03711, False, "Acrylamide [CHCH2CONH2]")
        SetModificationSymbol("$", 227.127, False, "Cleavable ICAT [(^12C10)H17N3O3]")
        SetModificationSymbol("%", 236.127, False, "Cleavable ICAT [(^13C9)(^12C)H17N3O3]")
        SetModificationSymbol("~", 442.225, False, "ICAT D0 [C20H34N4O5S]")
        SetModificationSymbol("`", 450.274, False, "ICAT D8 [C20H26D8N4O5S]")

        Exit Sub

SetDefaultModificationSymbolsErrorHandler:
        GeneralErrorHandler("MWPeptideClass.SetDefaultModificationSymbols", Err.Number)

    End Sub

    Public Sub SetDefaultOptions()
        Dim intIonIndex As Integer

        With mFragSpectrumOptions
            With .IntensityOptions
                ReDim .IonType(ION_TYPE_COUNT - 1)
                .IonType(IonTypeConstants.AIon) = 20
                .IonType(IonTypeConstants.BIon) = 100
                .IonType(IonTypeConstants.YIon) = 100
                .BYIonShoulder = 50
                .NeutralLoss = 20
            End With

            ' A ions can have ammonia and phosphate loss, but not water loss
            ReDim .IonTypeOptions(ION_TYPE_COUNT - 1)
            With .IonTypeOptions(IonTypeConstants.AIon)
                .ShowIon = True
                .NeutralLossAmmonia = True
                .NeutralLossPhosphate = True
                .NeutralLossWater = False
            End With

            For intIonIndex = IonTypeConstants.BIon To IonTypeConstants.YIon
                With .IonTypeOptions(intIonIndex)
                    .ShowIon = True
                    .NeutralLossAmmonia = True
                    .NeutralLossPhosphate = True
                    .NeutralLossWater = True
                End With
            Next intIonIndex

            .DoubleChargeIonsShow = True
            .DoubleChargeIonsThreshold = 800
        End With

    End Sub

    Public Sub SetElementMode(ByVal eNewElementMode As ElementModeConstants)
        mCurrentElementMode = eNewElementMode
        InitializeSharedData()
    End Sub

    Public Sub SetFragmentationSpectrumOptions(ByVal udtNewFragSpectrumOptions As udtFragmentationSpectrumOptionsType)
        mFragSpectrumOptions = udtNewFragSpectrumOptions
    End Sub

    Public Function SetModificationSymbol(ByVal strModSymbol As String, ByVal dblModificationMass As Double, ByVal blnIndicatesPhosphorylation As Boolean, ByVal strComment As String) As Integer
        ' Adds a new modification or updates an existing one (based on strModSymbol)
        ' Returns 0 if successful, otherwise, returns -1

        Dim strTestChar As String
        Dim intIndex As Integer, intIndexToUse As Integer, intErrorID As Integer

        intErrorID = 0
        If Len(strModSymbol) < 1 Then
            intErrorID = -1
        Else
            ' Make sure strModSymbol contains no letters, numbers, spaces, dashes, or periods
            For intIndex = 0 To strModSymbol.Length - 1
                strTestChar = strModSymbol.Substring(intIndex, 1)
                If Not IsModSymbol(strTestChar) Then
                    intErrorID = -1
                End If
            Next intIndex

            If intErrorID = 0 Then
                ' See if the modification is alrady present
                intIndexToUse = GetModificationSymbolID(strModSymbol)

                If intIndexToUse < 0 Then
                    ' Need to add the modification
                    ModificationSymbolCount += 1
                    ReserveMemoryForModifications(ModificationSymbolCount, True)
                    intIndexToUse = ModificationSymbolCount - 1
                End If

                With ModificationSymbols(intIndexToUse)
                    .Symbol = strModSymbol
                    .ModificationMass = dblModificationMass
                    .IndicatesPhosphorylation = blnIndicatesPhosphorylation
                    .Comment = strComment
                End With
            End If
        End If

        SetModificationSymbol = intErrorID

    End Function

    Private Function SetNTerminus(ByVal strFormula As String, Optional ByVal strPrecedingResidue As String = "", Optional ByVal blnUse3LetterCode As Boolean = True) As Integer
        ' Note: strFormula can only contain C, H, N, O, S, or P, and cannot contain any parentheses or other advanced formula features
        ' Returns 0 if success; 1 if error

        ' Typical N terminus mods
        ' Hydrogen = H
        ' Acetyl = C2OH3
        ' PyroGlu = C5O2NH6
        ' Carbamyl = CONH2
        ' PTC = C7H6NS

        With mNTerminus
            .Formula = strFormula
            .Mass = ComputeFormulaWeightCHNOSP(.Formula)
            If .Mass < 0 Then
                .Mass = 0
                SetNTerminus = 1
            Else
                SetNTerminus = 0
            End If
            .PrecedingResidue = FillResidueStructureUsingSymbol(strPrecedingResidue, blnUse3LetterCode)
            .FollowingResidue = FillResidueStructureUsingSymbol(String.Empty)
        End With

        UpdateResidueMasses()
    End Function

    Public Function SetNTerminusGroup(ByVal eNTerminusGroup As ntgNTerminusGroupConstants.NTerminusGroupConstants, Optional ByVal strPrecedingResidue As String = "", Optional ByVal blnUse3LetterCode As Boolean = True) As Integer
        ' Returns 0 if success; 1 if error
        Dim intError As Integer

        intError = 0
        Select Case eNTerminusGroup
            Case ntgNTerminusGroupConstants.Hydrogen : intError = SetNTerminus("H", strPrecedingResidue, blnUse3LetterCode)
            Case ntgNTerminusGroupConstants.HydrogenPlusProton : intError = SetNTerminus("HH", strPrecedingResidue, blnUse3LetterCode)
            Case ntgNTerminusGroupConstants.Acetyl : intError = SetNTerminus("C2OH3", strPrecedingResidue, blnUse3LetterCode)
            Case ntgNTerminusGroupConstants.PyroGlu : intError = SetNTerminus("C5O2NH6", strPrecedingResidue, blnUse3LetterCode)
            Case ntgNTerminusGroupConstants.Carbamyl : intError = SetNTerminus("CONH2", strPrecedingResidue, blnUse3LetterCode)
            Case ntgNTerminusGroupConstants.PTC : intError = SetNTerminus("C7H6NS", strPrecedingResidue, blnUse3LetterCode)
            Case ntgNTerminusGroupConstants.None : intError = SetNTerminus("", strPrecedingResidue, blnUse3LetterCode)
            Case Else : intError = 1
        End Select

        SetNTerminusGroup = intError

    End Function

    Public Function SetResidue(ByVal intResidueNumber As Integer, ByVal strSymbol As String, Optional ByVal blnIs3LetterCode As Boolean = True, Optional ByVal blnPhosphorylated As Boolean = False) As Integer
        ' Sets or adds a residue (must add residues in order)
        ' Returns the index of the modified residue, or the new index if added
        ' Returns -1 if a problem

        Dim intIndexToUse As Integer, str3LetterSymbol As String

        If Len(strSymbol) = 0 Then
            SetResidue = -1
            Exit Function
        End If

        If intResidueNumber > ResidueCount Then
            ResidueCount = ResidueCount + 1
            ReserveMemoryForResidues(ResidueCount, True)
            intIndexToUse = ResidueCount
        Else
            intIndexToUse = intResidueNumber
        End If

        With Residues(intIndexToUse)
            If blnIs3LetterCode Then
                str3LetterSymbol = strSymbol
            Else
                str3LetterSymbol = GetAminoAcidSymbolConversionInternal(strSymbol, True)
            End If

            If Len(str3LetterSymbol) = 0 Then
                .Symbol = UNKNOWN_SYMBOL
            Else
                .Symbol = str3LetterSymbol
            End If

            .Phosphorylated = blnPhosphorylated
            If blnPhosphorylated Then
                ' Only Ser, Thr, or Tyr should be phosphorylated
                ' However, if the user sets other residues as phosphorylated, we'll allow that
                Debug.Assert(.Symbol = "Ser" Or .Symbol = "Thr" Or .Symbol = "Tyr")
            End If

            .ModificationIDCount = 0
        End With

        UpdateResidueMasses()

        SetResidue = intIndexToUse
    End Function

    Public Function SetResidueModifications(ByVal intResidueNumber As Integer, ByVal intModificationCount As Integer, ByRef intModificationIDsOneBased() As Integer) As Integer
        ' Sets the modifications for a specific residue
        ' Modification Symbols are defined using successive calls to SetModificationSymbol()

        ' Returns 0 if modifications set; returns 1 if an error

        Dim intIndex As Integer, intNewModID As Integer

        If intResidueNumber >= 1 And intResidueNumber <= ResidueCount And intModificationCount >= 0 Then
            With Residues(intResidueNumber)
                If intModificationCount > MAX_MODIFICATIONS Then
                    intModificationCount = MAX_MODIFICATIONS
                End If

                .ModificationIDCount = 0
                .Phosphorylated = False
                For intIndex = 1 To intModificationCount
                    intNewModID = intModificationIDsOneBased(intIndex)
                    If intNewModID >= 1 And intNewModID <= ModificationSymbolCount Then
                        .ModificationIDs(.ModificationIDCount) = intNewModID

                        ' Check for phosphorylation
                        If ModificationSymbols(intNewModID).IndicatesPhosphorylation Then
                            .Phosphorylated = True
                        End If

                        .ModificationIDCount = .ModificationIDCount + 1
                    End If
                Next intIndex

            End With

            SetResidueModifications = 0
        Else
            SetResidueModifications = 1
        End If

    End Function

    Public Function SetSequence(ByVal strSequence As String, Optional ByVal eNTerminus As ntgNTerminusGroupConstants.NTerminusGroupConstants = ntgNTerminusGroupConstants.Hydrogen, Optional ByVal eCTerminus As ctgCTerminusGroupConstants = CTerminusGroupConstants.Hydroxyl, Optional ByVal blnIs3LetterCode As Boolean = True, Optional ByVal bln1LetterCheckForPrefixAndSuffixResidues As Boolean = True, Optional ByVal bln3LetterCheckForPrefixHandSuffixOH As Boolean = True, Optional ByVal blnAddMissingModificationSymbols As Boolean = False) As Integer
        ' If blnIs3LetterCode = false, then look for sequence of the form: R.ABCDEF.R
        ' If found, remove the leading and ending residues since these aren't for this peptide
        ' Returns 0 if success or 1 if an error
        ' Will return 0 even in strSequence is blank or if it contains no valid residues

        Dim intSequenceStrLength As Integer, intIndex As Integer, intModSymbolLength As Integer
        Dim str1LetterSymbol As String, str3LetterSymbol As String, strFirstChar As String

        strSequence = Trim(strSequence)

        intSequenceStrLength = Len(strSequence)
        If intSequenceStrLength = 0 Then Exit Function

        ' Clear any old residue information
        ResidueCount = 0
        ReserveMemoryForResidues(ResidueCount, False)

        If Not blnIs3LetterCode Then
            ' Sequence is 1 letter codes

            If bln1LetterCheckForPrefixAndSuffixResidues Then
                ' First look if sequence is in the form A.BCDEFG.Z or -.BCDEFG.Z or A.BCDEFG.-
                ' If so, then need to strip out the preceding A and Z residues since they aren't really part of the sequence
                If intSequenceStrLength > 1 And InStr(strSequence, ".") Then
                    If Mid(strSequence, 2, 1) = "." Then
                        strSequence = Mid(strSequence, 3)
                        intSequenceStrLength = Len(strSequence)
                    End If

                    If Mid(strSequence, intSequenceStrLength - 1, 1) = "." Then
                        strSequence = Left(strSequence, intSequenceStrLength - 2)
                        intSequenceStrLength = Len(strSequence)
                    End If

                    ' Also check for starting with a . or ending with a .
                    If Left(strSequence, 1) = "." Then
                        strSequence = Mid(strSequence, 2)
                    End If

                    If Right(strSequence, 1) = "." Then
                        strSequence = Left(strSequence, Len(strSequence) - 1)
                    End If

                    intSequenceStrLength = Len(strSequence)
                End If

            End If

            For intIndex = 1 To intSequenceStrLength
                str1LetterSymbol = Mid(strSequence, intIndex, 1)
                If IsCharacter(str1LetterSymbol) Then
                    ' Character found
                    ' Look up 3 letter symbol
                    ' If none is found, this will return an empty string
                    str3LetterSymbol = GetAminoAcidSymbolConversionInternal(str1LetterSymbol, True)

                    If Len(str3LetterSymbol) = 0 Then str3LetterSymbol = UNKNOWN_SYMBOL

                    SetSequenceAddResidue(str3LetterSymbol)

                    ' Look at following character(s), and record any modification symbols present
                    intModSymbolLength = CheckForModifications(Mid(strSequence, intIndex + 1), ResidueCount, blnAddMissingModificationSymbols)

                    intIndex = intIndex + intModSymbolLength
                Else
                    ' If . or - or space, then ignore it
                    ' If a number, ignore it
                    ' If anything else, then should have been skipped, or should be skipped
                    If str1LetterSymbol = "." Or str1LetterSymbol = "-" Or str1LetterSymbol = " " Then
                        ' All is fine; we can skip this
                    Else
                        ' Ignore it
                    End If
                End If
            Next intIndex

        Else
            ' Sequence is 3 letter codes
            intIndex = 1

            If bln3LetterCheckForPrefixHandSuffixOH Then
                ' Look for a leading H or trailing OH, provided those don't match any of the amino acids
                RemoveLeadingH(strSequence)
                RemoveTrailingOH(strSequence)

                ' Recompute sequence length
                intSequenceStrLength = Len(strSequence)
            End If

            Do While intIndex <= intSequenceStrLength - 2
                strFirstChar = Mid(strSequence, intIndex, 1)
                If IsCharacter(strFirstChar) Then
                    If IsCharacter(Mid(strSequence, intIndex + 1, 1)) And _
                       IsCharacter(Mid(strSequence, intIndex + 2, 1)) Then

                        str3LetterSymbol = UCase(strFirstChar) & LCase((Mid(strSequence, intIndex + 1, 2)))

                        If GetAbbreviationIDInternal(str3LetterSymbol, True) = 0 Then
                            ' 3 letter symbol not found
                            ' Add anyway, but mark as Xxx
                            str3LetterSymbol = UNKNOWN_SYMBOL
                        End If

                        SetSequenceAddResidue(str3LetterSymbol)

                        ' Look at following character(s), and record any modification symbols present
                        intModSymbolLength = CheckForModifications(Mid(strSequence, intIndex + 3), ResidueCount, blnAddMissingModificationSymbols)

                        intIndex = intIndex + 3
                        intIndex = intIndex + intModSymbolLength

                    Else
                        ' First letter is a character, but next two are not; ignore it
                        intIndex = intIndex + 1
                    End If
                Else
                    ' If . or - or space, then ignore it
                    ' If a number, ignore it
                    ' If anything else, then should have been skipped or should be skipped
                    If strFirstChar = "." Or strFirstChar = "-" Or strFirstChar = " " Then
                        ' All is fine; we can skip this
                    Else
                        ' Ignore it
                    End If
                    intIndex = intIndex + 1
                End If
            Loop
        End If

        ' By calling SetNTerminus and SetCTerminus, the UpdateResidueMasses() Sub will also be called
        ' We don't want to compute the mass yet
        mDelayUpdateResidueMass = True
        SetNTerminusGroup(eNTerminus)
        SetCTerminusGroup(eCTerminus)

        mDelayUpdateResidueMass = False
        UpdateResidueMasses()

        SetSequence = 0
        Exit Function

SetSequenceErrorHandler:
        SetSequence = AssureNonZero(Err.Number)
    End Function

    Private Sub SetSequenceAddResidue(ByVal str3LetterSymbol As String)

        If Len(str3LetterSymbol) = 0 Then
            str3LetterSymbol = UNKNOWN_SYMBOL
        End If

        ResidueCount = ResidueCount + 1
        ReserveMemoryForResidues(ResidueCount, True)

        With Residues(ResidueCount)
            .Symbol = str3LetterSymbol
            .Phosphorylated = False
            .ModificationIDCount = 0
        End With

    End Sub

    Private Sub ShellSortFragSpectrum(ByRef FragSpectrumWork() As udtFragmentationSpectrumDataType, ByRef PointerArray() As Integer, ByVal intLowIndex As Integer, ByVal intHighIndex As Integer)
        ' Sort the list using a shell sort
        Dim intCount As Integer
        Dim intIncrement As Integer
        Dim intIndex As Integer
        Dim intIndexCompare As Integer
        Dim intPointerSwap As Integer

        ' Sort PointerArray[intLowIndex..intHighIndex] by comparing FragSpectrumWork(PointerArray(x)).Mass

        ' Compute largest increment
        intCount = intHighIndex - intLowIndex + 1
        intIncrement = 1
        If (intCount < 14) Then
            intIncrement = 1
        Else
            Do While intIncrement < intCount
                intIncrement = 3 * intIncrement + 1
            Loop
            intIncrement = intIncrement \ 3
            intIncrement = intIncrement \ 3
        End If

        Do While intIncrement > 0
            ' Sort by insertion in increments of intIncrement
            For intIndex = intLowIndex + intIncrement To intHighIndex
                intPointerSwap = PointerArray(intIndex)
                For intIndexCompare = intIndex - intIncrement To intLowIndex Step -intIncrement
                    ' Use <= to sort ascending; Use > to sort descending
                    If FragSpectrumWork(PointerArray(intIndexCompare)).Mass <= FragSpectrumWork(intPointerSwap).Mass Then Exit For
                    PointerArray(intIndexCompare + intIncrement) = PointerArray(intIndexCompare)
                Next intIndexCompare
                PointerArray(intIndexCompare + intIncrement) = intPointerSwap
            Next intIndex
            intIncrement = intIncrement \ 3
        Loop

    End Sub

    Private Sub UpdateResidueMasses()
        Dim intIndex As Integer, intAbbrevID As Integer
        Dim intValidResidueCount As Integer
        Dim intModIndex As Integer
        Dim dblRunningTotal As Double
        Dim blnPhosphorylationMassAdded As Boolean
        Dim blnProtonatedNTerminus As Boolean

        If mDelayUpdateResidueMass Then Exit Sub

        ' The N-terminus ions are the basis for the running total
        dblRunningTotal = mNTerminus.Mass
        If UCase(mNTerminus.Formula) = "HH" Then
            ' ntgNTerminusGroupConstants.HydrogenPlusProton; since we add back in the proton below when computing the fragment masses,
            '  we need to subtract it out here
            ' However, we need to subtract out mHydrogenMass, and not mChargeCarrierMass since the current
            '  formula's mass was computed using two hydrogens, and not one hydrogen and one charge carrier
            blnProtonatedNTerminus = True
            dblRunningTotal = dblRunningTotal - mHydrogenMass
        End If

        For intIndex = 1 To ResidueCount
            With Residues(intIndex)
                intAbbrevID = GetAbbreviationIDInternal(.Symbol, True)

                If intAbbrevID > 0 Then
                    intValidResidueCount = intValidResidueCount + 1
                    .Mass = GetAbbreviationMass(intAbbrevID)

                    blnPhosphorylationMassAdded = False

                    ' Compute the mass, including the modifications
                    .MassWithMods = .Mass
                    For intModIndex = 1 To .ModificationIDCount
                        If .ModificationIDs(intModIndex) <= ModificationSymbolCount Then
                            .MassWithMods = .MassWithMods + ModificationSymbols(.ModificationIDs(intModIndex)).ModificationMass
                            If ModificationSymbols(.ModificationIDs(intModIndex)).IndicatesPhosphorylation Then
                                blnPhosphorylationMassAdded = True
                            End If
                        Else
                            ' Invalid ModificationID
                            Debug.Assert(False)
                        End If
                    Next intModIndex

                    If .Phosphorylated Then
                        ' Only add a mass if none of the .ModificationIDs has .IndicatesPhosphorylation = True
                        If Not blnPhosphorylationMassAdded Then
                            .MassWithMods = .MassWithMods + mPhosphorylationMass
                        End If
                    End If

                    dblRunningTotal = dblRunningTotal + .MassWithMods

                    .IonMass(IonTypeConstants.AIon) = dblRunningTotal - mImmoniumMassDifference - mChargeCarrierMass
                    .IonMass(IonTypeConstants.BIon) = dblRunningTotal
                Else
                    .Mass = 0
                    .MassWithMods = 0
                    Erase .IonMass()
                End If
            End With
        Next intIndex

        dblRunningTotal = dblRunningTotal + mCTerminus.Mass
        If blnProtonatedNTerminus Then
            dblRunningTotal = dblRunningTotal + mChargeCarrierMass
        End If

        If intValidResidueCount > 0 Then
            mTotalMass = dblRunningTotal
        Else
            mTotalMass = 0
        End If

        ' Now compute the y-ion masses
        dblRunningTotal = mCTerminus.Mass + mChargeCarrierMass

        For intIndex = ResidueCount To 1 Step -1
            With Residues(intIndex)
                If .IonMass(IonTypeConstants.AIon) > 0 Then
                    dblRunningTotal = dblRunningTotal + .MassWithMods
                    .IonMass(IonTypeConstants.YIon) = dblRunningTotal + mChargeCarrierMass
                    If intIndex = 1 Then
                        ' Add the N-terminus mass to highest y ion
                        .IonMass(IonTypeConstants.YIon) = .IonMass(IonTypeConstants.YIon) + mNTerminus.Mass - mChargeCarrierMass
                        If blnProtonatedNTerminus Then
                            ' ntgNTerminusGroupConstants.HydrogenPlusProton; since we add back in the proton below when computing the fragment masses,
                            '  we need to subtract it out here
                            ' However, we need to subtract out mHydrogenMass, and not mChargeCarrierMass since the current
                            '  formula's mass was computed using two hydrogens, and not one hydrogen and one charge carrier
                            .IonMass(IonTypeConstants.YIon) = .IonMass(IonTypeConstants.YIon) - mHydrogenMass
                        End If
                    End If
                End If
            End With
        Next intIndex

    End Sub

    Private Sub UpdateStandardMasses()
        Const DEFAULT_CHARGE_CARRIER_MASS_AVG As Double = 1.00739
        Const DEFAULT_CHARGE_CARRIER_MASS_MONOISO As Double = 1.00727649

        If mCurrentElementMode = ElementModeConstants.AverageMass Then
            mChargeCarrierMass = DEFAULT_CHARGE_CARRIER_MASS_AVG
        Else
            mChargeCarrierMass = DEFAULT_CHARGE_CARRIER_MASS_MONOISO
        End If

        ' Update standard mass values
        mHOHMass = ComputeFormulaWeightCHNOSP("HOH")
        mNH3Mass = ComputeFormulaWeightCHNOSP("NH3")
        mH3PO4Mass = ComputeFormulaWeightCHNOSP("H3PO4")
        mHydrogenMass = ComputeFormulaWeightCHNOSP("H")

        ' Phosphorylation is the loss of OH and the addition of H2PO4, for a net change of HPO3
        mPhosphorylationMass = ComputeFormulaWeightCHNOSP("HPO3")

        ' The immonium mass is equal to the mass of CO minus the mass of H, thus typically 26.9871
        mImmoniumMassDifference = ComputeFormulaWeightCHNOSP("CO") - mHydrogenMass

        mHistidineFW = LookupResidueMassOneLetter("H")
        mPhenylalanineFW = LookupResidueMassOneLetter("F")
        mTyrosineFW = LookupResidueMassOneLetter("Y")

    End Sub

    Private Sub InitializeSharedData()

        ' Symbol                            Formula            1 letter abbreviation
        Const AminoAbbrevCount As Integer = 28

        ReDim AminoAcidStats(AminoAbbrevCount)

        htOneLetterSymbolMap = New Hashtable
        htThreeLetterSymbolMap = New Hashtable

        AddAminoAcidStatEntry(0, "Ala", "C3H5NO", 0, "A", "Alanine")
        AddAminoAcidStatEntry(1, "Arg", "C6H12N4O", 0, "R", "Arginine, (unprotonated NH2)")
        AddAminoAcidStatEntry(2, "Asn", "C4H6N2O2", 0, "N", "Asparagine")
        AddAminoAcidStatEntry(3, "Asp", "C4H5NO3", 0, "D", "Aspartic acid (undissociated COOH)")
        AddAminoAcidStatEntry(4, "Cys", "C3H5NOS", 0, "C", "Cysteine (no disulfide link)")
        AddAminoAcidStatEntry(5, "Gla", "C6H7NO5", 0, "U", "gamma-Carboxyglutamate")
        AddAminoAcidStatEntry(6, "Gln", "C5H8N2O2", 0, "Q", "Glutamine")
        AddAminoAcidStatEntry(7, "Glu", "C5H7NO3", 0, "E", "Glutamic acid (undissociated COOH)")
        AddAminoAcidStatEntry(8, "Gly", "C2H3NO", 0, "G", "Glycine")
        AddAminoAcidStatEntry(9, "His", "C6H7N3O", 0, "H", "Histidine (unprotonated NH)")
        AddAminoAcidStatEntry(10, "Hse", "C4H7NO2", 0, "", "Homoserine")
        AddAminoAcidStatEntry(11, "Hyl", "C6H12N2O2", 0, "", "Hydroxylysine")
        AddAminoAcidStatEntry(12, "Hyp", "C5H7NO2", 0, "", "Hydroxyproline")
        AddAminoAcidStatEntry(13, "Ile", "C6H11NO", 0, "I", "Isoleucine")
        AddAminoAcidStatEntry(14, "Leu", "C6H11NO", 0, "L", "Leucine")
        AddAminoAcidStatEntry(15, "Lys", "C6H12N2O", 0, "K", "Lysine (unprotonated NH2)")
        AddAminoAcidStatEntry(16, "Met", "C5H9NOS", 0, "M", "Methionine")
        AddAminoAcidStatEntry(17, "Orn", "C5H10N2O", 0, "O", "Ornithine")
        AddAminoAcidStatEntry(18, "Phe", "C9H9NO", 0, "F", "Phenylalanine")
        AddAminoAcidStatEntry(19, "Pro", "C5H7NO", 0, "P", "Proline")
        AddAminoAcidStatEntry(20, "Pyr", "C5H5NO2", 0, "", "Pyroglutamic acid")
        AddAminoAcidStatEntry(21, "Sar", "C3H5NO", 0, "", "Sarcosine")
        AddAminoAcidStatEntry(22, "Ser", "C3H5NO2", 0, "S", "Serine")
        AddAminoAcidStatEntry(23, "Thr", "C4H7NO2", 0, "T", "Threonine")
        AddAminoAcidStatEntry(24, "Trp", "C11H10N2O", 0, "W", "Tryptophan")
        AddAminoAcidStatEntry(25, "Tyr", "C9H9NO2", 0, "Y", "Tyrosine")
        AddAminoAcidStatEntry(26, "Val", "C5H9NO", 0, "V", "Valine")
        AddAminoAcidStatEntry(27, "Xxx", "C6H12N2O", 0, "X", "Unknown")

        mWaterLossSymbol = "-H2O"
        mAmmoniaLossSymbol = "-NH3"
        mPhosphoLossSymbol = "-H3PO4"

        UpdateStandardMasses()

        SetDefaultModificationSymbols()

    End Sub

    Public Sub New()

        If Not mSharedArraysInitialized Then
            mCurrentElementMode = ElementModeConstants.IsotopicMass
            InitializeSharedData()
            mSharedArraysInitialized = True
        End If

        SetDefaultOptions()

        ResidueCount = 0
        ReserveMemoryForResidues(50, False)

        ModificationSymbolCount = 0
        ReserveMemoryForModifications(10, False)

    End Sub
End Class
