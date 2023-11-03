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

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ucConfigureServerProtocols
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.m_clbProtocols = New System.Windows.Forms.CheckedListBox()
        Me.m_tlpMain = New System.Windows.Forms.TableLayoutPanel()
        Me.m_tlpMain.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_clbProtocols
        '
        Me.m_clbProtocols.CheckOnClick = True
        Me.m_clbProtocols.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_clbProtocols.FormattingEnabled = True
        Me.m_clbProtocols.Location = New System.Drawing.Point(3, 3)
        Me.m_clbProtocols.Name = "m_clbProtocols"
        Me.m_clbProtocols.Size = New System.Drawing.Size(307, 108)
        Me.m_clbProtocols.Sorted = True
        Me.m_clbProtocols.TabIndex = 0
        '
        'm_tlpMain
        '
        Me.m_tlpMain.ColumnCount = 1
        Me.m_tlpMain.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.m_tlpMain.Controls.Add(Me.m_clbProtocols, 0, 0)
        Me.m_tlpMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_tlpMain.Location = New System.Drawing.Point(0, 0)
        Me.m_tlpMain.Name = "m_tlpMain"
        Me.m_tlpMain.RowCount = 2
        Me.m_tlpMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.m_tlpMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.m_tlpMain.Size = New System.Drawing.Size(313, 228)
        Me.m_tlpMain.TabIndex = 1
        '
        'ucConfigureServerProtocols
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.m_tlpMain)
        Me.Name = "ucConfigureServerProtocols"
        Me.Size = New System.Drawing.Size(313, 228)
        Me.m_tlpMain.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Private WithEvents m_clbProtocols As CheckedListBox
    Private WithEvents m_tlpMain As TableLayoutPanel
End Class
