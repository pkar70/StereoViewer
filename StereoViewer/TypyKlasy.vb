Imports System.Xml.Serialization

'Public Class jedenPlik
'    Public Property sFName As String
'    ' Public Property sFNameR As String
'    Public Property sPicMode As String
'End Class

Public Class jednoStereo
    <XmlAttribute()>
    Public Property imageLeft As String = ""
    <XmlAttribute()>
    Public Property imageRight As String = ""
    <XmlIgnore>
    Public Property bMising As Boolean = False
    <XmlIgnore>
    Public Property bInSlideshow As Boolean = False
    ' dodatkowe parametry - w tym przesuniecia wzajemne
    ' iOffsetX, iOffsetY
    ' iRotate
    ' iBrightness
End Class

Public Class ListaStereo
    Private mLista As Collection(Of jednoStereo)
    Private bDirty = False
    Private msDir As String

    Public Sub Clear()
        mLista.Clear()
    End Sub

    Public Function IsDirty()
        Return bDirty
    End Function

    Public Sub Add(sNameL As String, sNameR As String)
        Dim oNew As jednoStereo = New jednoStereo
        oNew.imageLeft = sNameL
        oNew.imageRight = sNameR
        mLista.Add(oNew)
        bDirty = True
    End Sub
    Public Sub Delete(sName As String)
        bDirty = True
        For Each oDel As jednoStereo In mLista
            If oDel.imageLeft = sName Then
                mLista.Remove(oDel)
                Exit Sub
            End If
        Next
    End Sub

    Private Async Function GetDir() As Task(Of Windows.Storage.StorageFolder)
        Try
            Return Await Windows.Storage.StorageFolder.GetFolderFromPathAsync(msDir)
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Private Async Function GetFile(sFileName) As Task(Of Windows.Storage.StorageFile)
        Dim oFold As Windows.Storage.StorageFolder
        oFold = Await GetDir()
        If oFold Is Nothing Then Return Nothing

        Return Await oFold.TryGetItemAsync(sFileName)
    End Function
    Private Async Function GetFileWrite(sFileName) As Task(Of Windows.Storage.StorageFile)
        Dim oFold As Windows.Storage.StorageFolder
        oFold = Await GetDir()
        If oFold Is Nothing Then Return Nothing

        Return Await oFold.CreateFileAsync(sFileName, Windows.Storage.CreationCollisionOption.ReplaceExisting)
    End Function

    Public Async Function Load() As Task(Of Boolean)
        Dim oFile As Windows.Storage.StorageFile = Await GetFile("stereo.xml")
        If oFile Is Nothing Then Return False

        Dim oSer As XmlSerializer = New XmlSerializer(GetType(Collection(Of jednoStereo)))
        Dim oStream As Stream = Await oFile.OpenStreamForReadAsync
        mLista = TryCast(oSer.Deserialize(oStream), Collection(Of jednoStereo))

        Return True
    End Function

    Public Async Function Import() As Task(Of Boolean)
        Dim oFile As Windows.Storage.StorageFile = Await GetFile("stereo.txt")
        If oFile Is Nothing Then Return False

        App.DialogBoxRes("resTypyAktualizacja") 'aktualizuję listę...

        Dim slContent As IList(Of String) = Await Windows.Storage.FileIO.ReadLinesAsync(oFile)

        For Each sLine In slContent

            If sLine.Length < 5 Then Continue For

            Dim sData As String() = sLine.Split(vbTab)
            Dim oNew As jednoStereo = New jednoStereo

            oNew.imageLeft = sData(0)
            If sData.GetUpperBound(0) > 0 Then
                oNew.imageRight = sData(1)
            End If
            mLista.Add(oNew)
            bDirty = True
        Next

        Await Save(False)

        Await oFile.RenameAsync("stereo." & Date.Now.ToString("yyyyMMdd") & ".txt")
        Return True

    End Function

    Public Async Function VerifyExistence() As Task

        If Not App.GetSettingsBool("verifyFiles") Then Exit Function

        Dim oFold As Windows.Storage.StorageFolder
        oFold = Await GetDir()
        If oFold Is Nothing Then Exit Function

        For Each oItem In mLista
            If oFold.TryGetItemAsync(oItem.imageLeft) Is Nothing Then oItem.bMising = True
            If oItem.imageRight.Length > 5 Then
                If oFold.TryGetItemAsync(oItem.imageRight) Is Nothing Then oItem.bMising = True
            End If
        Next
    End Function

    Public Async Function Save(bForce As Boolean) As Task(Of Boolean)
        If Not bForce AndAlso Not bDirty Then Return True

        Dim oFile As Windows.Storage.StorageFile = Await GetFileWrite("stereo.xml")
        If oFile Is Nothing Then Return False

        Dim oSer As XmlSerializer = New XmlSerializer(GetType(Collection(Of jednoStereo)))
        Dim oStream As Stream = Await oFile.OpenStreamForWriteAsync
        oSer.Serialize(oStream, mLista)
        oStream.Dispose()   ' == fclose

        Return True
    End Function

    Public Sub New(sDirectory As String)
        mLista = New Collection(Of jednoStereo)
        msDir = sDirectory
        bDirty = False
    End Sub
    Public Sub New(oDirectory As Windows.Storage.StorageFolder)
        mLista = New Collection(Of jednoStereo)
        msDir = oDirectory.Path
        bDirty = False
    End Sub

    Protected Overrides Async Sub Finalize()
        If bDirty Then
            Await Save(False)
            bDirty = False
        End If
        mLista.Clear()
    End Sub

    Public Function GetListRaw() As ICollection(Of jednoStereo)
        Return mLista
    End Function
    Public Function GetList() As IEnumerable(Of jednoStereo)
        Return From c In mLista Where c.bMising = False
    End Function

    Public Function GetOrderedList() As IOrderedEnumerable(Of jednoStereo)
        Dim groups As IOrderedEnumerable(Of jednoStereo) = From c In mLista Order By c.imageLeft
        Return groups
    End Function

    Public Sub SelectAll(bSelect As Boolean)
        For Each oItem As jednoStereo In mLista
            oItem.bInSlideshow = bSelect
        Next
    End Sub

    Public Function Count(bOnlyExist As Boolean) As Integer
        If Not bOnlyExist Then Return mLista.Count

        Dim iCnt As Integer = 0
        For Each oItem As jednoStereo In mLista
            If Not oItem.bMising Then iCnt += 1
        Next
        Return iCnt
    End Function
End Class