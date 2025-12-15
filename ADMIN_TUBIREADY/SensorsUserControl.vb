Imports System.Data.SqlClient
Imports System.Net
Imports System.Net.Http
Imports System.Timers
Imports ADMIN_TUBIREADY.ChartHandler
Imports Microsoft.Data.SqlClient

Public Class SensorsUserControl

    Private connectionString As String = "server=10.148.172.193\SQLEXPRESS,1433;user id=TubiReadyAdmin;password=123456789;database=TubiReadyDB;TrustServerCertificate=True;"

    Private sensorTimer As System.Timers.Timer
    Private receiverIP As String = "10.148.172.199" ' Your Receiver IP
    Private http As New HttpClient()

    Private CurrentStation As String = "Alley18"

    ' Keep two styled datasets ready to go
    Private AlleyDataset As New Guna.Charts.WinForms.GunaAreaDataset()
    Private EntryDataset As New Guna.Charts.WinForms.GunaAreaDataset()


    Private Sub SensorsUserControl_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        UpdateChart()
        LoadRiverData()
        ' 1. Configure Alley 18 Dataset (Blue style)
        Alley18Dataset.Label = "Alley 18 Water Level"
        Alley18Dataset.FillColor = Color.FromArgb(100, 50, 150, 255)
        Alley18Dataset.BorderColor = Color.FromArgb(0, 120, 215)

        ' 2. Configure Entry 1 Dataset (Green style to match your UI icons)
        Entry1Dataset.Label = "Entry 1 Water Level"
        Entry1Dataset.FillColor = Color.FromArgb(100, 46, 204, 113)
        Entry1Dataset.BorderColor = Color.FromArgb(39, 174, 96)

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
        Dim connectionString As String = "Data Source=DESKTOP-RT61FIB\SQLEXPRESS;Initial Catalog=TubiReadyDB;Integrated Security=True;TrustServerCertificate=True"

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
        GunaChart1.Datasets.Clear()
        GunaChart1.Datasets.Add(Alley18Dataset)

        ' Optional: Highlight the card (assuming your panels are named GunaPanelAlley and GunaPanelEntry)
        ' GunaPanelAlley.BorderColor = Color.Blue
        ' GunaPanelEntry.BorderColor = Color.Transparent

        ' Force immediate update
        UpdateChart()
    End Sub

    Private Sub SwitchToEntry1()
        CurrentStation = "Entry1"

        ' Swap the chart dataset
        GunaChart1.Datasets.Clear()
        GunaChart1.Datasets.Add(Entry1Dataset)

        ' GunaPanelAlley.BorderColor = Color.Transparent
        ' GunaPanelEntry.BorderColor = Color.Green

        ' Force immediate update
        UpdateChart()
    End Sub

    Private Sub UpdateChart()
        ' 1. DEFINE YOUR QUERIES
        ' We sort by ReadingTime DESC to get the newest, then TOP 10 to keep it readable

        ' Query for Alley 18
        ' REPLACE 'StationID' with your actual column name!
        Dim queryAlley As String = "SELECT TOP 10 ReadingTime, WaterLevel FROM dbo.Ultrasonic ORDER BY ReadingTime DESC"

        ' Query for Entry 1
        Dim queryEntry As String = "SELECT TOP 10 ReadingTime, WaterLevel FROM dbo.Ultrasonic ORDER BY ReadingTime DESC"

        ' 2. CHECK WHICH STATION IS ACTIVE AND FETCH DATA
        If CurrentStation = "Alley18" Then
            ' Use the Module to feed the Alley Dataset
            ChartHandler.FeedChart(GunaChart1, AlleyDataset, queryAlley, "ReadingTime", "WaterLevel")

        ElseIf CurrentStation = "Entry1" Then
            ' Use the Module to feed the Entry Dataset
            ChartHandler.FeedChart(GunaChart1, EntryDataset, queryEntry, "ReadingTime", "WaterLevel")
        End If

        ' 3. REFRESH THE CHART VISUALLY
        GunaChart1.Update()
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