Option Strict On

' -------------------------------------------------------------------------------
' Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2003
'
' E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
' Website: https://omics.pnl.gov/ or https://www.pnnl.gov/sysbio/ or https://panomics.pnnl.gov/
' -------------------------------------------------------------------------------
'
' Licensed under the 2-Clause BSD License; you may not use this file except
' in compliance with the License.  You may obtain a copy of the License at
' https://opensource.org/licenses/BSD-2-Clause
'
' Copyright 2018 Battelle Memorial Institute

''' <summary>
''' This class can be used to search a list of values for a given value, plus or minus a given tolerance
''' The input list need not be sorted, since mPointerIndices() will be populated when the data is loaded,
''' after which the data array will be sorted
'''
''' To prevent this behavior, and save memory by not populating mPointerIndices, set mUsePointerIndexArray = False
''' </summary>
Public Class clsSearchRange

    Public Sub New()
        InitializeLocalVariables()
    End Sub

    Private Enum eDataTypeToUse
        NoDataPresent = 0
        IntegerType = 1
        SingleType = 2
        DoubleType = 3
        FillingIntegerType = 4
        FillingSingleType = 5
        FillingDoubleType = 6
    End Enum

    Private mDataType As eDataTypeToUse

    Private mDataInt() As Integer
    Private mDataSingle() As Single
    Private mDataDouble() As Double

    Private mPointByPointFillCount As Integer

    Private mPointerIndices() As Integer        ' Pointers to the original index of the data point in the source array

    Private mPointerArrayIsValid As Boolean
    Private mUsePointerIndexArray As Boolean    ' Set this to false to conserve memory usage

    Public ReadOnly Property DataCount As Integer
        Get
            Select Case mDataType
                Case eDataTypeToUse.IntegerType, eDataTypeToUse.FillingIntegerType
                    Return mDataInt.Length
                Case eDataTypeToUse.SingleType, eDataTypeToUse.FillingSingleType
                    Return mDataSingle.Length
                Case eDataTypeToUse.DoubleType, eDataTypeToUse.FillingDoubleType
                    Return mDataDouble.Length
                Case eDataTypeToUse.NoDataPresent
                    Return 0
                Case Else
                    Console.WriteLine("Unknown data type encountered: " & mDataType.ToString())
                    Return 0
            End Select
        End Get
    End Property

    Public ReadOnly Property OriginalIndex(index As Integer) As Integer
        Get
            If mPointerArrayIsValid Then
                Try
                    If index < mPointerIndices.Length Then
                        Return mPointerIndices(index)
                    Else
                        Return -1
                    End If
                Catch ex As Exception
                    Return -1
                End Try
            Else
                Return -1
            End If
        End Get
    End Property

    ' ReSharper disable once UnusedMember.Global
    Public Property UsePointerIndexArray As Boolean
        Get
            Return mUsePointerIndexArray
        End Get
        Set
            mUsePointerIndexArray = Value
        End Set
    End Property

    Private Sub BinarySearchRangeInt(searchValue As Integer, toleranceHalfWidth As Integer, ByRef matchIndexStart As Integer, ByRef matchIndexEnd As Integer)
        ' Recursive search function

        Dim indexMidpoint As Integer
        Dim leftDone As Boolean
        Dim rightDone As Boolean
        Dim leftIndex As Integer
        Dim rightIndex As Integer

        indexMidpoint = (matchIndexStart + matchIndexEnd) \ 2
        If indexMidpoint = matchIndexStart Then
            ' Min and Max are next to each other
            If Math.Abs(searchValue - mDataInt(matchIndexStart)) > toleranceHalfWidth Then matchIndexStart = matchIndexEnd
            If Math.Abs(searchValue - mDataInt(matchIndexEnd)) > toleranceHalfWidth Then matchIndexEnd = indexMidpoint
            Exit Sub
        End If

        If mDataInt(indexMidpoint) > searchValue + toleranceHalfWidth Then
            ' Out of range on the right
            matchIndexEnd = indexMidpoint
            BinarySearchRangeInt(searchValue, toleranceHalfWidth, matchIndexStart, matchIndexEnd)
        ElseIf mDataInt(indexMidpoint) < searchValue - toleranceHalfWidth Then
            ' Out of range on the left
            matchIndexStart = indexMidpoint
            BinarySearchRangeInt(searchValue, toleranceHalfWidth, matchIndexStart, matchIndexEnd)
        Else
            ' Inside range; figure out the borders
            leftIndex = indexMidpoint
            Do
                leftIndex -= 1
                If leftIndex < matchIndexStart Then
                    leftDone = True
                Else
                    If Math.Abs(searchValue - mDataInt(leftIndex)) > toleranceHalfWidth Then leftDone = True
                End If
            Loop While Not leftDone
            rightIndex = indexMidpoint

            Do
                rightIndex += 1
                If rightIndex > matchIndexEnd Then
                    rightDone = True
                Else
                    If Math.Abs(searchValue - mDataInt(rightIndex)) > toleranceHalfWidth Then rightDone = True
                End If
            Loop While Not rightDone

            matchIndexStart = leftIndex + 1
            matchIndexEnd = rightIndex - 1
        End If

    End Sub

    Private Sub BinarySearchRangeSng(searchValue As Single, toleranceHalfWidth As Single, ByRef matchIndexStart As Integer, ByRef matchIndexEnd As Integer)
        ' Recursive search function

        Dim indexMidpoint As Integer
        Dim leftDone As Boolean
        Dim rightDone As Boolean
        Dim leftIndex As Integer
        Dim rightIndex As Integer

        indexMidpoint = (matchIndexStart + matchIndexEnd) \ 2
        If indexMidpoint = matchIndexStart Then
            ' Min and Max are next to each other
            If Math.Abs(searchValue - mDataSingle(matchIndexStart)) > toleranceHalfWidth Then matchIndexStart = matchIndexEnd
            If Math.Abs(searchValue - mDataSingle(matchIndexEnd)) > toleranceHalfWidth Then matchIndexEnd = indexMidpoint
            Exit Sub
        End If

        If mDataSingle(indexMidpoint) > searchValue + toleranceHalfWidth Then
            ' Out of range on the right
            matchIndexEnd = indexMidpoint
            BinarySearchRangeSng(searchValue, toleranceHalfWidth, matchIndexStart, matchIndexEnd)
        ElseIf mDataSingle(indexMidpoint) < searchValue - toleranceHalfWidth Then
            ' Out of range on the left
            matchIndexStart = indexMidpoint
            BinarySearchRangeSng(searchValue, toleranceHalfWidth, matchIndexStart, matchIndexEnd)
        Else
            ' Inside range; figure out the borders
            leftIndex = indexMidpoint
            Do
                leftIndex -= 1
                If leftIndex < matchIndexStart Then
                    leftDone = True
                Else
                    If Math.Abs(searchValue - mDataSingle(leftIndex)) > toleranceHalfWidth Then leftDone = True
                End If
            Loop While Not leftDone
            rightIndex = indexMidpoint

            Do
                rightIndex += 1
                If rightIndex > matchIndexEnd Then
                    rightDone = True
                Else
                    If Math.Abs(searchValue - mDataSingle(rightIndex)) > toleranceHalfWidth Then rightDone = True
                End If
            Loop While Not rightDone

            matchIndexStart = leftIndex + 1
            matchIndexEnd = rightIndex - 1
        End If

    End Sub

    Private Sub BinarySearchRangeDbl(searchValue As Double, toleranceHalfWidth As Double, ByRef matchIndexStart As Integer, ByRef matchIndexEnd As Integer)
        ' Recursive search function

        Dim indexMidpoint As Integer
        Dim leftDone As Boolean
        Dim rightDone As Boolean
        Dim leftIndex As Integer
        Dim rightIndex As Integer

        indexMidpoint = (matchIndexStart + matchIndexEnd) \ 2
        If indexMidpoint = matchIndexStart Then
            ' Min and Max are next to each other
            If Math.Abs(searchValue - mDataDouble(matchIndexStart)) > toleranceHalfWidth Then matchIndexStart = matchIndexEnd
            If Math.Abs(searchValue - mDataDouble(matchIndexEnd)) > toleranceHalfWidth Then matchIndexEnd = indexMidpoint
            Exit Sub
        End If

        If mDataDouble(indexMidpoint) > searchValue + toleranceHalfWidth Then
            ' Out of range on the right
            matchIndexEnd = indexMidpoint
            BinarySearchRangeDbl(searchValue, toleranceHalfWidth, matchIndexStart, matchIndexEnd)
        ElseIf mDataDouble(indexMidpoint) < searchValue - toleranceHalfWidth Then
            ' Out of range on the left
            matchIndexStart = indexMidpoint
            BinarySearchRangeDbl(searchValue, toleranceHalfWidth, matchIndexStart, matchIndexEnd)
        Else
            ' Inside range; figure out the borders
            leftIndex = indexMidpoint
            Do
                leftIndex -= 1
                If leftIndex < matchIndexStart Then
                    leftDone = True
                Else
                    If Math.Abs(searchValue - mDataDouble(leftIndex)) > toleranceHalfWidth Then leftDone = True
                End If
            Loop While Not leftDone
            rightIndex = indexMidpoint

            Do
                rightIndex += 1
                If rightIndex > matchIndexEnd Then
                    rightDone = True
                Else
                    If Math.Abs(searchValue - mDataDouble(rightIndex)) > toleranceHalfWidth Then rightDone = True
                End If
            Loop While Not rightDone

            matchIndexStart = leftIndex + 1
            matchIndexEnd = rightIndex - 1
        End If

    End Sub

    Private Sub ClearUnusedData()
        If mDataType <> eDataTypeToUse.IntegerType Then ReDim mDataInt(-1)
        If mDataType <> eDataTypeToUse.SingleType Then ReDim mDataSingle(-1)
        If mDataType <> eDataTypeToUse.DoubleType Then ReDim mDataDouble(-1)

        If mDataType = eDataTypeToUse.NoDataPresent Then
            ReDim mPointerIndices(-1)
            mPointerArrayIsValid = False
        End If
    End Sub

    Public Sub ClearData()
        mDataType = eDataTypeToUse.NoDataPresent
        ClearUnusedData()
    End Sub

    Public Sub InitializeDataFillInteger(expectedDataCount As Integer)
        ' Call this sub to initialize the data arrays, which will allow you to
        '  then call FillWithDataAddPoint() repeatedly for each data point
        '  or call FillWithDataAddBlock() repeatedly with each block of data points
        ' When done, call FinalizeDataFill

        mDataType = eDataTypeToUse.NoDataPresent
        ClearUnusedData()

        mDataType = eDataTypeToUse.FillingIntegerType
        ReDim mDataInt(expectedDataCount - 1)

        mPointByPointFillCount = 0

    End Sub

    Public Sub InitializeDataFillSingle(dataCountToReserve As Integer)
        ' Call this sub to initialize the data arrays, which will allow you to
        '  then call FillWithDataAddPoint() repeatedly for each data point
        '  or call FillWithDataAddBlock() repeatedly with each block of data points
        ' When done, call FinalizeDataFill

        mDataType = eDataTypeToUse.NoDataPresent
        ClearUnusedData()

        mDataType = eDataTypeToUse.FillingSingleType
        ReDim mDataSingle(dataCountToReserve - 1)

    End Sub

    Public Sub InitializeDataFillDouble(dataCountToReserve As Integer)
        ' Call this sub to initialize the data arrays, which will allow you to
        '  then call FillWithDataAddPoint() repeatedly for each data point
        '  or call FillWithDataAddBlock() repeatedly with each block of data points
        ' When done, call FinalizeDataFill

        mDataType = eDataTypeToUse.NoDataPresent
        ClearUnusedData()

        mDataType = eDataTypeToUse.FillingDoubleType
        ReDim mDataDouble(dataCountToReserve - 1)

    End Sub

    Public Function FillWithData(ByRef values() As Integer) As Boolean

        Dim success As Boolean

        Try
            If values Is Nothing OrElse values.Length = 0 Then
                success = False
            Else
                ReDim mDataInt(values.Length - 1)
                values.CopyTo(mDataInt, 0)

                If mUsePointerIndexArray Then
                    InitializePointerIndexArray(mDataInt.Length)
                    Array.Sort(mDataInt, mPointerIndices)
                Else
                    Array.Sort(mDataInt)
                    ReDim mPointerIndices(-1)
                    mPointerArrayIsValid = False
                End If

                mDataType = eDataTypeToUse.IntegerType
                success = True
            End If
        Catch ex As Exception
            success = False
        End Try

        If success Then
            ClearUnusedData()
        Else
            mDataType = eDataTypeToUse.NoDataPresent
        End If
        Return success

    End Function

    Public Function FillWithData(ByRef values() As Single) As Boolean

        Dim success As Boolean

        Try
            If values Is Nothing OrElse values.Length = 0 Then
                success = False
            Else
                ReDim mDataSingle(values.Length - 1)
                values.CopyTo(mDataSingle, 0)

                If mUsePointerIndexArray Then
                    InitializePointerIndexArray(mDataSingle.Length)
                    Array.Sort(mDataSingle, mPointerIndices)
                Else
                    Array.Sort(mDataSingle)
                    ReDim mPointerIndices(-1)
                    mPointerArrayIsValid = False
                End If

                mDataType = eDataTypeToUse.SingleType
                success = True
            End If
        Catch ex As Exception
            success = False
        End Try

        If success Then
            ClearUnusedData()
        Else
            mDataType = eDataTypeToUse.NoDataPresent
        End If
        Return success

    End Function

    Public Function FillWithData(ByRef values() As Double) As Boolean

        Dim success As Boolean

        Try
            If values Is Nothing OrElse values.Length = 0 Then
                success = False
            Else
                ReDim mDataDouble(values.Length - 1)
                values.CopyTo(mDataDouble, 0)

                If mUsePointerIndexArray Then
                    InitializePointerIndexArray(mDataDouble.Length)
                    Array.Sort(mDataDouble, mPointerIndices)
                Else
                    Array.Sort(mDataDouble)
                    ReDim mPointerIndices(-1)
                    mPointerArrayIsValid = False
                End If

                mDataType = eDataTypeToUse.DoubleType
                success = True
            End If
        Catch ex As Exception
            success = False
        End Try

        If success Then
            ClearUnusedData()
        Else
            mDataType = eDataTypeToUse.NoDataPresent
        End If
        Return success

    End Function

    Public Function FillWithDataAddBlock(valuesToAdd() As Integer) As Boolean

        Dim success As Boolean

        Try

            If mDataInt.Length <= mPointByPointFillCount + valuesToAdd.Length - 1 Then
                ReDim Preserve mDataInt(CInt(mDataInt.Length + valuesToAdd.Length) - 1)
            End If

            Array.Copy(valuesToAdd, 0, mDataInt, mPointByPointFillCount - 1, valuesToAdd.Length)
            mPointByPointFillCount += valuesToAdd.Length

            'For index = 0 To valuesToAdd.Length - 1
            '    mDataInt(mPointByPointFillCount) = valuesToAdd(index)
            '    mPointByPointFillCount += 1
            'Next index

        Catch ex As Exception
            success = False
        End Try

        Return success

    End Function

    Public Function FillWithDataAddBlock(valuesToAdd() As Single) As Boolean

        Dim success As Boolean

        Try

            If mDataSingle.Length <= mPointByPointFillCount + valuesToAdd.Length - 1 Then
                ReDim Preserve mDataSingle(CInt(mDataSingle.Length + valuesToAdd.Length) - 1)
            End If

            Array.Copy(valuesToAdd, 0, mDataSingle, mPointByPointFillCount - 1, valuesToAdd.Length)
            mPointByPointFillCount += valuesToAdd.Length

        Catch ex As Exception
            success = False
        End Try

        Return success

    End Function

    Public Function FillWithDataAddBlock(valuesToAdd() As Double) As Boolean

        Dim success As Boolean

        Try

            If mDataDouble.Length <= mPointByPointFillCount + valuesToAdd.Length - 1 Then
                ReDim Preserve mDataDouble(CInt(mDataDouble.Length + valuesToAdd.Length) - 1)
            End If

            Array.Copy(valuesToAdd, 0, mDataDouble, mPointByPointFillCount, valuesToAdd.Length)
            mPointByPointFillCount += valuesToAdd.Length

        Catch ex As Exception
            success = False
        End Try

        Return success

    End Function

    Public Function FillWithDataAddPoint(valueToAdd As Integer) As Boolean

        Dim success As Boolean

        Try
            If mDataType <> eDataTypeToUse.FillingIntegerType Then
                Select Case mDataType
                    Case eDataTypeToUse.FillingSingleType
                        success = FillWithDataAddPoint(CSng(valueToAdd))
                    Case eDataTypeToUse.FillingDoubleType
                        success = FillWithDataAddPoint(CDbl(valueToAdd))
                    Case Else
                        success = False
                End Select
            Else
                If mDataInt.Length <= mPointByPointFillCount Then
                    ReDim Preserve mDataInt(CInt(mDataInt.Length * 1.1) - 1)
                End If

                mDataInt(mPointByPointFillCount) = valueToAdd
                mPointByPointFillCount += 1
            End If

        Catch ex As Exception
            success = False
        End Try

        Return success

    End Function

    Public Function FillWithDataAddPoint(valueToAdd As Single) As Boolean

        Dim success As Boolean

        Try
            If mDataType <> eDataTypeToUse.FillingSingleType Then
                Select Case mDataType
                    Case eDataTypeToUse.FillingIntegerType
                        success = FillWithDataAddPoint(CInt(valueToAdd))
                    Case eDataTypeToUse.FillingDoubleType
                        success = FillWithDataAddPoint(CDbl(valueToAdd))
                    Case Else
                        success = False
                End Select
            Else
                If mDataSingle.Length <= mPointByPointFillCount Then
                    ReDim Preserve mDataSingle(CInt(mDataSingle.Length * 1.1) - 1)
                End If

                mDataSingle(mPointByPointFillCount) = valueToAdd
                mPointByPointFillCount += 1
            End If

        Catch ex As Exception
            success = False
        End Try

        Return success

    End Function

    Public Function FillWithDataAddPoint(valueToAdd As Double) As Boolean

        Dim success As Boolean

        Try
            If mDataType <> eDataTypeToUse.FillingDoubleType Then
                Select Case mDataType
                    Case eDataTypeToUse.FillingIntegerType
                        success = FillWithDataAddPoint(CInt(valueToAdd))
                    Case eDataTypeToUse.FillingSingleType
                        success = FillWithDataAddPoint(CSng(valueToAdd))
                    Case Else
                        success = False
                End Select
            Else
                If mDataDouble.Length <= mPointByPointFillCount Then
                    ReDim Preserve mDataDouble(CInt(mDataDouble.Length * 1.1) - 1)
                End If

                mDataDouble(mPointByPointFillCount) = valueToAdd
                mPointByPointFillCount += 1
            End If

        Catch ex As Exception
            success = False
        End Try

        Return success

    End Function

    Public Function FinalizeDataFill() As Boolean
        ' Finalizes point-by-point data filling
        ' Call this after calling FillWithDataAddPoint with each point

        Dim DataArray As Array = Nothing
        Dim success As Boolean

        Try
            success = True
            Select Case mDataType
                Case eDataTypeToUse.FillingIntegerType
                    mDataType = eDataTypeToUse.IntegerType

                    ' Shrink mDataInt if necessary
                    If mDataInt.Length > mPointByPointFillCount Then
                        ReDim Preserve mDataInt(mPointByPointFillCount - 1)
                    End If

                    DataArray = mDataInt
                Case eDataTypeToUse.FillingSingleType
                    mDataType = eDataTypeToUse.SingleType

                    ' Shrink mDataSingle if necessary
                    If mDataSingle.Length > mPointByPointFillCount Then
                        ReDim Preserve mDataSingle(mPointByPointFillCount - 1)
                    End If

                    DataArray = mDataSingle
                Case eDataTypeToUse.FillingDoubleType
                    mDataType = eDataTypeToUse.DoubleType

                    ' Shrink mDataDouble if necessary
                    If mDataDouble.Length > mPointByPointFillCount Then
                        ReDim Preserve mDataDouble(mPointByPointFillCount - 1)
                    End If

                    DataArray = mDataDouble
                Case Else
                    ' Not filling
                    success = False
            End Select

            If success And DataArray IsNot Nothing Then
                If mUsePointerIndexArray Then
                    InitializePointerIndexArray(DataArray.Length)
                    Array.Sort(DataArray, mPointerIndices)
                Else
                    Array.Sort(DataArray)
                    ReDim mPointerIndices(-1)
                    mPointerArrayIsValid = False
                End If
            End If
        Catch ex As Exception
            success = False
        End Try

        Return success

    End Function

    Public Function FindValueRange(searchValue As Integer, toleranceHalfWidth As Integer, Optional ByRef matchIndexStart As Integer = 0, Optional ByRef matchIndexEnd As Integer = 0) As Boolean
        ' Searches the loaded data for searchValue with a tolerance of +-tolerance
        ' Returns True if a match is found; in addition, populates matchIndexStart and matchIndexEnd
        ' Otherwise, returns false

        Dim matchFound As Boolean

        ' See if user filled with data, but didn't call Finalize
        ' We'll call it for them
        If mDataType = eDataTypeToUse.FillingIntegerType Or mDataType = eDataTypeToUse.FillingSingleType Or mDataType = eDataTypeToUse.FillingDoubleType Then
            Me.FinalizeDataFill()
        End If

        If mDataType <> eDataTypeToUse.IntegerType Then
            Select Case mDataType
                Case eDataTypeToUse.SingleType
                    matchFound = FindValueRange(CSng(searchValue), CSng(toleranceHalfWidth), matchIndexStart, matchIndexEnd)
                Case eDataTypeToUse.DoubleType
                    matchFound = FindValueRange(CDbl(searchValue), CDbl(toleranceHalfWidth), matchIndexStart, matchIndexEnd)
                Case Else
                    matchFound = False
            End Select
        Else
            matchIndexStart = 0
            matchIndexEnd = mDataInt.Length - 1

            If mDataInt.Length = 0 Then
                matchIndexEnd = -1
            ElseIf mDataInt.Length = 1 Then
                If Math.Abs(searchValue - mDataInt(0)) > toleranceHalfWidth Then
                    ' Only one data point, and it is not within tolerance
                    matchIndexEnd = -1
                End If
            Else
                BinarySearchRangeInt(searchValue, toleranceHalfWidth, matchIndexStart, matchIndexEnd)
            End If

            If matchIndexStart > matchIndexEnd Then
                matchIndexStart = -1
                matchIndexEnd = -1
                matchFound = False
            Else
                matchFound = True
            End If
        End If

        Return matchFound
    End Function

    Public Function FindValueRange(searchValue As Double, toleranceHalfWidth As Double, Optional ByRef matchIndexStart As Integer = 0, Optional ByRef matchIndexEnd As Integer = 0) As Boolean
        ' Searches the loaded data for searchValue with a tolerance of +-tolerance
        ' Returns True if a match is found; in addition, populates matchIndexStart and matchIndexEnd
        ' Otherwise, returns false

        Dim matchFound As Boolean

        ' See if user filled with data, but didn't call Finalize
        ' We'll call it for them
        If mDataType = eDataTypeToUse.FillingIntegerType Or mDataType = eDataTypeToUse.FillingSingleType Or mDataType = eDataTypeToUse.FillingDoubleType Then
            Me.FinalizeDataFill()
        End If

        If mDataType <> eDataTypeToUse.DoubleType Then
            Select Case mDataType
                Case eDataTypeToUse.IntegerType
                    matchFound = FindValueRange(CInt(searchValue), CInt(toleranceHalfWidth), matchIndexStart, matchIndexEnd)
                Case eDataTypeToUse.SingleType
                    matchFound = FindValueRange(CSng(searchValue), CSng(toleranceHalfWidth), matchIndexStart, matchIndexEnd)
                Case Else
                    matchFound = False
            End Select
        Else
            matchIndexStart = 0
            matchIndexEnd = mDataDouble.Length - 1

            If mDataDouble.Length = 0 Then
                matchIndexEnd = -1
            ElseIf mDataDouble.Length = 1 Then
                If Math.Abs(searchValue - mDataDouble(0)) > toleranceHalfWidth Then
                    ' Only one data point, and it is not within tolerance
                    matchIndexEnd = -1
                End If
            Else
                BinarySearchRangeDbl(searchValue, toleranceHalfWidth, matchIndexStart, matchIndexEnd)
            End If

            If matchIndexStart > matchIndexEnd Then
                matchIndexStart = -1
                matchIndexEnd = -1
                matchFound = False
            Else
                matchFound = True
            End If
        End If

        Return matchFound
    End Function

    Public Function FindValueRange(searchValue As Single, toleranceHalfWidth As Single, Optional ByRef matchIndexStart As Integer = 0, Optional ByRef matchIndexEnd As Integer = 0) As Boolean
        ' Searches the loaded data for searchValue with a tolerance of +-tolerance
        ' Returns True if a match is found; in addition, populates matchIndexStart and matchIndexEnd
        ' Otherwise, returns false

        Dim matchFound As Boolean

        ' See if user filled with data, but didn't call Finalize
        ' We'll call it for them
        If mDataType = eDataTypeToUse.FillingIntegerType Or mDataType = eDataTypeToUse.FillingSingleType Or mDataType = eDataTypeToUse.FillingDoubleType Then
            Me.FinalizeDataFill()
        End If

        If mDataType <> eDataTypeToUse.SingleType Then
            Select Case mDataType
                Case eDataTypeToUse.IntegerType
                    matchFound = FindValueRange(CInt(searchValue), CInt(toleranceHalfWidth), matchIndexStart, matchIndexEnd)
                Case eDataTypeToUse.DoubleType
                    matchFound = FindValueRange(CDbl(searchValue), CDbl(toleranceHalfWidth), matchIndexStart, matchIndexEnd)
                Case Else
                    matchFound = False
            End Select
        Else
            matchIndexStart = 0
            matchIndexEnd = mDataSingle.Length - 1

            If mDataSingle.Length = 0 Then
                matchIndexEnd = -1
            ElseIf mDataSingle.Length = 1 Then
                If Math.Abs(searchValue - mDataSingle(0)) > toleranceHalfWidth Then
                    ' Only one data point, and it is not within tolerance
                    matchIndexEnd = -1
                End If
            Else
                BinarySearchRangeSng(searchValue, toleranceHalfWidth, matchIndexStart, matchIndexEnd)
            End If

            If matchIndexStart > matchIndexEnd Then
                matchIndexStart = -1
                matchIndexEnd = -1
                matchFound = False
            Else
                matchFound = True
            End If
        End If

        Return matchFound
    End Function

    Public Function GetValueByIndexInt(index As Integer) As Integer
        Try
            Return CInt(GetValueByIndex(index))
        Catch ex As Exception
            Return 0
        End Try
    End Function

    Public Function GetValueByIndex(index As Integer) As Double
        Try
            If mDataType = eDataTypeToUse.NoDataPresent Then
                Return 0
            Else
                Select Case mDataType
                    Case eDataTypeToUse.IntegerType, eDataTypeToUse.FillingIntegerType
                        Return mDataInt(index)
                    Case eDataTypeToUse.SingleType, eDataTypeToUse.FillingSingleType
                        Return mDataSingle(index)
                    Case eDataTypeToUse.DoubleType, eDataTypeToUse.FillingDoubleType
                        Return mDataDouble(index)
                End Select
            End If
        Catch ex As Exception
            ' index is probably out of range
            Return 0
        End Try
        Return 0
    End Function

    Public Function GetValueByIndexSng(index As Integer) As Single
        Try
            Return CSng(GetValueByIndex(index))
        Catch ex As Exception
            Return 0
        End Try
    End Function

    Public Function GetValueByOriginalIndexInt(index As Integer) As Integer
        Try
            Return CInt(GetValueByOriginalIndex(index))
        Catch ex As Exception
            Return 0
        End Try
    End Function

    Public Function GetValueByOriginalIndex(indexOriginal As Integer) As Double
        Dim index As Integer

        If Not mPointerArrayIsValid OrElse mDataType = eDataTypeToUse.NoDataPresent Then
            Return 0
        Else
            Try
                index = Array.IndexOf(mPointerIndices, indexOriginal)
                If index >= 0 Then
                    Select Case mDataType
                        Case eDataTypeToUse.IntegerType
                            Return mDataInt(mPointerIndices(index))
                        Case eDataTypeToUse.SingleType
                            Return mDataSingle(mPointerIndices(index))
                        Case eDataTypeToUse.DoubleType
                            Return mDataDouble(mPointerIndices(index))
                    End Select
                Else
                    Return 0
                End If
            Catch ex As Exception
                Return 0
            End Try
        End If
        Return 0
    End Function

    Public Function GetValueByOriginalIndexSng(index As Integer) As Single
        Try
            Return CSng(GetValueByOriginalIndex(index))
        Catch ex As Exception
            Return 0
        End Try
    End Function

    Private Sub InitializeLocalVariables()
        mDataType = eDataTypeToUse.NoDataPresent
        ClearUnusedData()

        mUsePointerIndexArray = True
        InitializePointerIndexArray(0)

    End Sub

    Private Sub InitializePointerIndexArray(length As Integer)
        Dim index As Integer

        If length < 0 Then length = 0
        ReDim mPointerIndices(length - 1)

        For index = 0 To length - 1
            mPointerIndices(index) = index
        Next index

        If length > 0 Then
            mPointerArrayIsValid = True
        Else
            mPointerArrayIsValid = False
        End If
    End Sub

End Class
