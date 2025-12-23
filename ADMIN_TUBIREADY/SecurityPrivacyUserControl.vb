Imports Microsoft.Data.SqlClient
Imports System.Text.RegularExpressions
Imports ADMIN_TUBIREADY.HashingModule
Public Class SecurityPrivacyUserControl

    Private Function IsPasswordStrong(password As String) As Boolean
        ' Regex Explanation:
        ' ^                 -> Start of string
        ' (?=.*[0-9])       -> Must contain at least one digit
        ' (?=.*[!@#$%^&*])  -> Must contain at least one special character
        ' .{8,}             -> Must be at least 8 characters long
        ' $                 -> End of string
        Dim pattern As String = "^(?=.*[0-9])(?=.*[!@#$%^&*]).{8,}$"
        Return Regex.IsMatch(password, pattern)
    End Function

    Private Sub btnUpdatePassword_Click(sender As Object, e As EventArgs) Handles btnUpdatePassword.Click
        Dim currentPass As String = txtCurrentPassword.Text
        Dim newPass As String = txtNewPassword.Text
        Dim confirmPass As String = txtConfirmNewPassword.Text
        Dim username As String = "admin" ' Or use your global variable: CurrentUser

        ' --- STEP 1: BASIC VALIDATION ---
        If String.IsNullOrEmpty(currentPass) Or String.IsNullOrEmpty(newPass) Then
            MessageBox.Show("Please fill in all password fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If newPass <> confirmPass Then
            MessageBox.Show("New passwords do not match.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        If Not IsPasswordStrong(newPass) Then
            MessageBox.Show("Password must be at least 8 characters and contain a number and special symbol.", "Weak Password", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' --- STEP 2: VERIFY CURRENT PASSWORD & UPDATE ---
        Using conn As New SqlConnection(ConnString)
            Try
                conn.Open()

                ' A. Get the OLD Salt and Hash to verify identity
                Dim verifyQuery As String = "SELECT PasswordHash, PasswordSalt FROM Users WHERE Uname = @u"
                Using cmd As New SqlCommand(verifyQuery, conn)
                    cmd.Parameters.AddWithValue("@u", username)

                    Using reader As SqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Dim storedHash As String = reader("PasswordHash").ToString()
                            Dim storedSalt As String = reader("PasswordSalt").ToString()

                            ' Hash the INPUT current password with the STORED salt
                            Dim checkHash As String = ComputeHash(currentPass, storedSalt)

                            If checkHash <> storedHash Then
                                MessageBox.Show("Current password is incorrect.", "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                Return
                            End If
                        Else
                            MessageBox.Show("User not found.", "Error")
                            Return
                        End If
                    End Using
                End Using

                ' B. If we get here, the Current Password was correct. 
                '    Generate NEW Salt and NEW Hash.
                Dim newSalt As String = CreateRandomSalt()
                Dim newHash As String = ComputeHash(newPass, newSalt)

                ' Checkbox Logic: (Optional) You might want to save this preference to DB
                ' Dim autoReset As Boolean = chkAutoReset.Checked 

                Dim updateQuery As String = "UPDATE Users SET " &
                                            "PasswordHash = @h, " &
                                            "PasswordSalt = @s, " &
                                            "LastPassChange = GETDATE() " &
                                            "WHERE Uname = @u"

                Using cmdUpdate As New SqlCommand(updateQuery, conn)
                    cmdUpdate.Parameters.AddWithValue("@h", newHash)
                    cmdUpdate.Parameters.AddWithValue("@s", newSalt)
                    cmdUpdate.Parameters.AddWithValue("@u", username)

                    cmdUpdate.ExecuteNonQuery()
                    MessageBox.Show("Password updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

                    ' Clear fields for security
                    txtCurrentPassword.Clear()
                    txtNewPassword.Clear()
                    txtConfirmNewPassword.Clear()
                End Using

            Catch ex As Exception
                MessageBox.Show("Database Error: " & ex.Message)
            End Try
        End Using
    End Sub

    Private Sub btnRecoveryEmail_Click(sender As Object, e As EventArgs) Handles btnRecoveryEmail.Click
        Dim newEmail As String = txtRecoveryEmail.Text
        Dim username As String = "ADMIN"

        ' Simple Email Regex Validation
        If Not Regex.IsMatch(newEmail, "^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$") Then
            MessageBox.Show("Please enter a valid email address.", "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Using conn As New SqlConnection(ConnString)
            Try
                conn.Open()
                Dim query As String = "UPDATE Users SET Email = @email WHERE Uname = @u"

                Using cmd As New SqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@email", newEmail)
                    cmd.Parameters.AddWithValue("@u", username)

                    Dim rowsAffected As Integer = cmd.ExecuteNonQuery()

                    If rowsAffected > 0 Then
                        MessageBox.Show("Recovery email updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Else
                        MessageBox.Show("Update failed. User not found.", "Error")
                    End If
                End Using
            Catch ex As Exception
                MessageBox.Show("Error: " & ex.Message)
            End Try
        End Using
    End Sub
End Class
