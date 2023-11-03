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
Partial Class dlgConfig
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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
        Me.m_tlpConfig = New System.Windows.Forms.TableLayoutPanel()
        Me.m_tlpButtons = New System.Windows.Forms.TableLayoutPanel()
        Me.m_btnOK = New System.Windows.Forms.Button()
        Me.m_btnCancel = New System.Windows.Forms.Button()
        Me.m_tc = New System.Windows.Forms.TabControl()
        Me.m_tlpConfig.SuspendLayout()
        Me.m_tlpButtons.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_tlpConfig
        '
        Me.m_tlpConfig.ColumnCount = 1
        Me.m_tlpConfig.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.m_tlpConfig.Controls.Add(Me.m_tlpButtons, 0, 1)
        Me.m_tlpConfig.Controls.Add(Me.m_tc, 0, 0)
        Me.m_tlpConfig.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_tlpConfig.Location = New System.Drawing.Point(0, 0)
        Me.m_tlpConfig.Name = "m_tlpConfig"
        Me.m_tlpConfig.RowCount = 2
        Me.m_tlpConfig.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.m_tlpConfig.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38.0!))
        Me.m_tlpConfig.Size = New System.Drawing.Size(432, 459)
        Me.m_tlpConfig.TabIndex = 0
        '
        'm_tlpButtons
        '
        Me.m_tlpButtons.ColumnCount = 2
        Me.m_tlpButtons.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.m_tlpButtons.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.m_tlpButtons.Controls.Add(Me.m_btnOK, 0, 0)
        Me.m_tlpButtons.Controls.Add(Me.m_btnCancel, 1, 0)
        Me.m_tlpButtons.Dock = System.Windows.Forms.DockStyle.Right
        Me.m_tlpButtons.Location = New System.Drawing.Point(229, 424)
        Me.m_tlpButtons.Name = "m_tlpButtons"
        Me.m_tlpButtons.RowCount = 1
        Me.m_tlpButtons.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.m_tlpButtons.Size = New System.Drawing.Size(200, 32)
        Me.m_tlpButtons.TabIndex = 0
        '
        'm_btnOK
        '
        Me.m_btnOK.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_btnOK.Location = New System.Drawing.Point(3, 3)
        Me.m_btnOK.Name = "m_btnOK"
        Me.m_btnOK.Size = New System.Drawing.Size(94, 26)
        Me.m_btnOK.TabIndex = 0
        Me.m_btnOK.Text = "OK"
        Me.m_btnOK.UseVisualStyleBackColor = True
        '
        'm_btnCancel
        '
        Me.m_btnCancel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_btnCancel.Location = New System.Drawing.Point(103, 3)
        Me.m_btnCancel.Name = "m_btnCancel"
        Me.m_btnCancel.Size = New System.Drawing.Size(94, 26)
        Me.m_btnCancel.TabIndex = 1
        Me.m_btnCancel.Text = "Cancel"
        Me.m_btnCancel.UseVisualStyleBackColor = True
        '
        'm_tc
        '
        Me.m_tc.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_tc.Location = New System.Drawing.Point(3, 3)
        Me.m_tc.Name = "m_tc"
        Me.m_tc.SelectedIndex = 0
        Me.m_tc.Size = New System.Drawing.Size(426, 415)
        Me.m_tc.TabIndex = 1
        '
        'dlgConfig
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(432, 459)
        Me.ControlBox = False
        Me.Controls.Add(Me.m_tlpConfig)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "dlgConfig"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.Text = "Configure"
        Me.m_tlpConfig.ResumeLayout(False)
        Me.m_tlpButtons.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Private WithEvents m_tlpConfig As TableLayoutPanel
    Private WithEvents m_tlpButtons As TableLayoutPanel
    Private WithEvents m_btnOK As Button
    Private WithEvents m_btnCancel As Button
    Private WithEvents m_tc As TabControl
End Class
