Imports System.Data.SqlClient
Imports System.Drawing.Drawing2D
Imports Microsoft.Data.SqlClient
Imports Guna.Charts.WinForms

Public Class ReportsUserControl

    Private contentHeight As Integer

    Private connectionString As String =
        "server=DESKTOP-011N7DN;user id=TubiReadyAdmin;password=123456789;database=TubiReadyDB;TrustServerCertificate=True;"

    ' ===================== LOAD =====================
    Private Sub ReportsUserControl_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetupWaterLevelTable()
        LoadWaterLevelData()

        LoadWaterLevelSummary()

        InitChartFilters()
        LoadWaterLevelChart()

        Me.BeginInvoke(Sub() SetupPageScroll())
    End Sub

    Private Sub ReportsUserControl_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        SetupPageScroll()
    End Sub

    ' ===================== SUMMARY LABELS =====================
    Private Sub LoadWaterLevelSummary()

        Using con As New SqlConnection(connectionString)
            con.Open()

            Dim q As String =
        "SELECT 
            COUNT(*) AS TotalReadings,
            AVG(WaterLevel) AS AvgLevel,
            MIN(WaterLevel) AS MinLevel,
            MAX(WaterLevel) AS MaxLevel,
            SUM(CASE WHEN Severity = 'Low' THEN 1 ELSE 0 END) AS LowCnt,
            SUM(CASE WHEN Severity = 'Normal' THEN 1 ELSE 0 END) AS NormalCnt,
            SUM(CASE WHEN Severity = 'High' THEN 1 ELSE 0 END) AS HighCnt,
            SUM(CASE WHEN Severity = 'Critical' THEN 1 ELSE 0 END) AS CriticalCnt
         FROM Ultrasonic"

            Using cmd As New SqlCommand(q, con)
                Using rdr = cmd.ExecuteReader()
                    If rdr.Read() Then

                        lblReadings.Text = rdr("TotalReadings").ToString()

                        lblLevel.Text =
                        If(IsDBNull(rdr("AvgLevel")),
                           "0 cm",
                           Math.Round(CDbl(rdr("AvgLevel")), 2) & " cm")

                        lblMinMax.Text =
                        If(IsDBNull(rdr("MinLevel")),
                           "0 / 0 cm",
                           rdr("MinLevel").ToString() & " / " & rdr("MaxLevel").ToString() & " cm")

                        lblLow.Text = rdr("LowCnt").ToString()
                        lblNormal.Text = rdr("NormalCnt").ToString()
                        lblHigh.Text = rdr("HighCnt").ToString()
                        lblCritical.Text = rdr("CriticalCnt").ToString()

                    End If
                End Using
            End Using
        End Using

    End Sub

    ' ===================== CHART FILTERS =====================
    Private Sub InitChartFilters()
        cmbGunaDay.Items.Clear()
        cmbGunaDay.Items.AddRange(New String() {
            "Today",
            "Yesterday",
            "Last 7 Days",
            "Last 30 Days"
        })
        cmbGunaDay.SelectedIndex = 0

        cmbGunaTime.Items.Clear()
        cmbGunaTime.Items.AddRange(New String() {
            "Hourly",
            "Daily",
            "Weekly"
        })
        cmbGunaTime.SelectedIndex = 0
    End Sub

    ' ===================== GUNA BAR CHART =====================
    Private Sub LoadWaterLevelChart()

        chartWaterLevel.Datasets.Clear()

        Dim bar As New GunaBarDataset()
        bar.Label = "Water Level"
        bar.FillColors.Add(Color.FromArgb(66, 133, 244)) ' blue bars

        Dim startDate As DateTime
        Dim endDate As DateTime = DateTime.Now

        ' ===== DAY FILTER =====
        Select Case cmbGunaDay.Text.Trim().ToLower()
            Case "today"
                startDate = Date.Today

            Case "yesterday"
                startDate = Date.Today.AddDays(-1)
                endDate = Date.Today

            Case "last 7 days"
                startDate = Date.Today.AddDays(-7)

            Case "last 30 days"
                startDate = Date.Today.AddDays(-30)

            Case Else
                startDate = Date.Today
        End Select

        ' ===== TIME GROUPING =====
        Dim query As String = ""

        Select Case cmbGunaTime.Text.Trim().ToLower()
            Case "hourly"
                query =
            "SELECT DATEPART(HOUR, ReadingTime) AS Lbl,
                    AVG(WaterLevel) AS Val
             FROM Ultrasonic
             WHERE ReadingTime BETWEEN @s AND @e
             GROUP BY DATEPART(HOUR, ReadingTime)
             ORDER BY Lbl"

            Case "daily"
                query =
            "SELECT CAST(ReadingTime AS DATE) AS Lbl,
                    AVG(WaterLevel) AS Val
             FROM Ultrasonic
             WHERE ReadingTime BETWEEN @s AND @e
             GROUP BY CAST(ReadingTime AS DATE)
             ORDER BY Lbl"

            Case "weekly"
                query =
            "SELECT DATEPART(WEEK, ReadingTime) AS Lbl,
                    AVG(WaterLevel) AS Val
             FROM Ultrasonic
             WHERE ReadingTime BETWEEN @s AND @e
             GROUP BY DATEPART(WEEK, ReadingTime)
             ORDER BY Lbl"

            Case Else
                ' 🔐 fallback safety
                query =
            "SELECT DATEPART(HOUR, ReadingTime) AS Lbl,
                    AVG(WaterLevel) AS Val
             FROM Ultrasonic
             WHERE ReadingTime BETWEEN @s AND @e
             GROUP BY DATEPART(HOUR, ReadingTime)
             ORDER BY Lbl"
        End Select

        Using con As New SqlConnection(connectionString)
            con.Open()

            Using cmd As New SqlCommand(query, con)
                cmd.Parameters.AddWithValue("@s", startDate)
                cmd.Parameters.AddWithValue("@e", endDate)

                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()

                        Dim label As String

                        If cmbGunaTime.Text = "Hourly" Then
                            label = rdr("Lbl").ToString().PadLeft(2, "0"c) & ":00"
                        Else
                            label = rdr("Lbl").ToString()
                        End If

                        ' ✅ GunaChart requires LPoint
                        bar.DataPoints.Add(
                        New LPoint(label, Math.Round(CDbl(rdr("Val")), 2))
                    )

                    End While
                End Using
            End Using
        End Using

        chartWaterLevel.Datasets.Add(bar)
        chartWaterLevel.Update()
    End Sub

    ' ===================== PAGE SCROLL =====================
    Private Sub SetupPageScroll()
        contentHeight = pnlContent.Height
        Dim visibleHeight As Integer = Me.ClientSize.Height

        If contentHeight <= visibleHeight Then
            Guna2vScrollBar1.Visible = False
            pnlContent.Top = 0
            Exit Sub
        End If

        Guna2vScrollBar1.Visible = True
        Guna2vScrollBar1.Minimum = 0
        Guna2vScrollBar1.Maximum = contentHeight - visibleHeight
        Guna2vScrollBar1.LargeChange = 60
        Guna2vScrollBar1.SmallChange = 20
        Guna2vScrollBar1.Value = 0
    End Sub

    ' ===================== TABLE SETUP =====================
    Private Sub SetupWaterLevelTable()
        dgvWaterLevel.Columns.Clear()
        dgvWaterLevel.Rows.Clear()

        dgvWaterLevel.ReadOnly = True
        dgvWaterLevel.RowHeadersVisible = False
        dgvWaterLevel.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvWaterLevel.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvWaterLevel.RowTemplate.Height = 42

        dgvWaterLevel.Columns.Add("Time", "Time")
        dgvWaterLevel.Columns.Add("Level", "Level (cm)")
        dgvWaterLevel.Columns.Add("Status", "Status")
    End Sub

    ' ===================== LOAD TABLE DATA =====================
    Private Sub LoadWaterLevelData()
        dgvWaterLevel.Rows.Clear()

        Using con As New SqlConnection(connectionString)
            con.Open()

            Dim q As String =
                "SELECT ReadingTime, WaterLevel, Severity
                 FROM Ultrasonic
                 ORDER BY ReadingTime DESC"

            Using cmd As New SqlCommand(q, con)
                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()
                        dgvWaterLevel.Rows.Add(
                            CType(rdr("ReadingTime"), DateTime).ToString("HH:mm"),
                            rdr("WaterLevel"),
                            rdr("Severity")
                        )
                    End While
                End Using
            End Using
        End Using
    End Sub

    ' ===================== STATUS PILL =====================
    Private Sub dgvWaterLevel_CellPainting(sender As Object,
                                           e As DataGridViewCellPaintingEventArgs) _
                                           Handles dgvWaterLevel.CellPainting

        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Exit Sub
        If dgvWaterLevel.Columns(e.ColumnIndex).Name <> "Status" Then Exit Sub

        e.Handled = True
        e.PaintBackground(e.ClipBounds, True)

        Dim text = e.FormattedValue.ToString()
        Dim bgColor As Color = Color.LightGray
        Dim textColor As Color = Color.Black

        Select Case text.ToLower()
            Case "normal" : bgColor = Color.FromArgb(170, 240, 180)
            Case "warning", "high" : bgColor = Color.FromArgb(250, 235, 150)
            Case "critical"
                bgColor = Color.FromArgb(220, 90, 90)
                textColor = Color.White
        End Select

        Dim rect As New Rectangle(e.CellBounds.X + 10, e.CellBounds.Y + 8,
                                  e.CellBounds.Width - 20, e.CellBounds.Height - 16)

        Using path As New GraphicsPath()
            path.AddArc(rect.X, rect.Y, 15, 15, 180, 90)
            path.AddArc(rect.Right - 15, rect.Y, 15, 15, 270, 90)
            path.AddArc(rect.Right - 15, rect.Bottom - 15, 15, 15, 0, 90)
            path.AddArc(rect.X, rect.Bottom - 15, 15, 15, 90, 90)
            path.CloseFigure()

            Using b As New SolidBrush(bgColor)
                e.Graphics.FillPath(b, path)
            End Using
        End Using

        TextRenderer.DrawText(e.Graphics, text, e.CellStyle.Font,
                              rect, textColor,
                              TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
    End Sub

    ' ===================== COMBO EVENTS =====================
    Private Sub cmbGunaDay_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbGunaDay.SelectedIndexChanged
        LoadWaterLevelChart()
    End Sub

    Private Sub cmbGunaTime_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbGunaTime.SelectedIndexChanged
        LoadWaterLevelChart()
    End Sub

End Class
