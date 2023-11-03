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
Imports System.Runtime.InteropServices
Imports System.Timers

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' A reasonably smart wrapper of a <see cref="FileSystemWatcher"/> to monitor 
''' a folder structure for appearance and disappearance of files and directories. 
''' This class is built to observe folders managed by cloud storage providers, 
''' where folder content may arrive slowly. Only when files have fully arrived 
''' and are no longer locked, their presence is announced through an 
''' <see cref="OnFileListChanged">event</see>.
''' </summary>
''' ===========================================================================
Public Class cFolderContentWatcher
    Implements IDisposable

#Region " Private vars "

    ''' <summary>Sync lock.</summary>
    Private m_syncObj As New Object()

    ''' <summary>System file watcher.</summary>
    Private m_watcher As FileSystemWatcher = Nothing
    ''' <summary>List of files that are freely available.</summary>
    Private m_filesAvailable As New List(Of String)
    ''' <summary>List of files that are partially available but that haven't fully arrived yet.</summary>
    Private m_filesQueue As New List(Of String)
    ''' <summary>Timer to process the queued files</summary>
    Private m_timerFilesQueue As Timer = Nothing
    ''' <summary>Time stamp of last file activity, handy to deduct if a file
    ''' delivery process is running.</summary>
    Private m_lastactivity As Date = Date.Now

#End Region ' Private vars

    ''' <summary>
    ''' Event to announce the state of a file in the observed folder(s).
    ''' </summary>
    Public Enum eChangeType As Integer
        Added
        Removed
        Changed
    End Enum

#Region " Construction / destruction "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Create a new file watcher.
    ''' </summary>
    ''' <param name="folder"></param>
    ''' <param name="bIncludeSubFolders"></param>
    ''' -----------------------------------------------------------------------
    Public Sub New(folder As String, Optional bIncludeSubFolders As Boolean = False)

        Me.Folder = Path.GetFullPath(folder)
        Me.IncludeSubFolders = bIncludeSubFolders

        Me.m_watcher = New FileSystemWatcher(folder)
        Me.m_watcher.NotifyFilter = NotifyFilters.FileName Or NotifyFilters.LastWrite Or NotifyFilters.Attributes
        Me.m_watcher.Filter = "*.*"
        Me.m_watcher.IncludeSubdirectories = bIncludeSubFolders
        Me.m_watcher.EnableRaisingEvents = True
        AddHandler Me.m_watcher.Created, AddressOf OnFileSystemChanged
        AddHandler Me.m_watcher.Changed, AddressOf OnFileSystemChanged
        AddHandler Me.m_watcher.Deleted, AddressOf OnFileSystemDeleted

        Me.m_timerFilesQueue = New Timer()
        Me.m_timerFilesQueue.Interval = 1000
        Me.m_timerFilesQueue.AutoReset = True
        AddHandler Me.m_timerFilesQueue.Elapsed, AddressOf OnProcessQueueTimer

        ' Kick off by queueing all files in the observed folder
        For Each fin As String In Directory.GetFiles(folder, Me.m_watcher.Filter, If(Me.IncludeSubFolders, SearchOption.AllDirectories, SearchOption.TopDirectoryOnly))
            Me.QueueChange(fin)
        Next

    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose

        SyncLock Me.m_syncObj
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End SyncLock

    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)

        If Not m_bIsDisposed Then

            Me.m_timerFilesQueue.Stop()
            RemoveHandler Me.m_timerFilesQueue.Elapsed, AddressOf OnProcessQueueTimer
            Me.m_timerFilesQueue = Nothing

            Me.m_watcher.EnableRaisingEvents = False
            RemoveHandler Me.m_watcher.Changed, AddressOf OnFileSystemChanged
            RemoveHandler Me.m_watcher.Deleted, AddressOf OnFileSystemDeleted
            Me.m_watcher = Nothing

            Me.m_bIsDisposed = True
        End If
    End Sub

#End Region ' Construction / destruction

#Region " Public bits"

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the folder that this watcher was created for.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Folder As String

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get if this watcher also watches subfolders.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property IncludeSubFolders As Boolean

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get if the most recent file change occurred within a specified interval.
    ''' </summary>
    ''' <param name="ms">Optional interval (in ms) to check for.</param>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property IsActive(Optional ms As ULong = 60000) As Boolean
        Get
            Return (Date.Now.Ticks - Me.m_lastactivity.Ticks) <= ms
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the list of available files in (and underneath) the watched folder.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Files As ICollection(Of String)
        Get
            Return Me.m_filesAvailable.ToArray()
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event to notify when the file list has changed.
    ''' </summary>
    ''' <param name="sender">The sender of the event.</param>
    ''' <param name="args"><see cref="cFolderContentWatcherEventArgs"/> instance.</param>
    ''' -----------------------------------------------------------------------
    Public Event OnFileListChanged(sender As Object, args As cFolderContentWatcherEventArgs)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether the indicated file is contained within the folder structure
    ''' that is being watched. The comparison honours <see cref="cFileUtils.IsOSCaseSensitive()">
    ''' OS-specific case sensitivity</see>.
    ''' </summary>
    ''' <param name="file">The file to find in the containing folder.</param>
    ''' -----------------------------------------------------------------------
    Public Function IsWatching(file As String) As Boolean
        Return cFileUtils.IsContained(file, Me.Folder, Me.IncludeSubFolders)
    End Function

#End Region ' Public bits

#Region " Shared access "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Singleton access is provided to minimize the number of file watchers.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Private Class cCache

        Public Sub New(instance As cFolderContentWatcher)
            Me.Instance = instance
            Me.RefCount = 0
        End Sub

        Public ReadOnly Instance As cFolderContentWatcher
        Public Property RefCount As Integer = 0

    End Class

    Private Shared s_watchers As New List(Of cCache)
    Private m_bIsDisposed As Boolean

    ''' <summary>
    ''' Obtain a folder content watcher for a given directory and subfolder flag. Watchers are
    ''' re-used for the same settings to reduce file system load. Release any watcher obtained
    ''' via this method with <see cref="ReleaseInstance"/>
    ''' </summary>
    ''' <param name="folder"></param>
    ''' <param name="bIncludeSubfolders"></param>
    ''' <returns></returns>
    Public Shared Function GetInstance(folder As String, bIncludeSubfolders As Boolean) As cFolderContentWatcher

        Dim wi As cCache = Nothing
        Dim bFound As Boolean = False

        SyncLock cFolderContentWatcher.s_watchers
            For Each wi In cFolderContentWatcher.s_watchers
                Dim w As cFolderContentWatcher = wi.Instance
                If (w.Folder = folder And w.IncludeSubFolders = bIncludeSubfolders) Then
                    bFound = True
                    Exit For
                End If
            Next

            If (Not bFound) Then
                wi = New cCache(New cFolderContentWatcher(folder, bIncludeSubfolders))
                s_watchers.Add(wi)
            End If

            wi.RefCount += 1
        End SyncLock
        Return wi.Instance

    End Function

    Public Shared Function ReleaseInstance(w As cFolderContentWatcher) As Boolean

        If (w Is Nothing) Then Return False
        Return cFolderContentWatcher.ReleaseInstance(w.Folder, w.IncludeSubFolders)

    End Function

    Public Shared Function ReleaseInstance(folder As String, bIncludeSubfolders As Boolean) As Boolean

        Dim cache As cCache = Nothing
        Dim bFound As Boolean = False

        SyncLock cFolderContentWatcher.s_watchers

            For Each cache In cFolderContentWatcher.s_watchers
                Dim w As cFolderContentWatcher = cache.Instance
                If (w.Folder = folder And w.IncludeSubFolders = bIncludeSubfolders) Then
                    bFound = True
                    Exit For
                End If
            Next

            If Not bFound Then Return False

            cache.RefCount -= 1
            If (cache.RefCount <= 0) Then
                cFolderContentWatcher.s_watchers.Remove(cache)
                cache.Instance.Dispose()
            End If
        End SyncLock

        Return True

    End Function

#End Region ' Shared access

#Region " Event handling "

    ''' <summary>
    ''' FileSystemWatcher changed event handler.
    ''' </summary>
    ''' <param name="sender">The sender.</param>
    ''' <param name="args">The <see cref="FileSystemEventArgs"/> instance containing the event data.</param>
    Private Sub OnFileSystemChanged(sender As Object, args As FileSystemEventArgs)
        Me.m_lastactivity = Date.Now
        Me.QueueChange(args.FullPath)
    End Sub

    ''' <summary>
    ''' FileSystemWatcher deleted event handler.
    ''' </summary>
    ''' <param name="sender">The sender.</param>
    ''' <param name="args">The <see cref="FileSystemEventArgs"/> instance containing the event data.</param>
    Private Sub OnFileSystemDeleted(sender As Object, args As FileSystemEventArgs)
        Me.m_lastactivity = Date.Now
        Me.ProcessDeletion(args.FullPath)
    End Sub

    ''' <summary>
    ''' Queue processing timer event handler.
    ''' </summary>
    ''' <param name="sender">The sender.</param>
    ''' <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
    Private Sub OnProcessQueueTimer(sender As Object, args As EventArgs)

        SyncLock Me.m_syncObj
            Me.ProcessQueue()
        End SyncLock

    End Sub

#End Region ' Event handling

#Region " Internals "

    Private Sub ProcessFileAddedOrChanged(fin As String)
        If cFileUtils.IsDirectory(fin) Then
            ' Ignore
        Else
            If (Not Me.m_filesAvailable.Contains(fin)) Then
                Me.m_filesAvailable.Add(fin)
                Me.NotifyWorld(fin, eChangeType.Added)
            Else
                Me.NotifyWorld(fin, eChangeType.Changed)
            End If
        End If
    End Sub

    Private Sub ProcessDeletion(fin As String)
        If (Me.m_filesAvailable.Contains(fin)) Then
            Me.m_filesAvailable.Remove(fin)
            Me.NotifyWorld(fin, eChangeType.Removed)
        End If
    End Sub

    Private Sub NotifyWorld(file As String, change As eChangeType)
        Try
            RaiseEvent OnFileListChanged(Me, New cFolderContentWatcherEventArgs(file, change))
        Catch ex As Exception
            ' NOP
        End Try
    End Sub

#End Region ' Internals

#Region " Queue "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Add a file to the file processing queue to lazily check if the file is 
    ''' fully available.
    ''' </summary>
    ''' <param name="fin">The file to add to the queue.</param>
    ''' -----------------------------------------------------------------------
    Private Sub QueueChange(fin As String)

        If Me.m_bIsDisposed Then Return

        SyncLock m_syncObj
            If Not Me.m_filesQueue.Contains(fin) Then
                ' Start timer when first item is queued
                If (Me.m_filesQueue.Count = 0) Then Me.m_timerFilesQueue.Start()
                Me.m_filesQueue.Add(fin)
            End If
        End SyncLock
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Processes the pending changed files queue.
    ''' </summary>
    ''' <returns>True if all pending files are available.</returns>
    ''' -----------------------------------------------------------------------
    Private Function ProcessQueue() As Boolean

        Dim bDone As Boolean = False
        If Me.m_bIsDisposed Then Return True

        SyncLock m_syncObj
            Dim i As Integer = 0
            While i < Me.m_filesQueue.Count

                Dim fin As String = Me.m_filesQueue(i)

                ' Is file present and no longer locked?
                If cFileUtils.IsFileReady(fin) Then
                    ' #Yes: 
                    Me.ProcessFileAddedOrChanged(fin)
                    m_filesQueue.RemoveAt(i)

                Else
                    i += 1
                End If
            End While

            If (Me.m_filesQueue.Count = 0) Then
                Me.m_timerFilesQueue.Stop()
                'Console.WriteLine("Pending file list processed")
                bDone = True
            End If
        End SyncLock

        Return bDone

    End Function

#End Region ' Queue

End Class
