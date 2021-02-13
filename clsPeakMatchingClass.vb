Option Strict On

Imports System.Runtime.InteropServices

Public Class clsPeakMatchingClass

    Public Sub New()
        InitializeLocalVariables()
    End Sub

    Public Const DEFAULT_SLIC_MAX_SEARCH_DISTANCE_MULTIPLIER As Single = 2

    Private Const STDEV_SCALING_FACTOR As Integer = 2
    Private Const ONE_PART_PER_MILLION As Double = 1000000.0#

    Public Enum MessageTypeConstants
        Normal = 0
        ErrorMsg = 1
        Warning = 2
        Health = 3
    End Enum

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

    Friend Class PMFeatureInfoClass

        Protected Const MEMORY_RESERVE_CHUNK As Integer = 100000
        Protected mFeatureCount As Integer
        Protected mFeatures() As udtFeatureInfoType
        Protected mFeaturesArrayIsSorted As Boolean

        Protected mUseFeatureIDHashTable As Boolean
        Protected featureIDToRowIndex As Hashtable

        Public Event SortingList()

        Public Sub New()
            mUseFeatureIDHashTable = True                       ' Set this to False to conserve memory; you must call Clear() after changing this for it to take effect
            Me.Clear()
        End Sub

        Public Overridable Function Add(ByRef udtFeatureInfo As udtFeatureInfoType) As Boolean
            Return Add(udtFeatureInfo.FeatureID, udtFeatureInfo.FeatureName, udtFeatureInfo.Mass, udtFeatureInfo.NET)
        End Function

        Public Overridable Function Add(featureID As Integer, peptideName As String, peptideMass As Double, peptideNET As Single) As Boolean
            ' Returns True if the feature was added

            If ContainsFeature(featureID) Then
                Return False
            Else
                ' Add the feature
                If mFeatureCount >= mFeatures.Length Then
                    ReDim Preserve mFeatures(mFeatures.Length * 2 - 1)
                    'ReDim Preserve mFeatures(mFeatures.Length + MEMORY_RESERVE_CHUNK - 1)
                End If

                mFeatures(mFeatureCount).FeatureID = featureID
                mFeatures(mFeatureCount).FeatureName = peptideName
                mFeatures(mFeatureCount).Mass = peptideMass
                mFeatures(mFeatureCount).NET = peptideNET

                If mUseFeatureIDHashTable Then
                    featureIDToRowIndex.Add(featureID, mFeatureCount)
                End If

                mFeatureCount += 1
                mFeaturesArrayIsSorted = False

                ' If we get here, all went well
                Return True
            End If

        End Function

        Protected Function BinarySearchFindFeature(featureIDToFind As Integer) As Integer
            ' Looks through mFeatures() for featureIDToFind, returning the index of the item if found, or -1 if not found

            Dim midIndex As Integer
            Dim firstIndex = 0
            Dim lastIndex As Integer = mFeatureCount - 1

            Dim matchingRowIndex As Integer = -1

            If mFeatureCount <= 0 OrElse Not SortFeatures() Then
                Return matchingRowIndex
            End If

            Try
                midIndex = (firstIndex + lastIndex) \ 2            ' Note: Using Integer division
                If midIndex < firstIndex Then midIndex = firstIndex

                Do While firstIndex <= lastIndex And mFeatures(midIndex).FeatureID <> featureIDToFind
                    If featureIDToFind < mFeatures(midIndex).FeatureID Then
                        ' Search the lower half
                        lastIndex = midIndex - 1
                    ElseIf featureIDToFind > mFeatures(midIndex).FeatureID Then
                        ' Search the upper half
                        firstIndex = midIndex + 1
                    End If
                    ' Compute the new mid point
                    midIndex = (firstIndex + lastIndex) \ 2
                    If midIndex < firstIndex Then Exit Do
                Loop

                If midIndex >= firstIndex And midIndex <= lastIndex Then
                    If mFeatures(midIndex).FeatureID = featureIDToFind Then
                        matchingRowIndex = midIndex
                    End If
                End If

            Catch ex As Exception
                matchingRowIndex = -1
            End Try

            Return matchingRowIndex

        End Function

        Public Overridable Sub Clear()
            mFeatureCount = 0

            If mFeatures Is Nothing Then
                ReDim mFeatures(MEMORY_RESERVE_CHUNK - 1)
            End If
            mFeaturesArrayIsSorted = False

            If mUseFeatureIDHashTable Then
                If featureIDToRowIndex Is Nothing Then
                    featureIDToRowIndex = New Hashtable
                Else
                    featureIDToRowIndex.Clear()
                End If
            Else
                If Not featureIDToRowIndex Is Nothing Then
                    featureIDToRowIndex.Clear()
                    featureIDToRowIndex = Nothing
                End If
            End If
        End Sub

        Protected Function ContainsFeature(featureID As Integer) As Boolean
            Return ContainsFeature(featureID, 0)
        End Function

        Protected Function ContainsFeature(featureID As Integer, <Out> ByRef rowIndex As Integer) As Boolean
            ' Returns True if the features array contains the feature
            ' If found, returns the row index in rowIndex
            ' Note that the data will be sorted if necessary, which could lead to slow execution if this function is called repeatedly, while adding new data between calls

            If mUseFeatureIDHashTable Then
                If featureIDToRowIndex.Contains(featureID) Then
                    rowIndex = CInt(featureIDToRowIndex.Item(featureID))
                Else
                    rowIndex = -1
                End If
            Else
                ' Perform a binary search of mFeatures for featureID
                rowIndex = BinarySearchFindFeature(featureID)
            End If

            If rowIndex >= 0 Then
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

        Public Overridable Function GetFeatureInfoByFeatureID(featureID As Integer, ByRef udtFeatureInfo As udtFeatureInfoType) As Boolean
            ' Return the feature info for featureID


            Dim rowIndex As Integer

            Dim matchFound = Me.ContainsFeature(featureID, rowIndex)

            If matchFound Then
                udtFeatureInfo = mFeatures(rowIndex)
                Return True
            Else
                udtFeatureInfo.FeatureID = 0
                udtFeatureInfo.FeatureName = String.Empty
                udtFeatureInfo.Mass = 0
                udtFeatureInfo.NET = 0

                Return False
            End If
        End Function

        Public Overridable Function GetFeatureInfoByRowIndex(rowIndex As Integer, ByRef udtFeatureInfo As udtFeatureInfoType) As Boolean

            If rowIndex >= 0 And rowIndex < mFeatureCount Then
                udtFeatureInfo = mFeatures(rowIndex)
                Return True
            Else
                udtFeatureInfo.FeatureID = 0
                udtFeatureInfo.FeatureName = String.Empty
                udtFeatureInfo.Mass = 0
                udtFeatureInfo.NET = 0

                Return False
            End If

        End Function

        Public Overridable Function GetMassArrayByRowRange(rowIndexStart As Integer, rowIndexEnd As Integer) As Double()
            Dim masses() As Double
            Dim matchCount As Integer

            If rowIndexEnd < rowIndexStart Then
                ReDim masses(-1)
                Return masses
            End If

            ReDim masses(rowIndexEnd - rowIndexStart)

            Dim index As Integer

            Try
                If rowIndexEnd >= mFeatureCount Then
                    rowIndexEnd = mFeatureCount - 1
                End If

                matchCount = 0
                For index = rowIndexStart To rowIndexEnd
                    masses(matchCount) = mFeatures(index).Mass
                    matchCount += 1
                Next index

                If masses.Length > matchCount Then
                    ReDim Preserve masses(matchCount - 1)
                End If

            Catch ex As Exception
                Throw
            End Try

            Return masses
        End Function

        Public Overridable Function GetMassByRowIndex(rowIndex As Integer) As Double
            If rowIndex >= 0 And rowIndex < mFeatureCount Then
                Return mFeatures(rowIndex).Mass
            Else
                Return 0
            End If
        End Function

        Protected Overridable Function SortFeatures(Optional forceSort As Boolean = False) As Boolean

            If Not mFeaturesArrayIsSorted OrElse forceSort Then
                RaiseEvent SortingList()
                Try
                    Dim comparer As New FeatureInfoComparerClass
                    Array.Sort(mFeatures, 0, mFeatureCount, comparer)
                Catch ex As Exception
                    Throw
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

        Public Overloads Function Add(ByRef udtFeatureInfo As udtFeatureInfoType, peptideNETStDev As Single, peptideDiscriminantScore As Single) As Boolean
            Return Add(udtFeatureInfo.FeatureID, udtFeatureInfo.FeatureName, udtFeatureInfo.Mass, udtFeatureInfo.NET, peptideNETStDev, peptideDiscriminantScore)
        End Function

        Public Overloads Function Add(featureID As Integer, peptideName As String, peptideMass As Double, peptideNET As Single, peptideNETStDev As Single, peptideDiscriminantScore As Single) As Boolean

            ' Add the base feature info
            If Not MyBase.Add(featureID, peptideName, peptideMass, peptideNET) Then
                ' The feature already existed, and therefore wasn't added
                Return False
            Else

                ' Add the extended feature info
                If mExtendedInfo.Length < mFeatures.Length Then
                    ReDim Preserve mExtendedInfo(mFeatures.Length - 1)
                End If

                mExtendedInfo(mFeatureCount - 1).NETStDev = peptideNETStDev
                mExtendedInfo(mFeatureCount - 1).DiscriminantScore = peptideDiscriminantScore

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

        Public Overloads Function GetFeatureInfoByFeatureID(featureID As Integer, ByRef udtFeatureInfo As udtFeatureInfoType, ByRef netStDev As Single, ByRef discriminantScore As Single) As Boolean
            ' Return the feature info for featureID

            Dim rowIndex As Integer

            Dim matchFound = Me.ContainsFeature(featureID, rowIndex)

            If matchFound Then
                udtFeatureInfo = mFeatures(rowIndex)
                netStDev = mExtendedInfo(rowIndex).NETStDev
                discriminantScore = mExtendedInfo(rowIndex).DiscriminantScore
                Return True
            Else
                udtFeatureInfo.FeatureID = 0
                udtFeatureInfo.FeatureName = String.Empty
                udtFeatureInfo.Mass = 0
                udtFeatureInfo.NET = 0
                netStDev = 0
                discriminantScore = 0

                Return False
            End If
        End Function

        Public Overloads Function GetFeatureInfoByRowIndex(rowIndex As Integer, ByRef udtFeatureInfo As udtFeatureInfoType, ByRef netStDev As Single, ByRef discriminantScore As Single) As Boolean

            If rowIndex >= 0 And rowIndex < mFeatureCount Then
                udtFeatureInfo = mFeatures(rowIndex)
                netStDev = mExtendedInfo(rowIndex).NETStDev
                discriminantScore = mExtendedInfo(rowIndex).DiscriminantScore
                Return True
            Else
                udtFeatureInfo.FeatureID = 0
                udtFeatureInfo.FeatureName = String.Empty
                udtFeatureInfo.Mass = 0
                udtFeatureInfo.NET = 0
                netStDev = 0
                discriminantScore = 0

                Return False
            End If

        End Function

        Public Overridable Function GetNETStDevByRowIndex(rowIndex As Integer) As Single
            If rowIndex >= 0 And rowIndex < mFeatureCount Then
                Return mExtendedInfo(rowIndex).NETStDev
            Else
                Return 0
            End If
        End Function

    End Class

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

        Public Function AddMatch(featureID As Integer, ByRef matchResultInfo As udtPeakMatchingResultType) As Boolean
            Return AddMatch(featureID, matchResultInfo.MatchingID,
                            matchResultInfo.SLiCScore, matchResultInfo.DelSLiC,
                            matchResultInfo.MassErr, matchResultInfo.NETErr,
                            matchResultInfo.MultiAMTHitCount)
        End Function

        Public Function AddMatch(featureID As Integer, matchingID As Integer, slicScore As Double, delSLiC As Double, massErr As Double, netErr As Double, multiAMTHitCount As Integer) As Boolean

            ' Add the match
            If mPMResultsCount >= mPMResults.Length Then
                ReDim Preserve mPMResults(mPMResults.Length * 2 - 1)
            End If

            mPMResults(mPMResultsCount).FeatureID = featureID
            mPMResults(mPMResultsCount).Details.MatchingID = matchingID
            mPMResults(mPMResultsCount).Details.SLiCScore = slicScore
            mPMResults(mPMResultsCount).Details.DelSLiC = delSLiC
            mPMResults(mPMResultsCount).Details.MassErr = massErr
            mPMResults(mPMResultsCount).Details.NETErr = netErr
            mPMResults(mPMResultsCount).Details.MultiAMTHitCount = multiAMTHitCount

            mPMResultsCount += 1
            mPMResultsIsSorted = False

            ' If we get here, all went well
            Return True

        End Function

        Private Function BinarySearchPMResults(featureIDToFind As Integer) As Integer
            ' Looks through mPMResults() for featureIDToFind, returning the index of the item if found, or -1 if not found
            ' Since mPMResults() can contain multiple entries for a given Feature, this function returns the first entry found

            Dim midIndex As Integer
            Dim firstIndex = 0
            Dim lastIndex As Integer = mPMResultsCount - 1

            Dim matchingRowIndex As Integer = -1

            If mPMResultsCount <= 0 OrElse Not SortPMResults() Then
                Return matchingRowIndex
            End If

            Try
                midIndex = (firstIndex + lastIndex) \ 2            ' Note: Using Integer division
                If midIndex < firstIndex Then midIndex = firstIndex

                Do While firstIndex <= lastIndex And mPMResults(midIndex).FeatureID <> featureIDToFind
                    If featureIDToFind < mPMResults(midIndex).FeatureID Then
                        ' Search the lower half
                        lastIndex = midIndex - 1
                    ElseIf featureIDToFind > mPMResults(midIndex).FeatureID Then
                        ' Search the upper half
                        firstIndex = midIndex + 1
                    End If
                    ' Compute the new mid point
                    midIndex = (firstIndex + lastIndex) \ 2
                    If midIndex < firstIndex Then Exit Do
                Loop

                If midIndex >= firstIndex And midIndex <= lastIndex Then
                    If mPMResults(midIndex).FeatureID = featureIDToFind Then
                        matchingRowIndex = midIndex
                    End If
                End If

            Catch ex As Exception
                matchingRowIndex = -1
            End Try

            Return matchingRowIndex

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

        Public Function GetMatchInfoByFeatureID(featureID As Integer, ByRef matchResults() As udtPeakMatchingResultType, ByRef matchCount As Integer) As Boolean
            ' Returns all of the matches for the given feature ID row index
            ' Returns false if the feature has no matches
            ' Note that this function never shrinks matchResults; it only expands it if needed

            Dim matchesFound = False

            Try
                Dim index As Integer
                Dim indexFirst As Integer
                Dim indexLast As Integer

                If GetRowIndicesForFeatureID(featureID, indexFirst, indexLast) Then

                    matchCount = indexLast - indexFirst + 1
                    If matchResults Is Nothing OrElse matchCount > matchResults.Length Then
                        ReDim matchResults(matchCount - 1)
                    End If

                    For index = indexFirst To indexLast
                        matchResults(index - indexFirst) = mPMResults(index).Details
                    Next index
                    matchesFound = True

                End If

            Catch ex As Exception
                matchesFound = False
            End Try


            Return matchesFound

        End Function

        Public Function GetMatchInfoByRowIndex(rowIndex As Integer, ByRef featureID As Integer, ByRef matchResultInfo As udtPeakMatchingResultType) As Boolean
            ' Populates featureID and matchResultInfo with the peak matching results for the given row index

            Dim matchFound = False

            Try
                If rowIndex < mPMResultsCount Then
                    featureID = mPMResults(rowIndex).FeatureID
                    matchResultInfo = mPMResults(rowIndex).Details

                    matchFound = True
                End If
            Catch ex As Exception
                matchFound = False
            End Try

            Return matchFound

        End Function

        Private Function GetRowIndicesForFeatureID(featureID As Integer, ByRef indexFirst As Integer, ByRef indexLast As Integer) As Boolean
            ' Looks for featureID in mPMResults
            ' If found, returns the range of rows that contain matches for featureID


            ' Perform a binary search of mPMResults for featureID
            indexFirst = BinarySearchPMResults(featureID)

            If indexFirst >= 0 Then

                ' Match found; need to find all of the rows with featureID
                indexLast = indexFirst

                ' Step backward through mPMResults to find the first match for featureID
                Do While indexFirst > 0 AndAlso mPMResults(indexFirst - 1).FeatureID = featureID
                    indexFirst -= 1
                Loop

                ' Step forward through mPMResults to find the last match for featureID
                Do While indexLast < mPMResultsCount - 1 AndAlso mPMResults(indexLast + 1).FeatureID = featureID
                    indexLast += 1
                Loop

                Return True
            Else
                indexFirst = -1
                indexLast = -1
                Return False
            End If

        End Function

        Public ReadOnly Property MatchCountForFeatureID(featureID As Integer) As Integer
            Get
                Dim indexFirst As Integer
                Dim indexLast As Integer

                If GetRowIndicesForFeatureID(featureID, indexFirst, indexLast) Then
                    Return indexLast - indexFirst + 1
                Else
                    Return 0
                End If

            End Get
        End Property

        Private Function SortPMResults(Optional forceSort As Boolean = False) As Boolean

            If Not mPMResultsIsSorted OrElse forceSort Then
                RaiseEvent SortingList()
                Try
                    Dim comparer As New PeakMatchingResultsComparerClass
                    Array.Sort(mPMResults, 0, mPMResultsCount, comparer)
                Catch ex As Exception
                    Throw
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

    Private mMaxPeakMatchingResultsPerFeatureToSave As Integer
    Private mSearchModeOptions As udtSearchModeOptionsType

    Private mProgressDescription As String
    Private mProgressPercent As Single
    Private mAbortProcessing As Boolean

    Public Event ProgressContinues()
    Public Event LogEvent(Message As String, EventType As MessageTypeConstants)

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

    Public ReadOnly Property ProgressPct As Single
        Get
            Return mProgressPercent
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

    Public Sub AbortProcessingNow()
        mAbortProcessing = True
    End Sub

    Private Sub ComputeSLiCScores(ByRef udtFeatureToIdentify As udtFeatureInfoType, ByRef featureMatchResults As PMFeatureMatchResultsClass, ByRef rawMatches() As udtPeakMatchingRawMatchesType, ByRef comparisonFeatures As PMComparisonFeatureInfoClass, ByRef searchThresholds As clsSearchThresholds, udtComputedTolerances As clsSearchThresholds.udtSearchTolerancesType)

        Dim index As Integer
        Dim newMatchCount As Integer

        Dim massStDevPPM As Double
        Dim massStDevAbs As Double

        Dim netStDevCombined As Double
        Dim numeratorSum As Double

        Dim udtComparisonFeatureInfo = New udtFeatureInfoType()

        Dim message As String

        ' Compute the match scores (aka SLiC scores)

        massStDevPPM = searchThresholds.SLiCScoreMassPPMStDev
        If massStDevPPM <= 0 Then massStDevPPM = 3

        massStDevAbs = searchThresholds.PPMToMass(massStDevPPM, udtFeatureToIdentify.Mass)
        If massStDevAbs <= 0 Then
            message = "Assertion failed in ComputeSLiCScores; massStDevAbs is <= 0, which isn't allowed; will assume 0.003"
            Console.WriteLine(message)
            PostLogEntry(message, MessageTypeConstants.ErrorMsg)
            massStDevAbs = 0.003
        End If

        ' Compute the standardized squared distance and the numerator sum
        numeratorSum = 0
        For index = 0 To rawMatches.Length - 1

            If searchThresholds.SLiCScoreUseAMTNETStDev Then
                ' The NET StDev is computed by combining the default NETStDev value with the Comparison Features' specific NETStDev
                ' The combining is done by "adding in quadrature", which means to square each number, add together, and take the square root
                netStDevCombined = Math.Sqrt(searchThresholds.SLiCScoreNETStDev ^ 2 + comparisonFeatures.GetNETStDevByRowIndex(rawMatches(index).MatchingIDIndex) ^ 2)
            Else
                ' Simply use the default NETStDev value
                netStDevCombined = searchThresholds.SLiCScoreNETStDev
            End If

            If netStDevCombined <= 0 Then
                message = "Assertion failed in ComputeSLiCScores; netStDevCombined is <= 0, which isn't allowed; will assume 0.025"
                Console.WriteLine(message)
                PostLogEntry(message, MessageTypeConstants.ErrorMsg)
                netStDevCombined = 0.025
            End If

            rawMatches(index).StandardizedSquaredDistance = rawMatches(index).MassErr ^ 2 / massStDevAbs ^ 2 +
                                                            rawMatches(index).NETErr ^ 2 / netStDevCombined ^ 2

            rawMatches(index).SLiCScoreNumerator = (1 / (massStDevAbs * netStDevCombined)) * Math.Exp(-rawMatches(index).StandardizedSquaredDistance / 2)

            numeratorSum += rawMatches(index).SLiCScoreNumerator

        Next index

        ' Compute the match score for each match
        For index = 0 To rawMatches.Length - 1
            If numeratorSum > 0 Then
                rawMatches(index).SLiCScore = Math.Round(rawMatches(index).SLiCScoreNumerator / numeratorSum, 5)
            Else
                rawMatches(index).SLiCScore = 0
            End If
        Next index

        If rawMatches.Length > 1 Then
            ' Sort by SLiCScore descending
            Dim iPeakMatchingRawMatchesComparerClass As New PeakMatchingRawMatchesComparerClass
            Array.Sort(rawMatches, iPeakMatchingRawMatchesComparerClass)
            iPeakMatchingRawMatchesComparerClass = Nothing
        End If

        If rawMatches.Length > 0 Then

            ' Compute the DelSLiC value
            ' If there is only one match, then the DelSLiC value is 1
            ' If there is more than one match, then the highest scoring match gets a DelSLiC value,
            '  computed by subtracting the next lower scoring value from the highest scoring value; all
            '  other matches get a DelSLiC score of 0
            ' This allows one to quickly identify the features with a single match (DelSLiC = 1) or with a match
            '  distinct from other matches (DelSLiC > threshold)

            If rawMatches.Length > 1 Then
                rawMatches(0).DelSLiC = (rawMatches(0).SLiCScore - rawMatches(1).SLiCScore)

                For index = 1 To rawMatches.Length - 1
                    rawMatches(index).DelSLiC = 0
                Next index
            Else
                rawMatches(0).DelSLiC = 1
            End If

            ' Now filter the list using the tighter tolerances:
            ' Since we're shrinking the array, we can copy in place
            '
            ' When testing whether to keep the match or not, we're testing whether the match is in the ellipse bounded by MWTolAbsFinal and NETTolFinal
            ' Note that these are half-widths of the ellipse
            newMatchCount = 0
            For index = 0 To rawMatches.Length - 1
                If TestPointInEllipse(rawMatches(index).NETErr, rawMatches(index).MassErr, udtComputedTolerances.NETTolFinal, udtComputedTolerances.MWTolAbsFinal) Then
                    rawMatches(newMatchCount) = rawMatches(index)
                    newMatchCount += 1
                End If
            Next index

            If newMatchCount = 0 Then
                ReDim rawMatches(-1)
            ElseIf newMatchCount < rawMatches.Length Then
                ReDim Preserve rawMatches(newMatchCount - 1)
            End If

            ' Add new match results to featureMatchResults
            ' Record, at most, mMaxPeakMatchingResultsPerFeatureToSave entries
            For index = 0 To CInt(Math.Min(mMaxPeakMatchingResultsPerFeatureToSave, rawMatches.Length)) - 1
                comparisonFeatures.GetFeatureInfoByRowIndex(rawMatches(index).MatchingIDIndex, udtComparisonFeatureInfo)
                featureMatchResults.AddMatch(udtFeatureToIdentify.FeatureID, udtComparisonFeatureInfo.FeatureID,
                                             rawMatches(index).SLiCScore, rawMatches(index).DelSLiC,
                                             rawMatches(index).MassErr, rawMatches(index).NETErr,
                                             rawMatches.Length)
            Next index
        End If

    End Sub

    Friend Shared Function FillRangeSearchObject(ByRef rangeSearch As clsSearchRange, ByRef comparisonFeatures As PMComparisonFeatureInfoClass) As Boolean
        ' Initialize the range searching class

        Const LOAD_BLOCK_SIZE = 50000
        Dim index As Integer
        Dim comparisonFeatureCount As Integer
        Dim success As Boolean

        Try
            If rangeSearch Is Nothing Then
                rangeSearch = New clsSearchRange
            Else
                rangeSearch.ClearData()
            End If

            If comparisonFeatures.Count <= 0 Then
                ' No comparison features to search against
                success = False
            Else
                rangeSearch.InitializeDataFillDouble(comparisonFeatures.Count)

                ''For index = 0 To comparisonFeatures.Count - 1
                ''    rangeSearch.FillWithDataAddPoint(comparisonFeatures.GetMassByRowIndex(index))
                ''Next index

                comparisonFeatureCount = comparisonFeatures.Count
                index = 0
                Do While index < comparisonFeatureCount
                    rangeSearch.FillWithDataAddBlock(comparisonFeatures.GetMassArrayByRowRange(index, index + LOAD_BLOCK_SIZE - 1))
                    index += LOAD_BLOCK_SIZE
                Loop

                success = rangeSearch.FinalizeDataFill()
            End If

        Catch ex As Exception
            Throw
            success = False
        End Try

        Return success

    End Function

    Friend Function IdentifySequences(searchThresholds As clsSearchThresholds, ByRef featuresToIdentify As PMFeatureInfoClass, ByRef comparisonFeatures As PMComparisonFeatureInfoClass, ByRef featureMatchResults As PMFeatureMatchResultsClass, Optional ByRef rangeSearch As clsSearchRange = Nothing) As Boolean
        ' Returns True if success, False if the search is cancelled
        ' Will return true even if none of the features match any of the comparison features
        '
        ' If rangeSearch is Nothing or if rangeSearch contains a different number of entries than udtComparisonFeatures,
        '   then will auto-populate it; otherwise, assumes it is populated

        ' Note that featureMatchResults will only contain info on the features in featuresToIdentify that matched entries in comparisonFeatures

        Dim featureIndex As Integer
        Dim featureCount As Integer

        Dim matchIndex As Integer
        Dim comparisonFeaturesOriginalRowIndex As Integer

        Dim currentFeatureToIdentify = New udtFeatureInfoType()
        Dim currentComparisonFeature = New udtFeatureInfoType()

        Dim massTol As Double, netTol As Double
        Dim netDiff As Double

        Dim matchInd1, matchInd2 As Integer
        Dim udtComputedTolerances As clsSearchThresholds.udtSearchTolerancesType

        ' The following hold the matches using the broad search tolerances (if .UseMaxSearchDistanceMultiplierAndSLiCScore = True, otherwise, simply holds the matches)
        Dim rawMatchCount As Integer
        Dim rawMatches() As udtPeakMatchingRawMatchesType    ' Pointers into comparisonFeatures; list of peptides that match within both mass and NET tolerance

        Dim storeMatch As Boolean
        Dim success As Boolean

        Dim message As String

        If featureMatchResults Is Nothing Then
            ''If mUseSqlServerForMatchResults Then
            ''    featureMatchResults = New PMFeatureMatchResultsClass(mSqlServerConnectionString, mTableNameFeatureMatchResults)
            ''Else
            featureMatchResults = New PMFeatureMatchResultsClass
            ''End If
        Else
            ' Clear any existing results
            featureMatchResults.Clear()
        End If

        If rangeSearch Is Nothing OrElse rangeSearch.DataCount <> comparisonFeatures.Count Then
            success = FillRangeSearchObject(rangeSearch, comparisonFeatures)
        Else
            success = True
        End If

        If Not success Then Return False

        Try
            featureCount = featuresToIdentify.Count

            UpdateProgress("Finding matching peptides for given search thresholds", 0)
            mAbortProcessing = False

            PostLogEntry("IdentifySequences starting, total feature count = " & featureCount.ToString(), MessageTypeConstants.Normal)

            For featureIndex = 0 To featureCount - 1
                ' Use rangeSearch to search for matches to each peptide in comparisonFeatures

                If featuresToIdentify.GetFeatureInfoByRowIndex(featureIndex, currentFeatureToIdentify) Then
                    ' By Calling .ComputedSearchTolerances() with a mass, the tolerances will be auto re-computed
                    udtComputedTolerances = searchThresholds.ComputedSearchTolerances(currentFeatureToIdentify.Mass)

                    If mSearchModeOptions.UseMaxSearchDistanceMultiplierAndSLiCScore Then
                        massTol = udtComputedTolerances.MWTolAbsBroad
                        netTol = udtComputedTolerances.NETTolBroad
                    Else
                        massTol = udtComputedTolerances.MWTolAbsFinal
                        netTol = udtComputedTolerances.NETTolFinal
                    End If

                    matchInd1 = 0
                    matchInd2 = -1
                    If rangeSearch.FindValueRange(currentFeatureToIdentify.Mass, massTol, matchInd1, matchInd2) Then

                        rawMatchCount = 0
                        ReDim rawMatches(matchInd2 - matchInd1)

                        For matchIndex = matchInd1 To matchInd2
                            comparisonFeaturesOriginalRowIndex = rangeSearch.OriginalIndex(matchIndex)

                            If comparisonFeatures.GetFeatureInfoByRowIndex(comparisonFeaturesOriginalRowIndex, currentComparisonFeature) Then
                                netDiff = currentFeatureToIdentify.NET - currentComparisonFeature.NET
                                If Math.Abs(netDiff) <= netTol Then

                                    If mSearchModeOptions.UseMaxSearchDistanceMultiplierAndSLiCScore Then
                                        ' Store this match
                                        storeMatch = True
                                    Else
                                        ' The match is within a rectangle defined by udtComputedTolerances.MWTolAbsBroad and udtComputedTolerances.NETTolBroad
                                        If mSearchModeOptions.UseEllipseSearchRegion Then
                                            ' Only keep the match if it's within the ellipse defined by the search tolerances
                                            ' Note that the search tolerances we send to TestPointInEllipse should be half-widths (i.e. tolerance +- comparison value), not full widths
                                            storeMatch = TestPointInEllipse(netDiff, currentFeatureToIdentify.Mass - currentComparisonFeature.Mass, netTol, massTol)
                                        Else
                                            storeMatch = True
                                        End If
                                    End If

                                    If storeMatch Then
                                        rawMatches(rawMatchCount).MatchingIDIndex = comparisonFeaturesOriginalRowIndex
                                        rawMatches(rawMatchCount).SLiCScore = -1
                                        rawMatches(rawMatchCount).MassErr = currentFeatureToIdentify.Mass - currentComparisonFeature.Mass
                                        rawMatches(rawMatchCount).NETErr = netDiff

                                        rawMatchCount += 1
                                    End If
                                End If

                            End If

                        Next matchIndex

                        If rawMatchCount > 0 Then
                            ' Store the FeatureIDIndex in featureMatchResults
                            If rawMatchCount < rawMatches.Length Then
                                ' Shrink rawMatches
                                ReDim Preserve rawMatches(rawMatchCount - 1)
                            End If

                            ' Compute the SLiC Scores and store the results
                            ComputeSLiCScores(currentFeatureToIdentify, featureMatchResults, rawMatches, comparisonFeatures, searchThresholds, udtComputedTolerances)
                        End If

                    End If
                Else
                    message = "Programming error in IdentifySequences: Feature not found in featuresToIdentify using feature index: " & featureIndex.ToString()
                    Console.WriteLine(message)
                    PostLogEntry(message, MessageTypeConstants.ErrorMsg)
                End If

                If featureIndex Mod 100 = 0 Then
                    UpdateProgress(CSng(featureIndex / featureCount * 100))
                    If mAbortProcessing Then Exit For
                End If

                If featureIndex Mod 10000 = 0 AndAlso featureIndex > 0 Then
                    PostLogEntry("IdentifySequences, featureIndex = " & featureIndex.ToString(), MessageTypeConstants.Health)
                End If
            Next featureIndex

            UpdateProgress("IdentifySequences complete", 100)
            PostLogEntry("IdentifySequences complete", MessageTypeConstants.Normal)

            success = Not mAbortProcessing

        Catch ex As Exception
            success = False
        End Try

        Return success

    End Function

    Private Sub InitializeLocalVariables()
        mMaxPeakMatchingResultsPerFeatureToSave = 20

        mSearchModeOptions.UseMaxSearchDistanceMultiplierAndSLiCScore = True
        mSearchModeOptions.UseEllipseSearchRegion = True

        ''mUseSqlServerDBToCacheData = False
        ''mUseSqlServerForMatchResults = False
        ''mSqlServerConnectionString = SharedADONetFunctions.DEFAULT_CONNECTION_STRING_NO_PROVIDER
        ''mTableNameFeatureMatchResults = PMFeatureMatchResultsClass.DEFAULT_FEATURE_MATCH_RESULTS_TABLE_NAME
    End Sub

    Private Sub PostLogEntry(message As String, EntryType As MessageTypeConstants)
        RaiseEvent LogEvent(message, EntryType)
    End Sub

    Private Function TestPointInEllipse(pointX As Double, pointY As Double, xTol As Double, yTol As Double) As Boolean
        ' The equation for the points along the edge of an ellipse is x^2/a^2 + y^2/b^2 = 1 where a and b are
        ' the half-widths of the ellipse and x and y are the coordinates of each point on the ellipse's perimeter
        '
        ' This function takes x, y, a, and b as inputs and computes the result of this equation
        ' If the result is <= 1, then the point at x,y is inside the ellipse

        Try
            If pointX ^ 2 / xTol ^ 2 + pointY ^ 2 / yTol ^ 2 <= 1 Then
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            ' Error; return false
            Return False
        End Try

    End Function

    ''' <summary>
    ''' Update the progress
    ''' </summary>
    ''' <param name="progressPercent">Value between 0 and 100</param>
    Private Sub UpdateProgress(progressPercent As Single)
        mProgressPercent = progressPercent
        RaiseEvent ProgressContinues()
    End Sub

    ''' <summary>
    ''' Update the progress
    ''' </summary>
    ''' <param name="description">Progress description</param>
    ''' <param name="progressPercent">Value between 0 and 100</param>
    Private Sub UpdateProgress(description As String, progressPercent As Single)
        mProgressDescription = description
        mProgressPercent = progressPercent
        RaiseEvent ProgressContinues()
    End Sub

#Region "Peak Matching Raw Matches Sorting Class"

    Private Class PeakMatchingRawMatchesComparerClass
        Implements IComparer(Of udtPeakMatchingRawMatchesType)

        Public Function Compare(x As udtPeakMatchingRawMatchesType, y As udtPeakMatchingRawMatchesType) As Integer Implements IComparer(Of udtPeakMatchingRawMatchesType).Compare

            ' Sort by .SLiCScore descending, and by MatchingIDIndexOriginal Ascending

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

    Public Class clsSearchThresholds

        Public Sub New()
            InitializeLocalVariables()
        End Sub

        Public Enum MassToleranceConstants
            PPM = 0             ' parts per million
            Absolute = 1        ' absolute (Da)
        End Enum

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

        Private mMassTolerance As Double          ' Mass search tolerance, +- this value; TolType defines if this is PPM or Da
        Private mNETTolerance As Double           ' NET search tolerance, +- this value
        Private mSLiCScoreMaxSearchDistanceMultiplier As Single

        Private mSLiCScoreOptions As udtSLiCScoreOptionsType

        ' ReSharper disable once FieldCanBeMadeReadOnly.Local
        Private mComputedSearchTolerances As udtSearchTolerancesType = New udtSearchTolerancesType()

        Public Property AutoDefineSLiCScoreThresholds As Boolean

        Public ReadOnly Property ComputedSearchTolerances As udtSearchTolerancesType
            Get
                Return mComputedSearchTolerances
            End Get
        End Property

        Public ReadOnly Property ComputedSearchTolerances(referenceMass As Double) As udtSearchTolerancesType
            Get
                DefinePeakMatchingTolerances(referenceMass)
                Return mComputedSearchTolerances
            End Get
        End Property

        Public Property MassTolType As MassToleranceConstants

        Public Property MassTolerance As Double
            Get
                Return mMassTolerance
            End Get
            Set
                mMassTolerance = Value
                If AutoDefineSLiCScoreThresholds Then
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
                If AutoDefineSLiCScoreThresholds Then
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
                If AutoDefineSLiCScoreThresholds Then
                    InitializeSLiCScoreOptions(True)
                End If
            End Set
        End Property

        Public Sub DefinePeakMatchingTolerances(ByRef referenceMass As Double)
            ' Thresholds are all half-widths; i.e. tolerance +- comparison value

            Dim MWTolPPMBroad As Double

            Select Case MassTolType
                Case MassToleranceConstants.PPM
                    mComputedSearchTolerances.MWTolAbsFinal = PPMToMass(mMassTolerance, referenceMass)
                    MWTolPPMBroad = mMassTolerance
                Case MassToleranceConstants.Absolute
                    mComputedSearchTolerances.MWTolAbsFinal = mMassTolerance
                    If referenceMass > 0 Then
                        MWTolPPMBroad = MassToPPM(mMassTolerance, referenceMass)
                    Else
                        MWTolPPMBroad = mSLiCScoreOptions.MassPPMStDev
                    End If
                Case Else
                    Console.WriteLine("Programming error in DefinePeakMatchingTolerances; Unknown MassToleranceType: " & MassTolType.ToString())
            End Select

            If MWTolPPMBroad < mSLiCScoreOptions.MassPPMStDev * mSLiCScoreOptions.MaxSearchDistanceMultiplier * STDEV_SCALING_FACTOR Then
                MWTolPPMBroad = mSLiCScoreOptions.MassPPMStDev * mSLiCScoreOptions.MaxSearchDistanceMultiplier * STDEV_SCALING_FACTOR
            End If

            mComputedSearchTolerances.NETTolBroad = mSLiCScoreOptions.NETStDev * mSLiCScoreOptions.MaxSearchDistanceMultiplier * STDEV_SCALING_FACTOR
            If mComputedSearchTolerances.NETTolBroad < mNETTolerance Then
                mComputedSearchTolerances.NETTolBroad = mNETTolerance
            End If

            mComputedSearchTolerances.NETTolFinal = mNETTolerance

            ' Convert from PPM to Absolute mass
            mComputedSearchTolerances.MWTolAbsBroad = PPMToMass(MWTolPPMBroad, referenceMass)

        End Sub

        Private Sub InitializeSLiCScoreOptions(computeUsingSearchThresholds As Boolean)

            If computeUsingSearchThresholds Then
                ' Define the Mass StDev (in ppm) using the narrow mass tolerance divided by 2 = STDEV_SCALING_FACTOR
                Select Case MassTolType
                    Case MassToleranceConstants.Absolute
                        mSLiCScoreOptions.MassPPMStDev = MassToPPM(mMassTolerance, 1000) / STDEV_SCALING_FACTOR
                    Case MassToleranceConstants.PPM
                        mSLiCScoreOptions.MassPPMStDev = mMassTolerance / STDEV_SCALING_FACTOR
                    Case Else
                        ' Unknown type
                        mSLiCScoreOptions.MassPPMStDev = 3
                End Select

                ' Define the Net StDev using the narrow NET tolerance divided by 2 = STDEV_SCALING_FACTOR
                mSLiCScoreOptions.NETStDev = mNETTolerance / STDEV_SCALING_FACTOR
            Else
                mSLiCScoreOptions.MassPPMStDev = 3
                mSLiCScoreOptions.NETStDev = 0.025
            End If

            mSLiCScoreOptions.UseAMTNETStDev = False
            mSLiCScoreOptions.MaxSearchDistanceMultiplier = mSLiCScoreMaxSearchDistanceMultiplier
            If mSLiCScoreOptions.MaxSearchDistanceMultiplier < 1 Then
                mSLiCScoreOptions.MaxSearchDistanceMultiplier = DEFAULT_SLIC_MAX_SEARCH_DISTANCE_MULTIPLIER
            End If

        End Sub

        Private Sub InitializeLocalVariables()

            AutoDefineSLiCScoreThresholds = True

            MassTolType = MassToleranceConstants.PPM
            mMassTolerance = 5
            mNETTolerance = 0.05
            mSLiCScoreMaxSearchDistanceMultiplier = DEFAULT_SLIC_MAX_SEARCH_DISTANCE_MULTIPLIER

            InitializeSLiCScoreOptions(AutoDefineSLiCScoreThresholds)

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

End Class
