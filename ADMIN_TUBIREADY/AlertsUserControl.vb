Public Class AlertsUserControl

    Private Sub txtBroadcastMessage_TextChanged(sender As Object, e As EventArgs) Handles txtBroadcastMessage.TextChanged
        ' ts just changes the character count label as you type
        lblCharCount.Text = txtBroadcastMessage.Text.Length.ToString() & " Characters"
    End Sub

    Private Sub AlertsUserControl_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' TODO ADD CONNECTION TO DATABASE TO PULL SEARCH RESULTS
        ' REMOVE THIS AND POPULATE WITH ACTUAL DATA FROM DATABASE

        ' Avoid running at design time
        If Me.DesignMode Then
            Return
        End If

        ' Prefer the known FlowLayoutPanel name "flpContacts"
        Dim flp As FlowLayoutPanel = Nothing
        Dim found() As Control = Me.Controls.Find("flpContact", True)
        If found IsNot Nothing AndAlso found.Length > 0 AndAlso TypeOf found(0) Is FlowLayoutPanel Then
            flp = CType(found(0), FlowLayoutPanel)
        Else
            ' Known name not present - nothing to do
            Return
        End If

        ' Create and add 3 instances of UserControlAlerts for testing
        Dim count As Integer = 3
        For i As Integer = 1 To count
            Dim uc As New UserControlAlerts()
            uc.Name = "UserControlAlert" & i.ToString()
            uc.Margin = New Padding(6) ' slight spacing for visual clarity
            flp.Controls.Add(uc)
        Next
    End Sub

End Class
