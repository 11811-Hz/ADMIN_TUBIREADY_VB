Imports System.Net.Http
Imports System.Threading.Tasks
Imports Newtonsoft.Json.Linq

Public Class AlertsUserControl

    Public Async Function SendOtpSMS(phone As String, otp As String) As Task(Of Boolean)
        Dim baseUrl As String = "https://www.iprogsms.com/api/v1/sms_messages"

        ' Normalize phone: remove spaces, +, leading zeros, then prefix country code 63
        Dim fullNumber As String = NormalizePhone(phone)

        Dim apiToken As String = "8391dc35a8be121f396e2d1756cbf2f2999a2e59"
        Dim messageText As String = $"Your verification code is: {otp}"

        Dim values = New List(Of KeyValuePair(Of String, String)) From {
            New KeyValuePair(Of String, String)("api_token", apiToken),
            New KeyValuePair(Of String, String)("phone_number", fullNumber),
            New KeyValuePair(Of String, String)("message", messageText),
            New KeyValuePair(Of String, String)("sms_provider", "0")
        }

        Using client As New HttpClient()
            Dim content = New FormUrlEncodedContent(values)
            Dim response = Await client.PostAsync(baseUrl, content)

            Dim json As String = Await response.Content.ReadAsStringAsync()
            ' Log raw response for debugging
            Debug.WriteLine("IPROG response status: " & response.StatusCode.ToString())
            Debug.WriteLine("IPROG response body: " & json)

            If Not response.IsSuccessStatusCode Then
                Throw New Exception($"HTTP {(CInt(response.StatusCode))}: {response.ReasonPhrase} - {json}")
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


    Private Sub txtBroadcastMessage_TextChanged(sender As Object, e As EventArgs) Handles txtBroadcastMessage.TextChanged
        ' ts just changes the character count label as you type
        lblCharCount.Text = txtBroadcastMessage.Text.Length.ToString() & " Characters"
    End Sub

    Private Sub AlertsUserControl_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' TODO ADD CONNECTION TO DATABASE TO PULL SEARCH RESULTS
        ' REMOVE THIS AND POPULATE WITH ACTUAL DATA FROM DATABASE

        ' Avoid running at design time
        If Me.DesignMode Then
            Return
        End If

        ' Prefer the known FlowLayoutPanel name "flpContacts"
        Dim flp As FlowLayoutPanel = Nothing
        Dim found() As Control = Me.Controls.Find("flpContact", True)
        If found IsNot Nothing AndAlso found.Length > 0 AndAlso TypeOf found(0) Is FlowLayoutPanel Then
            flp = CType(found(0), FlowLayoutPanel)
        Else
            ' Known name not present - nothing to do
            Return
        End If

        ' Create and add 3 instances of UserControlAlerts for testing
        Dim count As Integer = 3
        For i As Integer = 1 To count
            Dim uc As New UserControlAlerts()
            uc.Name = "UserControlAlert" & i.ToString()
            uc.Margin = New Padding(6) ' slight spacing for visual clarity
            flp.Controls.Add(uc)
        Next
    End Sub

    Private Async Sub Guna2Button5_Click(sender As Object, e As EventArgs) Handles Guna2Button5.Click
        Try
            Dim phone As String = "09065867926"
            Dim otp As String = "123456"

            Dim success = Await SendOtpSMS(phone, otp)

            If success Then
                MessageBox.Show("SMS sent!" + otp)

            End If

        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message)
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
End Class
