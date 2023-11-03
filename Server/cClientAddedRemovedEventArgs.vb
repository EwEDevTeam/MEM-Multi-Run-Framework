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
''' <see cref="cServer"/> event data, sent out when a remote client has been 
''' added or removed.
''' </summary>
''' ===========================================================================
Public Class cClientAddedRemovedEventArgs
    Inherits EventArgs

    Public Sub New(id As Int32, status As eClientServerStatus, protocolname As String)
        Me.ClientID = id
        Me.Status = status
        Me.ProtocolName = protocolname
    End Sub

    ''' <summary>
    ''' Get the ID of the remote client that was added or removed.
    ''' </summary>
    Public ReadOnly Property ClientID As Int32

    ''' <summary>
    ''' Get whether the remote client <see cref="eClientServerStatus.Connected"/> or
    ''' <see cref="eClientServerStatus.Disconnected"/>.
    ''' </summary>
    Public ReadOnly Property Status As eClientServerStatus

    ''' <summary>
    ''' Get the name of the <see cref="cDataExchangeProtocol"/>. You might want to know that.
    ''' </summary>
    Public ReadOnly Property ProtocolName As String

End Class

