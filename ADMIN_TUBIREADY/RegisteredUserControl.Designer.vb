<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class RegisteredUserControl
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim DataGridViewCellStyle1 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim DataGridViewCellStyle3 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim CustomizableEdges7 As Guna.UI2.WinForms.Suite.CustomizableEdges = New Guna.UI2.WinForms.Suite.CustomizableEdges()
        Dim CustomizableEdges8 As Guna.UI2.WinForms.Suite.CustomizableEdges = New Guna.UI2.WinForms.Suite.CustomizableEdges()
        Dim CustomizableEdges1 As Guna.UI2.WinForms.Suite.CustomizableEdges = New Guna.UI2.WinForms.Suite.CustomizableEdges()
        Dim CustomizableEdges2 As Guna.UI2.WinForms.Suite.CustomizableEdges = New Guna.UI2.WinForms.Suite.CustomizableEdges()
        Dim CustomizableEdges3 As Guna.UI2.WinForms.Suite.CustomizableEdges = New Guna.UI2.WinForms.Suite.CustomizableEdges()
        Dim CustomizableEdges4 As Guna.UI2.WinForms.Suite.CustomizableEdges = New Guna.UI2.WinForms.Suite.CustomizableEdges()
        Dim CustomizableEdges5 As Guna.UI2.WinForms.Suite.CustomizableEdges = New Guna.UI2.WinForms.Suite.CustomizableEdges()
        Dim CustomizableEdges6 As Guna.UI2.WinForms.Suite.CustomizableEdges = New Guna.UI2.WinForms.Suite.CustomizableEdges()
        Guna2DataGridView1 = New Guna.UI2.WinForms.Guna2DataGridView()
        Guna2Panel7 = New Guna.UI2.WinForms.Guna2Panel()
        btnNext = New Guna.UI2.WinForms.Guna2Button()
        btnPrev = New Guna.UI2.WinForms.Guna2Button()
        lblPageInfo = New Guna.UI2.WinForms.Guna2HtmlLabel()
        Guna2Panel10 = New Guna.UI2.WinForms.Guna2Panel()
        Checkbox = New DataGridViewCheckBoxColumn()
        ResidentName = New DataGridViewTextBoxColumn()
        Role = New DataGridViewTextBoxColumn()
        Address = New DataGridViewTextBoxColumn()
        ContactNum = New DataGridViewTextBoxColumn()
        DateApplied = New DataGridViewTextBoxColumn()
        Edit = New DataGridViewButtonColumn()
        Archive = New DataGridViewButtonColumn()
        CType(Guna2DataGridView1, ComponentModel.ISupportInitialize).BeginInit()
        Guna2Panel7.SuspendLayout()
        SuspendLayout()
        ' 
        ' Guna2DataGridView1
        ' 
        DataGridViewCellStyle1.BackColor = Color.White
        Guna2DataGridView1.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle1
        DataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle2.BackColor = Color.White
        DataGridViewCellStyle2.Font = New Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        DataGridViewCellStyle2.ForeColor = Color.Black
        DataGridViewCellStyle2.SelectionBackColor = Color.LightGray
        DataGridViewCellStyle2.SelectionForeColor = Color.Black
        DataGridViewCellStyle2.WrapMode = DataGridViewTriState.True
        Guna2DataGridView1.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle2
        Guna2DataGridView1.ColumnHeadersHeight = 22
        Guna2DataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing
        Guna2DataGridView1.Columns.AddRange(New DataGridViewColumn() {Checkbox, ResidentName, Role, Address, ContactNum, DateApplied, Edit, Archive})
        DataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle3.BackColor = Color.White
        DataGridViewCellStyle3.Font = New Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        DataGridViewCellStyle3.ForeColor = Color.FromArgb(CByte(71), CByte(69), CByte(94))
        DataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(CByte(231), CByte(229), CByte(255))
        DataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(CByte(71), CByte(69), CByte(94))
        DataGridViewCellStyle3.WrapMode = DataGridViewTriState.False
        Guna2DataGridView1.DefaultCellStyle = DataGridViewCellStyle3
        Guna2DataGridView1.Dock = DockStyle.Top
        Guna2DataGridView1.GridColor = Color.FromArgb(CByte(231), CByte(229), CByte(255))
        Guna2DataGridView1.Location = New Point(0, 0)
        Guna2DataGridView1.Name = "Guna2DataGridView1"
        Guna2DataGridView1.RowHeadersVisible = False
        Guna2DataGridView1.Size = New Size(1164, 578)
        Guna2DataGridView1.TabIndex = 2
        Guna2DataGridView1.ThemeStyle.AlternatingRowsStyle.BackColor = Color.White
        Guna2DataGridView1.ThemeStyle.AlternatingRowsStyle.Font = Nothing
        Guna2DataGridView1.ThemeStyle.AlternatingRowsStyle.ForeColor = Color.Empty
        Guna2DataGridView1.ThemeStyle.AlternatingRowsStyle.SelectionBackColor = Color.Empty
        Guna2DataGridView1.ThemeStyle.AlternatingRowsStyle.SelectionForeColor = Color.Empty
        Guna2DataGridView1.ThemeStyle.BackColor = Color.White
        Guna2DataGridView1.ThemeStyle.GridColor = Color.FromArgb(CByte(231), CByte(229), CByte(255))
        Guna2DataGridView1.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(CByte(100), CByte(88), CByte(255))
        Guna2DataGridView1.ThemeStyle.HeaderStyle.BorderStyle = DataGridViewHeaderBorderStyle.None
        Guna2DataGridView1.ThemeStyle.HeaderStyle.Font = New Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Guna2DataGridView1.ThemeStyle.HeaderStyle.ForeColor = Color.White
        Guna2DataGridView1.ThemeStyle.HeaderStyle.HeaightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing
        Guna2DataGridView1.ThemeStyle.HeaderStyle.Height = 22
        Guna2DataGridView1.ThemeStyle.ReadOnly = False
        Guna2DataGridView1.ThemeStyle.RowsStyle.BackColor = Color.White
        Guna2DataGridView1.ThemeStyle.RowsStyle.BorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
        Guna2DataGridView1.ThemeStyle.RowsStyle.Font = New Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Guna2DataGridView1.ThemeStyle.RowsStyle.ForeColor = Color.FromArgb(CByte(71), CByte(69), CByte(94))
        Guna2DataGridView1.ThemeStyle.RowsStyle.Height = 25
        Guna2DataGridView1.ThemeStyle.RowsStyle.SelectionBackColor = Color.FromArgb(CByte(231), CByte(229), CByte(255))
        Guna2DataGridView1.ThemeStyle.RowsStyle.SelectionForeColor = Color.FromArgb(CByte(71), CByte(69), CByte(94))
        ' 
        ' Guna2Panel7
        ' 
        Guna2Panel7.BackColor = Color.Transparent
        Guna2Panel7.BorderRadius = 10
        Guna2Panel7.Controls.Add(btnNext)
        Guna2Panel7.Controls.Add(btnPrev)
        Guna2Panel7.Controls.Add(lblPageInfo)
        Guna2Panel7.Controls.Add(Guna2Panel10)
        Guna2Panel7.CustomizableEdges = CustomizableEdges7
        Guna2Panel7.Dock = DockStyle.Bottom
        Guna2Panel7.FillColor = Color.White
        Guna2Panel7.Location = New Point(0, 579)
        Guna2Panel7.Name = "Guna2Panel7"
        Guna2Panel7.ShadowDecoration.CustomizableEdges = CustomizableEdges8
        Guna2Panel7.Size = New Size(1164, 60)
        Guna2Panel7.TabIndex = 6
        ' 
        ' btnNext
        ' 
        btnNext.BorderRadius = 5
        btnNext.CustomizableEdges = CustomizableEdges1
        btnNext.DisabledState.BorderColor = Color.DarkGray
        btnNext.DisabledState.CustomBorderColor = Color.DarkGray
        btnNext.DisabledState.FillColor = Color.FromArgb(CByte(169), CByte(169), CByte(169))
        btnNext.DisabledState.ForeColor = Color.FromArgb(CByte(141), CByte(141), CByte(141))
        btnNext.Font = New Font("Segoe UI", 9F)
        btnNext.ForeColor = Color.White
        btnNext.Location = New Point(640, 20)
        btnNext.Name = "btnNext"
        btnNext.ShadowDecoration.CustomizableEdges = CustomizableEdges2
        btnNext.Size = New Size(72, 22)
        btnNext.TabIndex = 4
        btnNext.Text = "Next"
        ' 
        ' btnPrev
        ' 
        btnPrev.BorderRadius = 5
        btnPrev.CustomizableEdges = CustomizableEdges3
        btnPrev.DisabledState.BorderColor = Color.DarkGray
        btnPrev.DisabledState.CustomBorderColor = Color.DarkGray
        btnPrev.DisabledState.FillColor = Color.FromArgb(CByte(169), CByte(169), CByte(169))
        btnPrev.DisabledState.ForeColor = Color.FromArgb(CByte(141), CByte(141), CByte(141))
        btnPrev.Font = New Font("Segoe UI", 9F)
        btnPrev.ForeColor = Color.White
        btnPrev.Location = New Point(434, 20)
        btnPrev.Name = "btnPrev"
        btnPrev.ShadowDecoration.CustomizableEdges = CustomizableEdges4
        btnPrev.Size = New Size(72, 22)
        btnPrev.TabIndex = 3
        btnPrev.Text = "Previous"
        ' 
        ' lblPageInfo
        ' 
        lblPageInfo.BackColor = Color.Transparent
        lblPageInfo.Font = New Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblPageInfo.Location = New Point(540, 20)
        lblPageInfo.Name = "lblPageInfo"
        lblPageInfo.Size = New Size(67, 19)
        lblPageInfo.TabIndex = 2
        lblPageInfo.Text = "Page 1 of 1"
        ' 
        ' Guna2Panel10
        ' 
        Guna2Panel10.CustomizableEdges = CustomizableEdges5
        Guna2Panel10.Dock = DockStyle.Top
        Guna2Panel10.FillColor = Color.FromArgb(CByte(204), CByte(204), CByte(204))
        Guna2Panel10.Location = New Point(0, 0)
        Guna2Panel10.Name = "Guna2Panel10"
        Guna2Panel10.ShadowDecoration.CustomizableEdges = CustomizableEdges6
        Guna2Panel10.Size = New Size(1164, 2)
        Guna2Panel10.TabIndex = 1
        ' 
        ' Checkbox
        ' 
        Checkbox.HeaderText = ""
        Checkbox.Name = "Checkbox"
        ' 
        ' ResidentName
        ' 
        ResidentName.HeaderText = "Name"
        ResidentName.Name = "ResidentName"
        ResidentName.ReadOnly = True
        ' 
        ' Role
        ' 
        Role.HeaderText = "Role"
        Role.Name = "Role"
        Role.ReadOnly = True
        ' 
        ' Address
        ' 
        Address.HeaderText = "Address"
        Address.Name = "Address"
        Address.ReadOnly = True
        ' 
        ' ContactNum
        ' 
        ContactNum.HeaderText = "Contact No."
        ContactNum.Name = "ContactNum"
        ContactNum.ReadOnly = True
        ' 
        ' DateApplied
        ' 
        DateApplied.HeaderText = "Date Applied"
        DateApplied.Name = "DateApplied"
        DateApplied.ReadOnly = True
        ' 
        ' Edit
        ' 
        Edit.FlatStyle = FlatStyle.Popup
        Edit.HeaderText = ""
        Edit.Name = "Edit"
        ' 
        ' Archive
        ' 
        Archive.FlatStyle = FlatStyle.Popup
        Archive.HeaderText = ""
        Archive.Name = "Archive"
        ' 
        ' RegisteredUserControl
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(Guna2Panel7)
        Controls.Add(Guna2DataGridView1)
        Name = "RegisteredUserControl"
        Size = New Size(1164, 639)
        CType(Guna2DataGridView1, ComponentModel.ISupportInitialize).EndInit()
        Guna2Panel7.ResumeLayout(False)
        Guna2Panel7.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents Guna2DataGridView1 As Guna.UI2.WinForms.Guna2DataGridView
    Friend WithEvents Guna2Panel7 As Guna.UI2.WinForms.Guna2Panel
    Friend WithEvents btnNext As Guna.UI2.WinForms.Guna2Button
    Friend WithEvents btnPrev As Guna.UI2.WinForms.Guna2Button
    Friend WithEvents lblPageInfo As Guna.UI2.WinForms.Guna2HtmlLabel
    Friend WithEvents Guna2Panel10 As Guna.UI2.WinForms.Guna2Panel
    Friend WithEvents Checkbox As DataGridViewCheckBoxColumn
    Friend WithEvents ResidentName As DataGridViewTextBoxColumn
    Friend WithEvents Role As DataGridViewTextBoxColumn
    Friend WithEvents Address As DataGridViewTextBoxColumn
    Friend WithEvents ContactNum As DataGridViewTextBoxColumn
    Friend WithEvents DateApplied As DataGridViewTextBoxColumn
    Friend WithEvents Edit As DataGridViewButtonColumn
    Friend WithEvents Archive As DataGridViewButtonColumn

End Class
