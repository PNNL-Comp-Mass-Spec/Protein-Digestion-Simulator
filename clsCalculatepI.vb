Option Strict On

' This class will compute the pI and hydrophobicity for a peptide or protein seuqence
' Code originally written by Gordon Anderson for the application ICR-2LS
' Ported to VB.NET by Matthew Monroe in August 2005
'
' Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
' Copyright 2005, Battelle Memorial Institute.  All Rights Reserved.
'
' Last Modified August 26, 2005

Public Class clspICalculation

    Public Sub New()
        InitializeLocalVariables()
    End Sub

#Region "Constants and Enums"
    Public Enum eHydrophobicityTypeConstants As Integer
        HW = 0
        KD = 1
        Eisenberg = 2
        GES = 3
        MeekPH7p4 = 4
        MeekPH2p1 = 5
    End Enum

    ' Dissociation constants
    Private Const Ck As Double = 9.3        ' 8.3
    Private Const Dk As Double = 4.5        ' 3.91
    Private Const Ek As Double = 4.6        ' 4.25
    Private Const Hk As Double = 6.2        ' 6.5
    Private Const Kk As Double = 10.4       ' 10.79
    Private Const Rk As Double = 12         ' 12.5
    Private Const Yk As Double = 9.7        ' 10.95
    Private Const NH2k As Double = 7.3      ' 8.56
    Private Const COOHk As Double = 3.9     ' 3.56

    ' Alternate values for the dissociation constants
    'Private Const Ck As Double = 8.3
    'Private Const Dk As Double = 3.91
    'Private Const Ek As Double = 4.25
    'Private Const Hk As Double = 6.5
    'Private Const Kk As Double = 10.79
    'Private Const Rk As Double = 12.5
    'Private Const Yk As Double = 10.95
    'Private Const NH2k As Double = 8.56
    'Private Const COOHk As Double = 3.56

#End Region

#Region "Structures"
    Private Structure AA
        Public Aname As String              ' One letter abbreviation for the amino acid
        Public HW As Double
        Public KD As Double
        Public Eisenberg As Double
        Public GES As Double
        Public MeekPH7p4 As Double
        Public MeekPH2p1 As Double
    End Structure
#End Region

#Region "Classwide Variables"
    Private mAminoAcids() As AA
    Private mAminoAcidLookup As Hashtable            ' Allows for fast lookup of the index of a given amino acid in mAminoAcids()

    Private mHydrophobicityType As eHydrophobicityTypeConstants
    Private mReportMaximumpI As Boolean
    Private mSequenceWidthToExamineForMaximumpI As Integer

#End Region

#Region "Processing Options Interface Functions"
    Public Property HydrophobicityType() As eHydrophobicityTypeConstants
        Get
            Return mHydrophobicityType
        End Get
        Set(ByVal Value As eHydrophobicityTypeConstants)
            mHydrophobicityType = Value
        End Set
    End Property

    Public Property ReportMaximumpI() As Boolean
        Get
            Return mReportMaximumpI
        End Get
        Set(ByVal Value As Boolean)
            mReportMaximumpI = Value
        End Set
    End Property

    Public Property SequenceWidthToExamineForMaximumpI() As Integer
        Get
            Return mSequenceWidthToExamineForMaximumpI
        End Get
        Set(ByVal Value As Integer)
            If Value < 1 Then mSequenceWidthToExamineForMaximumpI = 1
            mSequenceWidthToExamineForMaximumpI = Value
        End Set
    End Property
#End Region

    Private Function CalculateCharge(ByVal pH As Double, ByVal numC As Integer, ByVal numD As Integer, ByVal numE As Integer, ByVal numH As Integer, ByVal numK As Integer, ByVal numR As Integer, ByVal numY As Integer) As Double
        Dim Value As Double

        Value = 0
        Value += CalculateNp(pH, Ck, numC)
        Value += CalculateNp(pH, Dk, numD)
        Value += CalculateNp(pH, Ek, numE)
        Value += CalculateNp(pH, Hk, numH)
        Value += CalculateNp(pH, Kk, numK)
        Value += CalculateNp(pH, Rk, numR)
        Value += CalculateNp(pH, Yk, numY)
        Value += CalculateNp(pH, NH2k, 1)
        Value += CalculateNp(pH, COOHk, 1)
        Value -= (numC + numD + numE + numY + 1)
        Return Value

    End Function

    Private Function CalculateHydrophobicity(ByVal seq As String, ByVal HT As eHydrophobicityTypeConstants) As Double
        Dim objMatch As Object
        Dim li As Integer, i As Integer
        Dim Sum As Double, Num As Integer
        Dim strResidue As String

        Sum = 0
        Num = 0

        For li = 1 To seq.Length
            strResidue = Char.ToUpper(seq.Chars(li - 1))

            Try
                objMatch = mAminoAcidLookup.Item(strResidue)
                If Not objMatch Is Nothing Then
                    i = CInt(objMatch)

                    Select Case HT
                        Case eHydrophobicityTypeConstants.HW
                            Sum += mAminoAcids(i).HW
                        Case eHydrophobicityTypeConstants.KD
                            Sum += mAminoAcids(i).KD
                        Case eHydrophobicityTypeConstants.Eisenberg
                            Sum += mAminoAcids(i).Eisenberg
                        Case eHydrophobicityTypeConstants.GES
                            Sum += mAminoAcids(i).GES
                        Case eHydrophobicityTypeConstants.MeekPH7p4
                            Sum += mAminoAcids(i).MeekPH7p4
                        Case eHydrophobicityTypeConstants.MeekPH2p1
                            Sum += mAminoAcids(i).MeekPH2p1
                    End Select
                    Num += 1

                End If
            Catch ex As Exception
                ' Residue is not present so ignore it
            End Try

        Next li
        If Num > 0 Then
            Return Sum / Num
        Else
            Return 0
        End If

    End Function

    Private Function CalculateNp(ByVal pH As Double, ByVal k As Double, ByVal n As Integer) As Double
        Return n * (10 ^ (-pH) / (10 ^ (-pH) + 10 ^ (-k)))
    End Function

    Public Function CalculateSequenceChargeState(ByVal seq As String, ByVal pH As Double) As Integer
        Dim li As Integer
        Dim intCS As Integer

        If seq Is Nothing OrElse seq.Length = 0 Then
            Return 0
        End If

        Try
            intCS = 0
            For li = 1 To seq.Length
                Select Case Char.ToUpper(seq.Chars(li - 1))
                    Case "C"c
                        If Ck > pH Then intCS += 1
                    Case "D"c
                        If Dk > pH Then intCS += 1
                    Case "E"c
                        If Ek > pH Then intCS += 1
                    Case "H"c
                        If Hk > pH Then intCS += 1
                    Case "K"c
                        If Kk > pH Then intCS += 1 + 1
                    Case "R"c
                        If Rk > pH Then intCS += 1
                    Case "Y"c
                        If Yk > pH Then intCS += 1
                End Select
            Next li

            If intCS = 0 Then intCS = 1
        Catch ex As Exception
            ' Error occurred
            intCS = 1
        End Try

        Return intCS

    End Function

    Public Function CalculateSequenceHydrophobicity(ByVal seq As String) As Single
        Dim intIndex As Integer
        Dim Hydro As Double, MaxHydro As Double

        If seq Is Nothing OrElse seq.Length = 0 Then
            Return 0
        End If

        Try
            If mReportMaximumpI AndAlso seq.Length > mSequenceWidthToExamineForMaximumpI Then
                MaxHydro = 0
                For intIndex = 1 To seq.Length - mSequenceWidthToExamineForMaximumpI
                    Hydro = CalculateHydrophobicity(seq.Substring(intIndex - 1, mSequenceWidthToExamineForMaximumpI), mHydrophobicityType)
                    If Hydro > MaxHydro Then MaxHydro = Hydro
                Next
                Hydro = MaxHydro

            Else
                Hydro = CalculateHydrophobicity(seq, mHydrophobicityType)
            End If
        Catch ex As Exception
            ' Error occurred
            Hydro = 0
        End Try

        Return CSng(Hydro)

    End Function

    Public Function CalculateSequencepI(ByVal seq As String) As Single
        Dim i As Integer
        Dim numC As Integer, numD As Integer, numE As Integer
        Dim numH As Integer, numK As Integer, numR As Integer
        Dim numY As Integer
        Dim Value As Double, value1 As Double, pH As Double
        Dim delta As Double

        If seq Is Nothing OrElse seq.Length = 0 Then
            Return 0
        End If

        Try
            numC = 0
            numD = 0
            numE = 0
            numH = 0
            numK = 0
            numR = 0
            numY = 0
            For i = 1 To seq.Length
                Select Case (Char.ToUpper(seq.Chars(i - 1)))
                    Case "C"c
                        numC += 1
                    Case "D"c
                        numD += 1
                    Case "E"c
                        numE += 1
                    Case "H"c
                        numH += 1
                    Case "K"c
                        numK += 1
                    Case "R"c
                        numR += 1
                    Case "Y"c
                        numY += 1
                End Select
            Next
            pH = 1
            delta = 1
            Value = CalculateCharge(pH, numC, numD, numE, numH, numK, numR, numY) + 1
            Do
                value1 = CalculateCharge(pH, numC, numD, numE, numH, numK, numR, numY)
                If Math.Abs(value1) <= Math.Abs(Value) Then
                    Value = value1
                    pH += delta
                Else
                    delta = delta / (-10)
                    Value = value1
                    pH += delta
                    If Math.Abs(delta) < 0.01 Then Exit Do
                End If
            Loop
        Catch ex As Exception
            ' Error occurred
            pH = 0
        End Try

        Return CSng(pH)

    End Function

    Private Sub DefineAminoAcid(ByRef udtAminoAcids() As AA, ByRef htAminoAcidLookup As Hashtable, ByVal intIndex As Integer, ByVal str1LetterSymbol As String, ByVal dblHW As Double, ByVal dblKD As Double, ByVal dblEisenberg As Double, ByVal dblGES As Double, ByVal dblMeekPH7p4 As Double, ByVal dblMeekPH2p1 As Double)

        With udtAminoAcids(intIndex)
            .Aname = str1LetterSymbol
            .HW = dblHW
            .KD = dblKD
            .Eisenberg = dblEisenberg
            .GES = dblGES
            .MeekPH7p4 = dblMeekPH7p4
            .MeekPH2p1 = dblMeekPH2p1
        End With

        htAminoAcidLookup.Add(str1LetterSymbol, intIndex)

    End Sub

    Private Sub InitializeLocalVariables()

        mHydrophobicityType = eHydrophobicityTypeConstants.HW
        mReportMaximumpI = False
        mSequenceWidthToExamineForMaximumpI = 10

        LoadAminoAcids()

    End Sub

    Private Sub LoadAminoAcids()

        Const AminoAcidCount As Integer = 19

        If mAminoAcidLookup Is Nothing Then
            mAminoAcidLookup = New Hashtable
        Else
            mAminoAcidLookup.Clear()
        End If

        ReDim mAminoAcids(AminoAcidCount - 1)

        DefineAminoAcid(mAminoAcids, mAminoAcidLookup, 0, "A", -0.5, 1.8, 0.25, -1.6, 0.5, -0.1)
        DefineAminoAcid(mAminoAcids, mAminoAcidLookup, 1, "C", -1, 2.5, 0.04, -2, -6.8, -2.2)
        DefineAminoAcid(mAminoAcids, mAminoAcidLookup, 2, "D", 3, -3.5, -0.72, 9.2, -8.2, -2.8)
        DefineAminoAcid(mAminoAcids, mAminoAcidLookup, 3, "E", 3, -3.5, -0.62, 8.2, -16.9, -7.5)
        DefineAminoAcid(mAminoAcids, mAminoAcidLookup, 4, "F", -2.5, 2.8, 0.61, -3.7, 13.2, 13.9)
        DefineAminoAcid(mAminoAcids, mAminoAcidLookup, 5, "G", 0, -0.4, 0.16, -1, 0, -0.5)
        DefineAminoAcid(mAminoAcids, mAminoAcidLookup, 6, "H", -0.5, -3.2, -0.4, 3, -3.5, 0.8)
        DefineAminoAcid(mAminoAcids, mAminoAcidLookup, 7, "I", -1.8, 4.5, 0.73, -3.1, 13.9, 11.8)
        DefineAminoAcid(mAminoAcids, mAminoAcidLookup, 8, "K", 3, -3.9, -1.1, 8.8, 0.1, -3.2)
        DefineAminoAcid(mAminoAcids, mAminoAcidLookup, 9, "L", -1.8, 3.8, 0.53, -2.8, 8.8, 10)
        DefineAminoAcid(mAminoAcids, mAminoAcidLookup, 10, "M", -1.3, 1.9, 0.26, -3.4, 4.8, 7.1)
        DefineAminoAcid(mAminoAcids, mAminoAcidLookup, 11, "N", 0.2, -3.5, -0.64, 4.8, 0.8, -1.6)
        DefineAminoAcid(mAminoAcids, mAminoAcidLookup, 12, "P", 0, -1.6, -0.07, 0.2, 6.1, 8)
        DefineAminoAcid(mAminoAcids, mAminoAcidLookup, 13, "R", 3, -4.5, -1.8, 12.3, 0.8, -4.5)
        DefineAminoAcid(mAminoAcids, mAminoAcidLookup, 14, "S", 0.3, -0.8, -0.26, -0.6, 1.2, -3.7)
        DefineAminoAcid(mAminoAcids, mAminoAcidLookup, 15, "T", -0.4, -0.7, -0.18, -1.2, 2.7, 1.5)
        DefineAminoAcid(mAminoAcids, mAminoAcidLookup, 16, "V", -1.5, 4.2, 0.54, -2.6, 2.1, 3.3)
        DefineAminoAcid(mAminoAcids, mAminoAcidLookup, 17, "W", -3.4, -0.9, 0.37, -1.9, 14.9, 18.1)
        DefineAminoAcid(mAminoAcids, mAminoAcidLookup, 18, "Y", -2.3, -1.3, 0.02, 0.7, 6.1, 8.2)

    End Sub


End Class
