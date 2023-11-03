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

Imports ClientServer
Imports Server

Public Class ucConfigureServerProtocols
    Implements IConfigUI

    Public Sub New()
        Me.InitializeComponent()
    End Sub

    Public Property Server As cServer

    Public Property Settings As cSettings Implements IConfigUI.Settings

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        'Debug.Assert(Me.Server IsNot Nothing)
        Me.m_clbProtocols.Items.Clear()
        Me.m_clbProtocols.Items.AddRange(cDataExchangeProtocolFinder.GetProtocols())

    End Sub

    Public Event OnChanged(sender As Object, args As EventArgs) Implements IConfigUI.OnChanged

    Public Function Apply() As Boolean Implements IConfigUI.Apply
        Return True
    End Function

    Public Function CanApply() As Boolean Implements IConfigUI.CanApply
        If Me.Server Is Nothing Then Return False
        Return Not Me.Server.IsServerStarted
    End Function

End Class
