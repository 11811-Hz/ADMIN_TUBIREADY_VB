<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class EvacuationListUserControl
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
        Dim CustomizableEdges3 As Guna.UI2.WinForms.Suite.CustomizableEdges = New Guna.UI2.WinForms.Suite.CustomizableEdges()
        Dim CustomizableEdges4 As Guna.UI2.WinForms.Suite.CustomizableEdges = New Guna.UI2.WinForms.Suite.CustomizableEdges()
        Dim CustomizableEdges1 As Guna.UI2.WinForms.Suite.CustomizableEdges = New Guna.UI2.WinForms.Suite.CustomizableEdges()
        Dim CustomizableEdges2 As Guna.UI2.WinForms.Suite.CustomizableEdges = New Guna.UI2.WinForms.Suite.CustomizableEdges()
        pnlMain = New Guna.UI2.WinForms.Guna2Panel()
        Guna2Panel1 = New Guna.UI2.WinForms.Guna2Panel()
        pnlMain.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlMain
        ' 
        pnlMain.Controls.Add(Guna2Panel1)
        pnlMain.CustomizableEdges = CustomizableEdges3
        pnlMain.Dock = DockStyle.Fill
        pnlMain.Location = New Point(0, 0)
        pnlMain.Name = "pnlMain"
        pnlMain.ShadowDecoration.CustomizableEdges = CustomizableEdges4
        pnlMain.Size = New Size(1235, 924)
        pnlMain.TabIndex = 0
        ' 
        ' Guna2Panel1
        ' 
        Guna2Panel1.CustomizableEdges = CustomizableEdges1
        Guna2Panel1.Location = New Point(33, 29)
        Guna2Panel1.Name = "Guna2Panel1"
        Guna2Panel1.ShadowDecoration.CustomizableEdges = CustomizableEdges2
        Guna2Panel1.Size = New Size(346, 78)
        Guna2Panel1.TabIndex = 0
        ' 
        ' EvacuationListUserControl
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(pnlMain)
        Name = "EvacuationListUserControl"
        Size = New Size(1235, 924)
        pnlMain.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlMain As Guna.UI2.WinForms.Guna2Panel
    Friend WithEvents Guna2Panel1 As Guna.UI2.WinForms.Guna2Panel

End Class
