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
Imports ClientServer

#End Region ' Imports

' ToDo_JS: find a neat and intuitive way to integrate the cleanup task (and possibly
' other server-side tasks)

''' ===========================================================================
''' <summary>
''' Scheduler to dispatch a workload across a clients-server network.
''' </summary>
''' ===========================================================================
Public Class cWorkScheduler

#Region " Private classes "

    ''' <summary>
    ''' The last known execution state of a client
    ''' </summary>
    Private Class cClientState

        Private m_job As cJob = Nothing
        Private m_execguid As Guid = Guid.Empty

        Public ReadOnly Property machineID As Int32

        Public Sub New(id As Int32)
            Me.machineID = id
        End Sub

        Public Property IsAssigned As Boolean
        Public Property RunState As eRunState = eRunState.Idle

        Public ReadOnly Property Job As cJob
            Get
                Return Me.m_job
            End Get
        End Property

        Public ReadOnly Property ExecutionID As Guid
            Get
                Return Me.m_execguid
            End Get
        End Property

        Public Sub Assign(job As cJob)
            Me.m_job = job
            Me.m_execguid = Guid.NewGuid()
            Me.IsAssigned = True
        End Sub

        Public Sub Clear()
            Me.m_job = Nothing
            Me.m_execguid = Guid.Empty
            Me.IsAssigned = False
        End Sub

    End Class

#End Region ' Private classes

#Region " Private vars "

    ''' <summary>
    ''' Lock to prevent concurrent access.
    ''' </summary>
    Private m_lock As New Object()

    ''' <summary>
    ''' Dictionary (client ID -> run state) of connected clients and what they are working on
    ''' </summary>
    Private m_clientStates As New Dictionary(Of Int32, cClientState)

    ''' <summary>
    ''' Queue of pending tasks
    ''' </summary>
    Private m_pending As New List(Of cJob)

    ''' <summary>
    ''' Dictionary (int32 -> job name) of tasks currently assigned
    ''' </summary>
    Private m_processing As New Dictionary(Of Int32, cClientState)

    ''' <summary>
    ''' The workload under execution
    ''' </summary>
    Private m_workload As cWorkload = Nothing

    ''' <summary>
    ''' The local job runner
    ''' </summary>
    Private m_runner As cJobRunner = Nothing

#End Region ' Private vars

#Region " Construction / destruction "

    Public Sub New(s As cServer)

        Me.Server = s
        Me.Output = New cWorkSchedulerStatus()

        AddHandler Me.Server.OnClientAddedRemoved, AddressOf OnClientAddedRemoved
        AddHandler Me.Server.OnClientData, AddressOf OnClientData

    End Sub

    Private Sub OnClientData(sender As Object, args As cClientDataEventArgs)
        If TypeOf (args.Data) Is cJobRunUpdate Then
            Try
                Me.OnClientData(args.ClientID, DirectCast(args.Data, cJobRunUpdate))
            Catch ex As Exception

            End Try
        End If
    End Sub

    Private Sub OnClientAddedRemoved(sender As Object, args As cClientAddedRemovedEventArgs)
        Try
            Select Case args.Status
                Case eClientServerStatus.Connected
                    Me.OnClientAdded(args.ClientID)
                Case eClientServerStatus.Disconnected
                    Me.OnClientRemoved(args.ClientID)
            End Select
        Catch ex As Exception

        End Try
    End Sub

#End Region ' Construction / destruction

#Region " Public access "

    Public ReadOnly Property Server As cServer

    Public ReadOnly Property Output As cWorkSchedulerStatus

    Public Sub SetWork(w As cWorkload)
        If (Object.ReferenceEquals(w, Me.m_workload)) Then Return

        SyncLock (Me.m_lock)

            If (Me.m_workload IsNot Nothing) Then
                ' De-assign all
                Dim working() As cClientState = Me.m_clientStates.Values().ToArray
                For Each s As cClientState In working
                    Me.RemoveWork(s)
                Next

                Me.m_processing.Clear()
                Me.m_pending.Clear()

                Me.Output.Workload = Nothing
            End If

            Me.m_workload = w
            Me.Output.Workload = w

            If (Me.m_workload IsNot Nothing) Then
                Me.m_pending.AddRange(Me.m_workload.Jobs)
                Me.TryAssignWork()
            End If

        End SyncLock

    End Sub

    Public Function IsRunning() As Boolean
        If (Me.m_workload Is Nothing) Then Return False
        Return ((Me.m_pending.Count + Me.m_processing.Count) > 0)
    End Function

#End Region ' Public access

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

    ''' <summary>
    ''' Callback for the server-side job runner.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="args"></param>
    Private Sub OnJobRunStatusChanged(sender As cJobRunner, args As cJobRunnerStatusEventArgs)
        ' Me.UpdateWork(....) 
    End Sub

#End Region ' JobRunner integration

#Region " Triggers "

    ' Only trigger methods are synclocked as the intermedium between event handling and execution

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Client added event handler.
    ''' </summary>
    ''' <param name="id"></param>
    ''' -----------------------------------------------------------------------
    Private Sub OnClientAdded(id As Int32)
        SyncLock (Me.m_lock)
            If (Not Me.m_clientStates.ContainsKey(id)) Then
                Me.AddClient(id)
            End If
        End SyncLock
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Client removed event handler.
    ''' </summary>
    ''' <param name="id"></param>
    ''' -----------------------------------------------------------------------
    Private Sub OnClientRemoved(id As Int32)
        SyncLock (Me.m_lock)
            If (Me.m_clientStates.ContainsKey(id)) Then
                Me.RemoveClient(id)
            End If
        End SyncLock
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Stub; process data from the client. To be replaced by server event handling
    ''' </summary>
    ''' <param name="id"></param>
    ''' -----------------------------------------------------------------------
    Public Sub OnClientData(id As Int32, update As cJobRunUpdate)
        SyncLock (Me.m_lock)
            If (Not Me.m_processing.ContainsKey(id)) Then Return
            Dim s As cClientState = Me.m_clientStates(id)
            If (Not Guid.Equals(update.ExecutionID, s.ExecutionID)) Then Return
            Me.UpdateWork(update, s)
        End SyncLock
    End Sub

#End Region ' Triggers

#Region " Internals "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' A client has been added. Try to send it work.
    ''' </summary>
    ''' <param name="id"></param>
    ''' -----------------------------------------------------------------------
    Private Sub AddClient(id As Int32)

        Me.m_clientStates(id) = New cClientState(id)

        Debug.WriteLine("Scheduler: Added client " & id)

        Me.TryAssignWork()

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' A client has been removed. Re-queue any job that was running on it.
    ''' </summary>
    ''' <param name="id"></param>
    ''' -----------------------------------------------------------------------
    Private Sub RemoveClient(id As Int32)

        If Me.m_clientStates.ContainsKey(id) Then
            Dim state As cClientState = Me.m_clientStates(id)
            If (state.Job IsNot Nothing) Then

                Debug.WriteLine("Scheduler: Removed working client {0}, re-queued job {1}", id, state.Job.Name)

                Me.m_pending.Add(state.Job)
            Else

                Debug.WriteLine("Scheduler: Removed idle client " & id)

            End If
            Me.RemoveWork(state)
            Me.m_clientStates.Remove(id)
        End If
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Try to assign work to any idle client.
    ''' </summary>
    ''' <returns>True if any work was assigned.</returns>
    ''' -----------------------------------------------------------------------
    Private Function TryAssignWork() As Boolean

        Dim bAssigned As Boolean = False

        For Each cl As Int32 In Me.m_clientStates.Keys

            Dim s As cClientState = Me.m_clientStates(cl)

            ' If client is idle and there is work to do:
            If (s.IsAssigned = False And Me.m_pending.Count > 0) Then

                ' Assign the next available job to this client

                ' Note that this is logical but can be super inefficient.
                ' - Load balancing could be considered here
                ' - A client should not attempt to run a job that failed on it before
                ' - etc.
                Me.AssignWork(Me.m_clientStates(cl), Me.m_pending(0))

                bAssigned = True
            End If
        Next

        Return bAssigned

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Assign a specific job to a specific client.
    ''' </summary>
    ''' <param name="state"></param>
    ''' <param name="job"></param>
    ''' -----------------------------------------------------------------------
    Private Sub AssignWork(state As cClientState, job As cJob)

        ' Calling this method with a non-job terminates any client activity
        If (job Is Nothing) Then
            Me.RemoveWork(state)
            Return
        End If

        state.Assign(job)
        Me.m_pending.Remove(job)
        Me.m_processing(state.machineID) = state

        If state.machineID = Me.Server.ID Then
            Me.m_runner.Run(Me.Server.ID, New cJobRunRequest(state.ExecutionID, job))
        Else
            Me.Server.Send(New cJobRunRequest(state.ExecutionID, job), state.machineID)
        End If

    End Sub

    Private Sub RemoveWork(state As cClientState)

        If Not Me.m_clientStates.ContainsKey(state.machineID) Then Return

        Dim s As cClientState = Me.m_clientStates(state.machineID)
        Dim guid As Guid = s.ExecutionID
        Me.m_processing.Remove(state.machineID)

        s.Clear()

    End Sub

    Private Sub UpdateWork(update As cJobRunUpdate, state As cClientState)

        If (Not Me.IsRunning) Then Return

        Try

            state.RunState = update.RunState
            Me.Output.Update(state.Job, update.TaskRunStates, state.machineID)

            Select Case update.RunState

                Case eRunState.Idle, eRunState.Assigned, eRunState.WaitingForData, eRunState.Running
                    ' Record this
                    state.RunState = update.RunState

                Case Else
                    ' Job has finished, either with success or an error
                    Dim job As cJob = state.Job

                    ' First, remove it from the running stack
                    Me.RemoveWork(state)

                    ' If a job failed, it should be re-queued
                    If update.RunState > eRunState.Success Then
                        ' There is room to add smarts here:
                        ' - ensure that a job is not dispatched to the same client that it failed on?
                        ' - Flag a failing client as badly configured and skip it?
                        Me.m_pending.Add(job)
                    End If

                    ' Oh, and what to do if there are no clients anymore? Wait forever? Timeout?

                    ' When last task is done, we're ready to fire off the clean-up jobs
                    If ((Me.m_pending.Count + Me.m_processing.Count) = 0) Then

                        ' Assign wrap-up task to the server. The wrap-up task may result in 
                        ' a new workload, therefore, move the current workload out of the way.
                        ' If this code becomes Async (which it should be) then the pre-emptive
                        ' workload removal is not strictly necessary
                        Dim w As cWorkload = Me.m_workload
                        Me.SetWork(Nothing)

                        If (Me.m_runner IsNot Nothing) Then
                            Dim guidWrapUp As Guid = Guid.NewGuid()
                            Dim request As cJobRunRequest = Nothing

                            If (Me.Output.RunState = eRunState.Success) Then
                                request = New cJobRunRequest(guidWrapUp, w.JobSuccess)
                            Else
                                request = New cJobRunRequest(guidWrapUp, w.JobError)
                            End If
                            Me.m_runner.Run(Me.Server.ID, request)
                        End If
                    Else
                        Me.TryAssignWork()
                    End If

            End Select
        Catch ex As Exception

        End Try

    End Sub

#End Region ' Internals

End Class

