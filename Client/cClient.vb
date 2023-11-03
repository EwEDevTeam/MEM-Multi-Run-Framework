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

Public Class cClient

    Private m_protocol As cDataExchangeProtocol = Nothing
    Private m_runner As cJobRunner = Nothing

    Public Sub New(iID As Int32)
        Me.ID = iID
    End Sub

    ReadOnly Property ID As Integer

    Public Function Connect(p As cDataExchangeProtocol) As Boolean

        Me.Disconnect()

        Me.m_protocol = p
        AddHandler Me.m_protocol.OnData, AddressOf OnDataReceived
        AddHandler Me.m_protocol.OnStatus, AddressOf OnStatusReceived
        Me.m_protocol.ConnectToServer()

        Return Me.IsConnected

    End Function

    Public Function Disconnect() As Boolean

        If (Me.m_protocol IsNot Nothing) Then
            RemoveHandler Me.m_protocol.OnData, AddressOf OnDataReceived
            RemoveHandler Me.m_protocol.OnStatus, AddressOf OnStatusReceived
            Me.m_protocol = Nothing
        End If

        Return Not Me.IsConnected

    End Function

    Public Function IsConnected() As Boolean
        If (Me.m_protocol Is Nothing) Then Return False
        Return Me.m_protocol.IsConnectedToServer
    End Function

    Private Sub OnStatusReceived(protocol As cDataExchangeProtocol, status As eClientServerStatus, sender As Int32)
        Debug.WriteLine("Client {0} received status {1}", ID, status)
    End Sub

    Private Sub OnDataReceived(protocol As cDataExchangeProtocol, data As cSerializableObject, sender As Int32)

        Debug.Assert(sender <> Me.ID)

        Debug.WriteLine("Client {0} received data {1}", Me.ID, data.ToString)

        If (Me.m_runner Is Nothing) Then
            Debug.Assert(False)
            Return
        End If

        If (TypeOf data Is cJobRunRequest) Then
            Me.m_runner.Run(sender, DirectCast(data, cJobRunRequest))
        End If

    End Sub

    Public Function Send(data As cSerializableObject) As Boolean

        If (Me.m_protocol Is Nothing) Then Return False
        Me.m_protocol.Send(data, Me.m_protocol.ServerID)
        Return True

    End Function

#Region " JobRunner integration "

    Public Property JobRunner As cJobRunner
        Get
            Return Me.m_runner
        End Get
        Set(value As cJobRunner)
            If (Me.m_runner IsNot Nothing) Then
                RemoveHandler Me.m_runner.OnJobRunStatusChanged, AddressOf OnJobRunStatusChanged
            End If

            Me.m_runner = value

            If (Me.m_runner IsNot Nothing) Then
                AddHandler Me.m_runner.OnJobRunStatusChanged, AddressOf OnJobRunStatusChanged
            End If
        End Set
    End Property

    Private Sub OnJobRunStatusChanged(sender As cJobRunner, args As cJobRunnerStatusEventArgs)
        ' Just inform the server
        Me.Send(args.SerializableUpdate)
    End Sub

#End Region ' JobRunner integration

End Class
