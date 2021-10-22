' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

''' <summary>
''' Dopasowanie do konkretnego Device
''' Skan normalnej klatki ma 1024x670 -> 800x523 -> 573x480
''' czyli jednoramkowe jest 800-573=227 pixeli Do wykorzystania po bokach (~55 z kazdej strony kazdego)
''' </summary>
Public NotInheritable Class GruboscPodzialu
    Inherits Page

    Private miEtap As Integer = 0   ' co jest ustalane
    ' =0: grubosc podzialu; App.GetSettingsInt("niewidocznyPodzial")
    ' =1: rozsuniecie srodkow obrazu; App.GetSettingsInt("gruboscPodzialu")
    ' =2: margines z duzymi znieksztalceniami, gora, dół, prawo, lewo
    Private Function GetHelpString() As String
        Select Case miEtap
            Case 0
                Return App.GetLangString("resGruboscAdjust") & vbCrLf &
                      App.GetLangString("resGruboscDec") & vbCrLf &
                      App.GetLangString("resGruboscInc") & vbCrLf &
                      "Space: ok"
            Case 1
                Return App.GetLangString("resGruboscAdjust2") & vbCrLf &
                      App.GetLangString("resGruboscDec") & vbCrLf &
                      App.GetLangString("resGruboscInc") & vbCrLf &
                      "Space: ok"
        End Select
        Return ""
    End Function

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        Dim iWidth As Integer = App.GetSettingsInt("niewidocznyPodzial", 5)
        uiSplitter.Width = New GridLength(iWidth)

        App.DialogBox(GetHelpString())

        AddHandler Window.Current.CoreWindow.KeyDown, AddressOf CoreWindow_KeyDown
        ApplicationView.GetForCurrentView.TryEnterFullScreenMode()
    End Sub
    Private Sub Page_Unloaded(sender As Object, e As RoutedEventArgs)
        RemoveHandler Window.Current.CoreWindow.KeyDown, AddressOf CoreWindow_KeyDown
        ApplicationView.GetForCurrentView.ExitFullScreenMode()
    End Sub
    Public Sub CoreWindow_KeyDown(sender As Windows.UI.Core.CoreWindow, args As Windows.UI.Core.KeyEventArgs)
        VirtKeyDown(args.VirtualKey)
    End Sub

    Private Async Function WczytajObrazekPic() As Task

        Select Case miEtap
            Case 0 ' szerokość paska dzielącego
                App.DialogBox("ERROR WczytajObrazekPic, miEtap=0")
                ' ewentualnie zszarzenie Image
            Case 1 ' rozsuniecie obrazow
                ' wczytanie assets\2235-1024.jpg

                ' Dim oFile As StreamReader = File.OpenText("Assets\opisyRelacji.txt")
                Dim oUri = New Uri("ms-appx:///Assets/2235-1024.jpg")
                Dim oFile As Windows.Storage.StorageFile
                oFile = Await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(oUri)
                Await App.LoadPic(oFile, Nothing)
                uiLeftImage.Source = App.moSoftBitmapL
                uiRightImage.Source = App.moSoftBitmapR
                Dim iWidth As Integer = App.GetSettingsInt("niewidocznyPodzial")
                uiSplitter.Width = New GridLength(iWidth)
        End Select
    End Function

    Private Sub ZmianaRozmiaru(bMinus As Boolean)
        Select Case miEtap
            Case 0  ' szerokość paska dzielącego
                Dim oGL As GridLength = uiSplitter.Width
                Dim iWidth As Integer = oGL.Value
                If bMinus Then
                    iWidth -= 1
                    If iWidth < 0 Then iWidth = 0
                Else
                    If iWidth < uiGrid.ActualWidth / 2 Then iWidth += 1
                End If
                uiSplitter.Width = New GridLength(iWidth)
            Case 1 ' rozsuniecie obrazow
                Dim oGL As GridLength = uiSplitter.Width
                Dim iWidth As Integer = oGL.Value
                If bMinus Then
                    iWidth -= 1
                    If iWidth < 0 Then iWidth = 0
                Else
                    If iWidth < uiGrid.ActualWidth / 2 Then iWidth += 1
                End If
                uiSplitter.Width = New GridLength(iWidth)
        End Select
    End Sub

    Private Async Sub Koniec()
        Select Case miEtap
            Case 0 ' szerokość paska dzielącego
                App.SetSettingsInt("niewidocznyPodzial", uiSplitter.Width.Value)
                miEtap = 1
                Await WczytajObrazekPic()
            Case 1 ' rozsuniecie obrazow
                App.SetSettingsInt("gruboscPodzialu", uiSplitter.Width.Value)
                Me.Frame.GoBack()
        End Select
    End Sub

    Private Sub VirtKeyDown(oKey As Windows.System.VirtualKey)
        Select Case oKey
            Case Windows.System.VirtualKey.Z
                ZmianaRozmiaru(True)
            Case Windows.System.VirtualKey.X
                ZmianaRozmiaru(False)
            Case Windows.System.VirtualKey.Left
                ZmianaRozmiaru(True)
            Case Windows.System.VirtualKey.Up
                ZmianaRozmiaru(False)
            Case Windows.System.VirtualKey.Down
                ZmianaRozmiaru(True)
            Case Windows.System.VirtualKey.Right
                ZmianaRozmiaru(False)
            Case Windows.System.VirtualKey.Space
                Koniec()
            Case Windows.System.VirtualKey.Enter
                Koniec()
        End Select

    End Sub
#Region "Rozne proby zdobycia klawisza"

    'Private Sub uiGrid_KeyDown(sender As Object, e As KeyRoutedEventArgs) Handles uiGrid.KeyDown
    '    VirtKeyDown(e.Key)
    'End Sub

    'Private Sub Page_KeyDown(sender As Object, e As KeyRoutedEventArgs)
    '    uiGrid_KeyDown(sender, e)
    'End Sub

    'Private Sub Grid_KeyDown(sender As Object, e As KeyRoutedEventArgs)
    '    uiGrid_KeyDown(sender, e)
    'End Sub

    'Private Sub uiGrid_KeyUp(sender As Object, e As KeyRoutedEventArgs) Handles uiGrid.KeyUp
    '    uiGrid_KeyDown(sender, e)
    'End Sub
#End Region

End Class
