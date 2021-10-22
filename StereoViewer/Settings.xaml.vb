' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class Settings
    Inherits Page

    ' mogą być dodatkowe screeny ustawiające
    ' dodatkowo: master/slave przy BT

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        uiHiddenPodzial.Value = App.GetSettingsInt("niewidocznyPodzial", 5)
        uiGruboscPodzialu.Value = App.GetSettingsInt("gruboscPodzialu", 5)
        uiTimerNext.Value = App.GetSettingsInt("timerNextSlide", 15)
        uiTimerStart.Value = App.GetSettingsInt("timerStart", 10)
        uiCzuloscGyro.Value = App.GetSettingsInt("gyroSensitiv", 5)
        uiCzuloscAccel.Value = App.GetSettingsInt("accelSensitiv", 5)
        uiVerifyExistence.IsOn = App.GetSettingsBool("verifyFiles")
        uiAutoAll.IsOn = App.GetSettingsBool("autoAll", True)

        Dim oGyro As Windows.Devices.Sensors.Gyrometer
        oGyro = Windows.Devices.Sensors.Gyrometer.GetDefault
        If oGyro IsNot Nothing Then
            uiCzuloscGyro.Visibility = Visibility.Visible
        Else
            uiCzuloscGyro.Visibility = Visibility.Collapsed
        End If

        Dim oAccel As Windows.Devices.Sensors.Accelerometer
        oAccel = Windows.Devices.Sensors.Accelerometer.GetDefault
        If oAccel IsNot Nothing Then
            uiWizardAccel.Visibility = Visibility.Visible
            uiCzuloscAccel.Visibility = Visibility.Visible
        Else
            uiWizardAccel.Visibility = Visibility.Collapsed
            uiCzuloscAccel.Visibility = Visibility.Collapsed
        End If

    End Sub

    Private Sub uiWizardGrubosc_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.Navigate(GetType(GruboscPodzialu))
    End Sub

    Private Sub Page_LosingFocus(sender As UIElement, args As LosingFocusEventArgs)
        App.SetSettingsInt("niewidocznyPodzial", uiHiddenPodzial.Value)
        App.SetSettingsInt("gruboscPodzialu", uiGruboscPodzialu.Value)
        App.SetSettingsInt("timerNextSlide", uiTimerNext.Value)
        App.SetSettingsInt("timerStart", uiTimerStart.Value)
        App.SetSettingsInt("accelSensitiv", uiCzuloscAccel.Value)
        App.SetSettingsInt("gyroSensitiv", uiCzuloscGyro.Value)
        App.SetSettingsBool("verifyFiles", uiVerifyExistence.IsOn)
        App.SetSettingsBool("autoAll", uiAutoAll.IsOn)

    End Sub

    Private Sub Page_Unloaded(sender As Object, e As RoutedEventArgs)
        Page_LosingFocus(Nothing, Nothing)
    End Sub

    Private Sub uiKbdWizard_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.Navigate(GetType(KbdWizard))
    End Sub

    Private Sub uiWizardAccel_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.Navigate(GetType(WizardGyro))
    End Sub
End Class
