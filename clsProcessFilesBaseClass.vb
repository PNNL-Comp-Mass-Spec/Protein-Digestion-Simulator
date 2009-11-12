Option Strict On

' This class can be used as a base class for classes that process a file or files, and create
' new output files in an output folder.  Note that this class contains simple error codes that
' can be set from any derived classes.  The derived classes can also set their own local error codes
'
' Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
' Copyright 2005, Battelle Memorial Institute.  All Rights Reserved.
' Started November 9, 2003

Public MustInherit Class clsProcessFilesBaseClass

    Public Sub New()
        mFileDate = "June 22, 2009"
        mErrorCode = eProcessFilesErrorCodes.NoError
        mProgressStepDescription = String.Empty

        mOutputFolderPath = String.Empty
        mLogFolderPath = String.Empty
        mLogFilePath = String.Empty
    End Sub

#Region "Constants and Enums"
    Public Enum eProcessFilesErrorCodes
        NoError = 0
        InvalidInputFilePath = 1
        InvalidOutputFolderPath = 2
        ParameterFileNotFound = 4
        InvalidParameterFile = 8
        FilePathError = 16
        LocalizedError = 32
        UnspecifiedError = -1
    End Enum

    Protected Enum eMessageTypeConstants
        Normal = 0
        ErrorMsg = 1
        Warning = 2
    End Enum

    '' Copy the following to any derived classes
    ''Public Enum eDerivedClassErrorCodes
    ''    NoError = 0
    ''    UnspecifiedError = -1
    ''End Enum
#End Region

#Region "Classwide Variables"
    ''Private mLocalErrorCode As eDerivedClassErrorCodes

    ''Public ReadOnly Property LocalErrorCode() As eDerivedClassErrorCodes
    ''    Get
    ''        Return mLocalErrorCode
    ''    End Get
    ''End Property

    Private mShowMessages As Boolean
    Private mErrorCode As eProcessFilesErrorCodes

    Protected mFileDate As String
    Protected mAbortProcessing As Boolean

    Protected mLogMessagesToFile As Boolean
    Protected mLogFilePath As String
    Protected mLogFile As System.IO.StreamWriter

    ' This variable is updated when CleanupFilePaths() is called
    Protected mOutputFolderPath As String
    Protected mLogFolderPath As String          ' If blank, then mOutputFolderPath will be used; if mOutputFolderPath is also blank, then the log is created in the same folder as the executing assembly

    Public Event ProgressReset()
    Public Event ProgressChanged(ByVal taskDescription As String, ByVal percentComplete As Single)     ' PercentComplete ranges from 0 to 100, but can contain decimal percentage values
    Public Event ProgressComplete()

    Protected mProgressStepDescription As String
    Protected mProgressPercentComplete As Single        ' Ranges from 0 to 100, but can contain decimal percentage values

#End Region

#Region "Interface Functions"
    Public Property AbortProcessing() As Boolean
        Get
            Return mAbortProcessing
        End Get
        Set(ByVal Value As Boolean)
            mAbortProcessing = Value
        End Set
    End Property

    Public ReadOnly Property ErrorCode() As eProcessFilesErrorCodes
        Get
            Return mErrorCode
        End Get
    End Property

    Public ReadOnly Property FileVersion() As String
        Get
            FileVersion = GetVersionForExecutingAssembly()
        End Get
    End Property

    Public ReadOnly Property FileDate() As String
        Get
            FileDate = mFileDate
        End Get
    End Property

    Public ReadOnly Property LogFilePath() As String
        Get
            Return mLogFilePath
        End Get
    End Property

    Public Property LogFolderPath() As String
        Get
            Return mLogFolderPath
        End Get
        Set(ByVal value As String)
            mLogFolderPath = value
        End Set
    End Property

    Public Property LogMessagesToFile() As Boolean
        Get
            Return mLogMessagesToFile
        End Get
        Set(ByVal value As Boolean)
            mLogMessagesToFile = value
        End Set
    End Property

    Public Overridable ReadOnly Property ProgressStepDescription() As String
        Get
            Return mProgressStepDescription
        End Get
    End Property

    ' ProgressPercentComplete ranges from 0 to 100, but can contain decimal percentage values
    Public ReadOnly Property ProgressPercentComplete() As Single
        Get
            Return CType(Math.Round(mProgressPercentComplete, 2), Single)
        End Get
    End Property

    Public Property ShowMessages() As Boolean
        Get
            Return mShowMessages
        End Get
        Set(ByVal Value As Boolean)
            mShowMessages = Value
        End Set
    End Property
#End Region

    Public Overridable Sub AbortProcessingNow()
        mAbortProcessing = True
    End Sub

    Protected Function CleanupFilePaths(ByRef strInputFilePath As String, ByRef strOutputFolderPath As String) As Boolean
        ' Returns True if success, False if failure

        Dim ioFileInfo As System.IO.FileInfo
        Dim ioFolder As System.IO.DirectoryInfo
        Dim blnSuccess As Boolean

        Try
            ' Make sure strInputFilePath points to a valid file
            ioFileInfo = New System.IO.FileInfo(strInputFilePath)

            If Not ioFileInfo.Exists() Then
                If Me.ShowMessages Then
                    ShowErrorMessage("Input file not found: " & strInputFilePath)
                Else
                    LogMessage("Input file not found: " & strInputFilePath, eMessageTypeConstants.ErrorMsg)
                End If

                mErrorCode = eProcessFilesErrorCodes.InvalidInputFilePath
                blnSuccess = False
            Else
                If strOutputFolderPath Is Nothing OrElse strOutputFolderPath.Length = 0 Then
                    ' Define strOutputFolderPath based on strInputFilePath
                    strOutputFolderPath = ioFileInfo.DirectoryName
                End If

                ' Make sure strOutputFolderPath points to a folder
                ioFolder = New System.IO.DirectoryInfo(strOutputFolderPath)

                If Not ioFolder.Exists() Then
                    ' strOutputFolderPath points to a non-existent folder; attempt to create it
                    ioFolder.Create()
                End If

                mOutputFolderPath = String.Copy(strOutputFolderPath)

                blnSuccess = True
            End If

        Catch ex As Exception
            HandleException("Error cleaning up the file paths", ex)
        End Try

        Return blnSuccess
    End Function

    Protected Function CleanupInputFilePath(ByRef strInputFilePath As String) As Boolean
        ' Returns True if success, False if failure

        Dim ioFileInfo As System.IO.FileInfo
        Dim blnSuccess As Boolean

        Try
            ' Make sure strInputFilePath points to a valid file
            ioFileInfo = New System.IO.FileInfo(strInputFilePath)

            If Not ioFileInfo.Exists() Then
                If Me.ShowMessages Then
                    ShowErrorMessage("Input file not found: " & strInputFilePath)
                Else
                    LogMessage("Input file not found: " & strInputFilePath, eMessageTypeConstants.ErrorMsg)
                End If

                mErrorCode = eProcessFilesErrorCodes.InvalidInputFilePath
                blnSuccess = False
            Else
                blnSuccess = True
            End If

        Catch ex As Exception
            HandleException("Error cleaning up the file paths", ex)
        End Try

        Return blnSuccess
    End Function
    Protected Function GetBaseClassErrorMessage() As String
        ' Returns String.Empty if no error

        Dim strErrorMessage As String

        Select Case Me.ErrorCode
            Case eProcessFilesErrorCodes.NoError
                strErrorMessage = String.Empty
            Case eProcessFilesErrorCodes.InvalidInputFilePath
                strErrorMessage = "Invalid input file path"
            Case eProcessFilesErrorCodes.InvalidOutputFolderPath
                strErrorMessage = "Invalid output folder path"
            Case eProcessFilesErrorCodes.ParameterFileNotFound
                strErrorMessage = "Parameter file not found"
            Case eProcessFilesErrorCodes.InvalidParameterFile
                strErrorMessage = "Invalid parameter file"
            Case eProcessFilesErrorCodes.FilePathError
                strErrorMessage = "General file path error"
            Case eProcessFilesErrorCodes.LocalizedError
                strErrorMessage = "Localized error"
            Case eProcessFilesErrorCodes.UnspecifiedError
                strErrorMessage = "Unspecified error"
            Case Else
                ' This shouldn't happen
                strErrorMessage = "Unknown error state"
        End Select

        Return strErrorMessage

    End Function

    Private Function GetVersionForExecutingAssembly() As String

        Dim strVersion As String

        Try
            strVersion = System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString()
        Catch ex As Exception
            strVersion = "??.??.??.??"
        End Try

        Return strVersion

    End Function

    Public Overridable Function GetDefaultExtensionsToParse() As String()
        Dim strExtensionsToParse(0) As String

        strExtensionsToParse(0) = ".*"

        Return strExtensionsToParse

    End Function

    Public MustOverride Function GetErrorMessage() As String

    Protected Sub HandleException(ByVal strBaseMessage As String, ByVal ex As System.Exception)
        If strBaseMessage Is Nothing OrElse strBaseMessage.Length = 0 Then
            strBaseMessage = "Error"
        End If

        If Me.ShowMessages Then
            ' Note that ShowErrorMessage() will call LogMessage()
            ShowErrorMessage(strBaseMessage & ": " & ex.Message, True)
        Else
            LogMessage(strBaseMessage & ": " & ex.Message, eMessageTypeConstants.ErrorMsg)
            Throw New System.Exception(strBaseMessage, ex)
        End If

    End Sub

    Protected Sub LogMessage(ByVal strMessage As String)
        LogMessage(strMessage, eMessageTypeConstants.Normal)
    End Sub

    Protected Sub LogMessage(ByVal strMessage As String, ByVal eMessageType As eMessageTypeConstants)
        ' Note that CleanupFilePaths() will update mOutputFolderPath, which is used here if mLogFolderPath is blank
        ' Thus, be sure to call CleanupFilePaths (or update mLogFolderPath) before the first call to LogMessage

        Dim strMessageType As String
        Dim blnOpeningExistingFile As Boolean = False

        If mLogFile Is Nothing AndAlso mLogMessagesToFile Then
            Try
                mLogFilePath = System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location)
                mLogFilePath &= "_log_" & System.DateTime.Now.ToString("yyyy-MM-dd") & ".txt"

                Try
                    If mLogFolderPath Is Nothing Then mLogFolderPath = String.Empty

                    If mLogFolderPath.Length = 0 Then
                        ' Log folder is undefined; use mOutputFolderPath if it is defined
                        If Not mOutputFolderPath Is Nothing AndAlso mOutputFolderPath.Length > 0 Then
                            mLogFolderPath = String.Copy(mOutputFolderPath)
                        End If
                    End If

                    If mLogFolderPath.Length > 0 Then
                        ' Create the log folder if it doesn't exist
                        If Not System.IO.Directory.Exists(mLogFolderPath) Then
                            System.IO.Directory.CreateDirectory(mLogFolderPath)
                        End If
                    End If
                Catch ex As Exception
                    mLogFolderPath = String.Empty
                End Try

                If mLogFolderPath.Length > 0 Then
                    mLogFilePath = System.IO.Path.Combine(mLogFolderPath, mLogFilePath)
                End If

                blnOpeningExistingFile = System.IO.File.Exists(mLogFilePath)

                mLogFile = New System.IO.StreamWriter(New System.IO.FileStream(mLogFilePath, IO.FileMode.Append, IO.FileAccess.Write, IO.FileShare.Read))
                mLogFile.AutoFlush = True

                If Not blnOpeningExistingFile Then
                    mLogFile.WriteLine("Date" & ControlChars.Tab & _
                                       "Type" & ControlChars.Tab & _
                                       "Message")
                End If

            Catch ex As Exception
                ' Error creating the log file; set mLogMessagesToFile to false so we don't repeatedly try to create it
                mLogMessagesToFile = False
            End Try

        End If

        If Not mLogFile Is Nothing Then
            Select Case eMessageType
                Case eMessageTypeConstants.Normal
                    strMessageType = "Normal"
                Case eMessageTypeConstants.ErrorMsg
                    strMessageType = "Error"
                Case eMessageTypeConstants.Warning
                    strMessageType = "Warning"
                Case Else
                    strMessageType = "Unknown"
            End Select

            mLogFile.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt") & ControlChars.Tab & _
                               strMessageType & ControlChars.Tab & _
                               strMessage)
        End If

    End Sub

    Public Function ProcessFilesWildcard(ByVal strInputFolderPath As String) As Boolean
        Return ProcessFilesWildcard(strInputFolderPath, String.Empty, String.Empty)
    End Function

    Public Function ProcessFilesWildcard(ByVal strInputFilePath As String, ByVal strOutputFolderPath As String) As Boolean
        Return ProcessFilesWildcard(strInputFilePath, strOutputFolderPath, String.Empty)
    End Function

    Public Function ProcessFilesWildcard(ByVal strInputFilePath As String, ByVal strOutputFolderPath As String, ByVal strParameterFilePath As String) As Boolean
        Return ProcessFilesWildcard(strInputFilePath, strOutputFolderPath, strParameterFilePath, True)
    End Function

    Public Function ProcessFilesWildcard(ByVal strInputFilePath As String, ByVal strOutputFolderPath As String, ByVal strParameterFilePath As String, ByVal blnResetErrorCode As Boolean) As Boolean
        ' Returns True if success, False if failure

        Dim blnSuccess As Boolean
        Dim intMatchCount As Integer

        Dim strCleanPath As String
        Dim strInputFolderPath As String

        Dim ioFileMatch As System.IO.FileInfo
        Dim ioFileInfo As System.IO.FileInfo
        Dim ioFolderInfo As System.IO.DirectoryInfo

        mAbortProcessing = False
        blnSuccess = True
        Try
            ' Possibly reset the error code
            If blnResetErrorCode Then mErrorCode = eProcessFilesErrorCodes.NoError

            If Not strOutputFolderPath Is Nothing AndAlso strOutputFolderPath.Length > 0 Then
                ' Update the cached output folder path
                mOutputFolderPath = String.Copy(strOutputFolderPath)
            End If

            ' See if strInputFilePath contains a wildcard (* or ?)
            If Not strInputFilePath Is Nothing AndAlso (strInputFilePath.IndexOf("*") >= 0 Or strInputFilePath.IndexOf("?") >= 0) Then
                ' Obtain a list of the matching  files

                ' Copy the path into strCleanPath and replace any * or ? characters with _
                strCleanPath = strInputFilePath.Replace("*", "_")
                strCleanPath = strCleanPath.Replace("?", "_")

                ioFileInfo = New System.IO.FileInfo(strCleanPath)
                If ioFileInfo.Directory.Exists Then
                    strInputFolderPath = ioFileInfo.DirectoryName
                Else
                    ' Use the current working directory
                    strInputFolderPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
                End If

                ioFolderInfo = New System.io.DirectoryInfo(strInputFolderPath)

                ' Remove any directory information from strInputFilePath
                strInputFilePath = System.IO.Path.GetFileName(strInputFilePath)

                intMatchCount = 0
                For Each ioFileMatch In ioFolderInfo.GetFiles(strInputFilePath)

                    blnSuccess = ProcessFile(ioFileMatch.FullName, strOutputFolderPath, strParameterFilePath, blnResetErrorCode)

                    If Not blnSuccess Or mAbortProcessing Then Exit For
                    intMatchCount += 1

                    If intMatchCount Mod 100 = 0 Then Console.Write(".")

                Next ioFileMatch

                If intMatchCount = 0 Then
                    If mErrorCode = eProcessFilesErrorCodes.NoError Then
                        If Me.ShowMessages Then
                            ShowErrorMessage("No match was found for the input file path: " & strInputFilePath)
                        Else
                            LogMessage("No match was found for the input file path: " & strInputFilePath, eMessageTypeConstants.ErrorMsg)
                        End If
                    End If
                Else
                    Console.WriteLine()
                End If

            Else
                blnSuccess = ProcessFile(strInputFilePath, strOutputFolderPath, strParameterFilePath, blnResetErrorCode)
            End If

        Catch ex As Exception
            HandleException("Error in ProcessFilesWildcard", ex)
        End Try

        Return blnSuccess

    End Function

    Public Function ProcessFile(ByVal strInputFilePath As String) As Boolean
        Return ProcessFile(strInputFilePath, String.Empty, String.Empty)
    End Function

    Public Function ProcessFile(ByVal strInputFilePath As String, ByVal strOutputFolderPath As String) As Boolean
        Return ProcessFile(strInputFilePath, strOutputFolderPath, String.Empty)
    End Function

    Public Function ProcessFile(ByVal strInputFilePath As String, ByVal strOutputFolderPath As String, ByVal strParameterFilePath As String) As Boolean
        Return ProcessFile(strInputFilePath, strOutputFolderPath, strParameterFilePath, True)
    End Function

    Public MustOverride Function ProcessFile(ByVal strInputFilePath As String, ByVal strOutputFolderPath As String, ByVal strParameterFilePath As String, ByVal blnResetErrorCode As Boolean) As Boolean


    Public Function ProcessFilesAndRecurseFolders(ByVal strInputFolderPath As String) As Boolean
        Return ProcessFilesAndRecurseFolders(strInputFolderPath, String.Empty, String.Empty)
    End Function

    Public Function ProcessFilesAndRecurseFolders(ByVal strInputFilePathOrFolder As String, ByVal strOutputFolderName As String) As Boolean
        Return ProcessFilesAndRecurseFolders(strInputFilePathOrFolder, strOutputFolderName, String.Empty)
    End Function

    Public Function ProcessFilesAndRecurseFolders(ByVal strInputFilePathOrFolder As String, ByVal strOutputFolderName As String, ByVal strParameterFilePath As String) As Boolean
        Return ProcessFilesAndRecurseFolders(strInputFilePathOrFolder, strOutputFolderName, String.Empty, False, strParameterFilePath)
    End Function

    Public Function ProcessFilesAndRecurseFolders(ByVal strInputFilePathOrFolder As String, ByVal strOutputFolderName As String, ByVal strParameterFilePath As String, ByVal strExtensionsToParse() As String) As Boolean
        Return ProcessFilesAndRecurseFolders(strInputFilePathOrFolder, strOutputFolderName, String.Empty, False, strParameterFilePath, 0, strExtensionsToParse)
    End Function

    Public Function ProcessFilesAndRecurseFolders(ByVal strInputFilePathOrFolder As String, ByVal strOutputFolderName As String, ByVal strOutputFolderAlternatePath As String, ByVal blnRecreateFolderHierarchyInAlternatePath As Boolean) As Boolean
        Return ProcessFilesAndRecurseFolders(strInputFilePathOrFolder, strOutputFolderName, strOutputFolderAlternatePath, blnRecreateFolderHierarchyInAlternatePath, String.Empty)
    End Function

    Public Function ProcessFilesAndRecurseFolders(ByVal strInputFilePathOrFolder As String, ByVal strOutputFolderName As String, ByVal strOutputFolderAlternatePath As String, ByVal blnRecreateFolderHierarchyInAlternatePath As Boolean, ByVal strParameterFilePath As String) As Boolean
        Return ProcessFilesAndRecurseFolders(strInputFilePathOrFolder, strOutputFolderName, strOutputFolderAlternatePath, blnRecreateFolderHierarchyInAlternatePath, strParameterFilePath, 0)
    End Function

    Public Function ProcessFilesAndRecurseFolders(ByVal strInputFilePathOrFolder As String, ByVal strOutputFolderName As String, ByVal strOutputFolderAlternatePath As String, ByVal blnRecreateFolderHierarchyInAlternatePath As Boolean, ByVal strParameterFilePath As String, ByVal intRecurseFoldersMaxLevels As Integer) As Boolean
        Return ProcessFilesAndRecurseFolders(strInputFilePathOrFolder, strOutputFolderName, strOutputFolderAlternatePath, blnRecreateFolderHierarchyInAlternatePath, strParameterFilePath, intRecurseFoldersMaxLevels, GetDefaultExtensionsToParse())
    End Function

    Public Function ProcessFilesAndRecurseFolders(ByVal strInputFilePathOrFolder As String, ByVal strOutputFolderName As String, ByVal strOutputFolderAlternatePath As String, ByVal blnRecreateFolderHierarchyInAlternatePath As Boolean, ByVal strParameterFilePath As String, ByVal intRecurseFoldersMaxLevels As Integer, ByVal strExtensionsToParse() As String) As Boolean
        ' Calls ProcessFiles for all files in strInputFilePathOrFolder and below having an extension listed in strExtensionsToParse()
        ' The extensions should be of the form ".TXT" or ".RAW" (i.e. a period then the extension)
        ' If any of the extensions is "*" or ".*" then all files will be processed
        ' If strInputFilePathOrFolder contains a filename with a wildcard (* or ?), then that information will be 
        '  used to filter the files that are processed
        ' If intRecurseFoldersMaxLevels is <=0 then we recurse infinitely

        Dim strCleanPath As String
        Dim strInputFolderPath As String

        Dim ioFileInfo As System.IO.FileInfo
        Dim ioFolderInfo As System.IO.DirectoryInfo

        Dim blnSuccess As Boolean
        Dim intFileProcessCount, intFileProcessFailCount As Integer

        ' Examine strInputFilePathOrFolder to see if it contains a filename; if not, assume it points to a folder
        ' First, see if it contains a * or ?
        Try
            If Not strInputFilePathOrFolder Is Nothing AndAlso (strInputFilePathOrFolder.IndexOf("*") >= 0 Or strInputFilePathOrFolder.IndexOf("?") >= 0) Then
                ' Copy the path into strCleanPath and replace any * or ? characters with _
                strCleanPath = strInputFilePathOrFolder.Replace("*", "_")
                strCleanPath = strCleanPath.Replace("?", "_")

                ioFileInfo = New System.IO.FileInfo(strCleanPath)
                If ioFileInfo.Directory.Exists Then
                    strInputFolderPath = ioFileInfo.DirectoryName
                Else
                    ' Use the current working directory
                    strInputFolderPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
                End If

                ' Remove any directory information from strInputFilePath
                strInputFilePathOrFolder = System.IO.Path.GetFileName(strInputFilePathOrFolder)

            Else
                ioFolderInfo = New System.IO.DirectoryInfo(strInputFilePathOrFolder)
                If ioFolderInfo.Exists Then
                    strInputFolderPath = ioFolderInfo.FullName
                    strInputFilePathOrFolder = "*"
                Else
                    If ioFolderInfo.Parent.Exists Then
                        strInputFolderPath = ioFolderInfo.Parent.FullName
                        strInputFilePathOrFolder = System.IO.Path.GetFileName(strInputFilePathOrFolder)
                    Else
                        ' Unable to determine the input folder path
                        strInputFolderPath = String.Empty
                    End If
                End If
            End If

            If Not strInputFolderPath Is Nothing AndAlso strInputFolderPath.Length > 0 Then

                ' Validate the output folder path
                If Not strOutputFolderAlternatePath Is Nothing AndAlso strOutputFolderAlternatePath.Length > 0 Then
                    Try
                        ioFolderInfo = New System.IO.DirectoryInfo(strOutputFolderAlternatePath)
                        If Not ioFolderInfo.Exists Then ioFolderInfo.Create()
                    Catch ex As Exception
                        Debug.Assert(False, ex.Message)
                        mErrorCode = clsProcessFilesBaseClass.eProcessFilesErrorCodes.InvalidOutputFolderPath
                        Return False
                    End Try
                End If

                ' Initialize some parameters
                mAbortProcessing = False
                intFileProcessCount = 0
                intFileProcessFailCount = 0

                ' Call RecurseFoldersWork
                blnSuccess = RecurseFoldersWork(strInputFolderPath, strInputFilePathOrFolder, strOutputFolderName, strParameterFilePath, strOutputFolderAlternatePath, blnRecreateFolderHierarchyInAlternatePath, strExtensionsToParse, intFileProcessCount, intFileProcessFailCount, 1, intRecurseFoldersMaxLevels)

            Else
                mErrorCode = clsProcessFilesBaseClass.eProcessFilesErrorCodes.InvalidInputFilePath
                Return False
            End If

        Catch ex As Exception
            HandleException("Error in ProcessFilesAndRecurseFolders", ex)
            blnSuccess = False
        End Try

        Return blnSuccess

    End Function

    Private Function RecurseFoldersWork(ByVal strInputFolderPath As String, ByVal strFileNameMatch As String, ByVal strOutputFolderName As String, ByVal strParameterFilePath As String, ByVal strOutputFolderAlternatePath As String, ByVal blnRecreateFolderHierarchyInAlternatePath As Boolean, ByVal strExtensionsToParse() As String, ByRef intFileProcessCount As Integer, ByRef intFileProcessFailCount As Integer, ByVal intRecursionLevel As Integer, ByVal intRecurseFoldersMaxLevels As Integer) As Boolean
        ' If intRecurseFoldersMaxLevels is <=0 then we recurse infinitely

        Dim ioInputFolderInfo As System.IO.DirectoryInfo
        Dim ioSubFolderInfo As System.IO.DirectoryInfo
        Dim ioFileMatch As System.io.FileInfo

        Dim intExtensionIndex As Integer
        Dim blnProcessAllExtensions As Boolean

        Dim strOutputFolderPathToUse As String
        Dim blnSuccess As Boolean

        Try
            ioInputFolderInfo = New System.IO.DirectoryInfo(strInputFolderPath)
        Catch ex As Exception
            ' Input folder path error
            HandleException("Error in RecurseFoldersWork", ex)
            mErrorCode = eProcessFilesErrorCodes.InvalidInputFilePath
            Return False
        End Try

        Try
            If Not strOutputFolderAlternatePath Is Nothing AndAlso strOutputFolderAlternatePath.Length > 0 Then
                If blnRecreateFolderHierarchyInAlternatePath Then
                    strOutputFolderAlternatePath = System.IO.Path.Combine(strOutputFolderAlternatePath, ioInputFolderInfo.Name)
                End If
                strOutputFolderPathToUse = System.IO.Path.Combine(strOutputFolderAlternatePath, strOutputFolderName)
            Else
                strOutputFolderPathToUse = strOutputFolderName
            End If
        Catch ex As Exception
            ' Output file path error
            HandleException("Error in RecurseFoldersWork", ex)
            mErrorCode = eProcessFilesErrorCodes.InvalidOutputFolderPath
            Return False
        End Try

        Try
            ' Validate strExtensionsToParse()
            For intExtensionIndex = 0 To strExtensionsToParse.Length - 1
                If strExtensionsToParse(intExtensionIndex) Is Nothing Then
                    strExtensionsToParse(intExtensionIndex) = String.Empty
                Else
                    If Not strExtensionsToParse(intExtensionIndex).StartsWith(".") Then
                        strExtensionsToParse(intExtensionIndex) = "." & strExtensionsToParse(intExtensionIndex)
                    End If

                    If strExtensionsToParse(intExtensionIndex) = ".*" Then
                        blnProcessAllExtensions = True
                        Exit For
                    Else
                        strExtensionsToParse(intExtensionIndex) = strExtensionsToParse(intExtensionIndex).ToUpper
                    End If
                End If
            Next intExtensionIndex
        Catch ex As Exception
            HandleException("Error in RecurseFoldersWork", ex)
            mErrorCode = eProcessFilesErrorCodes.UnspecifiedError
            Return False
        End Try

        Try
            If Not strOutputFolderPathToUse Is Nothing AndAlso strOutputFolderPathToUse.Length > 0 Then
                ' Update the cached output folder path
                mOutputFolderPath = String.Copy(strOutputFolderPathToUse)
            End If

            ShowMessage("Examining " & strInputFolderPath)

            ' Process any matching files in this folder
            blnSuccess = True
            For Each ioFileMatch In ioInputFolderInfo.GetFiles(strFileNameMatch)

                For intExtensionIndex = 0 To strExtensionsToParse.Length - 1
                    If blnProcessAllExtensions OrElse ioFileMatch.Extension.ToUpper = strExtensionsToParse(intExtensionIndex) Then
                        blnSuccess = ProcessFile(ioFileMatch.FullName, strOutputFolderPathToUse, strParameterFilePath, True)
                        If Not blnSuccess Then
                            intFileProcessFailCount += 1
                            blnSuccess = True
                        Else
                            intFileProcessCount += 1
                        End If
                        Exit For
                    End If

                    If mAbortProcessing Then Exit For

                Next intExtensionIndex
            Next ioFileMatch
        Catch ex As Exception
            HandleException("Error in RecurseFoldersWork", ex)
            mErrorCode = eProcessFilesErrorCodes.InvalidInputFilePath
            Return False
        End Try

        If Not mAbortProcessing Then
            ' If intRecurseFoldersMaxLevels is <=0 then we recurse infinitely
            '  otherwise, compare intRecursionLevel to intRecurseFoldersMaxLevels
            If intRecurseFoldersMaxLevels <= 0 OrElse intRecursionLevel <= intRecurseFoldersMaxLevels Then
                ' Call this function for each of the subfolders of ioInputFolderInfo
                For Each ioSubFolderInfo In ioInputFolderInfo.GetDirectories()
                    blnSuccess = RecurseFoldersWork(ioSubFolderInfo.FullName, strFileNameMatch, strOutputFolderName, strParameterFilePath, strOutputFolderAlternatePath, blnRecreateFolderHierarchyInAlternatePath, strExtensionsToParse, intFileProcessCount, intFileProcessFailCount, intRecursionLevel + 1, intRecurseFoldersMaxLevels)
                    If Not blnSuccess Then Exit For
                Next ioSubFolderInfo
            End If
        End If

        Return blnSuccess

    End Function

    Protected Sub ResetProgress()
        RaiseEvent ProgressReset()
    End Sub

    Protected Sub ResetProgress(ByVal strProgressStepDescription As String)
        UpdateProgress(strProgressStepDescription, 0)
        RaiseEvent ProgressReset()
    End Sub

    Protected Sub SetBaseClassErrorCode(ByVal eNewErrorCode As eProcessFilesErrorCodes)
        mErrorCode = eNewErrorCode
    End Sub

    Protected Sub ShowErrorMessage(ByVal strMessage As String)
        ShowErrorMessage(strMessage, True)
    End Sub

    Protected Sub ShowErrorMessage(ByVal strMessage As String, ByVal blnAllowLogToFile As Boolean)
        Dim strSeparator As String = "------------------------------------------------------------------------------"

        Console.WriteLine()
        Console.WriteLine(strSeparator)
        Console.WriteLine(strMessage)
        Console.WriteLine(strSeparator)
        Console.WriteLine()

        If blnAllowLogToFile Then
            LogMessage(strMessage, eMessageTypeConstants.ErrorMsg)
        End If

    End Sub

    Protected Sub ShowMessage(ByVal strMessage As String)
        ShowMessage(strMessage, True, False)
    End Sub

    Protected Sub ShowMessage(ByVal strMessage As String, ByVal blnAllowLogToFile As Boolean)
        ShowMessage(strMessage, blnAllowLogToFile, False)
    End Sub

    Protected Sub ShowMessage(ByVal strMessage As String, ByVal blnAllowLogToFile As Boolean, ByVal blnPrecedeWithNewline As Boolean)

        If blnPrecedeWithNewline Then
            Console.WriteLine()
        End If
        Console.WriteLine(strMessage)

        If blnAllowLogToFile Then
            LogMessage(strMessage, eMessageTypeConstants.Normal)
        End If

    End Sub

    Protected Sub UpdateProgress(ByVal strProgressStepDescription As String)
        UpdateProgress(strProgressStepDescription, mProgressPercentComplete)
    End Sub

    Protected Sub UpdateProgress(ByVal sngPercentComplete As Single)
        UpdateProgress(Me.ProgressStepDescription, sngPercentComplete)
    End Sub

    Protected Sub UpdateProgress(ByVal strProgressStepDescription As String, ByVal sngPercentComplete As Single)
        Dim blnDescriptionChanged As Boolean = False

        If strProgressStepDescription <> mProgressStepDescription Then
            blnDescriptionChanged = True
        End If

        mProgressStepDescription = String.Copy(strProgressStepDescription)
        If sngPercentComplete < 0 Then
            sngPercentComplete = 0
        ElseIf sngPercentComplete > 100 Then
            sngPercentComplete = 100
        End If
        mProgressPercentComplete = sngPercentComplete

        If blnDescriptionChanged Then
            If mProgressPercentComplete = 0 Then
                LogMessage(mProgressStepDescription)
            Else
                LogMessage(mProgressStepDescription & " (" & mProgressPercentComplete.ToString("0.0") & "% complete)")
            End If
        End If

        RaiseEvent ProgressChanged(Me.ProgressStepDescription, Me.ProgressPercentComplete)

    End Sub

    Protected Sub OperationComplete()
        RaiseEvent ProgressComplete()
    End Sub

    '' The following functions should be placed in any derived class
    '' Cannot define as MustOverride since it contains a customized enumerated type (eDerivedClassErrorCodes) in the function declaration

    ''Private Sub SetLocalErrorCode(ByVal eNewErrorCode As eDerivedClassErrorCodes)
    ''    SetLocalErrorCode(eNewErrorCode, False)
    ''End Sub

    ''Private Sub SetLocalErrorCode(ByVal eNewErrorCode As eDerivedClassErrorCodes, ByVal blnLeaveExistingErrorCodeUnchanged As Boolean)
    ''    If blnLeaveExistingErrorCodeUnchanged AndAlso mLocalErrorCode <> eDerivedClassErrorCodes.NoError Then
    ''        ' An error code is already defined; do not change it
    ''    Else
    ''        mLocalErrorCode = eNewErrorCode

    ''        If eNewErrorCode = eDerivedClassErrorCodes.NoError Then
    ''            If MyBase.ErrorCode = clsProcessFilesBaseClass.eProcessFilesErrorCodes.LocalizedError Then
    ''                MyBase.SetBaseClassErrorCode(clsProcessFilesBaseClass.eProcessFilesErrorCodes.NoError)
    ''            End If
    ''        Else
    ''            MyBase.SetBaseClassErrorCode(clsProcessFilesBaseClass.eProcessFilesErrorCodes.LocalizedError)
    ''        End If
    ''    End If

    ''End Sub

End Class
