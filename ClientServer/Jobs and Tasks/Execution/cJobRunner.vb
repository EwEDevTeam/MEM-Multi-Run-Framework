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
Imports System.Net
Imports System.Threading

#End Region ' Imports

#Const THREADED = 1

''' ===========================================================================
''' <summary>
''' Simple job runner.
''' </summary>
''' ===========================================================================
Public Class cJobRunner

#Region " Private vars "

    Private m_syncObject As SynchronizationContext = Nothing
    Private m_thread As Thread

    Private m_GUIDjob As Guid = Guid.Empty
    Private m_job As cJob = Nothing
    Private m_runner As cTaskRunner = Nothing
    Private m_bAbort As Boolean = False
    Private m_IDsender As Integer = 0

    Private m_settings As cSettings = Nothing

#End Region ' Private vars

#Region " Public access "

    Public Sub New(settings As cSettings)

        Me.m_settings = settings

        ' Get sync object
        Me.m_syncObject = SynchronizationContext.Current
        If (Me.m_syncObject Is Nothing) Then
            Me.m_syncObject = New SynchronizationContext()
        End If

    End Sub

    Public Property MaxThreads As Integer

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Run a job.
    ''' </summary>
    ''' <param name="jobrequest">The job to run. If set to nothing, the current
    ''' run will be aborted.</param>
    ''' <returns>True if the job is accepted for execution.</returns>
    ''' -----------------------------------------------------------------------
    Public Function Run(sender As Int32, jobrequest As cJobRunRequest) As Boolean

        If (jobrequest Is Nothing) Then
            If Me.IsRunning Then
                Me.Abort = True
                Return True
            End If
            Return False
        End If

        If (Me.IsRunning = True) Then Return False

        Me.m_job = jobrequest.Job
        Me.m_GUIDjob = jobrequest.ExecutionID
        Me.m_IDsender = sender
        Me.m_bAbort = False

#If THREADED Then
        Me.m_thread = New Thread(AddressOf RunProc)
        Me.m_thread.Start()
#Else
        Me.RunProc()
#End If
        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Test if a job can be executed by validating whether each task is configured
    ''' to run.
    ''' </summary>
    ''' <param name="job"></param>
    ''' <returns><see cref="eRunState.Success"/> if OK, or an <see cref="eRunState"/> 
    ''' error code if something went wrong.</returns>
    ''' -----------------------------------------------------------------------
    Public Function CanRun(job As cJob) As eRunState

        Dim fact As New cTaskRunnerFactory(Me.m_settings)

        ' ToDo: also test if all task paths are NOT rooted (e.g., "C:\Windows\System32") which poses a security risk.
        ' To mitigate, all paths in the task Parameters should start with an alias

        For iTask As Integer = 0 To job.Tasks.Count - 1
            Dim task As cTask = job.Tasks(iTask)
            If (Not Me.m_settings.ApplicationSettings.HasAlias(task.Alias)) Then
                Return eRunState.ErrorAliasMissing
            ElseIf (Not Me.m_settings.FolderSettings.CanExpand(task.Parameters)) Then
                Return eRunState.ErrorAliasMissing
            End If
        Next iTask

        Return eRunState.Success

    End Function

    Public Function IsRunning() As Boolean
        Return (Me.m_job IsNot Nothing) Or (Me.m_thread IsNot Nothing)
    End Function

    Public Property Abort As Boolean
        Get
            Return Me.m_bAbort
        End Get
        Set(value As Boolean)
            Me.m_bAbort = value
            If (Me.m_runner IsNot Nothing) Then
                Me.m_runner.Abort = value
            End If
        End Set
    End Property

#End Region ' Public access

#Region " Events "

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Public event reporting that something has changed while running a job.
    ''' </summary>
    ''' <param name="sender">The <see cref="cDataExchangeProtocol">instance</see>
    ''' instance that received the data.</param>
    ''' <param name="data">The <see cref="cSerializableObject">data</see>
    ''' that was received.</param>
    ''' -------------------------------------------------------------------
    Public Event OnJobRunStatusChanged(sender As cJobRunner, args As cJobRunnerStatusEventArgs)

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="status"></param>
    ''' <param name="sender">The <see cref="cDataExchangeProtocol.ID"/> of the sender.</param>
    ''' <param name="message">The <see cref="cDataPackage.Sequence"/> of the data package.</param>
    ''' -------------------------------------------------------------------
    Protected Sub SendJobRunStateChangeEvent(jobname As String, guid As Guid, taskStates As eRunState(), jobState As eRunState)
        Dim su As New cJobRunUpdate(guid, taskStates, jobState, (jobState >= eRunState.Success))
        Dim args As New cJobRunnerStatusEventArgs(jobname, su)
        Me.m_syncObject.Send(New SendOrPostCallback(AddressOf DoSendJobRunStateChangeEvent), args)
    End Sub

    Private Sub DoSendJobRunStateChangeEvent(obj As Object)

        Try
            RaiseEvent OnJobRunStatusChanged(Me, DirectCast(obj, cJobRunnerStatusEventArgs))
        Catch ex As Exception
            ' Whoah!
        End Try

    End Sub

#End Region ' Events 

#Region " Internals "

    Private Sub RunProc()

        Dim fact As New cTaskRunnerFactory(Me.m_settings)
        Dim iTask As Integer = 0
        Dim job As cJob = Me.m_job
        Dim guid As Guid = Me.m_GUIDjob
        Dim taskRunStates(Me.m_job.Tasks.Count - 1) As eRunState


        Dim test As eRunState = Me.CanRun(job)
        If (test <> eRunState.Success) Then
            Me.SendJobRunStateChangeEvent(job.Name, guid, taskRunStates, test)
            Return
        End If

        Try
            If (job.Inputs.Count > 0) Then

                Debug.WriteLine("{0} waiting for data", job.Name)

                Dim datawaiter As New cFileListWatcher()
                Dim folder As String = Me.m_settings.FolderSettings.Expand(job.InputFolderAlias)

                Array.Fill(taskRunStates, eRunState.WaitingForData)
                Me.SendJobRunStateChangeEvent(job.Name, guid, taskRunStates, eRunState.WaitingForData)

                If datawaiter.StartWatching(folder, job.Inputs) Then

                    Dim tNow As New Stopwatch()
                    tNow.Start()

                    While (datawaiter.AllFilesAvailable = False) And
                          (Me.m_bAbort = False) And
                          (tNow.ElapsedMilliseconds < Me.m_settings.RuntimeSettings.DataWaitTimeout)
                        Threading.Thread.Sleep(500)
                    End While

                Else
                    Array.Fill(taskRunStates, eRunState.ErrorInFolderMissing)
                    Me.m_bAbort = True
                End If

                ' Just in case
                datawaiter.StopWatching()

                If Not datawaiter.AllFilesAvailable Then
                    Array.Fill(taskRunStates, eRunState.ErrorNoInput)
                    Me.m_bAbort = True
                End If
            End If

        Catch ex As Exception
            Array.Fill(taskRunStates, eRunState.ErrorCrashed)
            Console.Error.WriteLine("cJobRunner Exception " & ex.Message)
            Me.m_bAbort = True

        End Try

        If (Me.m_bAbort = False) Then

            Debug.WriteLine("{0} running", job.Name)

            Array.Fill(taskRunStates, eRunState.Assigned)

            While (iTask < job.Tasks.Count) And (Me.m_bAbort = False)

                Dim task As cTask = job.Tasks(iTask)
                Dim runner As cTaskRunner = fact.GetTaskRunner(job, task)

                If (runner IsNot Nothing) Then

                    'Debug.WriteLine("{0} running {1}", m_strJob, m_strTask)

                    If Me.StartRunTask(runner) Then

                        taskRunStates(iTask) = eRunState.Running
                        Me.SendJobRunStateChangeEvent(job.Name, guid, taskRunStates, eRunState.Running)

                        ' Spin wheels while task runs
                        While (Me.IsTaskRunning() = True) And (Me.m_bAbort = False)
                            Threading.Thread.Sleep(500)
                        End While

                        taskRunStates(iTask) = runner.Status
                    Else
                        taskRunStates(iTask) = eRunState.ErrorConfig
                    End If
                Else
                    taskRunStates(iTask) = eRunState.ErrorConfig
                End If

                Me.m_bAbort = Me.m_bAbort Or (taskRunStates(iTask) <> eRunState.Success)
                Debug.WriteLine("{0} done {1} {2}. Abort = {3}", job.Name, task.Name, taskRunStates(iTask).ToString(), Me.m_bAbort)
                iTask += 1

            End While

        End If

        ' Calculate final run state now all task processing is done
        Dim runState As eRunState = eRunState.Idle
        For Each t As eRunState In taskRunStates
            If (t > runState) Then
                runState = t
            End If
        Next

        Me.m_job = Nothing
        Me.m_GUIDjob = Guid.Empty
        Me.m_thread = Nothing
        Me.m_bAbort = False
        Me.m_IDsender = 0

        Me.SendJobRunStateChangeEvent(job.Name, guid, taskRunStates, runState)

    End Sub

    Private Function StartRunTask(tr As cTaskRunner) As Boolean

        If (tr Is Nothing) Then Return False

        Me.m_runner = tr
        AddHandler Me.m_runner.OnTaskCompleted, AddressOf OnTaskRunCompleted
        If Not Me.m_runner.Start() Then Return False

        Return True

    End Function

    ''' <summary>
    ''' Clean-up at the end of the run.
    ''' </summary>
    Private Sub EndRunTask()

        If (Me.m_runner IsNot Nothing) Then
            RemoveHandler Me.m_runner.OnTaskCompleted, AddressOf OnTaskRunCompleted
            Me.m_runner = Nothing
        End If

    End Sub

    Private Function IsTaskRunning() As Boolean
        Return (Me.m_runner IsNot Nothing)
    End Function

    Private Sub OnTaskRunCompleted(sender As cTaskRunner)

        Dim o As String = Me.m_runner.OutputLog
        If (Not String.IsNullOrWhiteSpace(o)) Then
            Debug.WriteLine("- Output: {0}", o.Trim())
        End If

        Dim e As String = Me.m_runner.ErrorLog
        If (Not String.IsNullOrWhiteSpace(e)) Then
            Debug.WriteLine("- Errors: {0}", e.Trim())
        End If

        Me.EndRunTask()

    End Sub

#End Region ' Internals

End Class
