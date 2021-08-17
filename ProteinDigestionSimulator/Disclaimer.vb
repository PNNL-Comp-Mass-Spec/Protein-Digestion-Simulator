Option Strict On

Public Class Disclaimer
    Inherits System.Windows.Forms.Form

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        InitializeControls()
    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    Friend WithEvents txtNotice As System.Windows.Forms.TextBox
    Friend WithEvents cmdOK As System.Windows.Forms.Button
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.txtNotice = New System.Windows.Forms.TextBox
        Me.cmdOK = New System.Windows.Forms.Button
        Me.SuspendLayout()
        '
        'txtNotice
        '
        Me.txtNotice.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtNotice.Location = New System.Drawing.Point(8, 16)
        Me.txtNotice.Multiline = True
        Me.txtNotice.Name = "txtNotice"
        Me.txtNotice.ReadOnly = True
        Me.txtNotice.Size = New System.Drawing.Size(440, 176)
        Me.txtNotice.TabIndex = 2
        '
        'cmdOK
        '
        Me.cmdOK.Location = New System.Drawing.Point(168, 200)
        Me.cmdOK.Name = "cmdOK"
        Me.cmdOK.Size = New System.Drawing.Size(104, 24)
        Me.cmdOK.TabIndex = 7
        Me.cmdOK.Text = "&OK"
        '
        'frmDisclaimer
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(456, 238)
        Me.ControlBox = False
        Me.Controls.Add(Me.cmdOK)
        Me.Controls.Add(Me.txtNotice)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmDisclaimer"
        Me.Text = "Normalized Elution Time (NET) Prediction Utility"
        Me.TopMost = True
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

    ' Ignore Spelling: Kangas, Petritis, cmd, chk, txt, frm

    Const FORM_CLOSE_DELAY_SECONDS As Integer = 2

    Protected WithEvents mCloseDelayTimer As Timers.Timer
    Protected mTimerStartTime As Date

    Public Shared Function GetKangasPetritisDisclaimerText(Optional addNewlines As Boolean = True) As String

        Dim newlineText As String
        If addNewlines Then
            newlineText = ControlChars.NewLine & ControlChars.NewLine
        Else
            newlineText = ": "
        End If

        Return "NOTICE/DISCLAIMER" &
               newlineText &
               "The methods embodied in this software to derive the Kangas/Petritis retention time " &
               "prediction values are covered by U.S. patent 7,136,759 and pending patent 2005-0267688A1.  " &
               "The software is made available solely for non-commercial research purposes on an " &
               """as is"" basis by Battelle Memorial Institute.  If rights to deploy and distribute  " &
               "the code for commercial purposes are of interest, please contact proteomics@pnnl.gov"

    End Function

    Public Sub InitializeControls()
        txtNotice.Text = GetKangasPetritisDisclaimerText()
        txtNotice.SelectionStart = 0
        'txtNotice.SelectionLength = 0

        cmdOK.Text = FORM_CLOSE_DELAY_SECONDS.ToString()
        cmdOK.Enabled = False

        mTimerStartTime = Date.UtcNow

        mCloseDelayTimer = New Timers.Timer(250)
        mCloseDelayTimer.SynchronizingObject = Me

        mCloseDelayTimer.Start()

    End Sub

    Private Sub mCloseDelayTimer_Elapsed(sender As Object, e As Timers.ElapsedEventArgs) Handles mCloseDelayTimer.Elapsed
        Dim secondsRemaining As Integer

        secondsRemaining = CInt(Math.Round(FORM_CLOSE_DELAY_SECONDS - Date.UtcNow.Subtract(mTimerStartTime).TotalSeconds, 0))
        If secondsRemaining < 0 Then secondsRemaining = 0

        If secondsRemaining > 0 Then
            cmdOK.Text = secondsRemaining.ToString()
            Application.DoEvents()
        Else
            cmdOK.Text = "&OK"
            cmdOK.Enabled = True
            mCloseDelayTimer.Enabled = False
            Application.DoEvents()
        End If
    End Sub

    Private Sub cmdOK_Click(sender As Object, e As EventArgs) Handles cmdOK.Click
        Me.Close()
    End Sub
End Class
