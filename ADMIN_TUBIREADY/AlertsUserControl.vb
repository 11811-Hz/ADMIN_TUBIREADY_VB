Imports System.Net.Http
Imports System.Threading.Tasks
Imports Microsoft.Data.SqlClient
Imports MongoDB.Driver.Core.Configuration
Imports Newtonsoft.Json.Linq

Public Class AlertsUserControl
    Public connectionString As String = "server=DESKTOP-011N7DN;user id=TubiReadyAdmin;password=123456789;database=TubiReadyDB;TrustServerCertificate=True"
    ' Changed "otp" to "messageContent"
    Public Async Function SendGeneralSMS(phone As String, messageContent As String) As Task(Of Boolean)
        Dim baseUrl As String = "https://www.iprogsms.com/api/v1/sms_messages"
        Dim fullNumber As String = NormalizePhone(phone)
        Dim apiToken As String = "8391dc35a8be121f396e2d1756cbf2f2999a2e59"

        ' --- CHANGE: Use the message content directly ---
        Dim values = New List(Of KeyValuePair(Of String, String)) From {
        New KeyValuePair(Of String, String)("api_token", apiToken),
        New KeyValuePair(Of String, String)("phone_number", fullNumber),
        New KeyValuePair(Of String, String)("message", messageContent),
        New KeyValuePair(Of String, String)("sms_provider", "0")
    }

        Using client As New HttpClient()
            Dim content = New FormUrlEncodedContent(values)
            Dim response = Await client.PostAsync(baseUrl, content)
            Dim json As String = Await response.Content.ReadAsStringAsync()

            If Not response.IsSuccessStatusCode Then
                ' Log error but don't crash the app
                Debug.WriteLine($"Failed to send to {phone}: {json}")
                Return False
            End If

            Dim data As JObject = Nothing
            Try
                data = JObject.Parse(json)
            Catch ex As Exception
                Throw New Exception("Failed to parse IPROG response JSON: " & ex.Message)
            End Try

            ' Handle status that might be number or string
            Dim statusToken = data("status")
            If statusToken Is Nothing Then
                Throw New Exception("IPROG response missing 'status': " & json)
            End If

            Dim statusInt As Integer = -1
            If Integer.TryParse(statusToken.ToString(), statusInt) Then
                If statusInt <> 200 Then
                    Throw New Exception(data("message")?.ToString() Or $"IPROG returned status {statusInt}")
                End If
            Else
                ' If API returns a non-numeric status, treat non-success as error
                Dim statusStr = statusToken.ToString()
                If Not statusStr.Equals("success", StringComparison.OrdinalIgnoreCase) AndAlso Not statusStr.Equals("ok", StringComparison.OrdinalIgnoreCase) Then
                    Throw New Exception("IPROG returned status: " & statusStr & " - " & (data("message")?.ToString() Or json))
                End If
            End If

            Debug.WriteLine("SMS sent id: " & (data("message_id")?.ToString()))
            Return True
        End Using
    End Function

    Private Sub LoadStreets()
        ' 1. SQL Query to get unique streets
        Dim query As String = "SELECT DISTINCT street FROM Residents ORDER BY street ASC"

        Using conn As New SqlConnection(connectionString)
            Try
                conn.Open()
                Using cmd As New SqlCommand(query, conn)
                    Using reader As SqlDataReader = cmd.ExecuteReader()
                        ' Clear existing items just in case
                        cboStreet.Items.Clear()

                        ' Add a default placeholder option
                        cboStreet.Items.Add("-- Select Street --")

                        ' 2. Loop through results and add to ComboBox
                        While reader.Read()
                            ' Check if value is not null to avoid errors
                            If Not IsDBNull(reader("street")) Then
                                cboStreet.Items.Add(reader("street").ToString())
                            End If
                        End While
                    End Using
                End Using

                ' Set default selection to the first item ("-- Select Street --")
                cboStreet.SelectedIndex = 0

            Catch ex As Exception
                MessageBox.Show("Error loading streets: " & ex.Message)
            End Try
        End Using
    End Sub

    ' 1. LOG THE MAIN BROADCAST EVENT (Header)
    Private Function CreateBroadcastLog(msgBody As String, targetGroup As String) As Integer
        Dim newID As Integer = 0

        Dim sql As String = "INSERT INTO SMS_Broadcast (User_ID, TargetGroup, MessageBody, Status, BroadcastTime, MessageType) " &
                        "VALUES (@UID, @Target, @Msg, 'Sent', GETDATE(), 'SMS'); " &
                        "SELECT SCOPE_IDENTITY();" ' This gets the ID of the row we just made

        Using conn As New SqlConnection(connectionString)
            conn.Open()
            Using cmd As New SqlCommand(sql, conn)
                ' HARDCODED User_ID = 1 (Admin). Change this if you have a login system variable.
                cmd.Parameters.AddWithValue("@UID", 1)
                cmd.Parameters.AddWithValue("@Target", targetGroup)
                cmd.Parameters.AddWithValue("@Msg", msgBody)

                ' Execute and get the new Broadcast_ID
                newID = Convert.ToInt32(cmd.ExecuteScalar())
            End Using
        End Using

        Return newID
    End Function

    ' 2. LOG THE INDIVIDUAL RECIPIENT (Detail)
    Private Sub LogBroadcastDetail(broadcastID As Integer, residentID As Integer, success As Boolean)
        ' Assuming Table Name is 'BroadcastLogs' - Change if needed
        Dim sql As String = "INSERT INTO SMS_RECIPIENTS (Broadcast_ID, Resident_ID, DeliveryStatus) " &
                        "VALUES (@BID, @RID, @Status)"

        Using conn As New SqlConnection(connectionString)
            conn.Open()
            Using cmd As New SqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@BID", broadcastID)
                cmd.Parameters.AddWithValue("@RID", residentID)

                If success Then
                    cmd.Parameters.AddWithValue("@Status", "Sent")
                Else
                    cmd.Parameters.AddWithValue("@Status", "Failed")
                End If

                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Private Sub LoadResidents()
        ' 1. Clear the current list to avoid duplicates
        flpContact.Controls.Clear()

        ' Performance: Stop drawing the layout while we add items (makes it much faster)
        flpContact.SuspendLayout()

        Using conn As New SqlConnection(connectionString)
            Try
                conn.Open()

                ' 2. Start building the SQL Query
                ' We start with "WHERE 1=1" so we can easily append "AND..." conditions
                Dim sql As String = "SELECT * FROM Residents WHERE 1=1"

                ' --- FILTER 1: STREET ---
                ' Only filter if a street is actually selected and it's not the placeholder
                If cboStreet.SelectedIndex > 0 Then
                    sql &= " AND street = @Street"
                End If

                ' --- FILTER 2: SEARCH TEXT ---
                ' Map the "Search By" dropdown to real Database Column names
                Dim searchColumn As String = ""
                If txtSearch.Text.Trim() <> "" AndAlso cboSearchBy.SelectedIndex >= 0 Then
                    Select Case cboSearchBy.Text
                        Case "Surname"
                            searchColumn = "lastname" ' CHANGE THIS to your real column name
                        Case "First Name"
                            searchColumn = "firstname" ' CHANGE THIS to your real column name
                        Case "Phone"
                            searchColumn = "mobilenumber" ' CHANGE THIS to your real column name
                    End Select

                    If searchColumn <> "" Then
                        sql &= " AND " & searchColumn & " LIKE @Search"
                    End If
                End If

                ' 3. Execute Query
                Using cmd As New SqlCommand(sql, conn)

                    ' Add Parameters (Prevents SQL Injection and errors)
                    If cboStreet.SelectedIndex > 0 Then
                        cmd.Parameters.AddWithValue("@Street", cboStreet.Text)
                    End If

                    If searchColumn <> "" Then
                        ' The % symbols are for the "LIKE" wildcard search
                        cmd.Parameters.AddWithValue("@Search", "%" & txtSearch.Text.Trim() & "%")
                    End If

                    Using reader As SqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            ' 4. Create the User Control for each row
                            Dim card As New UserControlAlerts()

                            ' Combine names if you have separate columns, or just grab the FullName
                            Dim fName As String = reader("firstname").ToString()
                            Dim lName As String = reader("lastname").ToString()

                            card.ResidentID = Convert.ToInt32(reader("id"))
                            card.ResidentName = fName & " " & lName
                            card.PhoneNumber = reader("MobileNumber").ToString()

                            ' Add to the FlowLayoutPanel
                            flpContact.Controls.Add(card)
                        End While
                    End Using
                End Using

            Catch ex As Exception
                MessageBox.Show("Error loading residents: " & ex.Message)
            Finally
                ' Resume drawing the layout
                flpContact.ResumeLayout()
            End Try
        End Using
    End Sub

    Private Sub UpdateSelectionCount()
        Dim total As Integer = flpContact.Controls.Count
        Dim selected As Integer = 0

        For Each ctrl As Control In flpContact.Controls
            If TypeOf ctrl Is UserControlAlerts Then
                Dim card As UserControlAlerts = DirectCast(ctrl, UserControlAlerts)
                If card.IsSelected Then
                    selected += 1
                End If
            End If
        Next

        ' Assuming your counter label is named lblCount
        lblCount.Text = selected.ToString() & "/" & total.ToString()
    End Sub

    Private Sub txtBroadcastMessage_TextChanged(sender As Object, e As EventArgs) Handles txtMessage.TextChanged
        ' ts just changes the character count label as you type
        lblCharCount.Text = txtMessage.Text.Length.ToString() & " Characters"
    End Sub

    Private Sub AlertsUserControl_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' TODO ADD CONNECTION TO DATABASE TO PULL SEARCH RESULTS
        ' REMOVE THIS AND POPULATE WITH ACTUAL DATA FROM DATABASE
        LoadStreets()
    End Sub

    Private Async Sub btnSendBroadcast_Click(sender As Object, e As EventArgs) Handles btnSendBroadcast.Click
        Dim msg As String = txtMessage.Text.Trim()

        If String.IsNullOrEmpty(msg) Then
            MessageBox.Show("Please enter a message.")
            Return
        End If

        btnSendBroadcast.Enabled = False
        btnSendBroadcast.Text = "Sending..."

        Try
            ' 1. Determine Target Group Name (for the log)
            Dim targetName As String = "Selected Residents"
            If cboStreet.SelectedIndex > 0 Then
                targetName = "Street: " & cboStreet.Text
            End If

            ' 2. CREATE MASTER LOG (Table 1)
            Dim broadcastID As Integer = CreateBroadcastLog(msg, targetName)

            Dim successCount As Integer = 0
            Dim failCount As Integer = 0

            ' 3. LOOP AND SEND
            For Each ctrl As Control In flpContact.Controls
                If TypeOf ctrl Is UserControlAlerts Then
                    Dim card As UserControlAlerts = DirectCast(ctrl, UserControlAlerts)

                    If card.IsSelected Then
                        Dim isSent As Boolean = False

                        Try
                            ' Send the SMS
                            isSent = Await SendGeneralSMS(card.PhoneNumber, msg)
                        Catch ex As Exception
                            Debug.WriteLine($"SMS Error for {card.ResidentName}: {ex.Message}")
                            isSent = False
                        End Try

                        ' 4. LOG INDIVIDUAL RESULT (Table 2)
                        ' We pass the Master BroadcastID and the specific ResidentID
                        LogBroadcastDetail(broadcastID, card.ResidentID, isSent)

                        If isSent Then
                            successCount += 1
                        Else
                            failCount += 1
                        End If

                        ' Small delay to prevent API flooding
                        Await Task.Delay(100)
                    End If
                End If
            Next

            MessageBox.Show($"Broadcast Complete!" & vbCrLf &
                        $"Sent: {successCount}" & vbCrLf &
                        $"Failed: {failCount}")

        Catch ex As Exception
            MessageBox.Show("Critical Error during broadcast: " & ex.Message)
        Finally
            btnSendBroadcast.Enabled = True
            btnSendBroadcast.Text = "Send to Recipients"
        End Try
    End Sub

    Private Function NormalizePhone(phone As String) As String
        If String.IsNullOrWhiteSpace(phone) Then
            Throw New ArgumentException("Phone is required", NameOf(phone))
        End If
        Dim normalized = phone.Trim().Replace(" "c, "")
        If normalized.StartsWith("+") Then normalized = normalized.Substring(1)
        If normalized.StartsWith("0") Then normalized = normalized.TrimStart("0"c)
        Return "63" & normalized
    End Function

    Private Sub Guna2TextBox1_TextChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        LoadResidents()
        UpdateSelectionCount()
    End Sub

    Private Sub cboSearchBy_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboSearchBy.SelectedIndexChanged
        LoadResidents()
        UpdateSelectionCount()
    End Sub

    Private Sub cboStreet_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboStreet.SelectedIndexChanged
        LoadResidents()
        UpdateSelectionCount()
    End Sub

    Private Sub chkSelectAll_CheckedChanged(sender As Object, e As EventArgs) Handles chkSelectAll.CheckedChanged
        ' Loop through every user card in the flow layout panel
        For Each ctrl As Control In flpContact.Controls
            ' Check if the control is actually one of our resident cards
            If TypeOf ctrl Is UserControlAlerts Then
                ' Turn the card into a variable we can use
                Dim card As UserControlAlerts = DirectCast(ctrl, UserControlAlerts)

                ' Set the card's checkbox to match the "Select All" checkbox
                card.IsSelected = chkSelectAll.Checked
            End If
        Next

        ' Optional: Update the counter (e.g., "3/3 Selected")
        UpdateSelectionCount()
    End Sub
End Class
