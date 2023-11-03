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

#Const REDIRECT_OUT = 0

''' ===========================================================================
''' <summary>
''' Task that runs as an external executable through shell execution.
''' </summary>
''' ===========================================================================
Public Class cExecutableTaskRunner
    Inherits cTaskRunner

#Region " Private vars "

    Private m_monitor As New cProcessMonitor()

#End Region ' Private vars

#Region " Construction / destruction "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="exe">The path to the executable.</param>
    ''' <param name="params">The command line parameters for the executable.</param>
    ''' <param name="settings">The configuration settings.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(exe As String, params As String, settings As cSettings)
        MyBase.New(params)
        Me.Executable = exe
        Me.Settings = settings
    End Sub

#End Region ' Construction / destruction

#Region " Public access "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the path to the executable.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Executable As String

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the configuration settings.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Settings As cSettings

    ''' -----------------------------------------------------------------------
    ''' <inheritdoc cref="cTaskRunner.IsRunning"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property IsRunning As Boolean
        Get
            Return ((MyBase.IsRunning = True) And (Me.m_monitor.Heartbeat <> cProcessMonitor.eProcessHeartbeat.Deceased))
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdoc cref="cTaskRunner.ToString"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Function ToString() As String
        Return "Executable"
    End Function

    ''' -----------------------------------------------------------------------
    ''' <inheritdoc cref="cTaskRunner.Abort"/>
    ''' -----------------------------------------------------------------------
    Public Overrides Property Abort As Boolean
        Get
            Return MyBase.Abort
        End Get
        Set(value As Boolean)
            MyBase.Abort = value
            If ((value = True) And (Me.m_monitor IsNot Nothing)) Then
                Try
                    Me.m_monitor.Abort()
                Catch ex As Exception
                    ' Hmmm
                End Try
            End If
        End Set
    End Property

#End Region ' Public access

#Region " Internal execution "

#If DEBUG Then
    Protected Overrides Sub DumpConfiguration()
        Debug.WriteLine("- Exe   {0}", Me.Executable)
        Debug.WriteLine("- Parms {0}", Me.Parameters)
    End Sub
#End If

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Execute the remote task, monitor its process, intercept stdout and 
    ''' stderr output, and wait for task termination. Note that this call is 
    ''' blocking!
    ''' </summary>
    ''' <returns>The run state of the task.</returns>
    ''' -----------------------------------------------------------------------
    Protected Overrides Function RunTask() As eRunState

        Dim result As eRunState = eRunState.Assigned

        Using p As New Process()

            p.StartInfo.FileName = Me.Executable
            p.StartInfo.Arguments = Me.Parameters

#If REDIRECT_OUT Then
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
            p.StartInfo.CreateNoWindow = True
#Else
            p.StartInfo.WindowStyle = ProcessWindowStyle.Normal
            p.StartInfo.CreateNoWindow = False
#End If

            ' The section marked below looks totally convoluted but works like
            ' a charm. Logic is based on this article:
            ' https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo.redirectstandarderror?view=net-6.0
            ' == START DO NOT TOUCH THIS SECTION, IT JUST WORKS
            p.StartInfo.UseShellExecute = False

#If REDIRECT_OUT Then
            p.StartInfo.RedirectStandardError = True
            AddHandler p.ErrorDataReceived, AddressOf OnErrorDataReceived
            p.StartInfo.RedirectStandardOutput = True
#End If

            Try
                p.Start()
                result = eRunState.Running
            Catch ex As Exception
                result = eRunState.ErrorCrashed
            End Try

            If (result = eRunState.Running) Then

                Me.m_monitor.Attach(p, CInt(Me.Settings.RuntimeSettings.KillTimeout / 10), Me.Settings.RuntimeSettings.KillTimeout)
                AddHandler Me.m_monitor.OnProcessHeartbeat, AddressOf OnProcessHeartbeat

#If REDIRECT_OUT Then
                p.BeginErrorReadLine()
                Me.LogOutput(p.StandardOutput.ReadToEnd)
#End If

                p.WaitForExit()

                If (p.ExitCode <> 0) Then
                    result = eRunState.ErrorOther
                Else
                    result = eRunState.Success
                End If

                ' Free up resources. Henceforth p is unusable
                p.Close()

                RemoveHandler Me.m_monitor.OnProcessHeartbeat, AddressOf OnProcessHeartbeat
                Me.m_monitor.Detach()
            End If
            ' == END DO NOT TOUCH THIS SECTION, IT JUST WORKS

#If REDIRECT_OUT Then
            ' Clean up
            RemoveHandler p.ErrorDataReceived, AddressOf OnErrorDataReceived
            'RemoveHandler p.OutputDataReceived, AddressOf OnOutputDataReceived
#End If

        End Using

        Return result

    End Function

#End Region ' Internal execution

#Region " Internal event handling "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event handler, intercepts stderror data from a remote executable.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' -----------------------------------------------------------------------
    Private Sub OnErrorDataReceived(sender As Object, e As DataReceivedEventArgs)
        Me.LogError(e.Data)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Event handler, intercepts <see cref="cProcessMonitor"/> events to pulse-
    ''' check the remotely running executable.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="args"></param>
    ''' -----------------------------------------------------------------------
    Private Sub OnProcessHeartbeat(sender As Object, args As cProcessMonitorEventArgs)

        Select Case args.State
            Case cProcessMonitor.eProcessHeartbeat.Alive
                ' NOP

            Case cProcessMonitor.eProcessHeartbeat.NoPulse
                ' NOP

            Case cProcessMonitor.eProcessHeartbeat.Deceased
                args.Process.Kill()

        End Select

    End Sub

#End Region ' Internal event handling

End Class
