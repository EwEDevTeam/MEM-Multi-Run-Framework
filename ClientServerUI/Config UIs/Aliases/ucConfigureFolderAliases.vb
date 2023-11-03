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
Imports ClientServer

#End Region ' Imports

Public Class ucConfigureFolderAliases
    Implements IConfigUI

    Public Property Settings As cSettings Implements IConfigUI.Settings

    Public Event OnChanged(sender As Object, args As EventArgs) Implements IConfigUI.OnChanged

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        Dim s As cFolderAliases = Me.Settings.FolderSettings
        Dim f As New cTaskRunnerFactory(Me.Settings)

        If (s.Aliases.Count > 0) Then
            Me.m_dgv.Rows.Add(s.Aliases.Count)
            For i As Integer = 0 To s.Aliases.Count - 1
                Dim a As String = s.Aliases(i)
                Dim v As String = s.Alias(a)
                Me.m_dgv(Me.m_colAlias.Index, i).Value = a
                Me.m_dgv(Me.m_colFolder.Index, i).Value = v
            Next
        End If

    End Sub

    Public Function Apply() As Boolean Implements IConfigUI.Apply
        Dim s As cFolderAliases = Me.Settings.FolderSettings
        s.Clear()
        For i As Integer = 0 To Me.m_dgv.Rows.Count - 1
            Try
                Dim a As String = CStr(Me.m_dgv(Me.m_colAlias.Index, i).Value)
                Dim f As String = CStr(Me.m_dgv(Me.m_colFolder.Index, i).Value)
                s.Alias(a) = f
            Catch ex As Exception

            End Try
        Next
        Return True
    End Function

    Public Function CanApply() As Boolean Implements IConfigUI.CanApply
        Return True
    End Function


    Private Sub m_dgv_CellBeginEdit(sender As Object, e As DataGridViewCellCancelEventArgs) Handles m_dgv.CellBeginEdit

        If (e.ColumnIndex = Me.m_colFolder.Index) Then

            Dim val As Object = Me.m_dgv(e.ColumnIndex, e.RowIndex).Value
            Dim fld As String = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)

            If (val IsNot Nothing) Then
                fld = CStr(val)
            End If

            Dim dlg As New FolderBrowserDialog()
            dlg.SelectedPath = fld
            dlg.ShowNewFolderButton = False

            If (dlg.ShowDialog = DialogResult.OK) Then
                Me.m_dgv(e.ColumnIndex, e.RowIndex).Value = dlg.SelectedPath
                Me.RaiseChangeEvent()
            End If
            e.Cancel = True
        End If

    End Sub

    Private Sub m_dgv_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs) Handles m_dgv.CellEndEdit
        Me.RaiseChangeEvent()
    End Sub

    Private Sub RaiseChangeEvent()
        Try
            RaiseEvent OnChanged(Me, New EventArgs())
        Catch ex As Exception

        End Try
    End Sub

End Class
