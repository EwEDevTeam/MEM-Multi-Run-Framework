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
Imports System.Reflection

#End Region ' Imports

''' -----------------------------------------------------------------------
''' <summary>
''' Factory to obtain a <see cref="cTaskRunner"/> for a given <see cref="cTask"/>.
''' </summary>
''' -----------------------------------------------------------------------
Public Class cTaskRunnerFactory

#Region " Privates "

    Private ReadOnly Property Settings As cSettings
    Private Shared ReadOnly Property EmbeddedTaskList As New List(Of String)

#End Region ' Privates

#Region " Construction "

    Public Sub New(settings As cSettings)
        Me.Settings = settings
    End Sub

#End Region ' Construction

#Region " Factory "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Task runnner factory.
    ''' </summary>
    ''' <param name="task">The <see cref="cTask"/> to obtain a runner for.</param>
    ''' <returns>A <see cref="cTaskRunner"/>, or <see cref="IsNothing(Object)"/> 
    ''' if an error occurred.</returns>
    ''' -----------------------------------------------------------------------
    Public Function GetTaskRunner(job As cJob, task As cTask) As cTaskRunner

        Dim appls As cApplicationAliases = Me.Settings.ApplicationSettings
        Dim flds As cFolderAliases = Me.Settings.FolderSettings
        Dim rnts As cRuntimeSettings = Me.Settings.RuntimeSettings
        Dim tr As cTaskRunner = Nothing

        Try

            Dim target As String = appls.Alias(task.Alias)
            Dim parms As String = flds.Expand(task.Parameters, job.Name, task.Name, rnts.MaxThreads)

            If (Not String.IsNullOrWhiteSpace(target)) Then

                If (File.Exists(target)) Then
                    tr = New cExecutableTaskRunner(target, parms, Me.Settings)
                Else
                    Dim t As Type = cTypeUtils.StringToType(target)
                    If (t IsNot Nothing) Then
                        If (t.IsAssignableTo(GetType(cEmbeddedTaskRunner))) Then
                            tr = CType(Activator.CreateInstance(t, New Object() {parms}), cTaskRunner)
                        End If
                    End If
                End If
            End If

        Catch ex As Exception
            ' Whoah
        End Try

        If (tr Is Nothing) Then
            Console.Error.WriteLine("! Task alias {0} could not be resolved", task.Alias)
        End If

        Return tr

    End Function

    Public Shared Function EmbeddedTasks() As String()
        If (cTaskRunnerFactory.EmbeddedTaskList.Count = 0) Then

            For Each r As Assembly In AppDomain.CurrentDomain.GetAssemblies()
                For Each t As Type In r.GetTypes()
                    If t.IsAssignableTo(GetType(cEmbeddedTaskRunner)) Then
                        cTaskRunnerFactory.EmbeddedTaskList.Add(cTypeUtils.TypeToString(t))
                    End If
                Next
            Next
        End If
        Return cTaskRunnerFactory.EmbeddedTaskList.ToArray()
    End Function

#End Region ' Factory

End Class
