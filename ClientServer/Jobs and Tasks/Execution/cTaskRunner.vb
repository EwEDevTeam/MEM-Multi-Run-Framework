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
Imports System.Text
Imports System.Threading

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Foundation class for executing a task. This class provides the foundation for 
''' collecting (std)out and (std)error information relevant to the framework
''' user.
''' </summary>
''' ===========================================================================
Public MustInherit Class cTaskRunner

#Region " Privates "

    ''' <summary>Event marshaller dude thingy. Yeah.</summary>
    Private m_syncObject As SynchronizationContext = Nothing
    ''' <summary>Thread that the task is executed on.</summary>
    Private m_thread As Thread = Nothing
    ''' <summary>Task execution status code.</summary>
    Private m_status As eRunState = eRunState.Assigned
    ''' <summary>Task execution summary, if any.</summary>
    Private m_activity As String = ""
    ''' <summary>Intercepted standard error output.</summary>
    Private m_errorlog As StringBuilder = Nothing
    ''' <summary>Intercepted standard output.</summary>
    Private m_outputlog As StringBuilder = Nothing

#End Region ' Privates

    Public Sub New(parms As String)
        Me.Parameters = parms
        Me.m_syncObject = New SynchronizationContext()
        Me.m_outputlog = New StringBuilder()
        Me.m_errorlog = New StringBuilder()
    End Sub

#Region " Public access "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get parameters for the <see cref="cTask"/> to run.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Parameters As String

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Start the task.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overridable Function Start() As Boolean

        If (Me.IsRunning) Then Return False

        Me.Abort = False
        Me.m_thread = New Thread(AddressOf RunThread)
        Me.m_thread.Start()
        Return True

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the run abort flag.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overridable Property Abort As Boolean

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Poll for status
    ''' </summary>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Overridable ReadOnly Property IsRunning As Boolean
        Get
            If (Me.m_thread Is Nothing) Then Return False
            Return (Me.m_thread.IsAlive)
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event to notify that the task completed. To query the task execution result,
    ''' see <see cref="Status"/>, <see cref="ErrorLog"/>
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Event OnTaskCompleted(sender As cTaskRunner)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the last relevant activity of the task.
    ''' </summary>
    ''' <remarks>
    ''' If no activity has been provided this will return a string representation
    ''' of the current <see cref="Status"/>.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Property LastActivity() As String
        Get
            If Not String.IsNullOrWhiteSpace(Me.m_activity) Then
                Return Me.m_activity
            End If
            Return Me.m_status.ToString()
        End Get
        Set(value As String)
            Me.m_activity = value
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="eRunState">task runtime status</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Property Status() As eRunState
        Get
            Return Me.m_status
        End Get
        Protected Set(value As eRunState)
            Me.m_status = value
            ' No need to send out events; this info is needed on request only
        End Set
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Errors produced during execution
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property ErrorLog() As String
        Get
            Return Me.m_errorlog.ToString()
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Output produced during execution
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property OutputLog() As String
        Get
            Return Me.m_outputlog.ToString()
        End Get
    End Property

#End Region ' Public access

#Region " Internals "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Archive (std)out information into the local log.
    ''' </summary>
    ''' <param name="text"></param>
    ''' -----------------------------------------------------------------------
    Protected Sub LogOutput(text As String)
        Me.m_outputlog.AppendLine(text)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Archive (std)error information into the local log.
    ''' </summary>
    ''' <param name="text"></param>
    ''' -----------------------------------------------------------------------
    Protected Sub LogError(text As String)
        Me.m_errorlog.AppendLine(text)
    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Override this to implement the actual execution of the task.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Protected MustOverride Function RunTask() As eRunState

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="status"></param>
    ''' <param name="sender">The <see cref="cDataExchangeProtocol.ID"/> of the sender.</param>
    ''' <param name="message">The <see cref="cDataPackage.Sequence"/> of the data package.</param>
    ''' -------------------------------------------------------------------
    Protected Sub SendTaskCompletedEvent()
        Me.m_syncObject.Send(New SendOrPostCallback(AddressOf DoSendEvent), Nothing)
    End Sub

    Private Sub RunThread()

        Try
            Me.m_errorlog.Clear()
            Me.m_outputlog.Clear()

#If DEBUG Then
            Me.DumpConfiguration()
#End If
            Me.Status = Me.RunTask()

        Catch ex As Exception
            Me.Status = eRunState.ErrorCrashed
            Me.LogError("# cTaskRunner.RunThread(): " & ex.Message)
        End Try
        Me.SendTaskCompletedEvent()

    End Sub

    Private Sub DoSendEvent(state As Object)
        Try
            RaiseEvent OnTaskCompleted(Me)
        Catch ex As Exception
            ' Whoah
            Me.LogError("# cTaskRunner.DoSendEvent(): " & ex.Message)
        End Try
    End Sub

#If DEBUG Then
    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Write out the runner configuration
    ''' </summary>
    ''' -------------------------------------------------------------------
    Protected MustOverride Sub DumpConfiguration()
#End If

#End Region ' Internals

End Class
