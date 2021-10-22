' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class KbdWizard
    Inherits Page

    Dim miWizardMode As Integer = -1

    Private Sub Page_Unloaded(sender As Object, e As RoutedEventArgs)
        RemoveHandler Window.Current.CoreWindow.KeyDown, AddressOf CoreWindow_KeyDown

        App.SetSettingsInt("kbdNext", uiKbdNext.Text)
        App.SetSettingsInt("kbdPrev", uiKbdPrev.Text)
        App.SetSettingsInt("kbdMenu", uiKbdMenu.Text)
        App.SetSettingsInt("kbdExit", uiKbdExit.Text)
    End Sub

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        AddHandler Window.Current.CoreWindow.KeyDown, AddressOf CoreWindow_KeyDown

        uiKbdNext.Text = App.GetSettingsInt("kbdNext", 177)
        uiKbdPrev.Text = App.GetSettingsInt("kbdPrev", 176)
        uiKbdMenu.Text = App.GetSettingsInt("kbdMenu", 179)
        uiKbdExit.Text = App.GetSettingsInt("kbdExit", 0)
    End Sub

    Public Sub CoreWindow_KeyDown(sender As Windows.UI.Core.CoreWindow, args As Windows.UI.Core.KeyEventArgs)
        Dim iInt As Integer
        iInt = args.VirtualKey
        uiLastKey.Text = "Last key: " & iInt
        Select Case miWizardMode
            Case 0
                uiKbdNext.Text = iInt
            Case 1
                uiKbdPrev.Text = iInt
            Case 2
                uiKbdMenu.Text = iInt
            Case 3
                uiKbdExit.Text = iInt
        End Select
        miWizardMode = -1
    End Sub

    Private Sub uiKbdGet_Click(sender As Object, e As RoutedEventArgs)
        Dim oButton As Button
        oButton = TryCast(sender, Button)
        miWizardMode = TryCast(oButton.parent, Grid).GetRow(oButton)
    End Sub
End Class
