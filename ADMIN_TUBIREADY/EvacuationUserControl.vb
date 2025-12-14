Public Class EvacuationUserControl
    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click

    End Sub

    Private Sub Label3_Click(sender As Object, e As EventArgs) Handles Label3.Click

    End Sub

    Private Sub Label12_Click(sender As Object, e As EventArgs) Handles Label12.Click

    End Sub

    Private Sub Label2_Click(sender As Object, e As EventArgs) Handles Label2.Click

    End Sub

    Private Sub Label15_Click(sender As Object, e As EventArgs) Handles Label15.Click

    End Sub

    Private Sub Label14_Click(sender As Object, e As EventArgs) Handles Label14.Click

    End Sub

    Private Sub Guna2ImageButton1_Click(sender As Object, e As EventArgs) Handles Guna2ImageButton1.Click

    End Sub

    Private Sub Label32_Click(sender As Object, e As EventArgs) Handles Label32.Click

    End Sub

    Private Sub Guna2CircleButton1_Click(sender As Object, e As EventArgs) Handles Guna2CircleButton1.Click

    End Sub

    Private Sub Label21_Click(sender As Object, e As EventArgs) Handles Label21.Click

    End Sub

    Private Sub Guna2Button1_Click(sender As Object, e As EventArgs) Handles Guna2Button1.Click
        ' 1. Create the EvacuationListUserControl instance
        Dim evacuationList As New EvacuationListUserControl()
        evacuationList.Dock = DockStyle.Fill

        ' 2. Try to find a suitable container on the parent form
        Dim parentForm As Form = Me.FindForm()
        Dim container As Control = Nothing

        If parentForm IsNot Nothing Then
            Dim candidateNames As String() = New String() {"PanelContainer", "MainPanel", "panelMain", "ContentPanel", "pnlMain", "panelContent"}
            For Each n As String In candidateNames
                container = FindControlRecursive(parentForm, n)
                If container IsNot Nothing Then Exit For
            Next
        End If

        ' 3. Fallbacks
        If container Is Nothing Then
            container = Me.Parent
        End If
        If container Is Nothing Then
            container = Me ' last resort: add to this control
        End If

        ' 4. Remove existing instances of EvacuationListUserControl in the container
        Dim toRemove As New List(Of Control)()
        For Each c As Control In container.Controls
            If TypeOf c Is EvacuationListUserControl Then
                toRemove.Add(c)
            End If
        Next
        For Each c As Control In toRemove
            container.Controls.Remove(c)
            Try
                c.Dispose()
            Catch
                ' ignore dispose errors
            End Try
        Next

        ' 5. Add and show the new control
        container.SuspendLayout()
        container.Controls.Add(evacuationList)
        evacuationList.BringToFront()
        container.ResumeLayout()
    End Sub

    ' Helper: recursively find a control by name (case-insensitive)
    Private Function FindControlRecursive(root As Control, name As String) As Control
        If root Is Nothing OrElse String.IsNullOrEmpty(name) Then Return Nothing
        If String.Equals(root.Name, name, StringComparison.OrdinalIgnoreCase) Then
            Return root
        End If
        For Each c As Control In root.Controls
            Dim found As Control = FindControlRecursive(c, name)
            If found IsNot Nothing Then Return found
        Next
        Return Nothing
    End Function
End Class
