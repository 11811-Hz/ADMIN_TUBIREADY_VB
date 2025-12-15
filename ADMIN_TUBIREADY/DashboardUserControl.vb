Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Linq
Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Guna.Charts.Interfaces
Imports Guna.Charts.WinForms ' Required for Guna Charts
Imports Microsoft.Data.SqlClient
Imports ADMIN_TUBIREADY.ChartHandler


Public Class DashboardUserControl

    ' 1. DEFINE THE DATASET MANUALLY HERE
    Private MyAreaDataset As New Guna.Charts.WinForms.GunaAreaDataset()

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' 2. CONFIGURE AND LINK IT ON LOAD

        ' Give it a name for the legend
        MyAreaDataset.Label = "Water Level"

        ' Optional: Style it (Blue color)
        MyAreaDataset.FillColor = Color.FromArgb(50, 0, 120, 215)
        MyAreaDataset.BorderColor = Color.FromArgb(0, 120, 215)

        ' 3. CRITICAL: Add it to the Chart manually
        GunaChart1.Datasets.Add(MyAreaDataset)

        ' Start your timer here if you haven't already
        Timer1.Start()
    End Sub


    Private connectionString As String = "server=10.148.172.193\SQLEXPRESS,1433;user id=TubiReadyAdmin;password=123456789;database=TubiReadyDB;TrustServerCertificate=True;"

    ' === NEW: 5-minute database interval ===
    Private lastSaveTime As DateTime = DateTime.MinValue
    Private saveInterval As TimeSpan = TimeSpan.FromMinutes(5)

    ' Add a shared HttpClient (reuse to avoid socket exhaustion)
    Private Shared ReadOnly httpClient As New HttpClient() With {
        .Timeout = TimeSpan.FromSeconds(2)
    }

    ' ---------------- SENSOR CLASS (simulation only for status/signal) ----------------
    Public Class SensorSimulation
        Public Property Name As String
        Public Property IsActive As Boolean = True
        Public Property Signal As String = "Strong"
        Public Property Battery As Integer = 100

        Private rand As New Random()

        Public Sub Tick()
            ' Random active/inactive
            If rand.Next(0, 100) < 3 Then
                IsActive = Not IsActive
            End If

            ' Battery drift
            Battery += rand.Next(-1, 2)
            Battery = Math.Max(5, Math.Min(100, Battery))

            ' Signal variations
            Dim s = rand.Next(0, 100)
            If s > 75 Then
                Signal = "Strong"
            ElseIf s > 50 Then
                Signal = "Medium"
            ElseIf s > 25 Then
                Signal = "Weak"
            Else
                Signal = "Low"
            End If
        End Sub
    End Class

    Private Sub DashboardUserControl_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Setup Alley 18
        Alley18Dataset.Label = "Alley 18 Level"
        Alley18Dataset.FillColor = Color.FromArgb(100, 50, 150, 255)
        GunaChart1.Datasets.Clear()
        GunaChart1.Datasets.Add(Alley18Dataset)

        ' Setup Entry 1 (If you have a second chart, e.g. GunaChart2)
        ' Entry1Dataset.Label = "Entry 1 Level"
        ' Entry1Dataset.FillColor = Color.Red
        ' GunaChart2.Datasets.Clear()
        ' GunaChart2.Datasets.Add(Entry1Dataset)

        Timer1.Interval = 2000
        Timer1.Start()
    End Sub

    ' new line for database 

    Private Sub SaveWaterLevelToDatabase(waterLevel As Double, severity As String)
        Try
            Using conn As New SqlConnection(connectionString)
                conn.Open()

                Dim query As String =
                "INSERT INTO Ultrasonic (ReadingTime, WaterLevel, Severity, Is_Synced) 
                 VALUES (@ReadingTime, @WaterLevel, @Severity, @Is_Synced)"

                Using cmd As New SqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@ReadingTime", DateTime.Now)
                    cmd.Parameters.AddWithValue("@WaterLevel", waterLevel)
                    cmd.Parameters.AddWithValue("@Severity", severity)
                    cmd.Parameters.AddWithValue("@Is_Synced", "false")

                    cmd.ExecuteNonQuery()
                End Using
            End Using

            Console.WriteLine("Water level saved to SQL Server.")

        Catch ex As Exception
            MessageBox.Show("Database error: " & ex.Message)
        End Try
    End Sub

    ' ---------------- TIMER LOOP ----------------
    ' Make the Tick handler async and await the network call
    Private savingInProgress As Boolean = False

    Private Async Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        ' ---------------------------------------------------------
        ' ONE LINE OF CODE PER CHART NOW!
        ' ---------------------------------------------------------

        ' 1. Update Alley 18
        Dim query1 As String = "SELECT TOP 10 ReadingTime, WaterLevel FROM dbo.Ultrasonic ORDER BY ReadingTime DESC"
        ChartHandler.FeedChart(GunaChart1, Alley18Dataset, query1, "ReadingTime", "WaterLevel")

        ' 2. Update Entry 1 (Example for when you are ready)
        ' Dim query2 As String = "SELECT TOP 10 ReadingTime, WaterLevel FROM dbo.Ultrasonic WHERE Station='Entry1' ORDER BY ReadingTime DESC"
        ' ChartHandler.FeedChart(GunaChart2, Entry1Dataset, query2, "ReadingTime", "WaterLevel")
        ' ---------------------------------------------------------
        If savingInProgress Then Return
        savingInProgress = True
        Try
            For Each s In sensors : s.Tick() : Next
            Dim waterTuple = Await GetWaterDataFromReceiverAsync()
            If (DateTime.Now - lastSaveTime) >= saveInterval Then
                ' Run DB save off the UI thread and avoid modal dialogs there
                Await Task.Run(Sub() SaveWaterLevelToDatabase(waterTuple.Item1, waterTuple.Item2))
                lastSaveTime = DateTime.Now
            End If
            UpdateDashboardWithWater(waterTuple.Item1, waterTuple.Item2)
        Finally
            savingInProgress = False
        End Try
    End Sub

    ' ---------------- MAIN DASHBOARD UPDATE ----------------
    ' small refactor: UpdateDashboard that accepts water result (keeps UI updates on UI thread)
    Private Sub UpdateDashboardWithWater(level As Double, status As String)
        ' use Where().Count() to avoid ambiguity with Count overloads
        Dim activeCount As Integer = sensors.Where(Function(s) s.IsActive).Count()
        Dim offlineCount As Integer = sensors.Count - activeCount

        lbl_sensors_active.Text = activeCount.ToString()
        lbl_sensors_offline.Text = offlineCount.ToString()
        lblTimestamp.Text = "As of " & DateTime.Now.ToString("h:mm tt, MMMM d, yyyy")

        Dim meter As Double = level / 1000.0
        lbl_waterlevel.Text = meter.ToString("0.00")
        lblOverallWaterStatus.Text = status
        lblOverallWaterStatus.ForeColor =
            If(status = "SAFE", Color.Green, If(status = "WARNING", Color.Orange, Color.Red))


        ' ---------------- Dummy population ----------------
        ' Implement cap on "safe". Stop population simulation when cap is reached.
        Const SAFE_CAP As Integer = 100
        Static safe As Integer = 0
        Static unsafe As Integer = 100
        Static populationSimulationStopped As Boolean = False

        If Not populationSimulationStopped Then
            If safe < SAFE_CAP Then
                safe += 1
                unsafe -= 1
                If unsafe < 0 Then unsafe = 0

                If safe >= SAFE_CAP Then
                    populationSimulationStopped = True
                End If
            End If
        End If

        lbl_safe_residents.Text = safe.ToString()
        lbl_unsafe_residents.Text = unsafe.ToString()

        ' ---------------- Update sensor cards ----------------
        ' Guard against missing sensors
        If sensors.Count > 0 Then
            UpdateSensorCard(sensors(0), lblS1Status, lblS1Signal, lblS1Battery, dotS1)
        End If
        If sensors.Count > 1 Then
            UpdateSensorCard(sensors(1), lblS2Status, lblS2Signal, lblS2Battery, dotS2)
        End If
    End Sub

    ' ---------------- DOT RENDERING ----------------
    Private dotPaintAttached As New HashSet(Of Control)()

    ' Helper to safely marshal actions to UI and guard against disposed controls
    Private Sub SafeInvoke(ctrl As Control, action As Action)
        If ctrl Is Nothing Then Return
        Try
            If ctrl.IsDisposed OrElse ctrl.Disposing Then Return
            If ctrl.InvokeRequired Then
                ctrl.Invoke(action)
            Else
                action()
            End If
        Catch
            ' If invocation fails (control is disposed mid-call), swallow to avoid crash.
        End Try
    End Sub

    Private Sub SetDotFillColor(dotControl As Control, fillColor As Color)
        ' Defensive: ensure control exists and not disposed
        If dotControl Is Nothing Then Return
        If dotControl.IsDisposed OrElse dotControl.Disposing Then Return

        ' All UI updates must run on the control's UI thread
        SafeInvoke(dotControl, Sub()
                                   'Store fill color in Tag property
                                   dotControl.Tag = fillColor

                                   If Not dotPaintAttached.Contains(dotControl) Then
                                       Try
                                           AddHandler dotControl.Paint, AddressOf DotControl_Paint
                                           dotPaintAttached.Add(dotControl)
                                       Catch
                                           ' In rare cases handler attach can fail if control is disposing; ignore.
                                       End Try

                                       Try
                                           ' Set dot background to parent's background if available
                                           If dotControl.Parent IsNot Nothing Then
                                               dotControl.BackColor = dotControl.Parent.BackColor
                                           End If
                                       Catch
                                           ' ignore parent access errors
                                       End Try
                                   End If

                                   ' Invalidate to force repaint if it's safe
                                   Try
                                       If Not dotControl.IsDisposed AndAlso dotControl.IsHandleCreated Then
                                           dotControl.Invalidate()
                                       End If
                                   Catch
                                       ' ignore invalidation errors
                                   End Try
                               End Sub)
    End Sub

    Private Sub DotControl_Paint(sender As Object, e As PaintEventArgs)
        'This function is literally just drawing a colored circle since Control.fillColor() doesn't work on controls fym bro
        Dim c = TryCast(sender, Control)
        If c Is Nothing Then Return
        If c.IsDisposed OrElse c.Disposing Then Return

        Dim fill As Color = If(TypeOf c.Tag Is Color, CType(c.Tag, Color), c.BackColor)

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias

        Dim rect = c.ClientRectangle
        rect.Inflate(-2, -2)

        If rect.Width <= 0 Or rect.Height <= 0 Then Return

        Using brush As New SolidBrush(fill)
            e.Graphics.FillEllipse(brush, rect)
        End Using

        Using pen As New Pen(Color.FromArgb(100, Color.Black), 1)
            e.Graphics.DrawEllipse(pen, rect)
        End Using
    End Sub

    ' ---------------- SENSOR CARD UPDATE ----------------
    Private Sub UpdateSensorCard(s As SensorSimulation,
                                 lblStatus As Label,
                                 lblSignal As Label,
                                 lblBattery As Label,
                                 dotControl As Control)

        ' Guard against labels being Nothing or disposed
        If lblStatus IsNot Nothing AndAlso Not lblStatus.IsDisposed Then
            SafeInvoke(lblStatus, Sub() lblStatus.Text = If(s.IsActive, "Active", "Offline"))
        End If
        If lblSignal IsNot Nothing AndAlso Not lblSignal.IsDisposed Then
            SafeInvoke(lblSignal, Sub() lblSignal.Text = s.Signal)
        End If
        If lblBattery IsNot Nothing AndAlso Not lblBattery.IsDisposed Then
            SafeInvoke(lblBattery, Sub() lblBattery.Text = s.Battery.ToString() & "%")
        End If

        ' Dot rendering is handled safely inside SetDotFillColor
        If dotControl IsNot Nothing Then
            SetDotFillColor(dotControl, If(s.IsActive, Color.LimeGreen, Color.Red))
        End If
    End Sub

    ' ---------------- SENSOR SIM LIST ----------------
    Private sensors As New List(Of SensorSimulation) From {
        New SensorSimulation With {.Name = "Alley 18 Station"},
        New SensorSimulation With {.Name = "Entry 1 (Upstream)"}
    }

    ' ---------------- SEND BUZZER COMMAND ----------------
    Private Async Function SendCommandToESPAsync(endpoint As String) As Task
        Try
            Dim baseUrl As String = "http://10.148.172.199"
            Dim url As String = If(endpoint.StartsWith("/"), baseUrl & endpoint, baseUrl & "/" & endpoint)
            Dim resp = Await httpClient.GetAsync(url)
            Dim body = Await resp.Content.ReadAsStringAsync()
            Console.WriteLine("ESP Response: " & body)
        Catch ex As Exception
            MessageBox.Show("Failed to send command to ESP32: " & ex.Message)
        End Try
    End Function

    Private buzzerState As Boolean = False

    Private Sub Guna2Button11_Click(sender As Object, e As EventArgs) Handles Guna2Button11.Click
        If Not buzzerState Then

            Task.Run(Async Function()
                         Await SendCommandToESPAsync("/buzzer_on")
                     End Function)

            Label4.Text = "Deactivate Buzzer"
            Guna2Button11.Text = "TURN BUZZER OFF"
            buzzerState = True
            Guna2Button11.FillColor = Color.Red
            MessageBox.Show("Siren Activated")

        Else

            Task.Run(Async Function()
                         Await SendCommandToESPAsync("/buzzer_off")
                     End Function)

            Label4.Text = "Activate Buzzer"
            Guna2Button11.Text = "TURN BUZZER ON"
            buzzerState = False
            Guna2Button11.FillColor = Color.White
            MessageBox.Show("Siren Deactivated")

        End If
    End Sub

    ' ---------------- GET WATER LEVEL FROM RECEIVER ----------------
    ' Return Tuple(Of Double, String) to maximize compatibility
    Private Async Function GetWaterDataFromReceiverAsync() As Task(Of Tuple(Of Double, String))
        Try
            Dim url As String = "http://10.148.172.199/waterlevel"
            Dim raw As String = (Await httpClient.GetStringAsync(url)).Trim()

            Dim level As Double = 0
            Dim status As String = "SAFE"

            Dim pos = raw.IndexOf("L:")
            If pos >= 0 Then
                Dim sb As New Text.StringBuilder()
                For i As Integer = pos + 2 To raw.Length - 1
                    If Char.IsDigit(raw(i)) Then
                        sb.Append(raw(i))
                    Else
                        Exit For
                    End If
                Next
                Double.TryParse(sb.ToString(), level)
            End If

            If level < 2000 Then
                status = "SAFE"
            ElseIf level < 4000 Then
                status = "WARNING"
            Else
                status = "DANGER"
            End If

            Return Tuple.Create(level, status)
        Catch ex As Exception
            Console.WriteLine("Receiver error: " & ex.Message)
            Return Tuple.Create(0.0, "UNKNOWN")
        End Try
    End Function

    Private Sub btnWaterLevelALley_Click(sender As Object, e As EventArgs)
        lblWaterLevel.Text = "ALLEY WATER LEVEL"
    End Sub

    Private Sub btnWaterLevelEntry_Click(sender As Object, e As EventArgs)
        lblWaterLevel.Text = "ENTRY WATER LEVEL"
    End Sub

    Private Sub Guna2vScrollBar1_Scroll(sender As Object, e As ScrollEventArgs)

    End Sub
End Class
