Imports Microsoft.Data.SqlClient
Imports System.Security.Cryptography
Imports System.Text
Imports ADMIN_TUBIREADY.HashingModule
' Ensure this matches your actual Project Name namespace
Imports ADMIN_TUBIREADY.ConnectionHelper

Public Class LoginForm

    ' ---------------------------------------------------------
    ' LOGIN BUTTON
    ' ---------------------------------------------------------
    Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        Dim inputUser As String = txtUsername.Text
        Dim inputPass As String = txtPassword.Text

        ' UPDATE: Using UniversalConnString from your ConnectionHelper Module
        Using conn As New SqlConnection(ConnectionHelper.UniversalConnString)
            Try
                conn.Open()
                ' Retrieve the Hash, Salt, and Password Change Date
                Dim query As String = "SELECT PasswordHash, PasswordSalt, LastPassChange FROM Users WHERE Uname = @uname"

                Using cmd As New SqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@uname", inputUser)

                    Using reader As SqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            ' 1. Get DB values
                            Dim dbHash As String = reader("PasswordHash").ToString()
                            Dim dbSalt As String = reader("PasswordSalt").ToString()
                            Dim lastChange As Date = Convert.ToDateTime(reader("LastPassChange"))

                            ' 2. Re-create the hash using the stored salt + input password
                            Dim checkHash As String = ComputeHash(inputPass, dbSalt)

                            ' 3. Compare
                            If dbHash = checkHash Then
                                ' Login Success! Now Check the 30-Day Rule
                                Dim daysDiff As Integer = (Date.Now - lastChange).Days

                                If daysDiff >= 30 Then
                                    MessageBox.Show("Your password has expired. Please change it now.", "Security Policy")
                                    ' TODO: Redirect to ForceChangePassword Form
                                Else
                                    ' Update LastLogin Date
                                    UpdateLastLogin(inputUser)

                                    MessageBox.Show("Welcome back, Admin.", "Success")
                                    MainForm.Show()
                                    Me.Hide()
                                End If
                            Else
                                MessageBox.Show("Invalid Password.", "Error")
                            End If
                        Else
                            MessageBox.Show("User not found.", "Error")
                        End If
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show("Login Error: " & ex.Message)
            End Try
        End Using
    End Sub

    ' ---------------------------------------------------------
    ' UPDATE LAST LOGIN HELPER
    ' ---------------------------------------------------------
    Private Sub UpdateLastLogin(username As String)
        ' UPDATE: Using UniversalConnString
        Using conn As New SqlConnection(ConnectionHelper.UniversalConnString)
            conn.Open()
            Dim cmd As New SqlCommand("UPDATE Users SET LastLogin = GETDATE() WHERE Uname = @u", conn)
            cmd.Parameters.AddWithValue("@u", username)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    ' ---------------------------------------------------------
    ' FIRST RUN CHECK (Create Default Admin)
    ' ---------------------------------------------------------
    Private Sub CheckAndCreateDefaultUser()
        ' UPDATE: Using UniversalConnString
        Using conn As New SqlConnection(ConnectionHelper.UniversalConnString)
            Try
                conn.Open()
                ' Check if table is empty
                Dim checkCmd As New SqlCommand("SELECT COUNT(*) FROM Users", conn)
                Dim count As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())

                If count = 0 Then
                    ' 1. Define Default Credentials
                    Dim defUser As String = "ADMIN"
                    Dim defPass As String = "ADMIN123"
                    Dim defEmail As String = "admin@tubiready.com"

                    ' 2. Generate Salt & Hash
                    Dim salt As String = CreateRandomSalt()
                    Dim hash As String = ComputeHash(defPass, salt)

                    ' 3. Insert into YOUR schema
                    Dim query As String = "INSERT INTO Users (Uname, PasswordHash, PasswordSalt, Email, CreatedOn, LastPassChange) " &
                                          "VALUES (@uname, @hash, @salt, @email, GETDATE(), GETDATE())"

                    Using cmd As New SqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@uname", defUser)
                        cmd.Parameters.AddWithValue("@hash", hash)
                        cmd.Parameters.AddWithValue("@salt", salt)
                        cmd.Parameters.AddWithValue("@email", defEmail)
                        cmd.ExecuteNonQuery()
                    End Using

                    MessageBox.Show("First Run Detected: Default Admin Created." & vbCrLf &
                                    "User: admin" & vbCrLf & "Pass: admin123", "Setup Complete")
                End If
            Catch ex As Exception
                MessageBox.Show("Setup Error: " & ex.Message)
            End Try
        End Using
    End Sub

    Private Sub LoginForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CheckAndCreateDefaultUser()
    End Sub

End Class