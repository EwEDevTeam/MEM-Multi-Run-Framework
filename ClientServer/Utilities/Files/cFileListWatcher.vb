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
Imports System.Runtime.Serialization
Imports System.IO

#End Region ' Imports

''' ===========================================================================
''' <summary>    
''' <para>Helper class to watch for a list of files to become available in a 
''' single folder and optionally subfolders. Only files that reside in or under the 
''' <paramref name="folder"/> will be watched, and their presence will be checked.</para>
''' <para>
''' <list type="bullet"><item>Files that have not been checked yet, or that may 
''' not have yet arrived, will be listed in the <see cref="Awaiting()"/> collection.</item>
''' <item>Files that have arrived and that are not locked will be listed in the 
''' <see cref="Available()"/> collection.</item>
''' <item>Files that do not occur in the folder will be listed in the 
''' <see cref="NotWatched()"/> collection.</item>
''' </list></para>
''' The <see cref="OnFileListComplete"/> event will be broadcasted whenever file
''' availability states change.
''' </summary>
''' ===========================================================================
Public Class cFileListWatcher
    Implements IDisposable

#Region " Private vars "

    Private m_syncObject As SynchronizationContext = Nothing
    Private m_watcher As cFolderContentWatcher = Nothing
    Private m_lAwaiting As New List(Of String)
    Private m_lAvailable As New List(Of String)
    Private m_lNotWatched As New List(Of String)
    Private m_bIsDisposed As Boolean = False

#End Region ' Private vars

#Region " Construction / destruction "

    Public Sub New()

        ' Get sync object
        Me.m_syncObject = SynchronizationContext.Current
        If (Me.m_syncObject Is Nothing) Then
            Me.m_syncObject = New SynchronizationContext()
        End If

    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not m_bIsDisposed Then
            Me.StopWatching()
            Me.m_syncObject = Nothing
            Me.m_bIsDisposed = True
        End If
    End Sub

#End Region ' Construction / destruction

#Region " Watching "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' <para>Start watching for a list of files. Only files that reside in or under the 
    ''' <paramref name="folder"/> will be watched, and their presence will be checked.</para>
    ''' </summary>
    ''' <param name="folder">The folder to watch.</param>
    ''' <param name="files">The files to watch for, underneath the folder.</param>
    ''' <param name="bSubDirs">Flag, stating if subfolders should be watched too.</param>
    ''' <returns>True if the watched <paramref name="folder"/> exists.</returns>
    ''' <seealso cref="StopWatching()"/>
    ''' <seealso cref="IsWatching()"/>
    ''' -----------------------------------------------------------------------
    Public Function StartWatching(folder As String, files As IEnumerable(Of String), Optional bSubDirs As Boolean = True) As Boolean

        If (Me.IsWatching) Then Me.StopWatching()

        If (Not Directory.Exists(folder)) Then
            Console.Error.WriteLine("Unable to watch non-existing folder " & folder)
            Return False
        End If

        Me.m_watcher = cFolderContentWatcher.GetInstance(folder, bSubDirs)
        AddHandler Me.m_watcher.OnFileListChanged, AddressOf OnFileListChanged

        For Each f As String In files

            ' Presume that the file resides within the folder
            f = Path.Combine(folder, f)
            Try
                If (Me.m_watcher.IsWatching(f)) Then
                    Me.m_lAwaiting.Add(f)
                Else
                    Me.m_lNotWatched.Add(f)
                    If (File.Exists(f)) Then Me.m_lAvailable.Add(f)
                End If
            Catch ex As Exception
                ' Cascade error, somehow
                Console.Error.WriteLine("Unable to watch file " & f)
                Return False
            End Try
        Next
        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Stop watching for files.
    ''' </summary>
    ''' <seealso cref="StartWatching(String, IEnumerable(Of String), Boolean)"/>
    ''' <seealso cref="IsWatching()"/>
    ''' -----------------------------------------------------------------------
    Public Sub StopWatching()

        If (Not Me.IsWatching) Then Return

        RemoveHandler Me.m_watcher.OnFileListChanged, AddressOf OnFileListChanged

        cFolderContentWatcher.ReleaseInstance(Me.m_watcher)
        Me.m_watcher = Nothing

        Me.m_lAwaiting.Clear()
        Me.m_lNotWatched.Clear()
        Me.m_lAvailable.Clear()

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns whether a files watch operation is in progress.
    ''' </summary>
    ''' <returns><c>True</c> if a files watch operation is in progress.</returns>
    ''' -----------------------------------------------------------------------
    Public Function IsWatching() As Boolean
        Return (Me.m_watcher IsNot Nothing)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the folder being watched, or an empty string if a watch operation is
    ''' not in progress.
    ''' </summary>
    ''' <seealso cref="StartWatching(String, IEnumerable(Of String), Boolean)"/>
    ''' <seealso cref="StopWatching()"/>
    ''' <seealso cref="IsWatching()"/>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Folder As String
        Get
            If (Me.m_watcher IsNot Nothing) Then
                Return Me.m_watcher.Folder
            End If
            Return ""
        End Get
    End Property


    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns all files that are presumably part of the watched folder 
    ''' structure but that have not yet arrived.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function Awaiting() As IEnumerable(Of String)
        Return Me.m_lAwaiting.ToArray
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns all files that are part of the watched folder structure and that
    ''' are fully available.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function Available() As IEnumerable(Of String)
        Return Me.m_lAvailable.ToArray
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns all files that are outside of the watched folder structure. They
    ''' are not being watched because, well, they are outside the watched
    ''' folder structure.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Function NotWatched() As IEnumerable(Of String)
        Return Me.m_lNotWatched.ToArray
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get whether all files are fully available.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property AllFilesAvailable As Boolean
        Get
            Return Me.m_lAwaiting.Count = 0
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Public event, fired when file availability has changed.
    ''' </summary>
    ''' <param name="sender">The sender of the event.</param>
    ''' <param name="args"><see cref="cFileListWatcherEventArgs">Relevant data.</see></param>
    ''' -----------------------------------------------------------------------
    Public Event OnFileListComplete(sender As Object, args As cFileListWatcherEventArgs)

#End Region ' Watching

#Region " Events "

    Protected Sub SendThreadSafeStatusEvent()
        Me.m_syncObject.Send(New SendOrPostCallback(AddressOf DoSendEvent),
                             New cFileListWatcherEventArgs(Me.m_lAvailable.ToArray(), Me.m_lAwaiting.ToArray()))
    End Sub

    Private Sub DoSendEvent(obj As Object)
        Try
            RaiseEvent OnFileListComplete(Me, DirectCast(obj, cFileListWatcherEventArgs))
        Catch ex As Exception
            ' Whoah!
        End Try
    End Sub

#End Region ' Events

#Region " Event handling "

    Private Sub OnFileListChanged(sender As Object, args As cFolderContentWatcherEventArgs)

        Dim bSendEvent As Boolean = False

        ' Use incremental evaluation here
        Select Case args.Change
            Case cFolderContentWatcher.eChangeType.Added,
                 cFolderContentWatcher.eChangeType.Changed

                Dim w As String() = Me.m_lAwaiting.ToArray()
                For Each f As String In w
                    If cFileUtils.FilesEqual(f, args.File) Then
                        Me.m_lAwaiting.Remove(f)
                        Me.m_lAvailable.Add(f)
                        'Debug.WriteLine("FLW: moved {0} from awaiting to available", f)
                        bSendEvent = True
                    End If
                Next

            Case cFolderContentWatcher.eChangeType.Removed

                Dim w As String() = Me.m_lAvailable.ToArray()
                For Each f As String In w
                    If cFileUtils.FilesEqual(f, args.File) Then
                        Me.m_lAvailable.Remove(f)
                        Me.m_lAwaiting.Add(f)
                        'Debug.WriteLine("FLW: moved {0} from available to awaiting", f)
                        bSendEvent = True
                    End If
                Next

        End Select

        If (bSendEvent) Then
            Me.SendThreadSafeStatusEvent()
        End If

    End Sub

#End Region ' Event handling

End Class