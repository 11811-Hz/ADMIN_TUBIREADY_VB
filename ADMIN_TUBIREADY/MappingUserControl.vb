Imports System.Windows

Imports System.Windows.Forms.VisualStyles.VisualStyleElement

Imports Microsoft.Data.SqlClient

Imports Microsoft.Web.WebView2.Core

Imports Microsoft.Web.WebView2.WinForms

Public Class MappingUserControl

    Private mapLoaded As Boolean = False

    Private WithEvents refreshTimer As New Timer()

    Private routingActive As Boolean = False

    Private selectedLat As Double?
    Private selectedLng As Double?

    Private pinSelected As Boolean = False

    Private connectionString As String =
        "Server=10.69.185.193\SQLEXPRESS,1433;" &
        "Database=TubiReadyDB;" &
        "User ID=TubiReadyAdmin;" &
        "Password=123456789;" &
        "TrustServerCertificate=True;"

    ' ================= LOAD =================
    Private Async Sub MappingUserControl_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Await WebView2Mapping.EnsureCoreWebView2Async()

        ' 🔓 Enable JS features
        WebView2Mapping.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = True
        WebView2Mapping.CoreWebView2.Settings.AreDevToolsEnabled = True

        ' 🔗 RECEIVE DATA FROM JAVASCRIPT (ONLY ADD ONCE)
        AddHandler WebView2Mapping.CoreWebView2.WebMessageReceived,
        Sub(sender2, args)

            Dim msg = Newtonsoft.Json.Linq.JObject.Parse(args.WebMessageAsJson)

            selectedLat = CDbl(msg("lat"))
            selectedLng = CDbl(msg("lng"))
            pinSelected = True
            routingActive = True

            ' 📝 Populate textboxes
            txtFamID.Text = msg("familyID").ToString()
            txtSurName.Text = msg("lastName").ToString()
            txtFamAdd.Text = msg("address").ToString()
            txtTimStamp.Text = msg("statusDate").ToString()
            txtStatus.Text = msg("status").ToString()

        End Sub

        LoadMap()

    End Sub

    ' ================= LOAD MAP =================
    Private Sub LoadMap()

        Dim html As String =
"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<title>TubiReady Map</title>

<link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css'/>

<script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>

<style>
html, body {
    margin: 0;
    padding: 0;
    height: 100%;
}
#map {
    width: 100%;
    height: 100%;
}
</style>

</head>

<body>

<div id='map'></div>

<script>
    // Default center: Barangay Bagong Pag-asa (Based on your request)
    window.map = L.map('map').setView([14.6550586, 121.0251324], 15);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(window.map);

    window.markerLayer = L.layerGroup().addTo(window.map);

    // Selected destination pin
    window.selectedLatLng = null;

    // Routing control
    window.routeControl = null;

    // 🚑 START LOCATION (SK Academy Building)
    window.startLatLng = L.latLng(14.662315, 121.034256);

    // 🟢 Green marker icon
    var greenIcon = L.icon({
        iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-green.png',
        shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
        iconSize: [25, 41],
        iconAnchor: [12, 41],
        popupAnchor: [1, -34]
    });

    // Add SK Academy Building marker
    L.marker(window.startLatLng, { icon: greenIcon })
        .addTo(window.map)
        .bindPopup('<b>SK Academy Building</b><br>Response Starting Point');

window.redIcon = L.icon({
    iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-red.png',
    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
    iconSize: [25, 41],
    iconAnchor: [12, 41]
});

window.blueIcon = L.icon({
    iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-blue.png',
    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
    iconSize: [25, 41],
    iconAnchor: [12, 41]
});
</script>

</body>
</html>"

        WebView2Mapping.NavigateToString(html)

    End Sub

    ' ================= MAP READY =================
    Private Sub WebView2Mapping_NavigationCompleted(
    sender As Object,
    e As CoreWebView2NavigationCompletedEventArgs
) Handles WebView2Mapping.NavigationCompleted

        If mapLoaded Then Exit Sub
        mapLoaded = True

        LoadUnsafePins()

        ' Start auto-refresh ONLY AFTER map is ready
        refreshTimer.Interval = 5000
        refreshTimer.Start()

    End Sub

    ' ================= LOAD UNSAFE USERS =================
    Private Sub LoadUnsafePins()

        Using con As New SqlConnection(connectionString)
            con.Open()

            Dim sql As String =
            "SELECT FamilyID, LastName, Address, Latitude, Longitude, StatusDate, Status " &
            "FROM SafeStatus " &
            "WHERE Status = 0"

            Using cmd As New SqlCommand(sql, con)
                Using dr As SqlDataReader = cmd.ExecuteReader()

                    While dr.Read()

                        Dim familyID As String =
                        If(IsDBNull(dr("FamilyID")), "", dr("FamilyID").ToString())

                        Dim lastName As String = dr("LastName").ToString()

                        Dim address As String =
                        If(IsDBNull(dr("Address")),
                           "Address not available",
                           dr("Address").ToString())

                        Dim lat As String = dr("Latitude").ToString()
                        Dim lng As String = dr("Longitude").ToString()

                        Dim statusDate As String =
                        Convert.ToDateTime(dr("StatusDate")).ToString("yyyy-MM-dd HH:mm")

                        Dim statusText As String =
                        If(CBool(dr("Status")) = False, "Unsafe", "Safe")

                        AddUnsafePin(
                        familyID,
                        lastName,
                        address,
                        lat,
                        lng,
                        statusDate,
                        statusText
                    )

                    End While

                End Using
            End Using
        End Using

    End Sub


    ' ================= ADD PIN =================
    Private Async Sub AddUnsafePin(
    familyID As String,
    lastName As String,
    address As String,
    lat As String,
    lng As String,
    statusDate As String,
    statusText As String
)

        If WebView2Mapping.CoreWebView2 Is Nothing Then Exit Sub
        If Not mapLoaded Then Exit Sub

        familyID = familyID.Replace("'", "\'")
        lastName = lastName.Replace("'", "\'")
        address = address.Replace("'", "\'")

        Dim js As String =
$"
var marker = L.marker([{lat},{lng}], {{ icon: window.redIcon }})
.addTo(window.markerLayer)
.bindPopup(
    '<b>{lastName}</b><br>' +
    '<b>Status:</b> {statusText}<br>' +
    '<b>Address:</b><br>{address}<br>' +
    '<small>Reported: {statusDate}</small>'
);

marker.on('click', function (e) {{
    var clickedMarker = e.target;

    window.chrome.webview.postMessage({{
        lat: clickedMarker.getLatLng().lat,
        lng: clickedMarker.getLatLng().lng,
        familyID: '{familyID}',
        lastName: '{lastName}',
        address: '{address}',
        statusDate: '{statusDate}',
        status: '{statusText}'
    }});

    if (window.selectedMarker) {{
        window.selectedMarker.setIcon(window.redIcon);
    }}

    clickedMarker.setIcon(window.blueIcon);
    window.selectedMarker = clickedMarker;
}});
"
        Await WebView2Mapping.ExecuteScriptAsync(js)

    End Sub

    Private Async Sub DrawRoute()

        If Not mapLoaded Then Exit Sub
        If Not pinSelected Then
            MessageBox.Show("Please click a pin first.", "No Pin Selected")
            Exit Sub
        End If

        routingActive = True

        Dim startLat = 14.662315
        Dim startLng = 121.034256
        Dim endLat = selectedLat
        Dim endLng = selectedLng

        Dim url = $"https://router.project-osrm.org/route/v1/driving/{startLng},{startLat};{endLng},{endLat}?overview=full&geometries=geojson"

        Using http As New Net.Http.HttpClient()
            Dim json = Await http.GetStringAsync(url)

            Dim obj = Newtonsoft.Json.Linq.JObject.Parse(json)
            Dim coords = obj("routes")(0)("geometry")("coordinates")

            Dim jsCoords As New Text.StringBuilder("[")
            For Each c In coords
                jsCoords.Append($"[{c(1)},{c(0)}],")
            Next
            jsCoords.Length -= 1
            jsCoords.Append("]")

            Dim js As String =
$"
if (window.routeLine) {{
    window.map.removeLayer(window.routeLine);
}}

var coords = {jsCoords};

window.routeLine = L.polyline(coords, {{
    color: 'blue',
    weight: 5
}}).addTo(window.map);

window.map.fitBounds(window.routeLine.getBounds());
"

            Await WebView2Mapping.ExecuteScriptAsync(js)
        End Using

    End Sub

    Private Async Sub ClearPins()

        If WebView2Mapping.CoreWebView2 Is Nothing Then Exit Sub
        If Not mapLoaded Then Exit Sub

        Await WebView2Mapping.ExecuteScriptAsync(
        "if (window.markerLayer) { window.markerLayer.clearLayers(); }")

    End Sub

    Private Sub refreshTimer_Tick(sender As Object, e As EventArgs) _
Handles refreshTimer.Tick

        If Not mapLoaded Then Exit Sub
        If routingActive Then Exit Sub

        ' 🔥 RESET selection
        pinSelected = False
        selectedLat = Nothing
        selectedLng = Nothing

        ClearPins()
        LoadUnsafePins()

    End Sub

    Private Sub Guna2Button1_Click(sender As Object, e As EventArgs) Handles btnDirection.Click
        DrawRoute()
    End Sub

    Private Async Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click

        routingActive = False
        pinSelected = False
        selectedLat = Nothing
        selectedLng = Nothing

        If WebView2Mapping.CoreWebView2 Is Nothing Then Exit Sub

        Await WebView2Mapping.ExecuteScriptAsync(
        "
    // Remove route
    if (window.routeLine) {
        window.map.removeLayer(window.routeLine);
        window.routeLine = null;
    }

    // Reset selected marker color
    if (window.selectedMarker) {
        window.selectedMarker.setIcon(window.redIcon);
        window.selectedMarker = null;
    }

    // 🔍 ZOOM OUT + RECENTER MAP
    window.map.setView([14.6550586, 121.0251324], 15);
    ")

    End Sub

    Private Sub txtFamID_TextChanged(sender As Object, e As EventArgs) Handles txtFamID.TextChanged

    End Sub

    Private Sub txtSurName_TextChanged(sender As Object, e As EventArgs) Handles txtSurName.TextChanged

    End Sub

    Private Sub txtFamAdd_TextChanged(sender As Object, e As EventArgs) Handles txtFamAdd.TextChanged

    End Sub

    Private Sub txtTimStamp_TextChanged(sender As Object, e As EventArgs) Handles txtTimStamp.TextChanged

    End Sub

    Private Sub txtStatus_TextChanged(sender As Object, e As EventArgs) Handles txtStatus.TextChanged

    End Sub
End Class