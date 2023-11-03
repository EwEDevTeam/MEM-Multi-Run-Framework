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

Imports System.IO
Imports Newtonsoft.Json

#End Region ' Imports

' ToDo:
' - Introduce queueing if file conflicts arise
' - All files should be SHA128 checksummed, somehow
' - Add file deletion timer

' VERBOSE_LEVEL implementation:
'   0: no console feedback
'   1: Main info, main failures
'   2: Status updates
'   3: Data details (for the hardcore debuggers)

''' ===========================================================================
''' <summary>
''' A <see cref="cDataExchangeProtocol"/> using file-based data exchange between
''' two machines with known IDs.
''' </summary>
''' <remarks>
''' <para>
''' This protocol relies on file-based communication to synchronize client/server
''' communication. Albeit clunky and slow, this type of communication is most
''' basic and easy to setup via cloud storage providers such as Dropbox, Sync.com, 
''' Google Drive, Box, pCloud, Icedrive, MEGA, IDrive, SpiderOak, MediaFire, 
''' Degoo, Yandex.Disk, Blomp,  Letsupload, Internxt, Jumpshare, TeraBox, and 
''' yeah, OneDrive too.
''' </para><para>
''' The Dropbox development team advised to keep all data exchange within the 
''' same account in order not to exceed data limits. It is safe to presume that 
''' this applies to other cloud storage providers, too.
''' </para>
''' </remarks>
''' ===========================================================================
Public Class cFileDataExchangeProtocol
    Inherits cDataExchangeProtocol

#Region " Private vars "

    ''' <summary>The <see cref="cFolderContentWatcher"/> to rely on</summary>
    Private m_watcher As cFolderContentWatcher = Nothing

    ''' <summary>Serialization settings</summary>
    ''' <seealso cref="Serialize(cSerializableObject)"/>
    ''' <seealso cref="Deserialize(String)"/>
    Private m_settings As JsonSerializerSettings = Nothing

    ''' <summary>Time tick (in UTM) to start processing data.</summary>
    Private m_utmStart As DateTime = DateTime.MinValue

    ''' <summary>Garbage collection of processed files.</summary>
    Private m_lGC As New List(Of String)

    ''' <summary>Messages pending a reply</summary>
    Private m_dictPendingReplies As New Dictionary(Of ULong, String)

#End Region ' Private vars

#Region " Construction / destruction "

    Public Sub New(iID As Integer, bIsServer As Boolean, Optional format As eSerializationFormat = eSerializationFormat.BSON)
        MyBase.New(iID, bIsServer)

        Me.SerializationFormat = format
        Me.Name = "fdep"
        Me.DisplayName = "File-based data exchange"

        ' JSON serialization must include full Type information 
        Me.m_settings = New JsonSerializerSettings() With {.TypeNameHandling = TypeNameHandling.All}

    End Sub

#End Region ' Construction / destruction

#Region " Public access - file transfer "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the framework traffic folder to observe.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Folder As String = ""

    ''' -----------------------------------------------------------------------
    ''' <summary>Supported serialization formats.</summary>
    ''' -----------------------------------------------------------------------
    Public Enum eSerializationFormat As Integer
        ''' <summary>Text-based JSON serialization.</summary>
        JSON = 0
        ''' <summary>Binary JSON serialization.</summary>
        BSON
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the <see cref="eSerializationFormat">serialization format</see> to use.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property SerializationFormat As eSerializationFormat

#End Region ' Public access - file transfer

#Region " Server "

    Public Overrides Function StartServer() As Boolean

        If MyBase.StartServer() Then
            ' Discard all old sync files immediately
            For Each fn As String In Directory.GetFiles(Me.Folder, "fdep-*.sync")
                Me.FlagForDeletion(fn)
            Next
            Me.StartWatching()
        End If
        Return Me.IsServerStarted

    End Function

    Public Overrides Function StopServer() As Boolean

        If MyBase.StopServer() Then
            Me.StopWatching()
        End If
        Return Not Me.IsServerStarted

    End Function

#End Region ' Server

#Region " Client "

    Public Overrides Function ConnectToServer() As Boolean

        If Me.IsServer() Then
            Debug.Assert(False, "Cannot connect server to server")
            Return False
        End If
        Me.StartWatching()
        Return MyBase.ConnectToServer()

    End Function

    Public Overrides Function IsConnectedToServer() As Boolean

        If Me.IsServer Then Return False
        Return (Me.m_serverID <> 0)

    End Function

    Public Overrides Function DisconnectFromServer() As Boolean

        If MyBase.DisconnectFromServer() Then
            Me.StopWatching()
        End If
        Return True

    End Function

#End Region ' Client

#Region " Overrides "

    Public Overrides Function IsConfigured() As Boolean
        Return Not String.IsNullOrWhiteSpace(Me.Folder)
    End Function

    Public Overrides ReadOnly Property DefaultPingInterval As Long
        Get
            Return 60 * 1000 ' file-based data exchange is slow. 1 minute should be acceptable
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdoc cref="Send()"/>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function SendThroughProtocol(obj As cDataPackage) As Boolean

        Dim bSuccess As Boolean = False

        ' Sanity checks
        If (Me.m_watcher Is Nothing) Then
            Debug.Assert(False, " Watcher not started")
            Return False
        End If

        If (Not Me.CanSend(obj)) Then Return bSuccess

        Try
            ' 'Sending' means parking a file with a pre-defined name in the watched
            ' folder, destined for the 'connected' remote machine
            Dim fn As String = Path.Combine(Me.m_watcher.Folder, Me.ProtocolSyncFileName(obj.Recipient, Date.Now.Ticks))
            Using sw As New StreamWriter(fn)
                sw.Write(Me.Serialize(obj))
                sw.Flush()
            End Using
            bSuccess = True

            ' Remember broadcast file. Upon reception of a reply, the pending reply file will be flagged for deletion
            If ((Me.IsServer) And (obj.Recipient = 0)) Then
                Me.m_dictPendingReplies(obj.Sequence) = fn
            End If

        Catch ex As Exception
            ' Hmm, not sure what else to do here
            bSuccess = False
        End Try

        Return bSuccess

    End Function

#End Region ' Overrides

#Region " Folder watching "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Start watching the framework folder for relevant traffic.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Private Function StartWatching() As Boolean

        ' Sanity checks
        If (String.IsNullOrWhiteSpace(Folder)) Then Return False
        If Not Directory.Exists(Folder) Then Return False

        ' Clean up, just in case
        Me.StopWatching()

        ' Start watching the indicated folder for changes
        Me.m_watcher = cFolderContentWatcher.GetInstance(Me.Folder, False)
        Me.m_utmStart = Date.UtcNow

        AddHandler Me.m_watcher.OnFileListChanged, AddressOf OnFileListChanged

        Me.StartConnectionPinging()

        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Stop watching the framework folder for relevant traffic.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Private Function StopWatching() As Boolean

        ' Stop any current folder watch
        If (Me.m_watcher IsNot Nothing) Then

            ' Thanks for all the fish
            cFolderContentWatcher.ReleaseInstance(Me.m_watcher)
            ' Disconnect
            RemoveHandler Me.m_watcher.OnFileListChanged, AddressOf OnFileListChanged
            ' Discard watcher
            Me.m_watcher = Nothing
        End If

        Me.StopConnectionPinging()
        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' File watcher callback. Implemented to filter out and process protocol
    ''' sync files.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="args"></param>
    ''' <seealso cref="ConsiderProtocolSyncFile(String, eFrameworkCommTypes)"/>
    ''' -----------------------------------------------------------------------
    Private Sub OnFileListChanged(sender As Object, args As cFolderContentWatcherEventArgs)

        Select Case args.Change

            Case cFolderContentWatcher.eChangeType.Added
                Dim fn As String = Path.GetFileName(args.File)
                If (IsProtocolSyncFileForMe(args.File)) Then
                    Me.ProcessProtocolSyncFile(args.File)
                End If
            Case Else
                ' NOP

        End Select

    End Sub

#End Region ' Folder watching

#Region " Protocol file management "

    Private Function ReadProtocolSyncFile(fn As String) As cDataPackage

        Dim obj As cDataPackage = Nothing
        Try
            Using sr As New StreamReader(fn)
                obj = Me.Deserialize(sr.ReadToEnd)
            End Using
        Catch ex As Exception
            'Track(String.Format("{0} processing exception {1}", Me, ex.Message))
        End Try
        Return obj

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Process a given protocol sync file by trying to deserialize its content
    ''' and pass it on for processing. The file is deleted after.
    ''' </summary>
    ''' <param name="fn">The protocol sync file to process.</param>
    ''' -----------------------------------------------------------------------
    Private Function ProcessProtocolSyncFile(fn As String) As Boolean

        Dim obj As cDataPackage = Me.ReadProtocolSyncFile(fn)

        If (obj Is Nothing) Then Return False
        If (obj.Data Is Nothing) Then Return False

        If (obj.TimeUTM < Me.m_utmStart) Then
            'Track(String.Format("{0} ignoring old file {1}", Me, fn))
            ' Put old files up for deletion
            Me.FlagForDeletion(fn)
            Return False
        End If

        ' Files that have been processed can be discarded
        If ((obj.Recipient = Me.ID) Or (Me.IsServer())) Then
            ' Put a file exclusively meant for me in the trash
            ' This includes files sent to the server (which are never broadcasted)
            Me.FlagForDeletion(fn)
        End If

        ' Extra deletion test: perhaps this message is in response to a pending reply?
        If (Me.IsServer()) Then
            SyncLock (Me.m_dictPendingReplies)
                If (Me.m_dictPendingReplies.ContainsKey(obj.ReplyToSequence)) Then
                    ' Remove original command that has now been replied to
                    Me.FlagForDeletion(Me.m_dictPendingReplies(obj.ReplyToSequence))
                    ' Done
                    Me.m_dictPendingReplies.Remove(obj.ReplyToSequence)
                End If
            End SyncLock
        End If

        Me.QueueIncomingData(obj)
        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Future improvement
    ''' </summary>
    ''' <param name="obj"></param>
    ''' -----------------------------------------------------------------------
    Private Sub QueueIncomingData(obj As cDataPackage)

        ' ToDo: here is room to add robustness smarts here:
        ' - Files may arrive out of sequence; queueing can be used to wait for missing sequence numbers to process commands in the correct order
        ' - Request missing data again
        ' - etc

        ' For now, just pass through
        Me.ProcessIncomingData(obj)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Return a standardized protocol sync file name.
    ''' </summary>
    ''' <param name="IDto">Recipient, if any</param>
    ''' <param name="tick">Time stamp</param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function ProtocolSyncFileName(IDto As Int32, tick As Long) As String
        Return String.Format("fdep-{0}-{1}.sync", Me.ID, tick)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Super solid and robust check whether a given file is a protocol sync
    ''' file. I mean, this stuff is completely bulletproof.
    ''' </summary>
    ''' <param name="fn"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function IsProtocolSyncFile(fn As String) As Boolean
        fn = Path.GetFileName(fn).ToLower()
        Return fn.StartsWith("fdep-") And fn.EndsWith(".sync")
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether a given file is a protocol sync file that is meant for 
    ''' me - the instance of this file data exchange protocol 
    ''' instance.
    ''' </summary>
    ''' <param name="fn"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function IsProtocolSyncFileForMe(fn As String) As Boolean
        If Not Me.IsProtocolSyncFile(fn) Then Return False
        fn = Path.GetFileName(fn).ToLower()
        Return fn.StartsWith(String.Format("fdep-{0}-", Me.ID))
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether a given file is a protocol sync file that is sent by me 
    ''' - the instance of this file data exchange protocol 
    ''' instance.
    ''' </summary>
    ''' <param name="fn"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function IsProtocolSyncFileFromMe(fn As String) As Boolean
        If Not Me.IsProtocolSyncFile(fn) Then Return False
        If Me.IsProtocolSyncFileForMe(fn) Then Return False
        Dim p As cDataPackage = Me.ReadProtocolSyncFile(fn)
        If (p Is Nothing) Then Return False
        Return (p.Sender = Me.ID)
    End Function

#End Region ' Folder watching

#Region " Serialization "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Serialize an object to a protocol sync text file.
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function Serialize(obj As cDataPackage) As String

        Dim result As String = ""

        Select Case Me.SerializationFormat

            Case eSerializationFormat.JSON
                result = JsonConvert.SerializeObject(obj, Formatting.Indented, Me.m_settings)

            Case eSerializationFormat.BSON
                Dim ms As New MemoryStream()
                Using bs As New Bson.BsonDataWriter(ms)
                    Dim s As JsonSerializer = JsonSerializer.Create(Me.m_settings)
                    s.Serialize(bs, obj)
                End Using
                result = Convert.ToBase64String(ms.ToArray())

            Case Else
                Debug.Assert(False, "Format not supported")

        End Select
        Return result

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Deserialize an object from protocol sync file text.
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function Deserialize(obj As String) As cDataPackage

        Dim result As cDataPackage = Nothing

        Select Case Me.SerializationFormat

            Case eSerializationFormat.JSON
                result = JsonConvert.DeserializeObject(Of cDataPackage)(obj, Me.m_settings)

            Case eSerializationFormat.BSON
                Dim data As Byte() = Convert.FromBase64String(obj)
                Dim ms As New MemoryStream(data)
                Using bs As New Bson.BsonDataReader(ms)
                    Dim s As JsonSerializer = JsonSerializer.Create(Me.m_settings)
                    result = s.Deserialize(Of cDataPackage)(bs)
                End Using

            Case Else
                Debug.Assert(False, "Format not supported")

        End Select
        Return result

    End Function

#End Region ' Serialization

#Region " Settings persistence "

    Public Overrides Function Load(settings As cXMLSettings, section As String) As Boolean

        If (Not MyBase.Load(settings, section) = False) Then Return False
        Me.Folder = settings.ReadSetting(section, "FDEP_folder", "")
        Return True

    End Function

    Public Overrides Function Save(settings As cXMLSettings, section As String) As Boolean

        If (Not MyBase.Save(settings, section) = False) Then Return False
        settings.WriteSetting(section, "FDEP_folder", Me.Folder)
        Return True

    End Function

#End Region ' Settings persistence

#Region " Dead file collection "

    Private m_timerDelete As System.Threading.Timer

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Flag files for deletion.
    ''' </summary>
    ''' <param name="fn"></param>
    ''' <param name="bDeleteImmediately"></param>
    ''' <remarks>
    ''' <para>
    ''' Perhaps set a timer when a file can safely deleted? Hmm, this is highly
    ''' application specific. Instead, perhaps any command sent should have an
    ''' expiry timer, which can be used to revoke (e.g. delete) files too.
    ''' </para>
    ''' <para>
    ''' Another approach can be to delete broadcasted commands to which a reply 
    ''' has been received. This would require SENDING SERVERS to remember 
    ''' broadcasted messages, and when RECEIVING SERVER detects a reply, the 
    ''' sent file can be safely dispatched. Hmm, tricky.
    ''' </para>
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Private Sub FlagForDeletion(fn As String)

        Dim bDeleted As Boolean = False

        ' No need to hurt yourself
        If (String.IsNullOrWhiteSpace(fn)) Then Return

        'Track(String.Format("{0} deleting {1}", Me, fn))

        Try
            File.Delete(fn)
            bDeleted = True
        Catch ex As FileNotFoundException
            ' Ok then
            bDeleted = True
        Catch ex As Exception
            ' Swallow this one
        End Try

        ' The deed has been done
        If (bDeleted) Then Return

        ' Flag for deletion at a later stage
        SyncLock (Me.m_lGC)
            If Not Me.m_lGC.Contains(fn) Then
                Me.m_lGC.Add(fn)
            End If

            If Me.m_lGC.Count > 0 Then
                ' ToDo: Start timer to delete a file at a later stage
            End If
        End SyncLock

    End Sub

#End Region ' Dead file collection

End Class
