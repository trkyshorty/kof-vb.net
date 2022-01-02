Imports System.Management

Public Class Main
    Private _App As App

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim SearchRunningProcess As New ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE Name='KnightOnLine.exe'")

        For Each ProcessInfo As ManagementObject In SearchRunningProcess.Get()

            Dim ProcessCommandLine() As String = ProcessInfo("CommandLine").Split(" ")
            Dim ProcessRunningPath As String = ProcessCommandLine(0).Replace("""", String.Empty)

            Dim Process As Long = ProcessInfo("ProcessId")

            Dim Handle As Long = OpenProcess(PROCESS_VM_OPERATION Or PROCESS_VM_READ Or PROCESS_VM_WRITE, False, Process)

            If Handle = 0 Then Continue For

            _App.SetClient(Process, Handle)
            _App.SetAccount(ProcessCommandLine(2), ProcessCommandLine(3), ProcessRunningPath)

            ListBox1.DataSource = New BindingSource(_App.GetAllClientName(), Nothing)
            ListBox1.DisplayMember = "Value"
            ListBox1.ValueMember = "Key"

            ListBox2.DataSource = New BindingSource(_App.GetAllAccountName(), Nothing)
            ListBox2.DisplayMember = "Value"
            ListBox2.ValueMember = "Key"

            ComboBox1.DataSource = New BindingSource(_App.GetAllClientName(), Nothing)
            ComboBox1.DisplayMember = "Value"
            ComboBox1.ValueMember = "Key"

            If _App.GetClientSize() > 0 Then
                CheckBox1.Enabled = True
                Button3.Enabled = True
                Button7.Enabled = True
                CheckBox2.Enabled = True
                Attack.Enabled = True
                AreaControl.Enabled = True
                ActionMove.Enabled = True
                ActionSetCoordinate.Enabled = True
                Target.Enabled = True
                CheckedListBox3.Enabled = True
                Button4.Enabled = True
                Button5.Enabled = True
                Button6.Enabled = True
                Button8.Enabled = True
            Else
                CheckBox1.Enabled = False
                Button3.Enabled = False
                Button7.Enabled = False
                CheckBox2.Enabled = False
                Attack.Enabled = False
                AreaControl.Enabled = False
                ActionMove.Enabled = False
                ActionSetCoordinate.Enabled = False
                Target.Enabled = False
                CheckedListBox3.Enabled = False
                Button4.Enabled = False
                Button5.Enabled = False
                Button6.Enabled = False
                Button8.Enabled = False
            End If

            For Each ClientList As KeyValuePair(Of String, Client) In _App.GetAllClient()

                Dim Clients As Client = ClientList.Value

                If Clients.IsFollowDisable() = True Then Continue For

                If Attack.Checked Then Clients.SetControl(Attack.Name, Attack.Checked)
                If ActionMove.Checked Then Clients.SetControl(ActionMove.Name, ActionMove.Checked)
                If ActionSetCoordinate.Checked Then Clients.SetControl(ActionSetCoordinate.Name, ActionSetCoordinate.Checked)
                If Target.Checked Then Clients.SetControl(Target.Name, Target.Checked)

                If Attack.Checked Then
                    Clients.SetControl(AreaControl.Name, AreaControl.Checked)
                    Clients.SetControl("Area_Control_X", Clients.GetX())
                    Clients.SetControl("Area_Control_Y", Clients.GetY())
                End If

                For ListBoxIndex As Long = 0 To CheckedListBox3.Items.Count - 1
                    Clients.SetMobListState(CheckedListBox3.Items(ListBoxIndex), CheckedListBox3.GetItemChecked(ListBoxIndex))
                Next
            Next
        Next
    End Sub

    Private Sub ListBox1_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles ListBox1.DoubleClick
        If ListBox1.SelectedItem Is Nothing Then Return
        Dim Process = DirectCast(ListBox1.SelectedItem, KeyValuePair(Of String, String)).Key
        Dim Client As Client = _App.GetClient(Process)

        If Client IsNot Nothing Then
            Client.GetConfiguration().ShowDialog()
        End If
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        If _App.GetClientSize() = 0 Then Exit Sub

        Dim Process As Long = DirectCast(ComboBox1.SelectedItem, KeyValuePair(Of String, String)).Key
        Dim Name As String = DirectCast(ComboBox1.SelectedItem, KeyValuePair(Of String, String)).Value

        If CheckBox1.Checked = True Then
            For Each ClientList As KeyValuePair(Of String, Client) In _App.GetAllClient()
                Dim Client As Client = ClientList.Value

                If Client.IsFollowDisable() = True Then Continue For

                Client.SetFollower(0)

                If Client.GetProcess() <> Process Then
                    Client.SetFollower(Process)
                End If

                If Attack.Checked Then Client.SetControl(Attack.Name, Attack.Checked)
                If ActionMove.Checked Then Client.SetControl(ActionMove.Name, ActionMove.Checked)
                If ActionSetCoordinate.Checked Then Client.SetControl(ActionSetCoordinate.Name, ActionSetCoordinate.Checked)
                If Target.Checked Then Client.SetControl(Target.Name, Target.Checked)
            Next
        End If

    End Sub

    Private Sub List_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        TopMost = True

        _App = New App()
        _App.Load()

        If _App.GetAccountSize() <> 0 Then
            ListBox2.DataSource = New BindingSource(_App.GetAllAccountName(), Nothing)
            ListBox2.DisplayMember = "Value"
            ListBox2.ValueMember = "Key"
        End If

    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If ComboBox1.Items.Count = 0 Then Return
        Dim Process As Long = DirectCast(ComboBox1.SelectedItem, KeyValuePair(Of String, String)).Key
        For Each ClientList As KeyValuePair(Of String, Client) In _App.GetAllClient()
            Dim Client As Client = ClientList.Value
            Client.SetFollower(0)
            If CheckBox1.Checked Then
                If Client.GetProcess() <> Process Then
                    Client.SetFollower(Process)
                End If
            End If
        Next
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        _App.Launcher(DirectCast(ListBox2.SelectedItem, KeyValuePair(Of String, String)).Key)
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        For Each ClientList As KeyValuePair(Of String, Client) In _App.GetAllClient()
            Dim Client As Client = ClientList.Value

            Dim ProcessId As Long = Client.GetProcess()
            Dim ProcessRunning As Boolean = Process.GetProcesses().Any(Function(b) b.Id = ProcessId)

            If ProcessRunning Then
                Dim p As Process = Process.GetProcessById(ProcessId)
                Try
                    p.Kill()
                    p.WaitForExit()
                Catch ex As Exception
                End Try
            End If
        Next
    End Sub

    Private Sub CheckBox3_CheckedChanged(sender As Object, e As EventArgs) Handles Attack.CheckedChanged
        For Each ClientList As KeyValuePair(Of String, Client) In _App.GetAllClient()
            Dim Client As Client = ClientList.Value
            If Client.IsFollowDisable() = True Then Continue For
            Client.SetControl(Attack.Name, Attack.Checked)
        Next
    End Sub

    Private Sub CheckBox4_CheckedChanged(sender As Object, e As EventArgs) Handles AreaControl.CheckedChanged
        For Each ClientList As KeyValuePair(Of String, Client) In _App.GetAllClient()
            Dim Client As Client = ClientList.Value

            If Client.IsFollowDisable() = True Then Continue For

            Client.SetControl(AreaControl.Name, AreaControl.Checked)

            Client.SetControl("AreaControlX", Client.GetX())
            Client.SetControl("AreaControlY", Client.GetY())
        Next
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Dim Process As Long = DirectCast(ComboBox1.SelectedItem, KeyValuePair(Of String, String)).Key
        Dim Name As String = DirectCast(ComboBox1.SelectedItem, KeyValuePair(Of String, String)).Value

        For Each ClientList As KeyValuePair(Of String, Client) In _App.GetAllClient()
            Dim Client As Client = ClientList.Value

            Dim MobList = Client.UpdateMobList()

            If MobList.Count = 0 Then Continue For

            For Each Mob As KeyValuePair(Of String, Boolean) In MobList
                If CheckedListBox3.Items.Contains(Mob.Key) Then Continue For
                CheckedListBox3.Items.Add(Mob.Key)
            Next
        Next
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        For Each ClientList As KeyValuePair(Of String, Client) In _App.GetAllClient()
            Dim Client As Client = ClientList.Value
            Client.ClearMobListState()
        Next
        CheckedListBox3.Items.Clear()
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        For ListBoxIndex As Long = 0 To CheckedListBox3.Items.Count - 1
            For Each ClientList As KeyValuePair(Of String, Client) In _App.GetAllClient()
                Dim Client As Client = ClientList.Value
                Client.SetMobListState(CheckedListBox3.Items(ListBoxIndex), True)
            Next
            CheckedListBox3.SetItemChecked(ListBoxIndex, True)
        Next
    End Sub

    Private Sub CheckedListBox3_SelectedIndexChanged(sender As Object, e As ItemCheckEventArgs) Handles CheckedListBox3.ItemCheck
        For Each ClientList As KeyValuePair(Of String, Client) In _App.GetAllClient()
            Dim Client As Client = ClientList.Value
            If Client.IsFollowDisable() = True Then Continue For
            Client.SetMobListState(CheckedListBox3.Items(e.Index), e.NewValue)
        Next
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        Dim ProcessId = DirectCast(ListBox1.SelectedItem, KeyValuePair(Of String, String)).Key

        For Each ClientList As KeyValuePair(Of String, Client) In _App.GetAllClient()
            Dim Client As Client = ClientList.Value

            If ProcessId <> Client.GetProcess() Then Continue For

            Dim p As Process = Process.GetProcessById(ProcessId)

            Try
                p.Kill()
                p.WaitForExit()
            Catch ex As Exception
            End Try

        Next
    End Sub

    Private Sub CheckBox5_CheckedChanged(sender As Object, e As EventArgs) Handles ActionMove.CheckedChanged
        For Each ClientList As KeyValuePair(Of String, Client) In _App.GetAllClient()
            Dim Client As Client = ClientList.Value

            If Client.IsFollowDisable() = True Then Continue For
            Client.SetControl(ActionMove.Name, ActionMove.Checked)
        Next
    End Sub

    Private Sub CheckBox6_CheckedChanged(sender As Object, e As EventArgs) Handles ActionSetCoordinate.CheckedChanged
        For Each ClientList As KeyValuePair(Of String, Client) In _App.GetAllClient()
            Dim Client As Client = ClientList.Value

            If Client.IsFollowDisable() = True Then Continue For
            Client.SetControl(ActionSetCoordinate.Name, ActionSetCoordinate.Checked)
        Next
    End Sub

    Private Sub CheckBox7_CheckedChanged(sender As Object, e As EventArgs) Handles Target.CheckedChanged
        For Each ClientList As KeyValuePair(Of String, Client) In _App.GetAllClient()
            Dim Client As Client = ClientList.Value

            If Client.IsFollowDisable() = True Then Continue For
            Client.SetControl(Target.Name, Target.Checked)
        Next
    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        Dim Process As Long = DirectCast(ComboBox1.SelectedItem, KeyValuePair(Of String, String)).Key
        Dim Name As String = DirectCast(ComboBox1.SelectedItem, KeyValuePair(Of String, String)).Value

        For Each ClientList As KeyValuePair(Of String, Client) In _App.GetAllClient()
            Dim Client As Client = ClientList.Value

            Dim MobList = Client.UpdatePlayerList()

            If MobList.Count = 0 Then Continue For

            For Each Mob As KeyValuePair(Of String, Boolean) In MobList
                If CheckedListBox3.Items.Contains(Mob.Key) Then Continue For
                CheckedListBox3.Items.Add(Mob.Key)
            Next
        Next
    End Sub
End Class
