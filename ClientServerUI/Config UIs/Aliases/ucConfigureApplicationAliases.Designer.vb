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
Partial Class ucConfigureApplicationAliases
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
        Me.m_dgv = New System.Windows.Forms.DataGridView()
        Me.m_colAlias = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.m_colEmbedded = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.m_colApplication = New System.Windows.Forms.DataGridViewComboBoxColumn()
        CType(Me.m_dgv, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'm_dgv
        '
        Me.m_dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.m_dgv.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.m_colAlias, Me.m_colEmbedded, Me.m_colApplication})
        Me.m_dgv.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_dgv.Location = New System.Drawing.Point(0, 0)
        Me.m_dgv.Name = "m_dgv"
        Me.m_dgv.RowTemplate.Height = 25
        Me.m_dgv.Size = New System.Drawing.Size(263, 272)
        Me.m_dgv.TabIndex = 0
        '
        'm_colAlias
        '
        Me.m_colAlias.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.m_colAlias.HeaderText = "Alias"
        Me.m_colAlias.Name = "m_colAlias"
        Me.m_colAlias.Width = 57
        '
        'm_colEmbedded
        '
        Me.m_colEmbedded.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.m_colEmbedded.HeaderText = "Embedded"
        Me.m_colEmbedded.Name = "m_colEmbedded"
        Me.m_colEmbedded.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.m_colEmbedded.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        Me.m_colEmbedded.Width = 89
        '
        'm_colApplication
        '
        Me.m_colApplication.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.m_colApplication.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.[Nothing]
        Me.m_colApplication.HeaderText = "Application"
        Me.m_colApplication.Name = "m_colApplication"
        Me.m_colApplication.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.m_colApplication.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        '
        'ucConfigureApplicationAliases
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.m_dgv)
        Me.Name = "ucConfigureApplicationAliases"
        Me.Size = New System.Drawing.Size(263, 272)
        CType(Me.m_dgv, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents m_dgv As DataGridView
    Friend WithEvents m_colAlias As DataGridViewTextBoxColumn
    Friend WithEvents m_colEmbedded As DataGridViewCheckBoxColumn
    Friend WithEvents m_colApplication As DataGridViewComboBoxColumn
End Class
