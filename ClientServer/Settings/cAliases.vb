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

''' ===========================================================================
''' <summary>
''' Maintains a list of aliases with alternatives. Alias names are not case sensitive.
''' </summary>
''' ===========================================================================
Public Class cAliases

#Region " Private vars "

    Private m_aliases As New Dictionary(Of String, String)
    Private m_reservedAliases As New List(Of String)

#End Region ' Private vars

#Region " Public access "

    Public Sub New()
        ' NOP
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get or set an alias. Alias names are not case sensitive.
    ''' </summary>
    ''' <param name="name"></param>
    ''' -----------------------------------------------------------------------
    Public Property [Alias](name As String) As String
        Get
            If (String.IsNullOrWhiteSpace(name)) Then Return ""
            name = name.ToLower

            If (Not Me.m_aliases.ContainsKey(name)) Then Return ""
            Return Me.m_aliases(name)
        End Get
        Set(value As String)
            If (String.IsNullOrWhiteSpace(name)) Then Return
            name = name.ToLower

            If (Me.IsReservedAlias(name)) Then Return
            If (String.IsNullOrWhiteSpace(value)) Then
                Me.m_aliases.Remove(name)
            Else
                Me.m_aliases(name) = value
            End If
        End Set
    End Property

    Public Function Aliases() As IEnumerable(Of String)
        ' Return a copy to allow for looped manipulation
        Return Me.m_aliases.Keys.ToArray()
    End Function

    Public Function ReservedAliases() As IEnumerable(Of String)
        ' Return a copy to allow for looped manipulation
        Return Me.m_reservedAliases.ToArray()
    End Function

    Public Function IsReservedAlias(name As String) As Boolean
        If (String.IsNullOrWhiteSpace(name)) Then Return False
        Return (Me.m_reservedAliases.IndexOf(name) >= 0)
    End Function

    Public Function HasAlias(name As String) As Boolean
        Return Not String.IsNullOrWhiteSpace(Me.Alias(name))
    End Function

    Public Sub Clear()
        Me.m_aliases.Clear()
    End Sub

#End Region ' Public access

#Region " Persistence "

    Public Overridable Function Load(settings As cXMLSettings, section As String) As Boolean

        ' Sanity checks
        If (settings Is Nothing) Then Return False

        Me.Clear()

        Dim n As Integer = settings.ReadSetting(section, "AliasCount", 0)
        For i As Integer = 1 To n
            Dim name As String = settings.ReadSetting(section, "AliasName" & CStr(i), "")
            Dim value As String = settings.ReadSetting(section, "AliasValue" & CStr(i), "")
            Me.Alias(name) = value
        Next
        Return True

    End Function

    Public Overridable Function Save(settings As cXMLSettings, section As String) As Boolean

        ' Sanity checks
        If (settings Is Nothing) Then Return False

        Dim sys As New List(Of String)

        settings.ClearSettings(section)
        settings.WriteSetting(section, "AliasCount", Me.m_aliases.Count())
        For i As Integer = 1 To Me.Aliases.Count
            Dim a As String = Me.Aliases(i - 1)
            If (Not Me.IsReservedAlias(a)) Then
                settings.WriteSetting(section, "AliasName" & CStr(i), a)
                settings.WriteSetting(section, "AliasValue" & CStr(i), Me.Alias(a))
            End If
        Next
        Return True

    End Function

#End Region ' Persistence

#Region " Internals "

    Protected Sub ReserveAlias(name As String)

        Debug.Assert(Not String.IsNullOrWhiteSpace(name))
        Me.m_reservedAliases.Append(name.ToLower())

    End Sub

#End Region ' Internals

End Class
