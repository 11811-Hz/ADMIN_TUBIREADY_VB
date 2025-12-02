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
        ckcAlerts = New Guna.UI2.WinForms.Guna2CheckBox()
        lblAlertsName = New Label()
        lblAlertsNumber = New Label()
        SuspendLayout()
        ' 
        ' ckcAlerts
        ' 
        ckcAlerts.AutoSize = True
        ckcAlerts.CheckedState.BorderColor = Color.FromArgb(CByte(94), CByte(148), CByte(255))
        ckcAlerts.CheckedState.BorderRadius = 0
        ckcAlerts.CheckedState.BorderThickness = 0
        ckcAlerts.CheckedState.FillColor = Color.FromArgb(CByte(94), CByte(148), CByte(255))
        ckcAlerts.ForeColor = SystemColors.ControlText
        ckcAlerts.Location = New Point(13, 16)
        ckcAlerts.Name = "ckcAlerts"
        ckcAlerts.Size = New Size(15, 14)
        ckcAlerts.TabIndex = 0
        ckcAlerts.UncheckedState.BorderColor = Color.FromArgb(CByte(125), CByte(137), CByte(149))
        ckcAlerts.UncheckedState.BorderRadius = 0
        ckcAlerts.UncheckedState.BorderThickness = 0
        ckcAlerts.UncheckedState.FillColor = Color.FromArgb(CByte(125), CByte(137), CByte(149))
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
        Controls.Add(ckcAlerts)
        Name = "UserControlAlerts"
        Size = New Size(257, 52)
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents ckcAlerts As Guna.UI2.WinForms.Guna2CheckBox
    Friend WithEvents lblAlertsName As Label
    Friend WithEvents lblAlertsNumber As Label

End Class
