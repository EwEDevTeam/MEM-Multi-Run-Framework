' ===============================================================================
' This file is part of the Marine Ecosystem Model multi-run framework prototype, 
' developed for the PhD thesis of Jeroen Steenbeek (2020-2023) in Marine Sciences 
' at the Polytechnical University of Catalunya.
'
' The MEM multi-run framework prototype integrates with Ecopath with Ecosim (EwE).
'
' EwE is free software: you can redistribute it and/or modify it under the terms
' of the GNU General Public License version 2 as published by the Free Software 
' Foundation.
'
' EwE is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
' without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
' PURPOSE. See the GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License along with EwE.
' If not, see <http://www.gnu.org/licenses/gpl-2.0.html>. 
'
' Copyright 1991- 
'    Ecopath International Initiative, Barcelona, Spain
' ===============================================================================
'

#Region " Imports "

Option Strict On
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports Newtonsoft.Json

#End Region ' Imports

' VERBOSE_LEVEL feedback:
'   0: no console feedback
'   1: Main info, main failures
'   2: Status updates
'   3: Data details (for the hardcore debuggers)

''' =======================================================================
''' <summary>
''' A <see cref="cDataExchangeProtocol"/> using socket-based data exchange.
''' </summary>
''' <remarks>
''' <para>Every new chunk of data sent through the wrapped socket are 
''' preceded by a 4-byte integer indicating the size of the data. Since
''' data sent through a socket is split into packages, the receiving
''' socket will keep combining packages until the original data is
''' reassembled, which is then made available.</para>
''' </remarks>
''' =======================================================================
Public Class cSocketDataExchangeProtocol
    Inherits cDataExchangeProtocol

#Region " Privates "

    ''' <summary>
    ''' Socket wrapper read states
    ''' </summary>
    Private Enum eReadStates
        ''' <summary>Not reading.</summary>
        NotReading
        ''' <summary>Reading data size bytes.</summary>
        ReadingSize
        ''' <summary>Reading data bytes.</summary>
        ReadingObject
        ''' <summary>Data has been read.</summary>
        ObjectRead
    End Enum

    ''' <summary>Size of buffer for receiving data.</summary>
    Private Const cBUFFER_SIZE As Integer = 1024
    ''' <summary>The one buffer for receiving data.</summary>
    Private m_buffer(cSocketDataExchangeProtocol.cBUFFER_SIZE) As Byte
    ''' <summary>The wrapped socket.</summary>
    Private m_socket As Socket = Nothing
    ''' <summary>Size, in number of bytes, of the most recent data package.</summary>
    Private m_iDataSize As Integer = 0
    ''' <summary>Number of bytes read of the most recent data package.</summary>
    Private m_iDataRead As Integer = 0
    ''' <summary>States whether or not the socket connection is authorized for communication.</summary>
    Private m_bAuthorized As Boolean = False

    ''' <summary>Received bytes buffer.</summary>
    Private m_readbuffer() As Byte
    ''' <summary>Buffer for object size </summary>
    Private m_abSizeBuff(4) As Byte

    ''' <summary> Number of bytes in the size buffer m_abSizeBuff </summary>
    Private m_iSizeRead As Integer
    Private m_iQueue As Integer = 0

    Private m_readState As eReadStates = eReadStates.ReadingSize

    Private m_readlock As New System.Threading.Semaphore(1, 1)
    Private m_iNumPendingSend As Integer = 0

    ''' <summary>Serialization settings</summary>
    ''' <seealso cref="Serialize(cSerializableObject)"/>
    ''' <seealso cref="Deserialize(String)"/>
    Private m_settings As JsonSerializerSettings = Nothing

#End Region ' Privates

#Region " Constructor "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Connect to a remote machine by socket.
    ''' </summary>
    ''' <param name="s">The <see cref="Socket"/> to wrap.</param>
    ''' -------------------------------------------------------------------
    Public Sub New(iID As Integer, bIsServer As Boolean)

        MyBase.New(iID, bIsServer)

        ' JSON settings include full type information to re-stablish original objects on deserialization
        Me.m_settings = New JsonSerializerSettings() With {.TypeNameHandling = TypeNameHandling.All}

        Me.m_socket = cSocketDataExchangeProtocol.CreateSocket()
        Me.Name = "sdep"

        If Me.IsConnected Then
            Me.StartListening()
        End If

    End Sub

#End Region ' Constructor

#Region " Public access "

#Region " Connection "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the IP address to connect to.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property IP As IPAddress

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the port on <see cref="IP"/> to connect to.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Port As Integer

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Attempt to connect the to the given <see cref="IP">IP address</see> and
    ''' <see cref="Port"/>.
    ''' </summary>
    ''' <returns>True if connected succesfully.</returns>
    ''' -------------------------------------------------------------------
    Public Overrides Function Connect() As Boolean

        If (Me.IP Is Nothing) Then
#If VERBOSE_LEVEL >= 1 Then
            Console.WriteLine("sdep {0} IP address is not specified")
#End If
            Return False
        End If

        If (Me.Port <= 0) Then
#If VERBOSE_LEVEL >= 1 Then
            Console.WriteLine("sdep {0} Port address is not specified or is invalid")
#End If
            Return False
        End If

        Try
#If VERBOSE_LEVEL >= 2 Then
            Console.WriteLine("sdep {0} attempting to connect to {1}:{2}", Me, Me.IP.ToString(), Me.Port)
#End If
            ' Try to connect
            Me.m_socket.Connect(Me.IP, Me.Port)

        Catch ex As SocketException
#If VERBOSE_LEVEL >= 1 Then
            Console.WriteLine("sdep {0} exception '{1}' while attempting to connect to {2}:{3}", Me, ex.Message, Me.IP.ToString(), Me.Port)
#End If
        End Try

        ' No luck?
        If Not Me.IsConnected Then
            ' #Ouch! Raise event
            Console.WriteLine("sdep {0} failed to connect to {1}:{2}", Me, Me.IP.ToString(), Me.Port)
            Me.SendThreadSafeStatusEvent(eClientServerStatus.Disconnected, Me.ID)
            Return False
        End If

        ' Connected: raise event
#If VERBOSE_LEVEL >= 1 Then
        Console.WriteLine("sdep {0} connected", Me)
#End If
        Me.SendThreadSafeStatusEvent(eClientServerStatus.Connected, Me.ID)
        Me.StartListening()

        Return True
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' States whether the socket is connected.
    ''' </summary>
    ''' <returns>True if connected.</returns>
    ''' -------------------------------------------------------------------
    Public Overrides Function IsConnected() As Boolean
        Return Me.m_socket.Connected
    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Disconnects the socket from a server.
    ''' </summary>
    ''' <returns>True if disconnected succesfully.</returns>
    ''' -------------------------------------------------------------------
    Public Overrides Function Disconnect() As Boolean
        Try
            If Me.m_socket.Connected() Then
                Me.m_socket.Disconnect(True)
            End If
        Catch ex As Exception
            ' Whoopy
        End Try

        ' Create new socket, because we cannot do without one
        Me.m_socket = Nothing
        Me.m_socket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        Return Not Me.m_socket.Connected()
    End Function

#End Region ' Connection

#Region " Sending "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Send <see cref="cSerializableObject">a serializable object</see> across the socket.
    ''' </summary>
    ''' <param name="obj">The object to send.</param>
    ''' <param name="bRequiresAuthorization">Flag indicating whether the socket
    ''' needs to be <see cref="CanSend">Authorized</see> to send this data.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Overrides Function Send(obj As cSerializableObject,
                                   Optional bRequiresAuthorization As Boolean = True) As Boolean

        Dim bSucces As Boolean = False

        ' Sanity check(s)
        If (obj Is Nothing) Then Return bSucces

        Me.m_iNumPendingSend += 1

        Try
#If VERBOSE_LEVEL >= 3 Then
            Console.WriteLine("sdep {0} sending {1}", Me, obj.ToString())
#End If
            bSucces = SendBinary(Me.Serialize(obj), bRequiresAuthorization)
        Catch ex As Exception

            bSucces = False
        End Try

        Me.m_iNumPendingSend -= 1

        Return bSucces

    End Function

#End Region ' Sending

#End Region ' Public access

#Region " Internals "

    Private Shared Function CreateSocket() As Socket

        Dim s As Socket = Nothing

        ' Configure socket for use with IPv6
        If Socket.OSSupportsIPv6 Then
            s = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)

            ' After http://forum.soft32.com/windows/Socket-problem-migrating-Vista-ftopict363802.html
            s.SetSocketOption(SocketOptionLevel.IPv6, DirectCast(27, SocketOptionName), 0)
        Else
            s = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        End If
        Return s

    End Function

#Region " Reading internals "

    Private Sub StartListening()
        ' Start waiting for data
        Me.m_socket.BeginReceive(Me.m_buffer, 0, cSocketDataExchangeProtocol.cBUFFER_SIZE, SocketFlags.None, AddressOf ReceiveCallBack, Me)
    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Read the most recently received chunk of data from the local socket
    ''' <see cref="m_buffer">buffer</see>.
    ''' </summary>
    ''' <param name="iNumBytes">The number of bytes that were received.</param>
    ''' -------------------------------------------------------------------
    Private Function ReadBuffer(iNumBytes As Integer) As List(Of cSerializableObject)

        Dim objData As cSerializableObject = Nothing
        Dim iOffset As Integer = 0
        Dim iChunkSize As Integer = 0
        Dim nSizeCopy As Integer
        Dim lstObs As New List(Of cSerializableObject)
        Dim buffer As Byte() = Nothing

        Try

#If VERBOSE_LEVEL >= 3 Then
            Console.WriteLine("sdep {0} read buffer {1} bytes ", Me, iNumBytes)
#End If

            ' Has data?
            While (iOffset < iNumBytes)
                ' Is this a new message?
                If Me.m_readState = eReadStates.ReadingSize Then
                    ' #Yes: Start new message
                    ' Extract new message length

                    'how many byte should we read from the buffer
                    nSizeCopy = Math.Min(4 - Me.m_iSizeRead, iNumBytes - iOffset)
                    Array.Copy(Me.m_buffer, iOffset, Me.m_abSizeBuff, m_iSizeRead, nSizeCopy)

                    m_iSizeRead += nSizeCopy
                    iOffset += nSizeCopy

#If VERBOSE_LEVEL >= 3 Then
                    Console.WriteLine("sdep {0} read size buffer {1} bytes total of {2}", Me, nSizeCopy, m_iSizeRead)
#End If

                    'is the size buffer full
                    If Me.m_iSizeRead = 4 Then
                        'yes we have all the bytes for the size buffer

                        'how big is this object
                        Me.m_iDataSize = CInt(Me.m_abSizeBuff(0)) +
                                         CInt(Me.m_abSizeBuff(1) * 2 ^ 8) +
                                         CInt(Me.m_abSizeBuff(2) * 2 ^ 16) +
                                         CInt(Me.m_abSizeBuff(3) * 2 ^ 24)

                        ' Allocate size
                        ReDim buffer(Me.m_iDataSize)

                        'clear out the data size counters
                        Me.m_iSizeRead = 0
                        nSizeCopy = 0
                        'change the read state
                        Me.m_readState = eReadStates.ReadingObject

                    End If ' If Me.m_nSizeRead = 4 Then
                End If ' If Me.m_readState = eReadStates.ReadingSize Then

                If Me.m_readState = eReadStates.ReadingObject Then

                    ' Sanity check
                    Debug.Assert(buffer IsNot Nothing)

                    ' Determine number of bytes to read from this block
                    iChunkSize = Math.Min(Me.m_iDataSize - Me.m_iDataRead, iNumBytes - iOffset)

                    ' Copy bytes
                    Array.Copy(Me.m_buffer, iOffset, buffer, Me.m_iDataRead, iChunkSize)

                    'the number of byte read for this object
                    Me.m_iDataRead += iChunkSize
                    ' Update offset
                    iOffset += iChunkSize

#If VERBOSE_LEVEL >= 3 Then
                    Console.WriteLine("sdep {0} read {1} bytes (buffer {2}, read {3} of {4})", Me, iChunkSize, iNumBytes, Me.m_iDataRead, Me.m_iDataSize)
#End If

                    ' Have we read all the bytes for this object?
                    If (Me.m_iDataSize = Me.m_iDataRead) Then
                        ' #Yes: change the readstate
                        Me.m_readState = eReadStates.ObjectRead
                    End If

                End If 'If Me.m_readState = eReadStates.ReadingObject Then

                ' Is entire message read?
                If Me.m_readState = eReadStates.ObjectRead Then
                    ' #Yes: extract transferred binary data
#If VERBOSE_LEVEL >= 2 Then
                    Console.WriteLine("sdep {0} data read, deserializing object", Me)
#End If

                    Try
                        ' Reconstruct data
                        lstObs.Add(Me.Deserialize(buffer))
                    Catch ex As Exception
                        Debug.Assert(False, ex.Message)
                    End Try

                    ' Reset read buffer
                    Me.m_iDataSize = 0
                    Me.m_iDataRead = 0

                    ' Start reading the next bytes in the buffer, which are the size bytes
                    Me.m_readState = eReadStates.ReadingSize
                End If

            End While

        Catch ex As Exception
            Debug.Assert(False, ex.Message)
            Throw New Exception("sdep ReadBuffer() Error: " & ex.Message, ex)
        End Try

        Return lstObs

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Callback for asynchronous socket data reading.
    ''' </summary>
    ''' <param name="ar"></param>
    ''' -----------------------------------------------------------------------
    Private Sub ReceiveCallBack(ar As IAsyncResult)

        ' Retrieve SocketData
        Dim protocol As cSocketDataExchangeProtocol = CType(ar.AsyncState, cSocketDataExchangeProtocol)
        Dim iNumBytes As Integer = -1
        Dim bSendAuthorization As Boolean = False

        Debug.Assert(Object.ReferenceEquals(protocol, Me))

        'list of objects assembled from the buffer in ReadBuffer()
        Dim ObjectsInStream As List(Of cSerializableObject)

        ' Read incoming bytes
        Try
            ' Get block from socket
            iNumBytes = protocol.m_socket.EndReceive(ar)

        Catch ex As SocketException
#If VERBOSE_LEVEL >= 1 Then
            Console.WriteLine("{0} socket exception '{1}'", Me, ex.Message)
#End If
            ' Screw this, I'm out of here
            iNumBytes = 0
        Catch ex As Exception
#If VERBOSE_LEVEL >= 1 Then
            Console.WriteLine("sdep {0} exception '{1}'", Me, ex.Message)
#End If
        End Try

        ' Disconnected message?
        If iNumBytes = 0 Then
            ' #Yes: disconnect the socket
            protocol.Disconnect()

#If VERBOSE_LEVEL >= 1 Then
            Console.WriteLine("sdep {0} disconnected", Me)
#End If
            Try
                Me.SendThreadSafeStatusEvent(eClientServerStatus.Disconnected, Me.ID)
            Catch ex As Exception
#If VERBOSE_LEVEL >= 1 Then
                Console.WriteLine(">> sdep {0} OnStatus(Disconnected) exception '{1}'", Me, ex.Message)
#End If
            End Try
            Return
        End If

        ' get a list of cSerializableObject objects from the buffer
        ObjectsInStream = protocol.ReadBuffer(iNumBytes)

        For Each objRead As cSerializableObject In ObjectsInStream
            Me.ProcessIncomingData(objRead)
        Next objRead

        ' After all data is handled prepare for receiving next chunk
        If iNumBytes > 1 Then
            Try
                protocol.m_socket.BeginReceive(protocol.m_buffer, 0, cSocketDataExchangeProtocol.cBUFFER_SIZE, SocketFlags.None, AddressOf ReceiveCallBack, protocol)
            Catch ex As Exception
                ' TODO: Likely need better feedback, maybe send an event.
                Debug.Assert(False, String.Format("Failed to recieve object.  May be attributed to client or server disconnecting during transfer. {0}", ex.ToString()))
            End Try
        End If

    End Sub

#End Region ' Reading internals

#Region " Sending internals "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Send a message through the socket. This call is blocking (for now)
    ''' </summary>
    ''' <param name="byMessage">The message bytes to send</param>
    ''' <param name="bRequiresAuthorization">Flag indicating whether the socket
    ''' needs to be <see cref="CanSend">Authorized</see> to send this data.</param>
    ''' <remarks>
    ''' This method will prepend the message with 4 bytes stating the
    ''' length of the original message. This will allow the receiving
    ''' end to deduct whether all packets for incoming data have arrived.
    ''' </remarks>
    ''' -------------------------------------------------------------------
    Private Function SendBinary(byMessage As Byte(),
                                Optional bRequiresAuthorization As Boolean = True) As Boolean

        ' Sanity checks
        If (Not Me.IsConnected) Then Return False
        If (bRequiresAuthorization And Not Me.CanSend) Then Return False
        If (byMessage Is Nothing) Then Return False

        Dim byData() As Byte
        Dim iLength As Integer = 0
        iLength = byMessage.Length()

        ReDim byData(4 + iLength - 1)
        byData(0) = CByte(iLength And &HFF)
        byData(1) = CByte((iLength >> 8) And &HFF)
        byData(2) = CByte((iLength >> 16) And &HFF)
        byData(3) = CByte((iLength >> 24) And &HFF)

        Array.Copy(byMessage, 0, byData, 4, iLength)

        Try
#If VERBOSE_LEVEL >= 3 Then
            Me.m_iQueue += (iLength + 4)
            Console.WriteLine("sdep BeginSend() {0} sending {1} bytes (queue size {2})", Me, (iLength + 4), Me.m_iQueue)
#End If

            Me.m_socket.BeginSend(byData, 0, byData.Length, SocketFlags.None, AddressOf Me.SendCallback, Me.m_socket)

        Catch ex As SocketException
            ' Socket has been closed, just fail this attempt and hope the failure is handled well elsewhere
#If VERBOSE_LEVEL >= 1 Then
            Console.WriteLine("sdep BeginSend() {0} failed: {1}", Me, ex.Message)
#End If
            Return False
        Catch ex As Exception
            ' TODO: Likely need better feedback, maybe send an event.
            Debug.Assert(False, "Cannot send message.  Failed at sending Binary" + ex.ToString())
            Return False
        End Try
        Return True

    End Function

    Private Sub SendCallback(ar As IAsyncResult)

        Dim s As Socket = CType(ar.AsyncState, Socket)
        Dim nb As Integer

        Try
            ' Sanity checks
            If s Is Nothing Then Return
            If Not s.Connected Then Return
            nb = s.EndSend(ar)
            m_iNumPendingSend -= 1

#If VERBOSE_LEVEL >= 3 Then
            Me.m_iQueue -= (nb)
            Console.WriteLine("sdep SendCallback() {0} received {1} bytes (queue size {2})", Me, nb, Me.m_iQueue)
#End If
        Catch ex As Exception
            ' Woops!
            Debug.Assert(False, ex.Message)
        Finally

        End Try

    End Sub

#End Region ' Sending internals


    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Serialize an object to protocol sync file text.
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function Serialize(obj As cSerializableObject) As Byte()

        Dim ms As New MemoryStream()
        Try
            Using bs As New Bson.BsonDataWriter(ms)
                Dim s As JsonSerializer = JsonSerializer.Create(Me.m_settings)
                s.Serialize(bs, obj)
            End Using
            ms.Seek(0, 0) ' Should not be necessary
        Catch ex As Exception

        End Try
        Return ms.GetBuffer()

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Deserialize an object from protocol sync file text.
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function Deserialize(obj As Byte()) As cSerializableObject

        Dim result As cSerializableObject = Nothing

        Try
            Dim ms As New MemoryStream(obj)
            Using bs As New Newtonsoft.Json.Bson.BsonDataReader(ms)
                Dim s As JsonSerializer = JsonSerializer.Create(Me.m_settings)
                result = s.Deserialize(Of cSerializableObject)(bs)
            End Using
        Catch ex As Exception

        End Try

        Return result

    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Returns a string representation of a cSocketWrapper instance.
    ''' </summary>
    ''' <returns>
    ''' A string representing the Remote End Point that the wrapped socket 
    ''' is connected to.
    ''' </returns>
    ''' -------------------------------------------------------------------
    Public Overrides Function ToString() As String
        If Me.m_socket.RemoteEndPoint Is Nothing Then Return "(not connected)"
        Return Me.m_socket.RemoteEndPoint.ToString
    End Function

    Protected Overrides Function SendThroughProtocol(obj As cDataPackage) As Boolean
        Throw New NotImplementedException()
    End Function

#End Region ' Internals

#Region " Disabled code "
#If 0 Then ' DISABLED FOR TESTING PURPOSES. THIS TYPE OF MAGIC CAN WAIT UNTIL LATER

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Attempt to connect the socket to a URI, port combination.
    ''' </summary>
    ''' <param name="strURI">The URI to connect to.</param>
    ''' <param name="iPort">The IP port to connect to.</param>
    ''' <returns>True if connected succesfully.</returns>
    ''' -------------------------------------------------------------------
    Public Overloads Function Connect(strURI As String, iPort As Integer) As Boolean
        Dim aIP As IPAddress() = Nothing
        Try
            Dim ipEntry As IPHostEntry = Dns.GetHostEntry(strURI)
            aIP = ipEntry.AddressList
        Catch ex As Exception
#If VERBOSE_LEVEL >= 1 Then
            Console.Write("sdep {0}: exception '{1}' occurred while attempting to connect to {2}:{3}",
                          Me, ex.Message, strURI, iPort)
#End If
        End Try

        If (aIP Is Nothing) Then Return False
        If (aIP.Length = 0) Then Return False

        ' Attempt to any host entry
        For Each ip As IPAddress In aIP
            If Me.Connect(ip, iPort) Then Return True
        Next
        Return False

    End Function

#End If
#End Region ' Disabled code

End Class

