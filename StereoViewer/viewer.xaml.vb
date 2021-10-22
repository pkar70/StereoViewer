' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class PicViewer
    Inherits Page

    Private miPicNo As Integer
    Private moTimer As DispatcherTimer
    Private moGyro As Windows.Devices.Sensors.Gyrometer
    Private moAccel As Windows.Devices.Sensors.Accelerometer
    Private moInclin As Windows.Devices.Sensors.Inclinometer
    Private moAccelLast As Windows.Devices.Sensors.AccelerometerReading = Nothing
    Private moDisplReq As Windows.System.Display.DisplayRequest
    Private miMenuItem As Integer = 0
    Private moBright As BrightnessOverride

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        miPicNo = 0
        moTimer = New DispatcherTimer
        AddHandler moTimer.Tick, AddressOf TimerTick
        AddHandler Window.Current.CoreWindow.KeyDown, AddressOf CoreWindow_KeyDown

        If App.GetSettingsInt("GyroSensitiv", 5) > 0 Then
            moGyro = Windows.Devices.Sensors.Gyrometer.GetDefault
            If moGyro IsNot Nothing Then
                moGyro.ReportInterval = 200
                AddHandler moGyro.ReadingChanged, AddressOf GyroTick
            Else
                'App.DialogBox("No gyroscope")
            End If
        End If

        If App.GetSettingsInt("accelSensitiv", 5) > 0 Then
            moAccel = Windows.Devices.Sensors.Accelerometer.GetDefault
            If moAccel IsNot Nothing Then
                moAccel.ReportInterval = 1000
                AddHandler moAccel.Shaken, AddressOf AccelShaken
                AddHandler moAccel.ReadingChanged, AddressOf AccelTick
            End If
        End If

        ' it gets its data from other two sensors, the accelerometer And the gyroscope
        moInclin = Windows.Devices.Sensors.Inclinometer.GetDefault
        If moInclin IsNot Nothing Then
            AddHandler moInclin.ReadingChanged, AddressOf InclinTick
        Else
            'App.DialogBox("No inclin")
        End If

        If App.IsMobile Then ApplicationView.GetForCurrentView.TryEnterFullScreenMode()
        moDisplReq = New Windows.System.Display.DisplayRequest
        moDisplReq.RequestActive()

        moBright = BrightnessOverride.GetForCurrentView
        ShowPic()
    End Sub

    Private Sub Page_Unloaded(sender As Object, e As RoutedEventArgs)
        Sprzatamy()
    End Sub
    Private Sub TimerTick(sender As Object, e As Object)
        NextPic()
    End Sub

    Private Sub AccelTick(sender As Object, e As Windows.Devices.Sensors.AccelerometerReadingChangedEventArgs)
        Dim bZmiana As Boolean = False
        Dim dZmianaX, dZmianaY, dZmianaZ, dZmianaSec As Double

        If App.GetSettingsInt("accelSensitiv", 5) = 0 Then Exit Sub 'choc nie ma prawa sie zdarzyc, bo gdy =0 to nie wlaczamy akcelerometru
        Dim dMinChange As Double = 1 / App.GetSettingsInt("accelSensitiv", 5)

        With e.Reading
            If moAccelLast IsNot Nothing Then

                dZmianaX = .AccelerationX - moAccelLast.AccelerationX
                dZmianaY = .AccelerationY - moAccelLast.AccelerationY
                dZmianaZ = .AccelerationZ - moAccelLast.AccelerationZ
                dZmianaSec = .Timestamp.ToUnixTimeSeconds - moAccelLast.Timestamp.ToUnixTimeSeconds

                If Math.Abs(dZmianaX) > dMinChange Then bZmiana = True
                If Math.Abs(dZmianaY) > dMinChange Then bZmiana = True
                If Math.Abs(dZmianaZ) > dMinChange Then bZmiana = True
            End If
            moAccelLast = e.Reading
            If Not bZmiana Then Exit Sub
            If dZmianaSec < 5 Then Exit Sub  ' interesuje nas POCZATEK ruchu

            Debug.WriteLine("Zmiana X:" & dZmianaX)
            Debug.WriteLine("Zmiana Y:" & dZmianaY)
            Debug.WriteLine("Zmiana Z:" & dZmianaZ)

            Dim bLandscape As Boolean = True
            ' dla aplikacji
            Try
                If ApplicationView.GetForCurrentView.Orientation = ApplicationViewOrientation.Portrait Then bLandscape = False
            Catch ex As Exception
            End Try
            ' ale Sensor sa wedle osi dla device, a nie osi aplikacji...
            Dim oOrientSens As Windows.Devices.Sensors.OrientationSensor
            oOrientSens = Windows.Devices.Sensors.OrientationSensor.GetDefault
            If oOrientSens IsNot Nothing Then
                Dim oOrient As Windows.Devices.Sensors.OrientationSensorReading = oOrientSens.GetCurrentReading
                If oOrient.RotationMatrix.M11 = 0 Then bLandscape = False
            End If

            '' jesli w osi X, to znaczy ze gora/dol (landscape przynajmniej)
            'If Math.Abs(dZmianaX) < Math.Abs(dZmianaY) Then Exit Sub
            'If Math.Abs(dZmianaX) < Math.Abs(dZmianaZ) Then Exit Sub

            NextPic()

            ' proby
            ' w dol
            'Zmiana X: -1.53505796194077 <-- MAX MINUS
            'Zmiana Y: -0.312480009160936
            'Zmiana Z: 1.34756994247437

            'w gore
            'Zmiana X: -0.609335958957672   <-- MAX, ale czemu tez minus?
            'Zmiana Y: -0.136709991842508
            'Zmiana Z: 0.152333997189999

            ' w prawo
            'Zmiana X: -0.230453968048096
            'Zmiana Y: 0.628865972161293
            'Zmiana Z: 0.867131978273392 <-- MAX PLUS

            ' w lewo
            'Zmiana X: 0.0624959468841553
            'Zmiana Y: -0.132803998887539
            'Zmiana Z: -0.523403994739056 <-- MAX MINUS



            ' proby - jeszcze przed ograniczeniem sekundowym
            'land dol
            'Zmiana X: 0.656207919120789 <--MAX
            'Zmiana Y: 0.421847984194756
            'Zmiana Z: -0.503873966634274
            'Zmiana X: -0.824165940284729
            'Zmiana Y: -0.316385988146067
            'Zmiana Z: 0.101555973291397

            'land up
            'Zmiana X: 0.277326047420502 <-- MAX
            'Zmiana Y: 0.0585899986326694
            'Zmiana Z: -0.160145998001099
            'Zmiana X: -0.238266050815582
            'Zmiana Y: -0.0742140002548695
            'Zmiana Z: 0.675737991929054

            ' Dim dG As Double = .AccelerationX * .AccelerationX + .AccelerationY * .AccelerationY + .AccelerationZ * .AccelerationZ
        End With

    End Sub
    Private Sub AccelShaken(sender As Object, e As Windows.Devices.Sensors.AccelerometerShakenEventArgs)
        NextPic()
    End Sub

    Private Sub InclinTick(sender As Object, e As Windows.Devices.Sensors.InclinometerReadingChangedEventArgs)
        NextPic()
    End Sub

    Private Sub GyroTick(sender As Object, e As Windows.Devices.Sensors.GyrometerReadingChangedEventArgs)
        ' ale ze to raz na jakis czas, i AngularVelocity X/Y/Z?
        ' a moze OrientationSensor?
        ' w zaleznosci od OrientationSensor, roznie z akcelerometrem? (obracanie osi)
        ' inclinometer?
        ' accelerometer.shaken event?
    End Sub

    Public Sub CoreWindow_KeyDown(sender As Windows.UI.Core.CoreWindow, args As Windows.UI.Core.KeyEventArgs)
        VirtKeyDown(args.VirtualKey)
    End Sub

    Private Sub UstawMenu()
        miMenuItem = miMenuItem + 1
        If miMenuItem > 5 Then miMenuItem = 0

        Dim sLetter = ""
        Select Case miMenuItem
            Case 0
                sLetter = ""
            Case 1
                sLetter = "B"   ' brightness
            Case 2
                sLetter = "W"   ' rozstaw
            Case 3
                sLetter = "Z"   ' zoom
            Case 4
                sLetter = "H"   ' panning horizontal
            Case 5
                sLetter = "V"   ' panning vertical
        End Select
        uiMenuL.Text = sLetter
        uiMenuR.Text = sLetter
        If sLetter = "" Then
            uiMenuL.Visibility = Visibility.Collapsed
            uiMenuR.Visibility = Visibility.Collapsed
        Else
            uiMenuL.Visibility = Visibility.Visible
            uiMenuR.Visibility = Visibility.Visible
        End If

        ' menu (jasnosc, rozsuniecie, zoom, panning, exit menu - pokazywanie literki, oraz zmiana reakcji na Up/Down)

    End Sub

    Private Sub VirtKeyDown(oKey As Windows.System.VirtualKey)
        Debug.WriteLine("VirtKey" & oKey.ToString)

        Select Case miMenuItem
            Case 0  ' nie ma menu
                Select Case oKey
                    Case Windows.System.VirtualKey.Space, App.GetSettingsInt("kbdNext", 177)
                        NextPic()
                    Case Windows.System.VirtualKey.Enter
                        NextPic()
                    Case Windows.System.VirtualKey.Escape, App.GetSettingsInt("kbdExit", 0)
                        Sprzatamy()
                        Me.Frame.Navigate(GetType(MainPage))
                    Case App.GetSettingsInt("kbdPrev", 176)
                        NextPic(True)
                    Case App.GetSettingsInt("kbdMenu", 179)
                        UstawMenu()
                End Select
            Case 1
                Select Case oKey
                    Case App.GetSettingsInt("kbdNext", 177)
                        Jasnosc(1)
                    Case Windows.System.VirtualKey.Enter, Windows.System.VirtualKey.Space, App.GetSettingsInt("kbdMenu", 179)
                        UstawMenu()
                    Case App.GetSettingsInt("kbdPrev", 176)
                        Jasnosc(-1)
                    Case App.GetSettingsInt("kbdExit", 0)
                        miMenuItem = -1
                        UstawMenu()
                End Select
            Case 2
                Select Case oKey
                    Case App.GetSettingsInt("kbdNext", 177)
                        Rozsun(1)
                    Case Windows.System.VirtualKey.Enter, Windows.System.VirtualKey.Space, App.GetSettingsInt("kbdMenu", 179)
                        UstawMenu()
                    Case App.GetSettingsInt("kbdPrev", 176)
                        Rozsun(-1)
                    Case App.GetSettingsInt("kbdExit", 0)
                        miMenuItem = -1
                        UstawMenu()
                End Select
            Case 3
                Select Case oKey
                    Case App.GetSettingsInt("kbdNext", 177)
                        Zoomek(1)
                    Case Windows.System.VirtualKey.Enter, Windows.System.VirtualKey.Space, App.GetSettingsInt("kbdMenu", 179)
                        UstawMenu()
                    Case App.GetSettingsInt("kbdPrev", 176)
                        Zoomek(-1)
                    Case App.GetSettingsInt("kbdExit", 0)
                        miMenuItem = -1
                        UstawMenu()
                End Select
            Case 4
                Select Case oKey
                    Case App.GetSettingsInt("kbdNext", 177)
                        Panning(1, 0)
                    Case Windows.System.VirtualKey.Enter, Windows.System.VirtualKey.Space, App.GetSettingsInt("kbdMenu", 179)
                        UstawMenu()
                    Case App.GetSettingsInt("kbdPrev", 176)
                        Panning(-1, 0)
                    Case App.GetSettingsInt("kbdExit", 0)
                        miMenuItem = -1
                        UstawMenu()
                End Select
            Case 5
                Select Case oKey
                    Case App.GetSettingsInt("kbdNext", 177)
                        Panning(0, 1)
                    Case Windows.System.VirtualKey.Enter, Windows.System.VirtualKey.Space, App.GetSettingsInt("kbdMenu", 179)
                        UstawMenu()
                    Case App.GetSettingsInt("kbdPrev", 176)
                        Panning(0, -1)
                    Case App.GetSettingsInt("kbdExit", 0)
                        miMenuItem = -1
                        UstawMenu()
                End Select

        End Select



        'guzik   iOS	= Android
        '------|-------|---------
        'dolny   173	(mute?)
        'górny   179	
        '------|-------|---------
        'joy U -
        'joy D	-
        'joy R	176
        'joy L	177
        '------|-------|---------
        'left    -   
        'rigth   -
        '------|-------|---------
        'key U	175 (glosnosc?)
        'key D	174 (glosnosc?)
        'key R	179
        'key L	VirtKeyGoHome (browser)
        '------|-------|---------
    End Sub
    Private Sub NextPic()
        NextPic(False)  ' tak dziwnie, bo RunTime etc. nie lubi Optional
    End Sub

    Private Sub NextPic(bDown As Boolean)
        If bDown Then
            If miPicNo > 0 Then miPicNo = miPicNo - 1
        Else
            miPicNo = miPicNo + 1
        End If
        Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, AddressOf ShowPic)
    End Sub

    Private Sub Jasnosc(iZmiana As Integer)
        Dim dJasnosc As Double  ' 0.0 do 1.0
        dJasnosc = moBright.BrightnessLevel
        dJasnosc = dJasnosc + iZmiana * 0.1
        dJasnosc = Math.Min(dJasnosc, 1.0)
        dJasnosc = Math.Max(dJasnosc, 0.0)

        If Not moBright.IsOverrideActive Then moBright.StartOverride()
        moBright.SetBrightnessLevel(dJasnosc, DisplayBrightnessOverrideOptions.UseDimmedPolicyWhenBatteryIsLow)
    End Sub
    Private Sub Rozsun(iZmiana As Integer)
        Dim iWidth As Integer
        iWidth = uiSplitter.Width.Value + iZmiana
        iWidth = Math.Max(iWidth, App.GetSettingsInt("gruboscPodzialu", 5))
        uiSplitter.Width = New GridLength(iWidth)
    End Sub

    Private Sub Zoomek(iZmiana As Integer)
        If iZmiana > 0 Then
            uiLeftImage.Stretch = Stretch.None
        Else
            uiLeftImage.Stretch = Stretch.Uniform
        End If
    End Sub

    Private Sub Panning(iX As Integer, iY As Integer)

    End Sub
    Private Async Function ShowPic() As Task

        Try
            moTimer.Stop()      ' w Try, bo jesli nie tyka, to Stop dalby blad
        Catch ex As Exception
        End Try

        If moBright.IsOverrideActive Then moBright.StopOverride()

        Dim iSkipPics As Integer = miPicNo

        For Each oStereoPic As jednoStereo In App.mListaStereo.GetListRaw

            ' pomijamy te, ktore nie sa do pokazania
            If Not oStereoPic.bInSlideshow Then Continue For

            ' pomijamy juz widziane
            If iSkipPics > 0 Then
                iSkipPics -= 1
                Continue For
            End If

            Dim iWidth As Integer
            iWidth = App.GetSettingsInt("gruboscPodzialu", 5)


            ' a ten pokazujemy
            Dim oFileL, oFileR As Windows.Storage.StorageFile
            oFileL = Await App.mFolder.GetFileAsync(oStereoPic.imageLeft)
            oFileR = Nothing
            If oStereoPic.imageRight <> "" Then
                oFileR = Await App.mFolder.GetFileAsync(oStereoPic.imageRight)
                iWidth = App.GetSettingsInt("niewidocznyPodzial", 5)
            End If

            uiSplitter.Width = New GridLength(iWidth)
            If Not Await App.LoadPic(oFileL, oFileR) Then Continue For
            uiLeftImage.Source = App.moSoftBitmapL
            uiRightImage.Source = App.moSoftBitmapR

            ' ustawienie timera do auto-next
            Dim iTicks As Integer = App.GetSettingsInt("timerNextSlide", 0)
            If iTicks > 0 Then
                moTimer.Interval = TimeSpan.FromSeconds(iTicks)
                moTimer.Start()
            End If

            ' skoro pokazalismy, to nie mamy nic wiecej do roboty
            Exit Function
        Next

        ' a skoro tu dotarlismy, to znaczy ze nic wiecej nie mamy do pokazania
        Sprzatamy()
        Me.Frame.Navigate(GetType(MainPage))
    End Function

    Private Sub Sprzatamy()

        If moBright.IsOverrideActive Then moBright.StopOverride()

        If moDisplReq IsNot Nothing Then
            Try
                moDisplReq.RequestRelease()
            Catch ex As Exception
                ' bo przeciez moze nie byc :)
            End Try
        End If

        If App.IsMobile Then ApplicationView.GetForCurrentView.ExitFullScreenMode()

        Try
            moTimer.Stop()
            RemoveHandler Window.Current.CoreWindow.KeyDown, AddressOf CoreWindow_KeyDown
            If moGyro IsNot Nothing Then
                RemoveHandler moGyro.ReadingChanged, AddressOf GyroTick
            End If
            If moAccel IsNot Nothing Then
                RemoveHandler moAccel.ReadingChanged, AddressOf AccelTick
            End If
        Catch ex As Exception

        End Try

    End Sub

    Private Sub uiImage_Tapped(sender As Object, e As TappedRoutedEventArgs) Handles uiLeftImage.Tapped, uiRightImage.Tapped
        NextPic()
    End Sub
End Class
