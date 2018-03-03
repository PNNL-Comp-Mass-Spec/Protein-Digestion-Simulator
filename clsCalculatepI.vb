Option Strict On

' This class will compute the pI and hydrophobicity for a peptide or protein sequence
' Code originally written by Gordon Anderson for the application ICR-2LS
' Ported to VB.NET by Matthew Monroe in August 2005
'
' Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
' Copyright 2005, Battelle Memorial Institute.  All Rights Reserved.

Public Class clspICalculation

    Public Sub New()
        mAminoAcids = New Dictionary(Of Char, AA)
        InitializeLocalVariables()
    End Sub

#Region "Constants and Enums"
    Public Enum eHydrophobicityTypeConstants As Integer
        HW = 0                  ' Hopp and Woods
        KD = 1                  ' Kyte and Doolittle
        Eisenberg = 2           ' Eisenberg
        GES = 3                 ' Engleman et. al.
        MeekPH7p4 = 4           ' Meek, pH 7.4
        MeekPH2p1 = 5           ' Meek, pH 2.1
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
    Private ReadOnly mAminoAcids As Dictionary(Of Char, AA)
#End Region

#Region "Processing Options Interface Functions"

    ''' <summary>
    ''' Hydrophobicity type
    ''' </summary>
    ''' <returns></returns>
    Public Property HydrophobicityType As eHydrophobicityTypeConstants

    ''' <summary>
    ''' When true, examine the protein residues in chunks of SequenceWidthToExamineForMaximumpI,
    ''' compute the pI for each chunk, then report the largest pI
    ''' </summary>
    ''' <returns></returns>
    Public Property ReportMaximumpI As Boolean

    ''' <summary>
    ''' Number of residues to use for computation of pI when ReportMaximumpI is true
    ''' </summary>
    ''' <returns></returns>
    Public Property SequenceWidthToExamineForMaximumpI As Integer

#End Region

    Private Function CalculateCharge(pH As Double, numC As Integer, numD As Integer, numE As Integer, numH As Integer, numK As Integer, numR As Integer, numY As Integer) As Double
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

    Private Function CalculateHydrophobicity(seq As String, HT As eHydrophobicityTypeConstants) As Double
        Dim runningSum As Double = 0
        Dim residueCount = 0

        Dim aaInfo = New AA()

        For li = 1 To seq.Length
            Dim residue = Char.ToUpper(seq.Chars(li - 1))

            Try
                If Not mAminoAcids.TryGetValue(residue, aaInfo) Then
                    Continue For
                End If

                Select Case HT
                    Case eHydrophobicityTypeConstants.HW
                        runningSum += aaInfo.HW
                    Case eHydrophobicityTypeConstants.KD
                        runningSum += aaInfo.KD
                    Case eHydrophobicityTypeConstants.Eisenberg
                        runningSum += aaInfo.Eisenberg
                    Case eHydrophobicityTypeConstants.GES
                        runningSum += aaInfo.GES
                    Case eHydrophobicityTypeConstants.MeekPH7p4
                        runningSum += aaInfo.MeekPH7p4
                    Case eHydrophobicityTypeConstants.MeekPH2p1
                        runningSum += aaInfo.MeekPH2p1
                End Select

                residueCount += 1

            Catch ex As Exception
                ' Residue is not present so ignore it
            End Try

        Next li

        If residueCount > 0 Then
            Return runningSum / residueCount
        Else
            Return 0
        End If

    End Function

    Private Function CalculateNp(pH As Double, k As Double, n As Integer) As Double
        Return n * (10 ^ (-pH) / (10 ^ (-pH) + 10 ^ (-k)))
    End Function

    Public Function CalculateSequenceChargeState(seq As String, pH As Double) As Integer
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

    Public Function CalculateSequenceHydrophobicity(seq As String) As Single

        If seq Is Nothing OrElse seq.Length = 0 Then
            Return 0
        End If

        Try
            If ReportMaximumpI AndAlso seq.Length > SequenceWidthToExamineForMaximumpI Then
                Dim maxHydrophobicity As Double = 0
                For intIndex = 1 To seq.Length - SequenceWidthToExamineForMaximumpI
                    Dim segmentHydrophobicity = CalculateHydrophobicity(seq.Substring(intIndex - 1, SequenceWidthToExamineForMaximumpI), HydrophobicityType)
                    If segmentHydrophobicity > maxHydrophobicity Then maxHydrophobicity = segmentHydrophobicity
                Next
                Return CSng(maxHydrophobicity)

            Else
                Dim hydrophobicity = CalculateHydrophobicity(seq, HydrophobicityType)
                Return CSng(hydrophobicity)
            End If
        Catch ex As Exception
            ' Error occurred
            Return 0
        End Try

    End Function

    Public Function CalculateSequencepI(seq As String) As Single
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

    Private Sub AddAminoAcid(str1LetterSymbol As Char, dblHW As Double, dblKD As Double, dblEisenberg As Double, dblGES As Double, dblMeekPH7p4 As Double, dblMeekPH2p1 As Double)

        Dim aaInfo = New AA()

        With aaInfo
            .Aname = str1LetterSymbol
            .HW = dblHW
            .KD = dblKD
            .Eisenberg = dblEisenberg
            .GES = dblGES
            .MeekPH7p4 = dblMeekPH7p4
            .MeekPH2p1 = dblMeekPH2p1
        End With

        mAminoAcids.Add(str1LetterSymbol, aaInfo)

    End Sub

    Private Sub InitializeLocalVariables()

        HydrophobicityType = eHydrophobicityTypeConstants.HW
        ReportMaximumpI = False
        SequenceWidthToExamineForMaximumpI = 10

        LoadAminoAcids()

    End Sub

    Private Sub LoadAminoAcids()

        mAminoAcids.Clear()

        AddAminoAcid("A"c, -0.5, 1.8, 0.25, -1.6, 0.5, -0.1)
        AddAminoAcid("C"c, -1, 2.5, 0.04, -2, -6.8, -2.2)
        AddAminoAcid("D"c, 3, -3.5, -0.72, 9.2, -8.2, -2.8)
        AddAminoAcid("E"c, 3, -3.5, -0.62, 8.2, -16.9, -7.5)
        AddAminoAcid("F"c, -2.5, 2.8, 0.61, -3.7, 13.2, 13.9)
        AddAminoAcid("G"c, 0, -0.4, 0.16, -1, 0, -0.5)
        AddAminoAcid("H"c, -0.5, -3.2, -0.4, 3, -3.5, 0.8)
        AddAminoAcid("I"c, -1.8, 4.5, 0.73, -3.1, 13.9, 11.8)
        AddAminoAcid("K"c, 3, -3.9, -1.1, 8.8, 0.1, -3.2)
        AddAminoAcid("L"c, -1.8, 3.8, 0.53, -2.8, 8.8, 10)
        AddAminoAcid("M"c, -1.3, 1.9, 0.26, -3.4, 4.8, 7.1)
        AddAminoAcid("N"c, 0.2, -3.5, -0.64, 4.8, 0.8, -1.6)
        AddAminoAcid("P"c, 0, -1.6, -0.07, 0.2, 6.1, 8)
        AddAminoAcid("R"c, 3, -4.5, -1.8, 12.3, 0.8, -4.5)
        AddAminoAcid("S"c, 0.3, -0.8, -0.26, -0.6, 1.2, -3.7)
        AddAminoAcid("T"c, -0.4, -0.7, -0.18, -1.2, 2.7, 1.5)
        AddAminoAcid("V"c, -1.5, 4.2, 0.54, -2.6, 2.1, 3.3)
        AddAminoAcid("W"c, -3.4, -0.9, 0.37, -1.9, 14.9, 18.1)
        AddAminoAcid("Y"c, -2.3, -1.3, 0.02, 0.7, 6.1, 8.2)

    End Sub


End Class
