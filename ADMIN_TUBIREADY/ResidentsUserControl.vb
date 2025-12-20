Public Class ResidentsUserControl

    Private Sub ResidentsUserControl_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Default to Pending view and set indicator lights
        PendingUserLight.FillColor = Color.FromArgb(3, 83, 164)
        RegisteredUserLight.FillColor = Color.Transparent

        ShowPendingControl()
    End Sub

    Private Sub btnPendingUserApproval_Click(sender As Object, e As EventArgs) Handles btnPendingUserApproval.Click
        ' Set Pending light to the requested color and reset the other
        PendingUserLight.FillColor = Color.FromArgb(3, 83, 164)
        RegisteredUserLight.FillColor = Color.Transparent

        ShowPendingControl()
    End Sub

    Private Sub btnRegisteredUser_Click(sender As Object, e As EventArgs) Handles btnRegisteredUser.Click
        ' Set Registered light to the requested color and reset the other
        RegisteredUserLight.FillColor = Color.FromArgb(3, 83, 164)
        PendingUserLight.FillColor = Color.Transparent

        ShowRegisteredControl()
    End Sub

    Private Sub ShowPendingControl()
        If Me.ResidentPanelContainer Is Nothing Then
            Return
        End If

        Try
            Me.ResidentPanelContainer.SuspendLayout()

            ' Reuse existing PendingUserControl if present
            Dim pendingCtrl As PendingUserControl = Nothing
            For Each c As Control In Me.ResidentPanelContainer.Controls
                If TypeOf c Is PendingUserControl Then
                    pendingCtrl = DirectCast(c, PendingUserControl)
                    Exit For
                End If
            Next

            ' If not found, create and add one
            If pendingCtrl Is Nothing Then
                pendingCtrl = New PendingUserControl()
                pendingCtrl.Dock = DockStyle.Fill
                pendingCtrl.Visible = True
                Me.ResidentPanelContainer.Controls.Add(pendingCtrl)
            End If

            pendingCtrl.BringToFront()
        Finally
            Me.ResidentPanelContainer.ResumeLayout()
        End Try
    End Sub

    Private Sub ShowRegisteredControl()
        If Me.ResidentPanelContainer Is Nothing Then
            Return
        End If

        Try
            Me.ResidentPanelContainer.SuspendLayout()

            ' Reuse existing RegisteredUserControl if present
            Dim regCtrl As RegisteredUserControl = Nothing
            For Each c As Control In Me.ResidentPanelContainer.Controls
                If TypeOf c Is RegisteredUserControl Then
                    regCtrl = DirectCast(c, RegisteredUserControl)
                    Exit For
                End If
            Next

            ' If not found, create and add one
            If regCtrl Is Nothing Then
                regCtrl = New RegisteredUserControl()
                regCtrl.Dock = DockStyle.Fill
                regCtrl.Visible = True
                Me.ResidentPanelContainer.Controls.Add(regCtrl)
            End If

            regCtrl.BringToFront()
        Finally
            Me.ResidentPanelContainer.ResumeLayout()
        End Try
    End Sub

    Private Sub Guna2DataGridView1_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles Guna2DataGridView1.CellContentClick

    End Sub
End Class