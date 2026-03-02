Imports Microsoft.Data.SqlClient

Module ConnectionHelper

    Private _universalConnString As String = Nothing

    Public ReadOnly Property UniversalConnString As String
        Get
            If String.IsNullOrWhiteSpace(_universalConnString) Then
                _universalConnString = ResolveConnectionString()
            End If

            Return _universalConnString
        End Get
    End Property

    Private Function ResolveConnectionString() As String
        Dim connStrExpress As String =
            "Data Source=.\SQLEXPRESS;Initial Catalog=TubiReadyDB;Integrated Security=True;TrustServerCertificate=True"

        Dim connStrDefault As String =
            "Data Source=.;Initial Catalog=TubiReadyDB;Integrated Security=True;TrustServerCertificate=True"

        If CanConnect(connStrExpress) Then
            Debug.WriteLine("Connected using SQLEXPRESS.")
            Return connStrExpress
        End If

        If CanConnect(connStrDefault) Then
            Debug.WriteLine("Connected using Default SQL instance.")
            Return connStrDefault
        End If

        Throw New InvalidOperationException(
            "No valid SQL Server instance found on this machine."
        )
    End Function

    Private Function CanConnect(connStr As String) As Boolean
        Try
            Using conn As New SqlConnection(connStr)
                Dim builder As New SqlConnectionStringBuilder(connStr)
                builder.ConnectTimeout = 2
                conn.ConnectionString = builder.ConnectionString
                conn.Open()
                Return True
            End Using
        Catch
            Return False
        End Try
    End Function

End Module
