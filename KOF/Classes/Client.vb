Imports System.Threading
Imports System.Runtime.InteropServices

Public Class Client

#Region "Constants"
    Private _App As App
    Private _Configuration As Configuration

    Private _BlackMarketerMem As Long = 0
    Private _CodeMem As Long = 0
    Private _PacketMem As Long = 0

    Private _Process As Long
    Private _Handle As Long
    Private _MailSlotHandle As Long
    Private _CharacterName As String = ""

    Private _ControlList As New Dictionary(Of String, String)

    Private _AttackEventTime As Long = Environment.TickCount
    Private _TimedSkillEventTime As Long = Environment.TickCount
    Private _DeBuffSkillEventTime As Long = Environment.TickCount

    Private _EventThread As Thread = Nothing
    Private _MailSlotThread As Thread = Nothing
    Private _PartyPriestEventThread As Thread = Nothing
    Private _PartyRogueEventThread As Thread = Nothing

    Private _RepairEventThread As Thread = Nothing
    Private _SupplyEventThread As Thread = Nothing
    Private _AttackSkillEventThread As Thread = Nothing
    Private _TimedSkillEventThread As Thread = Nothing
    Private _DeBuffSkillEventThread As Thread = Nothing

    Private _ProtectionEventTime As Long = Environment.TickCount
    Private _MinorEventTime As Long = Environment.TickCount
    Private _FollowEventTime As Long = Environment.TickCount
    Private _TargetEventTime As Long = Environment.TickCount
    Private _ActionEventTime As Long = Environment.TickCount
    Private _BlackMarketerEventTime As Long = Environment.TickCount
    Private _CharacterDeadEventTime As Long = Environment.TickCount
    Private _PriestPartyControlEventTime As Long = Environment.TickCount
    Private _RoguePartyControlEventTime As Long = Environment.TickCount
    Private _AreaHealEventTime As Long = Environment.TickCount
    Private _StatScrollEventTime As Long = Environment.TickCount
    Private _AttackScrollEventTime As Long = Environment.TickCount
    Private _DefanceScrollEventTime As Long = Environment.TickCount
    Private _DropScrollEventTime As Long = Environment.TickCount
    Private _PartyRequestEventTime As Long = Environment.TickCount
    Private _TransformationEventTime As Long = Environment.TickCount
    Private _RepairEventTime As Long = Environment.TickCount
    Private _AreaControlEventTime As Long = Environment.TickCount
    Private _SupplyEventTime As Long = Environment.TickCount
    Private _RepairAfterWaitTime As Long = Environment.TickCount
    Private _SupplyAfterWaitTime As Long = Environment.TickCount

    Private _FollowDisable As Boolean = False
    Private _IsRepairing As Boolean = False
    Private _IsSupplying As Boolean = False
    Private _IsLooting As Boolean = False
    Private _CureEventRunning As Boolean = False
    Private _IsJumping As Boolean = False

    Private _Follow As Long = 0

    Private _MobList As New Dictionary(Of String, Boolean)()
    Private _LootStore As New List(Of String)

    Private _RepairBackX As Long = 0
    Private _RepairBackY As Long = 0

    Private _Supply(,) As String = {
        {"1", "SupplyHpPotion", "SupplyHpPotionCount", "SupplyHpPotionItem"},                   ' HP Potion
        {"1", "SupplyMpPotion", "SupplyMpPotionCount", "SupplyMpPotionItem"},                   ' HP Potion
        {"2", "SupplyArrow", "SupplyArrowCount", "Arrow"},                                      ' Arrow
        {"2", "SupplyWolf", "SupplyWolfCount", "Wolf"},                                         ' Wolf
        {"2", "SupplyTsGem", "SupplyTsGemCount", "Transformation Gem"},                         ' TS Gem
        {"2", "SupplyBook", "SupplyBookCount", "Prayer Of God's Power"},                        ' Kitap
        {"2", "SupplyMasterStone", "SupplyMasterStoneCount", "Master Taş"},                     ' Master Taş
        {"3", "SupplyInnHpPotion", "SupplyInnHpPotionCount", "SupplyInnHpPotionItem"},          ' HP Potion (Inn Hostes)
        {"3", "SupplyInnMpPotion", "SupplyInnMpPotionCount", "SupplyInnMpPotionItem"}           ' MP Potion (Inn Hostes)
    }

    Private _SupplyStore As New List(Of Tuple(Of String, String, String))
    Private _SupplyLocationStore As New List(Of Tuple(Of String, String, String, String, String, String))

    Private _SkillStore As New List(Of Tuple(Of String, String, String, String, String))

    Private _AttackSkillStore As New Dictionary(Of String, String)
    Private _ActiveAttackSkillStore As New Dictionary(Of String, String)

    Private _TimedSkillStore As New Dictionary(Of String, String)
    Private _ActiveTimedSkillStore As New Dictionary(Of String, String)

    Private _DeBuffSkillStore As New Dictionary(Of String, String)
    Private _ActiveDeBuffSkillStore As New Dictionary(Of String, String)

    Private _SupplyBackX As Long = 0
    Private _SupplyBackY As Long = 0

    Private _ControlListLoaded As Boolean = False

#End Region

#Region "Read Memory Function Helpers"
    Private Function ReadByte(ByVal Address As Long) As Byte
        Return Module1.ReadByte(_Handle, Address)
    End Function

    Private Function ReadLong(ByVal Address As Long) As Long
        Return Module1.ReadLong(_Handle, Address)
    End Function

    Private Function ReadFloat(ByVal Address As Long) As Single
        Return Module1.ReadFloat(_Handle, Address)
    End Function

    Private Function ReadString(ByVal Address As Integer, ByVal Size As Integer) As String
        Return Module1.ReadString(_Handle, Address, Size)
    End Function
#End Region

#Region "Write Memory Function Helpers"
    Private Sub WriteLong(ByVal Address As Long, ByVal Value As Long)
        Module1.WriteLong(_Handle, Address, Value)
    End Sub
    Private Sub WriteByte(ByVal Address As Long, ByVal Value As Long)
        Module1.WriteByte(_Handle, Address, Value)
    End Sub
    Private Sub WriteFloat(ByVal Address As Long, ByVal Value As Long)
        Module1.WriteFloat(_Handle, Address, Value)
    End Sub
#End Region

#Region "Game Function Helpers"
    Private Sub InjectPatch(ByVal Address As Long, ByVal Code As String)
        Module1.InjectPatch(_Handle, Address, Code)
    End Sub

    Private Sub ExecuteRemoteCode(ByVal Code As String)
        Module1.ExecuteRemoteCode(_Handle, _CodeMem, Code)
    End Sub

    Public Sub SendPacket(ByVal Packet As String)
        Module1.SendPacket(_Handle, _PacketMem, _CodeMem, Packet)
    End Sub
#End Region

#Region "Application Functions"
    Public Sub New(App As App, Process As Long, Handle As Long)
        _Process = Process
        _Handle = Handle
        _CharacterName = GetName()

        _Configuration = New Configuration(Me)

        _App = App

        _SkillStore = _App.GetMainDatabase().GetAllSkill()
        _SupplyLocationStore = _App.GetMainDatabase().GetAllSupplyLocation()

        LoadAllSkillData()
        LoadAllControl()

        _BlackMarketerMem = VirtualAllocEx(_Handle, 0, 1, MEM_COMMIT, &H10)
        _CodeMem = VirtualAllocEx(_Handle, 0, 1, MEM_COMMIT, &H10)
        _PacketMem = VirtualAllocEx(_Handle, 0, 1, MEM_COMMIT, &H10)

        InitializeMailSlot()

        _EventThread = New Thread(AddressOf EventLoop)
        _EventThread.IsBackground = True
        _EventThread.Start()

        _AttackSkillEventThread = New Thread(AddressOf AttackSkillEventThread)
        _AttackSkillEventThread.IsBackground = True

        _TimedSkillEventThread = New Thread(AddressOf TimedSkillEventThread)
        _TimedSkillEventThread.IsBackground = True

        _DeBuffSkillEventThread = New Thread(AddressOf DebuffSkillEventThread)
        _DeBuffSkillEventThread.IsBackground = True
    End Sub

    Public Function GetConfiguration() As Configuration
        Return _Configuration
    End Function

    Private Sub InitializeMailSlot()
        Dim CreateFileAddr As IntPtr
        Dim WriteFileAddr As IntPtr
        Dim CloseHandleAddr As IntPtr
        Dim KO_PTR_RCVFNC As IntPtr
        Dim KO_PTR_RECVHK As Long
        Dim KO_PTR_RCVHKB As Long

        Dim pStr As String
        Dim MAILSLOT_NAME As String

        Dim enc As New System.Text.UnicodeEncoding()

        KO_PTR_RECVHK = ReadLong(ReadLong(KO_PTR_DLG - &H14)) + &H8
        KO_PTR_RCVHKB = ReadLong(KO_PTR_RECVHK)

        MAILSLOT_NAME = "\\.\mailslot\KNIGHTONLINE" & CStr(_Process)
        _MailSlotHandle = CreateMailslot(MAILSLOT_NAME, 0, -1, 0)

        CreateFileAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "CreateFileW")
        WriteFileAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "WriteFile")
        CloseHandleAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "CloseHandle")

        KO_PTR_RCVFNC = VirtualAllocEx(_Handle, IntPtr.Zero, 1, MEM_COMMIT, PAGE_READWRITE)

        Dim pBytesMSName() As Byte = enc.GetBytes(MAILSLOT_NAME)

        WriteProcessMemory2(_Handle, New IntPtr(Convert.ToInt32(KO_PTR_RCVFNC.ToString()) + &H400), pBytesMSName, Marshal.SizeOf(GetType(System.Byte)) * pBytesMSName.Length, 0)

        pStr = "608B4424248B40088905" & AlignDWORD(KO_PTR_RCVFNC + &H104) & "8B4424248B40048905" &
                    AlignDWORD(KO_PTR_RCVFNC + &H100) & "3D004000007D3D6A0068800000006A036A006A01680000004068" &
                    AlignDWORD(KO_PTR_RCVFNC + &H400) & "E8" & AddressDistance(KO_PTR_RCVFNC + &H39, CreateFileAddr) &
                    "83F8FF741C6A005490FF35" & AlignDWORD(KO_PTR_RCVFNC + &H100) & "FF35" & AlignDWORD(KO_PTR_RCVFNC + &H104) &
                    "50E8" & AddressDistance(KO_PTR_RCVFNC + &H54, WriteFileAddr) & "50E8" &
                    AddressDistance(KO_PTR_RCVFNC + &H5A, CloseHandleAddr) & "61E9" &
                    AddressDistance(KO_PTR_RCVFNC + &H60, KO_PTR_RCVHKB)

        Dim pbytes() As Byte = StringToByte(pStr)
        WriteProcessMemory1(_Handle, KO_PTR_RCVFNC, pbytes(LBound(pbytes)), UBound(pbytes) - LBound(pbytes), 0&)

        WriteLong(KO_PTR_RECVHK, KO_PTR_RCVFNC)

        _MailSlotThread = New Thread(Sub() ReadRecvMessage())

        With _MailSlotThread
            .IsBackground = True
            .Start()
        End With
    End Sub

    Public Function GetProcess() As Long
        Return _Process
    End Function

    Public Function IsFollowDisable() As Boolean
        Return _FollowDisable
    End Function

    Public Sub InitializeAllControl()
        Dim Control As Control = _Configuration.GetNextControl(_Configuration, True)

        Do Until Control Is Nothing
            Control = _Configuration.GetNextControl(Control, True)

            If Control Is Nothing Then Continue Do
            If Control.GetType = GetType(CheckBox) Then
                Dim _CheckBox As CheckBox = TryCast(Control, CheckBox)
                _CheckBox.Checked = GetControl(_CheckBox.Name, _CheckBox.Checked)
            ElseIf Control.GetType = GetType(NumericUpDown) Then
                Dim _NumericUpDown As NumericUpDown = TryCast(Control, NumericUpDown)
                _NumericUpDown.Value = GetControl(_NumericUpDown.Name, _NumericUpDown.Value)
            ElseIf Control.GetType = GetType(ComboBox) Then
                Dim _ComboBox As ComboBox = TryCast(Control, ComboBox)
                _ComboBox.SelectedIndex = 0
                _ComboBox.SelectedItem = GetControl(_ComboBox.Name, _ComboBox.SelectedItem)
            ElseIf Control.GetType = GetType(TextBox) Then
                Dim _TextBox As TextBox = TryCast(Control, TextBox)
                _TextBox.Text = GetControl(_TextBox.Name, _TextBox.Text)
            End If
        Loop

    End Sub

    Public Function GetControl(ByVal Param As String, Optional ByVal DefaultValue As String = "") As String
        If _ControlList.ContainsKey(Param) Then
            Return _ControlList(Param)
        End If

        If DefaultValue <> "" Then
            SetControl(Param, DefaultValue)
        End If

        GetControl = DefaultValue
    End Function

    Public Sub SetControl(ByVal Param As String, ByVal Value As String)
        If _ControlList.ContainsKey(Param) Then
            _ControlList(Param) = Value
        Else
            _ControlList.Add(Param, Value)
        End If
    End Sub

    Public Sub SetActiveSkill(ByVal Index As String, ByVal Type As String)
        If Type = 1 Then
            Dim Skill = DirectCast(_Configuration.ActiveSkillList.Items(Index), KeyValuePair(Of String, String)).Key
            If _ActiveAttackSkillStore.ContainsKey(Skill) = False Then
                _ActiveAttackSkillStore.Add(Skill, 0)
                _App.GetUserDatabase().SetActiveSkill(GetName(), Skill, 1)
            End If
        ElseIf Type = 2 Then
            Dim Skill = DirectCast(_Configuration.TimedSkillList.Items(Index), KeyValuePair(Of String, String)).Key
            If _ActiveTimedSkillStore.ContainsKey(Skill) = False Then
                _ActiveTimedSkillStore.Add(Skill, 0)
                _App.GetUserDatabase().SetActiveSkill(GetName(), Skill, 2)
            End If
        Else
            Dim Skill = DirectCast(_Configuration.DeBuffSkillList.Items(Index), KeyValuePair(Of String, String)).Key
            If _ActiveDeBuffSkillStore.ContainsKey(Skill) = False Then
                _ActiveDeBuffSkillStore.Add(Skill, 0)
                _App.GetUserDatabase().SetActiveSkill(GetName(), Skill, 3)
            End If
        End If
    End Sub

    Public Sub DeleteActiveSkill(ByVal Index As String, ByVal Type As String)
        If Type = 1 Then
            Dim Skill = DirectCast(_Configuration.ActiveSkillList.Items(Index), KeyValuePair(Of String, String)).Key
            If _ActiveAttackSkillStore.ContainsKey(Skill) = True Then
                _ActiveAttackSkillStore.Remove(Skill)
                _App.GetUserDatabase().DeleteActiveSkill(GetName(), Skill, 1)
            End If
        ElseIf Type = 2 Then
            Dim Skill = DirectCast(_Configuration.TimedSkillList.Items(Index), KeyValuePair(Of String, String)).Key
            If _ActiveTimedSkillStore.ContainsKey(Skill) = True Then
                _ActiveTimedSkillStore.Remove(Skill)
                _App.GetUserDatabase().DeleteActiveSkill(GetName(), Skill, 2)
            End If
        Else
            Dim Skill = DirectCast(_Configuration.DeBuffSkillList.Items(Index), KeyValuePair(Of String, String)).Key
            If _ActiveDeBuffSkillStore.ContainsKey(Skill) = True Then
                _ActiveDeBuffSkillStore.Remove(Skill)
                _App.GetUserDatabase().DeleteActiveSkill(GetName(), Skill, 3)
            End If
        End If
    End Sub

    Public Sub LoadAllSkillData()
        Dim SkillStore = _SkillStore.Select(Function(x) New String() {x.Item1, x.Item2, x.Item3, x.Item4, x.Item5}).ToArray()

        For Each Skill As String() In SkillStore

            If GetJob(GetClass()).Equals(Skill(1)) = False Then Continue For
            If Skill(4) = 1 Then
                _AttackSkillStore.Add(Skill(0), Skill(2))
            ElseIf Skill(4) = 2 Then
                _TimedSkillStore.Add(Skill(0), Skill(2))
            Else
                _DeBuffSkillStore.Add(Skill(0), Skill(2))
            End If
        Next

        If _AttackSkillStore.Count() <> 0 Then
            _Configuration.ActiveSkillList.DataSource = New BindingSource(_AttackSkillStore, Nothing)
            _Configuration.ActiveSkillList.DisplayMember = "Value"
            _Configuration.ActiveSkillList.ValueMember = "Key"
        End If

        If _TimedSkillStore.Count() <> 0 Then
            _Configuration.TimedSkillList.DataSource = New BindingSource(_TimedSkillStore, Nothing)
            _Configuration.TimedSkillList.DisplayMember = "Value"
            _Configuration.TimedSkillList.ValueMember = "Key"
        End If

        If _DeBuffSkillStore.Count() <> 0 Then
            _Configuration.DeBuffSkillList.DataSource = New BindingSource(_DeBuffSkillStore, Nothing)
            _Configuration.DeBuffSkillList.DisplayMember = "Value"
            _Configuration.DeBuffSkillList.ValueMember = "Key"
        End If

        If (_App.GetUserDatabase().GetAllActiveSkill(_CharacterName) Is Nothing) Then Return
        For Each Skill As KeyValuePair(Of String, String) In _App.GetUserDatabase().GetAllActiveSkill(_CharacterName)

            If Skill.Value = 1 Then
                _ActiveAttackSkillStore.Add(Skill.Key, 0)

                For i As Long = 0 To _Configuration.ActiveSkillList.Items.Count() - 1
                    Dim FindSkill = DirectCast(_Configuration.ActiveSkillList.Items(i), KeyValuePair(Of String, String)).Key
                    If Skill.Key = FindSkill Then
                        _Configuration.ActiveSkillList.SetItemChecked(i, True)
                    End If
                Next
            ElseIf Skill.Value = 2 Then
                _ActiveTimedSkillStore.Add(Skill.Key, 0)

                For i As Long = 0 To _Configuration.TimedSkillList.Items.Count() - 1
                    Dim FindSkill = DirectCast(_Configuration.TimedSkillList.Items(i), KeyValuePair(Of String, String)).Key
                    If Skill.Key = FindSkill Then
                        _Configuration.TimedSkillList.SetItemChecked(i, True)
                    End If
                Next
            Else
                _ActiveDeBuffSkillStore.Add(Skill.Key, 0)

                For i As Long = 0 To _Configuration.DeBuffSkillList.Items.Count() - 1
                    Dim FindSkill = DirectCast(_Configuration.DeBuffSkillList.Items(i), KeyValuePair(Of String, String)).Key
                    If Skill.Key = FindSkill Then
                        _Configuration.DeBuffSkillList.SetItemChecked(i, True)
                    End If
                Next
            End If
        Next
    End Sub

    Public Function GetSkillData(ByVal Id As String, ByVal Job As String) As Tuple(Of String, String, String, String)
        Dim SkillStore = _SkillStore.Select(Function(x) New String() {x.Item1, x.Item2, x.Item3, x.Item4}).ToArray()
        For Each Skill As String() In SkillStore
            If Skill(0) = Id And Skill(1).Equals(Job) Then
                Return New Tuple(Of String, String, String, String)(Skill(0), Skill(1), Skill(2), Skill(3))
            End If
        Next
        Return Nothing
    End Function

    Public Sub LoadAllControl()
        If _ControlListLoaded = False Then
            'If (_App.GetUserDatabase().GetAllControl(_CharacterName) Is Nothing) Then Return
            For Each Control As KeyValuePair(Of String, String) In _App.GetUserDatabase().GetAllControl(_CharacterName)
                If _ControlList.ContainsKey(Control.Key) Then
                    _ControlList(Control.Key) = Control.Value
                Else
                    _ControlList.Add(Control.Key, Control.Value)
                End If
            Next
            _ControlListLoaded = True
        End If

        InitializeAllControl()
    End Sub

    Public Sub SaveAllControl()
        If (_ControlList.Count() = 0) Then Return
        For Each Config As KeyValuePair(Of String, String) In _ControlList
            _App.GetUserDatabase().SetControl(_CharacterName, Config.Key, Config.Value)
        Next
    End Sub

    Public Sub SetSkill(ByVal Id As String, ByVal Type As String)
        If _ControlList.ContainsKey(Id) = False Then
            _ControlList.Add(Type, Type)
        Else
            _ControlList(Type) = Type
        End If
    End Sub

    Public Function GetFollower() As Long
        Return _Follow
    End Function

    Public Sub SetFollower(ByVal Follow As Long)
        _Follow = Follow
    End Sub

    Public Function GetFollowDisable() As Boolean
        Return _FollowDisable
    End Function

    Public Function GetMobListCheckEnabled() As Boolean
        Return _MobList.ContainsValue(True)
    End Function

    Public Sub SetMobListState(ByVal Index As String, ByVal Value As Boolean)
        If _MobList.ContainsKey(Index) Then
            _MobList(Index) = Value
        Else
            _MobList.Add(Index, Value)
        End If
    End Sub

    Public Sub ClearMobListState()
        _MobList.Clear()
    End Sub

    Public Function GetMobListState(ByVal Index As String) As Boolean
        If _MobList.ContainsKey(Index) Then
            Return _MobList(Index)
        End If
        Return False
    End Function

    Public Function IsRepairing() As Boolean
        Return _IsRepairing
    End Function

    Public Function IsSupplying() As Boolean
        Return _IsSupplying
    End Function

    Public Sub EventLoop()
        Do While (True)
            If (_ControlList.Count() = 0) Then Continue Do

            Console.WriteLine(GetState())

            Dim TargetBaseAddr1 As Long = GetTargetBase()

            If TargetBaseAddr1 <> 0 Then
                Dim MobNameLen As Long = ReadByte(TargetBaseAddr1 + KO_OFF_STATE)
                'Console.WriteLine(MobNameLen)
            End If

            If _Configuration.Wallhack.Checked Then
                If ReadLong(ReadLong(KO_PTR_CHR) + KO_OFF_WH) = 1 Then
                    WriteLong(ReadLong(KO_PTR_CHR) + KO_OFF_WH, 0)
                End If
            Else
                If ReadLong(ReadLong(KO_PTR_CHR) + KO_OFF_WH) = 0 Then
                    WriteLong(ReadLong(KO_PTR_CHR) + KO_OFF_WH, 1)
                End If
            End If

            If _Configuration.Oreads.Checked Then
                If ReadLong(ReadLong(ReadLong(KO_PTR_CHR) + &H58) + &H5C4) = 0 Then
                    ExecuteRemoteCode("608B0D" + AlignDWORD(KO_PTR_CHR) + "6A006858BFB929B8" + AlignDWORD(KO_PTR_FAKE_ITEM) + "FFD061C3")

                    WriteLong(ReadLong(ReadLong(KO_PTR_CHR) + &H58) + &H5C4, 1)
                    WriteLong(ReadLong(ReadLong(KO_PTR_CHR) + &H58) + &H5C6, 1)
                    WriteLong(ReadLong(ReadLong(KO_PTR_CHR) + &H58) + &H5C7, 1)
                End If
            Else
                If ReadLong(ReadLong(ReadLong(KO_PTR_CHR) + &H58) + &H5C4) <> 0 Then
                    ExecuteRemoteCode("608B0D" + AlignDWORD(KO_PTR_CHR) + "6A006858BFB929B8" + AlignDWORD(KO_PTR_FAKE_ITEM) + "FFD061C3")

                    WriteLong(ReadLong(ReadLong(KO_PTR_CHR) + &H58) + &H5C4, 0)
                    WriteLong(ReadLong(ReadLong(KO_PTR_CHR) + &H58) + &H5C6, 0)
                    WriteLong(ReadLong(ReadLong(KO_PTR_CHR) + &H58) + &H5C7, 0)
                End If
            End If

            If GetAsyncKeyState(71) <> 0 And _Configuration.SpeedHack.Checked Then ' Speed Hack (G)
                If GetTargetID() <> "FFFF" Then
                    SetCoordinate(GetTargetX(), GetTargetY())
                Else
                    If CoordinateDistance(GetX(), GetY(), GetMouseX(), GetMouseY()) <= 150 Then
                        SetCoordinate(GetMouseX(), GetMouseY())
                    End If
                End If
            End If

            If GetHp() > 0 And Environment.TickCount - _AttackEventTime >= _Configuration.AttackSpeed.Value And _Configuration.Attack.Checked And _IsJumping = False And _IsRepairing = False And _IsSupplying = False Then ' Attack Event

                If _AttackSkillEventThread.IsAlive = False Then
                    _AttackSkillEventThread = New Thread(AddressOf AttackSkillEventThread)
                    _AttackSkillEventThread.IsBackground = True
                    _AttackSkillEventThread.Start()
                End If

                _AttackEventTime = Environment.TickCount
            End If

            If GetHp() > 0 And Environment.TickCount - _TimedSkillEventTime >= 750 Then ' Timed Skill Event
                If _TimedSkillEventThread.IsAlive = False Then
                    _TimedSkillEventThread = New Thread(AddressOf TimedSkillEventThread)
                    _TimedSkillEventThread.IsBackground = True
                    _TimedSkillEventThread.Start()
                End If

                _TimedSkillEventTime = Environment.TickCount
            End If

            If GetHp() > 0 And Environment.TickCount - _DeBuffSkillEventTime >= 1250 And _Configuration.Attack.Checked And _IsJumping = False And _IsRepairing = False And _IsSupplying = False Then ' DeBuff Skill Event
                If _DeBuffSkillEventThread.IsAlive = False Then
                    _DeBuffSkillEventThread = New Thread(AddressOf DebuffSkillEventThread)
                    _DeBuffSkillEventThread.IsBackground = True
                    _DeBuffSkillEventThread.Start()
                End If

                _DeBuffSkillEventTime = Environment.TickCount
            End If

            If _Configuration.BlackMarketer.Checked And Environment.TickCount - _BlackMarketerEventTime >= _Configuration.BlackMarketerEventTime.Value Then ' Blackmarketer Event
                For i As Long = 0 To _Configuration.BlackMarketerLoop.Value
                    InjectPatch(_BlackMarketerMem, "608B0D" &
                        AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(_BlackMarketerMem + &H100) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                        AlignDWORD(KO_PTR_PKT) & "6A1768" & AlignDWORD(_BlackMarketerMem + &H120) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                        AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(_BlackMarketerMem + &H140) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                        AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(_BlackMarketerMem + &H160) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                        AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(_BlackMarketerMem + &H180) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761C3")

                    InjectPatch(_BlackMarketerMem + &H100, "20018E38FFFFFFFF")
                    InjectPatch(_BlackMarketerMem + &H120, "55000D32353032325F425F4D2E6C7561")
                    InjectPatch(_BlackMarketerMem + &H140, "55000D32353032325F425F4D2E6C7561")
                    InjectPatch(_BlackMarketerMem + &H160, "55000D32353032325F425F4D2E6C7561")
                    InjectPatch(_BlackMarketerMem + &H180, "55000D32353032325F425F4D2E6C7561")

                    Dim hMultipleShotThread As Integer = CreateRemoteThread(_Handle, 0, 0, _BlackMarketerMem, 0, 0, 0)
                    WaitForSingleObject(hMultipleShotThread, &HFFFF)
                    CloseHandle(hMultipleShotThread)
                Next

                _BlackMarketerEventTime = Environment.TickCount
            End If

            If GetHp() > 0 And Environment.TickCount - _TransformationEventTime >= 250 Then
                If _Configuration.Transformation.Checked Then
                    UseTransformation(GetControl("TransformationName"))
                End If

                _TransformationEventTime = Environment.TickCount
            End If

            If Environment.TickCount - _ProtectionEventTime >= 2500 And GetHp() > 0 Then ' Protection Event
                Dim percentHP As Long = CInt(Math.Round((100 * GetHp()) / GetMaxHp()))

                If GetHp() < GetMaxHp() And percentHP < _Configuration.HpPercent.Value And _Configuration.Hp.Checked Then
                    UseHealthPotion(GetControl("HpPotion"))
                End If

                Dim percentMP As Long = CInt(Math.Round((100 * GetMp()) / GetMaxMp()))

                If GetMp() < GetMaxMp() And percentMP < _Configuration.MpPercent.Value And _Configuration.Mp.Checked Then
                    UseManaPotion(GetControl("MpPotion"))
                End If

                _ProtectionEventTime = Environment.TickCount
            End If

            If Environment.TickCount - _MinorEventTime >= 250 And GetHp() > 0 And GetHp() < GetMaxHp() Then ' Minor Event
                Dim percentHP As Long = CInt(Math.Round((100 * GetHp()) / GetMaxHp()))

                If percentHP < _Configuration.MinorPercent.Value And _Configuration.Minor.Checked Then
                    UseMinorHealing(GetLongID())
                End If
                _MinorEventTime = Environment.TickCount
            End If

            If Environment.TickCount - _PriestPartyControlEventTime >= 1250 And GetHp() > 0 And _IsRepairing = False And _IsSupplying = False And GetJob(GetClass()) = "Priest" Then ' Priest Party Event
                _PartyPriestEventThread = New Thread(AddressOf PartyPriestEventThread)
                _PartyPriestEventThread.IsBackground = True
                _PartyPriestEventThread.Start()

                _PriestPartyControlEventTime = Environment.TickCount
            End If

            If Environment.TickCount - _RoguePartyControlEventTime >= 250 And GetHp() > 0 And _IsRepairing = False And _IsSupplying = False And GetJob(GetClass()) = "Rogue" Then ' Rogue Party Event
                If _Configuration.PartyMinor.Checked = True Then
                    _PartyRogueEventThread = New Thread(AddressOf PartyRogueEvent)
                    _PartyRogueEventThread.IsBackground = True
                    _PartyRogueEventThread.Start()
                End If

                _RoguePartyControlEventTime = Environment.TickCount
            End If

            If Environment.TickCount - _TargetEventTime >= 250 And _IsJumping = False And (_Follow = 0 Or _FollowDisable = True) And (_Configuration.Target.Checked Or GetMobListCheckEnabled()) And _IsRepairing = False And _IsSupplying = False Then ' Target Event
                Dim TargetID As String = GetTargetID()
                If GetTargetID() = "FFFF" Then
                    Dim TargetFindID As Long = FindMob()

                    If TargetFindID <> 0 Then
                        SelectTarget(TargetFindID)
                    End If
                Else
                    If GetMobListCheckEnabled() Then
                        Dim TargetBaseAddr As Long = GetTargetBase()

                        If TargetBaseAddr <> 0 Then
                            Dim MobName As String = ""
                            Dim MobNameLen As Long = ReadLong(TargetBaseAddr + KO_OFF_NAME_LEN)
                            If MobNameLen > 15 Then
                                MobName = ReadString(ReadLong(TargetBaseAddr + KO_OFF_NAME), MobNameLen)
                            Else
                                MobName = ReadString(TargetBaseAddr + KO_OFF_NAME, MobNameLen)
                            End If

                            For Each Mob As KeyValuePair(Of String, Boolean) In _MobList
                                If Mob.Key = MobName And Mob.Value = False Then
                                    SelectTarget(-1)
                                End If
                            Next
                        End If
                    End If

                    If IsSelectNextTarget() Then
                        Dim TargetFindID As Long = FindMob()

                        If TargetFindID <> 0 Then
                            SelectTarget(TargetFindID)
                        End If
                    End If
                End If

                _TargetEventTime = Environment.TickCount
            End If

            If Environment.TickCount - _FollowEventTime >= 250 And _Follow <> 0 And _FollowDisable = False And GetHp() > 0 And _IsRepairing = False And _IsSupplying = False Then ' Follow Event

                Dim Client As Client = _App.GetClient(_Follow)

                If Client IsNot Nothing Then
                    If Client.GetHp() > 0 And Client.IsRepairing() = False And Client.IsSupplying() = False And Client.GetZone() = GetZone() Then

                        If Client.IsAttackableTarget() And Client.GetTargetID() <> "FFFF" And GetTargetLongID() <> Client.GetTargetLongID() Then
                            SelectTarget(Client.GetTargetLongID())
                        End If

                        If Client.GetX() <> GetX() Or Client.GetY() <> GetY() Then
                            Dim Distance = CoordinateDistance(Client.GetX(), GetY(), Client.GetY(), GetY())

                            If (Distance >= 1 And Distance <= 3) Then
                                Move(Client.GetX(), Client.GetY())
                            Else
                                SetCoordinate(Client.GetX(), Client.GetY())
                            End If

                        End If
                    End If
                End If

                _FollowEventTime = Environment.TickCount
            End If

            If Environment.TickCount - _ActionEventTime >= 250 And _IsJumping = False And (_Follow = 0 Or _FollowDisable = True) And GetHp() > 0 And _IsRepairing = False And _IsSupplying = False Then ' Action Event
                If GetTargetID() <> "FFFF" And IsAttackableTarget() And _Configuration.Attack.Checked Then
                    Dim TargetAddr As Long = GetTargetBase()
                    Dim TargetId As Long = ReadLong(TargetAddr + KO_OFF_ID)
                    Dim TargetX As Long = CInt(ReadFloat(TargetAddr + KO_OFF_X))
                    Dim TargetY As Long = CInt(ReadFloat(TargetAddr + KO_OFF_Y))

                    'Console.WriteLine(CoordinateDistance(GetX(), GetY(), TargetX, TargetY))

                    If GetTargetLongID() = TargetId And _Configuration.ActionMove.Checked And (GetX() <> TargetX Or GetY() <> TargetY) Then
                        Move(TargetX, TargetY)
                    ElseIf GetTargetLongID() = TargetId And _Configuration.ActionSetCoordinate.Checked And (GetX() <> TargetX Or GetY() <> TargetY) Then
                        Dim Distance = CoordinateDistance(GetX(), GetY(), TargetX, TargetY)

                        If (Distance >= 1 And Distance <= 3) Then
                            Move(TargetX, TargetY)
                        Else
                            SetCoordinate(TargetX, TargetY)
                        End If
                    End If

                    _ActionEventTime = Environment.TickCount
                End If

            End If

            If _Configuration.DeathOnBorn.Checked And Environment.TickCount - _CharacterDeadEventTime >= 3000 Then ' Character Dead Event
                If GetHp() = 0 And GetID() <> "FFFF" Then
                    SendPacket("1200")
                End If
                _CharacterDeadEventTime = Environment.TickCount
            End If

            If Environment.TickCount - _AreaControlEventTime >= 3000 And _IsJumping = False And (_Configuration.AreaControl.Checked Or (_Follow <> 0 And _FollowDisable = False)) And GetHp() > 0 And _IsRepairing = False And _IsSupplying = False Then ' Area Control Event
                If _Follow <> 0 Then
                    Dim Client As Client = _App.GetClient(_Follow)

                    If Client.GetHp() > 0 And Client.GetControl("AreaControl") = True And Client.GetControl("AreaControlX") > 0 And Client.GetControl("AreaControlY") > 0 Then

                        If CoordinateDistance(GetX(), GetY(), Client.GetControl("AreaControlX"), Client.GetControl("AreaControlY")) > Client.GetControl("TargetRange") Then
                            SelectTarget(-1)
                            SetCoordinate(Client.GetControl("AreaControlX"), Client.GetControl("AreaControlY"))
                        End If
                    End If
                Else
                    If CoordinateDistance(GetX(), GetY(), _Configuration.AreaControlX.Value, _Configuration.AreaControlY.Value) > _Configuration.TargetRange.Value And _Configuration.AreaControlX.Value > 0 And _Configuration.AreaControlY.Value > 0 Then
                        SelectTarget(-1)
                        SetCoordinate(_Configuration.AreaControlX.Value, _Configuration.AreaControlY.Value)
                    End If
                End If
                _AreaControlEventTime = Environment.TickCount
            End If

            If Environment.TickCount - _RepairEventTime >= 15000 And _IsJumping = False And IsNeedRepair() And _IsRepairing = False And _IsSupplying = False And Environment.TickCount - _RepairAfterWaitTime >= 60000 Then ' Repair Event
                If _Configuration.RepairSunderies.Checked Or _Configuration.RepairMagicHammer.Checked Then
                    _RepairEventThread = New Thread(AddressOf RepairEventThread)
                    _RepairEventThread.IsBackground = True
                    _RepairEventThread.Start()
                End If

                _RepairEventTime = Environment.TickCount
            End If

            If Environment.TickCount - _SupplyEventTime >= 15000 And _IsJumping = False And _IsRepairing = False And _IsSupplying = False And Environment.TickCount - _SupplyAfterWaitTime >= 60000 Then ' Supply Event
                _SupplyEventThread = New Thread(AddressOf SupplyEventThread)
                _SupplyEventThread.IsBackground = True
                _SupplyEventThread.Start()

                _SupplyEventTime = Environment.TickCount
            End If

            If GetHp() > 0 And Environment.TickCount - _AreaHealEventTime >= 2000 And _Configuration.AreaHeal.Checked Then ' Area Heal Event
                If GetPartyCount() > 0 And GetHp() = GetMaxHp() Then
                    If PartyMemberNeedHeal() Then
                        SendPacket("3103" & "1C820700" & GetID() & "FFFF00000000000000000000000000000000")
                        _AreaHealEventTime = Environment.TickCount
                    End If
                ElseIf GetHp() <> GetMaxHp() Then
                    SendPacket("3103" & "1C820700" & GetID() & "FFFF00000000000000000000000000000000")
                    _AreaHealEventTime = Environment.TickCount
                End If
            End If

            If GetHp() > 0 And Environment.TickCount - _StatScrollEventTime >= 3000 And _Configuration.StatScroll.Checked Then ' Stat Scroll Event
                If IsRightSkillAffected(59) = False Then
                    SendPacket("3106" & "1B820700" & GetID() & "FFFF00000000000000000000000000000000")
                    SendPacket("3103" & "1B820700" & GetID() & "FFFF00000000000000000000000000000000")
                End If

                _StatScrollEventTime = Environment.TickCount
            End If

            If GetHp() > 0 And Environment.TickCount - _AttackScrollEventTime >= 3000 And _Configuration.AttackScroll.Checked Then ' Attack Scroll Event
                If IsRightSkillAffected(271) = False Then
                    SendPacket("3103" & "2FA20700" & GetID() & GetID() & "00000000000000000000000000000000")
                End If

                _AttackScrollEventTime = Environment.TickCount
            End If

            If GetHp() > 0 And Environment.TickCount - _DefanceScrollEventTime >= 3000 And _Configuration.AcScroll.Checked Then ' Ac Scroll Event
                If IsRightSkillAffected(61) = False Then
                    SendPacket("3103" & "1D820700" & GetID() & "FFFF00000000000000000000000000000000")
                End If

                _DefanceScrollEventTime = Environment.TickCount
            End If

            If Environment.TickCount - _DropScrollEventTime >= 3000 And GetHp() > 0 And _Configuration.DropScroll.Checked Then ' Drop Scroll Event
                If IsRightSkillAffected(23) = False And IsRightSkillAffected(24) = False Then
                    SendPacket("3103" & "F7810700" & GetID() & GetID() & "00000000000000000000000000000000")
                    SendPacket("3103" & "F8810700" & GetID() & GetID() & "00000000000000000000000000000000")
                End If

                _DropScrollEventTime = Environment.TickCount
            End If

            Thread.Sleep(1)
        Loop
    End Sub

    Public Sub AttackSkillEventThread()
        For Each i As String In _ActiveAttackSkillStore.Keys.ToArray()
            If _ActiveAttackSkillStore.ContainsKey(i) = False Then Continue For
            Dim UseTime As Long = (DateTime.UtcNow - New DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds
            Dim SkillData = GetSkillData(i, GetJob(GetClass()))
            Dim LastUseTime = _ActiveAttackSkillStore(i)
            If SkillData Is Nothing Then Continue For
            If SkillData.Item4 = 0 Or UseTime > LastUseTime + (SkillData.Item4 + 1) Then
                If UseSkill(SkillData.Item3, _Configuration.AttackDirect.Checked) Then
                    _ActiveAttackSkillStore(i) = UseTime

                    If i <> 1000 Then
                        Thread.Sleep(_Configuration.AttackSpeed.Value)
                    End If
                End If
            End If
        Next
    End Sub

    Public Sub TimedSkillEventThread()
        For Each i As String In _ActiveTimedSkillStore.Keys.ToArray()
            If _ActiveTimedSkillStore.ContainsKey(i) = False Then Continue For
            Dim UseTime As Long = (DateTime.UtcNow - New DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds
            Dim SkillData = GetSkillData(i, GetJob(GetClass()))
            Dim LastUseTime = _ActiveTimedSkillStore(i)
            If SkillData Is Nothing Then Continue For
            If SkillData.Item4 = 0 Or UseTime > LastUseTime + (SkillData.Item4 + 1) Then
                If UseTimedSkill(SkillData.Item3) Then
                    'Console.WriteLine(SkillData.Item3)
                    _ActiveTimedSkillStore(i) = UseTime
                    Thread.Sleep(750)
                End If
            End If
        Next
    End Sub

    Public Sub DebuffSkillEventThread()
        For Each i As String In _ActiveDeBuffSkillStore.Keys.ToArray()
            If _ActiveDeBuffSkillStore.ContainsKey(i) = False Then Continue For
            Dim UseTime As Long = (DateTime.UtcNow - New DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds
            Dim SkillData = GetSkillData(i, GetJob(GetClass()))
            Dim LastUseTime = _ActiveDeBuffSkillStore(i)
            If SkillData Is Nothing Then Continue For
            If SkillData.Item4 = 0 Or UseTime > LastUseTime + (SkillData.Item4) Then
                If UseDeBuffSkill(SkillData.Item3) Then
                    _ActiveDeBuffSkillStore(i) = UseTime
                    Thread.Sleep(1250)
                End If
            End If
        Next
    End Sub

    Public Function GetPartyLeaderId() As Long
        Return ReadLong(ReadLong(ReadLong(ReadLong(ReadLong(KO_PTR_DLG) + KO_OFF_PARTY_BASE) + KO_OFF_PARTY) + &H0) + &H8)
    End Function

    Public Function IsPartyMember(ByVal RequestedMemberName As String) As Boolean
        Dim PartyBase As Long = ReadLong(ReadLong(ReadLong(ReadLong(KO_PTR_DLG) + KO_OFF_PARTY_BASE) + KO_OFF_PARTY))

        For i As Long = 0 To GetPartyCount() - 1
            Dim MemberNickLen As Long = ReadLong(PartyBase + KO_OFF_PARTY_NAME_LEN)

            Dim MemberName As String = ""

            If MemberNickLen > 15 Then
                MemberName = ReadString(ReadLong(PartyBase + KO_OFF_PARTY_NAME), MemberNickLen)
            Else
                MemberName = ReadString(PartyBase + KO_OFF_PARTY_NAME, MemberNickLen)
            End If

            If RequestedMemberName = MemberName Then
                Return True
            End If

            PartyBase = ReadLong(PartyBase)
        Next
        Return False
    End Function

    Public Function PartyMemberNeedHeal() As Boolean
        Dim PartyBase As Long = ReadLong(ReadLong(ReadLong(ReadLong(KO_PTR_DLG) + KO_OFF_PARTY_BASE) + KO_OFF_PARTY))
        For i As Long = 0 To GetPartyCount() - 1
            Dim MemberHp As Long = ReadLong(PartyBase + KO_OFF_PARTY_HP)
            Dim MemberMaxHp As Long = ReadLong(PartyBase + KO_OFF_PARTY_MAXHP)
            If MemberHp < MemberMaxHp Then
                Return True
            End If
            PartyBase = ReadLong(PartyBase)
        Next
        Return False
    End Function

    Public Function GetPartyMemberId(ByVal RequestedMemberName As String) As Long
        Dim PartyBase As Long = ReadLong(ReadLong(ReadLong(ReadLong(KO_PTR_DLG) + KO_OFF_PARTY_BASE) + KO_OFF_PARTY))

        For i As Long = 0 To GetPartyCount() - 1
            Dim MemberNickLen As Long = ReadLong(PartyBase + KO_OFF_PARTY_NAME_LEN)

            Dim MemberName As String = ""

            If MemberNickLen > 15 Then
                MemberName = ReadString(ReadLong(PartyBase + KO_OFF_PARTY_NAME), MemberNickLen)
            Else
                MemberName = ReadString(PartyBase + KO_OFF_PARTY_NAME, MemberNickLen)
            End If

            If RequestedMemberName = MemberName Then
                Return ReadLong(PartyBase + KO_OFF_PARTY_ID)
            End If

            PartyBase = ReadLong(PartyBase)
        Next
        Return 0
    End Function

    Public Function GetSupplyLocation(ByVal Type As String, ByVal Zone As String, ByVal Nation As String) As Tuple(Of String, String, String)
        Dim SupplyLocationStore = _SupplyLocationStore.Select(Function(x) New String() {x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6}).ToArray()
        For Each SupplyLocation As String() In SupplyLocationStore
            If SupplyLocation(1) <> Type Or SupplyLocation(2) <> Zone Or (SupplyLocation(3) <> 0 And SupplyLocation(3) <> Nation) Then Continue For
            Return New Tuple(Of String, String, String)(SupplyLocation(0), SupplyLocation(4), SupplyLocation(5))
        Next
        Return Nothing
    End Function

    Public Sub RepairEventThread()
        If _Configuration.RepairSunderies.Checked = True And _Configuration.RepairMagicHammer.Checked = False Then
            Dim RepairLocation = GetSupplyLocation(2, GetZone(), GetNation())

            If RepairLocation IsNot Nothing Then
                _IsRepairing = True
                _RepairBackX = GetX() : _RepairBackY = GetY()

                SendPacket("4800")
                Thread.Sleep(2000)

                If (CoordinateDistance(GetX(), GetY(), RepairLocation.Item2, RepairLocation.Item3) > 3) Then
                    SetCoordinate(RepairLocation.Item2, RepairLocation.Item3)
                    Thread.Sleep(3000)
                End If

                RepairAllEquipment(RepairLocation.Item1)
                Thread.Sleep(2000)
                SupplyEventThread(True)
                Thread.Sleep(2000)

                _RepairAfterWaitTime = Environment.TickCount

                SetCoordinate(_RepairBackX, _RepairBackY)
            End If
        End If

        If _Configuration.RepairMagicHammer.Checked = True And IsNeedRepair() Then
            SendPacket("3103" & AlignDWORD(490202) & GetID() & GetID() & "00000000000000000000000000000000")
        End If
    End Sub

    Public Sub SupplyEventThread(Optional ByVal IsRepairEvent As Boolean = False)

        For i As Long = 0 To _Supply.GetUpperBound(0)
            If GetControl(_Supply(i, 1)) = False Then Continue For

            Dim ItemName As String = GetControl(_Supply(i, 3))

            If (ItemName = "") Then
                ItemName = _Supply(i, 3)
            End If

            Dim ItemID As Long = GetItemID(ItemName)
            Dim ItemInventorySlot As Long = SearchInventory(ItemID)

            If ItemInventorySlot = -1 Then
                _SupplyStore.Add(Tuple.Create(_Supply(i, 0), ItemName, GetControl(_Supply(i, 2))))
            Else
                If IsRepairEvent = False And GetInventoryItemCount(ItemInventorySlot) > 25 Then Continue For
                If GetInventoryItemCount(ItemInventorySlot) < GetControl(_Supply(i, 2)) Then
                    _SupplyStore.Add(Tuple.Create(_Supply(i, 0), ItemName, Math.Abs(GetInventoryItemCount(ItemInventorySlot) - GetControl(_Supply(i, 2))).ToString()))
                End If
            End If
        Next

        If (_SupplyStore.Count() <> 0) Then
            If IsRepairEvent = False Then
                _IsSupplying = True
                _SupplyBackX = GetX() : _SupplyBackY = GetY()

                SendPacket("4800")
                Thread.Sleep(2000)
            End If

            Dim IsRepaired As Boolean = False
            Dim SupplyStore = _SupplyStore.Select(Function(x) New String() {x.Item1, x.Item2, x.Item3}).ToArray()

            For Each Supply As String() In SupplyStore
                Dim SupplyLocation = GetSupplyLocation(Supply(0), GetZone(), GetNation())
                If SupplyLocation Is Nothing Then Continue For
                If IsRepairEvent = True And Supply(0) = 2 Then Continue For

                If (CoordinateDistance(GetX(), GetY(), SupplyLocation.Item2, SupplyLocation.Item3) > 3) Then
                    SetCoordinate(SupplyLocation.Item2, SupplyLocation.Item3)
                    Thread.Sleep(3000)
                End If

                If (IsRepairEvent = False And Supply(0) = 2 And IsRepaired = False) Then
                    IsRepaired = True
                    RepairAllEquipment(SupplyLocation.Item1, True)
                    Thread.Sleep(2000)
                End If

                If Supply(0) = 3 Then
                    WareHouseCheckOutItem(SupplyLocation.Item1, Supply(1), Supply(2))
                    Thread.Sleep(2000)
                Else
                    BuyItem(SupplyLocation.Item1, Supply(1), Supply(2))
                    Thread.Sleep(2000)
                End If
            Next

            _SupplyStore.Clear()

            _SupplyAfterWaitTime = Environment.TickCount

            If IsRepairEvent = False Then
                SetCoordinate(_SupplyBackX, _SupplyBackY)
            End If
        End If
    End Sub

    Public Sub StartUpgradeEvent()
        SendPacket("5B0C0306323932393239")
        For i As Long = 14 To 40
            Dim ItemID As String = AlignDWORD(GetInventoryItemID(i))
            If ItemID <> "00000000" Then
                Dim UpgradeScrollID As String = AlignDWORD(379021000) ' Bus
                'Dim UpgradeScrollID As String = AlignDWORD(379221000) ' Low Class
                'Dim UpgradeScrollID As String = AlignDWORD(379205000) ' Middle Class
                Dim ItemSlot = Strings.Mid(AlignDWORD(i - 14), 1, 2)
                SendPacket("5B02" + "01" + "1427" + ItemID + ItemSlot + UpgradeScrollID + "1B" + "00000000FF00000000FF00000000FF00000000FF00000000FF00000000FF00000000FF00000000FF")
            End If
        Next
        SendPacket("790101001E7A0700")
    End Sub

    Public Sub PartyPriestEventThread()

        For Each ClientList As KeyValuePair(Of String, Client) In _App.GetAllClient()
            Dim Client As Client = ClientList.Value

            If Client.GetHp() = 0 Or Client.GetMaxHp() = 0 Then Continue For
            If IsPartyMember(Client.GetName()) Then
                If _Configuration.PartyBuff.Checked And Client.IsAffectedHealthBuff() = False Then
                    UseBuff(GetHealthBuff(Client.GetMaxHp()), Client.GetLongID())
                End If

                If _Configuration.PartyAc.Checked And Client.IsAffectedDefenceBuff() = False Then
                    UseBuff(GetDefenceBuff(), Client.GetLongID())
                End If

                If _Configuration.PartyMind.Checked And Client.IsAffectedMindBuff() = False Then
                    UseBuff(GetMindBuff(), Client.GetLongID())
                End If
            End If
        Next
    End Sub

    Public Sub PriestCureEventThread()
        _CureEventRunning = True
        Dim PartyBase As Long = ReadLong(ReadLong(ReadLong(ReadLong(KO_PTR_DLG) + KO_OFF_PARTY_BASE) + KO_OFF_PARTY))
        For i As Long = 0 To GetPartyCount() - 1
            Dim MemberId As Long = ReadLong(PartyBase + KO_OFF_PARTY_ID)
            If ReadLong(PartyBase + KO_OFF_PARTY_CURE1) = 256 Then
                UseCure(MemberId)
                Thread.Sleep(3000)
            ElseIf ReadLong(PartyBase + KO_OFF_PARTY_CURE1) = 257 Or ReadLong(PartyBase + KO_OFF_PARTY_CURE1) = 1 Or ReadLong(PartyBase + KO_OFF_PARTY_CURE1) = 65536 Then
                UseDisease(MemberId)
                Thread.Sleep(3000)
            End If
            PartyBase = ReadLong(PartyBase)
        Next
        _CureEventRunning = False
    End Sub

    Public Sub PartyRogueEvent()
        For Each ClientList As KeyValuePair(Of String, Client) In _App.GetAllClient()
            Dim Client As Client = ClientList.Value
            If Client.GetHp() = 0 Then Continue For
            If _Configuration.PartyMinor.Checked Then
                Dim percentHP As Long = CInt(Math.Round((100 * Client.GetHp()) / Client.GetMaxHp()))
                If percentHP < _Configuration.PartyMinorPercent.Value Then
                    UseMinorHealing(Client.GetLongID())
                End If
            End If
        Next
    End Sub

    Public Sub PriestHealEventThread()
        For Each ClientList As KeyValuePair(Of String, Client) In _App.GetAllClient()
            Dim Client As Client = ClientList.Value
            If Client.GetHp() = 0 Then Continue For
            If _Configuration.PartyHeal.Checked Then
                Dim percentHP As Long = CInt(Math.Round((100 * Client.GetHp()) / Client.GetMaxHp()))
                If percentHP < _Configuration.PartyHealPercent.Value Then
                    UseHeal(GetHeal(), Client.GetLongID(), True)
                End If
            End If
        Next
    End Sub

    Public Sub ReadRecvMessage()
        Do While (True)
            If (_ControlList.Count() = 0) Then Continue Do
            If _Configuration.AutoLoot.Checked Then
                Dim MessageSize As Integer
                Dim MessagesLeft As Long
                Dim MessageRead As Long

                GetMailslotInfo(_MailSlotHandle, 0, MessageSize, MessagesLeft, 0)

                Do While (MessagesLeft <> 0)
                    Dim MessageBuffer As [Byte]() = New Byte(MessageSize) {}

                    ReadFile(_MailSlotHandle, MessageBuffer(0), MessageSize, MessageRead, IntPtr.Zero)

                    If MessageRead <> 0 Then

                        Dim Message = ByteToString(MessageBuffer)
                        Select Case Strings.Left(Message, 2)
                            Case "23" ' WIZ_ITEM_DROP

                                If _Configuration.AutoLoot.Checked And Not _LootStore.Contains(Strings.Mid(Message, 7, 4)) Then
                                    _IsLooting = True
                                    _LootStore.Add(Strings.Mid(Message, 7, 4))
                                    SendPacket("24" & Strings.Mid(Message, 7, 8))
                                End If

                            Case "24" ' WIZ_BUNDLE_OPEN_REQ 
                                If _Configuration.AutoLoot.Checked And _LootStore.Contains(Strings.Mid(Message, 3, 4)) Then

                                    If Strings.Mid(Message, 49, 8) <> "00000000" Then
                                        SendPacket("26" & Strings.Mid(Message, 3, 4) & Strings.Mid(Message, 7, 4) & Strings.Mid(Message, 49, 8) & "03" & "00")
                                    End If

                                    If Strings.Mid(Message, 37, 8) <> "00000000" Then
                                        SendPacket("26" & Strings.Mid(Message, 3, 4) & Strings.Mid(Message, 7, 4) & Strings.Mid(Message, 37, 8) & "02" & "00")
                                    End If

                                    If Strings.Mid(Message, 25, 8) <> "00000000" Then
                                        SendPacket("26" & Strings.Mid(Message, 3, 4) & Strings.Mid(Message, 7, 4) & Strings.Mid(Message, 25, 8) & "01" & "00")
                                    End If

                                    If Strings.Mid(Message, 13, 8) <> "00000000" Then
                                        SendPacket("26" & Strings.Mid(Message, 3, 4) & Strings.Mid(Message, 7, 4) & Strings.Mid(Message, 13, 8) & "00" & "00")
                                    End If

                                    _IsLooting = False
                                    _LootStore.Remove(Strings.Mid(Message, 3, 4))
                                End If

                            Case "5B" ' WIZ_BUNDLE_OPEN_REQ 
                                If Strings.Left(Message, 6) = "5B0B01" Then
                                    For c As Long = 0 To 5
                                        SendPacket("5B0B7C3800000000000504030201003C333839323433303030303031333839323434303030303036333839323435303030303036333839323436303030303032333839323731303030303032")
                                        SendPacket("5B0B7C3800000000000505030201003C333836303030303030303031333839323434303030303037333839323435303030303037333839323436303030303033333839323731303030303033")
                                        SendPacket("5B0B7C3800000000000503020106003C333839323434303030303038333839323435303030303038333839323436303030303034333839323439303030303031333839323731303030303034")
                                        SendPacket("5B0B7C3800000000000503020107003C333839323434303030303036333839323435303030303036333839323436303030303033333839323438303030303031333839323731303030303032")
                                        SendPacket("5B0B7C3800000000000503020108003C333839323434303030303037333839323435303030303037333839323436303030303034333839323437303030303031333839323731303030303033")
                                    Next
                                End If
                        End Select
                    End If
                    MessagesLeft -= 1
                Loop
            End If

            Thread.Sleep(10)
        Loop
    End Sub
#End Region

#Region "Game Functions"
    Public Function GetID() As String
        Return AlignDWORD(ReadLong(ReadLong(KO_PTR_CHR) + KO_OFF_ID)).Substring(0, 4).ToUpper()
    End Function

    Public Function GetLongID() As Long
        Return ReadLong(ReadLong(KO_PTR_CHR) + KO_OFF_ID)
    End Function

    Public Function GetClass() As Long
        Return ReadLong(ReadLong(KO_PTR_CHR) + KO_OFF_CLASS)
    End Function

    Public Function GetNation() As Long
        Return ReadLong(ReadLong(KO_PTR_CHR) + KO_OFF_NT)
    End Function

    Public Function IsDisconnected() As String
        Return ReadLong(ReadLong(KO_PTR_PKT) + &H40064) = 0
    End Function

    Public Function IsMoving() As String
        Return ReadLong(ReadLong(KO_PTR_CHR) + KO_OFF_MOVE) = 1
    End Function

    Public Function GetName() As String
        Dim NameLen As Long = ReadLong(ReadLong(KO_PTR_CHR) + KO_OFF_NAME_LEN)
        If NameLen > 15 Then
            Return ReadString(ReadLong(ReadLong(KO_PTR_CHR) + KO_OFF_NAME), NameLen)
        Else
            Return ReadString(ReadLong(KO_PTR_CHR) + KO_OFF_NAME, NameLen)
        End If
    End Function

    Public Function GetX() As Long
        Return CInt(ReadFloat(ReadLong(KO_PTR_CHR) + KO_OFF_X))
    End Function

    Public Function GetY() As Long
        Return CInt(ReadFloat(ReadLong(KO_PTR_CHR) + KO_OFF_Y))
    End Function

    Public Function GetZ() As Long
        Return CInt(ReadFloat(ReadLong(KO_PTR_CHR) + KO_OFF_Z))
    End Function

    Public Function GetHp() As Long
        Return ReadLong(ReadLong(KO_PTR_CHR) + KO_OFF_HP)
    End Function

    Public Function GetMaxHp() As Long
        Return ReadLong(ReadLong(KO_PTR_CHR) + KO_OFF_MAX_HP)
    End Function

    Public Function GetMp() As Long
        Return ReadLong(ReadLong(KO_PTR_CHR) + KO_OFF_MP)
    End Function

    Public Function GetMaxMp() As Long
        Return ReadLong(ReadLong(KO_PTR_CHR) + KO_OFF_MAX_MP)
    End Function

    Public Function GetTargetID() As String
        Return AlignDWORD(ReadLong(ReadLong(KO_PTR_CHR) + KO_OFF_MOB)).Substring(0, 4).ToUpper()
    End Function

    Public Function GetState() As Byte
        Return ReadByte(ReadLong(KO_PTR_CHR) + KO_OFF_STATE)
    End Function

    Public Function GetTargetLongID() As Long
        Return ReadLong(ReadLong(KO_PTR_CHR) + KO_OFF_MOB)
    End Function

    Public Function GetTargetX() As Long
        Dim TargetID As String = GetTargetID()
        If TargetID = "FFFF" Then Return 0
        Dim TargetAddr As Long = GetTargetBase()
        If (TargetAddr = 0) Then Return 0
        Return CInt(ReadFloat(TargetAddr + KO_OFF_X))
    End Function

    Public Function GetTargetY() As Long
        Dim TargetID As String = GetTargetID()
        If TargetID = "FFFF" Then Return 0
        Dim TargetAddr As Long = GetTargetBase()
        If (TargetAddr = 0) Then Return 0
        Return CInt(ReadFloat(TargetAddr + KO_OFF_Y))
    End Function

    Public Function GetTargetZ() As Long
        Dim TargetID As String = GetTargetID()
        If TargetID = "FFFF" Then Return 0
        Dim TargetAddr As Long = GetTargetBase()
        If (TargetAddr = 0) Then Return 0
        Return CInt(ReadFloat(TargetAddr + KO_OFF_Z))
    End Function

    Public Function IsSelectNextTarget() As Boolean
        Dim TargetID As String = GetTargetID()
        If TargetID = "FFFF" Then Return True
        Dim TargetAddr As Long = GetTargetBase()
        If (TargetAddr = 0) Then Return True
        If _Configuration.TargetWaitDown.Checked = False And ReadLong(TargetAddr + KO_OFF_MAX_HP) > 0 And ReadLong(TargetAddr + KO_OFF_HP) = 0 Then Return True
        If _Configuration.TargetWaitDown.Checked = False And ReadByte(TargetAddr + KO_OFF_STATE) = 10 Then Return True
        'If CoordinateDistance(GetX(), GetY(), ReadFloat(TargetAddr + KO_OFF_X), ReadFloat(TargetAddr + KO_OFF_Y)) > 12 Then Return False
        Return False
    End Function

    Public Function IsAttackableTarget() As Boolean
        Dim TargetID As String = GetTargetID()
        If TargetID = "FFFF" Then Return False
        Dim TargetAddr As Long = GetTargetBase()
        If (TargetAddr = 0) Then Return False
        If ReadLong(TargetAddr + KO_OFF_MAX_HP) > 0 And ReadLong(TargetAddr + KO_OFF_HP) = 0 Then Return False
        If ReadByte(TargetAddr + KO_OFF_STATE) = 10 Then Return False
        If ReadLong(TargetAddr + KO_OFF_NT) = 0 Or ReadLong(TargetAddr + KO_OFF_NT) <> GetNation() Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function GetSkill(Slot As Long) As Long
        Return ReadLong((ReadLong(ReadLong(ReadLong(ReadLong(KO_PTR_DLG) + KO_OFF_SKILL_POINT_BASE) + &H184 + (Slot * 4)) + &H68)))
    End Function

    Public Function GetSkillPoint1() As Long
        Return ReadLong(ReadLong(ReadLong(KO_PTR_DLG) + KO_OFF_SKILL_POINT_BASE) + KO_OFF_SKILL_POINT1)
    End Function

    Public Function GetSkillPoint2() As Long
        Return ReadLong(ReadLong(ReadLong(KO_PTR_DLG) + KO_OFF_SKILL_POINT_BASE) + KO_OFF_SKILL_POINT2)
    End Function

    Public Function GetSkillPoint3() As Long
        Return ReadLong(ReadLong(ReadLong(KO_PTR_DLG) + KO_OFF_SKILL_POINT_BASE) + KO_OFF_SKILL_POINT3)
    End Function

    Public Function GetSkillPoint4() As Long
        Return ReadLong(ReadLong(ReadLong(KO_PTR_DLG) + KO_OFF_SKILL_POINT_BASE) + KO_OFF_SKILL_POINT4)
    End Function

    Public Function GetZone() As Long
        Return ReadLong(ReadLong(KO_PTR_CHR) + KO_OFF_ZONE)
    End Function

    Public Function GetMouseX() As Long
        Return CInt(ReadFloat(ReadLong(KO_PTR_CHR) + KO_OFF_MX))
    End Function

    Public Function GetMouseY() As Long
        Return CInt(ReadFloat(ReadLong(KO_PTR_CHR) + KO_OFF_MY))
    End Function

    Public Function GetExp() As Long
        Return ReadLong(ReadLong(KO_PTR_CHR) + KO_OFF_EXP)
    End Function

    Public Function GetMaxExp() As Long
        Return ReadLong(ReadLong(KO_PTR_CHR) + KO_OFF_MAX_EXP)
    End Function

    Public Function GetGold() As Long
        Return ReadLong(ReadLong(KO_PTR_CHR) + KO_OFF_GOLD)
    End Function

    Public Function GetItemID(ByVal ItemName As String) As Long
        Select Case ItemName
            Case "Holy Water"
                Return 389010000
            Case "Water Of Life"
                Return 389011000
            Case "Water Of Love"
                Return 389012000
            Case "Water Of Grace"
                Return 389013000
            Case "Water Of Favors"
                Return 389014000
            Case "Potion Of Spirit"
                Return 389016000
            Case "Potion Of Intelligence"
                Return 389017000
            Case "Potion Of Sagacity"
                Return 389018000
            Case "Potion Of Wisdom"
                Return 389019000
            Case "Potion Of Soul"
                Return 389020000
            Case "Arrow"
                Return 391010000
            Case "Wolf"
                Return 370004000
            Case "Transformation Gem"
                Return 379091000
            Case "Prayer Of God's Power"
                Return 389026000
            Case "Prayer Of Cronos"
                Return 389046000
            Case "Stone Of Mage"
                Return 379061000
            Case "Stone Of Priest"
                Return 379062000
            Case "Stone Of Warrior"
                Return 379059000
            Case "Stone Of Rogue"
                Return 379060000
            Case "Master Taş" ' Custom Item
                Select Case GetJob(GetClass())
                    Case "Warrior" : Return GetItemID("Stone Of Warrior")
                    Case "Rogue" : Return GetItemID("Stone Of Rogue")
                    Case "Priest" : Return GetItemID("Stone Of Priest")
                    Case "Mage" : Return GetItemID("Stone Of Mage")
                End Select
        End Select
        Return 0
    End Function

    Public Function BuyItem(ByVal NpcID As Long, ByVal ItemName As String, ByVal ItemCount As Long) As Boolean
        Dim Type As Long
        Dim ItemID As Long = 0
        Dim PacketEnd As String = ""
        Dim ItemCountString As String = ""

        Select Case ItemName
            Case "Holy Water"
                Type = 0 : ItemID = GetItemID(ItemName)
                ItemCountString = Strings.Mid(AlignDWORD(ItemCount), 1, 7) : PacketEnd = "0"
            Case "Water Of Life"
                Type = 0 : ItemID = GetItemID(ItemName)
                ItemCountString = Strings.Mid(AlignDWORD(ItemCount), 1, 7) : PacketEnd = "1"
            Case "Water Of Love"
                Type = 0 : ItemID = GetItemID(ItemName)
                ItemCountString = Strings.Mid(AlignDWORD(ItemCount), 1, 7) : PacketEnd = "2"
            Case "Water Of Grace"
                Type = 0 : ItemID = GetItemID(ItemName)
                ItemCountString = Strings.Mid(AlignDWORD(ItemCount), 1, 7) : PacketEnd = "3"
            Case "Water Of Favors"
                Type = 0 : ItemID = GetItemID(ItemName)
                ItemCountString = Strings.Mid(AlignDWORD(ItemCount), 1, 7) : PacketEnd = "4"
            Case "Potion Of Spirit"
                Type = 0 : ItemID = GetItemID(ItemName)
                ItemCountString = Strings.Mid(AlignDWORD(ItemCount), 1, 7) : PacketEnd = "6"
            Case "Potion Of Intelligence"
                Type = 0 : ItemID = GetItemID(ItemName)
                ItemCountString = Strings.Mid(AlignDWORD(ItemCount), 1, 7) : PacketEnd = "7"
            Case "Potion Of Sagacity"
                Type = 0 : ItemID = GetItemID(ItemName)
                ItemCountString = Strings.Mid(AlignDWORD(ItemCount), 1, 7) : PacketEnd = "8"
            Case "Potion Of Wisdom"
                Type = 0 : ItemID = GetItemID(ItemName)
                ItemCountString = Strings.Mid(AlignDWORD(ItemCount), 1, 7) : PacketEnd = "9"
            Case "Potion Of Soul"
                Type = 0 : ItemID = GetItemID(ItemName)
                ItemCountString = Strings.Mid(AlignDWORD(ItemCount), 1, 7) : PacketEnd = "A"
            Case "Arrow"
                Type = 1 : ItemID = GetItemID(ItemName)
                ItemCountString = AlignDWORD(ItemCount) : PacketEnd = ""
            Case "Wolf"
                Type = 1 : ItemID = GetItemID(ItemName)
                ItemCountString = Strings.Mid(AlignDWORD(ItemCount), 1, 7) : PacketEnd = "7"
            Case "Transformation Gem"
                Type = 1 : ItemID = GetItemID(ItemName)
                ItemCountString = Strings.Mid(AlignDWORD(ItemCount), 1, 4) : PacketEnd = "0103"
            Case "Prayer Of God's Power"
                Type = 1 : ItemID = GetItemID(ItemName)
                ItemCountString = Strings.Mid(AlignDWORD(ItemCount), 1, 7) : PacketEnd = "B"
            Case "Stone Of Mage"
                Type = 1 : ItemID = GetItemID(ItemName)
                ItemCountString = Strings.Mid(AlignDWORD(ItemCount), 1, 6) : PacketEnd = "16"
            Case "Stone Of Priest"
                Type = 1 : ItemID = GetItemID(ItemName)
                ItemCountString = Strings.Mid(AlignDWORD(ItemCount), 1, 6) : PacketEnd = "17"
            Case "Stone Of Warrior"
                Type = 1 : ItemID = GetItemID(ItemName)
                ItemCountString = Strings.Mid(AlignDWORD(ItemCount), 1, 6) : PacketEnd = "14"
            Case "Stone Of Rogue"
                Type = 1 : ItemID = GetItemID(ItemName)
                ItemCountString = Strings.Mid(AlignDWORD(ItemCount), 1, 6) : PacketEnd = "15"
            Case "Master Taş" ' Custom Item
                Select Case GetJob(GetClass())
                    Case "Warrior" : Return BuyItem(NpcID, "Stone Of Warrior", ItemCount)
                    Case "Rogue" : Return BuyItem(NpcID, "Stone Of Rogue", ItemCount)
                    Case "Priest" : Return BuyItem(NpcID, "Stone Of Priest", ItemCount)
                    Case "Mage" : Return BuyItem(NpcID, "Stone Of Mage", ItemCount)
                End Select
        End Select

        If ItemID = 0 Then Return False

        Dim Slot As Long = SearchInventory(ItemID)

        If Slot = -1 Then
            Slot = GetEmptyInventorySlot()
        End If

        If Slot = -1 Then Return False

        If Type = 1 Then
            SendPacket("2101" & "18E40300" & AlignDWORD(NpcID).Substring(0, 4).ToUpper() & "01" & AlignDWORD(ItemID) & Strings.Mid(AlignDWORD(Slot - 14), 1, 2) & ItemCountString & PacketEnd) ' Sundries
        Else
            SendPacket("2101" & "48DC0300" & AlignDWORD(NpcID).Substring(0, 4).ToUpper() & "01" & AlignDWORD(ItemID) & Strings.Mid(AlignDWORD(Slot - 14), 1, 2) & ItemCountString & PacketEnd) ' Potion
        End If
        Thread.Sleep(500)
        SendPacket("6A02")
        Return False
    End Function

    Public Function GetInventoryItemID(ByVal Slot As Integer) As Long
        Dim TempLength1 As Long = ReadLong(ReadLong(KO_PTR_DLG) + KO_OFF_INVENTORY_BASE)
        Dim TempLength2 As Long = ReadLong(TempLength1 + (KO_OFF_INVENTORY_SLOT + (4 * Slot)))
        Dim ItemId As Long = ReadLong(TempLength2 + &H68)
        Dim ItemExt As Long = ReadLong(TempLength2 + &H6C)
        ItemId = ReadLong(ItemId)
        ItemExt = ReadLong(ItemExt)
        Return ItemId + ItemExt
    End Function

    Public Function GetInventoryItemCount(ByVal Slot As Integer) As Long
        Dim TempLength1 As Long = ReadLong(ReadLong(KO_PTR_DLG) + KO_OFF_INVENTORY_BASE)
        Dim TempLength2 As Long = ReadLong(TempLength1 + (KO_OFF_INVENTORY_SLOT + (4 * Slot)))
        Dim ItemCount As Long = ReadLong(TempLength2 + &H70)
        Return ItemCount
    End Function

    Public Function IsItemExist(ByVal ItemName As String) As Boolean
        Return SearchInventory(GetItemID(ItemName)) <> -1
    End Function

    Public Function GetInventoryItemName(ByVal Slot As Integer) As String
        Dim a As Long, b As Long, c As Long, len As Long
        a = ReadLong(ReadLong(KO_PTR_DLG) + KO_OFF_INVENTORY_BASE)
        b = ReadLong(a + KO_OFF_INVENTORY_SLOT + (4 * Slot))
        c = ReadLong(b + &H68)
        len = ReadLong(c + &H1C)
        If len > 15 Then
            Return ReadString(ReadLong(c + &HC), len)
        Else
            Return ReadString(c + &HC, len)
        End If
    End Function

    Function GetEmptyInventorySlot() As Integer
        For i = 14 To 41
            If GetInventoryItemID(i) = 0 Then
                Return i
                Exit For
                Exit Function
            End If
            If i = 41 Then Return -1 : Exit Function
        Next
        Return -1
    End Function

    Public Function SearchInventory(ByVal ItemID As Long) As Long
        For Slot = 14 To 41
            Dim TempLength1 As Long = ReadLong(ReadLong(KO_PTR_DLG) + KO_OFF_INVENTORY_BASE)
            Dim TempLength2 As Long = ReadLong(TempLength1 + (KO_OFF_INVENTORY_SLOT + (4 * Slot)))
            If (ReadLong(ReadLong(TempLength2 + &H68)) + ReadLong(ReadLong(TempLength2 + &H6C))) = ItemID Then
                Return Slot
            End If
        Next
        Return -1
    End Function

    Public Function GetWareHouseItemID(ByVal Slot As Integer) As Long
        Dim TempLength1 As Long = ReadLong(ReadLong(KO_PTR_DLG) + KO_OFF_WAREHOUSE_BASE)
        Dim TempLength2 As Long = ReadLong(TempLength1 + (KO_OFF_WAREHOUSE_SLOT + (4 * Slot)))
        Dim ItemId As Long = ReadLong(TempLength2 + &H68)
        Dim ItemExt As Long = ReadLong(TempLength2 + &H6C)
        ItemId = ReadLong(ItemId)
        ItemExt = ReadLong(ItemExt)
        Return ItemId + ItemExt
    End Function

    Public Function GetWareHouseItemCount(ByVal Slot As Integer) As Long
        Dim TempLength1 As Long = ReadLong(ReadLong(KO_PTR_DLG) + KO_OFF_WAREHOUSE_BASE)
        Dim TempLength2 As Long = ReadLong(TempLength1 + (KO_OFF_WAREHOUSE_SLOT + (4 * Slot)))
        Dim ItemCount As Long = ReadLong(TempLength2 + &H70)
        Return ItemCount
    End Function

    Public Function GetWarehouseItemName(ByVal Slot As Integer) As String
        Dim a As Long, b As Long, c As Long, len As Long
        a = ReadLong(ReadLong(KO_PTR_DLG) + KO_OFF_WAREHOUSE_BASE)
        b = ReadLong(a + KO_OFF_WAREHOUSE_SLOT + (4 * Slot))
        c = ReadLong(b + &H68)
        len = ReadLong(c + &H1C)
        If len > 15 Then
            Return ReadString(ReadLong(c + &HC), len)
        Else
            Return ReadString(c + &HC, len)
        End If
    End Function

    Public Sub ReadWarehouse()
        SendPacket("2001" & AlignDWORD("12926").Substring(0, 4).ToUpper() & "FFFFFFFF")
        Thread.Sleep(25)
        SendPacket("4501" & AlignDWORD("12926").Substring(0, 4).ToUpper())
    End Sub

    Public Function WareHouseCheckOutItem(ByVal NpcID As Long, ByVal ItemName As String, ByVal ItemCount As Long) As Boolean
        Dim ItemID As Long = 0
        Dim ItemType As String = "0"

        Select Case ItemName
            Case "Holy Water"
                ItemType = 0 : ItemID = GetItemID(ItemName)
            Case "Water Of Life"
                ItemType = 0 : ItemID = GetItemID(ItemName)
            Case "Water Of Love"
                ItemType = 0 : ItemID = GetItemID(ItemName)
            Case "Water Of Grace"
                ItemType = 0 : ItemID = GetItemID(ItemName)
            Case "Water Of Favors"
                ItemType = 0 : ItemID = GetItemID(ItemName)
            Case "Potion Of Spirit"
                ItemType = 1 : ItemID = GetItemID(ItemName)
            Case "Potion Of Intelligence"
                ItemType = 1 : ItemID = GetItemID(ItemName)
            Case "Potion Of Sagacity"
                ItemType = 1 : ItemID = GetItemID(ItemName)
            Case "Potion Of Wisdom"
                ItemType = 1 : ItemID = GetItemID(ItemName)
            Case "Potion Of Soul"
                ItemType = 1 : ItemID = GetItemID(ItemName)
        End Select

        If ItemID = 0 Then Return False

        ReadWarehouse()

        Thread.Sleep(2000)

        For n = 0 To 191
            If GetWareHouseItemID(n) = ItemID Then
                If GetWareHouseItemCount(n) < ItemCount Then
                    Return False
                End If

                Dim Slot As Long = 0
                Dim Sayfa As Long = 0

                Select Case n
                    Case 0 To 23
                        Sayfa = 0
                        Slot = n
                    Case 24 To 47
                        Sayfa = 1
                        Slot = n - 24
                    Case 48 To 71
                        Sayfa = 2
                        Slot = n - 48
                    Case 72 To 95
                        Sayfa = 3
                        Slot = n - 72
                    Case 96 To 119
                        Sayfa = 4
                        Slot = n - 96
                    Case 120 To 143
                        Sayfa = 5
                        Slot = n - 120
                    Case 144 To 167
                        Sayfa = 6
                        Slot = n - 144
                    Case 168 To 191
                        Sayfa = 7
                        Slot = n - 168
                End Select

                SendPacket("4503" + AlignDWORD(NpcID).Substring(0, 4) + AlignDWORD(ItemID) + Strings.Mid(AlignDWORD(Sayfa), 1, 2) + Strings.Mid(AlignDWORD(Slot), 1, 2) + Strings.Mid(AlignDWORD(ItemType), 1, 2) + Strings.Mid(AlignDWORD(ItemCount), 1, 4) + "0000")
            End If
        Next

        Thread.Sleep(2000)
        SendPacket("6A02")

        Return True
    End Function

    Public Function GetItemDurability(ByVal Slot As Integer) As Long
        Dim TempLength1 As Long = ReadLong(ReadLong(KO_PTR_DLG) + KO_OFF_INVENTORY_BASE)
        Dim TempLength2 As Long = ReadLong(TempLength1 + (KO_OFF_INVENTORY_SLOT + (4 * Slot)))
        Dim Durability As Long = ReadLong(TempLength2 + &H74)
        Return Durability
    End Function

    Public Sub SetItemDurability(ByVal Slot As Integer, ByVal Durability As Integer)
        Dim TempLength1 As Long = ReadLong(ReadLong(KO_PTR_DLG) + KO_OFF_INVENTORY_BASE)
        Dim TempLength2 As Long = ReadLong(TempLength1 + (KO_OFF_INVENTORY_SLOT + (4 * Slot)))
        WriteLong(TempLength2 + &H74, Durability)
    End Sub

    Public Sub RepairAllEquipment(ByVal NpcID As Long, Optional ByVal Force As Boolean = False)
        If GetItemDurability(1) = 0 Or Force Then
            SendPacket("3B01" & "01" & AlignDWORD(NpcID).Substring(0, 4).ToUpper() & AlignDWORD(GetInventoryItemID(1)))
            Thread.Sleep(300)
        End If

        If GetItemDurability(4) = 0 Or Force Then
            SendPacket("3B01" & "04" & AlignDWORD(NpcID).Substring(0, 4).ToUpper() & AlignDWORD(GetInventoryItemID(4)))
            Thread.Sleep(300)
        End If

        If GetItemDurability(6) = 0 Or Force Then
            SendPacket("3B01" & "06" & AlignDWORD(NpcID).Substring(0, 4).ToUpper() & AlignDWORD(GetInventoryItemID(6)))
            Thread.Sleep(300)
        End If

        If GetItemDurability(8) = 0 Or Force Then
            SendPacket("3B01" & "08" & AlignDWORD(NpcID).Substring(0, 4).ToUpper() & AlignDWORD(GetInventoryItemID(8)))
            Thread.Sleep(300)
        End If

        If GetItemDurability(10) = 0 Or Force Then
            SendPacket("3B01" & "0A" & AlignDWORD(NpcID).Substring(0, 4).ToUpper() & AlignDWORD(GetInventoryItemID(10)))
            Thread.Sleep(300)
        End If

        If GetItemDurability(12) = 0 Or Force Then
            SendPacket("3B01" & "0C" & AlignDWORD(NpcID).Substring(0, 4).ToUpper() & AlignDWORD(GetInventoryItemID(12)))
            Thread.Sleep(300)
        End If

        If GetItemDurability(13) = 0 Or Force Then
            SendPacket("3B01" & "0D" & AlignDWORD(NpcID).Substring(0, 4).ToUpper() & AlignDWORD(GetInventoryItemID(13)))
            Thread.Sleep(300)
        End If
    End Sub

    Public Function GetUndyHp(ByVal Hp As Long) As Long
        Return CInt(Math.Round(Hp * 60 / 100))
    End Function

    Public Function GetHeal() As String
        If GetControl("PartyHealSkill") <> "Otomatik" Then
            Return GetControl("PartyHealSkill")
        End If

        If GetSkillPoint1() >= 45 Then
            Return "Superior Healing"
        ElseIf GetSkillPoint1() >= 36 Then ' 1200 Açık ise
            Return "Massive Healing"
        ElseIf GetSkillPoint1() >= 27 Then ' 960 Açık ise
            Return "Great Healing"
        ElseIf GetSkillPoint1() >= 18 Then ' 720 Açık ise
            Return "Major Healing"
        ElseIf GetSkillPoint1() >= 9 Then ' 360 Açık ise
            Return "Healing"
        ElseIf GetSkillPoint1() >= 0 Then ' 240 Açık ise
            Return "Minor Healing"
        End If
        Return ""
    End Function

    Public Function IsAffectedHealthBuff() As Boolean
        If IsRightSkillAffected(606) = True Or IsRightSkillAffected(615) = True Or IsRightSkillAffected(624) = True Or
           IsRightSkillAffected(633) = True Or IsRightSkillAffected(642) = True Or IsRightSkillAffected(654) = True Or
           IsRightSkillAffected(655) = True Or IsRightSkillAffected(656) = True Or IsRightSkillAffected(657) = True Or
           IsRightSkillAffected(670) = True Or IsRightSkillAffected(672) = True Or IsRightSkillAffected(675) = True Then
            Return True
        End If
        Return False
    End Function

    Public Function IsNeedRepair() As Boolean
        If (GetInventoryItemID(1) > 0 And GetItemDurability(1) < 0) Or (GetInventoryItemID(4) > 0 And GetItemDurability(4) = 0) Or (GetInventoryItemID(6) > 0 And GetItemDurability(6) = 0) Or
           (GetInventoryItemID(8) > 0 And GetItemDurability(8) = 0) Or (GetInventoryItemID(10) > 0 And GetItemDurability(10) = 0) Or (GetInventoryItemID(12) > 0 And GetItemDurability(12) = 0) Or
           (GetInventoryItemID(13) > 0 And GetItemDurability(13) = 0) Then
            Return True
        End If
        Return False
    End Function

    Public Function IsTransformationAvailableZone() As Boolean
        If (GetZone() = 71 Or GetZone() = 81 Or GetZone() = 82 Or GetZone() = 83 Or GetZone() = 75) Then
            Return False
        End If
        Return True
    End Function

    Public Function GetHealthBuff(ByVal Hp As Long) As String
        If GetControl("PartyBuffSkill") <> "Otomatik" Then
            Return GetControl("PartyBuffSkill")
        End If

        If GetSkill(117) > 0 And GetSkillPoint2() >= 78 Then
            If GetUndyHp(Hp) >= 2500 Then
                Return "Undying"
            Else
                Return "Superioris"
            End If
        ElseIf GetSkill(112) > 0 And GetSkillPoint2() >= 70 Then
            If GetUndyHp(Hp) >= 2000 Then
                Return "Undying"
            Else
                Return "Imposingness"
            End If
        ElseIf GetSkillPoint2() >= 57 Then
            If GetUndyHp(Hp) >= 1500 Then
                Return "Undying"
            Else
                Return "Massiveness"
            End If
        ElseIf GetSkillPoint2() >= 54 Then
            If GetUndyHp(Hp) >= 1200 Then
                Return "Undying"
            Else
                Return "Heapness"
            End If
        ElseIf GetSkillPoint2() >= 42 Then
            Return "Mightness"
        ElseIf GetSkillPoint2() >= 33 Then
            Return "Hardness"
        ElseIf GetSkillPoint2() >= 24 Then
            Return "Strong"
        ElseIf GetSkillPoint2() >= 15 Then
            Return "Brave"
        ElseIf GetSkillPoint2() >= 6 Then
            Return "Grace"
        End If
        Return ""
    End Function

    Public Function IsAffectedDefenceBuff() As Boolean
        If IsRightSkillAffected(603) = True Or IsRightSkillAffected(612) = True Or IsRightSkillAffected(621) = True Or
           IsRightSkillAffected(630) = True Or IsRightSkillAffected(639) = True Or IsRightSkillAffected(651) = True Or
           IsRightSkillAffected(660) = True Or IsRightSkillAffected(673) = True Or IsRightSkillAffected(674) = True Or
           IsRightSkillAffected(670) Then
            Return True
        End If
        Return False
    End Function

    Public Function GetDefenceBuff() As String
        If GetControl("PartyAcSkill") <> "Otomatik" Then
            Return GetControl("PartyAcSkill")
        End If

        If GetSkill(116) > 0 And GetSkillPoint2() >= 76 Then
            Return "Insensibility Guard"
        ElseIf GetSkillPoint2() >= 60 Then
            Return "Insensibility Peel"
        ElseIf GetSkillPoint2() >= 51 Then
            Return "Insensibility Protector"
        ElseIf GetSkillPoint2() >= 39 Then
            Return "Insensibility Barrier"
        ElseIf GetSkillPoint2() >= 30 Then
            Return "Insensibility Shield"
        ElseIf GetSkillPoint2() >= 21 Then
            Return "Insensibility Armor"
        ElseIf GetSkillPoint2() >= 12 Then
            Return "Insensibility Shell"
        ElseIf GetSkillPoint2() >= 3 Then
            Return "Insensibility Skin"
        End If
        Return ""
    End Function

    Public Function GetMindBuff() As String

        If GetControl("PartyMindSkill") <> "Otomatik" Then
            Return GetControl("PartyMindSkill")
        End If

        If GetSkillPoint2() >= 45 And GetSkillPoint2() <= 80 Then
            Return "Fresh Mind"
        ElseIf GetSkillPoint2() >= 36 And GetSkillPoint2() <= 44 Then
            Return "Calm Mind"
        ElseIf GetSkillPoint2() >= 27 And GetSkillPoint2() <= 35 Then
            Return "Bright Mind"
        ElseIf GetSkillPoint2() >= 9 And GetSkillPoint2() <= 26 Then
            Return "Resist All"
        End If
        Return ""
    End Function


    Public Function IsAffectedMindBuff() As Boolean
        If IsRightSkillAffected(627) = True Or IsRightSkillAffected(636) = True Or IsRightSkillAffected(645) = True Then
            Return True
        End If
        Return False
    End Function

    Public Function GetJob(ByVal ClassId As Long) As String
        Select Case ClassId
            Case 102 : Return "Rogue"
            Case 107 : Return "Rogue"
            Case 108 : Return "Rogue"
            Case 202 : Return "Rogue"
            Case 207 : Return "Rogue"
            Case 208 : Return "Rogue"
            Case 104 : Return "Priest"
            Case 111 : Return "Priest"
            Case 112 : Return "Priest"
            Case 204 : Return "Priest"
            Case 211 : Return "Priest"
            Case 212 : Return "Priest"
            Case 101 : Return "Warrior"
            Case 105 : Return "Warrior"
            Case 106 : Return "Warrior"
            Case 201 : Return "Warrior"
            Case 205 : Return "Warrior"
            Case 206 : Return "Warrior"
            Case 103 : Return "Mage"
            Case 109 : Return "Mage"
            Case 110 : Return "Mage"
            Case 203 : Return "Mage"
            Case 209 : Return "Mage"
            Case 210 : Return "Mage"
        End Select
        Return "Undefined"
    End Function

    ' 8.11.2021 Güncel KO Sürümlerinde çalışıyor Eski Pvp Server ve Jpko'da çalıştırmadım 
    Public Function GetMobBase(ByVal TargetId As String) As Long
        Dim Addr As Long = VirtualAllocEx(_Handle, 0, 1, MEM_COMMIT Or MEM_RESERVE, &H40)
        ExecuteRemoteCode("608B0D" + AlignDWORD(KO_PTR_FLDB) + "6A0168" + AlignDWORD(TargetId) + "BF" + AlignDWORD(KO_PTR_FMBS) + "FFD7A3" + AlignDWORD(Addr) + "61C3")
        Dim TargetBaseAddr As Long = ReadLong(Addr)
        VirtualFreeEx(_Handle, Addr, 0, MEM_RELEASE)
        Return TargetBaseAddr
    End Function

    ' 8.11.2021 Güncel KO Sürümlerinde çalışıyor Eski Pvp Server ve Jpko'da çalıştırmadım 
    Public Function GetPlayerBase(ByVal TargetId As String) As Long
        Dim Addr As Long = VirtualAllocEx(_Handle, 0, 1, MEM_COMMIT Or MEM_RESERVE, &H40)
        ExecuteRemoteCode("608B0D" + AlignDWORD(KO_PTR_FLDB) + "6A0168" + AlignDWORD(TargetId) + "BF" + AlignDWORD(KO_PTR_FPBS) + "FFD7A3" + AlignDWORD(Addr) + "61C3")
        Dim TargetBaseAddr As Long = ReadLong(Addr)
        VirtualFreeEx(_Handle, Addr, 0, MEM_RELEASE)
        Return TargetBaseAddr
    End Function

    Public Function FindMobBase(ByVal TargetId As Long) As Long
        Dim EBP As Long, ESI As Long, EAX As Long, FEnd As Long, Tick As Long
        Dim BaseAddr As Long = 0
        EBP = ReadLong(ReadLong(KO_PTR_FLDB) + &H34)
        FEnd = ReadLong(ReadLong(EBP + 4) + 4)
        ESI = ReadLong(EBP)
        Tick = GetTickCount
        Do While ESI <> EBP And GetTickCount - Tick < 150
            BaseAddr = ReadLong(ESI + &H10)
            If BaseAddr = 0 Then Return 0
            EAX = ReadLong(ESI + 8)
            If ReadLong(ESI + 8) <> FEnd Then
                Do While ReadLong(EAX) <> FEnd And GetTickCount - Tick < 150
                    EAX = ReadLong(EAX)
                Loop
                ESI = EAX
            Else
                EAX = ReadLong(ESI + 4)
                Do While ESI = ReadLong(EAX + 8) And GetTickCount - Tick < 150
                    ESI = EAX
                    EAX = ReadLong(EAX + 4)
                Loop
                If ReadLong(ESI + 8) <> EAX Then
                    ESI = EAX
                End If
            End If
            If ReadLong(BaseAddr + KO_OFF_ID) = TargetId Then
                Return BaseAddr
            End If
        Loop
        Return BaseAddr
    End Function

    Public Function FindPlayerBase(ByVal TargetId As Long) As Long
        Dim EBP As Long, ESI As Long, EAX As Long, FEnd As Long, Tick As Long
        Dim BaseAddr As Long = 0
        EBP = ReadLong(ReadLong(KO_PTR_FLDB) + &H40)
        FEnd = ReadLong(ReadLong(EBP + 4) + 4)
        ESI = ReadLong(EBP)
        Tick = GetTickCount
        Do While ESI <> EBP And GetTickCount - Tick < 150
            BaseAddr = ReadLong(ESI + &H10)
            If BaseAddr = 0 Then Return 0
            EAX = ReadLong(ESI + 8)
            If ReadLong(ESI + 8) <> FEnd Then
                Do While ReadLong(EAX) <> FEnd And GetTickCount - Tick < 150
                    EAX = ReadLong(EAX)
                Loop
                ESI = EAX
            Else
                EAX = ReadLong(ESI + 4)
                Do While ESI = ReadLong(EAX + 8) And GetTickCount - Tick < 150
                    ESI = EAX
                    EAX = ReadLong(EAX + 4)
                Loop
                If ReadLong(ESI + 8) <> EAX Then
                    ESI = EAX
                End If
            End If

            If ReadLong(BaseAddr + KO_OFF_ID) = TargetId Then
                Return BaseAddr
            End If
        Loop
        Return BaseAddr
    End Function

    Public Function GetTargetBase() As Long
        If GetTargetLongID() > 9999 Then
            Return FindMobBase(GetTargetLongID())
        Else
            Return FindPlayerBase(GetTargetLongID())
        End If
    End Function

    Public Function FindMob() As Long
        Dim EBP As Long, ESI As Long, EAX As Long, FEnd As Long
        Dim LDist As Long, CrrDist As Long, LID As Long, LBase As Long, Tick As Long
        Dim base_addr As Long
        LDist = _Configuration.TargetRange.Value
        EBP = ReadLong(ReadLong(KO_PTR_FLDB) + &H34)
        FEnd = ReadLong(ReadLong(EBP + 4) + 4)
        ESI = ReadLong(EBP)
        Tick = GetTickCount
        Do While ESI <> EBP And GetTickCount - Tick < 150
            base_addr = ReadLong(ESI + &H10)
            If base_addr = 0 Then Return 0
            Dim name As String
            Dim namelen As Long = ReadLong(base_addr + KO_OFF_NAME_LEN)
            If namelen > 15 Then
                name = ReadString(ReadLong(base_addr + KO_OFF_NAME), namelen)
            Else
                name = ReadString(base_addr + KO_OFF_NAME, namelen)
            End If

            If ReadLong(base_addr + KO_OFF_NT) = 0 And ReadByte(base_addr + KO_OFF_STATE) <> 10 And (ReadLong(base_addr + KO_OFF_NT) = 0 Or ReadLong(base_addr + KO_OFF_NT) <> GetNation()) Then
                CrrDist = CoordinateDistance(GetX(), GetY(), ReadFloat(base_addr + KO_OFF_X), ReadFloat(base_addr + KO_OFF_Y))
                If GetMobListCheckEnabled() Then
                    If _MobList.ContainsKey(name) Then
                        If _MobList(name) = True And CrrDist <= LDist Then
                            LID = ReadLong(base_addr + KO_OFF_ID)
                            LBase = (base_addr)
                            LDist = CrrDist
                        End If
                    End If
                Else
                    If CrrDist <= LDist Then
                        LID = ReadLong(base_addr + KO_OFF_ID)
                        LBase = (base_addr)
                        LDist = CrrDist
                    End If
                End If
            End If
            EAX = ReadLong(ESI + 8)
            If ReadLong(ESI + 8) <> FEnd Then
                Do While ReadLong(EAX) <> FEnd And GetTickCount - Tick < 150
                    EAX = ReadLong(EAX)
                Loop
                ESI = EAX
            Else
                EAX = ReadLong(ESI + 4)
                Do While ESI = ReadLong(EAX + 8) And GetTickCount - Tick < 150
                    ESI = EAX
                    EAX = ReadLong(EAX + 4)
                Loop
                If ReadLong(ESI + 8) <> EAX Then
                    ESI = EAX
                End If
            End If
        Loop
        If LBase = 0 Then Return 0
        Return LID
    End Function

    Public Function UpdateMobList() As Dictionary(Of String, Boolean)
        Dim EBP As Long, ESI As Long, EAX As Long, FEnd As Long, Tick As Long
        Dim BaseAddr As Long = 0
        EBP = ReadLong(ReadLong(KO_PTR_FLDB) + &H34)
        FEnd = ReadLong(ReadLong(EBP + 4) + 4)
        ESI = ReadLong(EBP)
        Tick = GetTickCount
        Do While ESI <> EBP And GetTickCount - Tick < 150
            BaseAddr = ReadLong(ESI + &H10)
            If BaseAddr = 0 Then Return Nothing
            EAX = ReadLong(ESI + 8)
            If ReadLong(ESI + 8) <> FEnd Then
                Do While ReadLong(EAX) <> FEnd And GetTickCount - Tick < 150
                    EAX = ReadLong(EAX)
                Loop
                ESI = EAX
            Else
                EAX = ReadLong(ESI + 4)
                Do While ESI = ReadLong(EAX + 8) And GetTickCount - Tick < 150
                    ESI = EAX
                    EAX = ReadLong(EAX + 4)
                Loop
                If ReadLong(ESI + 8) <> EAX Then
                    ESI = EAX
                End If
            End If
            Dim Nation As Long = ReadLong(BaseAddr + KO_OFF_NT)
            If Nation <> 0 Then Continue Do

            Dim Name As String = ""

            If ReadLong(BaseAddr + KO_OFF_NAME_LEN) > 15 Then
                Name = ReadString(ReadLong(BaseAddr + KO_OFF_NAME), ReadLong(BaseAddr + KO_OFF_NAME_LEN))
            Else
                Name = ReadString(BaseAddr + KO_OFF_NAME, ReadLong(BaseAddr + KO_OFF_NAME_LEN))
            End If

            If Name <> "" And Not _MobList.ContainsKey(Name) Then
                _MobList.Add(Name, False)
            End If
        Loop

        Return _MobList
    End Function

    Public Function UpdatePlayerList() As Dictionary(Of String, Boolean)
        Dim EBP As Long, ESI As Long, EAX As Long, FEnd As Long, Tick As Long
        Dim BaseAddr As Long = 0
        EBP = ReadLong(ReadLong(KO_PTR_FLDB) + &H40)
        FEnd = ReadLong(ReadLong(EBP + 4) + 4)
        ESI = ReadLong(EBP)
        Tick = GetTickCount
        Do While ESI <> EBP And GetTickCount - Tick < 150
            BaseAddr = ReadLong(ESI + &H10)
            If BaseAddr = 0 Then Return Nothing
            EAX = ReadLong(ESI + 8)
            If ReadLong(ESI + 8) <> FEnd Then
                Do While ReadLong(EAX) <> FEnd And GetTickCount - Tick < 150
                    EAX = ReadLong(EAX)
                Loop
                ESI = EAX
            Else
                EAX = ReadLong(ESI + 4)
                Do While ESI = ReadLong(EAX + 8) And GetTickCount - Tick < 150
                    ESI = EAX
                    EAX = ReadLong(EAX + 4)
                Loop
                If ReadLong(ESI + 8) <> EAX Then
                    ESI = EAX
                End If
            End If
            'Dim Nation As Long = ReadLong(BaseAddr + KO_OFF_NT)
            'If Nation <> 0 Then Continue Do

            Dim Name As String = ""

            If ReadLong(BaseAddr + KO_OFF_NAME_LEN) > 15 Then
                Name = ReadString(ReadLong(BaseAddr + KO_OFF_NAME), ReadLong(BaseAddr + KO_OFF_NAME_LEN))
            Else
                Name = ReadString(BaseAddr + KO_OFF_NAME, ReadLong(BaseAddr + KO_OFF_NAME_LEN))
            End If

            If Name <> "" And Not _MobList.ContainsKey(Name) Then
                _MobList.Add(Name, False)
            End If
        Loop

        Return _MobList
    End Function

    Public Function GetPartyCount() As Long
        Return ReadLong(ReadLong(ReadLong(KO_PTR_DLG) + KO_OFF_PARTY_BASE) + KO_OFF_PARTY_COUNT)
    End Function

    Public Sub SelectTarget(ByVal EnemyId As Long)
        WriteLong(ReadLong(KO_PTR_CHR) + KO_OFF_MOB, EnemyId)

        If EnemyId = 0 Then
            WriteByte(ReadLong(ReadLong(KO_PTR_DLG) + &H1D0) + &HFC, 0)
        Else
            WriteByte(ReadLong(ReadLong(KO_PTR_DLG) + &H1D0) + &HFC, 1)
        End If
    End Sub

    Public Sub Move(ByVal x As Single, ByVal y As Single)
        WriteLong(ReadLong(KO_PTR_CHR) + KO_OFF_MOVE, 1)
        WriteFloat(ReadLong(KO_PTR_CHR) + KO_OFF_GO_X, x)
        WriteFloat(ReadLong(KO_PTR_CHR) + KO_OFF_GO_Y, y)
        WriteLong(ReadLong(KO_PTR_CHR) + KO_OFF_MOVE_TYPE, 2)
    End Sub

    Public Function SetCoordinate(crx As Single, cry As Single)
        On Error Resume Next
        If crx <= 0 Or cry <= 0 Then Exit Function
        _IsJumping = True
        Dim zipla, X, Y, uzak, a, b, d, e, i, isrtx, isrty
        Dim tx As Single, ty As Single
        Dim x1 As Single, y1 As Single
        Dim bykx, byky, kckx, kcky
        zipla = 3.0#
        tx = CInt(ReadFloat(ReadLong(KO_PTR_CHR) + KO_OFF_X))
        ty = CInt(ReadFloat(ReadLong(KO_PTR_CHR) + KO_OFF_Y))
        X = Math.Abs(crx - tx)
        Y = Math.Abs(cry - ty)
        If tx > crx Then isrtx = -1 : bykx = tx : kckx = crx Else isrtx = 1 : bykx = crx : kckx = tx
        If ty > cry Then isrty = -1 : byky = ty : kcky = cry Else isrty = 1 : byky = cry : kcky = ty
        uzak = Int(Math.Sqrt((X ^ 2 + Y ^ 2)))

        For i = zipla To uzak Step zipla
            a = i ^ 2 * X ^ 2
            b = X ^ 2 + Y ^ 2
            d = Math.Sqrt(a / b)
            e = Math.Sqrt(i ^ 2 - d ^ 2)
            x1 = Int(tx + isrtx * d)
            y1 = Int(ty + isrty * e)
            If (kckx > x1 And x1 > bykx) Or (kcky > y1 And y1 > byky) Then Continue For

            WriteFloat(ReadLong(KO_PTR_CHR) + KO_OFF_X, x1)
            WriteFloat(ReadLong(KO_PTR_CHR) + KO_OFF_Y, y1)

            SendPacket("06" _
                    & Left(AlignDWORD(ReadFloat(ReadLong(KO_PTR_CHR) + KO_OFF_X) * 10), 4) _
                    & Left(AlignDWORD(ReadFloat(ReadLong(KO_PTR_CHR) + KO_OFF_Y) * 10), 4) _
                    & Left(AlignDWORD(ReadFloat(ReadLong(KO_PTR_CHR) + KO_OFF_Z)), 4) _
                    & "2D0000" _
                    & AlignDWORD(GetX() * 10).Substring(0, 4) & AlignDWORD(GetY() * 10).Substring(0, 4) & AlignDWORD(GetZ()).Substring(0, 4))

            Thread.Sleep(20)
        Next

        Dim Distance = CoordinateDistance(GetX(), GetY(), crx, cry)

        If Distance >= 0 And Distance <= 3 Then
            WriteFloat(ReadLong(KO_PTR_CHR) + KO_OFF_X, crx)
            WriteFloat(ReadLong(KO_PTR_CHR) + KO_OFF_Y, cry)

            SendPacket("06" _
                    & Left(AlignDWORD(ReadFloat(ReadLong(KO_PTR_CHR) + KO_OFF_X) * 10), 4) _
                    & Left(AlignDWORD(ReadFloat(ReadLong(KO_PTR_CHR) + KO_OFF_Y) * 10), 4) _
                    & Left(AlignDWORD(ReadFloat(ReadLong(KO_PTR_CHR) + KO_OFF_Z) * 10), 4) _
                    & "2D0000" _
                    & AlignDWORD(GetX() * 10).Substring(0, 4) & AlignDWORD(GetY() * 10).Substring(0, 4) & AlignDWORD(GetZ() * 10).Substring(0, 4))

        Else
            'Return SetCoordinate(crx, cry, crz)
        End If

        Move(crx, cry)

        If (_IsSupplying Or _IsRepairing) Then
            If (crx = _SupplyBackX And cry = _SupplyBackY) Then
                _IsSupplying = False
            End If

            If (crx = _RepairBackX And cry = _RepairBackY) Then
                _IsRepairing = False
            End If
        End If

        _IsJumping = False
    End Function

    Public Sub SendAttackPacket()
        SendPacket("080101" & GetTargetID() & "FF00000000")
    End Sub

    Public Sub SendParty(ByRef nick As String)
        Dim NameLen As String : NameLen = Strings.Left(AlignDWORD(Len(nick)), 2)
        SendPacket("2F01" & NameLen & "00" & StringToHex(nick))
        SendPacket("2F03" & NameLen & "00" & StringToHex(nick))
    End Sub

    Public Sub UseHealthPotion(ByVal Potion As String)
        Dim PotionID As String = ""
        If GetHp() < GetMaxHp() Then
            Select Case Potion
                Case "Water Of Favors" : PotionID = "014"
                Case "Water Of Grace" : PotionID = "013"
                Case "Water Of Love" : PotionID = "012"
                Case "Water Of Life" : PotionID = "011"
                Case "Holy Water" : PotionID = "010"
                Case "Ibexs" : PotionID = "071"
            End Select

            If IsItemExist(Potion) Then
                SendPacket("3103" + AlignDWORD("490" + PotionID).Substring(0, 6) + "00" + GetID() + GetID() + "0000000000000000000000000000")
            End If
        End If
    End Sub

    Public Sub UseManaPotion(ByVal Potion As String)
        Dim PotionID As String = ""
        If GetMp() < GetMaxMp() Then
            Select Case Potion
                Case "Potion Of Soul" : PotionID = "020"
                Case "Potion Of Wisdom" : PotionID = "019"
                Case "Potion Of Sagacity" : PotionID = "018"
                Case "Potion Of Wisdom" : PotionID = "017"
                Case "Potion Of Spirit" : PotionID = "016"
                Case "Crisis" : PotionID = "072"
            End Select

            If IsItemExist(Potion) Then
                SendPacket("3103" + AlignDWORD("490" + PotionID).Substring(0, 6) + "00" + GetID() + GetID() + "0000000000000000000000000000")
            End If
        End If
    End Sub

    Public Function UseBuff(ByVal Skill As String, ByVal TargetId As Long, Optional ByVal Affected As Boolean = False) As Boolean
        Dim SkillSelected As String = ""
        Dim ManaCost As Long = 0

        Select Case Skill
            Case "Superioris" : SkillSelected = "675" : ManaCost = 120
            Case "Insensibility Guard" : SkillSelected = "674" : ManaCost = 120
            Case "Imposingness" : SkillSelected = "670" : ManaCost = 120
            Case "Insensibility Peel" : SkillSelected = "660" : ManaCost = 150
            Case "Massiveness" : SkillSelected = "657" : ManaCost = 360
            Case "Heapness" : SkillSelected = "655" : ManaCost = 300
            Case "Undying" : SkillSelected = "654" : ManaCost = 240
            Case "Insensibility Protector" : SkillSelected = "651" : ManaCost = 100
            Case "Fresh Mind" : SkillSelected = "645" : ManaCost = 60
            Case "Mightness" : SkillSelected = "642" : ManaCost = 240
            Case "Insensibility Barrier" : SkillSelected = "639" : ManaCost = 80
            Case "Calm Mind" : SkillSelected = "636" : ManaCost = 45
            Case "Hardness" : SkillSelected = "633" : ManaCost = 120
            Case "Insensibility Shield" : SkillSelected = "630" : ManaCost = 80
            Case "Bright Mind" : SkillSelected = "627" : ManaCost = 30
            Case "Strong" : SkillSelected = "624" : ManaCost = 60
            Case "Insensibility Armor" : SkillSelected = "621" : ManaCost = 40
            Case "Brave" : SkillSelected = "615" : ManaCost = 30
            Case "Insensibility Shell" : SkillSelected = "612" : ManaCost = 20
            Case "Resist All" : SkillSelected = "609" : ManaCost = 15
            Case "Grace" : SkillSelected = "606" : ManaCost = 15
            Case "Insensibility Skin" : SkillSelected = "603" : ManaCost = 10
            Case "Strength" : SkillSelected = "004" : ManaCost = 10
            Case "Resist Fire" : SkillSelected = "506" : ManaCost = 15
            Case "Endure Fire" : SkillSelected = "524" : ManaCost = 50
            Case "Immunity Fire" : SkillSelected = "548" : ManaCost = 80
            Case "Resist Cold" : SkillSelected = "606" : ManaCost = 15
            Case "Frozen Armor" : SkillSelected = "612" : ManaCost = 40
            Case "Endure Cold" : SkillSelected = "624" : ManaCost = 50
            Case "Immunity Cold" : SkillSelected = "648" : ManaCost = 80
            Case "Resist Lightning" : SkillSelected = "706" : ManaCost = 15
            Case "Endure Lightning" : SkillSelected = "724" : ManaCost = 50
            Case "Immunity Lightning" : SkillSelected = "748" : ManaCost = 80
        End Select

        If SkillSelected = "" Or TargetId = 0 Then Return False

        If (Affected And IsRightSkillAffected(SkillSelected) = True) Then Return False

        If GetMp() >= ManaCost Then
            If GetLongID() = TargetId Then
                'SendPacket("3106" + AlignDWORD(GetClass().ToString() & SkillSelected).Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")
            End If

            SendPacket("3101" + AlignDWORD(GetClass().ToString() & SkillSelected).Substring(0, 6) + "00" + GetID() + AlignDWORD(TargetId).Substring(0, 4).ToUpper() + "00000000000000000000000000000F00")
            SendPacket("3103" + AlignDWORD(GetClass().ToString() & SkillSelected).Substring(0, 6) + "00" + GetID() + AlignDWORD(TargetId).Substring(0, 4).ToUpper() + "000000000000000000000000")
            Return True
        End If

        Return False
    End Function

    Public Function UseTransformation(ByVal Transformation As String) As Boolean
        Dim TransformationID As String = ""

        Select Case Transformation
            Case "Kecoon" : TransformationID = "472020"
            Case "Orc Bowman" : TransformationID = "472310"
            Case "Death Knight" : TransformationID = "472150"
            Case "Burning Skeleton" : TransformationID = "472202"
            Case "Bulture" : TransformationID = "472040"
            Case "Human Hera (L) 1" : TransformationID = "520530"
            Case "Human Hera (L) 2" : TransformationID = "520531"
            Case "Karus Hera (PUS)" : TransformationID = "500512"
            Case "Karus Cougar (PUS)" : TransformationID = "500511"
            Case "Karus Menicia (PUS)" : TransformationID = "500510"
            Case "Karus Patrick (PUS)" : TransformationID = "500509"
            Case "Human Hera (PUS)" : TransformationID = "500508"
            Case "Human Cougar (PUS)" : TransformationID = "500507"
            Case "Human Menicia (PUS)" : TransformationID = "500506"
            Case "Human Patrick (PUS)" : TransformationID = "500505"
        End Select

        If TransformationID >= 500505 And TransformationID <= 520530 Then
            If IsLeftSkillAffected(520, 3) = True Then Return False
        Else
            If IsLeftSkillAffected(472, 3) = True Or IsItemExist("Transformation Gem") = False Or IsTransformationAvailableZone() = False Then Return False
        End If

        SendPacket("3103" + AlignDWORD(TransformationID).Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")

        Return True

    End Function

    Public Function UseHeal(ByVal Skill As String, ByVal TargetId As Long, Optional ByVal Affected As Boolean = False) As Boolean
        Dim SkillId As String = ""
        Dim ManaCost As Long = 0

        Select Case Skill
            Case "Past Restore" : SkillId = "580" : ManaCost = 120
            Case "Past Recovery	" : SkillId = "575" : ManaCost = 120
            Case "Critical Restore" : SkillId = "570" : ManaCost = 120
            Case "Group Complete Healing" : SkillId = "560" : ManaCost = 1920
            Case "Group Massive Healing" : SkillId = "557" : ManaCost = 960
            Case "Complete Healing" : SkillId = "554" : ManaCost = 960
            Case "Superior Restore" : SkillId = "548" : ManaCost = 625
            Case "Superior Healing" : SkillId = "545" : ManaCost = 320
            Case "Massive Restore" : SkillId = "539" : ManaCost = 375
            Case "Massive Healing" : SkillId = "536" : ManaCost = 160
            Case "Great Restore" : SkillId = "530" : ManaCost = 200
            Case "Great Healing" : SkillId = "527" : ManaCost = 80
            Case "Major Restore" : SkillId = "521" : ManaCost = 100
            Case "Major Healing" : SkillId = "518" : ManaCost = 40
            Case "Restore" : SkillId = "512" : ManaCost = 30
            Case "Healing" : SkillId = "509" : ManaCost = 20
            Case "Light Restore" : SkillId = "503" : ManaCost = 25
            Case "Minor Healing" : SkillId = "500" : ManaCost = 10
        End Select

        If (Affected And IsRightSkillAffected(SkillId) = True) Then Return False

        If GetMp() >= ManaCost Then
            SendPacket("3101" + AlignDWORD(GetClass().ToString() & SkillId).Substring(0, 6) + "00" + GetID() + AlignDWORD(TargetId).Substring(0, 4).ToUpper() + "00000000000000000000000000000F00")
            SendPacket("3103" + AlignDWORD(GetClass().ToString() & SkillId).Substring(0, 6) + "00" + GetID() + AlignDWORD(TargetId).Substring(0, 4).ToUpper() + "000000000000000000000000")
            Return True
        End If

        Return False
    End Function
    Function ReadSkillAffectedFirstLine(ByVal skill As Integer) As Long
        Dim i As Integer
        Dim tmpBase As Long
        tmpBase = ReadLong(ReadLong(KO_PTR_DLG) + KO_OFF_SKILL_BASE)
        tmpBase = ReadLong(tmpBase + &H4)
        tmpBase = ReadLong(tmpBase + KO_OFF_SKILL_ID)
        For i = 1 To skill
            tmpBase = ReadLong(tmpBase + &H0)
        Next
        tmpBase = ReadLong(tmpBase + &H8)
        If tmpBase > 0 Then
            tmpBase = ReadLong(tmpBase + &H0)
            ReadSkillAffectedFirstLine = tmpBase
        Else
            ReadSkillAffectedFirstLine = 0
        End If
    End Function

    Function ReadSkillAffectedSecondLine(ByVal skill As Integer) As Long
        Dim i As Integer
        Dim tmpBase As Long
        tmpBase = ReadLong(ReadLong(KO_PTR_DLG) + KO_OFF_SKILL_BASE)
        tmpBase = ReadLong(tmpBase + &H8)
        tmpBase = ReadLong(tmpBase + KO_OFF_SKILL_ID)
        For i = 1 To skill
            tmpBase = ReadLong(tmpBase + &H0)
        Next
        tmpBase = ReadLong(tmpBase + &H8)
        If tmpBase > 0 Then
            tmpBase = ReadLong(tmpBase + &H0)
            ReadSkillAffectedSecondLine = tmpBase
        Else
            ReadSkillAffectedSecondLine = 0
        End If
    End Function

    Function ReadSkillAffected(ByVal skill As Integer) As Long
        If ReadSkillAffectedFirstLine(skill) > 0 Then
            Return ReadSkillAffectedFirstLine(skill)
        End If
        If ReadSkillAffectedSecondLine(skill) > 0 Then
            Return ReadSkillAffectedSecondLine(skill)
        End If
        Return 0
    End Function

    Function IsRightSkillAffected(ByVal Skill As Long, Optional ByVal Limit As Long = 3) As Boolean
        Dim i As Integer
        IsRightSkillAffected = False
        For i = 1 To 20
            Console.WriteLine(ReadSkillAffected(i))
            If Right(ReadSkillAffected(i), Limit) = Skill Then IsRightSkillAffected = True : Exit For : Exit Function
            If i = 20 Then IsRightSkillAffected = False
        Next
    End Function

    Function IsLeftSkillAffected(ByVal Skill As Long, Optional ByVal Limit As Long = 3) As Boolean
        Dim i As Integer
        IsLeftSkillAffected = False
        For i = 1 To 20
            If Left(ReadSkillAffected(i), Limit) = Skill Then IsLeftSkillAffected = True : Exit For : Exit Function
            If i = 20 Then IsLeftSkillAffected = False
        Next
    End Function

    Public Function UseSkill(ByVal Skill As String, Optional ByVal Attack As Boolean = False) As Boolean
        Select Case GetJob(GetClass())
            Case "Warrior" : Return UseWarriorSkill(Skill, Attack)
            Case "Rogue" : Return UseRogueSkill(Skill, Attack)
            Case "Priest" : Return UsePriestSkill(Skill, Attack)
            Case "Mage" : Return UseMageSkill(Skill, Attack)
        End Select
        Return False
    End Function

    Public Function UseTimedSkill(ByVal Skill As String) As Boolean
        Select Case Skill
            Case "Sprint" : Return UseSprint()
            Case "Wolf" : Return UseWolf()
            Case "Swift" : Return UseSwift(GetLongID(), True)
            Case "Light Feet" : Return UseLightFeet()
            Case "Evade" : Return UseEvade()
            Case "Safety" : Return UseSafety()
            Case "Lupine Eyes" : Return UseLupineEyes()
            Case "Hide" : Return UseHide()
            Case "Stealth" : Return UseStealth()
            Case "Gain" : Return UseGain()
            Case "Defense" : Return UseDefense()
            Case "Strength" : Return UseBuff(Skill, GetLongID(), True)
            Case "Resist Fire" : Return UseBuff(Skill, GetLongID(), True)
            Case "Endure Fire" : Return UseBuff(Skill, GetLongID(), True)
            Case "Immunity Fire" : Return UseBuff(Skill, GetLongID(), True)
            Case "Resist Cold" : Return UseBuff(Skill, GetLongID(), True)
            Case "Frozen Armor" : Return UseBuff(Skill, GetLongID(), True)
            Case "Endure Cold" : Return UseBuff(Skill, GetLongID(), True)
            Case "Immunity Cold" : Return UseBuff(Skill, GetLongID(), True)
            Case "Resist Lightning" : Return UseBuff(Skill, GetLongID(), True)
            Case "Endure Lightning" : Return UseBuff(Skill, GetLongID(), True)
            Case "Immunity Lightning" : Return UseBuff(Skill, GetLongID(), True)
            Case "Prayer Of Cronos" : Return UsePrayerOfCronos()
            Case "Prayer Of God's Power" : Return UsePrayerOfGodPower()
            Case "Blasting" : Return UseBlasting()
            Case "Wildness" : Return UseWildness()
            Case "Eruption" : Return UseEruption()
        End Select
        Return False
    End Function

    Public Function UseDeBuffSkill(ByVal Skill As String) As Boolean
        If IsAttackableTarget() = False Then Return False

        Select Case Skill
            Case "Malice" : Return UseMalice()
            Case "Confusion" : Return UseConfusion()
            Case "Slow" : Return UseSlow()
            Case "Reverse Life" : Return UseReverseLife()
            Case "Sleep Wing" : Return UseSleepWing()
            Case "Parasite" : Return UseParasite()
            Case "Sleep Carpet" : Return UseSleepCarpet()
            Case "Torment" : Return UseTorment()
            Case "Massive" : Return UseMassive()
        End Select

        Return False
    End Function

    Public Function UseTorment() As Boolean
        If GetMp() >= 150 And GetTargetID() <> "FFFF" Then
            SendPacket("3101" & AlignDWORD(Int32.Parse(GetClass().ToString() & "757")).Substring(0, 6) & "00" & GetID() & "FFFF" & AlignDWORD(GetTargetX()).Substring(0, 4) & AlignDWORD(GetTargetZ()).Substring(0, 4) & AlignDWORD(GetTargetY()).Substring(0, 4) & "00000000000000000F00")
            SendPacket("3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & "757")).Substring(0, 6) & "00" & GetID() & "FFFF" & AlignDWORD(GetTargetX()).Substring(0, 4) & AlignDWORD(GetTargetZ()).Substring(0, 4) & AlignDWORD(GetTargetY()).Substring(0, 4) & "000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseSleepCarpet() As Boolean
        If GetMp() >= 240 And GetTargetID() <> "FFFF" Then
            SendPacket("3101" & AlignDWORD(Int32.Parse(GetClass().ToString() & "751")).Substring(0, 6) & "00" & GetID() & "FFFF" & AlignDWORD(GetTargetX()).Substring(0, 4) & AlignDWORD(GetTargetZ()).Substring(0, 4) & AlignDWORD(GetTargetY()).Substring(0, 4) & "00000000000000000F00")
            SendPacket("3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & "751")).Substring(0, 6) & "00" & GetID() & "FFFF" & AlignDWORD(GetTargetX()).Substring(0, 4) & AlignDWORD(GetTargetZ()).Substring(0, 4) & AlignDWORD(GetTargetY()).Substring(0, 4) & "000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseMassive() As Boolean
        If GetMp() >= 180 And GetTargetID() <> "FFFF" Then
            SendPacket("3101" & AlignDWORD(Int32.Parse(GetClass().ToString() & "760")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000000000000000000000F00")
            SendPacket("3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & "760")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseParasite() As Boolean
        If GetMp() >= 100 And GetTargetID() <> "FFFF" Then
            SendPacket("3101" & AlignDWORD(Int32.Parse(GetClass().ToString() & "745")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000000000000000000000F00")
            SendPacket("3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & "745")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseSleepWing() As Boolean
        If GetMp() >= 120 And GetTargetID() <> "FFFF" Then
            SendPacket("3101" & AlignDWORD(Int32.Parse(GetClass().ToString() & "730")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000000000000000000000F00")
            SendPacket("3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & "730")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseReverseLife() As Boolean
        If GetMp() >= 50 And GetTargetID() <> "FFFF" Then
            SendPacket("3101" & AlignDWORD(Int32.Parse(GetClass().ToString() & "727")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000000000000000000000F00")
            SendPacket("3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & "727")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseSlow() As Boolean
        If GetMp() >= 120 And GetTargetID() <> "FFFF" Then
            SendPacket("3101" & AlignDWORD(Int32.Parse(GetClass().ToString() & "724")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000000000000000000000F00")
            SendPacket("3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & "724")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseMalice() As Boolean
        If GetMp() >= 40 And GetTargetID() <> "FFFF" Then
            SendPacket("3101" & AlignDWORD(Int32.Parse(GetClass().ToString() & "703")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000000000000000000000F00")
            SendPacket("3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & "703")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseConfusion() As Boolean
        If GetMp() >= 80 And GetTargetID() <> "FFFF" Then
            SendPacket("3101" & AlignDWORD(Int32.Parse(GetClass().ToString() & "715")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000000000000000000000F00")
            SendPacket("3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & "715")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseRogueSkill(ByVal Skill As String, ByVal Attack As Boolean) As Boolean
        Dim SkillSelected As String = ""
        Dim AttackDistance As Long = 12

        Select Case Skill
            Case "Archery" : SkillSelected = "003" : AttackDistance = 12
            Case "Through Shot" : SkillSelected = "500" : AttackDistance = 12
            Case "Fire Arrow" : SkillSelected = "505" : AttackDistance = 12
            Case "Poison Arrow" : SkillSelected = "510" : AttackDistance = 12
            Case "Multiple Shot" : SkillSelected = "515" : AttackDistance = 1
            Case "Guided Arrow" : SkillSelected = "520" : AttackDistance = 12
            Case "Perfect Shot" : SkillSelected = "525" : AttackDistance = 12
            Case "Fire Shot" : SkillSelected = "530" : AttackDistance = 12
            Case "Poison Shot" : SkillSelected = "535" : AttackDistance = 12
            Case "Arc Shot" : SkillSelected = "540" : AttackDistance = 12
            Case "Explosive Shot" : SkillSelected = "545" : AttackDistance = 12
            Case "Viper" : SkillSelected = "550" : AttackDistance = 12
            Case "Counter Strike" : SkillSelected = "552" : AttackDistance = 12
            Case "Arrow Shower" : SkillSelected = "555" : AttackDistance = 1
            Case "Shadow Shot" : SkillSelected = "557" : AttackDistance = 12
            Case "Shadow Hunter" : SkillSelected = "560" : AttackDistance = 12
            Case "Ice Shot" : SkillSelected = "562" : AttackDistance = 12
            Case "Lightning Shot" : SkillSelected = "566" : AttackDistance = 12
            Case "Dark Pursuer" : SkillSelected = "570" : AttackDistance = 12
            Case "Blow Arrow" : SkillSelected = "572" : AttackDistance = 12
            Case "Blinding Strafe" : SkillSelected = "580" : AttackDistance = 12
            Case "Süper Archer" : SkillSelected = "1000" : AttackDistance = 3 ' Custom Skill ID
        End Select

        If SkillSelected.Equals("") = False Then
            If IsItemExist("Arrow") = False Or CoordinateDistance(GetX(), GetY(), GetTargetX(), GetTargetY()) > AttackDistance Then Return False
            If SkillSelected.Equals("515") Then ' Multiple Shot
                If IsAttackableTarget() = False Then Return False

                Dim MultipleShotMem As Long = VirtualAllocEx(_Handle, 0, 1, MEM_COMMIT Or MEM_RESERVE, &H40)

                InjectPatch(MultipleShotMem, "608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(MultipleShotMem + &H100) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1768" & AlignDWORD(MultipleShotMem + &H120) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(MultipleShotMem + &H140) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(MultipleShotMem + &H160) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(MultipleShotMem + &H180) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761C3")

                InjectPatch(MultipleShotMem + &H100, "3101" & AlignDWORD(Int32.Parse(GetClass().ToString() & SkillSelected)).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000000000000000000000D00")
                InjectPatch(MultipleShotMem + &H120, "3102" & AlignDWORD(Int32.Parse(GetClass().ToString() & SkillSelected)).Substring(0, 6) & "00" & GetID() & GetTargetID() & "000000000000010000000000")
                InjectPatch(MultipleShotMem + &H140, "3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & SkillSelected)).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000001000000000000000000")
                InjectPatch(MultipleShotMem + &H160, "3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & SkillSelected)).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000002000000000000000000")
                InjectPatch(MultipleShotMem + &H180, "3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & SkillSelected)).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000003000000000000000000")

                Dim hMultipleShotThread As Integer = CreateRemoteThread(_Handle, 0, 0, MultipleShotMem, 0, 0, 0)
                WaitForSingleObject(hMultipleShotThread, &HFFFF)
                CloseHandle(hMultipleShotThread)

                VirtualFreeEx(_Handle, MultipleShotMem, 0, MEM_RELEASE)
                Return True
            ElseIf SkillSelected.Equals("552") Then ' Counter Strike
                If IsAttackableTarget() = False Then Return False

                Dim CounterStrikeMem As Long = VirtualAllocEx(_Handle, 0, 1, MEM_COMMIT Or MEM_RESERVE, &H40)

                InjectPatch(CounterStrikeMem, "608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(CounterStrikeMem + &H100) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1768" & AlignDWORD(CounterStrikeMem + &H120) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(CounterStrikeMem + &H140) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761C3")

                InjectPatch(CounterStrikeMem + &H100, "3101" & AlignDWORD(Int32.Parse(GetClass().ToString() & SkillSelected)).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000000000000000000000A00")
                InjectPatch(CounterStrikeMem + &H120, "3102" & AlignDWORD(Int32.Parse(GetClass().ToString() & SkillSelected)).Substring(0, 6) & "00" & GetID() & GetTargetID() & "000000000000010000000000")
                InjectPatch(CounterStrikeMem + &H140, "3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & SkillSelected)).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000001000000000000000000")

                Dim hArcheryThread As Integer = CreateRemoteThread(_Handle, 0, 0, CounterStrikeMem, 0, 0, 0)
                WaitForSingleObject(hArcheryThread, &HFFFF)
                CloseHandle(hArcheryThread)

                VirtualFreeEx(_Handle, CounterStrikeMem, 0, MEM_RELEASE)
                Return True
            ElseIf SkillSelected.Equals("555") Then ' Arrow Shower
                If IsAttackableTarget() = False Then Return False

                Dim ArrowShowerMem As Long = VirtualAllocEx(_Handle, 0, 1, MEM_COMMIT Or MEM_RESERVE, &H40)

                InjectPatch(ArrowShowerMem, "608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(ArrowShowerMem + &H100) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1768" & AlignDWORD(ArrowShowerMem + &H120) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(ArrowShowerMem + &H140) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(ArrowShowerMem + &H160) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(ArrowShowerMem + &H180) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(ArrowShowerMem + &H1A0) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(ArrowShowerMem + &H1C0) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761C3")

                InjectPatch(ArrowShowerMem + &H100, "3101" & AlignDWORD(Int32.Parse(GetClass().ToString() & SkillSelected)).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000000000000000000000F00")
                InjectPatch(ArrowShowerMem + &H120, "3102" & AlignDWORD(Int32.Parse(GetClass().ToString() & SkillSelected)).Substring(0, 6) & "00" & GetID() & GetTargetID() & "000000000000010000000000")
                InjectPatch(ArrowShowerMem + &H140, "3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & SkillSelected)).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000001000000000000000000")
                InjectPatch(ArrowShowerMem + &H160, "3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & SkillSelected)).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000002000000000000000000")
                InjectPatch(ArrowShowerMem + &H180, "3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & SkillSelected)).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000003000000000000000000")
                InjectPatch(ArrowShowerMem + &H1A0, "3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & SkillSelected)).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000004000000000000000000")
                InjectPatch(ArrowShowerMem + &H1C0, "3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & SkillSelected)).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000005000000000000000000")

                Dim hArrowShowerThread As Integer = CreateRemoteThread(_Handle, 0, 0, ArrowShowerMem, 0, 0, 0)
                WaitForSingleObject(hArrowShowerThread, &HFFFF)
                CloseHandle(hArrowShowerThread)

                VirtualFreeEx(_Handle, ArrowShowerMem, 0, MEM_RELEASE)
                Return True
            ElseIf SkillSelected.Equals("1000") Then ' Süper Archer
                If IsAttackableTarget() = False Then Return False

                Dim MultipleShotMem As Long = VirtualAllocEx(_Handle, 0, 1, MEM_COMMIT Or MEM_RESERVE, &H40)

                InjectPatch(MultipleShotMem, "608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(MultipleShotMem + &H100) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1768" & AlignDWORD(MultipleShotMem + &H120) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(MultipleShotMem + &H140) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(MultipleShotMem + &H160) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(MultipleShotMem + &H180) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761C3")

                InjectPatch(MultipleShotMem + &H100, "3101" & AlignDWORD(Int32.Parse(GetClass().ToString() & "515")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000000000000000000000D00")
                InjectPatch(MultipleShotMem + &H120, "3102" & AlignDWORD(Int32.Parse(GetClass().ToString() & "515")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "000000000000010000000000")
                InjectPatch(MultipleShotMem + &H140, "3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & "515")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000001000000000000000000")
                InjectPatch(MultipleShotMem + &H160, "3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & "515")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000002000000000000000000")
                InjectPatch(MultipleShotMem + &H180, "3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & "515")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000003000000000000000000")

                Dim hMultipleShotThread As Integer = CreateRemoteThread(_Handle, 0, 0, MultipleShotMem, 0, 0, 0)
                WaitForSingleObject(hMultipleShotThread, &HFFFF)
                CloseHandle(hMultipleShotThread)

                VirtualFreeEx(_Handle, MultipleShotMem, 0, MEM_RELEASE)

                'Thread.Sleep(100)

                'Dim ArcheryMem As Long = VirtualAllocEx(_Handle, 0, 1, MEM_COMMIT Or MEM_RESERVE, &H40)

                '''InjectPatch(ArcheryMem, "608B0D" &
                'AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(ArcheryMem + &H100) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                'AlignDWORD(KO_PTR_PKT) & "6A1768" & AlignDWORD(ArcheryMem + &H120) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                'AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(ArcheryMem + &H140) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761C3")

                'InjectPatch(ArcheryMem + &H100, "3101" & AlignDWORD(Int32.Parse(GetClass().ToString() & "500")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000000000000000000000D00")
                'InjectPatch(ArcheryMem + &H120, "3102" & AlignDWORD(Int32.Parse(GetClass().ToString() & "500")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "000000000000010000000000")
                'InjectPatch(ArcheryMem + &H140, "3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & "500")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000001000000000000000000")

                'Dim hArcheryThread As Integer = CreateRemoteThread(_Handle, 0, 0, ArcheryMem, 0, 0, 0)
                'WaitForSingleObject(hArcheryThread, &HFFFF)
                'CloseHandle(hArcheryThread)

                'VirtualFreeEx(_Handle, ArcheryMem, 0, MEM_RELEASE)

                'Thread.Sleep(100)

                Dim ArrowShowerMem As Long = VirtualAllocEx(_Handle, 0, 1, MEM_COMMIT Or MEM_RESERVE, &H40)

                InjectPatch(ArrowShowerMem, "608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(ArrowShowerMem + &H100) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1768" & AlignDWORD(ArrowShowerMem + &H120) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(ArrowShowerMem + &H140) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(ArrowShowerMem + &H160) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(ArrowShowerMem + &H180) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(ArrowShowerMem + &H1A0) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(ArrowShowerMem + &H1C0) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761C3")

                InjectPatch(ArrowShowerMem + &H100, "3101" & AlignDWORD(Int32.Parse(GetClass().ToString() & "555")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000000000000000000000F00")
                InjectPatch(ArrowShowerMem + &H120, "3102" & AlignDWORD(Int32.Parse(GetClass().ToString() & "555")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "000000000000010000000000")
                InjectPatch(ArrowShowerMem + &H140, "3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & "555")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000001000000000000000000")
                InjectPatch(ArrowShowerMem + &H160, "3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & "555")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000002000000000000000000")
                InjectPatch(ArrowShowerMem + &H180, "3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & "555")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000003000000000000000000")
                InjectPatch(ArrowShowerMem + &H1A0, "3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & "555")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000004000000000000000000")
                InjectPatch(ArrowShowerMem + &H1C0, "3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & "555")).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000005000000000000000000")

                Dim hArrowShowerThread As Integer = CreateRemoteThread(_Handle, 0, 0, ArrowShowerMem, 0, 0, 0)
                WaitForSingleObject(hArrowShowerThread, &HFFFF)
                CloseHandle(hArrowShowerThread)

                VirtualFreeEx(_Handle, ArrowShowerMem, 0, MEM_RELEASE)

                Return True
            Else
                If IsAttackableTarget() = False Then Return False

                Dim ArcheryMem As Long = VirtualAllocEx(_Handle, 0, 1, MEM_COMMIT Or MEM_RESERVE, &H40)

                InjectPatch(ArcheryMem, "608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(ArcheryMem + &H100) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1768" & AlignDWORD(ArcheryMem + &H120) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761608B0D" &
                    AlignDWORD(KO_PTR_PKT) & "6A1B68" & AlignDWORD(ArcheryMem + &H140) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD761C3")

                InjectPatch(ArcheryMem + &H100, "3101" & AlignDWORD(Int32.Parse(GetClass().ToString() & SkillSelected)).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000000000000000000000D00")
                InjectPatch(ArcheryMem + &H120, "3102" & AlignDWORD(Int32.Parse(GetClass().ToString() & SkillSelected)).Substring(0, 6) & "00" & GetID() & GetTargetID() & "000000000000010000000000")
                InjectPatch(ArcheryMem + &H140, "3103" & AlignDWORD(Int32.Parse(GetClass().ToString() & SkillSelected)).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000001000000000000000000")

                Dim hArcheryThread As Integer = CreateRemoteThread(_Handle, 0, 0, ArcheryMem, 0, 0, 0)
                WaitForSingleObject(hArcheryThread, &HFFFF)
                CloseHandle(hArcheryThread)

                VirtualFreeEx(_Handle, ArcheryMem, 0, MEM_RELEASE)
                Return True
            End If
        Else
            Select Case Skill
                Case "Stab" : SkillSelected = "005" : AttackDistance = 12
                Case "Stab2" : SkillSelected = "006" : AttackDistance = 12
                Case "Jab" : SkillSelected = "600" : AttackDistance = 12
                Case "Pierce" : SkillSelected = "615" : AttackDistance = 12
                Case "Shock" : SkillSelected = "620" : AttackDistance = 12
                Case "Thrust" : SkillSelected = "635" : AttackDistance = 12
                Case "Cut" : SkillSelected = "640" : AttackDistance = 12
                Case "Spike" : SkillSelected = "655" : AttackDistance = 12
                Case "Blody Beast" : SkillSelected = "670" : AttackDistance = 12
                Case "Blinding" : SkillSelected = "675" : AttackDistance = 12
                Case "Blood Drain" : SkillSelected = "610" : AttackDistance = 12
                Case "Vampiric Touch" : SkillSelected = "650" : AttackDistance = 12
            End Select

            If CoordinateDistance(GetX(), GetY(), GetTargetX(), GetTargetY()) > AttackDistance Then Return False
            If IsAttackableTarget() = False Then Return False

            If SkillSelected.Equals("610") Or SkillSelected.Equals("650") Then ' Blood Drain And Vampiric Touch
                SendPacket("3101" & AlignDWORD(Int32.Parse(GetClass().ToString() + SkillSelected)).Substring(0, 6) & "00" & GetID() & GetTargetID() & "00000000000000000000000000001000")
                SendPacket("3103" & AlignDWORD(Int32.Parse(GetClass().ToString() + SkillSelected)).Substring(0, 6) & "00" & GetID() & GetTargetID() & "000000000000000000000000")
            Else
                If Attack Then
                    SendAttackPacket()
                    Thread.Sleep(150)
                End If

                SendPacket("3103" & AlignDWORD(Int32.Parse(GetClass().ToString() + SkillSelected)).Substring(0, 6) & "00" & GetID() & GetTargetID() & "000000000000000000000000")
            End If

            Return True
        End If
        Return False
    End Function

    Public Function UseWarriorSkill(ByVal Skill As String, ByVal Attack As Boolean) As Boolean
        Dim SkillSelected As String = ""
        Dim AttackDistance As Long = 3

        Select Case Skill
            Case "Slash" : SkillSelected = "003" : AttackDistance = 12
            Case "Crash" : SkillSelected = "005" : AttackDistance = 12
            Case "Piercing" : SkillSelected = "009" : AttackDistance = 12
            Case "Whipping" : SkillSelected = "010" : AttackDistance = 12
            Case "Hash" : SkillSelected = "500" : AttackDistance = 12
            Case "Hoodwink" : SkillSelected = "505" : AttackDistance = 12
            Case "Shear" : SkillSelected = "510" : AttackDistance = 12
            Case "Pierce" : SkillSelected = "515" : AttackDistance = 12
            Case "Carwing" : SkillSelected = "525" : AttackDistance = 12
            Case "Sever" : SkillSelected = "530" : AttackDistance = 12
            Case "Prick" : SkillSelected = "535" : AttackDistance = 12
            Case "Multiple Shock" : SkillSelected = "540" : AttackDistance = 12
            Case "Cleave" : SkillSelected = "545" : AttackDistance = 12
            Case "Mangling" : SkillSelected = "550" : AttackDistance = 12
            Case "Thrust" : SkillSelected = "555" : AttackDistance = 12
            Case "Sword Aura" : SkillSelected = "557" : AttackDistance = 12
            Case "Sword Dancing" : SkillSelected = "560" : AttackDistance = 12
            Case "Howling Sword" : SkillSelected = "570" : AttackDistance = 12
            Case "Blooding" : SkillSelected = "575" : AttackDistance = 12
            Case "Hell Blade" : SkillSelected = "580" : AttackDistance = 12
        End Select

        If CoordinateDistance(GetX(), GetY(), GetTargetX(), GetTargetY()) > AttackDistance Then Return False
        If IsAttackableTarget() = False Then Return False

        If Attack Then
            SendAttackPacket()
            Thread.Sleep(150)
        End If

        'SendPacket("3101" + AlignDWORD(Int32.Parse(GetClass().ToString() + SkillSelected)).Substring(0, 6) + "00" + GetID() + GetTargetID() + "5D020600B6019BFF0000F0000A00")
        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + SkillSelected)).Substring(0, 6) + "00" + GetID() + GetTargetID() + "000000000000000000000000")

        Return True
    End Function

    Public Function UseMageSkill(ByVal Skill As String, ByVal Attack As Boolean) As Boolean
        Dim SkillSelected As String = ""
        Dim Type As Int32 = -1
        Dim AttackDistance As Long = 12

        Select Case Skill
            Case "Burn" : SkillSelected = "503" : Type = 1 : AttackDistance = 12
            Case "Ignition" : SkillSelected = "518" : Type = 1 : AttackDistance = 12
            Case "Specter Of Fire" : SkillSelected = "543" : Type = 1 : AttackDistance = 3
            Case "Manes Of Fire" : SkillSelected = "556" : Type = 1 : AttackDistance = 3
            Case "Manes Of Ice" : SkillSelected = "656" : Type = 1 : AttackDistance = 3
            Case "Specter Of Ice" : SkillSelected = "643" : Type = 1 : AttackDistance = 3
            Case "Specter Of Thunder" : SkillSelected = "743" : Type = 1 : AttackDistance = 3
            Case "Incineration" : SkillSelected = "570" : Type = 1 : AttackDistance = 12
            Case "Manes Of Thunder" : SkillSelected = "756" : Type = 1 : AttackDistance = 3
            Case "Charge" : SkillSelected = "703" : Type = 1 : AttackDistance = 12

            Case "Fire Blast" : SkillSelected = "535" : Type = 2 : AttackDistance = 12
            Case "Blaze" : SkillSelected = "509" : Type = 2 : AttackDistance = 12
            Case "Freeze" : SkillSelected = "603" : Type = 2 : AttackDistance = 12
            Case "Chill" : SkillSelected = "609" : Type = 2 : AttackDistance = 12
            Case "Solid" : SkillSelected = "618" : Type = 2 : AttackDistance = 12
            Case "Hell Fire" : SkillSelected = "539" : Type = 2 : AttackDistance = 12
            Case "Pillar Of Fire" : SkillSelected = "551" : Type = 2 : AttackDistance = 12
            Case "Fire Thorn" : SkillSelected = "554" : Type = 2 : AttackDistance = 12
            Case "Fire Impact" : SkillSelected = "557" : Type = 2 : AttackDistance = 12
            Case "Frostbite" : SkillSelected = "639" : Type = 2 : AttackDistance = 12
            Case "Ice Comet" : SkillSelected = "651" : Type = 2 : AttackDistance = 12
            Case "Ice Impact" : SkillSelected = "657" : Type = 2 : AttackDistance = 12
            Case "Ice Blast" : SkillSelected = "635" : Type = 2 : AttackDistance = 12
            Case "Counter Spell" : SkillSelected = "709" : Type = 2 : AttackDistance = 12
            Case "Lightining" : SkillSelected = "715" : Type = 2 : AttackDistance = 12
            Case "Static Hemispher" : SkillSelected = "718" : Type = 2 : AttackDistance = 12
            Case "Thunder" : SkillSelected = "727" : Type = 2 : AttackDistance = 12
            Case "Thunder Blast" : SkillSelected = "735" : Type = 2 : AttackDistance = 12
            Case "Static Thorn" : SkillSelected = "754" : Type = 2 : AttackDistance = 12

            Case "Fire Spear" : SkillSelected = "527" : Type = 3 : AttackDistance = 12
            Case "Fire Ball" : SkillSelected = "515" : Type = 3 : AttackDistance = 12
            Case "Ice Orb" : SkillSelected = "627" : Type = 3 : AttackDistance = 12
            Case "Static Orb" : SkillSelected = "751" : Type = 3 : AttackDistance = 12
            Case "Thunder Impact" : SkillSelected = "757" : Type = 3 : AttackDistance = 12

            Case "Flame Blade" : SkillSelected = "542" : Type = 4 : AttackDistance = 12
            Case "Frozen Blade" : SkillSelected = "642" : Type = 4 : AttackDistance = 12
            Case "Charged Blade" : SkillSelected = "742" : Type = 4 : AttackDistance = 12
            
            Case "Ice Burst" : SkillSelected = "633" : Type = 5 : AttackDistance = 12
            Case "Fire Burst" : SkillSelected = "533" : Type = 5 : AttackDistance = 12
            Case "Thunder Burst" : SkillSelected = "733" : Type = 5 : AttackDistance = 12
            Case "Inferno" : SkillSelected = "545" : Type = 5 : AttackDistance = 12
            Case "Blizzard" : SkillSelected = "645" : Type = 5 : AttackDistance = 12

            Case "Thundercloud" : SkillSelected = "745" : Type = 6 : AttackDistance = 12
            Case "Super Nova" : SkillSelected = "560" : Type = 6 : AttackDistance = 12
            Case "Frost Nova" : SkillSelected = "660" : Type = 6 : AttackDistance = 12
            Case "Static Nova" : SkillSelected = "760" : Type = 6 : AttackDistance = 12
            Case "Meteor Fall" : SkillSelected = "571" : Type = 6 : AttackDistance = 12
            Case "Ice Storm" : SkillSelected = "671" : Type = 6 : AttackDistance = 12
            Case "Chain Lightning" : SkillSelected = "771" : Type = 6 : AttackDistance = 12
            
        End Select

        If CoordinateDistance(GetX(), GetY(), GetTargetX(), GetTargetY()) > AttackDistance Then Return False
        If IsAttackableTarget() = False Then Return False

        If Attack Then
            SendAttackPacket()
            Thread.Sleep(150)
        End If

        Dim SkillID = AlignDWORD(Int32.Parse(GetClass().ToString() & SkillSelected)).Substring(0, 6).Substring(0, 6)

        Select Case Type
            Case 1
                SendPacket("3101" + SkillID + "00" + GetID() + GetTargetID() + "00000000000000000000000000000A00")
                SendPacket("3103" + SkillID + "00" + GetID() + GetTargetID() + "F020400A601000000000000")
                Return True
            Case 2
                SendPacket("3101" + SkillID + "00" + GetID() + GetTargetID() + "00000000000000000000000000000F00")
                SendPacket("3103" + SkillID + "00" + GetID() + GetTargetID() + "00000000000000000000000")
                Return True
            Case 3
                SendPacket("3101" + SkillID + "00" + GetID() + GetTargetID() + "00000000000000000000000000000F00")
                SendPacket("3102" & SkillID & "00" & GetID() & GetTargetID() & "000000000000000000000000")
                SendPacket("3103" & SkillID & "00" & GetID() & GetTargetID() & "0000000000000000000000000000000")
                SendPacket("3104" & SkillID & "00" & GetID() & GetTargetID() & AlignDWORD(GetTargetX()).Substring(0, 4) & AlignDWORD(GetTargetZ()).Substring(0, 4) & AlignDWORD(GetTargetY()).Substring(0, 4) & "9BFF00000000000000000")
                Return True
            Case 4
                SendPacket("3103" & SkillID & "00" & GetID() & GetTargetID() & "0000000000000100000000000000000")
                Return True
            Case 5
                SendPacket("3101" & SkillID & "00" & GetID() & "FFFF" & AlignDWORD(GetTargetX()).Substring(0, 4) & AlignDWORD(GetTargetZ()).Substring(0, 4) & AlignDWORD(GetTargetY()).Substring(0, 4) & "00000000000000000F00")
                SendPacket("3102" & SkillID & "00" & GetID() & "FFFF" & AlignDWORD(GetTargetX()).Substring(0, 4) & AlignDWORD(GetTargetZ()).Substring(0, 4) & AlignDWORD(GetTargetY()).Substring(0, 4) & "00000000000000000000")
                SendPacket("3103" & SkillID & "00" & GetID() & "FFFF" & AlignDWORD(GetTargetX()).Substring(0, 4) & AlignDWORD(GetTargetZ()).Substring(0, 4) & AlignDWORD(GetTargetY()).Substring(0, 4) & "00000000000000000000")
                SendPacket("3104" & SkillID & "00" & GetID() & "FFFF" & AlignDWORD(GetTargetX()).Substring(0, 4) & AlignDWORD(GetTargetZ()).Substring(0, 4) & AlignDWORD(GetTargetY()).Substring(0, 4) & "9BFF0000000000000000")
                Return True
            Case 6
                SendPacket("3101" & SkillID & "00" & GetID() & "FFFF" & AlignDWORD(GetTargetX()).Substring(0, 4) & AlignDWORD(GetTargetZ()).Substring(0, 4) & AlignDWORD(GetTargetY()).Substring(0, 4) & "00000000000000000F00")
                SendPacket("3103" & SkillID & "00" & GetID() & "FFFF" & AlignDWORD(GetTargetX()).Substring(0, 4) & AlignDWORD(GetTargetZ()).Substring(0, 4) & AlignDWORD(GetTargetY()).Substring(0, 4) & "000000000000")
                Return True
        End Select

        Return False
    End Function

    Public Function UsePriestSkill(ByVal Skill As String, ByVal Attack As Boolean) As Boolean
        Dim SkillSelected As String = ""
        Dim AttackDistance As Long = 3

        Select Case Skill
            Case "Stroke" : SkillSelected = "001" : AttackDistance = 12
            Case "Holy Attack" : SkillSelected = "006" : AttackDistance = 12
            Case "Wrath" : SkillSelected = "611" : AttackDistance = 12
            Case "Wield" : SkillSelected = "620" : AttackDistance = 12
            Case "Harsh" : SkillSelected = "641" : AttackDistance = 12
            Case "Collision" : SkillSelected = "511" : AttackDistance = 12
            Case "Collapse" : SkillSelected = "650" : AttackDistance = 12
            Case "Shuddering" : SkillSelected = "520" : AttackDistance = 12
            Case "Ruin" : SkillSelected = "542" : AttackDistance = 12
            Case "Hellish" : SkillSelected = "551" : AttackDistance = 12
            Case "Tilt" : SkillSelected = "712" : AttackDistance = 12
            Case "Bloody" : SkillSelected = "721" : AttackDistance = 12
            Case "Raving Edge" : SkillSelected = "739" : AttackDistance = 12
            Case "Hades" : SkillSelected = "750" : AttackDistance = 12
            Case "Judgement" : SkillSelected = "802" : AttackDistance = 12
            Case "Helis" : SkillSelected = "815" : AttackDistance = 12
        End Select

        If CoordinateDistance(GetX(), GetY(), GetTargetX(), GetTargetY()) > AttackDistance Then Return False
        If IsAttackableTarget() = False Then Return False

        If Attack Then
            SendAttackPacket()
            Thread.Sleep(150)
        End If

        SendPacket("3103" + AlignDWORD(Int32.Parse(GetClass().ToString() + SkillSelected)).Substring(0, 6) + "00" + GetID() + GetTargetID() + "000000000000000000000000")

        Return True
    End Function

    Public Function UseGain() As Boolean
        If IsRightSkillAffected(705) = False And GetMp() >= 10 Then
            SendPacket("3106" + AlignDWORD(GetClass().ToString() + "705").Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")
            SendPacket("3101" + AlignDWORD(GetClass().ToString() + "705").Substring(0, 6) + "00" + GetID() + GetID() + "0000000000000000000000001400")
            SendPacket("3103" + AlignDWORD(GetClass().ToString() + "705").Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseDefense() As Boolean
        If IsRightSkillAffected(7) = False And GetMp() >= 4 Then
            SendPacket("3106" + AlignDWORD(GetClass().ToString() + "007").Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")
            SendPacket("3103" + AlignDWORD(GetClass().ToString() + "007").Substring(0, 6) + "00" + GetID() + GetID() + "0000000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseSafety() As Boolean
        If IsRightSkillAffected(730) = False And GetMp() >= 80 And IsRightSkillAffected(710) = False Then
            SendPacket("3106" + AlignDWORD(GetClass().ToString() & "730").Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")
            SendPacket("3103" + AlignDWORD(GetClass().ToString() + "730").Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseEvade() As Boolean
        If IsRightSkillAffected(710) = False And GetMp() >= 40 And IsRightSkillAffected(730) = False Then
            SendPacket("3106" + AlignDWORD(GetClass().ToString() & "710").Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")
            SendPacket("3103" + AlignDWORD(GetClass().ToString() + "710").Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseHide() As Boolean
        If IsRightSkillAffected(700) = False And GetMp() >= 40 Then
            SendPacket("3106" + AlignDWORD(GetClass().ToString() & "700").Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")
            SendPacket("3101" + AlignDWORD(GetClass().ToString() + "700").Substring(0, 6) + "00" + GetID() + GetID() + "00000000000000000000000000000F00")
            SendPacket("3103" + AlignDWORD(GetClass().ToString() + "700").Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseStealth() As Boolean
        If IsRightSkillAffected(645) = False And GetMp() >= 270 Then
            SendPacket("3106" + AlignDWORD(GetClass().ToString() & "645").Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")
            SendPacket("3101" + AlignDWORD(GetClass().ToString() + "645").Substring(0, 6) + "00" + GetID() + GetID() + "00000000000000000000000000001E00")
            SendPacket("3103" + AlignDWORD(GetClass().ToString() + "645").Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseLightFeet() As Boolean
        If IsRightSkillAffected(725) = False And GetMp() >= 40 Then
            SendPacket("3106" + AlignDWORD(GetClass().ToString() + "725").Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")
            SendPacket("3103" + AlignDWORD(GetClass().ToString() + "725").Substring(0, 6) + "00" + GetID() + GetID() + "0000000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseSprint() As Boolean
        If IsRightSkillAffected(2) = False And GetMp() >= 5 Then
            SendPacket("3106" + AlignDWORD(GetClass().ToString() + "002").Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")
            SendPacket("3103" + AlignDWORD(GetClass().ToString() + "002").Substring(0, 6) + "00" + GetID() + GetID() + "0000000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseWolf() As Boolean
        If IsRightSkillAffected(30) = False And GetMp() >= 30 And IsItemExist("Wolf") Then
            SendPacket("3106" + AlignDWORD(GetClass().ToString() & "030").Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")
            SendPacket("3101" + AlignDWORD(GetClass().ToString() + "030").Substring(0, 6) & "00" + GetID() + "FFFF" + AlignDWORD(GetX()).Substring(0, 4) + "0000" + AlignDWORD(GetY()).Substring(0, 4) + "00000000000000001100")
            SendPacket("3103" + AlignDWORD(GetClass().ToString() + "030").Substring(0, 6) & "00" + GetID() + "FFFF" + AlignDWORD(GetX()).Substring(0, 4) + "0000" + AlignDWORD(GetY()).Substring(0, 4) + "000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseSwift(ByVal TargetID As Long, Optional ByVal Affected As Boolean = False) As Boolean

        If Affected = True And IsRightSkillAffected(10) = True Then Return False

        If GetMp() >= 15 Then

            If TargetID = GetLongID() Then
                SendPacket("3106" + AlignDWORD(GetClass().ToString() & "010").Substring(0, 6) + "00" + GetID() + AlignDWORD(TargetID).Substring(0, 4).ToUpper() + "000000000000000000000000")
            End If

            SendPacket("3101" + AlignDWORD(GetClass().ToString() + "010").Substring(0, 6) + "00" + GetID() + AlignDWORD(TargetID).Substring(0, 4).ToUpper() + "00000000000000000000000000000F00")
            SendPacket("3103" + AlignDWORD(GetClass().ToString() + "010").Substring(0, 6) + "00" + GetID() + AlignDWORD(TargetID).Substring(0, 4).ToUpper() + "000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseLupineEyes() As Boolean
        If IsRightSkillAffected(735) = False And GetMp() >= 15 Then
            SendPacket("3106" + AlignDWORD(GetClass().ToString() & "735").Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")
            SendPacket("3101" + AlignDWORD(GetClass().ToString() + "735").Substring(0, 6) + "00" + GetID() + GetID() + "00000000000000000000000000001400")
            SendPacket("3103" + AlignDWORD(GetClass().ToString() + "735").Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseMinorHealing(ByVal TargetID As Long) As Boolean
        If GetMp() >= 15 Then
            SendPacket("3103" + AlignDWORD(GetClass().ToString() + "705").Substring(0, 6) + "00" + GetID() + AlignDWORD(TargetID).Substring(0, 4).ToUpper() + "0000000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UsePrayerOfCronos() As Boolean
        If IsRightSkillAffected(12) = False And IsItemExist("Prayer Of Cronos") Then
            SendPacket("3106" + AlignDWORD(GetClass().ToString() & "012").Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")
            SendPacket("3103" + AlignDWORD(GetClass().ToString() + "012").Substring(0, 6) + "00" + GetID() + GetID() + "00000000000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UsePrayerOfGodPower() As Boolean
        If IsLeftSkillAffected(GetClass().ToString() & "020", 6) = False And IsItemExist("Prayer Of God's Power") Then
            SendPacket("3106" + AlignDWORD(GetClass().ToString() & "020").Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")
            SendPacket("3103" + AlignDWORD(GetClass().ToString() + "020").Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseBlasting() As Boolean
        If IsRightSkillAffected(529) = False And GetMp() >= 80 Then
            SendPacket("3106" + AlignDWORD(GetClass().ToString() & "529").Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")
            SendPacket("3103" + AlignDWORD(GetClass().ToString() + "529").Substring(0, 6) + "00" + GetID() + GetID() + "00000000000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseWildness() As Boolean
        If IsRightSkillAffected(629) = False And GetMp() >= 80 Then
            SendPacket("3106" + AlignDWORD(GetClass().ToString() & "629").Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")
            SendPacket("3103" + AlignDWORD(GetClass().ToString() + "629").Substring(0, 6) + "00" + GetID() + GetID() + "00000000000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseEruption() As Boolean
        If IsRightSkillAffected(729) = False And GetMp() >= 80 Then
            SendPacket("3106" + AlignDWORD(GetClass().ToString() & "729").Substring(0, 6) + "00" + GetID() + GetID() + "000000000000000000000000")
            SendPacket("3103" + AlignDWORD(GetClass().ToString() + "729").Substring(0, 6) + "00" + GetID() + GetID() + "00000000000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseCure(ByVal TargetID As Long) As Boolean
        If GetMp() >= 60 Then
            SendPacket("3101" + AlignDWORD(GetClass().ToString() + "525").Substring(0, 6) + "00" + GetID() + AlignDWORD(TargetID).Substring(0, 4).ToUpper() + "00000000000000000000000000000F00")
            SendPacket("3103" + AlignDWORD(GetClass().ToString() + "525").Substring(0, 6) + "00" + GetID() + AlignDWORD(TargetID).Substring(0, 4).ToUpper() + "000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function UseDisease(ByVal TargetID As Long) As Boolean
        If GetMp() >= 120 Then
            SendPacket("3101" + AlignDWORD(GetClass().ToString() + "535").Substring(0, 6) + "00" + GetID() + AlignDWORD(TargetID).Substring(0, 4).ToUpper() + "00000000000000000000000000000F00")
            SendPacket("3103" + AlignDWORD(GetClass().ToString() + "535").Substring(0, 6) + "00" + GetID() + AlignDWORD(TargetID).Substring(0, 4).ToUpper() + "000000000000000000000000")
            Return True
        End If
        Return False
    End Function

    Public Function CoordinateDistance(ChrkorX, ChrkorY, HedefX, HedefY) As Long
        CoordinateDistance = Math.Sqrt((HedefX - ChrkorX) ^ 2 + (HedefY - ChrkorY) ^ 2)
    End Function

#End Region

End Class