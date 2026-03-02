Imports System.Drawing.Text
Imports System.Drawing.Drawing2D
Imports Guna.UI2.WinForms

Public Class MainForm

    ' ===== AUTH STATE =====
    Public Property ForcePasswordChange As Boolean = False
    Public Property LoggedInUsername As String = ""

    ' ===== UI STATE =====
    Private currentControl As UserControl = Nothing
    Private currentButton As Guna2Button = Nothing

    ' ===== FORM EVENTS =====
    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ' Initialize sidebar button visuals
        For Each ctrl As Control In SidebarMenu.Controls
            Dim gbtn = TryCast(ctrl, Guna2Button)
            If gbtn IsNot Nothing Then
                AddHandler gbtn.Paint, AddressOf SidebarButton_Paint
                gbtn.ButtonMode = Enums.ButtonMode.RadioButton
                ResetButtonVisual(gbtn)
            End If
        Next

        ' Only show dashboard if NOT forcing password change
        If Not ForcePasswordChange Then
            ShowUserControl(New DashboardUserControl())
            ActivateButton(btnDashboard)
        End If
    End Sub

    Private Sub MainForm_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        If ForcePasswordChange Then
            DisableNavigation()
            OpenSecurityPrivacy()
        End If
    End Sub

    ' ===== CORE NAVIGATION =====
    Private Sub ShowUserControl(ctrl As UserControl)
        If currentControl IsNot Nothing Then
            PanelContainer.Controls.Remove(currentControl)
            currentControl.Dispose()
        End If

        currentControl = ctrl
        ctrl.Dock = DockStyle.Fill
        PanelContainer.Controls.Add(ctrl)
        ctrl.BringToFront()
    End Sub

    ' ===== FORCED PASSWORD FLOW =====
    Private Sub OpenSecurityPrivacy()
        Dim sec As New SecurityPrivacyUserControl()
        sec.Username = LoggedInUsername

        AddHandler sec.PasswordChangedSuccessfully, AddressOf OnPasswordChanged

        ShowUserControl(sec)
        TopbarTitle.Text = "Security & Privacy"
    End Sub

    Private Sub OnPasswordChanged()
        ForcePasswordChange = False
        EnableNavigation()

        MessageBox.Show(
            "Password updated successfully. Full access restored.",
            "Security",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        )

        ShowUserControl(New DashboardUserControl())
        ActivateButton(btnDashboard)
    End Sub

    ' ===== NAVIGATION LOCKING =====
    Private Sub DisableNavigation()
        For Each ctrl As Control In SidebarMenu.Controls
            Dim gbtn = TryCast(ctrl, Guna2Button)
            If gbtn IsNot Nothing AndAlso gbtn IsNot btnSettings Then
                gbtn.Enabled = False
            End If
        Next
    End Sub

    Private Sub EnableNavigation()
        For Each ctrl As Control In SidebarMenu.Controls
            Dim gbtn = TryCast(ctrl, Guna2Button)
            If gbtn IsNot Nothing Then
                gbtn.Enabled = True
            End If
        Next
    End Sub

    ' ===== BUTTON VISUALS =====
    Private Sub ResetButtonVisual(btn As Guna2Button)
        btn.Checked = False
        btn.FillColor = Color.Black
        btn.ForeColor = Color.White
        btn.Invalidate()
    End Sub

    Private Sub ApplyActiveVisual(btn As Guna2Button)
        btn.Checked = True
        btn.FillColor = Color.FromArgb(3, 83, 164)
        btn.ForeColor = Color.White
        btn.Invalidate()
    End Sub

    Private Sub ActivateButton(btn As Guna2Button)
        If currentButton IsNot Nothing AndAlso currentButton IsNot btn Then
            ResetButtonVisual(currentButton)
        End If

        currentButton = btn
        ApplyActiveVisual(btn)
        TopbarTitle.Text = btn.Text
    End Sub

    Private Sub SidebarButton_Paint(sender As Object, e As PaintEventArgs)
        Dim btn = TryCast(sender, Guna2Button)
        If btn Is Nothing OrElse btn.Checked Then Return

        If btn.FillColor = Color.Black Then Return

        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        Dim overlayWidth = CInt(btn.Width * 0.3)

        Using brush As New SolidBrush(Color.FromArgb(160, 0, 0, 0))
            g.FillRectangle(brush, btn.Width - overlayWidth, 0, overlayWidth, btn.Height)
        End Using
    End Sub

    ' ===== SIDEBAR CLICK EVENTS =====
    Private Sub btnDashboard_Click(sender As Object, e As EventArgs) Handles btnDashboard.Click
        ShowUserControl(New DashboardUserControl())
        ActivateButton(sender)
    End Sub

    Private Sub btnResidents_Click(sender As Object, e As EventArgs) Handles btnResidents.Click
        ShowUserControl(New ResidentsUserControl())
        ActivateButton(sender)
    End Sub

    Private Sub btnEvacuation_Click(sender As Object, e As EventArgs) Handles btnEvacuation.Click
        ShowUserControl(New EvacuationUserControl())
        ActivateButton(sender)
    End Sub

    Private Sub btnAlerts_Click(sender As Object, e As EventArgs) Handles btnAlerts.Click
        ShowUserControl(New AlertsUserControl())
        ActivateButton(sender)
    End Sub

    Private Sub btnSensors_Click(sender As Object, e As EventArgs) Handles btnSensors.Click
        ShowUserControl(New SensorsUserControl())
        ActivateButton(sender)
    End Sub

    Private Sub btnReports_Click(sender As Object, e As EventArgs) Handles btnReports.Click
        ShowUserControl(New ReportsUserControl())
        ActivateButton(sender)
    End Sub

    Private Sub btnSettings_Click(sender As Object, e As EventArgs) Handles btnSettings.Click
        ShowUserControl(New SettingsUserControl())
        ActivateButton(sender)
    End Sub

    Private Sub btnMappingGuna_Click(sender As Object, e As EventArgs) Handles btnMappingGuna.Click
        ShowUserControl(New MappingUserControl())
        ActivateButton(sender)
    End Sub

End Class
