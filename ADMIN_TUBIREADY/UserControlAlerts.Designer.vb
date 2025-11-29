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
        Guna2CheckBox1 = New Guna.UI2.WinForms.Guna2CheckBox()
        lblAlertsName = New Label()
        lblAlertsNumber = New Label()
        SuspendLayout()
        ' 
        ' Guna2CheckBox1
        ' 
        Guna2CheckBox1.AutoSize = True
        Guna2CheckBox1.CheckedState.BorderColor = Color.FromArgb(CByte(94), CByte(148), CByte(255))
        Guna2CheckBox1.CheckedState.BorderRadius = 0
        Guna2CheckBox1.CheckedState.BorderThickness = 0
        Guna2CheckBox1.CheckedState.FillColor = Color.FromArgb(CByte(94), CByte(148), CByte(255))
        Guna2CheckBox1.ForeColor = SystemColors.ControlText
        Guna2CheckBox1.Location = New Point(13, 16)
        Guna2CheckBox1.Name = "Guna2CheckBox1"
        Guna2CheckBox1.Size = New Size(15, 14)
        Guna2CheckBox1.TabIndex = 0
        Guna2CheckBox1.UncheckedState.BorderColor = Color.FromArgb(CByte(125), CByte(137), CByte(149))
        Guna2CheckBox1.UncheckedState.BorderRadius = 0
        Guna2CheckBox1.UncheckedState.BorderThickness = 0
        Guna2CheckBox1.UncheckedState.FillColor = Color.FromArgb(CByte(125), CByte(137), CByte(149))
        ' 
        ' lblAlertsName
        ' 
        lblAlertsName.AutoSize = True
        lblAlertsName.Font = New Font("Calibri", 11.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblAlertsName.Location = New Point(40, 7)
        lblAlertsName.Name = "lblAlertsName"
        lblAlertsName.Size = New Size(45, 18)
        lblAlertsName.TabIndex = 1
        lblAlertsName.Text = "Name"
        ' 
        ' lblAlertsNumber
        ' 
        lblAlertsNumber.AutoSize = True
        lblAlertsNumber.Font = New Font("Calibri", 11.25F)
        lblAlertsNumber.ForeColor = Color.Gray
        lblAlertsNumber.Location = New Point(40, 23)
        lblAlertsNumber.Name = "lblAlertsNumber"
        lblAlertsNumber.Size = New Size(59, 18)
        lblAlertsNumber.TabIndex = 2
        lblAlertsNumber.Text = "Number"
        ' 
        ' UserControlAlerts
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(lblAlertsNumber)
        Controls.Add(lblAlertsName)
        Controls.Add(Guna2CheckBox1)
        Name = "UserControlAlerts"
        Size = New Size(257, 52)
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Guna2CheckBox1 As Guna.UI2.WinForms.Guna2CheckBox
    Friend WithEvents lblAlertsName As Label
    Friend WithEvents lblAlertsNumber As Label

End Class
