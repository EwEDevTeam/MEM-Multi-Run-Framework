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
''' A tasks is a single execution, involving executing some code to transform
''' input data into output data.
''' </summary>
''' ==========================================================================
<Serializable()>
<XmlType("Task")>
Public Class cTask
    Inherits cSerializableObject

    Public Sub New()
        MyBase.New()
    End Sub

    Public Sub New(name As String, [alias] As String, parms As String)
        Me.Name = name
        Me.Alias = [alias]
        Me.Parameters = parms
    End Sub

    Public Sub New([alias] As String, parms As String)
        Me.Name = "Task" & [alias]
        Me.Alias = [alias]
        Me.Parameters = parms
    End Sub

    ''' <summary>Alias of the application to run for the task</summary>
    Public Property [Alias] As String = ""

    ''' <summary>Parameters to provide to the executable.</summary>
    Public Property Parameters As String = ""

    Public Overrides Function ToString() As String
        Return String.Format("[{0}]", Me.Alias)
    End Function

#Region " Serialization "

    Protected Sub New(info As SerializationInfo, context As StreamingContext)
        MyBase.New(info, context)
        Try
            Me.Alias = info.GetString("alias")
            ' Fix path characters on incoming deserialized parameters to the local operating system
            Me.Parameters = cFileUtils.FixPathCharacters(info.GetString("params"))

        Catch ex As Exception
            Debug.Assert(False, String.Format("Exception '{0}' while deserializing cTask", ex.Message))
        End Try
    End Sub

    Protected Overrides Sub GetObjectData(info As System.Runtime.Serialization.SerializationInfo, context As System.Runtime.Serialization.StreamingContext)
        MyBase.GetObjectData(info, context)
        info.AddValue("alias", Me.Alias, GetType(Int64))
        info.AddValue("params", Me.Parameters, GetType(Int64))
    End Sub

#End Region ' Serialization

End Class
