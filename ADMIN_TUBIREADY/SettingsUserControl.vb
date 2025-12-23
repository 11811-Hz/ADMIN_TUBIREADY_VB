Public Class SettingsUserControl

    Private _manageControl As ManageAccountUserControl
    Private _auditControl As AuditUserControl
    Private _securityControl As SecurityPrivacyUserControl
    Private _accountsControl As AccountUserControl

    Public Sub New()
        InitializeComponent()

        ' Wire button click events to update the lights and top bar.
        AddHandler btnProfile.Click, AddressOf BtnProfile_Click
        AddHandler btnSecurity.Click, AddressOf BtnSecurity_Click
        AddHandler btnManage.Click, AddressOf BtnManage_Click
        AddHandler btnAudit.Click, AddressOf BtnAudit_Click

        ' Ensure initial visual state matches the designer (Manage selected).
        SetActiveLight(ManageLight)

        ' Prepare/manage the ManageAccountUserControl and set it as the default view.
        _manageControl = New ManageAccountUserControl()
        ShowControl(_manageControl)
        TopBarLabel.Text = "View, edit, and control user accounts and access permissions."

        ' Prepare audit control so Audit button can sync immediately.
        _auditControl = New AuditUserControl()

        ' Prepare audit Control so Audit button can sync immediately.
        _securityControl = New SecurityPrivacyUserControl()

        ' Prepare accounts control so Accounts button can sync immediately.
        _accountsControl = New AccountUserControl()
    End Sub

    Private Sub SetActiveLight(activePanel As Guna.UI2.WinForms.Guna2Panel)
        Dim activeColor As System.Drawing.Color = System.Drawing.Color.FromArgb(3, 81, 161)
        Dim inactiveColor As System.Drawing.Color = System.Drawing.Color.Transparent

        ' Reset all to transparent first
        AccountLight.FillColor = inactiveColor
        SecurityLight.FillColor = inactiveColor
        ManageLight.FillColor = inactiveColor
        AuditLight.FillColor = inactiveColor

        ' Light up the selected one
        If activePanel IsNot Nothing Then
            activePanel.FillColor = activeColor
        End If
    End Sub

    ''' <summary>
    ''' Replace the content of the right-side container with the provided control.
    ''' Ensures the control is docked and brought to front.
    ''' </summary>
    Private Sub ShowControl(ctrl As System.Windows.Forms.UserControl)
        If ctrl Is Nothing Then
            SettingsPanelContainer.Controls.Clear()
            Return
        End If

        SettingsPanelContainer.Controls.Clear()
        ctrl.Dock = System.Windows.Forms.DockStyle.Fill
        SettingsPanelContainer.Controls.Add(ctrl)
        ctrl.BringToFront()
    End Sub

    Private Sub BtnProfile_Click(sender As Object, e As EventArgs)
        SetActiveLight(AccountLight)
        TopBarLabel.Text = "Manage your account profile and personal information."
        ' No dedicated profile control implemented yet — clear or add appropriate control here.
        SettingsPanelContainer.Controls.Clear()
    End Sub

    Private Sub BtnSecurity_Click(sender As Object, e As EventArgs)
        SetActiveLight(SecurityLight)
        TopBarLabel.Text = "Configure security settings, passwords, and privacy options."
        If _securityControl Is Nothing Then
            _securityControl = New SecurityPrivacyUserControl()
        End If
        ShowControl(_securityControl)
    End Sub

    Private Sub BtnManage_Click(sender As Object, e As EventArgs)
        SetActiveLight(ManageLight)
        TopBarLabel.Text = "View, edit, and control user accounts and access permissions."

        If _manageControl Is Nothing Then
            _manageControl = New ManageAccountUserControl()
        End If

        ShowControl(_manageControl)
    End Sub

    Private Sub BtnAudit_Click(sender As Object, e As EventArgs)
        SetActiveLight(AuditLight)
        TopBarLabel.Text = "View audit logs and system activity."

        If _auditControl Is Nothing Then
            _auditControl = New AuditUserControl()
        End If

        ShowControl(_auditControl)
    End Sub
End Class