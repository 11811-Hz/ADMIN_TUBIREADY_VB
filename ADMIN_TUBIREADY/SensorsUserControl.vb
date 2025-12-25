Imports System.Data.SqlClient
Imports System.Net
Imports System.Net.Http
Imports System.Timers
Imports ADMIN_TUBIREADY.ChartHandler
Imports Microsoft.Data.SqlClient
Imports Guna.UI2.WinForms

Public Class SensorsUserControl

    Private connectionString As String = "server=DESKTOP-011N7DN;user id=TubiReadyAdmin;password=123456789;database=TubiReadyDB;TrustServerCertificate=True"

    Private sensorTimer As System.Timers.Timer
    Private receiverIP As String = "10.148.172.199" ' Receiver IP
    Private http As New HttpClient()

    Private CurrentStation As String = "Alley18"

    ' Datasets
    Private AlleyDataset As New Guna.Charts.WinForms.GunaAreaDataset()
    Private EntryDataset As New Guna.Charts.WinForms.GunaAreaDataset()

    Private Sub SensorsUserControl_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' 1. Configure Datasets
        AlleyDataset.Label = "Alley 18 Water Level"
        AlleyDataset.FillColor = Color.FromArgb(100, 50, 150, 255)
        AlleyDataset.BorderColor = Color.FromArgb(0, 120, 215)

        EntryDataset.Label = "Entry 1 Water Level"
        EntryDataset.FillColor = Color.FromArgb(100, 46, 204, 113)
        EntryDataset.BorderColor = Color.FromArgb(39, 174, 96)

        ' 2. --- SHADOW CONFIGURATION ---
        ' REMOVED the line causing error. Default rectangular shadow is automatic.
        pnlAlleyCard.ShadowDecoration.Depth = 45
        pnlEntryCard.ShadowDecoration.Depth = 45

        ' 3. Initial Loads
        SwitchToAlley18()
        UpdateChart()

        ' 4. Timers
        Timer1.Interval = 2000
        Timer1.Start()

        sensorTimer = New System.Timers.Timer(3000)
        AddHandler sensorTimer.Elapsed, AddressOf UpdateSensorData
        sensorTimer.Start()

        lblDateTime.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy — hh:mm tt")
    End Sub

    Private Sub SwitchToAlley18()
        CurrentStation = "Alley18"

        ' Using ARGB: Alpha 220 (out of 255) makes it much less transparent (darker).
        ' Using slightly darker RGB values than standard blue.
        pnlAlleyCard.ShadowDecoration.Color = Color.FromArgb(220, 0, 80, 200)
        pnlAlleyCard.ShadowDecoration.Enabled = True

        ' Disable Entry Shadow
        pnlEntryCard.ShadowDecoration.Enabled = False

        ' Update Chart & Grid (Sensor ID 1)
        UpdateChart()
        LoadRiverData(1)
    End Sub

    Private Sub SwitchToEntry1()
        CurrentStation = "Entry1"

        ' Disable Alley Shadow
        pnlAlleyCard.ShadowDecoration.Enabled = False

        ' Using Alpha 220 and darker green RGB values.
        pnlEntryCard.ShadowDecoration.Color = Color.FromArgb(220, 0, 180, 60)
        pnlEntryCard.ShadowDecoration.Enabled = True

        ' Update Chart & Grid (Sensor ID 2)
        UpdateChart()
        LoadRiverData(2)
    End Sub

    ' Updated LoadRiverData to accept a SensorID filter
    Private Sub LoadRiverData(targetSensorID As Integer)
        ' Using the Network IP connection string
        Dim query As String = "SELECT TOP 15 ReadingTime, WaterLevel, Severity " &
                              "FROM ultrasonic " &
                              "WHERE Sensor_ID = @SensorID " &
                              "ORDER BY ReadingTime DESC"

        Using conn As New SqlConnection(connectionString)
            Try
                conn.Open()
                Dim cmd As New SqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@SensorID", targetSensorID)

                Dim da As New SqlDataAdapter(cmd)
                Dim dt As New DataTable()
                da.Fill(dt)

                dgvRiverActivityHistory.AutoGenerateColumns = False
                dgvRiverActivityHistory.DataSource = dt
            Catch ex As Exception
                ' Optional: Log error silently or show message
                ' MessageBox.Show("Error loading history: " & ex.Message)
            End Try
        End Using
    End Sub

    Private Sub UpdateChart()
        ' Sensor_ID 1 is Alley, 2 is Entry.
        Dim queryAlley As String = "
        SELECT *
        FROM (
            SELECT TOP 10 ReadingTime, WaterLevel
            FROM dbo.Ultrasonic
            WHERE Sensor_ID = 1 
            ORDER BY ReadingTime DESC
        ) AS t
        ORDER BY ReadingTime ASC"

        Dim queryEntry As String = "
        SELECT *
        FROM (
            SELECT TOP 10 ReadingTime, WaterLevel
            FROM dbo.Ultrasonic
            WHERE Sensor_ID = 2
            ORDER BY ReadingTime DESC
        ) AS t
        ORDER BY ReadingTime ASC"

        Try
            If CurrentStation = "Alley18" Then
                GunaChartLvlWater.Title.Text = "Alley 18 Real-time Level"
                If Not GunaChartLvlWater.Datasets.Contains(AlleyDataset) Then
                    GunaChartLvlWater.Datasets.Clear()
                    GunaChartLvlWater.Datasets.Add(AlleyDataset)
                End If
                ChartHandler.FeedChart(GunaChartLvlWater, AlleyDataset, queryAlley, "ReadingTime", "WaterLevel")

            ElseIf CurrentStation = "Entry1" Then
                GunaChartLvlWater.Title.Text = "Entry 1 Real-time Level"
                If Not GunaChartLvlWater.Datasets.Contains(EntryDataset) Then
                    GunaChartLvlWater.Datasets.Clear()
                    GunaChartLvlWater.Datasets.Add(EntryDataset)
                End If
                ChartHandler.FeedChart(GunaChartLvlWater, EntryDataset, queryEntry, "ReadingTime", "WaterLevel")
            End If

        Catch ex As Exception
            Debug.WriteLine("UpdateChart Error: " & ex.Message)
        Finally
            GunaChartLvlWater.Update()
        End Try
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        UpdateChart()
    End Sub

    Private Async Sub UpdateSensorData(source As Object, e As ElapsedEventArgs)
        Try
            Dim temp As String = Await GetDataAsync("temp")
            Dim humid As String = Await GetDataAsync("humidity")
            If Me.IsHandleCreated Then
                Me.BeginInvoke(Sub()
                                   lblTemp.Text = temp & "°C"
                                   lblHumid.Text = humid & "%"
                               End Sub)
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Async Function GetDataAsync(endpoint As String) As Task(Of String)
        Dim url As String = $"http://{receiverIP}/{endpoint}"
        Dim response As HttpResponseMessage = Await http.GetAsync(url)
        response.EnsureSuccessStatusCode()
        Return (Await response.Content.ReadAsStringAsync()).Trim()
    End Function

    Private Sub updateTimer_Tick(sender As Object, e As EventArgs) Handles updateTimer.Tick
        lblDateTime.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy — hh:mm tt")
    End Sub

    Private Sub Guna2Panel5_Click(sender As Object, e As EventArgs) Handles pnlAlleyCard.Click
        SwitchToAlley18()
    End Sub

    Private Sub Guna2Panel10_Click(sender As Object, e As EventArgs) Handles pnlEntryCard.Click
        SwitchToEntry1()
    End Sub
End Class