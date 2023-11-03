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

#End Region ' Imports

' VERBOSE_LEVEL feedback:
'   0: no console feedback
'   1: Main info, main failures
'   2: Status updates
'   3: Data details (for the hardcore debuggers)

''' ===========================================================================
''' <summary>
''' Base class for implementing thread-safe data exchange protocols.
''' </summary>
''' <remarks>
''' <para>A few basic principles to implmenting data exchange protocols:</para>
''' <para>One server connects to multiple clients. Both server and clients
''' inherit from the same base class, and both can seek connections via
''' <see cref="cDataExchangeProtocol.SeekConnections()"/></para>
''' <para>The run framework is built on the premises that computational clients
''' may come and go; the client/server network is not of a fixed structure.</para>
''' <para>A server is launched first, and clients seek connections with the
''' server. The server will not reconnect with clients; this is the responsibility
''' of the clients.</para>
''' <para>Connections will be polled through Quality of Service (QoS) data
''' to ensure that the framework connections are reliably functioning.</para>
''' </remarks>
''' ===========================================================================
Public MustInherit Class cDataExchangeProtocol

#Region " Private vars "

    ''' <summary>Thread marshalling thingummy.</summary>
    Private m_syncObject As SynchronizationContext = Nothing

    ''' <summary>Dual purpose dictionary, remembers client IDs and the last 
    ''' received connection ping.</summary>
    ''' <remarks>This can be improved by having a more detailed record per 
    ''' client that can host other info too.</remarks>
    Private m_clientPings As New Dictionary(Of Int32, DateTime)

    Private m_bServerStarted As Boolean = False

    ''' <summary>Internal message counter. Just incremental, and for now happily 
    ''' ignoring the fact that there is an upper limit to ULong. Yippee.</summary>
    Private m_iNextMessageID As ULong = 1

    ''' <summary>ID of the server.</summary>
    Protected m_serverID As Int32 = 0

    ' -- QoS --

    ''' <summary>Time step of the last received server ping for a client.</summary>
    Protected m_serverPing As DateTime = DateTime.MinValue

    ''' <summary>Threaded timer to ping the connection status.</summary>
    Private m_pingtimer As New Timer(AddressOf OnPingTimerElapsed, Nothing, Timeout.Infinite, Timeout.Infinite)

#End Region ' Private vars

#Region " Generics "

    Public Sub New(iID As Int32, bServer As Boolean)

        Me.ID = iID
        Me.IsServer = bServer

        Me.Name = "dep"

        ' Get sync object
        Me.m_syncObject = SynchronizationContext.Current
        If (Me.m_syncObject Is Nothing) Then
            Me.m_syncObject = New SynchronizationContext()
        End If

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the machine ID for this side of the protocol.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property ID As Int32 = 0

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether this side of the protocol is a server.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property IsServer As Boolean = False

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' For display in the UI.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property DisplayName As String = ""

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Protocol naming for persistence and debugging purposes.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Name As String = ""

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether this data protocol is sufficiently parameterized for use.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public MustOverride Function IsConfigured() As Boolean

    Public Overridable Function Load(settings As cXMLSettings, section As String) As Boolean
        Return (settings IsNot Nothing)
    End Function

    Public Overridable Function Save(settings As cXMLSettings, section As String) As Boolean
        Return (settings IsNot Nothing)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' The default ping interval for the given data exchange protocol
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public MustOverride ReadOnly Property DefaultPingInterval As Long

    ''' -----------------------------------------------------------------------
    ''' <summary>
    '''Get/set the life span ping interval, in MS.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property PingTimerInterval As Long = Me.DefaultPingInterval

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Yeah.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overrides Function ToString() As String
        Return Me.DisplayName()
    End Function

#End Region ' Generics

#Region " Events "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Public event reporting that data has been received via the data exchange protocol.
    ''' </summary>
    ''' <param name="sender">The <see cref="cDataExchangeProtocol">instance</see>
    ''' instance that received the data.</param>
    ''' <param name="data">The <see cref="cSerializableObject">data</see>
    ''' that was received.</param>
    ''' -------------------------------------------------------------------
    Public Event OnData(sender As cDataExchangeProtocol, data As cSerializableObject, source As Int32)

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Public event reporting data exchange protocol status changes.
    ''' </summary>
    ''' <param name="sender">The <see cref="cDataExchangeProtocol">instance</see>
    ''' instance that sent the event.</param>
    ''' <param name="status">The <see cref="eClientServerStatus">status</see>
    ''' of the socket wrapper that sent the event.</param>
    ''' -------------------------------------------------------------------
    Public Event OnStatus(sender As cDataExchangeProtocol, status As eClientServerStatus, source As Int32)

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="status"></param>
    ''' <param name="sender">The <see cref="cDataExchangeProtocol.ID"/> of the sender.</param>
    ''' <param name="message">The <see cref="cDataPackage.Sequence"/> of the data package.</param>
    ''' -------------------------------------------------------------------
    Protected Sub SendThreadSafeStatusEvent(status As eClientServerStatus, sender As Int32)
        Me.m_syncObject.Send(New SendOrPostCallback(AddressOf DoSendEvent),
                             New cDataExchangeProtocolEventArgs(cDataExchangeProtocolEventArgs.eEventType.Status, status, sender, 0))
    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="sender">The <see cref="cDataExchangeProtocol.ID"/> of the sender.</param>
    ''' <param name="message">The <see cref="cDataPackage.Sequence"/> of the data package.</param>
    ''' -------------------------------------------------------------------
    Protected Sub SendThreadSafeDataEvent(data As cSerializableObject, sender As Int32, message As ULong)
        Me.m_syncObject.Send(New SendOrPostCallback(AddressOf DoSendEvent),
                             New cDataExchangeProtocolEventArgs(cDataExchangeProtocolEventArgs.eEventType.Data, data, sender, message))
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Post-thead marshalling event broadcasting
    ''' </summary>
    ''' <param name="obj"></param>
    ''' -----------------------------------------------------------------------
    Private Sub DoSendEvent(obj As Object)

        Debug.Assert(TypeOf obj Is cDataExchangeProtocolEventArgs)
        Dim info As cDataExchangeProtocolEventArgs = DirectCast(obj, cDataExchangeProtocolEventArgs)

        Select Case info.EventType

            Case cDataExchangeProtocolEventArgs.eEventType.Data
                RaiseEvent OnData(Me, info.Data, info.Sender)

            Case cDataExchangeProtocolEventArgs.eEventType.Status
                RaiseEvent OnStatus(Me, info.Status, info.Sender)

        End Select

    End Sub

#End Region ' Events 

#Region " Server "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Start the protocol on the server side.
    ''' </summary>
    ''' <returns></returns>
    ''' <seealso cref="StopServer()"/>
    ''' <seealso cref="IsServerStarted()"/>
    ''' -------------------------------------------------------------------
    Public Overridable Function StartServer() As Boolean

        If Not Me.IsConfigured Then
            Debug.Assert(False, "Data exchange protocol not configured")
            Return False
        End If

        If Not Me.IsServer Then
            Debug.Assert(False, "Cannot start client as server")
            Return False
        End If

        Me.m_bServerStarted = True
        Return Me.IsServerStarted()

    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Stop the server. This will <see cref="RemoveClient(Integer)">remove 
    ''' all the clients</see>, too.
    ''' </summary>
    ''' <returns></returns>
    ''' <seealso cref="StartServer()"/>
    ''' <seealso cref="IsServerStarted()"/>
    ''' -------------------------------------------------------------------
    Public Overridable Function StopServer() As Boolean

        If Not Me.IsServer Then
            Debug.Assert(False, "Cannot stop client as server")
            Return False
        End If

        ' Disconnect clients
        Dim clients As Int32() = Me.ClientIDs
        For Each ID As Int32 In clients
            Me.RemoveClient(ID)
        Next

        Me.m_bServerStarted = False
        Return Not Me.IsServerStarted()

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get whether this server has been started.
    ''' </summary>
    ''' <returns></returns>
    ''' <seealso cref="StartServer()"/>
    ''' <seealso cref="StopServer()"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property IsServerStarted As Boolean
        Get
            Return Me.m_bServerStarted
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Add a client ID to the list of available clients. Upon success, this will 
    ''' broadcast the <see cref="eClientServerStatus.Connected"/> event.
    ''' </summary>
    ''' <param name="id">The client ID to add.</param>
    ''' <returns>True if this is a new client, false otherwise.</returns>
    ''' -----------------------------------------------------------------------
    Protected Overridable Function AddClient(id As Integer) As Boolean

        Dim bSuccess As Boolean = False

        If Not Me.IsConfigured Then
            Debug.Assert(False, "Data exchange protocol not configured")
            Return bSuccess
        End If

        If Not Me.IsServer Then
            Debug.Assert(False, "Cannot add client to a client")
            Return bSuccess
        End If

        SyncLock (Me.m_clientPings)
            If Not Me.m_clientPings.ContainsKey(id) Then
                ' #Yes: let the client know it's being added
                Me.Send(New cQoS(eQoS.Connect), id)
                ' Add to internal admin
                Me.m_clientPings(id) = Date.Now.ToUniversalTime
                ' Yeah
                bSuccess = True
            End If
        End SyncLock

        If (bSuccess) Then
            'Track(String.Format("{0} accepted client {1}", Me, id))
            Me.SendThreadSafeStatusEvent(eClientServerStatus.Connected, id)
        End If
        Return bSuccess

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Remove the ID of a connected client from the list of available clients. 
    ''' Upon success, this will broadcast the <see cref="eClientServerStatus.Disconnected"/> event.
    ''' </summary>
    ''' <param name="id">The client ID to remove.</param>
    ''' <returns>True if this client was removed, false otherwise.</returns>
    ''' -----------------------------------------------------------------------
    Protected Overridable Function RemoveClient(id As Integer) As Boolean

        Dim bSuccess As Boolean = False

        If Not Me.IsServer Then
            Debug.Assert(False, "Cannot remove client from a client")
            Return bSuccess
        End If

        SyncLock (Me.m_clientPings)
            ' Is a connected client?
            If Me.m_clientPings.ContainsKey(id) Then
                ' #Yes: let the client know it's going to go
                Me.Send(New cQoS(eQoS.Disconnect), Me.ID)
                ' Remove client from the internal admin
                Me.m_clientPings.Remove(id)
                ' Yeah
                bSuccess = True
            End If
        End SyncLock

        If (bSuccess) Then
            'Track(String.Format("{0} removed client {1}", Me, id))
            Me.SendThreadSafeStatusEvent(eClientServerStatus.Disconnected, id)
        End If
        Return bSuccess

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the IDs of all connected clients.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Protected ReadOnly Property ClientIDs As Int32()
        Get
            Return Me.m_clientPings.Keys.ToArray()
        End Get
    End Property

#End Region ' Server

#Region " Client "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Get the ID of the connected server.
    ''' </summary>
    ''' <returns></returns>
    ''' -------------------------------------------------------------------
    Public ReadOnly Property ServerID As Int32
        Get
            Return Me.m_serverID
        End Get
    End Property

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Attempt to connect a client to a server via the data exchange protocol 
    ''' using protocol-specific configuration settings. 
    ''' </summary>
    ''' <returns>True if connected succesfully.</returns>
    ''' -------------------------------------------------------------------
    Public Overridable Function ConnectToServer() As Boolean

        If Me.IsServer() Then
            Debug.Assert(False, "Cannot connect server to server")
            Return False
        End If

        Me.Send(New cQoS(eQoS.Connect))
        Return True

    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' States whether the exchange protocol is connected.
    ''' </summary>
    ''' <returns>True if connected.</returns>
    ''' -------------------------------------------------------------------
    Public Overridable Function IsConnectedToServer() As Boolean

        Return (Me.m_serverID <> 0)

    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Attempt to disconnect the exchange protocol from the server, and 
    ''' notify the server that the client will be gone.
    ''' </summary>
    ''' <returns>True if disconnected succesfully.</returns>
    ''' -------------------------------------------------------------------
    Public Overridable Function DisconnectFromServer() As Boolean

        If Me.IsServer() Then
            Debug.Assert(False, "Cannot connect server to server")
            Return False
        End If

        ' Nothing to disconnect from?
        If (Me.m_serverID = 0) Then Return False

        ' Inform the server that we're gone 
        Me.Send(New cQoS(eQoS.Disconnect))
        ' Adios
        Me.RemoveServer()

        ' And done
        Return Not Me.IsConnectedToServer()

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Add the ID the server to the local admin. Upon success, this will 
    ''' broadcast the <see cref="eClientServerStatus.Connected"/> event.
    ''' </summary>
    ''' <returns>True if this server was connected, false otherwise.</returns>
    ''' -----------------------------------------------------------------------
    Protected Overridable Function AddServer(id As Integer) As Boolean

        If Me.IsServer Then
            Debug.Assert(False, "Cannot add server to a server")
            Return False
        End If

        If Me.m_serverID <> 0 Then
            Return False
        End If
        Me.m_serverID = id

        'Track(String.Format("{0} accepted server {1}", Me, id))
        Me.SendThreadSafeStatusEvent(eClientServerStatus.Connected, id)
        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Remove the ID the server from the local admin. Upon success, this will 
    ''' broadcast the <see cref="eClientServerStatus.Disconnected"/> event.
    ''' </summary>
    ''' <returns>True if this server was disconnected, false otherwise.</returns>
    ''' -----------------------------------------------------------------------
    Protected Overridable Function RemoveServer() As Boolean

        If Me.IsServer Then
            'Debug.Assert(False, "Cannot remove server from a server")
            Return False
        End If

        If (Me.m_serverID = 0) Then Return False

        ' Forget
        Me.m_serverID = 0

        'Track(String.Format("{0} removed server {1}", Me, ID))
        Me.SendThreadSafeStatusEvent(eClientServerStatus.Disconnected, ID)

        Return True

    End Function

#End Region ' Client

#Region " Generics "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Gets whether this protocol instance is connected to a remote ID, either
    ''' as Client or as Server.
    ''' </summary>
    ''' <param name="id">The client or server <see cref="cDataExchangeProtocol.ID"/>
    ''' </param>
    ''' <returns>True if connected.</returns>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property IsConnectedTo(id As Int32) As Boolean
        Get
            If Me.IsServer Then
                Return Me.m_clientPings.ContainsKey(id)
            End If
            Return Me.m_serverID = id
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Send data across to protocol.
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="recipient"></param>
    ''' <param name="reply">Optional message sequence that this message replies to.</param>
    ''' <returns>The <see cref="cDataPackage.Sequence">data package sequence</see>
    ''' sent, or 0 if an error occurred.</returns>
    ''' <remarks>
    ''' This call wraps the data sent into a <see cref="cDataPackage"/>
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Function Send(data As cSerializableObject, Optional recipient As Int32 = 0, Optional reply As ULong = 0) As ULong

        Dim lID As ULong = 0
        SyncLock (Me)
            If Me.SendThroughProtocol(New cDataPackage(Me.m_iNextMessageID, data, Me.ID, recipient, reply)) Then
                lID = Me.m_iNextMessageID
                Me.m_iNextMessageID = CULng(Me.m_iNextMessageID + 1)
            End If
        End SyncLock
        Return lID

    End Function

#End Region ' Generics

#Region " Internals "

    Protected Function CanSend(obj As cDataPackage) As Boolean

        If (obj Is Nothing) Then Return False
        If (obj.Data Is Nothing) Then Return False

        ' I am not talking to myself. At least not in public
        If (obj.Recipient = obj.Sender) Then Return False

        If (TypeOf obj.Data Is cQoS) Then Return True

        If Me.IsServer Then Return (Me.m_clientPings.Count > 0)
        Return (Me.m_serverID <> 0)

    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Send data via the exchange protocol.
    ''' </summary>
    ''' <param name="obj">the data to send.</param>
    ''' <returns>True if sent correctly</returns>
    ''' -------------------------------------------------------------------
    Protected MustOverride Function SendThroughProtocol(obj As cDataPackage) As Boolean

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Process received data. Invoke this method to process and distribute any
    ''' data received by a protocol for proper, standardized handling and 
    ''' dispatching.
    ''' </summary>
    ''' <param name="obj">The data to process.</param>
    ''' -----------------------------------------------------------------------
    Protected Sub ProcessIncomingData(obj As cDataPackage)

        ' Ignore my own
        If (obj.Sender = Me.ID) Then Return

        ' Eavesdrop on all traffic that we didn't send out
        ' Note that recording last activity can be used to auto-connect clients
        Me.RecordLastActivity(obj.Sender, Date.Now.ToUniversalTime)

        ' Not for me?
        If (obj.Recipient <> Me.ID And obj.Recipient <> 0) Then Return

        ' Handle Quality of Service traffic
        If (TypeOf (obj.Data) Is cQoS) Then

            Select Case DirectCast(obj.Data, cQoS).QoS

                Case eQoS.Connect
                    If (Me.IsServer) Then
                        ' Reply to client to confirm connection 
                        Me.Send(New cQoS(eQoS.Connect), obj.Sender, obj.Sequence)
                        ' Process locally
                        Me.AddClient(obj.Sender)
                    Else
                        ' Process locally
                        Me.AddServer(obj.Sender)
                    End If

                Case eQoS.Disconnect
                    If (Me.IsServer) Then
                        ' Process locally
                        Me.RemoveClient(obj.Sender)
                    Else
                        ' Process locally
                        Me.RemoveServer()
                    End If

                Case eQoS.Ping
                    If (Not Me.IsServer) Then
                        If (Me.m_serverID = obj.Sender) Then
                            ' Reply to ping request sent by server to show that we're still alive
                            Me.Send(New cQoS(eQoS.Ping), 0, obj.Sequence)
                        End If
                    End If

            End Select

            ' Done; QoS data never surfaces directly
            Return

        End If

        Try
            Me.SendThreadSafeDataEvent(obj.Data, obj.Sender, obj.Sequence)
        Catch ex As Exception
            'Track(String.Format("{0} event exception '{1}' ", Me, ex.Message))
        End Try

    End Sub

#End Region ' Internals

#Region " Pinging "

    Protected Sub RecordLastActivity(id As Int32, dt As Date)

        If (Me.IsServer()) Then
            If (Me.m_clientPings.ContainsKey(id)) Then
                Me.m_clientPings(id) = New Date(Math.Max(dt.Ticks, Me.m_clientPings(id).Ticks))
            End If
        Else
            If (id = Me.ServerID) Then
                Me.m_serverPing = New Date(Math.Max(dt.Ticks, Me.m_serverPing.Ticks))
            End If
        End If
    End Sub

    Protected Sub StartConnectionPinging()

        ' Activate timer
        Me.m_pingtimer.Change(Me.PingTimerInterval, Me.PingTimerInterval)
        Debug.WriteLine(String.Format("{0} started QoS ping", Me))
    End Sub

    Protected Sub StopConnectionPinging()

        ' Stop timer
        Me.m_pingtimer.Change(Timeout.Infinite, Timeout.Infinite)
        ' Clear old data
        If (Me.IsServer) Then Me.m_clientPings.Clear()
        Debug.WriteLine(String.Format("{0} stopped QoS ping", Me))

    End Sub

    Private Sub OnPingTimerElapsed(state As Object)

        Dim utmNow As DateTime = Date.Now.ToUniversalTime
        Dim lUnresponsive As New List(Of Int32)

        If (Me.IsServer) Then
            ' Server pings the clients
            SyncLock (Me.m_clientPings)
                For Each ID As Int32 In Me.m_clientPings.Keys
                    Dim elapsed As TimeSpan = utmNow - Me.m_clientPings(ID)
                    If (elapsed.TotalMilliseconds > (Me.PingTimerInterval * 2)) Then
                        Debug.WriteLine(String.Format("{0} client {1} unresponsive", Me, ID))
                        lUnresponsive.Add(ID)
                    End If
                Next
            End SyncLock

            ' What to do here? Abort? Remove client?
            For Each ID As Int32 In lUnresponsive
                Me.RemoveClient(ID)
            Next

            ' Send out new ping to all clients
            Me.Send(New cQoS(eQoS.Ping))

        Else
            ' Client simply checks the time elapsed between the last received ping and the timer
            ' If time-out occurred: disconnect
            Dim elapsed As TimeSpan = utmNow - Me.m_serverPing
            If (elapsed.TotalMilliseconds > (PingTimerInterval * 2)) Then
                Debug.WriteLine(String.Format("{0} server unresponsive", Me))
                Me.RemoveServer()
            End If
        End If

    End Sub

#End Region ' Pinging

End Class
