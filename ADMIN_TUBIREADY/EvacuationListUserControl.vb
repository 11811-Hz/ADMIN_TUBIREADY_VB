Imports Guna.UI2.WinForms
Imports System.Drawing

Public Class EvacuationListUserControl

    ' State variables
    Private _totalRecords As Integer = 300 ' Total entries in DB
    Private _recordsPerPage As Integer = 10 ' How many rows per page
    Private _currentPage As Integer = 1

    ' Calculate total pages dynamically
    Private ReadOnly Property _totalPages As Integer
        Get
            Return Math.Ceiling(_totalRecords / _recordsPerPage)
        End Get
    End Property

    ' UI Controls (Defined here so we can access them easily)
    Private btnPrev As Guna2Button
    Private btnNext As Guna2Button
    Private txtPageNumber As Guna2TextBox

    Private Sub EvacuationListUserControl_Load(sender As Object, e As EventArgs) Handles Me.Load
        ' 1. Setup the Container
        flpPagination.WrapContents = False
        flpPagination.FlowDirection = FlowDirection.LeftToRight
        flpPagination.AutoSize = True
        flpPagination.AutoSizeMode = AutoSizeMode.GrowAndShrink
        ' Center the items vertically in the row
        flpPagination.WrapContents = False

        ' 2. Create the UI ONCE
        InitializePaginationControls()

        ' 3. Set Initial Values
        UpdatePaginationUI()
        SetupDgvBarangayResidents()
    End Sub

    ' --- ONE-TIME SETUP ---
    Private Sub InitializePaginationControls()
        flpPagination.Controls.Clear()

        ' 1. Previous Button (<)
        btnPrev = CreateNavButton("<")
        AddHandler btnPrev.Click, Sub(sender, e)
                                      If _currentPage > 1 Then
                                          _currentPage -= 1
                                          UpdatePaginationUI()
                                          ' LoadData() ' <--- Call your DB load here
                                      End If
                                  End Sub
        flpPagination.Controls.Add(btnPrev)

        ' 2. The Page Input TextBox
        txtPageNumber = New Guna2TextBox()
        With txtPageNumber
            .Size = New Size(60, 38) ' Compact width
            .TextAlign = HorizontalAlignment.Center
            .BorderRadius = 4
            .BorderColor = Color.FromArgb(213, 218, 223)
            .Font = New Font("Segoe UI", 9, FontStyle.Bold)
            .ForeColor = Color.FromArgb(64, 64, 64)

            ' Visual Polish
            .HoverState.BorderColor = Color.DodgerBlue
            .FocusedState.BorderColor = Color.DodgerBlue
        End With

        ' Handle the "Enter" Key to jump to page
        AddHandler txtPageNumber.KeyDown, AddressOf txtPageNumber_KeyDown
        ' Optional: Handle leaving focus to reset if invalid
        AddHandler txtPageNumber.Leave, AddressOf txtPageNumber_Leave

        flpPagination.Controls.Add(txtPageNumber)

        ' 3. Next Button (>)
        btnNext = CreateNavButton(">")
        AddHandler btnNext.Click, Sub(sender, e)
                                      If _currentPage < _totalPages Then
                                          _currentPage += 1
                                          UpdatePaginationUI()
                                          ' LoadData() ' <--- Call your DB load here
                                      End If
                                  End Sub
        flpPagination.Controls.Add(btnNext)
    End Sub

    ' --- UI UPDATE LOGIC ---
    Private Sub UpdatePaginationUI()
        ' --- 1. Update the TextBox ---
        txtPageNumber.Text = _currentPage.ToString()

        ' --- 2. Update the "Showing X to Y of Z" Label ---
        If _totalRecords = 0 Then
            lblCurrentPage.Text = "No records found"
        Else
            ' Logic to find the start and end index
            Dim startRecord As Integer = ((_currentPage - 1) * _recordsPerPage) + 1
            Dim endRecord As Integer = Math.Min(_currentPage * _recordsPerPage, _totalRecords)

            ' Format: "Showing 1 to 10 of 300 entries"
            lblCurrentPage.Text = String.Format("Showing {0} to {1} of {2} entries",
                                            startRecord, endRecord, _totalRecords)
        End If

        ' --- 3. Enable/Disable Arrows ---
        btnPrev.Enabled = (_currentPage > 1)
        btnNext.Enabled = (_currentPage < _totalPages)
    End Sub

    ' --- EVENT HANDLERS ---

    ' Logic: When user presses ENTER, validate and jump
    Private Sub txtPageNumber_KeyDown(sender As Object, e As KeyEventArgs)
        If e.KeyCode = Keys.Enter Then
            Dim newPage As Integer
            ' Check if it is a number and within valid range (1 to Total)
            If Integer.TryParse(txtPageNumber.Text, newPage) AndAlso newPage >= 1 AndAlso newPage <= _totalPages Then
                _currentPage = newPage
                UpdatePaginationUI()
                ' LoadData() ' <--- Call your DB load here

                ' Remove focus to stop the "ding" sound or just to show it's done
                flpPagination.Focus()
            Else
                ' Invalid input? Reset to current page
                txtPageNumber.Text = _currentPage.ToString()
                MessageBox.Show($"Please enter a page number between 1 and {_totalPages}", "Invalid Page", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If

            ' Suppress the "Ding" sound
            e.SuppressKeyPress = True
            e.Handled = True
        End If
    End Sub

    ' Logic: If they type garbage and click away, reset the number
    Private Sub txtPageNumber_Leave(sender As Object, e As EventArgs)
        txtPageNumber.Text = _currentPage.ToString()
    End Sub

    ' --- HELPER FOR ARROW BUTTONS ---
    Private Function CreateNavButton(text As String) As Guna2Button
        Dim btn As New Guna2Button()
        With btn
            .Text = text
            .Size = New Size(45, 38)
            .BorderRadius = 4
            .FillColor = Color.FromArgb(240, 240, 240) ' Light gray background
            .ForeColor = Color.DimGray
            .Font = New Font("Segoe UI", 10, FontStyle.Bold)
            .Cursor = Cursors.Hand

            ' Hover Effect
            .HoverState.FillColor = Color.DodgerBlue
            .HoverState.ForeColor = Color.White
        End With
        Return btn
    End Function

    ' --- DataGridView Setup (Kept as is) ---
    Private Sub SetupDgvBarangayResidents()
        If dgvBarangayResidents Is Nothing Then Return

        dgvBarangayResidents.Rows.Clear()
        dgvBarangayResidents.Columns.Clear()
        dgvBarangayResidents.AllowUserToAddRows = False
        dgvBarangayResidents.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvBarangayResidents.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill

        Dim colName As New DataGridViewTextBoxColumn With {.Name = "Name", .HeaderText = "Name", .ReadOnly = True}
        Dim colStatus As New DataGridViewTextBoxColumn With {.Name = "Status", .HeaderText = "Status", .ReadOnly = True}
        Dim colPhone As New DataGridViewTextBoxColumn With {.Name = "Phone", .HeaderText = "Phone", .ReadOnly = True}
        Dim colAddress As New DataGridViewTextBoxColumn With {.Name = "Address", .HeaderText = "Address", .ReadOnly = True}
        Dim colEvacCenter As New DataGridViewTextBoxColumn With {.Name = "EvacuationCenter", .HeaderText = "Evacuation Center", .ReadOnly = True}

        dgvBarangayResidents.Columns.AddRange({colName, colStatus, colPhone, colAddress, colEvacCenter})

        Dim colAction As New DataGridViewButtonColumn With {
            .Name = "Action",
            .HeaderText = "Action",
            .Text = "View",
            .UseColumnTextForButtonValue = True
        }
        dgvBarangayResidents.Columns.Add(colAction)

        ' Sample Data
        dgvBarangayResidents.Rows.Add({"Juan Dela Cruz", "Evacuated", "09145794410", "436 Bonifacio St.", "Barangay Bagong Pag-asa Covered Court"})
        dgvBarangayResidents.Rows.Add({"Juan Dela Cruz", "Evacuated", "09145794410", "436 Bonifacio St.", "Barangay Bagong Pag-asa Covered Court"})
        dgvBarangayResidents.Rows.Add({"Juan Dela Cruz", "Evacuated", "09145794410", "436 Bonifacio St.", "Barangay Bagong Pag-asa Covered Court"})
        dgvBarangayResidents.Rows.Add({"Juan Dela Cruz", "Evacuated", "09145794410", "436 Bonifacio St.", "Barangay Bagong Pag-asa Covered Court"})
        dgvBarangayResidents.Rows.Add({"Juan Dela Cruz", "Evacuated", "09145794410", "436 Bonifacio St.", "Barangay Bagong Pag-asa Covered Court"})
        dgvBarangayResidents.Rows.Add({"Juan Dela Cruz", "Evacuated", "09145794410", "436 Bonifacio St.", "Barangay Bagong Pag-asa Covered Court"})
        dgvBarangayResidents.Rows.Add({"Juan Dela Cruz", "Evacuated", "09145794410", "436 Bonifacio St.", "Barangay Bagong Pag-asa Covered Court"})
        dgvBarangayResidents.Rows.Add({"Juan Dela Cruz", "Evacuated", "09145794410", "436 Bonifacio St.", "Barangay Bagong Pag-asa Covered Court"})
        dgvBarangayResidents.Rows.Add({"Juan Dela Cruz", "Evacuated", "09145794410", "436 Bonifacio St.", "Barangay Bagong Pag-asa Covered Court"})
        dgvBarangayResidents.Rows.Add({"Juan Dela Cruz", "Evacuated", "09145794410", "436 Bonifacio St.", "Barangay Bagong Pag-asa Covered Court"})
    End Sub

    Private Sub dgvBarangayResidents_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvBarangayResidents.CellContentClick
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return

        If TypeOf dgvBarangayResidents.Columns(e.ColumnIndex) Is DataGridViewButtonColumn Then
            Dim residentName = Convert.ToString(dgvBarangayResidents.Rows(e.RowIndex).Cells("Name").Value)
            MessageBox.Show($"Button clicked for: {residentName}", "Action", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub Label12_Click(sender As Object, e As EventArgs) Handles Label12.Click

    End Sub

    Private Sub pnlMain_Paint(sender As Object, e As PaintEventArgs) Handles pnlMain.Paint

    End Sub
End Class
