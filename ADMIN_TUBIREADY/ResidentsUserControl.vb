Public Class ResidentsUserControl

    Private Sub btnPendingUserApproval_Click(sender As Object, e As EventArgs) Handles btnPendingUserApproval.Click
        ' Set Pending light to the requested color and reset the other
        PendingUserLight.FillColor = Color.FromArgb(3, 83, 164)
        RegisteredUserLight.FillColor = Color.Transparent
    End Sub

    Private Sub btnRegisteredUser_Click(sender As Object, e As EventArgs) Handles btnRegisteredUser.Click
        ' Set Registered light to the requested color and reset the other
        RegisteredUserLight.FillColor = Color.FromArgb(3, 83, 164)
        PendingUserLight.FillColor = Color.Transparent
    End Sub


End Class
