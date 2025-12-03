Public Class AlertsUserControl

    Private Sub txtBroadcastMessage_TextChanged(sender As Object, e As EventArgs) Handles txtBroadcastMessage.TextChanged
        lblCharCount.Text = txtBroadcastMessage.Text.Length.ToString() & " Characters"
    End Sub
End Class
