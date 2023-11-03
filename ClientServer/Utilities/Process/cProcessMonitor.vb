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

''' ===========================================================================
''' <summary>
''' Monitor whether a process has become unresponsive, and if so, optionally 
''' kill the process and all of its children. Note that the child process 
''' detection only works on Windows; need to find a way to implement this on 
''' Linux somehow.
''' </summary>
''' <remarks>
''' Based on https://www.hurryupandwait.io/blog/detecting-a-hung-windows-process
''' </remarks>
''' ===========================================================================
Public Class cProcessMonitor

#Region " Internals "

    Private m_name As String = ""
    Private m_process As Process = Nothing
    Private m_timer As Timer = Nothing
    Private m_lastMemUsage As Long = 0
    Private m_lastCheck As Long = 0
    Private m_heartbeat As eProcessHeartbeat
    Private m_intervalKill As Integer = 30000

#End Region ' Internals

    Public Sub New()
        Me.m_timer = New Timer(AddressOf OnCheckHeartbeat, Nothing, Timeout.Infinite, Timeout.Infinite)
    End Sub

    Public Enum eProcessHeartbeat As Integer
        ''' <summary>Process was alive at most recent heartbeat check.</summary>
        Alive = 0
        ''' <summary>Process did not have a pulse at the most recent heartbeat check.</summary>
        NoPulse
        ''' <summary>Process did not have a pulse for a number of heartbeat checks.</summary>
        Deceased
    End Enum

    Public Event OnProcessHeartbeat(sender As Object, args As cProcessMonitorEventArgs)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Start monitoring a started process.
    ''' </summary>
    ''' <param name="p">The <see cref="Process"/> to monitor.</param>
    ''' <param name="interval">Heartbeat interval, in ms.</param>
    ''' <param name="intervalKill">Process kill interval, in ms. Set to <see cref="Timer.Infinite"/>
    ''' to disable process killing.</param>
    ''' -----------------------------------------------------------------------
    Public Sub Attach(p As Process, interval As Long, intervalKill As Long)

        If (Me.m_process Is Nothing) Then

            Me.m_process = p
            Me.m_heartbeat = eProcessHeartbeat.Alive ' Think positive
            Me.m_timer.Change(interval, interval)
            Me.m_name = "ProcMon@" & CStr(p.Id)

        End If

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Stop monitoring a process.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub Detach()

        If (Me.m_process IsNot Nothing) Then
            Me.m_process = Nothing
            Me.m_timer.Change(Timeout.Infinite, Timeout.Infinite)
        End If

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Try to abort the monitored process.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub Abort()

        If (Me.m_process IsNot Nothing) Then
            Try
                Me.m_process.Kill(True)
            Catch ex As Exception

            End Try
        End If

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get whether the last monitored process had become unresponsive.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Heartbeat As eProcessHeartbeat
        Get
            Return Me.m_heartbeat
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return Me.m_name
    End Function

#Region " Internals "

    Private Sub OnCheckHeartbeat(state As Object)

        If (Me.m_process Is Nothing) Then Return

        If Me.m_process.HasExited Then
#If DEBUG Then
            Debug.WriteLine("ProcessMonitor: {0} process done", Me)
#End If
            Me.Detach()
            Return
        End If

        Dim memUsage As Long = Me.MemUsage(Me.m_process)

        If (memUsage <> Me.m_lastMemUsage) Then

#If VERBOSE_LEVEL >= 2 Then
            Debug.WriteLine("{0} mem now at {1}", Me, memUsage)
#End If
            Me.m_lastMemUsage = memUsage
            Me.m_lastCheck = Date.Now.Ticks

            If (Me.m_heartbeat <> eProcessHeartbeat.Alive) Then
                Me.m_heartbeat = eProcessHeartbeat.Alive
                Me.BroadcastDiagnosis()
            End If
        Else
            Dim ts As New TimeSpan(Date.Now.Ticks - Me.m_lastCheck)
            If (ts.Milliseconds < Me.m_intervalKill) Then
#If VERBOSE_LEVEL >= 2 Then
                Console.WriteLine("{0} mem not changed", Me)
#End If
                If (Me.m_heartbeat <> eProcessHeartbeat.NoPulse) Then
                    Me.m_heartbeat = eProcessHeartbeat.NoPulse
                    Me.BroadcastDiagnosis()
                End If
            Else
                ' Stop checking, patient has left the planet
#If DEBUG Then
                Debug.WriteLine("ProcessMonitor: {0} process presumed dead", Me)
#End If
                Me.m_timer.Change(Timeout.Infinite, Timeout.Infinite)
                Me.m_heartbeat = eProcessHeartbeat.Deceased
                Me.BroadcastDiagnosis()
                Me.Detach()
            End If
        End If

    End Sub

    Private Function MemUsage(p As Process) As Long

        Dim mem As Long = 0
        If Not (p.HasExited) Then

            Try
                mem = p.PrivateMemorySize64 + p.WorkingSet64
                For Each pChild In p.GetChildProcesses()
                    mem += MemUsage(pChild)
                Next
            Catch ex As Exception
                ' Whoah
            End Try
        End If
        Return mem

    End Function

    Private Sub BroadcastDiagnosis()
        Try
            RaiseEvent OnProcessHeartbeat(Me, New cProcessMonitorEventArgs(Me.m_process, Me.m_heartbeat))
        Catch ex As Exception

        End Try
    End Sub

#End Region ' Internals

End Class