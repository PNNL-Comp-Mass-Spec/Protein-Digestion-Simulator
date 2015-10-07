Option Strict On

' Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in November 2004
' Copyright 2005, Battelle Memorial Institute.  All Rights Reserved.
'
' Last modified April 16, 2005

Friend Class clsProteinInfo
    Public Enum eCleavageStateConstants
        None = 0
        [Partial] = 1
        Full = 2
        Unknown = -1
    End Enum

    Protected Structure udtProteinInfoType
        Public Name As String
        Public ProteinID As Integer
    End Structure

    Protected Const MEMORY_RESERVE_CHUNK As Integer = 100000

    Protected mProteinCount As Integer
    Protected mProteins() As udtProteinInfoType
    Protected mProteinArrayIsSorted As Boolean

    Protected mMaxProteinIDUsed As Integer

    Protected WithEvents objProteinToPeptideMapping As clsProteinToPeptideMappingInfo

    Protected mUseProteinNameHashTable As Boolean
    Protected htProteinNameToRowIndex As Hashtable

    Public Event SortingList()
    Public Event SortingMappings()

    Public Sub New()
        mUseProteinNameHashTable = True                       ' Set this to False to conserve memory; you must call Clear() after changing this for it to take effec
        Me.Clear()
        objProteinToPeptideMapping = New clsProteinToPeptideMappingInfo
    End Sub

    Public Function Add(strProteinName As String, ByRef intNewProteinID As Integer) As Boolean
        ' Adds the protein by name, auto-assigning the ID value
        Return Add(strProteinName, intNewProteinID, True)
    End Function

    Public Function Add(strProteinName As String, ByRef intProteinID As Integer, blnAutoDefineProteinID As Boolean) As Boolean
        ' Adds the protein by name
        ' Uses the given protein ID if blnAutoDefineProteinID = False or auto-assigns the ID value if blnAutoDefineProteinID = True
        ' Returns ByRef the protein ID via intProteinID
        ' If the protein already exists, then returns True and populates intProteinID

        ' First, look for an existing entry in mProteins
        ' Once a protein is present in the table, its ID cannot be updated
        If GetProteinIDByProteinName(strProteinName, intProteinID) Then
            ' Protein already exists; intProteinID now contains the Protein ID
        Else
            ' Add to mProteins

            If mProteinCount >= mProteins.Length Then
                ReDim Preserve mProteins(mProteins.Length * 2 - 1)
                'ReDim Preserve mProteins(mProteins.Length + MEMORY_RESERVE_CHUNK - 1)
            End If

            If blnAutoDefineProteinID Then
                intProteinID = mMaxProteinIDUsed + 1
            End If

            With mProteins(mProteinCount)
                .Name = strProteinName
                .ProteinID = intProteinID
            End With

            If mUseProteinNameHashTable Then
                htProteinNameToRowIndex.Add(strProteinName, mProteinCount)
            End If

            mProteinCount += 1
            mProteinArrayIsSorted = False

            mMaxProteinIDUsed = Math.Max(mMaxProteinIDUsed, intProteinID)

        End If

        ' If we get here, all went well
        Return True

    End Function

    Public Function AddProteinToPeptideMapping(intProteinID As Integer, intPeptideID As Integer) As Boolean
        Return AddProteinToPeptideMapping(intProteinID, intPeptideID, eCleavageStateConstants.Unknown)
    End Function

    Public Function AddProteinToPeptideMapping(intProteinID As Integer, intPeptideID As Integer, eCleavageState As eCleavageStateConstants) As Boolean
        Return objProteinToPeptideMapping.AddProteinToPeptideMapping(intProteinID, intPeptideID, eCleavageState)
    End Function

    Public Function AddProteinToPeptideMapping(strProteinName As String, intPeptideID As Integer) As Boolean
        Return AddProteinToPeptideMapping(strProteinName, intPeptideID, eCleavageStateConstants.Unknown)
    End Function

    Public Function AddProteinToPeptideMapping(strProteinName As String, intPeptideID As Integer, eCleavageState As eCleavageStateConstants) As Boolean
        Return objProteinToPeptideMapping.AddProteinToPeptideMapping(Me, strProteinName, intPeptideID, eCleavageState)
    End Function

    Private Function BinarySearchFindProtein(strProteinName As String) As Integer
        ' Looks through mProteins() for strProteinName, returning the index of the item if found, or -1 if not found

        Dim intMidIndex As Integer
        Dim intFirstIndex As Integer = 0
        Dim intLastIndex As Integer = mProteinCount - 1

        Dim intMatchingRowIndex As Integer = -1

        If mProteinCount <= 0 OrElse Not SortProteins() Then
            Return intMatchingRowIndex
        End If

        Try
            intMidIndex = (intFirstIndex + intLastIndex) \ 2            ' Note: Using Integer division
            If intMidIndex < intFirstIndex Then intMidIndex = intFirstIndex

            Do While intFirstIndex <= intLastIndex And mProteins(intMidIndex).Name <> strProteinName
                If strProteinName < mProteins(intMidIndex).Name Then
                    ' Search the lower half
                    intLastIndex = intMidIndex - 1
                ElseIf strProteinName > mProteins(intMidIndex).Name Then
                    ' Search the upper half
                    intFirstIndex = intMidIndex + 1
                End If
                ' Compute the new mid point
                intMidIndex = (intFirstIndex + intLastIndex) \ 2
                If intMidIndex < intFirstIndex Then Exit Do
            Loop

            If intMidIndex >= intFirstIndex And intMidIndex <= intLastIndex Then
                If mProteins(intMidIndex).Name = strProteinName Then
                    intMatchingRowIndex = intMidIndex
                End If
            End If

        Catch ex As Exception
            intMatchingRowIndex = -1
        End Try

        Return intMatchingRowIndex

    End Function

    Public Overridable Sub Clear()
        mProteinCount = 0

        If mProteins Is Nothing Then
            ReDim mProteins(MEMORY_RESERVE_CHUNK - 1)
        End If
        mProteinArrayIsSorted = False
        mMaxProteinIDUsed = 0

        If mUseProteinNameHashTable Then
            If htProteinNameToRowIndex Is Nothing Then
                htProteinNameToRowIndex = New Hashtable
            Else
                htProteinNameToRowIndex.Clear()
            End If
        Else
            If Not htProteinNameToRowIndex Is Nothing Then
                htProteinNameToRowIndex.Clear()
                htProteinNameToRowIndex = Nothing
            End If
        End If
    End Sub

    Public ReadOnly Property Count() As Integer
        Get
            Return mProteinCount
        End Get
    End Property

    Public Function GetPeptideIDsMappedToProteinID(intProteinID As Integer) As Integer()
        Return objProteinToPeptideMapping.GetPeptideIDsMappedToProteinID(intProteinID)
    End Function

    Public Function GetPeptideCountForProteinByID(intProteinID As Integer) As Integer
        Return objProteinToPeptideMapping.PeptideCountForProteinID(intProteinID)
    End Function

    Public Function GetProteinIDsMappedToPeptideID(intPeptideID As Integer) As Integer()
        Return objProteinToPeptideMapping.GetProteinIDsMappedToPeptideID(intPeptideID)
    End Function

    Public Function GetProteinNameByProteinID(intProteinID As Integer, ByRef strProteinName As String) As Boolean
        ' Since mProteins is sorted by Protein Name, we must fully search the array to obtain the protein name for intProteinID

        Dim blnMatchFound As Boolean = False

        Dim intIndex As Integer

        strProteinName = String.Empty

        For intIndex = 0 To mProteinCount - 1
            If mProteins(intIndex).ProteinID = intProteinID Then
                strProteinName = mProteins(intIndex).Name
                blnMatchFound = True
                Exit For
            End If
        Next intIndex

        Return blnMatchFound

    End Function

    Public Function GetProteinIDByProteinName(strProteinName As String, ByRef intProteinID As Integer) As Boolean
        ' Returns True if the proteins array contains the protein
        ' If found, returns ProteinID in intProteinID
        ' Note that the data will be sorted if necessary, which could lead to slow execution if this function is called repeatedly, while adding new data between calls

        Dim intRowIndex As Integer

        If mUseProteinNameHashTable Then
            If htProteinNameToRowIndex.Contains(strProteinName) Then
                intRowIndex = CInt(htProteinNameToRowIndex.Item(strProteinName))
            Else
                intRowIndex = -1
            End If
        Else
            ' Perform a binary search of mFeatures for intFeatureID
            intRowIndex = BinarySearchFindProtein(strProteinName)
        End If

        If intRowIndex >= 0 Then
            intProteinID = mProteins(intRowIndex).ProteinID
            Return True
        Else
            intProteinID = -1
            Return False
        End If

    End Function

    Public Function GetProteinInfoByRowIndex(intRowIndex As Integer, ByRef intProteinID As Integer, ByRef strProteinName As String) As Boolean

        If intRowIndex >= 0 And intRowIndex < mProteinCount Then
            With mProteins(intRowIndex)
                strProteinName = .Name
                intProteinID = .ProteinID
            End With
            Return True
        Else
            strProteinName = String.Empty
            intProteinID = -1
            Return False
        End If

    End Function

    Private Function SortProteins(Optional blnForceSort As Boolean = False) As Boolean

        If Not mProteinArrayIsSorted OrElse blnForceSort Then
            RaiseEvent SortingList()
            Try
                Dim objComparer As New ProteinInfoComparerClass
                Array.Sort(mProteins, 0, mProteinCount, objComparer)
            Catch
                Throw
            End Try

            mProteinArrayIsSorted = True
        End If

        Return mProteinArrayIsSorted

    End Function

    Public Property UseProteinNameHashTable() As Boolean
        Get
            Return mUseProteinNameHashTable
        End Get
        Set(Value As Boolean)
            mUseProteinNameHashTable = Value
        End Set
    End Property

    Private Sub objProteinToPeptideMapping_SortingList() Handles objProteinToPeptideMapping.SortingList
        RaiseEvent SortingMappings()
    End Sub

    Private Class ProteinInfoComparerClass
        Implements IComparer(Of udtProteinInfoType)

        Public Function Compare(x As udtProteinInfoType, y As udtProteinInfoType) As Integer Implements IComparer(Of udtProteinInfoType).Compare

            ' Sort by Protein Name, ascending

            If x.Name > y.Name Then
                Return 1
            ElseIf x.Name < y.Name Then
                Return -1
            Else
                Return 0
            End If

        End Function

    End Class

#Region "Protein to Peptide Mapping Class"
    Protected Class clsProteinToPeptideMappingInfo

        Public Structure udtProteinToPeptideMappingType
            Public ProteinID As Integer
            Public PeptideID As Integer
            Public CleavageState As eCleavageStateConstants
        End Structure

        Protected Const MEMORY_RESERVE_CHUNK As Integer = 100000
        Protected mMappingCount As Integer
        Protected mMappings() As udtProteinToPeptideMappingType
        Protected mMappingArrayIsSorted As Boolean

        Public Event SortingList()

        Public Sub New()
            Me.Clear()
        End Sub

        Public Function AddProteinToPeptideMapping(intProteinID As Integer, intPeptideID As Integer) As Boolean
            Return AddProteinToPeptideMapping(intProteinID, intPeptideID, eCleavageStateConstants.Unknown)
        End Function

        Public Function AddProteinToPeptideMapping(intProteinID As Integer, intPeptideID As Integer, eCleavageState As eCleavageStateConstants) As Boolean

            ' Add the mapping
            If mMappingCount >= mMappings.Length Then
                ReDim Preserve mMappings(mMappings.Length * 2 - 1)
                'ReDim Preserve mMappings(mMappings.Length + MEMORY_RESERVE_CHUNK - 1)
            End If

            With mMappings(mMappingCount)
                .ProteinID = intProteinID
                .PeptideID = intPeptideID
                .CleavageState = eCleavageState
            End With

            mMappingCount += 1
            mMappingArrayIsSorted = False

            ' If we get here, all went well
            Return True

        End Function

        Public Function AddProteinToPeptideMapping(ByRef objProteinInfo As clsProteinInfo, strProteinName As String, intPeptideID As Integer) As Boolean
            Return AddProteinToPeptideMapping(objProteinInfo, strProteinName, intPeptideID, eCleavageStateConstants.Unknown)
        End Function

        Public Function AddProteinToPeptideMapping(ByRef objProteinInfo As clsProteinInfo, strProteinName As String, intPeptideID As Integer, eCleavageState As eCleavageStateConstants) As Boolean
            Dim intProteinID As Integer

            If Not objProteinInfo.GetProteinIDByProteinName(strProteinName, intProteinID) Then
                ' Need to add the protein
                If Not objProteinInfo.Add(strProteinName, intProteinID) Then
                    Return False
                End If
            End If

            Return AddProteinToPeptideMapping(intProteinID, intPeptideID, eCleavageState)

        End Function

        Private Function BinarySearchFindProteinMapping(intProteinIDToFind As Integer) As Integer
            ' Looks through mMappings() for intProteinIDToFind, returning the index of the item if found, or -1 if not found
            ' Since mMappings() can contain multiple entries for a given Protein, this function returns the first entry found

            Dim intMidIndex As Integer
            Dim intFirstIndex As Integer = 0
            Dim intLastIndex As Integer = mMappingCount - 1

            Dim intMatchingRowIndex As Integer = -1

            If mMappingCount <= 0 OrElse Not SortMappings() Then
                Return intMatchingRowIndex
            End If

            Try
                intMidIndex = (intFirstIndex + intLastIndex) \ 2            ' Note: Using Integer division
                If intMidIndex < intFirstIndex Then intMidIndex = intFirstIndex

                Do While intFirstIndex <= intLastIndex And mMappings(intMidIndex).ProteinID <> intProteinIDToFind
                    If intProteinIDToFind < mMappings(intMidIndex).ProteinID Then
                        ' Search the lower half
                        intLastIndex = intMidIndex - 1
                    ElseIf intProteinIDToFind > mMappings(intMidIndex).ProteinID Then
                        ' Search the upper half
                        intFirstIndex = intMidIndex + 1
                    End If
                    ' Compute the new mid point
                    intMidIndex = (intFirstIndex + intLastIndex) \ 2
                    If intMidIndex < intFirstIndex Then Exit Do
                Loop

                If intMidIndex >= intFirstIndex And intMidIndex <= intLastIndex Then
                    If mMappings(intMidIndex).ProteinID = intProteinIDToFind Then
                        intMatchingRowIndex = intMidIndex
                    End If
                End If

            Catch ex As Exception
                intMatchingRowIndex = -1
            End Try

            Return intMatchingRowIndex

        End Function

        Public Overridable Sub Clear()
            mMappingCount = 0

            If mMappings Is Nothing Then
                ReDim mMappings(MEMORY_RESERVE_CHUNK - 1)
            End If
            mMappingArrayIsSorted = False

        End Sub

        Protected Function ContainsMapping(intProteinID As Integer, intPeptideID As Integer) As Boolean
            ' Returns True if the data table contains the mapping of intProteinID to intPeptideID
            ' Note that the data will be sorted if necessary, which could lead to slow execution if this function is called repeatedly, while adding new data between calls

            Dim intIndex As Integer
            Dim intIndexFirst As Integer
            Dim intIndexLast As Integer

            If GetRowIndicesForProteinID(intProteinID, intIndexFirst, intIndexLast) Then
                For intIndex = intIndexFirst To intIndexLast
                    If mMappings(intIndex).PeptideID = intPeptideID Then
                        Return True
                    End If
                Next intIndex
            End If

            ' If we get here, then the mapping wasn't found
            Return False

        End Function

        Public ReadOnly Property Count() As Integer
            Get
                Return mMappingCount
            End Get
        End Property

        Public Function GetPeptideIDsMappedToProteinID(intProteinID As Integer) As Integer()
            ' Returns all of the peptides for the given protein ID

            Dim intMatchingIDs() As Integer

            Dim intIndex As Integer
            Dim intIndexFirst As Integer
            Dim intIndexLast As Integer

            If GetRowIndicesForProteinID(intProteinID, intIndexFirst, intIndexLast) Then

                ReDim intMatchingIDs(intIndexLast - intIndexFirst)

                For intIndex = intIndexFirst To intIndexLast
                    intMatchingIDs(intIndex - intIndexFirst) = mMappings(intIndex).PeptideID
                Next intIndex

            Else
                ReDim intMatchingIDs(-1)
            End If

            Return intMatchingIDs

        End Function

        Public Function GetProteinIDsMappedToPeptideID(intPeptideID As Integer) As Integer()
            ' Since mMappings is sorted by Protein ID, we must fully search the array to obtain the ProteinIDs for intPeptideID

            Dim ARRAY_ALLOCATION_CHUNK As Integer = 10
            Dim intMatchingIDs() As Integer
            Dim intMatchCount As Integer

            Dim intindex As Integer

            ReDim intMatchingIDs(ARRAY_ALLOCATION_CHUNK - 1)
            intMatchCount = 0

            For intindex = 0 To mMappingCount - 1
                If mMappings(intindex).PeptideID = intPeptideID Then
                    If intMatchCount >= intMatchingIDs.Length Then
                        ReDim Preserve intMatchingIDs(intMatchingIDs.Length * 2 - 1)
                        'ReDim Preserve intMatchingIDs(intMatchingIDs.Length + ARRAY_ALLOCATION_CHUNK - 1)
                    End If

                    intMatchingIDs(intMatchCount) = mMappings(intindex).ProteinID
                    intMatchCount += 1
                End If
            Next intindex

            If intMatchingIDs.Length > intMatchCount Then
                ReDim Preserve intMatchingIDs(intMatchCount - 1)
            End If

            Return intMatchingIDs

        End Function

        Private Function GetRowIndicesForProteinID(intProteinID As Integer, ByRef intIndexFirst As Integer, ByRef intIndexLast As Integer) As Boolean
            ' Looks for intProteinID in mMappings
            ' If found, returns the range of rows that contain matches for intProteinID

            ' Perform a binary search of mMappings for intProteinID
            intIndexFirst = BinarySearchFindProteinMapping(intProteinID)

            If intIndexFirst >= 0 Then

                ' Match found; need to find all of the rows with intProteinID
                intIndexLast = intIndexFirst

                ' Step backward through mMappings to find the first match for intProteinID
                Do While intIndexFirst > 0 AndAlso mMappings(intIndexFirst - 1).ProteinID = intProteinID
                    intIndexFirst -= 1
                Loop

                ' Step forward through mMappings to find the last match for intProteinID
                Do While intIndexLast < mMappingCount - 1 AndAlso mMappings(intIndexLast + 1).ProteinID = intProteinID
                    intIndexLast += 1
                Loop

                Return True
            Else
                intIndexFirst = -1
                intIndexLast = -1
                Return False
            End If

        End Function

        Public ReadOnly Property PeptideCountForProteinID(intProteinID As Integer) As Integer
            Get
                Dim intIndexFirst As Integer
                Dim intIndexLast As Integer

                If GetRowIndicesForProteinID(intProteinID, intIndexFirst, intIndexLast) Then
                    Return intIndexLast - intIndexFirst + 1
                Else
                    Return 0
                End If

            End Get

        End Property

        Private Function SortMappings(Optional blnForceSort As Boolean = False) As Boolean

            If Not mMappingArrayIsSorted OrElse blnForceSort Then
                RaiseEvent SortingList()
                Try
                    Dim objComparer As New ProteinToPeptideMappingsComparerClass
                    Array.Sort(mMappings, 0, mMappingCount, objComparer)
                Catch
                    Throw
                End Try

                mMappingArrayIsSorted = True
            End If

            Return mMappingArrayIsSorted

        End Function

        Private Class ProteinToPeptideMappingsComparerClass
            Implements IComparer(Of udtProteinToPeptideMappingType)

            Public Function Compare(x As udtProteinToPeptideMappingType, y As udtProteinToPeptideMappingType) As Integer Implements IComparer(Of udtProteinToPeptideMappingType).Compare

                ' Sort by ProteinID, then by PeptideID

                If x.ProteinID > y.ProteinID Then
                    Return 1
                ElseIf x.ProteinID < y.ProteinID Then
                    Return -1
                Else
                    If x.PeptideID > y.PeptideID Then
                        Return 1
                    ElseIf x.PeptideID < y.PeptideID Then
                        Return -1
                    Else
                        Return 0
                    End If
                End If

            End Function

        End Class
    End Class
#End Region

End Class

