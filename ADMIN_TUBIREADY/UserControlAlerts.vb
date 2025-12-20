Imports Org.BouncyCastle.Asn1.X509

Public Class UserControlAlerts

    Public Property ResidentName As String
        Get
            Return lblName.Text ' Assuming your big label is named lblName
        End Get
        Set(value As String)
            lblName.Text = value
        End Set
    End Property

    Public Property PhoneNumber As String
        Get
            Return lblNumber.Text ' Assuming your small label is named lblNumber
        End Get
        Set(value As String)
            lblNumber.Text = value
        End Set
    End Property

    Public Property IsSelected As Boolean
        Get
            Return chkResidents.Checked ' Change CheckBox1 to your real checkbox name
        End Get
        Set(value As Boolean)
            chkResidents.Checked = value
        End Set
    End Property

    ' Optional: A hidden ID if you need to know which database ID this person is
    Public Property ResidentID As Integer
    Private Sub UserControlAlerts_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class
