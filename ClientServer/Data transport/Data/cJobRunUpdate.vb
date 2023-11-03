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
Imports System.Runtime.Serialization
Imports ClientServer

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Job run update, created by a <see cref="cJobRunner"/> to keep the world up
''' to date about the job execution.
''' </summary>
''' ===========================================================================
<Serializable()>
Public Class cJobRunUpdate
    Inherits cSerializableObject

#Region " Construction / destruction "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Yo.
    ''' </summary>
    ''' <param name="executionID">Execution ID as assigned on the server</param>
    ''' <param name="clientID">Client ID</param>
    ''' <param name="taskRunStates">Run states of the individual tasks.</param>
    ''' <param name="runstate">The state of the job run.</param>
    ''' <param name="bCompleted">Flag, stating if the job is completed.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(executionID As Guid, taskRunStates As eRunState(), runstate As eRunState, bCompleted As Boolean)
        Me.ExecutionID = executionID
        Me.TaskRunStates = taskRunStates
        Me.RunState = runstate
        Me.HasCompleted = bCompleted
    End Sub

#End Region ' Construction / destruction

#Region " Public access "

    ''' <summary>The unique run state identifier.</summary>
    ReadOnly Property ExecutionID As Guid
    ''' <summary>The state of each task.</summary>
    Property TaskRunStates As eRunState()
    ''' <summary>The final state of the job.</summary>
    Property RunState As eRunState
    ''' <summary>Flag to declare whether the run is done.</summary>
    Public Property HasCompleted As Boolean

#End Region ' Public access

#Region " Serialization "

    Protected Sub New(info As SerializationInfo, context As StreamingContext)
        MyBase.New(info, context)
        Try
            Me.ExecutionID = CType(info.GetValue("id", GetType(Guid)), Guid)
            Me.TaskRunStates = CType(info.GetValue("taskstates", GetType(eRunState())), eRunState())
            Me.RunState = CType(info.GetValue("state", GetType(eRunState)), eRunState)
            Me.HasCompleted = info.GetBoolean("hascompleted")
        Catch ex As Exception
            Debug.Assert(False, String.Format("Exception '{0}' while deserializing cJobRunUpdate", ex.Message))
        End Try
    End Sub

    Protected Overrides Sub GetObjectData(info As SerializationInfo, context As StreamingContext)
        MyBase.GetObjectData(info, context)
        info.AddValue("id", Me.ExecutionID)
        info.AddValue("taskstates", Me.TaskRunStates)
        info.AddValue("state", Me.RunState)
        info.AddValue("hascompleted", Me.HasCompleted)
    End Sub

#End Region ' Serialization

End Class