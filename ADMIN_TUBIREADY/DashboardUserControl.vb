Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms
Imports System.Drawing.Drawing2D
Imports System.Net
Imports System.Text
Imports System.IO

Public Class DashboardUserControl

    Public Class SensorSimulation
        Public Property Name As String
        Public Property IsActive As Boolean = True
        Public Property Signal As String = "Strong"
        Public Property Battery As Integer = 100
        Public Property WaterLevel As Double = 1.0
        Public Property WaterStatus As String = "Safe" ' status computed from water levels
        ' Now represent sensor readings in feet (0.0 .. 10.0)
        Public Property s1Water As Double = 1.0
        Public Property s2Water As Double = 1.0

        Private rand As New Random()

        ' Simulate a time tick - no UI references here
        Public Sub Tick()

            ' Random failures
            If rand.Next(0, 100) < 3 Then
                IsActive = Not IsActive
            End If

            ' Battery drift
            Battery += rand.Next(-1, 2)
            Battery = Math.Max(5, Math.Min(100, Battery))

            ' Water level drift (base) - keep a small generic jitter
            WaterLevel += rand.NextDouble() * 0.2 - 0.1
            WaterLevel = Math.Max(0, Math.Round(WaterLevel, 2))

            ' Signal strength changes
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

            ' Simulate two sensor readings (in feet) with an upward bias:
            ' - 70% chance to increase by 0.0 .. 0.4 ft
            ' - 30% chance to decrease by 0.0 .. 0.1 ft (smaller decreases)
            Dim delta1 As Double
            If rand.NextDouble() < 0.7 Then
                delta1 = rand.NextDouble() * 0.4
            Else
                delta1 = -rand.NextDouble() * 0.1
            End If
            s1Water = Math.Max(0.0, Math.Min(10.0, Math.Round(s1Water + delta1, 2)))

            Dim delta2 As Double
            If rand.NextDouble() < 0.7 Then
                delta2 = rand.NextDouble() * 0.4
            Else
                delta2 = -rand.NextDouble() * 0.1
            End If
            s2Water = Math.Max(0.0, Math.Min(10.0, Math.Round(s2Water + delta2, 2)))

            ' Update WaterLevel to the average of the two simulated readings
            Dim avg As Double = Math.Round((s1Water + s2Water) / 2.0, 2)
            WaterLevel = avg

            ' Determine status based on avg (10ft scale)
            If avg < 2.5 Then
                WaterStatus = "Safe"
            ElseIf avg < 5.0 Then
                WaterStatus = "Dangerous"
            ElseIf avg < 7.5 Then
                WaterStatus = "Severe"
            Else
                WaterStatus = "Critical"
            End If
        End Sub
    End Class

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        ' Update each sensor's simulation state (no UI updates here)
        For Each s In sensors
            s.Tick()
        Next

        UpdateDashboard()
    End Sub

    Private Sub UpdateDashboard()

        ' -------- GLOBAL METRICS --------
        Dim activeCount = sensors.Where(Function(s) s.IsActive).Count()
        Dim offlineCount = sensors.Count - activeCount

        lbl_sensors_active.Text = activeCount.ToString()
        lbl_sensors_offline.Text = offlineCount.ToString()

        lblTimestamp.Text = "As of " & DateTime.Now.ToString("h:mm tt, MMMM d, yyyy")

        ' River water level (based on average WaterLevel across sensors)
        Dim avgWater = sensors.Average(Function(s) s.WaterLevel)
        ' Show one decimal place (feet)
        lbl_waterlevel.Text = avgWater.ToString("0.0")

        ' Compute overall water status from the averaged water level using 10ft thresholds
        Dim overallStatus As String
        If avgWater < 2.5 Then
            overallStatus = "Safe"
        ElseIf avgWater < 5.0 Then
            overallStatus = "Dangerous"
        ElseIf avgWater < 7.5 Then
            overallStatus = "Severe"
        Else
            overallStatus = "Critical"
        End If

        lblOverallWaterStatus.Text = overallStatus

        ' -------- SAFETY METRIC SIMULATION --------
        Static safe As Integer = 0
        Static unsafe As Integer = 30
        safe += 1
        unsafe -= 1
        If unsafe < 0 Then unsafe = 0

        lbl_safe_residents.Text = safe.ToString()
        lbl_unsafe_residents.Text = unsafe.ToString()

        ' -------- PER SENSOR CARDS --------
        UpdateSensorCard(sensors(0), lblS1Status, lblS1Signal, lblS1Battery, dotS1)
        UpdateSensorCard(sensors(1), lblS2Status, lblS2Signal, lblS2Battery, dotS2)

    End Sub

    Private dotPaintAttached As New HashSet(Of Control)()

    Private Sub SetDotFillColor(dotControl As Control, fillColor As Color)
        ' Store the desired fill color in Tag (or overwrite existing)
        dotControl.Tag = fillColor

        ' Attach a Paint handler once per control to draw a filled circle
        If Not dotPaintAttached.Contains(dotControl) Then
            AddHandler dotControl.Paint, AddressOf DotControl_Paint
            dotPaintAttached.Add(dotControl)
            ' Optional: make background match parent to reduce visible square
            Try
                dotControl.BackColor = dotControl.Parent.BackColor
            Catch
                ' ignore if Parent is Nothing or setting fails
            End Try
        End If

        dotControl.Invalidate()
    End Sub

    Private Sub DotControl_Paint(sender As Object, e As PaintEventArgs)
        Dim c = TryCast(sender, Control)
        If c Is Nothing Then Return

        ' Determine fill color stored in Tag; fallback to BackColor
        Dim fill As Color = c.BackColor
        If TypeOf c.Tag Is Color Then
            fill = CType(c.Tag, Color)
        End If

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias

        Dim rect = c.ClientRectangle
        Dim padding As Integer = 2
        rect.Inflate(-padding, -padding)

        ' Ensure a valid rectangle
        If rect.Width <= 0 Or rect.Height <= 0 Then Return

        Using brush As New SolidBrush(fill)
            e.Graphics.FillEllipse(brush, rect)
        End Using

        ' Optional: draw a subtle border for visibility
        Using pen As New Pen(Color.FromArgb(100, Color.Black), 1)
            e.Graphics.DrawEllipse(pen, rect)
        End Using
    End Sub

    Private Sub UpdateSensorCard(s As SensorSimulation,
                             lblStatus As Label,
                             lblSignal As Label,
                             lblBattery As Label,
                             dotControl As Control)

        lblStatus.Text = If(s.IsActive, "Active", "Offline")
        lblSignal.Text = s.Signal
        lblBattery.Text = s.Battery.ToString() & "%"

        ' Draw a filled circular "dot" using the control's Paint handler
        SetDotFillColor(dotControl, If(s.IsActive, Color.LimeGreen, Color.Red))

    End Sub

    ' Simulated sensors
    Private sensors As New List(Of SensorSimulation) From {
        New SensorSimulation With {.Name = "Alley 18 Station", .s1Water = 1.0, .s2Water = 1.0},
        New SensorSimulation With {.Name = "Entry 1 (Upstream)", .s1Water = 1.2, .s2Water = 1.2}
    }

    Private Sub SendCommandToESP(endpoint As String)
        Try
            Dim espIP As String = "http://10.237.203.199"  ' your receiver IP
            Dim url As String = espIP & endpoint

            Dim request As HttpWebRequest = CType(WebRequest.Create(url), HttpWebRequest)
            request.Method = "GET"
            request.Timeout = 3000

            Using response As HttpWebResponse = CType(request.GetResponse(), HttpWebResponse)
                Using reader As New StreamReader(response.GetResponseStream())
                    Dim result As String = reader.ReadToEnd()
                    Console.WriteLine("ESP Response: " & result)
                End Using
            End Using

        Catch ex As Exception
            MessageBox.Show("Failed to send command to ESP32: " & ex.Message)
        End Try
    End Sub

    Private Sub Label12_Click(sender As Object, e As EventArgs) Handles Label12.Click

    End Sub

    Private buzzerState As Boolean = False

    Private Sub Guna2Button11_Click(sender As Object, e As EventArgs) Handles Guna2Button11.Click

        If buzzerState = False Then
            ' Turn ON buzzer
            SendCommandToESP("/buzzer_on")
            Label4.Text = "Deactivate Buzzer"
            Guna2Button11.Text = "TURN BUZZER OFF"
            buzzerState = True

        Else
            ' Turn OFF buzzer
            SendCommandToESP("/buzzer_off")
            Label4.Text = "Activate Buzzer"
            Guna2Button11.Text = "TURN BUZZER ON"
            buzzerState = False

        End If
    End Sub

    Private Sub Label4_Click(sender As Object, e As EventArgs) Handles Label4.Click

    End Sub
End Class
