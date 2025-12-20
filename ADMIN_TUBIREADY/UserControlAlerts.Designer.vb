<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class UserControlAlerts
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
        chkResidents = New Guna.UI2.WinForms.Guna2CheckBox()
        lblName = New Label()
        lblNumber = New Label()
        SuspendLayout()
        ' 
        ' chkResidents
        ' 
        chkResidents.AutoSize = True
        chkResidents.CheckedState.BorderColor = Color.FromArgb(CByte(94), CByte(148), CByte(255))
        chkResidents.CheckedState.BorderRadius = 0
        chkResidents.CheckedState.BorderThickness = 0
        chkResidents.CheckedState.FillColor = Color.FromArgb(CByte(94), CByte(148), CByte(255))
        chkResidents.ForeColor = SystemColors.ControlText
        chkResidents.Location = New Point(13, 16)
        chkResidents.Name = "chkResidents"
        chkResidents.Size = New Size(15, 14)
        chkResidents.TabIndex = 0
        chkResidents.UncheckedState.BorderColor = Color.FromArgb(CByte(125), CByte(137), CByte(149))
        chkResidents.UncheckedState.BorderRadius = 0
        chkResidents.UncheckedState.BorderThickness = 0
        chkResidents.UncheckedState.FillColor = Color.FromArgb(CByte(125), CByte(137), CByte(149))
        ' 
        ' lblName
        ' 
        lblName.AutoSize = True
        lblName.Font = New Font("Calibri", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblName.Location = New Point(40, 7)
        lblName.Name = "lblName"
        lblName.Size = New Size(45, 18)
        lblName.TabIndex = 1
        lblName.Text = "Name"
        ' 
        ' lblNumber
        ' 
        lblNumber.AutoSize = True
        lblNumber.Font = New Font("Calibri", 11.25F)
        lblNumber.ForeColor = Color.Gray
        lblNumber.Location = New Point(40, 23)
        lblNumber.Name = "lblNumber"
        lblNumber.Size = New Size(59, 18)
        lblNumber.TabIndex = 2
        lblNumber.Text = "Number"
        ' 
        ' UserControlAlerts
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(lblNumber)
        Controls.Add(lblName)
        Controls.Add(chkResidents)
        Name = "UserControlAlerts"
        Size = New Size(257, 52)
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents chkResidents As Guna.UI2.WinForms.Guna2CheckBox
    Friend WithEvents lblName As Label
    Friend WithEvents lblNumber As Label

End Class
