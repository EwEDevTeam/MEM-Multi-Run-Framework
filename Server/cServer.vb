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
Imports System.Threading
Imports ClientServer

#End Region ' Imports

''' =============================================================================
''' <summary>
''' Server class for the multi-run framework.
''' </summary>
''' =============================================================================
Public Class cServer

#Region " Private vars "

    ''' <summary>Thread marshalling for events</summary>
    Private m_syncObject As SynchronizationContext = Nothing

    ''' <summary>Connected clients and the respective protocols that the client is connected to</summary>
    Private m_clients As New Dictionary(Of Int32, cDataExchangeProtocol)

    ''' <summary>One or more data exchange protocols started on this server</summary>
    Private m_protocols As New List(Of cDataExchangeProtocol)

#End Region ' Private vars

#Region " Construction / destruction "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="iID">ID of the server which has to be unique on the 
    ''' multi-run framework that is being instantiated.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(iID As Int32)

        Me.ID = iID
        Me.m_clients.Clear()

        ' Get sync object
        Me.m_syncObject = SynchronizationContext.Current
        If (Me.m_syncObject Is Nothing) Then
            Me.m_syncObject = New SynchronizationContext()
        End If

    End Sub

#End Region ' Construction / destruction

#Region " Public access "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' The server ID
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property ID As Integer

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Start a server for a given <see cref="cDataExchangeProtocol"/>
    ''' </summary>
    ''' <param name="protocol"></param>
    ''' <returns>
    ''' True if the protocol was a valid server-side protocol, and was 
    ''' able to <see cref="cDataExchangeProtocol.StartServer()">start</see>.
    ''' </returns>
    ''' <seealso cref="IsServerStarted()"/>
    ''' <seealso cref="StopServer()"/>

    ''' -----------------------------------------------------------------------
    Public Function StartServer(protocol As cDataExchangeProtocol) As Boolean

        Return Me.StartServer(New cDataExchangeProtocol() {protocol})

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Start a server for a <see cref="cDataExchangeProtocol"/> collection. If
    ''' the server was already started, it will be stopped first.
    ''' </summary>
    ''' <param name="protocols">One or more protocols of different Type.</param>
    ''' <returns>
    ''' True if all provided protocols were valid server-side protocols, and 
    ''' were of different Type, and were able to <see cref="cDataExchangeProtocol.StartServer()">
    ''' start</see>.
    ''' </returns>
    ''' <seealso cref="IsServerStarted()"/>
    ''' <seealso cref="StopServer()"/>
    ''' -----------------------------------------------------------------------
    Public Function StartServer(protocols As IEnumerable(Of cDataExchangeProtocol)) As Boolean

        Dim lAccepted As New List(Of cDataExchangeProtocol)

        For Each protocol As cDataExchangeProtocol In protocols

            Dim bAccept As Boolean = False
            If (protocol IsNot Nothing) Then
                If (protocol.IsServer) Then
                    Dim bAlreadyExists As Boolean = False
                    For Each p As cDataExchangeProtocol In lAccepted
                        If p.GetType().Equals(protocol.GetType()) Then
                            bAlreadyExists = True
                        End If
                    Next
                    bAccept = Not bAlreadyExists
                End If
            End If
            If bAccept Then lAccepted.Add(protocol)
        Next

        If (lAccepted.Count = 0) Then Return False

        Me.StopServer()

        Dim bStarted As Boolean = True
        For Each protocol In lAccepted
            Me.m_protocols.Add(protocol)
            AddHandler protocol.OnStatus, AddressOf OnStatusReceived
            AddHandler protocol.OnData, AddressOf OnDataReceived
            bStarted = bStarted And protocol.StartServer()
        Next

        Return bStarted

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Stop a server.
    ''' </summary>
    ''' <seealso cref="IsServerStarted()"/>
    ''' <seealso cref="StartServer(cDataExchangeProtocol)()"/>
    ''' <seealso cref="StartServer()"/>
    ''' -----------------------------------------------------------------------
    Public Sub StopServer()

        For Each protocol In Me.m_protocols
            protocol.StopServer()
            RemoveHandler protocol.OnStatus, AddressOf OnStatusReceived
            RemoveHandler protocol.OnData, AddressOf OnDataReceived
        Next
        Me.m_protocols.Clear()

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether this server has been started.
    ''' </summary>
    ''' <returns></returns>
    ''' <seealso cref="StartServer(cDataExchangeProtocol)()"/>
    ''' <seealso cref="StartServer()"/>
    ''' <seealso cref="StopServer()"/>
    ''' -----------------------------------------------------------------------
    Public Function IsServerStarted() As Boolean

        Return (Me.m_protocols.Count > 0)

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns an array of IDs of connected clients.
    ''' </summary>
    ''' <returns>An array of connected client IDs.</returns>
    ''' -----------------------------------------------------------------------
    Public Function Clients() As Int32()
        Return Me.m_clients.Keys.ToArray()
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns an array of cDataExchangeProtocols used.
    ''' </summary>
    ''' <returns>An array of cDataExchangeProtocols used.</returns>
    ''' -----------------------------------------------------------------------
    Public Function Protocols() As cDataExchangeProtocol()
        Return Me.m_protocols.ToArray()
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Send data to a client.
    ''' </summary>
    ''' <param name="data">The data to send.</param>
    ''' <param name="recipient">The ID of the client to send data to.</param>
    ''' <returns>
    ''' True if data was sent.
    ''' </returns>
    ''' -----------------------------------------------------------------------
    Public Function Send(data As cSerializableObject, recipient As Int32) As Boolean

        If Not Me.m_clients.ContainsKey(recipient) Then Return False

        Dim protocol As cDataExchangeProtocol = Me.m_clients(recipient)
        If (protocol Is Nothing) Then Return False
        If (protocol.IsConnectedTo(recipient)) Then
            protocol.Send(data, recipient)
        End If
        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event that is sent out whenever a client has joined or has left.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="args"></param>
    ''' -----------------------------------------------------------------------
    Public Event OnClientAddedRemoved(sender As Object, args As cClientAddedRemovedEventArgs)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event that is sent out whenever data has arrived from a given client.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="args"></param>
    ''' -----------------------------------------------------------------------
    Public Event OnClientData(sender As Object, args As cClientDataEventArgs)

#End Region ' Public access

#Region " Internal event handling "

    Private Sub OnStatusReceived(sender As cDataExchangeProtocol, status As eClientServerStatus, source As Int32)

        If Not Me.m_protocols.Contains(sender) Then Return

        Select Case status
            Case eClientServerStatus.Connected
                Debug.WriteLine("Server: {0} connected to {1}", sender.ID, source)
                If (Not Me.m_clients.ContainsKey(source)) Then
                    Me.m_clients(source) = sender
                End If
                Me.SendClientAddedRemovedEvent(source, eClientServerStatus.Connected, sender.Name)

            Case eClientServerStatus.Disconnected
                Debug.WriteLine("Server: {0} disconnected from {1}", sender.ID, source)
                Me.m_clients.Remove(source)
                Me.SendClientAddedRemovedEvent(source, eClientServerStatus.Disconnected, sender.Name)

            Case Else
                Debug.Assert(False, "Unsupported status receved " & status)
        End Select
    End Sub

    Private Sub OnDataReceived(sender As cDataExchangeProtocol, data As cSerializableObject, source As Int32)
        Me.SendClientDataEvent(source, data)
    End Sub

#End Region ' Internal event handling

#Region " Internal event sending "

    Protected Sub SendClientAddedRemovedEvent(id As Int32, status As eClientServerStatus, protocolname As String)
        Me.m_syncObject.Send(New SendOrPostCallback(AddressOf DoSendClientAddedRemovedEvent),
                             New cClientAddedRemovedEventArgs(id, status, protocolname))
    End Sub

    Private Sub DoSendClientAddedRemovedEvent(obj As Object)
        Try
            RaiseEvent OnClientAddedRemoved(Me, DirectCast(obj, cClientAddedRemovedEventArgs))
        Catch ex As Exception

        End Try
    End Sub

    Protected Sub SendClientDataEvent(id As Int32, data As cSerializableObject)
        Me.m_syncObject.Send(New SendOrPostCallback(AddressOf DoSendClientDataEvent),
                             New cClientDataEventArgs(id, data))
    End Sub

    Private Sub DoSendClientDataEvent(obj As Object)
        Try
            RaiseEvent OnClientData(Me, DirectCast(obj, cClientDataEventArgs))
        Catch ex As Exception

        End Try
    End Sub

#End Region ' Internal event sending

End Class
