Imports System.Security.Cryptography
Imports System.Text

Module HashingModule
    Public Function ComputeSha256Hash(ByVal rawData As String) As String
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
    Public Function CreateRandomSalt() As String
        Dim mix As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+=][}{<>"
        Dim salt As New StringBuilder
        Dim rnd As New Random
        For i As Integer = 1 To 16 ' 16 character salt is standard
            salt.Append(mix.Substring(rnd.Next(mix.Length), 1))
        Next
        Return salt.ToString()
    End Function

    ' HELPER 2: Hash Password + Salt combined
    Public Function ComputeHash(ByVal plainText As String, ByVal salt As String) As String
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

End Module
