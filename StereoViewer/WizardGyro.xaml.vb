' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class WizardGyro
    Inherits Page

    Private miCountdown As Integer
    Private miTimer As DispatcherTimer
    Private moAccel As Windows.Devices.Sensors.Accelerometer
    Private moAccelLast As Windows.Devices.Sensors.AccelerometerReading = Nothing

    Private oBrushJasny As Brush = New SolidColorBrush(Windows.UI.Colors.DarkSeaGreen)
    Private oBrushCiemny As Brush = New SolidColorBrush(Windows.UI.Colors.DarkGray)

    Shared mTxtTmp As String

    Private Sub ShowVal_Int()
        uiLeft.Text = mTxtTmp
        uiRight.Text = mTxtTmp
    End Sub

    Private Sub ShowVal(sTxt As String)
        mTxtTmp = sTxt
        Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, AddressOf ShowVal_Int)
    End Sub

    Private Sub FlashMe_Int()
        uiGridLeft.Background = oBrushJasny
        uiGridRight.Background = oBrushJasny
        miTimer.Start()
    End Sub

    Private Sub FlashMe()
        Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, AddressOf FlashMe_Int)
    End Sub

    Private Sub uiGo_Click(sender As Object, e As RoutedEventArgs)
        Sprzatamy()
        Me.Frame.GoBack()
    End Sub

    Private Sub FlashOff_Int()
        uiGridLeft.Background = oBrushCiemny
        uiGridRight.Background = oBrushCiemny
        miTimer.Stop()
    End Sub

    Private Sub TimerNext(sender As Object, e As Object)
        Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, AddressOf FlashOff_Int)
    End Sub

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        Dim iWidth As Integer = App.GetSettingsInt("niewidocznyPodzial", 5)
        uiSplitter.Width = New GridLength(iWidth)

        miTimer = New DispatcherTimer
        miTimer.Interval = TimeSpan.FromMilliseconds(250)
        AddHandler miTimer.Tick, AddressOf TimerNext

        AddHandler Window.Current.CoreWindow.KeyDown, AddressOf CoreWindow_KeyDown
        moAccel = Windows.Devices.Sensors.Accelerometer.GetDefault
        If moAccel IsNot Nothing Then
            moAccel.ReportInterval = 1000
            AddHandler moAccel.ReadingChanged, AddressOf AccelTick
        End If

    End Sub

    Private Sub Page_Unloaded(sender As Object, e As RoutedEventArgs)
        Sprzatamy()
    End Sub

    Private Sub AccelTick(sender As Object, e As Windows.Devices.Sensors.AccelerometerReadingChangedEventArgs)
        Dim bZmiana As Boolean = False
        Dim dZmianaX, dZmianaY, dZmianaZ, dZmianaSec As Double

        Dim dMinChange As Double = 1 / App.GetSettingsInt("accelSensitiv", 5)

        With e.Reading
            If moAccelLast IsNot Nothing Then

                dZmianaX = .AccelerationX - moAccelLast.AccelerationX
                dZmianaY = .AccelerationY - moAccelLast.AccelerationY
                dZmianaZ = .AccelerationZ - moAccelLast.AccelerationZ
                dZmianaSec = .Timestamp.ToUnixTimeSeconds - moAccelLast.Timestamp.ToUnixTimeSeconds
                If dZmianaSec < 5 Then Exit Sub  ' interesuje nas POCZATEK ruchu

                If Math.Abs(dZmianaX) > dMinChange Then bZmiana = True
                If Math.Abs(dZmianaY) > dMinChange Then bZmiana = True
                If Math.Abs(dZmianaZ) > dMinChange Then bZmiana = True
            End If

            moAccelLast = e.Reading
            Dim dMax As Double = Math.Max(Math.Max(Math.Abs(dZmianaX), Math.Abs(dZmianaY)), Math.Abs(dZmianaZ))
            ShowVal(dMax.ToString("##.##"))
            If Not bZmiana Then Exit Sub

            FlashMe()

        End With

    End Sub


    Public Sub CoreWindow_KeyDown(sender As Windows.UI.Core.CoreWindow, args As Windows.UI.Core.KeyEventArgs)
        VirtKeyDown(args.VirtualKey)
    End Sub

    Private Sub ZmianaRozmiaru(iIle As Integer)
        Dim iAct As Integer = App.GetSettingsInt("accelSensitiv", 5)
        iAct = iAct + iIle
        iAct = Math.Min(10, iAct)
        iAct = Math.Max(0, iAct)
        App.SetSettingsInt("accelSensitiv", iAct)
    End Sub

    Private Sub VirtKeyDown(oKey As Windows.System.VirtualKey)
        Debug.WriteLine("VirtKey" & oKey.ToString)

        Select Case oKey
            Case Windows.System.VirtualKey.Z
                ZmianaRozmiaru(-1)
            Case Windows.System.VirtualKey.X
                ZmianaRozmiaru(1)
            Case Windows.System.VirtualKey.Left
                ZmianaRozmiaru(-1)
            Case Windows.System.VirtualKey.Up
                ZmianaRozmiaru(1)
            Case Windows.System.VirtualKey.Down
                ZmianaRozmiaru(-1)
            Case Windows.System.VirtualKey.Right
                ZmianaRozmiaru(1)
            Case Windows.System.VirtualKey.Space
                uiGo_Click(Nothing, Nothing)
            Case Windows.System.VirtualKey.Enter
                uiGo_Click(Nothing, Nothing)

        End Select
    End Sub

    Private Sub Sprzatamy()

        Try
            miTimer.Stop()
            RemoveHandler Window.Current.CoreWindow.KeyDown, AddressOf CoreWindow_KeyDown
            If moAccel IsNot Nothing Then
                RemoveHandler moAccel.ReadingChanged, AddressOf AccelTick
            End If
        Catch ex As Exception

        End Try

    End Sub


End Class
