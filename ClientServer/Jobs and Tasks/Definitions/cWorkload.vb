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

#End Region ' Imports

''' ==========================================================================
''' <summary>
''' A workload consists of a number of jobs that need executing across the
''' remote execution framework.
''' </summary>
''' <remarks>
''' Workload is file serializable.
''' </remarks>
''' ==========================================================================
<Serializable()>
Public Class cWorkload
    Inherits cSerializableObject

#Region " Private vars "

    ' NOP

#End Region ' Private vars

#Region " Construction / destruction "

    Public Sub New()
        MyBase.New()
    End Sub

#End Region ' Construction / destruction

#Region " Public access "

    ''' <summary>Client-side jobs that make up the workload.</summary>
    Public ReadOnly Property Jobs As New List(Of cJob)

    ''' <summary>The server side job that needs executing when a workload succeeded.</summary>
    Public Property JobSuccess As cJob = Nothing

    ''' <summary>The server side job that needs executing when a workload failed.</summary>
    Public Property JobError As cJob = Nothing

#End Region ' Public access

#Region " Serialization "

    Protected Sub New(info As SerializationInfo, context As StreamingContext)
        MyBase.New(info, context)
        Try
            Dim t As Type = GetType(cTask)
            Me.Jobs = CType(info.GetValue("jobs", GetType(List(Of cJob))), List(Of cJob))
            Me.JobSuccess = CType(info.GetValue("task_success", GetType(cTask)), cJob)
            Me.JobError = CType(info.GetValue("task_error", GetType(cTask)), cJob)
        Catch ex As Exception
            Debug.Assert(False, String.Format("Exception '{0}' while deserializing cJob", ex.Message))
        End Try
    End Sub

    Protected Overrides Sub GetObjectData(info As System.Runtime.Serialization.SerializationInfo, context As System.Runtime.Serialization.StreamingContext)
        MyBase.GetObjectData(info, context)
        info.AddValue("jobs", Me.Jobs, GetType(List(Of cJob)))
        info.AddValue("task_success", Me.JobSuccess, GetType(cJob))
        info.AddValue("task_error", Me.JobError, GetType(cJob))
    End Sub

#End Region ' Serialization

End Class
