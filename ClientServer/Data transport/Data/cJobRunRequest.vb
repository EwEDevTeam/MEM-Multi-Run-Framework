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
''' Job run request from server to clients. This data is taken by a job runner
''' to start execution. In return, <see cref="cJobRunUpdate"/> messages will be
''' sent back to inform how the job execution is progressing.
''' </summary>
''' <seealso cref="cJobRunUpdate"/>
''' <seealso cref="cJobRunner"/>
''' ===========================================================================
<Serializable()>
Public Class cJobRunRequest
    Inherits cSerializableObject

#Region " Construction / destruction "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Send a request for a client to execute a job.
    ''' </summary>
    ''' <param name="p_executionID">Execution ID as assigned on the server.</param>
    ''' <param name="p_job">The job that should be executed.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(p_executionID As Guid, p_job As cJob)
        Me.ExecutionID = p_executionID
        Me.Job = p_job
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Send a request for a client to cancel a given job.
    ''' </summary>
    ''' <param name="executionID">Execution ID of the job to cancel.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(executionID As Guid)
        Me.ExecutionID = executionID
        Me.Job = Nothing
    End Sub

#End Region ' Construction / destruction

#Region " Public access "

    ''' <summary>The unique run state identifier.</summary>
    Public ReadOnly Property ExecutionID As Guid = Guid.Empty
    ''' <summary>The job that is being executed.</summary>
    Public ReadOnly Property Job As cJob = Nothing

#End Region ' Public access

#Region " Serialization "

    Protected Sub New(info As SerializationInfo, context As StreamingContext)
        MyBase.New(info, context)
        Try
            Me.ExecutionID = CType(info.GetValue("id", GetType(Guid)), Guid)
            Me.Job = CType(info.GetValue("job", GetType(cJob)), cJob)
        Catch ex As Exception
            Debug.Assert(False, String.Format("Exception '{0}' while deserializing cJobRunRequest", ex.Message))
        End Try
    End Sub

    Protected Overrides Sub GetObjectData(info As SerializationInfo, context As StreamingContext)
        MyBase.GetObjectData(info, context)
        info.AddValue("id", Me.ExecutionID)
        info.AddValue("job", Me.Job)
    End Sub

#End Region ' Serialization

End Class