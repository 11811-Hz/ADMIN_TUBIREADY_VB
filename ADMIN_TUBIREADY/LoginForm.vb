Imports Microsoft.Data.SqlClient
Imports System.Security.Cryptography
Imports System.Text

Public Class LoginForm
    Dim ConnString As String = "Data Source=DESKTOP-RT61FIB\SQLEXPRESS;Initial Catalog=TubiReadyDB;Integrated Security=True;TrustServerCertificate=True"
    Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        Dim inputUser As String = txtUsername.Text
        Dim inputPass As String = txtPassword.Text

        Using conn As New SqlConnection(ConnString)
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
                                    ' Redirect to ForceChangePassword Form
                                Else
                                    ' Update LastLogin Date (Optional but good for auditing)
                                    UpdateLastLogin(inputUser)

                                    MessageBox.Show("Welcome back, Admin.", "Success")
                                    MainForm.Show()
                                    Me.Hide()
                                    ' Redirect to Dashboard
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

    Private Sub UpdateLastLogin(username As String)
        Using conn As New SqlConnection(ConnString)
            conn.Open()
            Dim cmd As New SqlCommand("UPDATE Users SET LastLogin = GETDATE() WHERE Uname = @u", conn)
            cmd.Parameters.AddWithValue("@u", username)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Private Function ComputeSha256Hash(ByVal rawData As String) As String
        Using sha256Hash As SHA256 = SHA256.Create()
            Dim bytes As Byte() = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData))
            Dim builder As New StringBuilder()
            For i As Integer = 0 To bytes.Length - 1
                builder.Append(bytes(i).ToString("x2"))
            Next
            Return builder.ToString()
        End Using
    End Function

    ' HELPER 1: Generate a random salt
    Private Function CreateRandomSalt() As String
        Dim mix As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+=][}{<>"
        Dim salt As New StringBuilder
        Dim rnd As New Random
        For i As Integer = 1 To 16 ' 16 character salt is standard
            salt.Append(mix.Substring(rnd.Next(mix.Length), 1))
        Next
        Return salt.ToString()
    End Function

    ' HELPER 2: Hash Password + Salt combined
    Private Function ComputeHash(ByVal plainText As String, ByVal salt As String) As String
        Dim rawData As String = plainText & salt ' Combine them
        Using sha256 As SHA256 = SHA256.Create()
            Dim bytes As Byte() = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData))
            Dim builder As New StringBuilder()
            For i As Integer = 0 To bytes.Length - 1
                builder.Append(bytes(i).ToString("x2"))
            Next
            Return builder.ToString()
        End Using
    End Function

    Private Sub CheckAndCreateDefaultUser()
        Using conn As New SqlConnection(ConnString)
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