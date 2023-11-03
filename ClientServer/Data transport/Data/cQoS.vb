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

''' ===========================================================================
''' <summary>
''' Quality of Service data package.
''' </summary>
''' ===========================================================================
<Serializable()>
Friend Class cQoS
    Inherits cSerializableObject

    Public Sub New(QoS As eQoS)
        Me.QoS = QoS
    End Sub

    Protected Sub New(info As SerializationInfo, context As StreamingContext)
        MyBase.New(info, context)
        Try
            Me.QoS = DirectCast(info.GetInt32("QoS"), eQoS)
        Catch ex As Exception
            Debug.Assert(False, String.Format("Exception '{0}' while deserializing cFileDataPackage", ex.Message))
        End Try
    End Sub

    Protected Overrides Sub GetObjectData(info As System.Runtime.Serialization.SerializationInfo, context As System.Runtime.Serialization.StreamingContext)
        MyBase.GetObjectData(info, context)
        info.AddValue("QoS", Me.QoS, GetType(Int32))
    End Sub

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the QoS payload. Yeah, it's just a single enum value. Really.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public ReadOnly Property QoS As eQoS

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' For debugging purposes.
    ''' </summary>
    ''' -------------------------------------------------------------------
    Public Overrides Function ToString() As String
        Return "QoS " & Me.QoS.ToString()
    End Function

End Class
