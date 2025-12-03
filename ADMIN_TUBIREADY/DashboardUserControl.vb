Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms
Imports System.Drawing.Drawing2D
Imports System.Net
Imports System.Text
Imports System.IO
Imports System.Net.Http
Imports System.Threading.Tasks

Public Class DashboardUserControl

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
    End Sub


    ' ---------------- TIMER LOOP ----------------
    ' Make the Tick handler async and await the network call
    Private Async Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        ' quick local updates
        For Each s In sensors
            s.Tick()
        Next

        ' do network I/O off the UI-blocking path
        Dim waterTuple = Await GetWaterDataFromReceiverAsync()

        ' then update UI (runs on UI context)
        UpdateDashboardWithWater(waterTuple.Item1, waterTuple.Item2)
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

    Private Sub SetDotFillColor(dotControl As Control, fillColor As Color)
        'Store fill color in Tag property
        dotControl.Tag = fillColor

        If Not dotPaintAttached.Contains(dotControl) Then
            AddHandler dotControl.Paint, AddressOf DotControl_Paint
            dotPaintAttached.Add(dotControl)

            Try
                dotControl.BackColor = dotControl.Parent.BackColor
            Catch
            End Try
        End If

        dotControl.Invalidate()
    End Sub

    Private Sub DotControl_Paint(sender As Object, e As PaintEventArgs)
        'This function is literally just drawing a colored circle since Control.fillColor() doesn't work on controls fym bro
        Dim c = TryCast(sender, Control)
        If c Is Nothing Then Return

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

        lblStatus.Text = If(s.IsActive, "Active", "Offline")
        lblSignal.Text = s.Signal
        lblBattery.Text = s.Battery.ToString() & "%"

        SetDotFillColor(dotControl, If(s.IsActive, Color.LimeGreen, Color.Red))
    End Sub



    ' ---------------- SENSOR SIM LIST ----------------
    Private sensors As New List(Of SensorSimulation) From {
        New SensorSimulation With {.Name = "Alley 18 Station"},
        New SensorSimulation With {.Name = "Entry 1 (Upstream)"}
    }



    ' ---------------- SEND BUZZER COMMAND ----------------
    Private Async Function SendCommandToESPAsync(endpoint As String) As Task
        Try
            Dim baseUrl As String = "http://10.237.203.199"
            Dim url As String = If(endpoint.StartsWith("/"), baseUrl & endpoint, baseUrl & "/" & endpoint)
            Dim resp = Await httpClient.GetAsync(url)
            Dim body = Await resp.Content.ReadAsStringAsync()
            Console.WriteLine("ESP Response: " & body)
        Catch ex As Exception
            MessageBox.Show("Failed to send command to ESP32: " & ex.Message)
        End Try
    End Function

    Private buzzerState As Boolean = False

    Private Async Sub Guna2Button11_Click(sender As Object, e As EventArgs) Handles Guna2Button11.Click
        If Not buzzerState Then
            Await SendCommandToESPAsync("/buzzer_on")
            Label4.Text = "Deactivate Buzzer"
            Guna2Button11.Text = "TURN BUZZER OFF"
            buzzerState = True
        Else
            Await SendCommandToESPAsync("/buzzer_off")
            Label4.Text = "Activate Buzzer"
            Guna2Button11.Text = "TURN BUZZER ON"
            buzzerState = False
        End If
    End Sub


    ' ---------------- GET WATER LEVEL FROM RECEIVER ----------------
    ' Return Tuple(Of Double, String) to maximize compatibility
    Private Async Function GetWaterDataFromReceiverAsync() As Task(Of Tuple(Of Double, String))
        Try
            Dim url As String = "http://10.237.203.199/waterlevel"
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

    Private Sub btnWaterLevelALley_Click(sender As Object, e As EventArgs) Handles btnWaterLevelAlley.Click
        lblWaterLevel.Text = "ALLEY WATER LEVEL"
    End Sub

    Private Sub btnWaterLevelEntry_Click(sender As Object, e As EventArgs) Handles btnWaterLevelEntry.Click
        lblWaterLevel.Text = "ENTRY WATER LEVEL"
    End Sub
End Class
