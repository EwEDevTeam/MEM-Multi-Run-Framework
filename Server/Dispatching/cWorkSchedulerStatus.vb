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
Imports System.Data
Imports System.Threading
Imports ClientServer
Imports ClientServer.modDefinitions

#End Region ' Imports

''' <summary>
''' Progress of a <see cref="cWorkScheduler"/>. This class maintains a datatable 
''' for display in a UI, and retains the over-all run state of a workload.
''' </summary>
Public Class cWorkSchedulerStatus

#Region " Private vars "

    Private m_workload As cWorkload = Nothing
    Private m_syncObject As SynchronizationContext = Nothing
    Private m_runstate As eRunState = eRunState.Idle

    Private m_tableOutput As New DataTable()
    Private m_tableJobLookup As New Dictionary(Of String, Integer)

#End Region ' Private vars

#Region " Construction / destruction "

    Public Sub New()
        Me.m_tableOutput.Columns.Add("Job", GetType(String))
        Me.m_tableOutput.Columns.Add("Task", GetType(String))
        Me.m_tableOutput.Columns.Add("Client", GetType(Int32))
        Me.m_tableOutput.Columns.Add("Status", GetType(eRunState))

        ' Get sync object
        Me.m_syncObject = SynchronizationContext.Current
        If (Me.m_syncObject Is Nothing) Then
            Me.m_syncObject = New SynchronizationContext()
        End If

    End Sub

#End Region ' Construction / destruction

#Region " Public access "

    Public Property Workload As cWorkload
        Get
            Return Me.m_workload
        End Get
        Friend Set(value As cWorkload)

            If (Me.m_workload IsNot Nothing) Then
                Me.m_tableOutput.Rows.Clear()
                Me.m_tableJobLookup.Clear()
                Me.m_runstate = eRunState.Idle
            End If

            Me.m_workload = value

            If (Me.m_workload IsNot Nothing) Then
                Dim nRows As Integer = 0

                For i As Integer = 0 To Me.m_workload.Jobs.Count - 1
                    Dim job As cJob = Me.m_workload.Jobs(i)
                    For j As Integer = 0 To job.Tasks.Count - 1
                        Dim task As cTask = job.Tasks(j)
                        Me.m_tableOutput.Rows.Add(job.Name, task.Name, 0, eRunState.Idle)
                        If (j = 0) Then
                            Me.m_tableJobLookup(job.Name) = nRows
                        End If
                        nRows += 1
                    Next j
                Next i
            End If

        End Set
    End Property

    Public ReadOnly Property RunState As eRunState
        Get
            Return Me.m_runstate
        End Get
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
    Public Event OnWorkloadStatusChanged(sender As cWorkSchedulerStatus, args As EventArgs)


#End Region ' Events 

#Region " Internals "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Helper method: get the index of the first job row in the output datatable.
    ''' </summary>
    ''' <param name="name"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Private Function GetJobRow(name As String) As Integer
        If Not Me.m_tableJobLookup.ContainsKey(name) Then
            Debug.Assert(False)
            Return -666 ' Because we're evil
        End If
        Return Me.m_tableJobLookup(name)
    End Function

    Friend Sub Update(job As cJob, taskstates As eRunState(), machineID As Int32)

        Return

        Dim i As Integer = Me.GetJobRow(job.Name)
        Dim bChanged As Boolean = False

        For j As Integer = 0 To job.Tasks.Count - 1
            If i >= 0 Then
                Dim drow As DataRow = Me.m_tableOutput.Rows(i + j)

                If (machineID <> CInt(drow.Item(2))) Then
                    drow.Item(2) = machineID
                    bChanged = True
                End If

                Dim newState As eRunState = DirectCast(Math.Max(DirectCast(drow.Item(3), eRunState), taskstates(j)), eRunState)
                If (newState <> DirectCast(drow.Item(3), eRunState)) Then
                    drow.Item(3) = newState
                    bChanged = True
                End If

            End If
            Me.m_runstate = CType(Math.Max(Me.m_runstate, taskstates(i)), eRunState)
        Next

        If bChanged Then
            DoSendWorkloadStatusChangedEvent(Nothing)
        End If

    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="status"></param>
    ''' <param name="sender">The <see cref="cDataExchangeProtocol.ID"/> of the sender.</param>
    ''' <param name="message">The <see cref="cDataPackage.Sequence"/> of the data package.</param>
    ''' -------------------------------------------------------------------
    Private Sub SendWorkloadStatusChangedEvent()

        Return

        Me.m_syncObject.Send(New SendOrPostCallback(AddressOf DoSendWorkloadStatusChangedEvent), Nothing)
    End Sub

    Private Sub DoSendWorkloadStatusChangedEvent(o As Object)

        Try
            RaiseEvent OnWorkloadStatusChanged(Me, New EventArgs())
        Catch ex As Exception
            ' Whoah!
        End Try

    End Sub

#End Region ' Internals

End Class
