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
Imports System.Runtime.Serialization
Imports System.Text
Imports System.Xml
Imports System.Xml.Serialization

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Base class for implementing serializable data that is to be transported 
''' across the multi-run framework.
''' </summary>
''' ===========================================================================
<Serializable()>
Public MustInherit Class cSerializableObject
    Implements ISerializable

#Region " Constructors "

    Public Sub New()
        MyBase.New()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Deserialization constructor.
    ''' </summary>
    ''' <param name="info"></param>
    ''' <param name="context"></param>
    ''' -----------------------------------------------------------------------
    Protected Sub New(info As SerializationInfo, context As StreamingContext)
        Me.Name = info.GetString("name")
    End Sub

#End Region ' Constructors

#Region " Serialization Implementation "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Serializes the object.
    ''' </summary>
    ''' <param name="info"></param>
    ''' <param name="context"></param>
    ''' <remarks>
    ''' This takes care of all objects in the inheritance hierarchy. Derived classes 
    ''' should only override this method to add extra data.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Protected Overridable Sub GetObjectData(info As SerializationInfo, context As StreamingContext) _
        Implements ISerializable.GetObjectData
        info.AddValue("name", Me.Name, GetType(String))
    End Sub

#End Region ' Serialization Implementation

#Region " Public interfaces "

    ''' <summary>
    ''' Get/set the name of this serializable object. Imagine not having a name...
    ''' </summary>
    Public Property Name As String = ""

    Public Overrides Function ToString() As String
        Return Me.Name
    End Function

#End Region ' Public interfaces

#Region " XML serialization "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Deserialize an instance from XML.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="data"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Shared Function FromXML(Of T)(data As String) As T
        Dim x As New XmlSerializer(GetType(cSerializableObject))
        Dim r As New StringReader(data)
        Dim obj As T = Nothing
        Try
            obj = DirectCast(x.Deserialize(r), T)
        Catch ex As Exception

        End Try
        Return obj
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Sserialize an instance to XML.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="data"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Shared Function ToXML(data As cSerializableObject) As String
        Dim x As New XmlSerializer(data.GetType)
        Dim sb As New StringBuilder()
        Dim w As New StringWriter(sb)
        x.Serialize(w, data)
        Return sb.ToString
    End Function

#End Region ' XML serialization

End Class

