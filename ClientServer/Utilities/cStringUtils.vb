' ===============================================================================
' This file is part of the Marine Ecosystem Model multi-run framework prototype, 
' developed for the PhD thesis of Jeroen Steenbeek (2020-2023) in Marine Sciences 
' at the Polytechnical University of Catalunya.
'
' The MEM run framework is distributed in the hope that it will be useful, but 
' WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or 
' FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more 
' details.
'
' You should have received a copy of the GNU General Public License along with EwE.
' If not, see <http:'www.gnu.org/licenses/gpl-2.0.html>. 
'
' Copyright 2020- 
'    Ecopath International Initiative, Barcelona, Spain
' ==============================================================================='

#Region " Imports "

Option Strict On

#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' Class offering string utilities.
''' </summary>
''' ---------------------------------------------------------------------------
Public Class cStringUtils

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Split function that supports text qualifiers. Code adapted from Larry Steinly,
    ''' http://www.codeproject.com/Articles/15361/Split-Function-that-Supports-Text-Qualifiers.
    ''' </summary>
    ''' <param name="strExpression">String to split.</param>
    ''' <param name="strDelimiter">Delimiting character to split by.</param>
    ''' <param name="strQualifier">String qualifier, such as single or double quotes. Qualified string
    ''' segments will not be subdivided by delimiting characters.</param>
    ''' <returns>An array of strings.</returns>
    ''' ---------------------------------------------------------------------------
    Public Shared Function SplitQualified(strExpression As String,
                                              strDelimiter As String,
                                              Optional strQualifier As String = """") As String()

        ' Sanity check
        If String.IsNullOrEmpty(strExpression) Then Return New String() {String.Empty}

        ' Ensure defaults. A whitespace delimiter is allowed!
        If String.IsNullOrEmpty(strDelimiter) Then strDelimiter = ","
        If String.IsNullOrWhiteSpace(strQualifier) Then strQualifier = """"

        Dim bQualifier As Boolean = False
        Dim iStart As Integer = 0
        Dim lValues As New List(Of String)
        Dim iQL As Integer = strQualifier.Length
        Dim iDL As Integer = strDelimiter.Length
        Dim strVal As String = ""

        For iChar As Integer = 0 To strExpression.Length - 1
            If String.Compare(strExpression.Substring(iChar, iQL), strQualifier, True) = 0 Then
                bQualifier = Not bQualifier
            ElseIf Not bQualifier And String.Compare(strExpression.Substring(iChar, strDelimiter.Length), strDelimiter, True) = 0 Then
                ' Crop leading and trainling delimiter
                strVal = strExpression.Substring(iStart, iChar - iStart)
                If strVal.StartsWith(strQualifier) Then strVal = strVal.Substring(iQL)
                If strVal.EndsWith(strQualifier) Then strVal = strVal.Substring(0, strVal.Length - iQL)
                lValues.Add(strVal)
                iStart = iChar + 1
            End If
        Next

        If (iStart < strExpression.Length) Then
            ' Crop leading and trainling delimiter
            strVal = strExpression.Substring(iStart)
            If strVal.StartsWith(strQualifier) Then strVal = strVal.Substring(iQL)
            If strVal.EndsWith(strQualifier) Then strVal = strVal.Substring(0, strVal.Length - iQL)
            lValues.Add(strVal)
        End If

        Return lValues.ToArray()

    End Function

    ''' ---------------------------------------------------------------------------
    ''' <summary>
    ''' Split function that supports text qualifiers.
    ''' </summary>
    ''' <param name="strExpression">String to split.</param>
    ''' <param name="cDelimiter">Delimiting character to split by.</param>
    ''' <param name="cQualifier">String qualifier, such as single or double quotes. Qualified string
    ''' segments will not be subdivided by delimiting characters.</param>
    ''' <returns>An array of strings.</returns>
    ''' <remarks>
    ''' <para>REgEx splitting is too slow. Replaced by a self-written, much faster method.</para>
    ''' <para>Support for "" to indicate " is needed!</para>
    ''' </remarks>
    ''' ---------------------------------------------------------------------------
    Public Shared Function SplitQualified(strExpression As String,
                                          cDelimiter As Char,
                                          Optional cQualifier As Char = """"c) As String()
        Return cStringUtils.SplitQualified(strExpression, CStr(cDelimiter), CStr(cQualifier))
    End Function

End Class
