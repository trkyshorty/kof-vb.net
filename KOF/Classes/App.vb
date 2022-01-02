Imports System.Data.SQLite
Imports System.Threading

Public Class App

    Private _SqlMainConnection As SQLiteConnection
    Private _SqlUserConnection As SQLiteConnection

    Private _MainDatabase As MainDatabase
    Private _UserDatabase As UserDatabase

    Private _ClientList As New Dictionary(Of String, Client)()
    Private _AccountList As New Dictionary(Of String, Tuple(Of String, String, String))

    Private _LauncherThread As Thread = New Thread((AddressOf Launcher))

    Private _SelectedAccount As String = ""

    Public Sub New()
        _SqlMainConnection = New SQLiteConnection("Data Source=main.db;Version=3;")
        _SqlUserConnection = New SQLiteConnection("Data Source=user.db;Version=3;")
    End Sub

    Public Sub Load()
        _SqlMainConnection.Open()

        _MainDatabase = New MainDatabase(_SqlMainConnection)
        _MainDatabase.Initialize()

        _AccountList = GetMainDatabase.GetAllAccount()

        _SqlUserConnection.Open()

        _UserDatabase = New UserDatabase(_SqlUserConnection)
        _UserDatabase.Initialize()
    End Sub
    Public Function GetUserDatabase() As UserDatabase
        Return _UserDatabase
    End Function

    Public Function GetMainDatabase() As MainDatabase
        Return _MainDatabase
    End Function
    Public Sub Launcher(ByVal Account As String)
        If _LauncherThread.IsAlive Or Account = "" Then
            Exit Sub
        End If

        _SelectedAccount = Account

        _LauncherThread = New Thread(AddressOf LauncherEvent)
        _LauncherThread.IsBackground = True
        _LauncherThread.Start()
    End Sub

    Public Function GetAccountSize() As Long
        Return _AccountList.Count()
    End Function

    Public Function GetClientSize() As Long
        Return _ClientList.Count()
    End Function

    Public Sub LauncherEvent()
        Dim Account = GetMainDatabase.GetAccount(_SelectedAccount)
        Dim Client As New ProcessStartInfo()
        Dim ClientFileInfo As System.IO.FileInfo = My.Computer.FileSystem.GetFileInfo(Account.Item3)

        Client.FileName = ClientFileInfo.Name
        Client.WorkingDirectory = ClientFileInfo.Directory.FullName
        Client.Arguments = "MGAMEJP" & " " & Account.Item1 & " " & Account.Item2

        Dim ClientProcess As Process = Process.Start(Client)

        If ClientProcess.Id = 0 Then Return

        Thread.Sleep(3000)

        Dim Patch As New ProcessStartInfo()

        Patch.FileName = System.Environment.CurrentDirectory() & "\Mutant.exe"
        Patch.WindowStyle = ProcessWindowStyle.Hidden
        Patch.WorkingDirectory = System.Environment.CurrentDirectory()
        Patch.Arguments = ClientProcess.Id

        Dim PatchProcess As Process = Process.Start(Patch)

        PatchProcess.WaitForExit(5000)

        If Process.GetProcesses().Any(Function(a) a.Id = ClientProcess.Id) Then
            SetWindowText(ClientProcess.MainWindowHandle, _SelectedAccount)
        End If

        _SelectedAccount = ""
    End Sub

    Public Sub SetAccount(ByVal User As String, ByVal Hash As String, ByVal Path As String)
        If _AccountList.ContainsKey(User) = False Then
            _AccountList.Add(User, New Tuple(Of String, String, String)(User, Hash, Path))
        End If

        _MainDatabase.SetAccount(User, Hash, Path)
    End Sub

    Public Function GetAccount(ByVal User As String, ByVal Hash As String) As Tuple(Of String, String, String)
        If _AccountList.ContainsKey(User) Then
            Return _AccountList(User)
        End If
        Return Nothing
    End Function

    Public Function GetAllAccount() As Dictionary(Of String, Tuple(Of String, String, String))
        Return _AccountList
    End Function

    Public Function GetAllAccountName() As Dictionary(Of String, String)
        Dim AccountNameList = New Dictionary(Of String, String)()
        For Each AccountList As KeyValuePair(Of String, Tuple(Of String, String, String)) In GetAllAccount()
            AccountNameList.Add(AccountList.Value.Item1, AccountList.Value.Item1)
        Next
        GetAllAccountName = AccountNameList
    End Function

    Public Function SetClient(ByVal Process As Long, ByVal Handle As Long) As Client
        If GetClient(Process) IsNot Nothing Then
            Return GetClient(Process)
        Else
            Dim Client As Client = New Client(Me, Process, Handle)
            _ClientList.Add(Process, Client)
            Return Client
        End If
    End Function

    Public Function GetClient(ByVal Process As Long) As Client
        If _ClientList.ContainsKey(Process) Then
            Return _ClientList(Process)
        End If
        Return Nothing
    End Function

    Public Function GetAllClient() As Dictionary(Of String, Client)
        Return _ClientList
    End Function

    Public Function GetAllClientName() As Dictionary(Of String, String)
        Dim ClientNameList = New Dictionary(Of String, String)()
        For Each ClientList As KeyValuePair(Of String, Client) In GetAllClient()
            Dim Client As Client = ClientList.Value
            ClientNameList.Add(ClientList.Key, Client.GetName())
        Next
        GetAllClientName = ClientNameList
    End Function

    Public Sub RemoveClient(Process)
        If _ClientList.ContainsKey(Process) Then
            _ClientList.Remove(Process)
        End If
    End Sub
End Class
