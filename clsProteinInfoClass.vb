Option Strict On

Imports System.Runtime.InteropServices

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

    Protected WithEvents mProteinToPeptideMapping As clsProteinToPeptideMappingInfo

    Protected mUseProteinNameHashTable As Boolean
    Protected mProteinNameToRowIndex As Hashtable

    Public Event SortingList()
    Public Event SortingMappings()

    Public Sub New()
        mUseProteinNameHashTable = True                       ' Set this to False to conserve memory; you must call Clear() after changing this for it to take effect
        Me.Clear()
        mProteinToPeptideMapping = New clsProteinToPeptideMappingInfo
    End Sub

    Public Function Add(proteinName As String, ByRef newProteinID As Integer) As Boolean
        ' Adds the protein by name, auto-assigning the ID value
        Return Add(proteinName, newProteinID, True)
    End Function

    Public Function Add(proteinName As String, ByRef proteinID As Integer, autoDefineProteinID As Boolean) As Boolean
        ' Adds the protein by name
        ' Uses the given protein ID if autoDefineProteinID = False or auto-assigns the ID value if autoDefineProteinID = True
        ' Returns ByRef the protein ID via proteinID
        ' If the protein already exists, then returns True and populates proteinID

        ' First, look for an existing entry in mProteins
        ' Once a protein is present in the table, its ID cannot be updated
        If GetProteinIDByProteinName(proteinName, proteinID) Then
            ' Protein already exists; proteinID now contains the Protein ID
        Else
            ' Add to mProteins

            If mProteinCount >= mProteins.Length Then
                ReDim Preserve mProteins(mProteins.Length * 2 - 1)
                'ReDim Preserve mProteins(mProteins.Length + MEMORY_RESERVE_CHUNK - 1)
            End If

            If autoDefineProteinID Then
                proteinID = mMaxProteinIDUsed + 1
            End If

            With mProteins(mProteinCount)
                .Name = proteinName
                .ProteinID = proteinID
            End With

            If mUseProteinNameHashTable Then
                mProteinNameToRowIndex.Add(proteinName, mProteinCount)
            End If

            mProteinCount += 1
            mProteinArrayIsSorted = False

            mMaxProteinIDUsed = Math.Max(mMaxProteinIDUsed, proteinID)

        End If

        ' If we get here, all went well
        Return True

    End Function

    Public Function AddProteinToPeptideMapping(proteinID As Integer, peptideID As Integer) As Boolean
        Return AddProteinToPeptideMapping(proteinID, peptideID, eCleavageStateConstants.Unknown)
    End Function

    Public Function AddProteinToPeptideMapping(proteinID As Integer, peptideID As Integer, eCleavageState As eCleavageStateConstants) As Boolean
        Return mProteinToPeptideMapping.AddProteinToPeptideMapping(proteinID, peptideID, eCleavageState)
    End Function

    Public Function AddProteinToPeptideMapping(proteinName As String, peptideID As Integer) As Boolean
        Return AddProteinToPeptideMapping(proteinName, peptideID, eCleavageStateConstants.Unknown)
    End Function

    Public Function AddProteinToPeptideMapping(proteinName As String, peptideID As Integer, eCleavageState As eCleavageStateConstants) As Boolean
        Return mProteinToPeptideMapping.AddProteinToPeptideMapping(Me, proteinName, peptideID, eCleavageState)
    End Function

    Private Function BinarySearchFindProtein(proteinName As String) As Integer
        ' Looks through mProteins() for proteinName, returning the index of the item if found, or -1 if not found

        Dim midIndex As Integer
        Dim firstIndex = 0
        Dim lastIndex = mProteinCount - 1

        Dim matchingRowIndex As Integer = -1

        If mProteinCount <= 0 OrElse Not SortProteins() Then
            Return matchingRowIndex
        End If

        Try
            midIndex = (firstIndex + lastIndex) \ 2            ' Note: Using Integer division
            If midIndex < firstIndex Then midIndex = firstIndex

            Do While firstIndex <= lastIndex And mProteins(midIndex).Name <> proteinName
                If proteinName < mProteins(midIndex).Name Then
                    ' Search the lower half
                    lastIndex = midIndex - 1
                ElseIf proteinName > mProteins(midIndex).Name Then
                    ' Search the upper half
                    firstIndex = midIndex + 1
                End If
                ' Compute the new mid point
                midIndex = (firstIndex + lastIndex) \ 2
                If midIndex < firstIndex Then Exit Do
            Loop

            If midIndex >= firstIndex And midIndex <= lastIndex Then
                If mProteins(midIndex).Name = proteinName Then
                    matchingRowIndex = midIndex
                End If
            End If

        Catch ex As Exception
            matchingRowIndex = -1
        End Try

        Return matchingRowIndex

    End Function

    Public Overridable Sub Clear()
        mProteinCount = 0

        If mProteins Is Nothing Then
            ReDim mProteins(MEMORY_RESERVE_CHUNK - 1)
        End If
        mProteinArrayIsSorted = False
        mMaxProteinIDUsed = 0

        If mUseProteinNameHashTable Then
            If mProteinNameToRowIndex Is Nothing Then
                mProteinNameToRowIndex = New Hashtable
            Else
                mProteinNameToRowIndex.Clear()
            End If
        Else
            If Not mProteinNameToRowIndex Is Nothing Then
                mProteinNameToRowIndex.Clear()
                mProteinNameToRowIndex = Nothing
            End If
        End If
    End Sub

    Public ReadOnly Property Count As Integer
        Get
            Return mProteinCount
        End Get
    End Property

    Public Function GetPeptideIDsMappedToProteinID(proteinID As Integer) As Integer()
        Return mProteinToPeptideMapping.GetPeptideIDsMappedToProteinID(proteinID)
    End Function

    Public Function GetPeptideCountForProteinByID(proteinID As Integer) As Integer
        Return mProteinToPeptideMapping.PeptideCountForProteinID(proteinID)
    End Function

    Public Function GetProteinIDsMappedToPeptideID(peptideID As Integer) As Integer()
        Return mProteinToPeptideMapping.GetProteinIDsMappedToPeptideID(peptideID)
    End Function

    Public Function GetProteinNameByProteinID(proteinID As Integer, ByRef proteinName As String) As Boolean
        ' Since mProteins is sorted by Protein Name, we must fully search the array to obtain the protein name for proteinID

        Dim matchFound = False

        Dim index As Integer

        proteinName = String.Empty

        For index = 0 To mProteinCount - 1
            If mProteins(index).ProteinID = proteinID Then
                proteinName = mProteins(index).Name
                matchFound = True
                Exit For
            End If
        Next index

        Return matchFound

    End Function

    Public Function GetProteinIDByProteinName(proteinName As String, ByRef proteinID As Integer) As Boolean
        ' Returns True if the proteins array contains the protein
        ' If found, returns ProteinID in proteinID
        ' Note that the data will be sorted if necessary, which could lead to slow execution if this function is called repeatedly, while adding new data between calls

        Dim rowIndex As Integer

        If mUseProteinNameHashTable Then
            If mProteinNameToRowIndex.Contains(proteinName) Then
                rowIndex = CInt(mProteinNameToRowIndex.Item(proteinName))
            Else
                rowIndex = -1
            End If
        Else
            ' Perform a binary search of mFeatures for featureID
            rowIndex = BinarySearchFindProtein(proteinName)
        End If

        If rowIndex >= 0 Then
            proteinID = mProteins(rowIndex).ProteinID
            Return True
        Else
            proteinID = -1
            Return False
        End If

    End Function

    Public Function GetProteinInfoByRowIndex(rowIndex As Integer, <Out> ByRef proteinID As Integer, <Out> ByRef proteinName As String) As Boolean

        If rowIndex >= 0 And rowIndex < mProteinCount Then
            With mProteins(rowIndex)
                proteinName = .Name
                proteinID = .ProteinID
            End With
            Return True
        Else
            proteinName = String.Empty
            proteinID = -1
            Return False
        End If

    End Function

    Private Function SortProteins(Optional forceSort As Boolean = False) As Boolean

        If Not mProteinArrayIsSorted OrElse forceSort Then
            RaiseEvent SortingList()
            Try
                Dim comparer As New ProteinInfoComparerClass
                Array.Sort(mProteins, 0, mProteinCount, comparer)
            Catch
                Throw
            End Try

            mProteinArrayIsSorted = True
        End If

        Return mProteinArrayIsSorted

    End Function

    Public Property UseProteinNameHashTable As Boolean
        Get
            Return mUseProteinNameHashTable
        End Get
        Set
            mUseProteinNameHashTable = Value
        End Set
    End Property

    Private Sub mProteinToPeptideMapping_SortingList() Handles mProteinToPeptideMapping.SortingList
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

        Public Function AddProteinToPeptideMapping(proteinID As Integer, peptideID As Integer) As Boolean
            Return AddProteinToPeptideMapping(proteinID, peptideID, eCleavageStateConstants.Unknown)
        End Function

        Public Function AddProteinToPeptideMapping(proteinID As Integer, peptideID As Integer, eCleavageState As eCleavageStateConstants) As Boolean

            ' Add the mapping
            If mMappingCount >= mMappings.Length Then
                ReDim Preserve mMappings(mMappings.Length * 2 - 1)
                'ReDim Preserve mMappings(mMappings.Length + MEMORY_RESERVE_CHUNK - 1)
            End If

            With mMappings(mMappingCount)
                .ProteinID = proteinID
                .PeptideID = peptideID
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

            If Not proteinInfo.GetProteinIDByProteinName(proteinName, proteinID) Then
                ' Need to add the protein
                If Not proteinInfo.Add(proteinName, proteinID) Then
                    Return False
                End If
            End If

            Return AddProteinToPeptideMapping(proteinID, peptideID, eCleavageState)

        End Function

        Private Function BinarySearchFindProteinMapping(proteinIDToFind As Integer) As Integer
            ' Looks through mMappings() for proteinIDToFind, returning the index of the item if found, or -1 if not found
            ' Since mMappings() can contain multiple entries for a given Protein, this function returns the first entry found

            Dim midIndex As Integer
            Dim firstIndex = 0
            Dim lastIndex = mMappingCount - 1

            Dim matchingRowIndex = -1

            If mMappingCount <= 0 OrElse Not SortMappings() Then
                Return matchingRowIndex
            End If

            Try
                midIndex = (firstIndex + lastIndex) \ 2            ' Note: Using Integer division
                If midIndex < firstIndex Then midIndex = firstIndex

                Do While firstIndex <= lastIndex And mMappings(midIndex).ProteinID <> proteinIDToFind
                    If proteinIDToFind < mMappings(midIndex).ProteinID Then
                        ' Search the lower half
                        lastIndex = midIndex - 1
                    ElseIf proteinIDToFind > mMappings(midIndex).ProteinID Then
                        ' Search the upper half
                        firstIndex = midIndex + 1
                    End If
                    ' Compute the new mid point
                    midIndex = (firstIndex + lastIndex) \ 2
                    If midIndex < firstIndex Then Exit Do
                Loop

                If midIndex >= firstIndex And midIndex <= lastIndex Then
                    If mMappings(midIndex).ProteinID = proteinIDToFind Then
                        matchingRowIndex = midIndex
                    End If
                End If

            Catch ex As Exception
                matchingRowIndex = -1
            End Try

            Return matchingRowIndex

        End Function

        Public Overridable Sub Clear()
            mMappingCount = 0

            If mMappings Is Nothing Then
                ReDim mMappings(MEMORY_RESERVE_CHUNK - 1)
            End If
            mMappingArrayIsSorted = False

        End Sub

        Protected Function ContainsMapping(proteinID As Integer, peptideID As Integer) As Boolean
            ' Returns True if the data table contains the mapping of proteinID to peptideID
            ' Note that the data will be sorted if necessary, which could lead to slow execution if this function is called repeatedly, while adding new data between calls

            Dim index As Integer
            Dim indexFirst As Integer
            Dim indexLast As Integer

            If GetRowIndicesForProteinID(proteinID, indexFirst, indexLast) Then
                For index = indexFirst To indexLast
                    If mMappings(index).PeptideID = peptideID Then
                        Return True
                    End If
                Next index
            End If

            ' If we get here, then the mapping wasn't found
            Return False

        End Function

        Public ReadOnly Property Count As Integer
            Get
                Return mMappingCount
            End Get
        End Property

        Public Function GetPeptideIDsMappedToProteinID(proteinID As Integer) As Integer()
            ' Returns all of the peptides for the given protein ID

            Dim matchingIDs() As Integer

            Dim index As Integer
            Dim indexFirst As Integer
            Dim indexLast As Integer

            If GetRowIndicesForProteinID(proteinID, indexFirst, indexLast) Then

                ReDim matchingIDs(indexLast - indexFirst)

                For index = indexFirst To indexLast
                    matchingIDs(index - indexFirst) = mMappings(index).PeptideID
                Next index

            Else
                ReDim matchingIDs(-1)
            End If

            Return matchingIDs

        End Function

        Public Function GetProteinIDsMappedToPeptideID(peptideID As Integer) As Integer()
            ' Since mMappings is sorted by Protein ID, we must fully search the array to obtain the ProteinIDs for peptideID

            Dim ARRAY_ALLOCATION_CHUNK = 10
            Dim matchingIDs() As Integer
            Dim matchCount As Integer

            ReDim matchingIDs(ARRAY_ALLOCATION_CHUNK - 1)
            matchCount = 0

            For index = 0 To mMappingCount - 1
                If mMappings(index).PeptideID = peptideID Then
                    If matchCount >= matchingIDs.Length Then
                        ReDim Preserve matchingIDs(matchingIDs.Length * 2 - 1)
                        'ReDim Preserve matchingIDs(matchingIDs.Length + ARRAY_ALLOCATION_CHUNK - 1)
                    End If

                    matchingIDs(matchCount) = mMappings(index).ProteinID
                    matchCount += 1
                End If
            Next index

            If matchingIDs.Length > matchCount Then
                ReDim Preserve matchingIDs(matchCount - 1)
            End If

            Return matchingIDs

        End Function

        Private Function GetRowIndicesForProteinID(proteinID As Integer, ByRef indexFirst As Integer, ByRef indexLast As Integer) As Boolean
            ' Looks for proteinID in mMappings
            ' If found, returns the range of rows that contain matches for proteinID

            ' Perform a binary search of mMappings for proteinID
            indexFirst = BinarySearchFindProteinMapping(proteinID)

            If indexFirst >= 0 Then

                ' Match found; need to find all of the rows with proteinID
                indexLast = indexFirst

                ' Step backward through mMappings to find the first match for proteinID
                Do While indexFirst > 0 AndAlso mMappings(indexFirst - 1).ProteinID = proteinID
                    indexFirst -= 1
                Loop

                ' Step forward through mMappings to find the last match for proteinID
                Do While indexLast < mMappingCount - 1 AndAlso mMappings(indexLast + 1).ProteinID = proteinID
                    indexLast += 1
                Loop

                Return True
            Else
                indexFirst = -1
                indexLast = -1
                Return False
            End If

        End Function

        Public ReadOnly Property PeptideCountForProteinID(proteinID As Integer) As Integer
            Get
                Dim indexFirst As Integer
                Dim indexLast As Integer

                If GetRowIndicesForProteinID(proteinID, indexFirst, indexLast) Then
                    Return indexLast - indexFirst + 1
                Else
                    Return 0
                End If

            End Get

        End Property

        Private Function SortMappings(Optional forceSort As Boolean = False) As Boolean

            If Not mMappingArrayIsSorted OrElse forceSort Then
                RaiseEvent SortingList()
                Try
                    Dim comparer As New ProteinToPeptideMappingsComparerClass
                    Array.Sort(mMappings, 0, mMappingCount, comparer)
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

