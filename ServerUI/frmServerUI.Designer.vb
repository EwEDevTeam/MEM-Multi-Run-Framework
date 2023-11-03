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
Partial Class frmServerUI
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmServerUI))
        Me.m_tlpMain = New System.Windows.Forms.TableLayoutPanel()
        Me.m_tlpOverview = New System.Windows.Forms.TableLayoutPanel()
        Me.m_lblWorkload = New System.Windows.Forms.Label()
        Me.m_lblClients = New System.Windows.Forms.Label()
        Me.m_dgvWL = New System.Windows.Forms.DataGridView()
        Me.m_colWLJob = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.m_colWLTask = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.m_colWLStatus = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.m_colWLnode = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.m_dgvClients = New System.Windows.Forms.DataGridView()
        Me.m_colCLid = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.m_colCLstatus = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.m_tsServer = New System.Windows.Forms.ToolStrip()
        Me.m_tsbnConfig = New System.Windows.Forms.ToolStripButton()
        Me.m_tsbnStartServer = New System.Windows.Forms.ToolStripButton()
        Me.m_tsbnStopServer = New System.Windows.Forms.ToolStripButton()
        Me.m_status = New System.Windows.Forms.StatusStrip()
        Me.m_tspbWorkload = New System.Windows.Forms.ToolStripProgressBar()
        Me.m_tslSpacer = New System.Windows.Forms.ToolStripStatusLabel()
        Me.m_tslProtocol = New System.Windows.Forms.ToolStripStatusLabel()
        Me.m_tslFolders = New System.Windows.Forms.ToolStripStatusLabel()
        Me.m_tslApplications = New System.Windows.Forms.ToolStripStatusLabel()
        Me.m_tsbnLoadWorkload = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.m_tlpMain.SuspendLayout()
        Me.m_tlpOverview.SuspendLayout()
        CType(Me.m_dgvWL, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.m_dgvClients, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.m_tsServer.SuspendLayout()
        Me.m_status.SuspendLayout()
        Me.SuspendLayout()
        '
        'm_tlpMain
        '
        Me.m_tlpMain.ColumnCount = 1
        Me.m_tlpMain.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.m_tlpMain.Controls.Add(Me.m_tlpOverview, 0, 1)
        Me.m_tlpMain.Controls.Add(Me.m_tsServer, 0, 0)
        Me.m_tlpMain.Controls.Add(Me.m_status, 0, 2)
        Me.m_tlpMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_tlpMain.Location = New System.Drawing.Point(0, 0)
        Me.m_tlpMain.Name = "m_tlpMain"
        Me.m_tlpMain.RowCount = 3
        Me.m_tlpMain.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.m_tlpMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.m_tlpMain.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.m_tlpMain.Size = New System.Drawing.Size(645, 438)
        Me.m_tlpMain.TabIndex = 2
        '
        'm_tlpOverview
        '
        Me.m_tlpOverview.ColumnCount = 2
        Me.m_tlpOverview.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.m_tlpOverview.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.m_tlpOverview.Controls.Add(Me.m_lblWorkload, 0, 0)
        Me.m_tlpOverview.Controls.Add(Me.m_lblClients, 1, 0)
        Me.m_tlpOverview.Controls.Add(Me.m_dgvWL, 0, 1)
        Me.m_tlpOverview.Controls.Add(Me.m_dgvClients, 1, 1)
        Me.m_tlpOverview.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_tlpOverview.Location = New System.Drawing.Point(3, 28)
        Me.m_tlpOverview.Name = "m_tlpOverview"
        Me.m_tlpOverview.RowCount = 2
        Me.m_tlpOverview.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.m_tlpOverview.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.m_tlpOverview.Size = New System.Drawing.Size(639, 385)
        Me.m_tlpOverview.TabIndex = 1
        '
        'm_lblWorkload
        '
        Me.m_lblWorkload.Dock = System.Windows.Forms.DockStyle.Top
        Me.m_lblWorkload.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point)
        Me.m_lblWorkload.Location = New System.Drawing.Point(3, 0)
        Me.m_lblWorkload.Name = "m_lblWorkload"
        Me.m_lblWorkload.Size = New System.Drawing.Size(313, 23)
        Me.m_lblWorkload.TabIndex = 0
        Me.m_lblWorkload.Text = "Workload"
        Me.m_lblWorkload.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'm_lblClients
        '
        Me.m_lblClients.Dock = System.Windows.Forms.DockStyle.Top
        Me.m_lblClients.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point)
        Me.m_lblClients.Location = New System.Drawing.Point(322, 0)
        Me.m_lblClients.Name = "m_lblClients"
        Me.m_lblClients.Size = New System.Drawing.Size(314, 23)
        Me.m_lblClients.TabIndex = 1
        Me.m_lblClients.Text = "Clients"
        Me.m_lblClients.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'm_dgvWL
        '
        Me.m_dgvWL.AllowDrop = True
        Me.m_dgvWL.AllowUserToAddRows = False
        Me.m_dgvWL.AllowUserToDeleteRows = False
        Me.m_dgvWL.AllowUserToResizeColumns = False
        Me.m_dgvWL.AllowUserToResizeRows = False
        Me.m_dgvWL.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.m_dgvWL.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.m_colWLJob, Me.m_colWLTask, Me.m_colWLStatus, Me.m_colWLnode})
        Me.m_dgvWL.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_dgvWL.Location = New System.Drawing.Point(3, 26)
        Me.m_dgvWL.Name = "m_dgvWL"
        Me.m_dgvWL.ReadOnly = True
        Me.m_dgvWL.RowHeadersVisible = False
        Me.m_dgvWL.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing
        Me.m_dgvWL.RowTemplate.Height = 25
        Me.m_dgvWL.ShowCellErrors = False
        Me.m_dgvWL.ShowCellToolTips = False
        Me.m_dgvWL.ShowEditingIcon = False
        Me.m_dgvWL.ShowRowErrors = False
        Me.m_dgvWL.Size = New System.Drawing.Size(313, 356)
        Me.m_dgvWL.TabIndex = 2
        '
        'm_colWLJob
        '
        Me.m_colWLJob.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.m_colWLJob.HeaderText = "Job"
        Me.m_colWLJob.Name = "m_colWLJob"
        Me.m_colWLJob.ReadOnly = True
        '
        'm_colWLTask
        '
        Me.m_colWLTask.HeaderText = "Task"
        Me.m_colWLTask.Name = "m_colWLTask"
        Me.m_colWLTask.ReadOnly = True
        '
        'm_colWLStatus
        '
        Me.m_colWLStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.m_colWLStatus.HeaderText = "Status"
        Me.m_colWLStatus.Name = "m_colWLStatus"
        Me.m_colWLStatus.ReadOnly = True
        '
        'm_colWLnode
        '
        Me.m_colWLnode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.m_colWLnode.HeaderText = "ID"
        Me.m_colWLnode.Name = "m_colWLnode"
        Me.m_colWLnode.ReadOnly = True
        '
        'm_dgvClients
        '
        Me.m_dgvClients.AllowUserToAddRows = False
        Me.m_dgvClients.AllowUserToDeleteRows = False
        Me.m_dgvClients.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.m_dgvClients.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.m_colCLid, Me.m_colCLstatus})
        Me.m_dgvClients.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_dgvClients.Location = New System.Drawing.Point(322, 26)
        Me.m_dgvClients.Name = "m_dgvClients"
        Me.m_dgvClients.ReadOnly = True
        Me.m_dgvClients.RowHeadersVisible = False
        Me.m_dgvClients.RowTemplate.Height = 25
        Me.m_dgvClients.Size = New System.Drawing.Size(314, 356)
        Me.m_dgvClients.TabIndex = 3
        '
        'm_colCLid
        '
        Me.m_colCLid.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        Me.m_colCLid.HeaderText = "ID"
        Me.m_colCLid.Name = "m_colCLid"
        Me.m_colCLid.ReadOnly = True
        Me.m_colCLid.Width = 43
        '
        'm_colCLstatus
        '
        Me.m_colCLstatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.m_colCLstatus.HeaderText = "Status"
        Me.m_colCLstatus.Name = "m_colCLstatus"
        Me.m_colCLstatus.ReadOnly = True
        '
        'm_tsServer
        '
        Me.m_tsServer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.m_tsServer.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.m_tsServer.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tsbnConfig, Me.m_tsbnStartServer, Me.m_tsbnStopServer, Me.ToolStripSeparator1, Me.m_tsbnLoadWorkload})
        Me.m_tsServer.Location = New System.Drawing.Point(0, 0)
        Me.m_tsServer.Name = "m_tsServer"
        Me.m_tsServer.Size = New System.Drawing.Size(645, 25)
        Me.m_tsServer.TabIndex = 2
        Me.m_tsServer.Text = "ToolStrip1"
        '
        'm_tsbnConfig
        '
        Me.m_tsbnConfig.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.m_tsbnConfig.Image = CType(resources.GetObject("m_tsbnConfig.Image"), System.Drawing.Image)
        Me.m_tsbnConfig.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.m_tsbnConfig.Name = "m_tsbnConfig"
        Me.m_tsbnConfig.Size = New System.Drawing.Size(23, 22)
        Me.m_tsbnConfig.Text = "Config"
        '
        'm_tsbnStartServer
        '
        Me.m_tsbnStartServer.CheckOnClick = True
        Me.m_tsbnStartServer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.m_tsbnStartServer.Image = CType(resources.GetObject("m_tsbnStartServer.Image"), System.Drawing.Image)
        Me.m_tsbnStartServer.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.m_tsbnStartServer.Name = "m_tsbnStartServer"
        Me.m_tsbnStartServer.Size = New System.Drawing.Size(27, 22)
        Me.m_tsbnStartServer.Text = "On"
        '
        'm_tsbnStopServer
        '
        Me.m_tsbnStopServer.Checked = True
        Me.m_tsbnStopServer.CheckOnClick = True
        Me.m_tsbnStopServer.CheckState = System.Windows.Forms.CheckState.Checked
        Me.m_tsbnStopServer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.m_tsbnStopServer.Image = CType(resources.GetObject("m_tsbnStopServer.Image"), System.Drawing.Image)
        Me.m_tsbnStopServer.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.m_tsbnStopServer.Name = "m_tsbnStopServer"
        Me.m_tsbnStopServer.Size = New System.Drawing.Size(28, 22)
        Me.m_tsbnStopServer.Text = "Off"
        '
        'm_status
        '
        Me.m_status.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.m_tspbWorkload, Me.m_tslSpacer, Me.m_tslProtocol, Me.m_tslFolders, Me.m_tslApplications})
        Me.m_status.Location = New System.Drawing.Point(0, 416)
        Me.m_status.Name = "m_status"
        Me.m_status.Size = New System.Drawing.Size(645, 22)
        Me.m_status.SizingGrip = False
        Me.m_status.TabIndex = 3
        Me.m_status.Text = "StatusStrip1"
        '
        'm_tspbWorkload
        '
        Me.m_tspbWorkload.Name = "m_tspbWorkload"
        Me.m_tspbWorkload.Size = New System.Drawing.Size(200, 16)
        Me.m_tspbWorkload.Style = System.Windows.Forms.ProgressBarStyle.Continuous
        '
        'm_tslSpacer
        '
        Me.m_tslSpacer.Name = "m_tslSpacer"
        Me.m_tslSpacer.Size = New System.Drawing.Size(258, 17)
        Me.m_tslSpacer.Spring = True
        Me.m_tslSpacer.Text = " "
        '
        'm_tslProtocol
        '
        Me.m_tslProtocol.DoubleClickEnabled = True
        Me.m_tslProtocol.Name = "m_tslProtocol"
        Me.m_tslProtocol.Size = New System.Drawing.Size(52, 17)
        Me.m_tslProtocol.Text = "Protocol"
        '
        'm_tslFolders
        '
        Me.m_tslFolders.DoubleClickEnabled = True
        Me.m_tslFolders.Name = "m_tslFolders"
        Me.m_tslFolders.Size = New System.Drawing.Size(45, 17)
        Me.m_tslFolders.Text = "Folders"
        '
        'm_tslApplications
        '
        Me.m_tslApplications.DoubleClickEnabled = True
        Me.m_tslApplications.Name = "m_tslApplications"
        Me.m_tslApplications.Size = New System.Drawing.Size(73, 17)
        Me.m_tslApplications.Text = "Applications"
        '
        'm_tsbnLoadWorkload
        '
        Me.m_tsbnLoadWorkload.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.m_tsbnLoadWorkload.Image = CType(resources.GetObject("m_tsbnLoadWorkload.Image"), System.Drawing.Image)
        Me.m_tsbnLoadWorkload.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.m_tsbnLoadWorkload.Name = "m_tsbnLoadWorkload"
        Me.m_tsbnLoadWorkload.Size = New System.Drawing.Size(89, 22)
        Me.m_tsbnLoadWorkload.Text = "&Load workload"
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        Me.ToolStripSeparator1.Size = New System.Drawing.Size(6, 25)
        '
        'frmServerUI
        '
        Me.AllowDrop = True
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(645, 438)
        Me.Controls.Add(Me.m_tlpMain)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmServerUI"
        Me.Text = "Multi-run framework server"
        Me.m_tlpMain.ResumeLayout(False)
        Me.m_tlpMain.PerformLayout()
        Me.m_tlpOverview.ResumeLayout(False)
        CType(Me.m_dgvWL, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.m_dgvClients, System.ComponentModel.ISupportInitialize).EndInit()
        Me.m_tsServer.ResumeLayout(False)
        Me.m_tsServer.PerformLayout()
        Me.m_status.ResumeLayout(False)
        Me.m_status.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents m_tlpMain As TableLayoutPanel
    Private WithEvents m_lblWorkload As Label
    Private WithEvents m_lblClients As Label
    Private WithEvents m_dgvWL As DataGridView
    Private WithEvents m_dgvClients As DataGridView
    Friend WithEvents m_colCLid As DataGridViewTextBoxColumn
    Friend WithEvents m_colCLstatus As DataGridViewTextBoxColumn
    Private WithEvents m_tsServer As ToolStrip
    Private WithEvents m_tsbnStartServer As ToolStripButton
    Private WithEvents m_tsbnStopServer As ToolStripButton
    Private WithEvents m_status As StatusStrip
    Private WithEvents m_tslSpacer As ToolStripStatusLabel
    Private WithEvents m_tslProtocol As ToolStripStatusLabel
    Private WithEvents m_tslFolders As ToolStripStatusLabel
    Private WithEvents m_tslApplications As ToolStripStatusLabel
    Private WithEvents m_tlpOverview As TableLayoutPanel
    Private WithEvents m_tspbWorkload As ToolStripProgressBar
    Friend WithEvents m_colWLJob As DataGridViewTextBoxColumn
    Friend WithEvents m_colWLTask As DataGridViewTextBoxColumn
    Friend WithEvents m_colWLStatus As DataGridViewTextBoxColumn
    Friend WithEvents m_colWLnode As DataGridViewTextBoxColumn
    Private WithEvents m_tsbnConfig As ToolStripButton
    Friend WithEvents ToolStripSeparator1 As ToolStripSeparator
    Private WithEvents m_tsbnLoadWorkload As ToolStripButton
End Class
