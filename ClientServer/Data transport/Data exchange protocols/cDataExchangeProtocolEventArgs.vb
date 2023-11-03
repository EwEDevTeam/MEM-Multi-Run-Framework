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

#End Region ' Imports

Public Class cDataExchangeProtocolEventArgs
    Inherits EventArgs

    Public Enum eEventType As Integer
        Status
        Data
    End Enum

    Public Sub New(eventType As eEventType, data As cSerializableObject, sender As Int32, message As ULong)
        Me.EventType = eventType
        Me.Data = data
        Me.Sender = sender
        Me.Message = message
    End Sub

    Public Sub New(eventType As eEventType, status As eClientServerStatus, sender As Int32, message As ULong)
        Me.EventType = eventType
        Me.Status = status
        Me.Sender = sender
        Me.Message = message
    End Sub

    Public ReadOnly Property EventType As eEventType
    Public ReadOnly Property Data As cSerializableObject
    Public ReadOnly Property Status As eClientServerStatus
    Public ReadOnly Property Sender As Int32
    Public ReadOnly Property Message As ULong

End Class
