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

''' ===========================================================================
''' <summary>
''' Data exchange protocol for direct class-to-class communication. This type
''' of client-server communication is warranted when server and clients run on
''' different threads in the same executable.
''' </summary>
''' ===========================================================================
Public Class cDirectDataExchangeProtocol
    Inherits cDataExchangeProtocol

    Protected Sub New(ID As Int32, bIsServer As Boolean)
        MyBase.New(ID, bIsServer)
        Me.Name = "ddep"
        Me.DisplayName = "embedded"
    End Sub

#Region " Shared access "

    Private Class cDirectDataExchangeProtocolInstances

        Public Sub New(instance As cDirectDataExchangeProtocol)
            Me.Instance = instance
            Me.RefCount = 0
        End Sub

        Public ReadOnly Instance As cDirectDataExchangeProtocol
        Public Property RefCount As Integer = 0

    End Class

    Private Shared s_clients As New List(Of cDirectDataExchangeProtocolInstances)
    Private Shared s_server As cDirectDataExchangeProtocolInstances = Nothing

    Public Shared Function GetServer(Optional id As Int32 = -1) As cDirectDataExchangeProtocol
        If s_server Is Nothing Then
            s_server = New cDirectDataExchangeProtocolInstances(New cDirectDataExchangeProtocol(id, True))
        End If
        Return s_server.Instance
    End Function

    ''' <summary>
    ''' </summary>
    ''' <param name="folder"></param>
    ''' <param name="bIncludeSubfolders"></param>
    ''' <returns></returns>
    Public Shared Function GetClient(clientID As Int32) As cDirectDataExchangeProtocol

        Dim c As cDirectDataExchangeProtocolInstances = Nothing
        Dim bFound As Boolean = False

        For Each c In cDirectDataExchangeProtocol.s_clients
            Dim p As cDirectDataExchangeProtocol = c.Instance
            If (p.ID = clientID) Then
                bFound = True
                Exit For
            End If
        Next

        If (Not bFound) Then
            c = New cDirectDataExchangeProtocolInstances(New cDirectDataExchangeProtocol(clientID, False))
            s_clients.Add(c)
        End If

        c.RefCount += 1
        Return c.Instance

    End Function

    Public Shared Function ReleaseInstance(w As cDirectDataExchangeProtocol) As Boolean

        If (w Is Nothing) Then Return False
        Return cDirectDataExchangeProtocol.ReleaseInstance(w.ID, w.IsServer)

    End Function

    Public Shared Function ReleaseInstance(ID As Int32, bIsServer As Boolean) As Boolean

        Dim c As cDirectDataExchangeProtocolInstances = Nothing
        Dim bFound As Boolean = False

        If bIsServer Then
            If s_server IsNot Nothing Then
                s_server.RefCount -= 1
                If s_server.RefCount = 0 Then s_server = Nothing
                Return True
            End If
            Return False
        End If

        For Each c In cDirectDataExchangeProtocol.s_clients
            Dim p As cDirectDataExchangeProtocol = c.Instance
            If (p.ID = ID) Then
                bFound = True
                Exit For
            End If
        Next

        If Not bFound Then Return False

        c.RefCount -= 1
        If (c.RefCount <= 0) Then
            cDirectDataExchangeProtocol.s_clients.Remove(c)
        End If
        Return True

    End Function

    Public Shared Function GetClients() As cDirectDataExchangeProtocol()
        Dim l As New List(Of cDirectDataExchangeProtocol)
        For Each c As cDirectDataExchangeProtocolInstances In s_clients
            l.Add(c.Instance)
        Next
        Return l.ToArray()
    End Function

#End Region ' Shared access

#Region " Generic access "

    Public Overrides Function IsConfigured() As Boolean
        Return True
    End Function

    Public Overrides ReadOnly Property DefaultPingInterval As Long
        Get
            Return 5 * 1000 ' 10 seconds
        End Get
    End Property

    Protected Overrides Function SendThroughProtocol(obj As cDataPackage) As Boolean

        Dim bSent As Boolean = False

        If Not Me.CanSend(obj) Then
            Return False
        End If

        Try
            If (Me.IsServer) Then

                For Each cl As cDirectDataExchangeProtocol In cDirectDataExchangeProtocol.GetClients
                    If cl.ID = obj.Recipient Or obj.Recipient = 0 Then
                        cl.ProcessIncomingData(obj)
                        bSent = True
                    End If
                Next
            Else
                Dim s As cDirectDataExchangeProtocol = cDirectDataExchangeProtocol.GetServer
                If s IsNot Nothing Then
                    s.ProcessIncomingData(obj)
                    bSent = True
                End If
            End If
        Catch ex As Exception

        End Try
        Return bSent

    End Function

#End Region ' Generic access

End Class
