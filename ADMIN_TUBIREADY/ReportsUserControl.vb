Public Class ReportsUserControl

    Private Sub ReportsUserControl_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetupWaterLevelTable()
        LoadSampleData()
    End Sub

    Private Sub SetupWaterLevelTable()

        dgvWaterLevel.Columns.Clear()
        dgvWaterLevel.Rows.Clear()

        ' ❌ Remove header boxes / vertical lines
        dgvWaterLevel.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None

        ' Behavior
        dgvWaterLevel.AllowUserToAddRows = False
        dgvWaterLevel.AllowUserToDeleteRows = False
        dgvWaterLevel.ReadOnly = True
        dgvWaterLevel.RowHeadersVisible = False
        dgvWaterLevel.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvWaterLevel.ScrollBars = ScrollBars.Vertical
        dgvWaterLevel.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill

        ' Appearance
        dgvWaterLevel.BackgroundColor = Color.White
        dgvWaterLevel.BorderStyle = BorderStyle.None
        dgvWaterLevel.RowTemplate.Height = 42

        dgvWaterLevel.DefaultCellStyle.SelectionBackColor = Color.White
        dgvWaterLevel.DefaultCellStyle.SelectionForeColor = Color.Black

        dgvWaterLevel.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
        dgvWaterLevel.GridColor = Color.Gainsboro

        ' Columns
        dgvWaterLevel.Columns.Add("Time", "Time")
        dgvWaterLevel.Columns.Add("Level", "Level (m)")
        dgvWaterLevel.Columns.Add("Status", "Status")

        ' Header
        dgvWaterLevel.ColumnHeadersVisible = True
        dgvWaterLevel.ColumnHeadersHeight = 45
        dgvWaterLevel.ColumnHeadersHeightSizeMode =
            DataGridViewColumnHeadersHeightSizeMode.DisableResizing

        dgvWaterLevel.EnableHeadersVisualStyles = False
        With dgvWaterLevel.ColumnHeadersDefaultCellStyle
            .BackColor = Color.White
            .ForeColor = Color.Black
            .SelectionBackColor = Color.White
            .SelectionForeColor = Color.Black
            .Font = New Font("Segoe UI", 10, FontStyle.Bold)
            .Alignment = DataGridViewContentAlignment.MiddleLeft
        End With

        ' Alignments
        dgvWaterLevel.Columns("Time").DefaultCellStyle.Alignment =
            DataGridViewContentAlignment.MiddleLeft

        dgvWaterLevel.Columns("Level").DefaultCellStyle.Alignment =
            DataGridViewContentAlignment.MiddleCenter
        dgvWaterLevel.Columns("Level").HeaderCell.Style.Alignment =
            DataGridViewContentAlignment.MiddleCenter

        dgvWaterLevel.Columns("Status").DefaultCellStyle.Alignment =
            DataGridViewContentAlignment.MiddleCenter
        dgvWaterLevel.Columns("Status").HeaderCell.Style.Alignment =
            DataGridViewContentAlignment.MiddleCenter

        dgvWaterLevel.ClearSelection()
        dgvWaterLevel.CurrentCell = Nothing

    End Sub

    Private Sub LoadSampleData()

        dgvWaterLevel.Rows.Add("00:00", "45.2", "Normal")
        dgvWaterLevel.Rows.Add("02:00", "46.8", "Normal")
        dgvWaterLevel.Rows.Add("04:00", "48.3", "Normal")
        dgvWaterLevel.Rows.Add("06:00", "52.1", "Warning")
        dgvWaterLevel.Rows.Add("08:00", "55.7", "Critical")
        dgvWaterLevel.Rows.Add("10:00", "58.2", "Critical")

    End Sub

    Private Sub dgvWaterLevel_CellPainting(
        sender As Object,
        e As DataGridViewCellPaintingEventArgs
    ) Handles dgvWaterLevel.CellPainting

        ' ---------------- HEADER UNDERLINE ----------------
        If e.RowIndex = -1 Then
            e.PaintBackground(e.ClipBounds, False)
            e.PaintContent(e.ClipBounds)

            Using pen As New Pen(Color.Gainsboro, 1)
                e.Graphics.DrawLine(
                    pen,
                    dgvWaterLevel.DisplayRectangle.Left,
                    e.CellBounds.Bottom - 1,
                    dgvWaterLevel.DisplayRectangle.Right,
                    e.CellBounds.Bottom - 1
                )
            End Using

            e.Handled = True
            Exit Sub
        End If

        ' ---------------- SAFETY ----------------
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 OrElse e.Value Is Nothing Then Exit Sub

        ' ---------------- STATUS COLOR PILL ----------------
        If dgvWaterLevel.Columns(e.ColumnIndex).Name = "Status" Then

            e.Handled = True
            e.PaintBackground(e.ClipBounds, True)

            Dim text As String = e.FormattedValue.ToString()
            Dim bgColor As Color = Color.LightGray
            Dim textColor As Color = Color.Black

            Select Case text
                Case "Normal"
                    bgColor = Color.FromArgb(170, 240, 180)
                Case "Warning"
                    bgColor = Color.FromArgb(250, 235, 150)
                Case "Critical"
                    bgColor = Color.FromArgb(220, 90, 90)
                    textColor = Color.White
            End Select

            Dim rect As New Rectangle(
                e.CellBounds.X + 10,
                e.CellBounds.Y + 8,
                e.CellBounds.Width - 20,
                e.CellBounds.Height - 16
            )

            Using path As New Drawing2D.GraphicsPath()
                Dim radius As Integer = 15
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90)
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90)
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90)
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90)
                path.CloseFigure()

                Using brush As New SolidBrush(bgColor)
                    e.Graphics.FillPath(brush, path)
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
        End If

    End Sub

End Class
