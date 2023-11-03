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

Public Class dlgConfig

#Region " Private vars "

    Private m_configs As New List(Of IConfigUI)
    Private m_dtPages As New Dictionary(Of String, Type)

#End Region ' Private vars

#Region " Construction / destruction "

    Public Sub New(settings As cSettings)
        Me.Settings = settings
        Me.InitializeComponent()
    End Sub

#End Region ' Construction / destruction

#Region " Public access "

    Public ReadOnly Property Settings As cSettings

#Region " Page management "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Add a configuration page.
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="t">A <see cref="Control"/> and <see cref="IConfigUI"/> derived
    ''' Type of the configuration page to add.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Function AddPage(name As String, t As Type) As Boolean
        If (String.IsNullOrEmpty(name)) Then Return False
        If (Not GetType(IConfigUI).IsAssignableFrom(t)) Then Return False
        If (Not GetType(Control).IsAssignableFrom(t)) Then Return False
        Me.m_dtPages(name) = t
        Return True
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Remove a configuration page.
    ''' </summary>
    ''' <param name="name"></param>
    ''' <returns></returns>
    ''' -----------------------------------------------------------------------
    Public Function RemovePage(name As String) As Boolean
        If (Not Me.m_dtPages.ContainsKey(name)) Then Return False
        Return Me.m_dtPages.Remove(name)
    End Function

    Public ReadOnly Property PageNames() As String()
        Get
            Return Me.m_dtPages.Keys.ToArray()
        End Get
    End Property

    Public ReadOnly Property Page(name As String) As Type
        Get
            If (Me.m_dtPages.ContainsKey(name)) Then
                Return Me.m_dtPages(name)

            End If
            Return Nothing
        End Get
    End Property

#End Region ' Page management

#End Region ' Public access

#Region " Overrides "

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        Me.AddPage("Folders", GetType(ucConfigureFolderAliases))
        Me.AddPage("Applications", GetType(ucConfigureApplicationAliases))

        For Each k As String In Me.m_dtPages.Keys
            Dim obj As Object = Activator.CreateInstance(Me.m_dtPages(k))
            If (obj IsNot Nothing) Then
                DirectCast(obj, IConfigUI).Settings = Me.Settings
                Me.AddTab(k, DirectCast(obj, Control))
            End If
        Next

        Me.CenterToParent()

    End Sub

#End Region ' Overrides

#Region " Control events "

    Private Sub OnOK(sender As Object, e As EventArgs) Handles m_btnOK.Click
        Dim bSuccess As Boolean = True
        For Each i As IConfigUI In Me.m_configs
            bSuccess = bSuccess And i.Apply()
        Next
        If bSuccess Then
            Me.DialogResult = DialogResult.OK
            Me.Close()
        End If
    End Sub

    Private Sub OnCancel(sender As Object, e As EventArgs) Handles m_btnCancel.Click
        Me.Close()
    End Sub

#End Region ' Control events

#Region " Internals "

    Private Sub AddTab(name As String, ctrl As Control)
        Dim tc As New TabPage(name)
        tc.Controls.Add(ctrl)
        ctrl.Dock = DockStyle.Fill
        Me.m_tc.TabPages.Add(tc)

        If (TypeOf ctrl Is IConfigUI) Then
            Me.m_configs.Add(DirectCast(ctrl, IConfigUI))
        End If

    End Sub

#End Region ' Internals

End Class