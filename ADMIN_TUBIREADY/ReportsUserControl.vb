Imports System.Data.SqlClient
Imports System.Drawing.Drawing2D
Imports Microsoft.Data.SqlClient
Imports Guna.Charts.WinForms

Public Class ReportsUserControl

    Private contentHeight As Integer

    ' ===================== LOAD =====================
    Private Sub ReportsUserControl_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetupWaterLevelTable()
        LoadWaterLevelData()

        LoadWaterLevelSummary()

        InitChartFilters()
        LoadWaterLevelChart()

        LoadMinMaxGraph()
        SetupMinAveMaxTable()
        LoadMinAveMaxTable()

        InitThresholdFilters()
        LoadThresholdMonitoring()

        EnableMouseWheelScroll(Me)
        RemoveHandler dgvWaterLevel.MouseWheel, AddressOf ForwardMouseWheel
        RemoveHandler minavemaxAnalysis.MouseWheel, AddressOf ForwardMouseWheel
        RemoveHandler minmaxGraph.MouseWheel, AddressOf ForwardMouseWheel

        InitMinMaxFilters()
        LoadMinMaxGraph()
        LoadMinAveMaxTable()

        Me.BeginInvoke(Sub() SetupPageScroll())
    End Sub

    ' ---------------------- UI for datagridiew --------------------
    Private Sub ApplyUnifiedGridStyle(dgv As DataGridView)

        dgv.SuspendLayout()

        ' ===== BASIC =====
        dgv.ReadOnly = True
        dgv.RowHeadersVisible = False
        dgv.MultiSelect = False
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill

        ' ===== ROWS =====
        dgv.RowTemplate.Height = 40
        dgv.AllowUserToAddRows = False
        dgv.AllowUserToDeleteRows = False
        dgv.AllowUserToResizeRows = False

        ' ===== HEADERS =====
        dgv.ColumnHeadersVisible = True
        dgv.EnableHeadersVisualStyles = False
        dgv.ColumnHeadersHeight = 40
        dgv.ColumnHeadersHeightSizeMode =
        DataGridViewColumnHeadersHeightSizeMode.DisableResizing

        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.White
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black
        ' dgv.ColumnHeadersDefaultCellStyle.Font =
        ' New Font("Segoe UI", 10, FontStyle.Bold)
        dgv.ColumnHeadersDefaultCellStyle.Alignment =
        DataGridViewContentAlignment.MiddleLeft

        ' ===== GRID =====
        dgv.BorderStyle = BorderStyle.FixedSingle
        dgv.CellBorderStyle = DataGridViewCellBorderStyle.Single
        dgv.GridColor = Color.FromArgb(220, 220, 220)
        dgv.BackgroundColor = Color.White

        ' ===== CELLS =====
        ' dgv.DefaultCellStyle.Font =
        '  New Font("Segoe UI", 10, FontStyle.Regular)
        dgv.ClearSelection()
        dgv.DefaultCellStyle.Alignment =
        DataGridViewContentAlignment.MiddleCenter

        ' ===== PREVENT COLUMN JITTER =====
        For Each col As DataGridViewColumn In dgv.Columns
            col.SortMode = DataGridViewColumnSortMode.NotSortable
        Next

        dgv.ResumeLayout()
    End Sub

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)

        Dim dgvType = GetType(DataGridView)
        Dim prop = dgvType.GetProperty("DoubleBuffered",
        Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic)
        prop.SetValue(dgvWaterLevel, True, Nothing)
    End Sub

    Private Sub ReportsUserControl_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        SetupPageScroll()
    End Sub

    ' ===================== SUMMARY LABELS =====================
    Private Sub LoadWaterLevelSummary()

        Using con As New SqlConnection(ConnString)
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
                           "0 m",
                           Math.Round(CDbl(rdr("AvgLevel")), 2) & " m")

                        lblMinMax.Text =
                        If(IsDBNull(rdr("MinLevel")),
                           "— / — m",
                            Math.Round(CDbl(rdr("MinLevel")), 2) & " / " &
                            Math.Round(CDbl(rdr("MaxLevel")), 2) & " m")

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

    Private Sub InitMinMaxFilters()
        cmbGunaDay2.Items.Clear()
        cmbGunaDay2.Items.AddRange(New String() {
        "Today",
        "Yesterday",
        "Last 7 Days",
        "Last 30 Days"
    })
        cmbGunaDay2.SelectedIndex = 0

        cmbGunaTime2.Items.Clear()
        cmbGunaTime2.Items.AddRange(New String() {
        "Hourly",
        "Daily",
        "Weekly"
    })
        cmbGunaTime2.SelectedIndex = 0
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

        Using con As New SqlConnection(ConnString)
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

                        ' ✅ ADD THIS PART
                        Dim val As Double = CDbl(rdr("Val"))
                        If val <= 0 Then Continue While

                        bar.DataPoints.Add(
                            New LPoint(label, Math.Round(val, 2))
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

        Guna2vScrollBar1.LargeChange = 80
        Guna2vScrollBar1.SmallChange = 25
    End Sub

    ' ===================== TABLE SETUP =====================
    Private Sub SetupWaterLevelTable()
        dgvWaterLevel.SuspendLayout()

        dgvWaterLevel.Columns.Clear()
        dgvWaterLevel.Rows.Clear()

        ' ===== BASIC SETTINGS =====
        dgvWaterLevel.ReadOnly = True
        dgvWaterLevel.RowHeadersVisible = False
        dgvWaterLevel.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvWaterLevel.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        ' dgvWaterLevel.RowTemplate.Height = 42
        dgvWaterLevel.MultiSelect = False

        ' ===== HEADER (FIXED & STABLE) =====
        ' dgvWaterLevel.EnableHeadersVisualStyles = False
        ' dgvWaterLevel.ColumnHeadersHeight = 40
        dgvWaterLevel.ColumnHeadersHeightSizeMode =
        DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        dgvWaterLevel.ColumnHeadersBorderStyle =
        DataGridViewHeaderBorderStyle.Single

        dgvWaterLevel.ColumnHeadersDefaultCellStyle.BackColor = Color.White
        dgvWaterLevel.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black
        dgvWaterLevel.ColumnHeadersDefaultCellStyle.Font =
        New Font("Segoe UI", 10, FontStyle.Bold)
        dgvWaterLevel.ColumnHeadersDefaultCellStyle.Alignment =
        DataGridViewContentAlignment.MiddleLeft

        ' ===== GRID & SCROLL STABILITY (IMPORTANT) =====
        dgvWaterLevel.BorderStyle = BorderStyle.FixedSingle
        dgvWaterLevel.CellBorderStyle = DataGridViewCellBorderStyle.Single
        dgvWaterLevel.GridColor = Color.FromArgb(220, 220, 220)

        ' ===== CELL STYLE =====
        ' dgvWaterLevel.DefaultCellStyle.Font =
        '  New Font("Segoe UI", 10, FontStyle.Regular)
        dgvWaterLevel.DefaultCellStyle.SelectionBackColor = Color.White
        dgvWaterLevel.DefaultCellStyle.SelectionForeColor = Color.Black
        dgvWaterLevel.BackgroundColor = Color.White

        ' ===== COLUMNS =====
        dgvWaterLevel.Columns.Add("Time", "Time")
        dgvWaterLevel.Columns.Add("Level", "Level (m)")
        dgvWaterLevel.Columns.Add("Status", "Status")

        dgvWaterLevel.Columns("Time").DefaultCellStyle.Alignment =
        DataGridViewContentAlignment.MiddleLeft
        dgvWaterLevel.Columns("Level").DefaultCellStyle.Alignment =
        DataGridViewContentAlignment.MiddleCenter
        dgvWaterLevel.Columns("Status").DefaultCellStyle.Alignment =
        DataGridViewContentAlignment.MiddleCenter

        ' ===== PREVENT JITTER =====
        For Each col As DataGridViewColumn In dgvWaterLevel.Columns
            col.SortMode = DataGridViewColumnSortMode.NotSortable
        Next

        dgvWaterLevel.ResumeLayout()
        ApplyUnifiedGridStyle(dgvWaterLevel)
    End Sub

    ' ===================== LOAD TABLE DATA =====================
    Private Sub LoadWaterLevelData()
        dgvWaterLevel.Rows.Clear()

        Using con As New SqlConnection(ConnString)
            con.Open()

            Dim q As String =
                "SELECT ReadingTime, WaterLevel, Severity
                 FROM Ultrasonic
                 ORDER BY ReadingTime DESC"

            Using cmd As New SqlCommand(q, con)
                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()

                        Dim level As Double = CDbl(rdr("WaterLevel"))
                        If level <= 0 Then Continue While   ' ✅ FILTER 0.00

                        dgvWaterLevel.Rows.Add(
                            CType(rdr("ReadingTime"), DateTime).ToString("HH:mm"),
                            Math.Round(level, 2),
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

        Dim text As String = e.FormattedValue.ToString()
        Dim bgColor As Color = Color.LightGray
        Dim textColor As Color = Color.Black

        Select Case text.ToLower()
            Case "low"
                bgColor = Color.FromArgb(76, 175, 80)     ' 🟢 Green
                textColor = Color.White

            Case "normal"
                bgColor = Color.FromArgb(255, 235, 59)    ' 🟡 Yellow

            Case "high"
                bgColor = Color.FromArgb(255, 152, 0)     ' 🟠 Orange
                textColor = Color.White

            Case "critical"
                bgColor = Color.FromArgb(244, 67, 54)     ' 🔴 Red
                textColor = Color.White
        End Select

        Dim rect As New Rectangle(
        e.CellBounds.X + 10,
        e.CellBounds.Y + 8,
        e.CellBounds.Width - 20,
        e.CellBounds.Height - 16
    )

        Using path As New GraphicsPath()
            path.AddArc(rect.X, rect.Y, 20, 20, 180, 90)
            path.AddArc(rect.Right - 20, rect.Y, 20, 20, 270, 90)
            path.AddArc(rect.Right - 20, rect.Bottom - 20, 20, 20, 0, 90)
            path.AddArc(rect.X, rect.Bottom - 20, 20, 20, 90, 90)
            path.CloseFigure()

            Using b As New SolidBrush(bgColor)
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
                e.Graphics.FillPath(b, path)
            End Using
        End Using

        TextRenderer.DrawText(
        e.Graphics,
        text,
        e.CellStyle.Font,
        rect,
        textColor,
        TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter
    )
    End Sub

    ' ===================== MIN / AVG / MAX TABLE SETUP =====================
    Private Sub SetupMinAveMaxTable()

        minavemaxAnalysis.SuspendLayout()
        minavemaxAnalysis.Columns.Clear()
        minavemaxAnalysis.Rows.Clear()

        minavemaxAnalysis.ReadOnly = True
        minavemaxAnalysis.RowHeadersVisible = False
        minavemaxAnalysis.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        ' minavemaxAnalysis.RowTemplate.Height = 38
        minavemaxAnalysis.SelectionMode = DataGridViewSelectionMode.FullRowSelect

        ' minavemaxAnalysis.EnableHeadersVisualStyles = False
        ' minavemaxAnalysis.ColumnHeadersHeight = 38
        minavemaxAnalysis.ColumnHeadersHeightSizeMode =
        DataGridViewColumnHeadersHeightSizeMode.DisableResizing

        ' minavemaxAnalysis.ColumnHeadersDefaultCellStyle.Font =
        ' New Font("Segoe UI", 10, FontStyle.Bold)
        minavemaxAnalysis.ColumnHeadersDefaultCellStyle.BackColor = Color.White
        minavemaxAnalysis.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black

        minavemaxAnalysis.BorderStyle = BorderStyle.FixedSingle
        minavemaxAnalysis.CellBorderStyle = DataGridViewCellBorderStyle.Single
        minavemaxAnalysis.GridColor = Color.FromArgb(220, 220, 220)
        minavemaxAnalysis.BackgroundColor = Color.White

        ' minavemaxAnalysis.DefaultCellStyle.Font =
        ' New Font("Segoe UI", 10)
        minavemaxAnalysis.DefaultCellStyle.SelectionBackColor = Color.White
        minavemaxAnalysis.DefaultCellStyle.SelectionForeColor = Color.Black

        minavemaxAnalysis.Columns.Add("Time", "Time")
        minavemaxAnalysis.Columns.Add("Min", "Min")
        minavemaxAnalysis.Columns.Add("Avg", "Avg")
        minavemaxAnalysis.Columns.Add("Max", "Max")

        minavemaxAnalysis.Columns("Time").DefaultCellStyle.Alignment =
        DataGridViewContentAlignment.MiddleLeft
        minavemaxAnalysis.Columns("Min").DefaultCellStyle.Alignment =
        DataGridViewContentAlignment.MiddleCenter
        minavemaxAnalysis.Columns("Avg").DefaultCellStyle.Alignment =
        DataGridViewContentAlignment.MiddleCenter
        minavemaxAnalysis.Columns("Max").DefaultCellStyle.Alignment =
        DataGridViewContentAlignment.MiddleCenter

        minavemaxAnalysis.ResumeLayout()
        ApplyUnifiedGridStyle(minavemaxAnalysis)
    End Sub

    ' ===================== MIN / AVG / MAX GRAPH =====================
    Private Sub LoadMinMaxGraph()

        minmaxGraph.Datasets.Clear()

        Dim minSet As New GunaLineDataset()
        Dim avgSet As New GunaLineDataset()
        Dim maxSet As New GunaLineDataset()

        minSet.Label = "Min"
        avgSet.Label = "Avg"
        maxSet.Label = "Max"

        minSet.PointRadius = 3
        avgSet.PointRadius = 3
        maxSet.PointRadius = 3

        minSet.BorderColor = Color.FromArgb(76, 175, 80)
        avgSet.BorderColor = Color.FromArgb(33, 150, 243)
        maxSet.BorderColor = Color.FromArgb(244, 67, 54)

        ' ===== DATE RANGE (USING cmbGunaDay2) =====
        Dim startDate As DateTime
        Dim endDate As DateTime = DateTime.Now

        Select Case cmbGunaDay2.Text.Trim().ToLower()
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

        ' ===== TIME GROUPING (USING cmbGunaTime2) =====
        Dim query As String = ""

        Select Case cmbGunaTime2.Text.Trim().ToLower()

            Case "hourly"
                query =
        "SELECT DATEPART(HOUR, ReadingTime) AS Lbl,
                MIN(WaterLevel) AS MinVal,
                AVG(WaterLevel) AS AvgVal,
                MAX(WaterLevel) AS MaxVal
         FROM Ultrasonic
         WHERE ReadingTime BETWEEN @s AND @e
         GROUP BY DATEPART(HOUR, ReadingTime)
         ORDER BY Lbl"

            Case "daily"
                query =
        "SELECT CAST(ReadingTime AS DATE) AS Lbl,
                MIN(WaterLevel) AS MinVal,
                AVG(WaterLevel) AS AvgVal,
                MAX(WaterLevel) AS MaxVal
         FROM Ultrasonic
         WHERE ReadingTime BETWEEN @s AND @e
         GROUP BY CAST(ReadingTime AS DATE)
         ORDER BY Lbl"

            Case "weekly"
                query =
        "SELECT DATEPART(WEEK, ReadingTime) AS Lbl,
                MIN(WaterLevel) AS MinVal,
                AVG(WaterLevel) AS AvgVal,
                MAX(WaterLevel) AS MaxVal
         FROM Ultrasonic
         WHERE ReadingTime BETWEEN @s AND @e
         GROUP BY DATEPART(WEEK, ReadingTime)
         ORDER BY Lbl"

            Case Else
                ' ✅ SAFE FALLBACK (IMPORTANT)
                query =
        "SELECT DATEPART(HOUR, ReadingTime) AS Lbl,
                MIN(WaterLevel) AS MinVal,
                AVG(WaterLevel) AS AvgVal,
                MAX(WaterLevel) AS MaxVal
         FROM Ultrasonic
         WHERE ReadingTime BETWEEN @s AND @e
         GROUP BY DATEPART(HOUR, ReadingTime)
         ORDER BY Lbl"
        End Select

        Using con As New SqlConnection(ConnString)
            con.Open()

            Using cmd As New SqlCommand(query, con)
                cmd.Parameters.AddWithValue("@s", startDate)
                cmd.Parameters.AddWithValue("@e", endDate)

                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()

                        Dim label As String

                        If cmbGunaTime2.Text = "Hourly" Then
                            label = rdr("Lbl").ToString().PadLeft(2, "0"c) & ":00"
                        Else
                            label = rdr("Lbl").ToString()
                        End If

                        Dim minVal As Double = CDbl(rdr("MinVal"))
                        Dim avgVal As Double = CDbl(rdr("AvgVal"))
                        Dim maxVal As Double = CDbl(rdr("MaxVal"))

                        If minVal <= 0 AndAlso avgVal <= 0 AndAlso maxVal <= 0 Then Continue While

                        minSet.DataPoints.Add(New LPoint(label, Math.Round(CDbl(rdr("MinVal")), 2)))
                        avgSet.DataPoints.Add(New LPoint(label, Math.Round(CDbl(rdr("AvgVal")), 2)))
                        maxSet.DataPoints.Add(New LPoint(label, Math.Round(CDbl(rdr("MaxVal")), 2)))

                    End While
                End Using
            End Using
        End Using

        minmaxGraph.Datasets.Add(minSet)
        minmaxGraph.Datasets.Add(avgSet)
        minmaxGraph.Datasets.Add(maxSet)

        minmaxGraph.Update()
    End Sub

    ' ===================== LOAD MIN / AVG / MAX DATA =====================
    Private Sub LoadMinAveMaxTable()

        minavemaxAnalysis.Rows.Clear()

        Using con As New SqlConnection(ConnString)
            con.Open()

            Dim q As String =
        "SELECT 
            DATEPART(HOUR, ReadingTime) AS Hr,
            MIN(WaterLevel) AS MinVal,
            AVG(WaterLevel) AS AvgVal,
            MAX(WaterLevel) AS MaxVal
         FROM Ultrasonic
         GROUP BY DATEPART(HOUR, ReadingTime)
         ORDER BY Hr"

            Using cmd As New SqlCommand(q, con)
                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()

                        Dim minVal As Double = CDbl(rdr("MinVal"))
                        Dim avgVal As Double = CDbl(rdr("AvgVal"))
                        Dim maxVal As Double = CDbl(rdr("MaxVal"))

                        If minVal <= 0 AndAlso avgVal <= 0 AndAlso maxVal <= 0 Then Continue While

                        minavemaxAnalysis.Rows.Add(
                        rdr("Hr").ToString().PadLeft(2, "0"c) & ":00",
                        Math.Round(CDbl(rdr("MinVal")), 1),
                        Math.Round(CDbl(rdr("AvgVal")), 1),
                        Math.Round(CDbl(rdr("MaxVal")), 1)
                    )

                    End While
                End Using
            End Using
        End Using
    End Sub

    Private Sub minavemaxAnalysis_CellFormatting(
    sender As Object,
    e As DataGridViewCellFormattingEventArgs) _
    Handles minavemaxAnalysis.CellFormatting

        If e.RowIndex < 0 Then Exit Sub

        Select Case minavemaxAnalysis.Columns(e.ColumnIndex).Name
            Case "Min"
                e.CellStyle.ForeColor = Color.FromArgb(76, 175, 80)   ' Green
            Case "Avg"
                e.CellStyle.ForeColor = Color.FromArgb(33, 150, 243)  ' Blue
            Case "Max"
                e.CellStyle.ForeColor = Color.FromArgb(244, 67, 54)   ' Red
        End Select
    End Sub

    ' ===================== COMBO EVENTS =====================
    Private Sub cmbGunaDay_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbGunaDay.SelectedIndexChanged
        LoadWaterLevelChart()
    End Sub

    Private Sub cmbGunaTime_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbGunaTime.SelectedIndexChanged
        LoadWaterLevelChart()
    End Sub

    Private Sub Guna2vScrollBar1_Scroll(
        sender As Object,
        e As ScrollEventArgs
    ) Handles Guna2vScrollBar1.Scroll

        ' Move the content panel vertically
        pnlContent.Top = -Guna2vScrollBar1.Value

    End Sub

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        MyBase.OnMouseWheel(e)

        If Not Guna2vScrollBar1.Visible Then Exit Sub

        Dim newValue As Integer =
        Guna2vScrollBar1.Value - (e.Delta \ 4)

        newValue = Math.Max(Guna2vScrollBar1.Minimum,
               Math.Min(Guna2vScrollBar1.Maximum, newValue))

        Guna2vScrollBar1.Value = newValue
        pnlContent.Top = -newValue
    End Sub
    Private Sub EnableMouseWheelScroll(ctrl As Control)
        AddHandler ctrl.MouseWheel, AddressOf ForwardMouseWheel

        For Each c As Control In ctrl.Controls
            EnableMouseWheelScroll(c)
        Next
    End Sub
    Private Sub ForwardMouseWheel(sender As Object, e As MouseEventArgs)
        If TypeOf sender Is DataGridView _
       OrElse TypeOf sender Is Guna.Charts.WinForms.GunaChart Then
            Exit Sub
        End If

        If Not Guna2vScrollBar1.Visible Then Exit Sub

        Dim newValue As Integer =
        Guna2vScrollBar1.Value - (e.Delta \ 4)

        newValue = Math.Max(Guna2vScrollBar1.Minimum,
                        Math.Min(Guna2vScrollBar1.Maximum, newValue))

        Guna2vScrollBar1.Value = newValue
        pnlContent.Top = -newValue
    End Sub

    Private Sub cmbGunaDay2_SelectedIndexChanged(
        sender As Object, e As EventArgs
    ) Handles cmbGunaDay2.SelectedIndexChanged

        LoadMinMaxGraph()
        LoadMinAveMaxTable()

    End Sub

    Private Sub cmbGunaTime2_SelectedIndexChanged(
        sender As Object, e As EventArgs
    ) Handles cmbGunaTime2.SelectedIndexChanged

        LoadMinMaxGraph()
        LoadMinAveMaxTable()

    End Sub

    ' ===================== THRESHOLD MONITORING =====================
    Private Sub LoadThresholdMonitoring()

        ' 🔐 Safety defaults (SAFE)
        If cmbGunaDay1.Items.Count > 0 AndAlso cmbGunaDay1.SelectedIndex = -1 Then
            cmbGunaDay1.SelectedIndex = 0
        End If

        If cmbGunaTime1.Items.Count > 0 AndAlso cmbGunaTime1.SelectedIndex = -1 Then
            cmbGunaTime1.SelectedIndex = 0
        End If

        GunaChartThreshold.Datasets.Clear()

        ' ===== FIXED THRESHOLDS =====
        Dim warningThreshold As Double = 50
        Dim criticalThreshold As Double = 57

        lblWarningThreshold.Text = warningThreshold & " m"
        lblCriticalThreshold.Text = criticalThreshold & " m"

        Dim waterSet As New GunaLineDataset() With {
        .Label = "Water Level",
        .BorderColor = Color.FromArgb(33, 150, 243),
        .PointRadius = 3
    }

        Dim warningSet As New GunaLineDataset() With {
         .Label = "Warning",
         .BorderColor = Color.FromArgb(255, 193, 7),
         .PointRadius = 0,
         .BorderWidth = 2
    }

        Dim criticalSet As New GunaLineDataset() With {
         .Label = "Critical",
         .BorderColor = Color.FromArgb(244, 67, 54),
         .PointRadius = 0,
         .BorderWidth = 2
    }

        ' ===== DATE RANGE =====
        Dim startDate As DateTime
        Dim endDate As DateTime = DateTime.Now

        Select Case cmbGunaDay1.Text.ToLower()
            Case "today" : startDate = Date.Today
            Case "yesterday"
                startDate = Date.Today.AddDays(-1)
                endDate = Date.Today
            Case "last 7 days" : startDate = Date.Today.AddDays(-7)
            Case "last 30 days" : startDate = Date.Today.AddDays(-30)
            Case Else : startDate = Date.Today
        End Select

        ' ===== QUERY BASED ON TIME =====
        Dim query As String

        Select Case cmbGunaTime1.Text.ToLower()
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

            Case Else
                query =
            "SELECT DATEPART(HOUR, ReadingTime) AS Lbl,
                    AVG(WaterLevel) AS Val
             FROM Ultrasonic
             WHERE ReadingTime BETWEEN @s AND @e
             GROUP BY DATEPART(HOUR, ReadingTime)
             ORDER BY Lbl"
        End Select

        Dim breachCount As Integer = 0

        Using con As New SqlConnection(ConnString)
            con.Open()

            Using cmd As New SqlCommand(query, con)
                cmd.Parameters.AddWithValue("@s", startDate)
                cmd.Parameters.AddWithValue("@e", endDate)

                Using rdr = cmd.ExecuteReader()
                    While rdr.Read()

                        Dim label As String =
                        If(cmbGunaTime1.Text = "Hourly",
                           rdr("Lbl").ToString().PadLeft(2, "0"c) & ":00",
                           rdr("Lbl").ToString())

                        Dim val As Double = Math.Round(CDbl(rdr("Val")), 2)

                        If val <= 0 Then Continue While   ' FILTER 0.00

                        waterSet.DataPoints.Add(New LPoint(label, val))
                        warningSet.DataPoints.Add(New LPoint(label, warningThreshold))
                        criticalSet.DataPoints.Add(New LPoint(label, criticalThreshold))

                        If val >= warningThreshold Then breachCount += 1
                    End While
                End Using
            End Using
        End Using

        lblThresholdBreaches.Text = breachCount & " Total"

        GunaChartThreshold.Datasets.Add(waterSet)
        GunaChartThreshold.Datasets.Add(warningSet)
        GunaChartThreshold.Datasets.Add(criticalSet)

        GunaChartThreshold.Update()
    End Sub
    Private Sub InitThresholdFilters()
        cmbGunaDay1.Items.Clear()
        cmbGunaDay1.Items.AddRange(New String() {
        "Today",
        "Yesterday",
        "Last 7 Days",
        "Last 30 Days"
    })
        cmbGunaDay1.SelectedIndex = 0

        cmbGunaTime1.Items.Clear()
        cmbGunaTime1.Items.AddRange(New String() {
        "Hourly",
        "Daily",
        "Weekly"
    })
        cmbGunaTime1.SelectedIndex = 0
    End Sub

    Private Sub cmbGunaDay1_SelectedIndexChanged(sender As Object, e As EventArgs) _
    Handles cmbGunaDay1.SelectedIndexChanged

        LoadThresholdMonitoring()
    End Sub

    Private Sub cmbGunaTime1_SelectedIndexChanged(sender As Object, e As EventArgs) _
    Handles cmbGunaTime1.SelectedIndexChanged

        LoadThresholdMonitoring()
    End Sub

    Private Sub GunaChartThreshold_Load(sender As Object, e As EventArgs) _
    Handles GunaChartThreshold.Load

        LoadThresholdMonitoring()
    End Sub

End Class
