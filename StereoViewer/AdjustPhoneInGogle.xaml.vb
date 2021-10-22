' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class AdjustPhoneInGogle
    Inherits Page

    Private miCountdown As Integer
    Private miTimer As DispatcherTimer

    Private Sub uiGo_Click(sender As Object, e As RoutedEventArgs)
        uiGo.Content = miCountdown
        uiGo.FontSize = 20

        miTimer = New DispatcherTimer
        miTimer.Interval = TimeSpan.FromSeconds(1)
        AddHandler miTimer.Tick, AddressOf TimerNext
        miTimer.Start()

        ' Me.Frame.Navigate(GetType(PicViewer))
        'Me.Frame.GoBack()
    End Sub

    Private Sub TimerNextUI()
        uiGo.Content = miCountdown
    End Sub
    Private Sub TimerNext(sender As Object, e As Object)
        miCountdown = miCountdown - 1
        If miCountdown < 1 Then
            miTimer.Stop()
            Me.Frame.Navigate(GetType(PicViewer))
        End If
        uiGo.Content = miCountdown
    End Sub

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        Dim iWidth As Integer = App.GetSettingsInt("niewidocznyPodzial", 5)
        uiSplitter.Width = New GridLength(iWidth)
        miCountdown = App.GetSettingsInt("timerStart", 9)
    End Sub
End Class
