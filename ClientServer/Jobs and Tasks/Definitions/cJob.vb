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
Imports System.Xml.Serialization

#End Region ' Imports

''' ==========================================================================
''' <summary>
''' A job consists of a linear sequence of tasks. A job is dispatched in its 
''' entirety to a remote machine.
''' </summary>
''' ==========================================================================
<Serializable()>
<XmlType("Job")>
Public Class cJob
    Inherits cSerializableObject

#Region " Private vars "

    ' NOP

#End Region ' Private vars

#Region " Construction "

    Public Sub New()
        MyBase.New()
    End Sub

    Public Sub New(name As String)
        Me.New()
        Me.Name = name
    End Sub

#End Region ' Construction

#Region " Public access "

    ''' <summary>Tasks that make up the job.</summary>
    Public ReadOnly Property Tasks As New List(Of cTask)

    ''' <summary>Alias of the folder to watch for input data.</summary>
    Public ReadOnly Property InputFolderAlias As String = ""

    ''' <summary>Input files that need to be present for a job to run.</summary>
    ''' <remarks>File names shown here are appended to the <see cref="InputFolderAlias"/></remarks>
    Public ReadOnly Property Inputs As New List(Of String)

    Public Sub Add(t As cTask)
        Me.Tasks.Add(t)
    End Sub

    Public Overrides Function ToString() As String
        Return Me.Name
    End Function

#End Region ' Public access

#Region " Serialization "

    Protected Sub New(info As SerializationInfo, context As StreamingContext)
        MyBase.New(info, context)
        Try
            Dim t As Type = GetType(cTask)
            Me.Tasks = CType(info.GetValue("tasks", GetType(List(Of cTask))), List(Of cTask))
            Me.Inputs = CType(info.GetValue("inputs", GetType(List(Of String))), List(Of String))
            Me.InputFolderAlias = info.GetString("inputfolderalias")

            ' Fix path characters on incoming deserialized path to the local operating system
            For i As Integer = 0 To Me.Inputs.Count - 1
                Me.Inputs(i) = cFileUtils.FixPathCharacters(Me.Inputs(i))
            Next
        Catch ex As Exception
            Debug.Assert(False, String.Format("Exception '{0}' while deserializing cJob", ex.Message))
        End Try
    End Sub

    Protected Overrides Sub GetObjectData(info As System.Runtime.Serialization.SerializationInfo, context As System.Runtime.Serialization.StreamingContext)
        MyBase.GetObjectData(info, context)
        info.AddValue("tasks", Me.Tasks, GetType(List(Of cTask)))
        info.AddValue("inputs", Me.Inputs, GetType(List(Of String)))
        info.AddValue("inputfolderalias", Me.InputFolderAlias)
    End Sub

#End Region ' Serialization

End Class

