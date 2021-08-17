Imports System.Text

Public Class CleavageRule

    ''' <summary>
    ''' Cleavage rule description
    ''' </summary>
    Public ReadOnly Property Description As String

    ''' <summary>
    ''' Residues to cleave after (or before if ReversedCleavageDirection is true)
    ''' Will not cleave after the residue if it is followed by any of the ExceptionResidues (or preceded by if ReversedCleavageDirection is true)
    ''' </summary>
    Public ReadOnly Property CleavageResidues As String

    ''' <summary>
    ''' Adjacent residue(s) that prevent cleavage
    ''' </summary>
    Public ReadOnly Property ExceptionResidues As String

    ''' <summary>
    ''' When false, cleave after the CleavageResidues, unless followed by the ExceptionResidues, e.g. Trypsin, CNBr, GluC
    ''' When true, cleave before the CleavageResidues, unless preceded by the ExceptionResidues, e.g. Asp-N
    ''' </summary>
    Public ReadOnly Property ReversedCleavageDirection As Boolean

    ''' <summary>
    ''' When true, allow for either end of a peptide to match the cleavage rules
    ''' When false, both ends must match the cleavage rules
    ''' </summary>
    Public ReadOnly Property AllowPartialCleavage As Boolean

    ''' <summary>
    ''' Additional cleavage rules to also consider when checking for cleavage points in a peptide
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property AdditionalCleavageRules As List(Of CleavageRule)

    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' <param name="ruleDescription"></param>
    ''' <param name="cleavageResidueList"></param>
    ''' <param name="exceptionResidueList"></param>
    ''' <param name="reversedCleavage"></param>
    ''' <param name="allowPartial"></param>
    Public Sub New(
        ruleDescription As String,
        cleavageResidueList As String,
        exceptionResidueList As String,
        reversedCleavage As Boolean,
        Optional allowPartial As Boolean = False,
        Optional additionalRules As IReadOnlyCollection(Of CleavageRule) = Nothing)

        Description = ruleDescription
        CleavageResidues = cleavageResidueList
        ExceptionResidues = exceptionResidueList
        ReversedCleavageDirection = reversedCleavage
        AllowPartialCleavage = allowPartial

        AdditionalCleavageRules = New List(Of CleavageRule)

        If additionalRules Is Nothing OrElse additionalRules.Count = 0 Then
            Exit Sub
        End If

        AdditionalCleavageRules.AddRange(additionalRules)
    End Sub

    ''' <summary>
    ''' Obtain a string describing the cleavage residues and exception residues
    ''' </summary>
    ''' <returns></returns>
    Public Function GetDetailedRuleDescription(Optional includeForwardCleavageDirectionWord As Boolean = False) As String

        Dim detailedDescription = New StringBuilder()

        If ReversedCleavageDirection Then
            detailedDescription.Append("before " & CleavageResidues)
            If ExceptionResidues.Length > 0 Then
                detailedDescription.Append(" not preceded by " & ExceptionResidues)
            End If
        Else
            If includeForwardCleavageDirectionWord Then
                detailedDescription.Append("after ")
            End If
            detailedDescription.Append(CleavageResidues)
            If ExceptionResidues.Length > 0 Then
                detailedDescription.Append(" not " & ExceptionResidues)
            End If
        End If

        For Each additionalRule In AdditionalCleavageRules
            detailedDescription.Append("; or " & additionalRule.GetDetailedRuleDescription(True))
        Next

        Return detailedDescription.ToString()

    End Function

    Public Overrides Function ToString() As String
        Return Description
    End Function

End Class
