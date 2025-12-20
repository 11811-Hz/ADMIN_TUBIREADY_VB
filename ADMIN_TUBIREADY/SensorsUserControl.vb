Imports System.Data.SqlClient
Imports System.Net
Imports System.Net.Http
Imports System.Timers
Imports ADMIN_TUBIREADY.ChartHandler
Imports Microsoft.Data.SqlClient

Public Class SensorsUserControl

    Private connectionString As String = "server=DESKTOP-011N7DN;user id=TubiReadyAdmin;password=123456789;database=TubiReadyDB;TrustServerCertificate=True"

    Private sensorTimer As System.Timers.Timer
    Private receiverIP As String = "10.148.172.199" ' Your Receiver IP
    Private http As New HttpClient()

    Private CurrentStation As String = "Alley18"

    ' Keep two styled datasets ready to go (consistent names)
    Private AlleyDataset As New Guna.Charts.WinForms.GunaAreaDataset()
    Private EntryDataset As New Guna.Charts.WinForms.GunaAreaDataset()


    Private Sub SensorsUserControl_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        UpdateChart()
        LoadRiverData()
        ' 1. Configure Alley Dataset (Blue style)
        AlleyDataset.Label = "Alley 18 Water Level"
        AlleyDataset.FillColor = Color.FromArgb(100, 50, 150, 255)
        AlleyDataset.BorderColor = Color.FromArgb(0, 120, 215)

        ' 2. Configure Entry Dataset (Green style to match your UI icons)
        EntryDataset.Label = "Entry 1 Water Level"
        EntryDataset.FillColor = Color.FromArgb(100, 46, 204, 113)
        EntryDataset.BorderColor = Color.FromArgb(39, 174, 96)

        ' 3. Load default (Alley 18)
        SwitchToAlley18()

        Timer1.Interval = 2000
        Timer1.Start()

        ' Auto update every 3 seconds
        sensorTimer = New System.Timers.Timer(3000)
        AddHandler sensorTimer.Elapsed, AddressOf UpdateSensorData
        sensorTimer.Start()

        ' ts is for the label, this does not need to be changed so don't remove it plz. if you do, i will find you.
        lblDateTime.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy — hh:mm tt")
    End Sub

    Private Sub LoadRiverData()
        ' REPLACE THIS with your actual connection string
        Dim connectionString As String = "Data Source=DESKTOP-011N7DN;user id=TubiReadyAdmin;password=123456789;database=TubiReadyDB;TrustServerCertificate=True"

        ' This query selects the columns matching your UI
        Dim query As String = "SELECT TOP 15 ReadingTime, WaterLevel, Severity FROM ultrasonic ORDER BY ReadingTime DESC"

        Using conn As New SqlConnection(connectionString)
            Try
                conn.Open()

                Dim da As New SqlDataAdapter(query, conn)
                Dim dt As New DataTable()
                da.Fill(dt)

                ' PREVENT DUPLICATES: 
                ' Since you already created columns in the designer, turn off auto-generation
                dgvRiverActivityHistory.AutoGenerateColumns = False

                ' Bind the data
                dgvRiverActivityHistory.DataSource = dt

            Catch ex As Exception
                MessageBox.Show("Error loading data: " & ex.Message)
            End Try
        End Using
    End Sub

    Private Sub SwitchToAlley18()
        CurrentStation = "Alley18"

        ' Swap the chart dataset
        GunaChartLvlWater.Datasets.Clear()
        GunaChartLvlWater.Datasets.Add(AlleyDataset)

        ' Optional: Highlight the card (assuming your panels are named GunaPanelAlley and GunaPanelEntry)
        ' GunaPanelAlley.BorderColor = Color.Blue
        ' GunaPanelEntry.BorderColor = Color.Transparent

        ' Force immediate update
        UpdateChart()
    End Sub

    Private Sub SwitchToEntry1()
        CurrentStation = "Entry1"

        ' Swap the chart dataset
        GunaChartLvlWater.Datasets.Clear()
        GunaChartLvlWater.Datasets.Add(EntryDataset)

        ' GunaPanelAlley.BorderColor = Color.Transparent
        ' GunaPanelEntry.BorderColor = Color.Green

        ' Force immediate update
        UpdateChart()
    End Sub

    Private Sub UpdateChart()

        Dim query As String =
    "SELECT ReadingTime, WaterLevel
     FROM (
        SELECT TOP 20 ReadingTime, WaterLevel
        FROM dbo.Ultrasonic
        ORDER BY ReadingTime DESC
     ) t
     ORDER BY ReadingTime ASC"

        Try
            ' Clear previous points to avoid stacking
            AlleyDataset.DataPoints.Clear()

            Using conn As New SqlConnection(connectionString)
                conn.Open()
                Using cmd As New SqlCommand(query, conn)
                    Using reader As SqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            AlleyDataset.DataPoints.Add(
                            reader("ReadingTime").ToString(),
                            Convert.ToDouble(reader("WaterLevel"))
                        )
                        End While
                    End Using
                End Using
            End Using

            ' Refresh chart
            GunaChartLvlWater.Datasets.Clear()
            GunaChartLvlWater.Datasets.Add(AlleyDataset)
            GunaChartLvlWater.Update()

        Catch ex As Exception
            Debug.WriteLine("Chart Update Error: " & ex.Message)
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
            If Me.IsHandleCreated Then
                Me.BeginInvoke(Sub()
                                   lblTemp.Text = "ERR"
                                   lblHumid.Text = "ERR"
                               End Sub)
            End If
        End Try
    End Sub

    Private Async Function GetDataAsync(endpoint As String) As Task(Of String)
        Dim url As String = $"http://{receiverIP}/{endpoint}"
        Dim response As HttpResponseMessage = Await http.GetAsync(url)
        response.EnsureSuccessStatusCode()

        Dim content As String = Await response.Content.ReadAsStringAsync()
        Return content.Trim()
    End Function

    Private Sub updateTimer_Tick(sender As Object, e As EventArgs) Handles updateTimer.Tick
        lblDateTime.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy — hh:mm tt")
    End Sub

    Private Sub Guna2Panel5_Click(sender As Object, e As EventArgs) Handles Guna2Panel5.Click
        SwitchToAlley18()
        UpdateChart()
    End Sub

    Private Sub Guna2Panel10_Click(sender As Object, e As EventArgs) Handles Guna2Panel10.Click
        SwitchToEntry1()
        UpdateChart()
    End Sub
End Class