Option Strict On

Public Class clsPeakMatchingClass

    ' Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in October 2004
    ' Copyright 2005, Battelle Memorial Institute.  All Rights Reserved.
    '
    ' Last modified July 27, 2010

    Public Sub New()
        InitializeLocalVariables()
    End Sub

#Region "Constants and Enums"
    Public Const DEFAULT_SLIC_MAX_SEARCH_DISTANCE_MULTIPLIER As Single = 2

    Private Const STDEV_SCALING_FACTOR As Integer = 2
    Private Const ONE_PART_PER_MILLION As Double = 1000000.0#

    Public Enum eMessageTypeConstants
        Normal = 0
        ErrorMsg = 1
        Warning = 2
        Health = 3
    End Enum

#End Region

#Region "Structures"

    Public Structure udtFeatureInfoType
        Public FeatureID As Int32                                   ' Each feature should have a unique ID
        Public FeatureName As String                                ' Optional
        Public Mass As Double
        Public NET As Single
    End Structure

    Private Structure udtPeakMatchingRawMatchesType
        Public MatchingIDIndex As Integer               ' Pointer into comparison features (RowIndex in PMComparisonFeatureInfo)
        Public StandardizedSquaredDistance As Double
        Public SLiCScoreNumerator As Double
        Public SLiCScore As Double                      ' SLiC Score (Spatially Localized Confidence score)
        Public DelSLiC As Double                        ' Similar to DelCN, difference in SLiC score between top match and match with score value one less than this score
        Public MassErr As Double                        ' Observed difference (error) between comparison mass and feature mass (in Da)
        Public NETErr As Double                         ' Observed difference (error) between comparison NET and feature NET
    End Structure

    Private Structure udtSearchModeOptionsType
        Public UseMaxSearchDistanceMultiplierAndSLiCScore As Boolean
        Public UseEllipseSearchRegion As Boolean        ' Only valid if UseMaxSearchDistanceMultiplierAndSLiCScore = False; if both UseMaxSearchDistanceMultiplierAndSLiCScore = False and UseEllipseSearchRegion = False, then uses a rectangle for peak matching
    End Structure
#End Region

#Region "PMFeatureInfoClass"

    Friend Class PMFeatureInfoClass

        Protected Const MEMORY_RESERVE_CHUNK As Integer = 100000
        Protected mFeatureCount As Integer
        Protected mFeatures() As udtFeatureInfoType
        Protected mFeaturesArrayIsSorted As Boolean

        Protected mUseFeatureIDHashTable As Boolean
        Protected htFeatureIDToRowIndex As Hashtable

        Public Event SortingList()

        Public Sub New()
            mUseFeatureIDHashTable = True                       ' Set this to False to conserve memory; you must call Clear() after changing this for it to take effec
            Me.Clear()
        End Sub

        Public Overridable Function Add(ByRef udtFeatureInfo As udtFeatureInfoType) As Boolean
            With udtFeatureInfo
                Return Add(.FeatureID, .FeatureName, .Mass, .NET)
            End With
        End Function

        Public Overridable Function Add(intFeatureID As Integer, strPeptideName As String, dblPeptideMass As Double, sngPeptideNET As Single) As Boolean
            ' Returns True if the feature was added

            If ContainsFeature(intFeatureID) Then
                Return False
            Else
                ' Add the feature
                If mFeatureCount >= mFeatures.Length Then
                    ReDim Preserve mFeatures(mFeatures.Length * 2 - 1)
                    'ReDim Preserve mFeatures(mFeatures.Length + MEMORY_RESERVE_CHUNK - 1)
                End If

                With mFeatures(mFeatureCount)
                    .FeatureID = intFeatureID
                    .FeatureName = strPeptideName
                    .Mass = dblPeptideMass
                    .NET = sngPeptideNET
                End With

                If mUseFeatureIDHashTable Then
                    htFeatureIDToRowIndex.Add(intFeatureID, mFeatureCount)
                End If

                mFeatureCount += 1
                mFeaturesArrayIsSorted = False

                ' If we get here, all went well
                Return True
            End If

        End Function

        Protected Function BinarySearchFindFeature(intFeatureIDToFind As Integer) As Integer
            ' Looks through mFeatures() for intFeatureIDToFind, returning the index of the item if found, or -1 if not found

            Dim intMidIndex As Integer
            Dim intFirstIndex As Integer = 0
            Dim intLastIndex As Integer = mFeatureCount - 1

            Dim intMatchingRowIndex As Integer = -1

            If mFeatureCount <= 0 OrElse Not SortFeatures() Then
                Return intMatchingRowIndex
            End If

            Try
                intMidIndex = (intFirstIndex + intLastIndex) \ 2            ' Note: Using Integer division
                If intMidIndex < intFirstIndex Then intMidIndex = intFirstIndex

                Do While intFirstIndex <= intLastIndex And mFeatures(intMidIndex).FeatureID <> intFeatureIDToFind
                    If intFeatureIDToFind < mFeatures(intMidIndex).FeatureID Then
                        ' Search the lower half
                        intLastIndex = intMidIndex - 1
                    ElseIf intFeatureIDToFind > mFeatures(intMidIndex).FeatureID Then
                        ' Search the upper half
                        intFirstIndex = intMidIndex + 1
                    End If
                    ' Compute the new mid point
                    intMidIndex = (intFirstIndex + intLastIndex) \ 2
                    If intMidIndex < intFirstIndex Then Exit Do
                Loop

                If intMidIndex >= intFirstIndex And intMidIndex <= intLastIndex Then
                    If mFeatures(intMidIndex).FeatureID = intFeatureIDToFind Then
                        intMatchingRowIndex = intMidIndex
                    End If
                End If

            Catch ex As Exception
                intMatchingRowIndex = -1
            End Try

            Return intMatchingRowIndex

        End Function

        Public Overridable Sub Clear()
            mFeatureCount = 0

            If mFeatures Is Nothing Then
                ReDim mFeatures(MEMORY_RESERVE_CHUNK - 1)
            End If
            mFeaturesArrayIsSorted = False

            If mUseFeatureIDHashTable Then
                If htFeatureIDToRowIndex Is Nothing Then
                    htFeatureIDToRowIndex = New Hashtable
                Else
                    htFeatureIDToRowIndex.Clear()
                End If
            Else
                If Not htFeatureIDToRowIndex Is Nothing Then
                    htFeatureIDToRowIndex.Clear()
                    htFeatureIDToRowIndex = Nothing
                End If
            End If
        End Sub

        Protected Function ContainsFeature(intFeatureID As Integer) As Boolean
            Return ContainsFeature(intFeatureID, 0)
        End Function

        Protected Function ContainsFeature(intFeatureID As Integer, ByRef intRowIndex As Integer) As Boolean
            ' Returns True if the features array contains the feature
            ' If found, returns the row index in intRowIndex
            ' Note that the data will be sorted if necessary, which could lead to slow execution if this function is called repeatedly, while adding new data between calls

            If mUseFeatureIDHashTable Then
                If htFeatureIDToRowIndex.Contains(intFeatureID) Then
                    intRowIndex = CInt(htFeatureIDToRowIndex.Item(intFeatureID))
                Else
                    intRowIndex = -1
                End If
            Else
                ' Perform a binary search of mFeatures for intFeatureID
                intRowIndex = BinarySearchFindFeature(intFeatureID)
            End If

            If intRowIndex >= 0 Then
                Return True
            Else
                Return False
            End If

        End Function

        Public ReadOnly Property Count As Integer
            Get
                Return mFeatureCount
            End Get
        End Property

        Public Overridable Function GetFeatureInfoByFeatureID(intFeatureID As Integer, ByRef udtFeatureInfo As udtFeatureInfoType) As Boolean
            ' Return the feature info for feature intFeatureID

            Dim blnMatchFound As Boolean = False
            Dim intRowIndex As Integer

            blnMatchFound = Me.ContainsFeature(intFeatureID, intRowIndex)

            If blnMatchFound Then
                udtFeatureInfo = mFeatures(intRowIndex)
                Return True
            Else
                With udtFeatureInfo
                    .FeatureID = 0
                    .FeatureName = String.Empty
                    .Mass = 0
                    .NET = 0
                End With

                Return False
            End If
        End Function

        Public Overridable Function GetFeatureInfoByRowIndex(intRowIndex As Integer, ByRef udtFeatureInfo As udtFeatureInfoType) As Boolean

            If intRowIndex >= 0 And intRowIndex < mFeatureCount Then
                udtFeatureInfo = mFeatures(intRowIndex)
                Return True
            Else
                With udtFeatureInfo
                    .FeatureID = 0
                    .FeatureName = String.Empty
                    .Mass = 0
                    .NET = 0
                End With
                Return False
            End If

        End Function

        Public Overridable Function GetMassArrayByRowRange(intRowIndexStart As Integer, intRowIndexEnd As Integer) As Double()
            Dim dblMasses() As Double
            Dim intMatchCount As Integer

            If intRowIndexEnd < intRowIndexStart Then
                ReDim dblMasses(-1)
                Return dblMasses
            End If

            ReDim dblMasses(intRowIndexEnd - intRowIndexStart)

            Dim intIndex As Integer

            Try
                If intRowIndexEnd >= mFeatureCount Then
                    intRowIndexEnd = mFeatureCount - 1
                End If

                intMatchCount = 0
                For intIndex = intRowIndexStart To intRowIndexEnd
                    dblMasses(intMatchCount) = mFeatures(intIndex).Mass
                    intMatchCount += 1
                Next intIndex

                If dblMasses.Length > intMatchCount Then
                    ReDim Preserve dblMasses(intMatchCount - 1)
                End If

            Catch ex As Exception
                Throw ex
            End Try

            Return dblMasses
        End Function

        Public Overridable Function GetMassByRowIndex(intRowIndex As Integer) As Double
            If intRowIndex >= 0 And intRowIndex < mFeatureCount Then
                Return mFeatures(intRowIndex).Mass
            Else
                Return 0
            End If
        End Function

        Protected Overridable Function SortFeatures(Optional blnForceSort As Boolean = False) As Boolean

            If Not mFeaturesArrayIsSorted OrElse blnForceSort Then
                RaiseEvent SortingList()
                Try
                    Dim objComparer As New FeatureInfoComparerClass
                    Array.Sort(mFeatures, 0, mFeatureCount, objComparer)
                Catch ex As Exception
                    Throw ex
                End Try

                mFeaturesArrayIsSorted = True
            End If

            Return mFeaturesArrayIsSorted

        End Function

        Public Property UseFeatureIDHashTable As Boolean
            Get
                Return mUseFeatureIDHashTable
            End Get
            Set
                mUseFeatureIDHashTable = Value
            End Set
        End Property

        Private Class FeatureInfoComparerClass
            Implements IComparer(Of udtFeatureInfoType)

            Public Function Compare(x As udtFeatureInfoType, y As udtFeatureInfoType) As Integer Implements IComparer(Of udtFeatureInfoType).Compare

                ' Sort by Feature ID, ascending

                If x.FeatureID > y.FeatureID Then
                    Return 1
                ElseIf x.FeatureID < y.FeatureID Then
                    Return -1
                Else
                    Return 0
                End If

            End Function

        End Class
    End Class

#End Region

#Region "PMComparisonFeatureInfoClass"

    Friend Class PMComparisonFeatureInfoClass
        Inherits PMFeatureInfoClass

        Protected Structure udtComparisonFeatureInfoExtendedType
            Public NETStDev As Single
            Public DiscriminantScore As Single
        End Structure

        Protected mExtendedInfo() As udtComparisonFeatureInfoExtendedType

        Public Sub New()
            Me.Clear()
        End Sub

        Public Overloads Function Add(ByRef udtFeatureInfo As udtFeatureInfoType, sngPeptideNETStDev As Single, sngPeptideDiscriminantScore As Single) As Boolean
            With udtFeatureInfo
                Return Add(.FeatureID, .FeatureName, .Mass, .NET, sngPeptideNETStDev, sngPeptideDiscriminantScore)
            End With
        End Function

        Public Overloads Function Add(intFeatureID As Integer, strPeptideName As String, dblPeptideMass As Double, sngPeptideNET As Single, sngPeptideNETStDev As Single, sngPeptideDiscriminantScore As Single) As Boolean

            ' Add the base feature info
            If Not MyBase.Add(intFeatureID, strPeptideName, dblPeptideMass, sngPeptideNET) Then
                ' The feature already existed, and therefore wasn't added
                Return False
            Else

                ' Add the extended feature info
                If mExtendedInfo.Length < mFeatures.Length Then
                    ReDim Preserve mExtendedInfo(mFeatures.Length - 1)
                End If

                With mExtendedInfo(mFeatureCount - 1)
                    .NETStDev = sngPeptideNETStDev
                    .DiscriminantScore = sngPeptideDiscriminantScore
                End With

                ' If we get here, all went well
                Return True

            End If
        End Function

        Public Overloads Sub Clear()
            MyBase.Clear()

            If mExtendedInfo Is Nothing Then
                ReDim mExtendedInfo(MEMORY_RESERVE_CHUNK - 1)
            End If

        End Sub

        Public Overloads Function GetFeatureInfoByFeatureID(intFeatureID As Integer, ByRef udtFeatureInfo As udtFeatureInfoType, ByRef sngNETStDev As Single, ByRef sngDiscriminantScore As Single) As Boolean
            ' Return the feature info for feature intFeatureID

            Dim blnMatchFound As Boolean = False
            Dim intRowIndex As Integer

            blnMatchFound = Me.ContainsFeature(intFeatureID, intRowIndex)

            If blnMatchFound Then
                udtFeatureInfo = mFeatures(intRowIndex)
                sngNETStDev = mExtendedInfo(intRowIndex).NETStDev
                sngDiscriminantScore = mExtendedInfo(intRowIndex).DiscriminantScore
                Return True
            Else
                With udtFeatureInfo
                    .FeatureID = 0
                    .FeatureName = String.Empty
                    .Mass = 0
                    .NET = 0
                End With
                sngNETStDev = 0
                sngDiscriminantScore = 0

                Return False
            End If
        End Function

        Public Overloads Function GetFeatureInfoByRowIndex(intRowIndex As Integer, ByRef udtFeatureInfo As udtFeatureInfoType, ByRef sngNETStDev As Single, ByRef sngDiscriminantScore As Single) As Boolean

            If intRowIndex >= 0 And intRowIndex < mFeatureCount Then
                udtFeatureInfo = mFeatures(intRowIndex)
                sngNETStDev = mExtendedInfo(intRowIndex).NETStDev
                sngDiscriminantScore = mExtendedInfo(intRowIndex).DiscriminantScore
                Return True
            Else
                With udtFeatureInfo
                    .FeatureID = 0
                    .FeatureName = String.Empty
                    .Mass = 0
                    .NET = 0
                End With
                sngNETStDev = 0
                sngDiscriminantScore = 0
                Return False
            End If

        End Function

        Public Overridable Function GetNETStDevByRowIndex(intRowIndex As Integer) As Single
            If intRowIndex >= 0 And intRowIndex < mFeatureCount Then
                Return mExtendedInfo(intRowIndex).NETStDev
            Else
                Return 0
            End If
        End Function

    End Class

#End Region

#Region "PMFeatureMatchResultsClass"
    Friend Class PMFeatureMatchResultsClass

        Public Structure udtPeakMatchingResultType
            Public MatchingID As Integer                ' ID of the comparison feature (this is the real ID, and not a RowIndex)
            Public SLiCScore As Double                  ' SLiC Score (Spatially Localized Confidence score)
            Public DelSLiC As Double                    ' Similar to DelCN, difference in SLiC score between top match and match with score value one less than this score
            Public MassErr As Double                    ' Observed difference (error) between comparison mass and feature mass (in Da)
            Public NETErr As Double                     ' Observed difference (error) between comparison NET and feature NET
            Public MultiAMTHitCount As Integer          ' The number of Unique mass tag hits for each UMC; only applies to AMT's
        End Structure

        Protected Structure udtPeakMatchingResultsType
            Public FeatureID As Integer
            Public Details As udtPeakMatchingResultType
        End Structure

        Protected Const MEMORY_RESERVE_CHUNK As Integer = 100000
        Protected mPMResultsCount As Integer
        Protected mPMResults() As udtPeakMatchingResultsType
        Protected mPMResultsIsSorted As Boolean

        Public Event SortingList()

        Public Sub New()
            Me.Clear()
        End Sub

        Public Function AddMatch(intFeatureID As Integer, ByRef udtMatchResultInfo As udtPeakMatchingResultType) As Boolean
            With udtMatchResultInfo
                Return AddMatch(intFeatureID, .MatchingID, .SLiCScore, .DelSLiC, .MassErr, .NETErr, .MultiAMTHitCount)
            End With
        End Function

        Public Function AddMatch(intFeatureID As Integer, intMatchingID As Integer, SLiCScore As Double, DelSLiC As Double, MassErr As Double, NETErr As Double, MultiAMTHitCount As Integer) As Boolean

            ' Add the match
            If mPMResultsCount >= mPMResults.Length Then
                ReDim Preserve mPMResults(mPMResults.Length * 2 - 1)
                'ReDim Preserve mPMResults(mPMResults.Length + MEMORY_RESERVE_CHUNK - 1)
            End If

            With mPMResults(mPMResultsCount)
                .FeatureID = intFeatureID
                .Details.MatchingID = intMatchingID
                .Details.SLiCScore = SLiCScore
                .Details.DelSLiC = DelSLiC
                .Details.MassErr = MassErr
                .Details.NETErr = NETErr
                .Details.MultiAMTHitCount = MultiAMTHitCount
            End With

            mPMResultsCount += 1
            mPMResultsIsSorted = False

            ' If we get here, all went well
            Return True

        End Function

        Private Function BinarySearchPMResults(intFeatureIDToFind As Integer) As Integer
            ' Looks through mPMResults() for intFeatureIDToFind, returning the index of the item if found, or -1 if not found
            ' Since mPMResults() can contain multiple entries for a given Feature, this function returns the first entry found

            Dim intMidIndex As Integer
            Dim intFirstIndex As Integer = 0
            Dim intLastIndex As Integer = mPMResultsCount - 1

            Dim intMatchingRowIndex As Integer = -1

            If mPMResultsCount <= 0 OrElse Not SortPMResults() Then
                Return intMatchingRowIndex
            End If

            Try
                intMidIndex = (intFirstIndex + intLastIndex) \ 2            ' Note: Using Integer division
                If intMidIndex < intFirstIndex Then intMidIndex = intFirstIndex

                Do While intFirstIndex <= intLastIndex And mPMResults(intMidIndex).FeatureID <> intFeatureIDToFind
                    If intFeatureIDToFind < mPMResults(intMidIndex).FeatureID Then
                        ' Search the lower half
                        intLastIndex = intMidIndex - 1
                    ElseIf intFeatureIDToFind > mPMResults(intMidIndex).FeatureID Then
                        ' Search the upper half
                        intFirstIndex = intMidIndex + 1
                    End If
                    ' Compute the new mid point
                    intMidIndex = (intFirstIndex + intLastIndex) \ 2
                    If intMidIndex < intFirstIndex Then Exit Do
                Loop

                If intMidIndex >= intFirstIndex And intMidIndex <= intLastIndex Then
                    If mPMResults(intMidIndex).FeatureID = intFeatureIDToFind Then
                        intMatchingRowIndex = intMidIndex
                    End If
                End If

            Catch ex As Exception
                intMatchingRowIndex = -1
            End Try

            Return intMatchingRowIndex

        End Function

        Public Overridable Sub Clear()
            mPMResultsCount = 0

            If mPMResults Is Nothing Then
                ReDim mPMResults(MEMORY_RESERVE_CHUNK - 1)
            End If
            mPMResultsIsSorted = False

        End Sub

        Public ReadOnly Property Count As Integer
            Get
                Return mPMResultsCount
            End Get
        End Property

        Public Function GetMatchInfoByFeatureID(intFeatureID As Integer, ByRef udtMatchResults() As udtPeakMatchingResultType, ByRef intMatchCount As Integer) As Boolean
            ' Returns all of the matches for the given feature ID row index
            ' Returns false if the feature has no matches
            ' Note that this function never shrinks udtMatchResults; it only expands it if needed

            Dim blnMatchesFound As Boolean = False

            Try
                Dim intIndex As Integer
                Dim intIndexFirst As Integer
                Dim intIndexLast As Integer

                If GetRowIndicesForFeatureID(intFeatureID, intIndexFirst, intIndexLast) Then

                    intMatchCount = intIndexLast - intIndexFirst + 1
                    If udtMatchResults Is Nothing OrElse intMatchCount > udtMatchResults.Length Then
                        ReDim udtMatchResults(intMatchCount - 1)
                    End If

                    For intIndex = intIndexFirst To intIndexLast
                        udtMatchResults(intIndex - intIndexFirst) = mPMResults(intIndex).Details
                    Next intIndex
                    blnMatchesFound = True

                End If

            Catch ex As Exception
                blnMatchesFound = False
            End Try


            Return blnMatchesFound

        End Function

        Public Function GetMatchInfoByRowIndex(intRowIndex As Integer, ByRef intFeatureID As Integer, ByRef udtMatchResultInfo As udtPeakMatchingResultType) As Boolean
            ' Populates intFeatureID and udtMatchResultInfo with the peak matching results for the given row index

            Dim blnMatchFound As Boolean = False

            Try
                If intRowIndex < mPMResultsCount Then
                    intFeatureID = mPMResults(intRowIndex).FeatureID
                    udtMatchResultInfo = mPMResults(intRowIndex).Details

                    blnMatchFound = True
                End If
            Catch ex As Exception
                blnMatchFound = False
            End Try

            Return blnMatchFound

        End Function

        Private Function GetRowIndicesForFeatureID(intFeatureID As Integer, ByRef intIndexFirst As Integer, ByRef intIndexLast As Integer) As Boolean
            ' Looks for intFeatureID in mPMResults
            ' If found, returns the range of rows that contain matches for intFeatureID


            ' Perform a binary search of mPMResults for intFeatureID
            intIndexFirst = BinarySearchPMResults(intFeatureID)

            If intIndexFirst >= 0 Then

                ' Match found; need to find all of the rows with intFeatureID
                intIndexLast = intIndexFirst

                ' Step backward through mPMResults to find the first match for intFeatureID
                Do While intIndexFirst > 0 AndAlso mPMResults(intIndexFirst - 1).FeatureID = intFeatureID
                    intIndexFirst -= 1
                Loop

                ' Step forward through mPMResults to find the last match for intFeatureID
                Do While intIndexLast < mPMResultsCount - 1 AndAlso mPMResults(intIndexLast + 1).FeatureID = intFeatureID
                    intIndexLast += 1
                Loop

                Return True
            Else
                intIndexFirst = -1
                intIndexLast = -1
                Return False
            End If

        End Function

        Public ReadOnly Property MatchCountForFeatureID(intFeatureID As Integer) As Integer
            Get
                Dim intIndexFirst As Integer
                Dim intIndexLast As Integer

                If GetRowIndicesForFeatureID(intFeatureID, intIndexFirst, intIndexLast) Then
                    Return intIndexLast - intIndexFirst + 1
                Else
                    Return 0
                End If

            End Get
        End Property

        Private Function SortPMResults(Optional blnForceSort As Boolean = False) As Boolean

            If Not mPMResultsIsSorted OrElse blnForceSort Then
                RaiseEvent SortingList()
                Try
                    Dim objComparer As New PeakMatchingResultsComparerClass
                    Array.Sort(mPMResults, 0, mPMResultsCount, objComparer)
                Catch ex As Exception
                    Throw ex
                End Try

                mPMResultsIsSorted = True
            End If

            Return mPMResultsIsSorted

        End Function

        Private Class PeakMatchingResultsComparerClass
            Implements IComparer(Of udtPeakMatchingResultsType)

            Public Function Compare(x As udtPeakMatchingResultsType, y As udtPeakMatchingResultsType) As Integer Implements IComparer(Of udtPeakMatchingResultsType).Compare

                ' Sort by .SLiCScore descending, and by MatchingIDIndexOriginal ascending

                If x.FeatureID > y.FeatureID Then
                    Return 1
                ElseIf x.FeatureID < y.FeatureID Then
                    Return -1
                Else
                    If x.Details.MatchingID > y.Details.MatchingID Then
                        Return 1
                    ElseIf x.Details.MatchingID < y.Details.MatchingID Then
                        Return -1
                    Else
                        Return 0
                    End If
                End If

            End Function

        End Class
    End Class
#End Region

#Region "Classwide Variables"
    Private mMaxPeakMatchingResultsPerFeatureToSave As Integer
    Private mSearchModeOptions As udtSearchModeOptionsType

    Private mProgressDescription As String
    Private mProgessPct As Single
    Private mAbortProcessing As Boolean

    Public Event ProgressContinues()
    Public Event LogEvent(Message As String, EventType As eMessageTypeConstants)

#End Region

#Region "Processing Options Interface Functions"

    Public Property MaxPeakMatchingResultsPerFeatureToSave As Integer
        Get
            Return mMaxPeakMatchingResultsPerFeatureToSave
        End Get
        Set
            If Value < 0 Then Value = 0
            mMaxPeakMatchingResultsPerFeatureToSave = Value
        End Set
    End Property

    Public ReadOnly Property ProgressDescription As String
        Get
            Return mProgressDescription
        End Get
    End Property

    Public ReadOnly Property ProgessPct As Single
        Get
            Return mProgessPct
        End Get
    End Property

    Public Property UseMaxSearchDistanceMultiplierAndSLiCScore As Boolean
        Get
            Return mSearchModeOptions.UseMaxSearchDistanceMultiplierAndSLiCScore
        End Get
        Set
            mSearchModeOptions.UseMaxSearchDistanceMultiplierAndSLiCScore = Value
        End Set
    End Property

    Public Property UseEllipseSearchRegion As Boolean
        Get
            Return mSearchModeOptions.UseEllipseSearchRegion
        End Get
        Set
            mSearchModeOptions.UseEllipseSearchRegion = Value
        End Set
    End Property

    ''Public Property SqlServerConnectionString() As String
    ''    Get
    ''        Return mSqlServerConnectionString
    ''    End Get
    ''    Set(Value As String)
    ''        mSqlServerConnectionString = Value
    ''    End Set
    ''End Property

    ''Public Property SqlServerTableNameFeatureMatchResults() As String
    ''    Get
    ''        Return mTableNameFeatureMatchResults
    ''    End Get
    ''    Set(Value As String)
    ''        mTableNameFeatureMatchResults = Value
    ''    End Set
    ''End Property

    ''Public Property UseSqlServerDBToCacheData() As Boolean
    ''    Get
    ''        Return mUseSqlServerDBToCacheData
    ''    End Get
    ''    Set(Value As Boolean)
    ''        mUseSqlServerDBToCacheData = Value
    ''    End Set
    ''End Property

    ''Public Property UseSqlServerForMatchResults() As Boolean
    ''    Get
    ''        Return mUseSqlServerForMatchResults
    ''    End Get
    ''    Set(Value As Boolean)
    ''        mUseSqlServerForMatchResults = Value
    ''    End Set
    ''End Property

#End Region

#Region "Peak Matching Functions"

    Public Sub AbortProcessingNow()
        mAbortProcessing = True
    End Sub

    Private Sub ComputeSLiCScores(ByRef udtFeatureToIdentify As udtFeatureInfoType, ByRef objFeatureMatchResults As PMFeatureMatchResultsClass, ByRef udtRawMatches() As udtPeakMatchingRawMatchesType, ByRef objComparisonFeatures As PMComparisonFeatureInfoClass, ByRef objSearchThresholds As clsSearchThresholds, udtComputedTolerances As clsSearchThresholds.udtSearchTolerancesType)

        Dim intIndex As Integer
        Dim intNewMatchCount As Integer

        Dim dblMassStDevPPM As Double
        Dim dblMassStDevAbs As Double

        Dim dblNETStDevCombined As Double
        Dim dblNumeratorSum As Double

        Dim udtComparisonFeatureInfo As udtFeatureInfoType = New udtFeatureInfoType

        Dim strMessage As String

        ' Compute the match scores (aka SLiC scores)

        dblMassStDevPPM = objSearchThresholds.SLiCScoreMassPPMStDev
        If dblMassStDevPPM <= 0 Then dblMassStDevPPM = 3

        dblMassStDevAbs = objSearchThresholds.PPMToMass(dblMassStDevPPM, udtFeatureToIdentify.Mass)
        If dblMassStDevAbs <= 0 Then
            strMessage = "Assertion failed in ComputeSLiCScores; dblMassStDevAbs is <= 0, which isn't allowed; will assume 0.003"
            Console.WriteLine(strMessage)
            PostLogEntry(strMessage, eMessageTypeConstants.ErrorMsg)
            dblMassStDevAbs = 0.003
        End If

        ' Compute the standarized squared distance and the numerator sum
        dblNumeratorSum = 0
        For intIndex = 0 To udtRawMatches.Length - 1

            If objSearchThresholds.SLiCScoreUseAMTNETStDev Then
                ' The NET StDev is computed by combining the default NETStDev value with the Comparison Features's specific NETStDev
                ' The combining is done by "adding in quadrature", which means to square each number, add together, and take the square root
                dblNETStDevCombined = Math.Sqrt(objSearchThresholds.SLiCScoreNETStDev ^ 2 + objComparisonFeatures.GetNETStDevByRowIndex(udtRawMatches(intIndex).MatchingIDIndex) ^ 2)
            Else
                ' Simply use the default NETStDev value
                dblNETStDevCombined = objSearchThresholds.SLiCScoreNETStDev
            End If

            If dblNETStDevCombined <= 0 Then
                strMessage = "Assertion failed in ComputeSLiCScores; dblNETStDevCombined is <= 0, which isn't allowed; will assume 0.025"
                Console.WriteLine(strMessage)
                PostLogEntry(strMessage, eMessageTypeConstants.ErrorMsg)
                dblNETStDevCombined = 0.025
            End If

            With udtRawMatches(intIndex)
                .StandardizedSquaredDistance = .MassErr ^ 2 / dblMassStDevAbs ^ 2 + .NETErr ^ 2 / dblNETStDevCombined ^ 2

                .SLiCScoreNumerator = (1 / (dblMassStDevAbs * dblNETStDevCombined)) * Math.Exp(- .StandardizedSquaredDistance / 2)

                dblNumeratorSum += .SLiCScoreNumerator

            End With
        Next intIndex

        ' Compute the match score for each match
        For intIndex = 0 To udtRawMatches.Length - 1
            With udtRawMatches(intIndex)
                If dblNumeratorSum > 0 Then
                    .SLiCScore = Math.Round(.SLiCScoreNumerator / dblNumeratorSum, 5)
                Else
                    .SLiCScore = 0
                End If
            End With
        Next intIndex

        If udtRawMatches.Length > 1 Then
            ' Sort by SLiCScore descending
            Dim iPeakMatchingRawMatchesComparerClass As New PeakMatchingRawMatchesComparerClass
            Array.Sort(udtRawMatches, iPeakMatchingRawMatchesComparerClass)
            iPeakMatchingRawMatchesComparerClass = Nothing
        End If

        If udtRawMatches.Length > 0 Then

            ' Compute the DelSLiC value
            ' If there is only one match, then the DelSLiC value is 1
            ' If there is more than one match, then the highest scoring match gets a DelSLiC value,
            '  computed by subtracting the next lower scoring value from the highest scoring value; all
            '  other matches get a DelSLiC score of 0
            ' This allows one to quickly identify the features with a single match (DelSLiC = 1) or with a match
            '  distinct from other matches (DelSLiC > threshold)

            If udtRawMatches.Length > 1 Then
                udtRawMatches(0).DelSLiC = (udtRawMatches(0).SLiCScore - udtRawMatches(1).SLiCScore)

                For intIndex = 1 To udtRawMatches.Length - 1
                    udtRawMatches(intIndex).DelSLiC = 0
                Next intIndex
            Else
                udtRawMatches(0).DelSLiC = 1
            End If

            ' Now filter the list using the tighter tolerances:
            ' Since we're shrinking the array, we can copy in place
            '
            ' When testing whether to keep the match or not, we're testing whether the match is in the ellipse bounded by MWTolAbsFinal and NETTolFinal
            ' Note that these are half-widths of the ellipse
            intNewMatchCount = 0
            For intIndex = 0 To udtRawMatches.Length - 1
                If TestPointInEllipse(udtRawMatches(intIndex).NETErr, udtRawMatches(intIndex).MassErr, udtComputedTolerances.NETTolFinal, udtComputedTolerances.MWTolAbsFinal) Then
                    udtRawMatches(intNewMatchCount) = udtRawMatches(intIndex)
                    intNewMatchCount += 1
                End If
            Next intIndex

            If intNewMatchCount = 0 Then
                ReDim udtRawMatches(-1)
            ElseIf intNewMatchCount < udtRawMatches.Length Then
                ReDim Preserve udtRawMatches(intNewMatchCount - 1)
            End If

            ' Add new match results to objFeatureMatchResults
            ' Record, at most, mMaxPeakMatchingResultsPerFeatureToSave entries
            For intIndex = 0 To CInt(Math.Min(mMaxPeakMatchingResultsPerFeatureToSave, udtRawMatches.Length)) - 1
                With udtRawMatches(intIndex)
                    objComparisonFeatures.GetFeatureInfoByRowIndex(.MatchingIDIndex, udtComparisonFeatureInfo)
                    objFeatureMatchResults.AddMatch(udtFeatureToIdentify.FeatureID, udtComparisonFeatureInfo.FeatureID, .SLiCScore, .DelSLiC, .MassErr, .NETErr, udtRawMatches.Length)
                End With
            Next intIndex
        End If

    End Sub

    Friend Shared Function FillRangeSearchObject(ByRef objRangeSearch As clsSearchRange, ByRef objComparisonFeatures As PMComparisonFeatureInfoClass) As Boolean
        ' Initialize the range searching class

        Const LOAD_BLOCK_SIZE As Integer = 50000
        Dim intIndex As Integer
        Dim intComparisonFeatureCount As Integer
        Dim blnSuccess As Boolean

        Try
            If objRangeSearch Is Nothing Then
                objRangeSearch = New clsSearchRange
            Else
                objRangeSearch.ClearData()
            End If

            If objComparisonFeatures.Count <= 0 Then
                ' No comparison features to search against
                blnSuccess = False
            Else
                objRangeSearch.InitializeDataFillDouble(objComparisonFeatures.Count)

                ''For intIndex = 0 To objComparisonFeatures.Count - 1
                ''    objRangeSearch.FillWithDataAddPoint(objComparisonFeatures.GetMassByRowIndex(intIndex))
                ''Next intIndex

                intComparisonFeatureCount = objComparisonFeatures.Count
                intIndex = 0
                Do While intIndex < intComparisonFeatureCount
                    objRangeSearch.FillWithDataAddBlock(objComparisonFeatures.GetMassArrayByRowRange(intIndex, intIndex + LOAD_BLOCK_SIZE - 1))
                    intIndex += LOAD_BLOCK_SIZE
                Loop

                blnSuccess = objRangeSearch.FinalizeDataFill()
            End If

        Catch ex As Exception
            Throw ex
            blnSuccess = False
        End Try

        Return blnSuccess

    End Function

    Friend Function IdentifySequences(objSearchThresholds As clsSearchThresholds, ByRef objFeaturesToIdentify As PMFeatureInfoClass, ByRef objComparisonFeatures As PMComparisonFeatureInfoClass, ByRef objFeatureMatchResults As PMFeatureMatchResultsClass, Optional ByRef objRangeSearch As clsSearchRange = Nothing) As Boolean
        ' Returns True if success, False if the search is cancelled
        ' Will return true even if none of the features match any of the comparison features
        '
        ' If objRangeSearch is Nothing or if objRangeSearch contains a different number of entries than udtComparisonfeatures,
        '   then will auto-populate it; otherwise, assumes it is populated

        ' Note that objFeatureMatchResults will only contain info on the features in objFeaturesToIdentify that matched entries in objComparisonFeatures

        Dim intFeatureIndex As Integer
        Dim intFeatureCount As Integer

        Dim intMatchIndex As Integer
        Dim intComparisonFeaturesOriginalRowIndex As Integer

        Dim udtCurrentFeatureToIdentify As udtFeatureInfoType = New udtFeatureInfoType
        Dim udtCurrentComparisonFeature As udtFeatureInfoType = New udtFeatureInfoType

        Dim dblMassTol As Double, dblNETTol As Double
        Dim dblNetDiff As Double

        Dim MatchInd1, MatchInd2 As Integer
        Dim udtComputedTolerances As clsSearchThresholds.udtSearchTolerancesType

        ' The following hold the matches using the broad search tolerances (if .UseMaxSearchDistanceMultiplierAndSLiCScore = True, otherwise, simply holds the matches)
        Dim intRawMatchCount As Integer
        Dim udtRawMatches() As udtPeakMatchingRawMatchesType    ' Pointers into objComparisonFeatures; list of peptides that match within both mass and NET tolerance

        Dim intMatchResultsCount As Integer

        Dim blnStoreMatch As Boolean
        Dim blnSuccess As Boolean

        Dim strMessage As String

        If objFeatureMatchResults Is Nothing Then
            ''If mUseSqlServerForMatchResults Then
            ''    objFeatureMatchResults = New PMFeatureMatchResultsClass(mSqlServerConnectionString, mTableNameFeatureMatchResults)
            ''Else
            objFeatureMatchResults = New PMFeatureMatchResultsClass
            ''End If
        Else
            ' Clear any existing results
            objFeatureMatchResults.Clear()
        End If

        If objRangeSearch Is Nothing OrElse objRangeSearch.DataCount <> objComparisonFeatures.Count Then
            blnSuccess = FillRangeSearchObject(objRangeSearch, objComparisonFeatures)
        Else
            blnSuccess = True
        End If

        If Not blnSuccess Then Return False

        Try
            intMatchResultsCount = 0
            intFeatureCount = objFeaturesToIdentify.Count

            UpdateProgress("Finding matching peptides for given search thresholds", 0)
            mAbortProcessing = False

            PostLogEntry("IdentifySequences starting, total feature count = " & intFeatureCount.ToString, eMessageTypeConstants.Normal)

            For intFeatureIndex = 0 To intFeatureCount - 1
                ' Use objRangeSearch to search for matches to each peptide in udtComparisonFeatures

                If objFeaturesToIdentify.GetFeatureInfoByRowIndex(intFeatureIndex, udtCurrentFeatureToIdentify) Then
                    ' By Calling .ComputedSearchTolerances() with a mass, the tolerances will be auto re-computed
                    udtComputedTolerances = objSearchThresholds.ComputedSearchTolerances(udtCurrentFeatureToIdentify.Mass)

                    If mSearchModeOptions.UseMaxSearchDistanceMultiplierAndSLiCScore Then
                        dblMassTol = udtComputedTolerances.MWTolAbsBroad
                        dblNETTol = udtComputedTolerances.NETTolBroad
                    Else
                        dblMassTol = udtComputedTolerances.MWTolAbsFinal
                        dblNETTol = udtComputedTolerances.NETTolFinal
                    End If

                    MatchInd1 = 0
                    MatchInd2 = -1
                    If objRangeSearch.FindValueRange(udtCurrentFeatureToIdentify.Mass, dblMassTol, MatchInd1, MatchInd2) Then

                        intRawMatchCount = 0
                        ReDim udtRawMatches(MatchInd2 - MatchInd1)

                        For intMatchIndex = MatchInd1 To MatchInd2
                            intComparisonFeaturesOriginalRowIndex = objRangeSearch.OriginalIndex(intMatchIndex)

                            If objComparisonFeatures.GetFeatureInfoByRowIndex(intComparisonFeaturesOriginalRowIndex, udtCurrentComparisonFeature) Then
                                dblNetDiff = udtCurrentFeatureToIdentify.NET - udtCurrentComparisonFeature.NET
                                If Math.Abs(dblNetDiff) <= dblNETTol Then

                                    If mSearchModeOptions.UseMaxSearchDistanceMultiplierAndSLiCScore Then
                                        ' Store this match
                                        blnStoreMatch = True
                                    Else
                                        ' The match is within a rectangle defined by udtComputedTolerances.MWTolAbsBroad and udtComputedTolerances.NETTolBroad
                                        If mSearchModeOptions.UseEllipseSearchRegion Then
                                            ' Only keep the match if it's within the ellipse defined by the search tolerances
                                            ' Note that the search tolerances we send to TestPointInEllipse should be half-widths (i.e. tolerance +- comparison value), not full widths
                                            blnStoreMatch = TestPointInEllipse(dblNetDiff, udtCurrentFeatureToIdentify.Mass - udtCurrentComparisonFeature.Mass, dblNETTol, dblMassTol)
                                        Else
                                            blnStoreMatch = True
                                        End If
                                    End If

                                    If blnStoreMatch Then
                                        udtRawMatches(intRawMatchCount).MatchingIDIndex = intComparisonFeaturesOriginalRowIndex
                                        udtRawMatches(intRawMatchCount).SLiCScore = -1
                                        udtRawMatches(intRawMatchCount).MassErr = udtCurrentFeatureToIdentify.Mass - udtCurrentComparisonFeature.Mass
                                        udtRawMatches(intRawMatchCount).NETErr = dblNetDiff

                                        intRawMatchCount += 1
                                    End If
                                End If

                            End If

                        Next intMatchIndex

                        If intRawMatchCount > 0 Then
                            ' Store the FeatureIDIndex in objFeatureMatchResults
                            If intRawMatchCount < udtRawMatches.Length Then
                                ' Shrink udtRawMatches
                                ReDim Preserve udtRawMatches(intRawMatchCount - 1)
                            End If

                            ' Compute the SLiC Scores and store the results
                            ComputeSLiCScores(udtCurrentFeatureToIdentify, objFeatureMatchResults, udtRawMatches, objComparisonFeatures, objSearchThresholds, udtComputedTolerances)
                        End If

                        intMatchResultsCount += 1
                    End If
                Else
                    strMessage = "Programming error in IdentifySequences: Feature not found in objFeaturesToIdentify using feature index: " & intFeatureIndex.ToString
                    Console.WriteLine(strMessage)
                    PostLogEntry(strMessage, eMessageTypeConstants.ErrorMsg)
                End If

                If intFeatureIndex Mod 100 = 0 Then
                    UpdateProgress(CSng(intFeatureIndex / intFeatureCount * 100))
                    If mAbortProcessing Then Exit For
                End If

                If intFeatureIndex Mod 10000 = 0 AndAlso intFeatureIndex > 0 Then
                    PostLogEntry("IdentifySequences, intFeatureIndex = " & intFeatureIndex.ToString, eMessageTypeConstants.Health)
                End If
            Next intFeatureIndex

            UpdateProgress("IdentifySequences complete", 100)
            PostLogEntry("IdentifySequences complete", eMessageTypeConstants.Normal)

            blnSuccess = Not mAbortProcessing

        Catch ex As Exception
            blnSuccess = False
        End Try

        Return blnSuccess

    End Function

    Private Sub InitializeLocalVariables()
        mMaxPeakMatchingResultsPerFeatureToSave = 20

        With mSearchModeOptions
            .UseMaxSearchDistanceMultiplierAndSLiCScore = True
            .UseEllipseSearchRegion = True
        End With

        ''mUseSqlServerDBToCacheData = False
        ''mUseSqlServerForMatchResults = False
        ''mSqlServerConnectionString = SharedADONetFunctions.DEFAULT_CONNECTION_STRING_NO_PROVIDER
        ''mTableNameFeatureMatchResults = PMFeatureMatchResultsClass.DEFAULT_FEATURE_MATCH_RESULTS_TABLE_NAME
    End Sub

    Private Sub PostLogEntry(strMessage As String, EntryType As eMessageTypeConstants)
        RaiseEvent LogEvent(strMessage, EntryType)
    End Sub

    Private Function TestPointInEllipse(dblPointX As Double, dblPointY As Double, dblXTol As Double, dblYTol As Double) As Boolean
        ' The equation for the points along the edge of an ellipse is x^2/a^2 + y^2/b^2 = 1 where a and b are 
        ' the half-widths of the ellipse and x and y are the coordinates of each point on the ellipse's perimeter
        '
        ' This function takes x, y, a, and b as inputs and computes the result of this equation
        ' If the result is <= 1, then the point at x,y is inside the ellipse

        Try
            If dblPointX ^ 2 / dblXTol ^ 2 + dblPointY ^ 2 / dblYTol ^ 2 <= 1 Then
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            ' Error; return false
            Return False
        End Try

    End Function

    Private Sub UpdateProgress(sngProgessPct As Single)
        mProgessPct = sngProgessPct
        RaiseEvent ProgressContinues()
    End Sub

    Private Sub UpdateProgress(strProgressDescription As String, sngProgessPct As Single)
        mProgressDescription = strProgressDescription
        mProgessPct = sngProgessPct
        RaiseEvent ProgressContinues()
    End Sub

#End Region

#Region "Peak Matching Raw Matches Sorting Class"

    Private Class PeakMatchingRawMatchesComparerClass
        Implements IComparer(Of udtPeakMatchingRawMatchesType)

        Public Function Compare(x As udtPeakMatchingRawMatchesType, y As udtPeakMatchingRawMatchesType) As Integer Implements IComparer(Of udtPeakMatchingRawMatchesType).Compare

            ' Sort by .SLiCScore descending, and by MatchingIDIndexOriginal Ascencing

            If x.SLiCScore > y.SLiCScore Then
                Return -1
            ElseIf x.SLiCScore < y.SLiCScore Then
                Return 1
            Else
                If x.MatchingIDIndex > y.MatchingIDIndex Then
                    Return 1
                ElseIf x.MatchingIDIndex < y.MatchingIDIndex Then
                    Return -1
                Else
                    Return 0
                End If
            End If

        End Function

    End Class

#End Region

#Region "Search Thresholds Class"
    Public Class clsSearchThresholds

        Public Sub New()
            InitializeLocalVariables()
        End Sub

#Region "Constants and Enums"

        Public Enum MassToleranceConstants
            PPM = 0             ' parts per million
            Absolute = 1        ' absolute (Da)
        End Enum

#End Region

#Region "Structures"

        ' The following defines how the SLiC scores (aka match scores) are computed
        Private Structure udtSLiCScoreOptionsType
            Public MassPPMStDev As Double                  ' Default 3
            Public NETStDev As Double                      ' Default 0.025
            Public UseAMTNETStDev As Boolean
            Public MaxSearchDistanceMultiplier As Single   ' Default 2
        End Structure

        ' Note that all of these tolerances are half-widths, i.e. tolerance +- comparison value
        Public Structure udtSearchTolerancesType
            Public MWTolAbsBroad As Double
            Public MWTolAbsFinal As Double

            Public NETTolBroad As Double
            Public NETTolFinal As Double
        End Structure
#End Region

#Region "Classwide Variables"

        Private mMassTolType As MassToleranceConstants
        Private mMassTolerance As Double          ' Mass search tolerance, +- this value; TolType defines if this is PPM or Da
        Private mNETTolerance As Double           ' NET search tolerance, +- this value
        Private mSLiCScoreMaxSearchDistanceMultiplier As Single

        Private mSLiCScoreOptions As udtSLiCScoreOptionsType

        Private mAutoDefineSLiCScoreThresholds As Boolean

        Private mComputedSearchTolerances As udtSearchTolerancesType

#End Region

#Region "Processing Options Interface Functions"

        Public Property AutoDefineSLiCScoreThresholds As Boolean
            Get
                Return mAutoDefineSLiCScoreThresholds
            End Get
            Set
                mAutoDefineSLiCScoreThresholds = Value
            End Set
        End Property

        Public ReadOnly Property ComputedSearchTolerances As udtSearchTolerancesType
            Get
                Return mComputedSearchTolerances
            End Get
        End Property

        Public ReadOnly Property ComputedSearchTolerances(dblReferenceMass As Double) As udtSearchTolerancesType
            Get
                DefinePeakMatchingTolerances(dblReferenceMass)
                Return mComputedSearchTolerances
            End Get
        End Property

        Public Property MassTolType As MassToleranceConstants
            Get
                Return mMassTolType
            End Get
            Set
                mMassTolType = Value
            End Set
        End Property

        Public Property MassTolerance As Double
            Get
                Return mMassTolerance
            End Get
            Set
                mMassTolerance = Value
                If mAutoDefineSLiCScoreThresholds Then
                    InitializeSLiCScoreOptions(True)
                End If
            End Set
        End Property

        Public Property NETTolerance As Double
            Get
                Return mNETTolerance
            End Get
            Set
                mNETTolerance = Value
                If mAutoDefineSLiCScoreThresholds Then
                    InitializeSLiCScoreOptions(True)
                End If
            End Set
        End Property

        Public Property SLiCScoreMassPPMStDev As Double
            Get
                Return mSLiCScoreOptions.MassPPMStDev
            End Get
            Set
                If Value < 0 Then Value = 0
                mSLiCScoreOptions.MassPPMStDev = Value
            End Set
        End Property

        Public Property SLiCScoreNETStDev As Double
            Get
                Return mSLiCScoreOptions.NETStDev
            End Get
            Set
                If Value < 0 Then Value = 0
                mSLiCScoreOptions.NETStDev = Value
            End Set
        End Property

        Public Property SLiCScoreUseAMTNETStDev As Boolean
            Get
                Return mSLiCScoreOptions.UseAMTNETStDev
            End Get
            Set
                mSLiCScoreOptions.UseAMTNETStDev = Value
            End Set
        End Property

        Public Property SLiCScoreMaxSearchDistanceMultiplier As Single
            Get
                Return mSLiCScoreMaxSearchDistanceMultiplier
            End Get
            Set
                If Value < 1 Then Value = 1
                mSLiCScoreMaxSearchDistanceMultiplier = Value
                If mAutoDefineSLiCScoreThresholds Then
                    InitializeSLiCScoreOptions(True)
                End If
            End Set
        End Property
#End Region

        Public Sub DefinePeakMatchingTolerances(ByRef dblReferenceMass As Double)
            ' Thresholds are all half-widths; i.e. tolerance +- comparison value

            Dim MWTolPPMBroad As Double

            With mComputedSearchTolerances
                Select Case mMassTolType
                    Case MassToleranceConstants.PPM
                        .MWTolAbsFinal = PPMToMass(mMassTolerance, dblReferenceMass)
                        MWTolPPMBroad = mMassTolerance
                    Case MassToleranceConstants.Absolute
                        .MWTolAbsFinal = mMassTolerance
                        If dblReferenceMass > 0 Then
                            MWTolPPMBroad = MassToPPM(mMassTolerance, dblReferenceMass)
                        Else
                            MWTolPPMBroad = mSLiCScoreOptions.MassPPMStDev
                        End If
                    Case Else
                        Console.WriteLine("Programming error in DefinePeakMatchingTolerances; Unknown MassToleranceType: " & mMassTolType.ToString)
                End Select

                With mSLiCScoreOptions
                    If MWTolPPMBroad < .MassPPMStDev * .MaxSearchDistanceMultiplier * STDEV_SCALING_FACTOR Then
                        MWTolPPMBroad = .MassPPMStDev * .MaxSearchDistanceMultiplier * STDEV_SCALING_FACTOR
                    End If
                End With

                .NETTolBroad = mSLiCScoreOptions.NETStDev * mSLiCScoreOptions.MaxSearchDistanceMultiplier * STDEV_SCALING_FACTOR
                If .NETTolBroad < mNETTolerance Then
                    .NETTolBroad = mNETTolerance
                End If

                .NETTolFinal = mNETTolerance

                ' Convert from PPM to Absolute mass
                .MWTolAbsBroad = PPMToMass(MWTolPPMBroad, dblReferenceMass)

            End With

        End Sub

        Private Sub InitializeSLiCScoreOptions(blnComputeUsingSearchThresholds As Boolean)

            With mSLiCScoreOptions
                If blnComputeUsingSearchThresholds Then
                    ' Define the Mass StDev (in ppm) using the narrow mass tolerance divided by 2 = STDEV_SCALING_FACTOR
                    Select Case mMassTolType
                        Case MassToleranceConstants.Absolute
                            .MassPPMStDev = MassToPPM(mMassTolerance, 1000) / STDEV_SCALING_FACTOR
                        Case MassToleranceConstants.PPM
                            .MassPPMStDev = mMassTolerance / STDEV_SCALING_FACTOR
                        Case Else
                            ' Unknown type
                            .MassPPMStDev = 3
                    End Select

                    ' Define the Net StDev using the narrow NET tolerance divided by 2 = STDEV_SCALING_FACTOR
                    .NETStDev = mNETTolerance / STDEV_SCALING_FACTOR
                Else
                    .MassPPMStDev = 3
                    .NETStDev = 0.025
                End If

                .UseAMTNETStDev = False
                .MaxSearchDistanceMultiplier = mSLiCScoreMaxSearchDistanceMultiplier
                If .MaxSearchDistanceMultiplier < 1 Then
                    .MaxSearchDistanceMultiplier = DEFAULT_SLIC_MAX_SEARCH_DISTANCE_MULTIPLIER
                End If
            End With

        End Sub

        Private Sub InitializeLocalVariables()

            mAutoDefineSLiCScoreThresholds = True

            mMassTolType = MassToleranceConstants.PPM
            mMassTolerance = 5
            mNETTolerance = 0.05
            mSLiCScoreMaxSearchDistanceMultiplier = DEFAULT_SLIC_MAX_SEARCH_DISTANCE_MULTIPLIER

            InitializeSLiCScoreOptions(mAutoDefineSLiCScoreThresholds)

        End Sub

        Public Function MassToPPM(MassToConvert As Double, ReferenceMZ As Double) As Double
            ' Converts MassToConvert to ppm, which is dependent on ReferenceMZ
            Return MassToConvert * ONE_PART_PER_MILLION / ReferenceMZ
        End Function

        Public Function PPMToMass(PPMToConvert As Double, ReferenceMZ As Double) As Double
            ' Converts PPMToConvert to a mass value, which is dependent on ReferenceMZ
            Return PPMToConvert / ONE_PART_PER_MILLION * ReferenceMZ
        End Function

        Public Sub ResetToDefaults()
            InitializeLocalVariables()
        End Sub

    End Class
#End Region

End Class
