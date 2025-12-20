Imports System.Windows

Imports System.Windows.Forms.VisualStyles.VisualStyleElement

Imports Microsoft.Data.SqlClient

Imports Microsoft.Web.WebView2.Core

Imports Microsoft.Web.WebView2.WinForms

Imports System.Drawing.Drawing2D

Public Class MappingUserControl

    Private mapLoaded As Boolean = False

    Private WithEvents refreshTimer As New Timer()

    Private routingActive As Boolean = False

    Private selectedLat As Double?
    Private selectedLng As Double?

    Private pinSelected As Boolean = False

    Private Const MAP_BORDER_RADIUS As Integer = 10

    Private ReadOnly defaultLat As Double = 14.662315
    Private ReadOnly defaultLng As Double = 121.034256
    Private ReadOnly defaultZoom As Integer = 15

    Private connectionString As String =
        "Server=DESKTOP-011N7DN;" &
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

            ' ❌ REMOVE THIS LINE
            ' routingActive = True

            ' Populate labels
            lblFamID.Text = msg("familyID").ToString()
            lblSurName.Text = msg("lastName").ToString()
            lblFamAdd.Text = msg("address").ToString()
            lblTimStamp.Text = msg("statusDate").ToString()
            lblStatus.Text = msg("status").ToString()
            lblContactNo.Text = msg("contactno").ToString()
            lblDeclaredBy.Text = msg("declaredby").ToString()

        End Sub

        LoadMap()

    End Sub

    Private Sub Guna2PanelMapContainer_Resize(sender As Object, e As EventArgs) _
    Handles GunaPanelMapping.Resize ' Ensure you link this handler in your designer!
        ClipWebView2(GunaPanelMapping, MAP_BORDER_RADIUS)
    End Sub

    ''' <summary>
    ''' Clips the region of the WebView2 control to match the parent Guna2Panel's rounded corners.
    ''' </summary>
    Private Sub ClipWebView2(parentPanel As Control, radius As Integer)
        If parentPanel.Controls.Contains(WebView2Mapping) Then

            Dim path As New GraphicsPath()
            Dim rect As Rectangle = parentPanel.ClientRectangle

            ' Adjust rectangle dimensions for accurate clipping
            rect.Width -= 1
            rect.Height -= 1

            ' Define the rounded rectangle path
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90) ' Top-Left
            path.AddArc(rect.Width - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90) ' Top-Right
            path.AddArc(rect.Width - radius * 2, rect.Height - radius * 2, radius * 2, radius * 2, 0, 90) ' Bottom-Right
            path.AddArc(rect.X, rect.Height - radius * 2, radius * 2, radius * 2, 90, 90) ' Bottom-Left
            path.CloseAllFigures()

            ' Apply the region to the WebView2 control
            WebView2Mapping.Region = New Region(path)

            path.Dispose()
        End If
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
    // Default center: Barangay Bagong Pag-asa
    window.map = L.map('map').setView([14.6623054, 121.0341577], 15);

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
            "SELECT FamilyID, LastName, Address, Latitude, Longitude, StatusDate, Status, ContactNum, DeclaredBy " &
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
                        If(CBool(dr("Status")) = False, "Need Help", "Safe")

                        Dim contactno As String = dr("ContactNum").ToString()

                        Dim declaredby As String = dr("DeclaredBy").ToString()

                        AddUnsafePin(
                        familyID,
                        lastName,
                        address,
                        lat,
                        lng,
                        statusDate,
                        statusText,
                        contactno,
                        declaredby
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
    statusText As String,
    contactno As String,
    declaredby As String
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
        status: '{statusText}',
        contactno: '{contactno}',
        declaredby: '{declaredby}'
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

        Dim startLat = defaultLat
        Dim startLng = defaultLng
        Dim endLat = selectedLat
        Dim endLng = selectedLng

        Dim url =
        $"https://router.project-osrm.org/route/v1/driving/{startLng},{startLat};{endLng},{endLat}?overview=full&geometries=geojson"

        Using http As New Net.Http.HttpClient()
            Dim json = Await http.GetStringAsync(url)
            Dim obj = Newtonsoft.Json.Linq.JObject.Parse(json)

            ' 🧮 DISTANCE & ETA
            Dim distanceMeters = CDbl(obj("routes")(0)("distance"))
            Dim durationSeconds = CDbl(obj("routes")(0)("duration"))

            Dim distanceKm = Math.Round(distanceMeters / 1000, 2)
            Dim etaMinutes = Math.Round(durationSeconds / 60, 1)

            lblDistance.Text = $"{distanceKm} km"
            lblETA.Text = $"{etaMinutes} mins"

            ' 🗺️ ROUTE LINE
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

    Private Async Sub ResetMapView()

        If WebView2Mapping.CoreWebView2 Is Nothing Then Exit Sub

        Await WebView2Mapping.ExecuteScriptAsync(
    $"
    if (window.map) {{
        window.map.setView([{defaultLat},{defaultLng}], {defaultZoom});
    }}
    ")
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

    Private Sub Guna2Button1_Click(sender As Object, e As EventArgs)
        DrawRoute()
    End Sub

    Private Sub btnDirections_Click(sender As Object, e As EventArgs) Handles btnDirections.Click

        If Not pinSelected Then
            MessageBox.Show("Please select a pin first.", "No Pin Selected")
            Exit Sub
        End If

        routingActive = True

        DrawRoute()

        btnClear.Visible = True
        btnDirections.Visible = False
        btnDone.Visible = True
    End Sub

    Private Async Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click

        routingActive = False
        pinSelected = False

        selectedLat = Nothing
        selectedLng = Nothing

        lblDistance.Text = ""
        lblETA.Text = ""

        If WebView2Mapping.CoreWebView2 Is Nothing Then Exit Sub

        Await WebView2Mapping.ExecuteScriptAsync(
    "
    if (window.routeLine) {
        window.map.removeLayer(window.routeLine);
        window.routeLine = null;
    }

    if (window.selectedMarker) {
        window.selectedMarker.setIcon(window.redIcon);
        window.selectedMarker = null;
    }
    ")

        ResetMapView()

        btnClear.Visible = False
        btnDone.Visible = True
        btnDirections.Visible = True
    End Sub

    Private Sub btnDone_Click(sender As Object, e As EventArgs) Handles btnDone.Click

        If Not pinSelected Then
            MessageBox.Show("Please select a pin first.", "No Pin Selected")
            Exit Sub
        End If

        If lblFamID.Text = "" Then Exit Sub

        Using con As New SqlConnection(connectionString)
            con.Open()

            Dim sql As String =
            "UPDATE SafeStatus SET Status = 1 WHERE FamilyID = @id"

            Using cmd As New SqlCommand(sql, con)
                cmd.Parameters.AddWithValue("@id", lblFamID.Text)
                cmd.ExecuteNonQuery()
            End Using
        End Using

        ' Reset UI
        lblFamID.Text = ""
        lblSurName.Text = ""
        lblFamAdd.Text = ""
        lblTimStamp.Text = ""
        lblStatus.Text = ""
        lblContactNo.Text = ""
        lblDeclaredBy.Text = ""
        lblDistance.Text = ""
        lblETA.Text = ""

        routingActive = False
        pinSelected = False

        ' Remove route + reset map
        btnClear.PerformClick()
        ResetMapView()

        ' Refresh map
        ClearPins()
        LoadUnsafePins()

        btnClear.Visible = False
        btnDone.Visible = True
        btnDirections.Visible = True

        If Not pinSelected Then
            MessageBox.Show("Resident successfully rescued and marked as safe.")
            Exit Sub
        End If
    End Sub
End Class