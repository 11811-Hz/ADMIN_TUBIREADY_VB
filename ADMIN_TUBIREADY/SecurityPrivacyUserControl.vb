Imports Microsoft.Data.SqlClient
Imports System.Text.RegularExpressions
Imports ADMIN_TUBIREADY.HashingModule

Public Class SecurityPrivacyUserControl

    ' ===== PROVIDED BY MAINFORM =====
    Public Property Username As String = ""

    ' ===== EVENT TO NOTIFY MAINFORM =====
    Public Event PasswordChangedSuccessfully()

    ' ===== PASSWORD STRENGTH CHECK =====
    Private Function IsPasswordStrong(password As String) As Boolean
        ' At least 8 chars, 1 digit, 1 special char
        Dim pattern As String = "^(?=.*[0-9])(?=.*[!@#$%^&*]).{8,}$"
        Return Regex.IsMatch(password, pattern)
    End Function

    ' ===== UPDATE PASSWORD =====
    Private Sub btnUpdatePassword_Click(sender As Object, e As EventArgs) Handles btnUpdatePassword.Click

        Dim currentPass As String = txtCurrentPassword.Text
        Dim newPass As String = txtNewPassword.Text
        Dim confirmPass As String = txtConfirmNewPassword.Text
        Dim username As String = Me.Username

        ' ---- VALIDATION ----
        If String.IsNullOrWhiteSpace(username) Then
            MessageBox.Show("Invalid user context.", "Error")
            Return
        End If

        If String.IsNullOrEmpty(currentPass) OrElse
           String.IsNullOrEmpty(newPass) OrElse
           String.IsNullOrEmpty(confirmPass) Then
            MessageBox.Show("Please fill in all password fields.", "Error")
            Return
        End If

        If newPass <> confirmPass Then
            MessageBox.Show("New passwords do not match.", "Error")
            Return
        End If

        If Not IsPasswordStrong(newPass) Then
            MessageBox.Show(
                "Password must be at least 8 characters and contain a number and special symbol.",
                "Weak Password"
            )
            Return
        End If

        ' ---- DATABASE ----
        Using conn As New SqlConnection(ConnectionHelper.UniversalConnString)
            Try
                conn.Open()

                ' Verify current password
                Dim verifyQuery As String =
                    "SELECT PasswordHash, PasswordSalt FROM Users WHERE Uname = @u"

                Using cmd As New SqlCommand(verifyQuery, conn)
                    cmd.Parameters.AddWithValue("@u", username)

                    Using reader = cmd.ExecuteReader()
                        If Not reader.Read() Then
                            MessageBox.Show("User not found.", "Error")
                            Return
                        End If

                        Dim storedHash = reader("PasswordHash").ToString()
                        Dim storedSalt = reader("PasswordSalt").ToString()

                        Dim checkHash = ComputeHash(currentPass, storedSalt)

                        If checkHash <> storedHash Then
                            MessageBox.Show("Current password is incorrect.", "Authentication Failed")
                            Return
                        End If
                    End Using
                End Using

                ' Generate new credentials
                Dim newSalt = CreateRandomSalt()
                Dim newHash = ComputeHash(newPass, newSalt)

                Dim updateQuery As String =
                    "UPDATE Users SET PasswordHash=@h, PasswordSalt=@s, LastPassChange=GETDATE() WHERE Uname=@u"

                Using cmdUpdate As New SqlCommand(updateQuery, conn)
                    cmdUpdate.Parameters.AddWithValue("@h", newHash)
                    cmdUpdate.Parameters.AddWithValue("@s", newSalt)
                    cmdUpdate.Parameters.AddWithValue("@u", username)
                    cmdUpdate.ExecuteNonQuery()
                End Using

                MessageBox.Show("Password updated successfully.", "Success")

                txtCurrentPassword.Clear()
                txtNewPassword.Clear()
                txtConfirmNewPassword.Clear()

                ' 🔔 Notify MainForm
                RaiseEvent PasswordChangedSuccessfully()

            Catch ex As Exception
                MessageBox.Show("Database Error: " & ex.Message)
            End Try
        End Using
    End Sub

    ' ===== UPDATE RECOVERY EMAIL =====
    Private Sub btnRecoveryEmail_Click(sender As Object, e As EventArgs) Handles btnRecoveryEmail.Click

        Dim newEmail As String = txtRecoveryEmail.Text
        Dim username As String = Me.Username

        If String.IsNullOrWhiteSpace(username) Then
            MessageBox.Show("Invalid user context.", "Error")
            Return
        End If

        If Not Regex.IsMatch(newEmail, "^[\w\-\.]+@([\w\-]+\.)+[\w\-]{2,4}$") Then
            MessageBox.Show("Please enter a valid email address.", "Invalid Email")
            Return
        End If

        Using conn As New SqlConnection(ConnectionHelper.UniversalConnString)
            Try
                conn.Open()

                Dim query As String = "UPDATE Users SET Email=@e WHERE Uname=@u"

                Using cmd As New SqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@e", newEmail)
                    cmd.Parameters.AddWithValue("@u", username)
                    cmd.ExecuteNonQuery()
                End Using

                MessageBox.Show("Recovery email updated.", "Success")

            Catch ex As Exception
                MessageBox.Show("Database Error: " & ex.Message)
            End Try
        End Using
    End Sub

End Class
