' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409


''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class MainPage
    Inherits Page

    Private Const MIN_WIDTH_TO_PREVIEW As Integer = 300

    Private Sub Page_SizeChanged(sender As Object, e As SizeChangedEventArgs)
        If uiGrid.ActualWidth > MIN_WIDTH_TO_PREVIEW Then
            uiColPreview.Width = New GridLength(1, GridUnitType.Star)
        Else
            uiColPreview.Width = New GridLength(2)
        End If
    End Sub

    Private Async Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        ' jesli juz mamy liste plikow, to nie odczytujemy na nowo
        Dim bFirst As Boolean = True
        If App.mListaStereo IsNot Nothing Then bFirst = False

        uiDirectory.Text = App.GetSettingsString("lastDir")
        If uiDirectory.Text.Length < 5 Then Exit Sub
        If bFirst Then
            App.mFolder = Await Windows.Storage.StorageFolder.GetFolderFromPathAsync(uiDirectory.Text)
            App.mListaStereo = New ListaStereo(uiDirectory.Text)
        End If
        Await RefreshDir()
    End Sub

    Private Async Sub uiBrowse_Click(sender As Object, e As RoutedEventArgs) Handles uiBrowse.Click
        ' browser
        Dim oPick As Windows.Storage.Pickers.FolderPicker = New Windows.Storage.Pickers.FolderPicker
        oPick.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder
        oPick.FileTypeFilter.Add("*")
        Dim oFold As Windows.Storage.StorageFolder = Await oPick.PickSingleFolderAsync
        Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(oFold)
        App.mFolder = oFold
        App.SetSettingsString("lastDir", oFold.Path)

        uiDirectory.Text = oFold.Path
        App.mListaStereo = New ListaStereo(uiDirectory.Text)
        Await RefreshDir()
    End Sub

    Private Sub uiSelectNone_Click(sender As Object, e As RoutedEventArgs)
        If App.mListaStereo Is Nothing Then Exit Sub
        App.mListaStereo.SelectAll(False)
        uiListItems.ItemsSource = App.mListaStereo.GetList

    End Sub

    Private Sub uiSelectAll_Click(sender As Object, e As RoutedEventArgs)
        If App.mListaStereo Is Nothing Then Exit Sub
        App.mListaStereo.SelectAll(True)
        uiListItems.ItemsSource = App.mListaStereo.GetList

    End Sub

    Private Sub uiRun_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.Navigate(GetType(AdjustPhoneInGogle))
        ' przed samymi slajdami zrobic adjust polozenia telefonu w goglach
        ' a wiec przejscie do AdjustPosition - pokazanie obrazka z Assets
    End Sub

    Private Sub uiSetup_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.Navigate(GetType(Settings))
    End Sub
#If False Then

    'Private Async Function RefreshDir_Iterate() As Task(Of Boolean)
    '    Dim bOk As Boolean = True
    '    Try
    '        Dim fTypes As List(Of String) = New List(Of String)
    '        fTypes.Add(".jpg")
    '        Dim oQueryOpt As Windows.Storage.Search.QueryOptions =
    '            New Windows.Storage.Search.QueryOptions(
    '                Windows.Storage.Search.CommonFileQuery.OrderByName,
    '                fTypes)

    '        Dim oRes As Windows.Storage.Search.StorageFileQueryResult = App.mFolder.CreateFileQueryWithOptions(oQueryOpt)
    '        Dim oFiles = oRes.GetFilesAsync()

    '        Dim oFilesWt = Await oFiles
    '        For Each oFile As Windows.Storage.StorageFile In oFilesWt
    '            Dim oNew As jedenPlik = New jedenPlik
    '            oNew.sFName = oFile.Name
    '            oNew.sPicMode = "-"
    '            App.mListaPlikow.Add(oNew)
    '        Next

    '        uiListItems.ItemsSource = App.mListaPlikow
    '    Catch ex As Exception
    '        bOk = False
    '    End Try

    '    Return bOk
    'End Function

    'Private Async Function RefreshDir_OwnIndex(bOnlyInfo As Boolean) As Task(Of Boolean)
    '    ' bOnlyInfo = true: App.mListaPlikow ma liste plikow, dopisz oNew.sPicMode
    '    ' = false, App.mListaPlikow pusta, dodaj elementy
    '    ' ret = true: był indeks
    '    ' = false: nie ma indeksu

    '    App.mListaStereo.Clear()

    '    Dim oFile As Windows.Storage.StorageFile = Await App.mFolder.TryGetItemAsync("stereo.xml")
    '    If oFile Is Nothing Then Return False

    '    Await App.mListaStereo.Load(oFile)

    '    For Each oStereo As jednoStereo In App.mListaStereo.GetList

    '    Next

    '    Return True
    'End Function
#End If

    Private Async Function RefreshDir() As Task
        If App.mFolder Is Nothing Then Exit Function
        App.mListaStereo = New ListaStereo(App.mFolder)

        Await App.mListaStereo.Load()
        Await App.mListaStereo.Import
        Await App.mListaStereo.VerifyExistence



        If App.mListaStereo.Count(False) < 1 Then
            Await App.DialogBoxRes("resMainNoKnownFile")
        Else
            If App.mListaStereo.Count(True) < 1 Then
                Await App.DialogBoxRes("resMainFilesRemoved")
            End If
        End If

        If App.GetSettingsBool("autoAll", True) Then uiSelectAll_Click(Nothing, Nothing)

        uiListItems.ItemsSource = App.mListaStereo.GetList

    End Function

    'Private Sub uiDirectory_TextChanged(sender As Object, e As TextChangedEventArgs) Handles uiDirectory.TextChanged
    '    ' zmieniono katalog - przeskanuj go.
    '    ' Ale nie robi skan, gdy Text jest ten sam - czyli [Browse...] jako rescan by nie działał

    'End Sub

    Private Async Sub uiFile_Tapped(sender As Object, e As TappedRoutedEventArgs)
        ' sender = Grid

        If uiGrid.ActualWidth < MIN_WIDTH_TO_PREVIEW Then Exit Sub ' bo i tak nie ma pokazanego Viewera

        Dim sFName As String = TryCast(TryCast(sender, Grid).DataContext, jednoStereo).imageLeft
        Dim oFile As Windows.Storage.StorageFile = Nothing
        Try
            oFile = Await App.mFolder.GetFileAsync(sFName)
        Catch ex As Exception
            Exit Sub  ' niestety, efekt mklink nie pozwala wyjść poza bezposrednio katalog
        End Try

        Dim oBmp As BitmapImage = New BitmapImage
        oBmp.SetSource(Await oFile.OpenAsync(Windows.Storage.FileAccessMode.Read))

        uiPreview.Source = oBmp

    End Sub
End Class
