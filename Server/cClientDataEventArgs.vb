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
Imports ClientServer

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' <see cref="cServer"/> event data, sent out when data has been received from
''' a remote client.
''' </summary>
''' ===========================================================================
Public Class cClientDataEventArgs
    Inherits EventArgs

    Public Sub New(id As Int32, data As cSerializableObject)
        Me.ClientID = id
        Me.Data = data
    End Sub

    ''' <summary>
    ''' Get the ID of the remote client that sent the data.
    ''' </summary>
    Public ReadOnly Property ClientID As Int32

    ''' <summary>
    ''' Get the data that was sent.
    ''' </summary>
    Public ReadOnly Property Data As cSerializableObject

End Class

