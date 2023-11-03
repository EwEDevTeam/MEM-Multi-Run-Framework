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
Imports Server
Imports ClientServer
Imports ClientServerUI
Imports System.IO
Imports System.Security.Cryptography.X509Certificates

#End Region ' Imports

Public Class frmServerUI

    Private m_server As cServer = Nothing
    Private m_protocol As cFileDataExchangeProtocol = Nothing
    Private m_dispatch As cWorkScheduler = Nothing
    Private m_settings As cSettings = Nothing

#Region " Construction / destruction "

    Public Sub New()

        Me.m_server = New cServer(666)
        Me.m_protocol = New cFileDataExchangeProtocol(Me.m_server.ID, True)
        Me.m_dispatch = New cWorkScheduler(Me.m_server)
        Me.m_settings = New cSettings()

        ' Use default settings file
        Me.m_settings.SettingsFile = ""
        Me.m_settings.Load()

        Me.InitializeComponent()

    End Sub

#End Region ' Construction / destruction

#Region " Overrides "

    Protected Overrides Sub OnLoad(e As EventArgs)

        MyBase.OnLoad(e)

        AddHandler Me.m_server.OnClientAddedRemoved, AddressOf OnClientAddedRemoved
        AddHandler Me.m_dispatch.Output.OnWorkloadStatusChanged, AddressOf OnWorkloadUpdate

        Me.m_protocol.Folder = "D:\Cloud\Dropbox\PhD-filesystem-test\.framework"
        Me.UpdateRunStateControls()
        Me.UpdateConfigurationStateControls()

    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)

        RemoveHandler Me.m_server.OnClientAddedRemoved, AddressOf OnClientAddedRemoved
        RemoveHandler Me.m_dispatch.Output.OnWorkloadStatusChanged, AddressOf OnWorkloadUpdate
        MyBase.OnFormClosing(e)

    End Sub

    Protected Overrides Sub OnDragEnter(e As DragEventArgs)
        If (e.Data.GetDataPresent(DataFormats.FileDrop)) Then
            e.Effect = DragDropEffects.All
        End If
        MyBase.OnDragEnter(e)
    End Sub

    Protected Overrides Sub OnDragDrop(e As DragEventArgs)

        ' Need to find a way to abort
        If (Me.m_dispatch.IsRunning) Then Return

        If (e.Data.GetDataPresent(DataFormats.FileDrop)) Then
            Dim fs As String() = DirectCast(e.Data.GetData(DataFormats.FileDrop), String())
            If fs.Length = 1 Then
                TryLoadWorkload(fs(0))
            End If
        End If

    End Sub

#End Region ' Overrides

#Region " Control events "

    Private Sub OnConfigure(sender As Object, e As EventArgs) Handles m_tsbnConfig.Click
        Dim dlg As New dlgConfig(Me.m_settings)
        dlg.AddPage("Protocols", GetType(ucConfigureServerProtocols))
        If dlg.ShowDialog(Me) = DialogResult.OK Then
            Me.m_settings.Save()
            Me.UpdateConfigurationStateControls()
        End If
    End Sub

    Private Sub OnBrowseWorkload(sender As Object, e As EventArgs) Handles m_tsbnLoadWorkload.Click

        Dim ofd As New OpenFileDialog()
        ofd.CheckFileExists = True
        ofd.CheckPathExists = True
        ofd.Filter = "Workloads|*.json;*.xml"
        ofd.Multiselect = False
        If ofd.ShowDialog() = DialogResult.OK Then
            Me.TryLoadWorkload(ofd.FileName)
        End If
    End Sub

    Private Sub OnStartServer(sender As Object, e As EventArgs) Handles m_tsbnStartServer.Click
        Me.m_server.StartServer(Me.m_protocol)
        Me.UpdateRunStateControls()
    End Sub

    Private Sub OnStopServer(sender As Object, e As EventArgs) Handles m_tsbnStopServer.Click
        Me.m_server.StopServer()
        Me.UpdateRunStateControls()
    End Sub

#End Region ' Control events

#Region " Framwork events "

    Private Sub OnClientAddedRemoved(sender As Object, args As cClientAddedRemovedEventArgs)
        Select Case args.Status
            Case eClientServerStatus.Connected
                OnAddClient(args.ClientID)
            Case eClientServerStatus.Disconnected
                OnRemoveClient(args.ClientID)
        End Select
    End Sub

    Private Sub OnWorkloadUpdate(sender As cWorkSchedulerStatus, args As EventArgs)
        Try
            Me.BeginInvoke(New MethodInvoker(AddressOf UpdateRunStateControls))
        Catch ex As Exception

        End Try
    End Sub

#End Region ' Framwork events

#Region " Internals - Client updating "

    Private Sub OnAddClient(id As Int32)
        Dim dr As DataGridViewRow = Me.ClientRow(id)
        If (dr Is Nothing) Then
            Me.m_dgvClients.Rows.Add({id, "Connected"})
        Else
            dr.Cells(1).Value = "Is back"
        End If
    End Sub

    Private Sub OnRemoveClient(id As Int32)
        Dim dr As DataGridViewRow = Me.ClientRow(id)
        If (dr IsNot Nothing) Then
            dr.Cells(1).Value = "Disconnected"
        End If
    End Sub

    Private Function ClientRow(id As Int32) As DataGridViewRow
        For Each dr As DataGridViewRow In Me.m_dgvClients.Rows
            If CInt(dr.Cells(0).Value) = id Then Return dr
        Next
        Return Nothing
    End Function

#End Region ' Internals - Client updating

#Region " Internals - Workload "

    Private Sub TryLoadWorkload(fn As String)

        Dim content As String = File.ReadAllText(fn)
        Dim data As cWorkload = cSerializableObject.FromXML(Of cWorkload)(content)
        If (TypeOf data Is cWorkload) Then
            Me.m_dispatch.SetWork(data)
        End If

    End Sub

#End Region ' Internals - Workload

#Region " Internals - UI "

    Private Sub UpdateRunStateControls()

        Dim bIsStarted As Boolean = Me.m_server.IsServerStarted()
        Me.m_tsbnStartServer.Checked = bIsStarted
        Me.m_tsbnStopServer.Checked = Not bIsStarted

        Me.m_tspbWorkload.Visible = Me.m_dispatch.IsRunning

    End Sub

    Private Sub UpdateConfigurationStateControls()

        Me.UpdateProtocolIndicator()
        Me.UpdateFolderAliasIndicator()
        Me.UpdateApplicationAliasIndicator()

    End Sub

    Private Sub UpdateProtocolIndicator()

        If (Me.m_protocol Is Nothing) Then
            Me.m_tslProtocol.Text = "(no protocol)"
        Else
            Me.m_tslProtocol.Text = Me.m_protocol.DisplayName
        End If

    End Sub

    Private Sub UpdateFolderAliasIndicator()

        Me.m_tslFolders.Text = String.Format("{0} folder aliase(s)", Me.m_settings.FolderSettings.Aliases.Count)

    End Sub

    Private Sub UpdateApplicationAliasIndicator()

        Me.m_tslApplications.Text = String.Format("{0} app aliase(s)", Me.m_settings.ApplicationSettings.Aliases.Count)

    End Sub

#End Region ' Internals - UI

End Class
