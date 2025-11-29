Imports System.Drawing.Text
Imports System.Drawing.Drawing2D
Imports Guna.UI2.WinForms

Public Class MainForm
    ' Keep track of currently displayed UserControl
    Private currentControl As UserControl

    ' Keep track of currently active button (Guna2Button)
    Private currentButton As Guna2Button = Nothing

    ' Function to display a UserControl in PanelContainer
    Private Sub ShowUserControl(control As UserControl)
        If currentControl IsNot Nothing Then
            PanelContainer.Controls.Remove(currentControl)
            currentControl.Dispose()
        End If

        currentControl = control
        control.Dock = DockStyle.Fill
        PanelContainer.Controls.Add(control)
        control.BringToFront()
    End Sub

    ' Reset visual for an inactive Guna2Button (non-gradient)
    Private Sub ResetButtonVisual(btn As Guna2Button)
        If btn Is Nothing Then Return

        btn.Checked = False
        btn.ForeColor = Color.White

        ' Use true black for the base (unchecked) color per your design
        btn.FillColor = Color.Black

        ' Force repaint so the Paint handler draws the darker right side only when appropriate
        btn.Invalidate()
    End Sub

    ' Apply visual for an active Guna2Button
    Private Sub ApplyActiveVisual(btn As Guna2Button)
        If btn Is Nothing Then Return

        btn.Checked = True
        btn.ForeColor = Color.White
        btn.FillColor = Color.FromArgb(3, 83, 164) ' active color

        ' Force repaint to remove the unchecked overlay
        btn.Invalidate()
    End Sub

    ' Function to activate clicked sidebar button
    Private Sub ActivateButton(btn As Guna2Button)
        If btn Is Nothing Then Return

        ' If a different button was active, reset it
        If currentButton IsNot Nothing AndAlso currentButton IsNot btn Then
            ResetButtonVisual(currentButton)
        End If

        ' Set new button as active
        currentButton = btn
        ApplyActiveVisual(currentButton)

        ' Update topbar title to match the clicked button text
        If TopbarTitle IsNot Nothing Then
            TopbarTitle.Text = btn.Text
        End If
    End Sub

    ' Paint handler that draws a darker right-side overlay when the button is unchecked.
    ' If the base FillColor is already black we skip the overlay so the right side stays black.
    Private Sub SidebarButton_Paint(sender As Object, e As PaintEventArgs)
        Dim btn = TryCast(sender, Guna2Button)
        If btn Is Nothing Then Return

        ' Only draw overlay for unchecked buttons
        If btn.Checked Then Return

        ' If base is already black, do not draw overlay (keeps right side black)
        If btn.FillColor.Equals(Color.Black) Then Return

        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias

        ' Width of the darker right side (adjust percentage as desired)
        Dim overlayWidth = CInt(btn.Width * 0.3)
        If overlayWidth <= 0 Then Return

        Dim rightRect = New Rectangle(btn.Width - overlayWidth, 0, overlayWidth, btn.Height)

        ' Use a semi-opaque black to darken the right side when base is not black
        Using brush = New SolidBrush(Color.FromArgb(160, 0, 0, 0))
            g.FillRectangle(brush, rightRect)
        End Using
    End Sub

    ' MainForm load event
    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Attach Paint handler and initialize visuals for sidebar buttons
        For Each ctrl As Control In SidebarMenu.Controls
            Dim gbtn = TryCast(ctrl, Guna2Button)
            If gbtn IsNot Nothing Then
                ' Avoid attaching multiple times
                If Not Object.ReferenceEquals(gbtn.Tag, "paintAttached") Then
                    AddHandler gbtn.Paint, AddressOf SidebarButton_Paint
                    gbtn.Tag = "paintAttached"
                End If

                ' Make the button behave like a radio (so Checked state toggles)
                Try
                    gbtn.ButtonMode = Guna.UI2.WinForms.Enums.ButtonMode.RadioButton
                Catch
                End Try

                ResetButtonVisual(gbtn)
            End If
        Next

        ShowUserControl(New DashboardUserControl())
        ActivateButton(DirectCast(btnDashboard, Guna2Button))
    End Sub

    ' Sidebar button click events
    Private Sub btnDashboard_Click(sender As Object, e As EventArgs) Handles btnDashboard.Click
        ShowUserControl(New DashboardUserControl())
        ActivateButton(DirectCast(sender, Guna2Button))
    End Sub

    Private Sub btnResidents_Click(sender As Object, e As EventArgs) Handles btnResidents.Click
        ShowUserControl(New ResidentsUserControl())
        ActivateButton(DirectCast(sender, Guna2Button))
    End Sub

    Private Sub btnLocalUpdates_Click(sender As Object, e As EventArgs)
        ShowUserControl(New LocalUpdatesUserControl)
        ActivateButton(DirectCast(sender, Guna2Button))
    End Sub

    Private Sub btnEvacuation_Click(sender As Object, e As EventArgs) Handles btnEvacuation.Click
        ShowUserControl(New EvacuationUserControl())
        ActivateButton(DirectCast(sender, Guna2Button))
    End Sub

    Private Sub btnAlerts_Click(sender As Object, e As EventArgs) Handles btnAlerts.Click
        ShowUserControl(New AlertsUserControl())
        ActivateButton(DirectCast(sender, Guna2Button))
    End Sub

    Private Sub btnSensors_Click(sender As Object, e As EventArgs) Handles btnSensors.Click
        ShowUserControl(New SensorsUserControl())
        ActivateButton(DirectCast(sender, Guna2Button))
    End Sub

    Private Sub btnReports_Click(sender As Object, e As EventArgs) Handles btnReports.Click
        ShowUserControl(New ReportsUserControl())
        ActivateButton(DirectCast(sender, Guna2Button))
    End Sub

    Private Sub btnSettings_Click(sender As Object, e As EventArgs) Handles btnSettings.Click
        ShowUserControl(New SettingsUserControl())
        ActivateButton(DirectCast(sender, Guna2Button))
    End Sub
End Class