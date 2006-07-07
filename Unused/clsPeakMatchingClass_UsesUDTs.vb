Public Class clsPeakMatchingClass

    Public Sub New()
        InitializeLocalVariables()
    End Sub

#Region "Constants and Enums"
    Private Const ONE_PART_PER_MILLION As Double = 1000000.0#

    Public Enum eCleavageStateConstants
        None = 0
        Partial = 1
        Full = 2
        Unknown = -1
    End Enum
#End Region

#Region "Structures"
    Public Structure udtPeptideToProteinMappingInfoType
        Public ProteinID As Integer
        Public CleavageState As eCleavageStateConstants
    End Structure

    Public Structure udtFeatureInfoType
        Public FeatureID As Integer                                 ' Each feature should have a unique ID
        Public FeatureName As String                                ' Optional
        Public Mass As Double
        Public NET As Single
    End Structure

    Public Structure udtComparisonFeatureExtendedInfoType
        Public NetStDev As Single
        Public DiscriminantScore As Single
        Public ProteinIDs() As udtPeptideToProteinMappingInfoType   ' List of proteins that contain this peptide (if applicable)
    End Structure

    Public Structure udtComparisonFeatureInfoType
        Public ComparisonFeatureID As Integer                       ' Each comparison feature should have a unique ID; for peptides, this is typically the Sequence_ID or Mass_Tag_ID
        Public FeatureName As String                                ' Optional
        Public Mass As Double
        Public NET As Single
        Public PMTInfo As udtComparisonFeatureExtendedInfoType      ' Extended info that is applicable to PMT tags
    End Structure

    Private Structure udtPeakMatchingRawMatchesType
        Public MatchingIDIndexOriginal As Integer                   ' Pointer into comparison features array
        Public StandardizedSquaredDistance As Double
        Public SLiCScoreNumerator As Double
        Public SLiCScore As Double                     ' SLiC Score (Spatially Localized Confidence score)
        Public DelSLiC As Double                       ' Similar to DelCN, difference in SLiC score between top match and match with score value one less than this score
        Public MassErr As Double                       ' Observed difference (error) between comparison mass and feature mass (in Da)
        Public NETErr As Double                        ' Observed difference (error) between comparison NET and feature NET
    End Structure

    Public Structure udtPeakMatchingMatchesType
        Public MatchingIDIndex As Integer           ' Pointer into comparison features array
        Public SLiCScore As Double                  ' SLiC Score (Spatially Localized Confidence score)
        Public DelSLiC As Double                    ' Similar to DelCN, difference in SLiC score between top match and match with score value one less than this score
        Public MassErr As Double                    ' Observed difference (error) between comparison mass and feature mass (in Da)
        Public NETErr As Double                     ' Observed difference (error) between comparison NET and feature NET
        Public MultiAMTHitCount As Integer          ' The number of Unique mass tag hits for each UMC; only applies to AMT's
    End Structure

    Public Structure udtPeptideMatchResultType
        Public FeatureIDIndex As Integer            ' Pointer into array of features that was searched (udtFeatureInfoType)
        Public MatchCount As Integer
        Public MatchStats() As udtPeakMatchingMatchesType
    End Structure

#End Region

#Region "Classwide Variables"
    Private mMaxPeakMatchingResultsPerFeatureToSave As Integer
#End Region

#Region "Processing Options Interface Functions"

    Public Property MaxPeakMatchingResultsPerFeatureToSave() As Integer
        Get
            Return mMaxPeakMatchingResultsPerFeatureToSave
        End Get
        Set(ByVal Value As Integer)
            If Value < 0 Then Value = 0
            mMaxPeakMatchingResultsPerFeatureToSave = Value
        End Set
    End Property

#End Region

    Private Sub ComputeSLiCScores(ByRef udtFeatureToIdentify As udtFeatureInfoType, ByRef udtFeatureMatchResults As udtPeptideMatchResultType, ByRef udtRawMatches() As udtPeakMatchingRawMatchesType, ByRef udtComparisonFeatures() As udtComparisonFeatureInfoType, ByRef objSearchThresholds As clsSearchThresholds, ByVal udtComputedTolerances As clsSearchThresholds.udtSearchTolerancesType)

        Dim intIndex As Integer
        Dim intNewMatchCount As Integer

        Dim dblMassStDevPPM As Double
        Dim dblMassStDevAbs As Double

        Dim dblNETStDevCombined As Double
        Dim dblNumeratorSum As Double

        ' Compute the match scores (aka SLiC scores)

        dblMassStDevPPM = objSearchThresholds.SLiCScoreMassPPMStDev
        If dblMassStDevPPM <= 0 Then dblMassStDevPPM = 3

        dblMassStDevAbs = objSearchThresholds.PPMToMass(dblMassStDevPPM, udtFeatureToIdentify.Mass)
        If dblMassStDevAbs <= 0 Then
            Debug.Assert(False, "dblMassStDevAbs was <= 0, which isn't allowed")
            dblMassStDevAbs = 0.003
        End If

        ' Compute the standarized squared distance and the numerator sum
        dblNumeratorSum = 0
        For intIndex = 0 To udtRawMatches.Length - 1

            If objSearchThresholds.SLiCScoreUseAMTNETStDev Then
                ' The NET StDev is computed by combining the default NETStDev value with the Comparison Features's specific NETStDev
                ' The combining is done by "adding in quadrature", which means to square each number, add together, and take the square root

                dblNETStDevCombined = Math.Sqrt(objSearchThresholds.SLiCScoreNETStDev ^ 2 + udtComparisonFeatures(udtRawMatches(intIndex).MatchingIDIndexOriginal).PMTInfo.NetStDev ^ 2)
            Else
                ' Simply use the default NETStDev value
                dblNETStDevCombined = objSearchThresholds.SLiCScoreNETStDev
            End If

            If dblNETStDevCombined <= 0 Then
                Debug.Assert(False, "dblNETStDevCombined was <= 0, which isn't allowed")
                dblNETStDevCombined = 0.025
            End If

            With udtRawMatches(intIndex)
                .StandardizedSquaredDistance = .MassErr ^ 2 / dblMassStDevAbs ^ 2 + .NETErr ^ 2 / dblNETStDevCombined ^ 2

                .SLiCScoreNumerator = (1 / (dblMassStDevAbs * dblNETStDevCombined)) * Math.Exp(-.StandardizedSquaredDistance / 2)

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


        ' Now filter the list using the tighter tolerances:
        ' Since we're shrinking the array, we can copy in place
        intNewMatchCount = 0
        For intIndex = 0 To udtRawMatches.Length - 1
            If Math.Abs(udtRawMatches(intIndex).MassErr) <= udtComputedTolerances.MWTolAbsFinal And Math.Abs(udtRawMatches(intIndex).NETErr) <= udtComputedTolerances.NETTolFinal Then
                udtRawMatches(intNewMatchCount) = udtRawMatches(intIndex)
                intNewMatchCount += 1
            End If
        Next intIndex

        If intNewMatchCount = 0 Then
            ReDim udtRawMatches(-1)
        ElseIf intNewMatchCount < udtRawMatches.Length Then
            ReDim Preserve udtRawMatches(intNewMatchCount - 1)
        End If

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


            ' Copy results into udtFeatureMatchResults
            With udtFeatureMatchResults
                .MatchCount = udtRawMatches.Length

                If udtRawMatches.Length > mMaxPeakMatchingResultsPerFeatureToSave Then
                    ReDim Preserve udtRawMatches(mMaxPeakMatchingResultsPerFeatureToSave - 1)
                End If

                ReDim .MatchStats(udtRawMatches.Length - 1)

                For intIndex = 0 To udtRawMatches.Length - 1
                    With .MatchStats(intIndex)
                        .MatchingIDIndex = udtRawMatches(intIndex).MatchingIDIndexOriginal
                        .SLiCScore = udtRawMatches(intIndex).SLiCScore
                        .DelSLiC = udtRawMatches(intIndex).DelSLiC
                        .MassErr = udtRawMatches(intIndex).MassErr
                        .NETErr = udtRawMatches(intIndex).NETErr
                        .MultiAMTHitCount = udtRawMatches.Length
                    End With
                Next intIndex

            End With

        Else
            ' No matches
            With udtFeatureMatchResults
                .MatchCount = 0
                ReDim .MatchStats(-1)
            End With
        End If


    End Sub

    Public Shared Function FillRangeSearchObject(ByRef objRangeSearch As clsSearchRange, ByRef udtComparisonFeatures() As udtComparisonFeatureInfoType) As Boolean
        ' Initialize the range searching class

        Dim dblMasses() As Double
        Dim intIndex As Integer
        Dim blnSuccess As Boolean

        Try
            If Not objRangeSearch Is Nothing Then
                objRangeSearch.ClearData()
            Else
                objRangeSearch = New clsSearchRange
            End If

            ReDim dblMasses(udtComparisonFeatures.Length - 1)
            For intIndex = 0 To udtComparisonFeatures.Length - 1
                dblMasses(intIndex) = udtComparisonFeatures(intIndex).Mass
            Next intIndex

            If udtComparisonFeatures.Length <= 0 Then
                ' No comparison features to search against
                blnSuccess = False
            ElseIf Not objRangeSearch.FillWithData(dblMasses) Then
                Debug.Assert(False, "Error calling objRangeSearch.FillWithData")
                blnSuccess = False
            Else
                blnSuccess = True
            End If

        Catch ex As Exception
            blnSuccess = False
        End Try

        Return blnSuccess

    End Function

    Public Function IdentifySequences(ByVal objSearchThresholds As clsSearchThresholds, ByRef udtFeaturesToIdentify() As udtFeatureInfoType, ByRef udtComparisonFeatures() As udtComparisonFeatureInfoType, ByRef udtFeatureMatchResults() As udtPeptideMatchResultType, Optional ByRef objProgressForm As ProgressFormNET.frmProgress = Nothing, Optional ByRef objRangeSearch As clsSearchRange = Nothing) As Boolean
        ' Returns True if success, False if the search is cancelled
        ' Will return true even if none of the features match any of the comparison features
        '
        ' If objRangeSearch is Nothing or if objRangeSearch contains a different number of entries than udtComparisonfeatures,
        '   then will auto-populate it; otherwise, assumes it is populated

        ' Note that udtFeatureMatchResults will only contain info on the features in udtFeaturesToIdentify() that matched entries in udtComparisonFeatures()

        Const MATCH_RESULTS_ALLOCATION_CHUNK As Integer = 100

        Dim intFeatureIndex As Integer
        Dim intMatchIndex As Integer
        Dim intComparisonFeaturesOriginalIndex As Integer

        Dim dblNetDiff As Double

        Dim MatchInd1, MatchInd2 As Integer
        Dim udtComputedTolerances As clsSearchThresholds.udtSearchTolerancesType

        ' The following hold the matches using the broad search tolerances
        Dim intRawMatchCount As Integer
        Dim udtRawMatches() As udtPeakMatchingRawMatchesType    ' Pointers into udtComparisonFeatures(); list of peptides that match within both mass and NET tolerance

        Dim intMatchResultsCount As Integer

        Dim blnUsingExistingProgressForm As Boolean
        Dim blnSuccess As Boolean







        If objRangeSearch Is Nothing OrElse objRangeSearch.DataCount <> udtComparisonFeatures.Length Then
            blnSuccess = FillRangeSearchObject(objRangeSearch, udtComparisonFeatures)
        Else
            blnSuccess = True
        End If

        If Not blnSuccess Then Return False

        Try
            ' We reserve space in udtFeatureMatchResults in chunks of MATCH_RESULTS_ALLOCATION_CHUNK
            intMatchResultsCount = 0
            ReDim udtFeatureMatchResults(MATCH_RESULTS_ALLOCATION_CHUNK)

            If objProgressForm Is Nothing Then
                objProgressForm = New ProgressFormNET.frmProgress
                objProgressForm.MoveToBottomCenter()
                objProgressForm.InitializeProgressForm("Finding matching peptides for given search thresholds", 0, udtFeaturesToIdentify.Length, True, False)
                blnUsingExistingProgressForm = False
            Else
                objProgressForm.InitializeSubtask("Finding matching peptides for given search thresholds", 0, udtFeaturesToIdentify.Length)
                blnUsingExistingProgressForm = True
            End If
            objProgressForm.Visible = True
            Windows.Forms.Application.DoEvents()

            ' ToDo: This all needs to be validated
            For intFeatureIndex = 0 To udtFeaturesToIdentify.Length - 1
                ' Use objRangeSearch to search for matches to each peptide in udtComparisonFeatures

                ' By Calling .ComputedSearchTolerances() with a mass, the tolerances will be auto re-computed
                udtComputedTolerances = objSearchThresholds.ComputedSearchTolerances(udtFeaturesToIdentify(intFeatureIndex).Mass)

                MatchInd1 = 0
                MatchInd2 = -1
                If objRangeSearch.FindValueRange(udtFeaturesToIdentify(intFeatureIndex).Mass, udtComputedTolerances.MWTolAbsBroad, MatchInd1, MatchInd2) Then

                    intRawMatchCount = 0
                    ReDim udtRawMatches(MatchInd2 - MatchInd1)

                    For intMatchIndex = MatchInd1 To MatchInd2
                        intComparisonFeaturesOriginalIndex = objRangeSearch.OriginalIndex(intMatchIndex)

                        dblNetDiff = udtFeaturesToIdentify(intFeatureIndex).NET - udtComparisonFeatures(intComparisonFeaturesOriginalIndex).NET
                        If Math.Abs(dblNetDiff) <= udtComputedTolerances.NETTolBroad Then
                            udtRawMatches(intRawMatchCount).MatchingIDIndexOriginal = intComparisonFeaturesOriginalIndex
                            udtRawMatches(intRawMatchCount).SLiCScore = -1
                            udtRawMatches(intRawMatchCount).MassErr = udtFeaturesToIdentify(intFeatureIndex).Mass - udtComparisonFeatures(intComparisonFeaturesOriginalIndex).Mass
                            udtRawMatches(intRawMatchCount).NETErr = dblNetDiff

                            intRawMatchCount += 1
                        End If

                    Next intMatchIndex

                    ' Possibly increase the length of udtFeatureMatchResults
                    If intMatchResultsCount >= udtFeatureMatchResults.Length Then
                        ReDim Preserve udtFeatureMatchResults(udtFeatureMatchResults.Length + MATCH_RESULTS_ALLOCATION_CHUNK)
                    End If

                    ' Store the FeatureIDIndex in udtFeatureMatchResults
                    udtFeatureMatchResults(intMatchResultsCount).FeatureIDIndex = intFeatureIndex

                    If intRawMatchCount <= 0 Then
                        With udtFeatureMatchResults(intMatchResultsCount)
                            .MatchCount = 0
                            ReDim .MatchStats(-1)
                        End With
                    Else
                        If intRawMatchCount < udtRawMatches.Length Then
                            ' Shrink udtRawMatches
                            ReDim Preserve udtRawMatches(intRawMatchCount - 1)
                        End If

                        ' Compute the SLiC Scores and store the results
                        ComputeSLiCScores(udtFeaturesToIdentify(intFeatureIndex), udtFeatureMatchResults(intMatchResultsCount), udtRawMatches, udtComparisonFeatures, objSearchThresholds, udtComputedTolerances)
                    End If

                    intMatchResultsCount += 1
                End If

                If intFeatureIndex Mod 100 = 0 Then
                    If blnUsingExistingProgressForm Then
                        objProgressForm.UpdateSubtaskProgressBar(intFeatureIndex)
                    Else
                        objProgressForm.UpdateProgressBar(intFeatureIndex)
                    End If
                    Windows.Forms.Application.DoEvents()

                    If objProgressForm.KeyPressAbortProcess Then Exit For
                End If
            Next intFeatureIndex

            blnSuccess = Not objProgressForm.KeyPressAbortProcess

        Catch ex As Exception
            blnSuccess = False
        End Try

        ' Shrink udtMatchResults to the correct length
        If intMatchResultsCount > 0 Then
            ReDim Preserve udtFeatureMatchResults(intMatchResultsCount - 1)
        Else
            ReDim udtFeatureMatchResults(-1)
        End If

        If Not blnUsingExistingProgressForm Then
            objProgressForm.HideForm()
            objProgressForm.Close()
            objProgressForm = Nothing
        End If

        Return blnSuccess

    End Function

    Private Sub InitializeLocalVariables()
        mMaxPeakMatchingResultsPerFeatureToSave = 20
    End Sub

#Region "Peak Matching Raw Matches Sorting Class"

    Private Class PeakMatchingRawMatchesComparerClass
        Implements IComparer

        Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements System.Collections.IComparer.Compare

            Dim udtRawMatch, udtRawMatch2 As udtPeakMatchingRawMatchesType

            udtRawMatch = CType(x, udtPeakMatchingRawMatchesType)
            udtRawMatch2 = CType(y, udtPeakMatchingRawMatchesType)

            ' Sort on .SLiCScore descending, and MatchingIDIndexOriginal Ascencing

            If udtRawMatch.SLiCScore > udtRawMatch2.SLiCScore Then
                Return -1
            ElseIf udtRawMatch.SLiCScore < udtRawMatch2.SLiCScore Then
                Return 1
            Else
                If udtRawMatch.MatchingIDIndexOriginal > udtRawMatch2.MatchingIDIndexOriginal Then
                    Return 1
                ElseIf udtRawMatch.MatchingIDIndexOriginal < udtRawMatch2.MatchingIDIndexOriginal Then
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
            Public MaxSearchDistanceMultiplier As Integer  ' Default 4
        End Structure

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

        Private mSLiCScoreOptions As udtSLiCScoreOptionsType

        Private mAutoDefineSLiCScoreThresholds As Boolean

        Private mComputedSearchTolerances As udtSearchTolerancesType

#End Region

#Region "Processing Options Interface Functions"

        Public Property AutoDefineSLiCScoreThresholds() As Boolean
            Get
                Return mAutoDefineSLiCScoreThresholds
            End Get
            Set(ByVal Value As Boolean)
                mAutoDefineSLiCScoreThresholds = Value
            End Set
        End Property

        Public ReadOnly Property ComputedSearchTolerances() As udtSearchTolerancesType
            Get
                Return mComputedSearchTolerances
            End Get
        End Property

        Public ReadOnly Property ComputedSearchTolerances(ByVal dblReferenceMass As Double) As udtSearchTolerancesType
            Get
                DefinePeakMatchingTolerances(dblReferenceMass)
                Return mComputedSearchTolerances
            End Get
        End Property

        Public Property MassTolType() As MassToleranceConstants
            Get
                Return mMassTolType
            End Get
            Set(ByVal Value As MassToleranceConstants)
                mMassTolType = Value
            End Set
        End Property

        Public Property MassTolerance() As Double
            Get
                Return mMassTolerance
            End Get
            Set(ByVal Value As Double)
                mMassTolerance = Value
                If mAutoDefineSLiCScoreThresholds Then
                    InitializeSLiCScoreOptions(True)
                End If
            End Set
        End Property

        Public Property NETTolerance() As Double
            Get
                Return mNETTolerance
            End Get
            Set(ByVal Value As Double)
                mNETTolerance = Value
                If mAutoDefineSLiCScoreThresholds Then
                    InitializeSLiCScoreOptions(True)
                End If
            End Set
        End Property

         Public Property SLiCScoreMassPPMStDev() As Double
            Get
                Return mSLiCScoreOptions.MassPPMStDev
            End Get
            Set(ByVal Value As Double)
                If Value < 0 Then Value = 0
                mSLiCScoreOptions.MassPPMStDev = Value
            End Set
        End Property

        Public Property SLiCScoreNETStDev() As Double
            Get
                Return mSLiCScoreOptions.NETStDev
            End Get
            Set(ByVal Value As Double)
                If Value < 0 Then Value = 0
                mSLiCScoreOptions.NETStDev = Value
            End Set
        End Property

        Public Property SLiCScoreUseAMTNETStDev() As Boolean
            Get
                Return mSLiCScoreOptions.UseAMTNETStDev
            End Get
            Set(ByVal Value As Boolean)
                mSLiCScoreOptions.UseAMTNETStDev = Value
            End Set
        End Property

        Public Property SLiCScoreMaxSearchDistanceMultiplier() As Integer
            Get
                Return mSLiCScoreOptions.MaxSearchDistanceMultiplier
            End Get
            Set(ByVal Value As Integer)
                If Value < 1 Then Value = 1
                mSLiCScoreOptions.MaxSearchDistanceMultiplier = Value
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
                        Debug.Assert(False, "Unknown MassToleranceType: " & mMassTolType.ToString)
                End Select

                With mSLiCScoreOptions
                    If MWTolPPMBroad < .MassPPMStDev * .MaxSearchDistanceMultiplier Then
                        MWTolPPMBroad = .MassPPMStDev * .MaxSearchDistanceMultiplier
                    End If
                End With

                .NETTolBroad = mSLiCScoreOptions.NETStDev * mSLiCScoreOptions.MaxSearchDistanceMultiplier
                If .NETTolBroad < mNETTolerance Then
                    .NETTolBroad = mNETTolerance
                End If

                .NETTolFinal = mNETTolerance

                ' Convert from PPM to Absolute mass
                .MWTolAbsBroad = PPMToMass(MWTolPPMBroad, dblReferenceMass)

            End With

        End Sub

        Private Sub InitializeSLiCScoreOptions(ByVal blnComputeUsingSearchThresholds As Boolean)

            With mSLiCScoreOptions
                If blnComputeUsingSearchThresholds Then
                    Select Case mMassTolType
                        Case MassToleranceConstants.Absolute
                            .MassPPMStDev = MassToPPM(mMassTolerance, 1000) / 2
                        Case MassToleranceConstants.PPM
                            .MassPPMStDev = mMassTolerance / 2
                        Case Else
                            ' Unknown type
                            .MassPPMStDev = 3
                    End Select

                    .NETStDev = mNETTolerance / 2

                Else
                    .MassPPMStDev = 3
                    .NETStDev = 0.025
                End If

                .UseAMTNETStDev = False
                .MaxSearchDistanceMultiplier = 4
            End With

        End Sub

        Private Sub InitializeLocalVariables()

            mAutoDefineSLiCScoreThresholds = True

            mMassTolType = MassToleranceConstants.PPM
            mMassTolerance = 6
            mNETTolerance = 0.05

            InitializeSLiCScoreOptions(mAutoDefineSLiCScoreThresholds)

        End Sub

        Public Function MassToPPM(ByVal MassToConvert As Double, ByVal ReferenceMZ As Double) As Double
            ' Converts MassToConvert to ppm, which is dependent on ReferenceMZ
            Return MassToConvert * ONE_PART_PER_MILLION / ReferenceMZ
        End Function

        Public Function PPMToMass(ByVal PPMToConvert As Double, ByVal ReferenceMZ As Double) As Double
            ' Converts PPMToConvert to a mass value, which is dependent on ReferenceMZ
            Return PPMToConvert / ONE_PART_PER_MILLION * ReferenceMZ
        End Function

        Public Sub ResetToDefaults()
            InitializeLocalVariables()
        End Sub

    End Class
#End Region

End Class
