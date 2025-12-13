Imports System.Net
Imports System.Net.Http
Imports System.Timers

Public Class SensorsUserControl

    Private sensorTimer As System.Timers.Timer
    Private receiverIP As String = "10.148.172.199" ' Your Receiver IP
    Private http As New HttpClient()

    Private Sub SensorsUserControl_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Auto update every 3 seconds
        sensorTimer = New System.Timers.Timer(3000)
        AddHandler sensorTimer.Elapsed, AddressOf UpdateSensorData
        sensorTimer.Start()

        ' ts is for the label, this does not need to be changed so don't remove it plz. if you do, i will find you.
        lblDateTime.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy — hh:mm tt")
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
End Class