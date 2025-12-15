Imports System.Data.SqlClient
Imports Guna.Charts.WinForms
Imports Microsoft.Data.SqlClient

Module ChartHandler
    ' 1. Centralized Connection String (Change once, updates everywhere)
    Public ReadOnly ConnString As String = "Data Source=DESKTOP-RT61FIB\SQLEXPRESS;Initial Catalog=TubiReadyDB;Integrated Security=True;TrustServerCertificate=True"

    ''' <summary>
    ''' Feeds data from SQL Server into a Guna Chart Dataset.
    ''' </summary>
    ''' <param name="targetChart">The GunaChart control to refresh.</param>
    ''' <param name="targetDataset">The dataset variable (e.g. MyAreaDataset).</param>
    ''' <param name="query">The SQL query (must return Date column first, Number column second).</param>
    ''' <param name="colTime">Name of the Time column in DB.</param>
    ''' <param name="colValue">Name of the Value column in DB.</param>
    Public Sub FeedChart(targetChart As GunaChart, targetDataset As GunaAreaDataset, query As String, colTime As String, colValue As String)

        Using conn As New SqlConnection(ConnString)
            Dim cmd As New SqlCommand(query, conn)
            Try
                conn.Open()
                Dim reader As SqlDataReader = cmd.ExecuteReader()

                If reader.HasRows Then
                    ' Clear old data to prevent duplicates
                    targetDataset.DataPoints.Clear()

                    While reader.Read()
                        ' 1. Generic Date Handling
                        ' We check if the column exists and is not null
                        If Not IsDBNull(reader(colTime)) AndAlso Not IsDBNull(reader(colValue)) Then

                            ' Convert DB Time to String
                            Dim rawDate As DateTime = Convert.ToDateTime(reader(colTime))
                            Dim timeString As String = rawDate.ToString("HH:mm:ss")

                            ' Convert DB Value to Double
                            Dim level As Double = Convert.ToDouble(reader(colValue))

                            ' Add to Dataset
                            targetDataset.DataPoints.Add(timeString, level)
                        End If
                    End While

                    ' Update the specific chart passed to this function
                    targetChart.Update()
                End If

            Catch ex As Exception
                ' Optional: Log error to console so it doesn't annoy the user with popups
                Console.WriteLine("Chart Error: " & ex.Message)
            End Try
        End Using
    End Sub
End Module
