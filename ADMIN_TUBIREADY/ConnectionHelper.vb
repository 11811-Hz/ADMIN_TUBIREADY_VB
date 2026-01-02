Imports Microsoft.Data.SqlClient

Module ConnectionHelper
    ' GLOBAL VARIABLE: Use this throughout your app instead of hardcoding
    Public UniversalConnString As String = GetUniversalConnString()

    Public Function GetUniversalConnString() As String
        ' 1. Define the two most common connection strings
        Dim connStrExpress As String = "Data Source=.\SQLEXPRESS;Initial Catalog=TubiReadyDB;Integrated Security=True;TrustServerCertificate=True"
        Dim connStrDefault As String = "Data Source=.;Initial Catalog=TubiReadyDB;Integrated Security=True;TrustServerCertificate=True"

        ' 2. Test the Express instance first (Your machine)
        If CanConnect(connStrExpress) Then
            Debug.WriteLine("Connected using SQLEXPRESS instance.")
            Return connStrExpress
        End If

        ' 3. If that failed, test the Default instance (Partner's machine)
        If CanConnect(connStrDefault) Then
            Debug.WriteLine("Connected using Default instance.")
            Return connStrDefault
        End If

        ' 4. If both fail, return an empty string
        Return ""
    End Function

    ' Helper function to test connection quickly
    Private Function CanConnect(connStr As String) As Boolean
        Try
            Using conn As New Microsoft.Data.SqlClient.SqlConnection(connStr)
                ' FIXED: The class name is SqlConnectionStringBuilder (fully spelled out)
                Dim builder As New Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connStr)

                builder.ConnectTimeout = 2 ' Wait only 2 seconds

                ' FIXED: The property is .ConnectionString (fully spelled out)
                conn.ConnectionString = builder.ConnectionString

                conn.Open()
                Return True
            End Using
        Catch
            Return False
        End Try
    End Function
End Module