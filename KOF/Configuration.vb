Public Class Configuration

    Private _Client As Client

    Public Sub New(ByVal Client As Client)
        _Client = Client

        InitializeComponent()
    End Sub

    Private Sub Configuration_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Text = _Client.GetName() : TopMost = True

        Dim Control As Control = GetNextControl(Me, True)

        Do Until Control Is Nothing
            Control = GetNextControl(Control, True)

            If Control Is Nothing Then Continue Do
            If Control.GetType = GetType(ComboBox) Then
                Dim _ComboBox As ComboBox = TryCast(Control, ComboBox)
                _ComboBox.SelectedIndex = 0
            End If
        Loop

        _Client.LoadAllControl()
    End Sub

    Private Sub Application_Closing(ByVal sender As System.Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        Me.Visible = False : e.Cancel = True

        _Client.SaveAllControl()
    End Sub

    Private Sub Protect_Hp_CheckedChanged(sender As Object, e As EventArgs) Handles Hp.CheckedChanged
        _Client.SetControl(Hp.Name, Hp.Checked)
    End Sub

    Private Sub Protect_Hp_Percent_ValueChanged(sender As Object, e As EventArgs) Handles HpPercent.ValueChanged
        _Client.SetControl(HpPercent.Name, HpPercent.Value)
    End Sub

    Private Sub Protect_Hp_Potion_SelectedIndexChanged(sender As Object, e As EventArgs) Handles HpPotion.SelectedIndexChanged
        _Client.SetControl(HpPotion.Name, HpPotion.SelectedItem)
    End Sub

    Private Sub Protect_Mp_CheckedChanged(sender As Object, e As EventArgs) Handles Mp.CheckedChanged
        _Client.SetControl(Mp.Name, Mp.Checked)
    End Sub

    Private Sub Protect_Mp_Percent_ValueChanged(sender As Object, e As EventArgs) Handles MpPercent.ValueChanged
        _Client.SetControl(MpPercent.Name, MpPercent.Value)
    End Sub

    Private Sub Protect_Mp_Potion_SelectedIndexChanged(sender As Object, e As EventArgs) Handles MpPotion.SelectedIndexChanged
        _Client.SetControl(MpPotion.Name, MpPotion.SelectedItem)
    End Sub

    Private Sub CheckBox3_CheckedChanged(sender As Object, e As EventArgs) Handles Minor.CheckedChanged
        _Client.SetControl(Minor.Name, Minor.Checked)
    End Sub

    Private Sub NumericUpDown3_ValueChanged(sender As Object, e As EventArgs) Handles MinorPercent.ValueChanged
        _Client.SetControl(MinorPercent.Name, MinorPercent.Value)
    End Sub

    Private Sub CheckBox4_CheckedChanged(sender As Object, e As EventArgs) Handles Wallhack.CheckedChanged
        _Client.SetControl(Wallhack.Name, Wallhack.Checked)
    End Sub

    Private Sub CheckBox5_CheckedChanged(sender As Object, e As EventArgs) Handles AutoLoot.CheckedChanged
        _Client.SetControl(AutoLoot.Name, AutoLoot.Checked)
    End Sub

    Private Sub CheckBox6_CheckedChanged(sender As Object, e As EventArgs) Handles Oreads.CheckedChanged
        _Client.SetControl(Oreads.Name, Oreads.Checked)
    End Sub

    Private Sub CheckedListBox1_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles ActiveSkillList.ItemCheck
        If e.NewValue = 1 Then
            _Client.SetActiveSkill(e.Index, 1)
        Else
            _Client.DeleteActiveSkill(e.Index, 1)
        End If
    End Sub

    Private Sub CheckedListBox2_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles TimedSkillList.ItemCheck
        If e.NewValue = 1 Then
            _Client.SetActiveSkill(e.Index, 2)
        Else
            _Client.DeleteActiveSkill(e.Index, 2)
        End If
    End Sub

    Private Sub CheckedListBox4_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles DeBuffSkillList.ItemCheck
        If e.NewValue = 1 Then
            _Client.SetActiveSkill(e.Index, 3)
        Else
            _Client.DeleteActiveSkill(e.Index, 3)
        End If
    End Sub

    Private Sub CheckBox7_CheckedChanged(sender As Object, e As EventArgs) Handles Attack.CheckedChanged
        _Client.SetControl(Attack.Name, Attack.Checked)
    End Sub

    Private Sub CheckBox8_CheckedChanged(sender As Object, e As EventArgs) Handles AttackDirect.CheckedChanged
        _Client.SetControl(AttackDirect.Name, AttackDirect.Checked)
    End Sub

    Private Sub CheckBox9_CheckedChanged(sender As Object, e As EventArgs) Handles Target.CheckedChanged
        _Client.SetControl(Target.Name, Target.Checked)
    End Sub

    Private Sub CheckBox10_CheckedChanged(sender As Object, e As EventArgs) Handles ActionMove.CheckedChanged
        _Client.SetControl(ActionMove.Name, ActionMove.Checked)
    End Sub

    Private Sub CheckBox11_CheckedChanged(sender As Object, e As EventArgs) Handles ActionSetCoordinate.CheckedChanged
        _Client.SetControl(ActionSetCoordinate.Name, ActionSetCoordinate.Checked)
    End Sub

    Private Sub CheckBox13_CheckedChanged(sender As Object, e As EventArgs) Handles DeathOnBorn.CheckedChanged
        _Client.SetControl(DeathOnBorn.Name, DeathOnBorn.Checked)
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        AreaControlX.Value = _Client.GetX()
        AreaControlY.Value = _Client.GetY()

        _Client.SetControl(AreaControlX.Name, AreaControlX.Value)
        _Client.SetControl(AreaControlY.Name, AreaControlY.Value)

        AreaControl.Checked = True
    End Sub

    Private Sub CheckBox14_CheckedChanged(sender As Object, e As EventArgs) Handles AreaControl.CheckedChanged
        _Client.SetControl(AreaControl.Name, AreaControl.Checked)
    End Sub

    Private Sub CheckedListBox3_SelectedIndexChanged(sender As Object, e As ItemCheckEventArgs) Handles CheckedListBox3.ItemCheck
        _Client.SetMobListState(CheckedListBox3.Items(e.Index), e.NewValue)
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim MobList = _Client.UpdateMobList()
        If MobList.Count = 0 Then Return
        For Each Mob As KeyValuePair(Of String, Boolean) In MobList
            If CheckedListBox3.Items.Contains(Mob.Key) Then Continue For
            CheckedListBox3.Items.Add(Mob.Key)
        Next
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        For ListBoxIndex As Int32 = 0 To CheckedListBox3.Items.Count - 1
            _Client.SetMobListState(CheckedListBox3.Items(ListBoxIndex), True)
            CheckedListBox3.SetItemChecked(ListBoxIndex, True)
        Next
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        For ListBoxIndex As Int32 = 0 To CheckedListBox3.Items.Count - 1
            _Client.SetMobListState(CheckedListBox3.Items(ListBoxIndex), False)
            CheckedListBox3.SetItemChecked(ListBoxIndex, False)
        Next
    End Sub

    Private Sub CheckBox17_CheckedChanged(sender As Object, e As EventArgs) Handles PartyBuff.CheckedChanged
        _Client.SetControl(PartyBuff.Name, PartyBuff.Checked)
    End Sub

    Private Sub ComboBox4_SelectedIndexChanged(sender As Object, e As EventArgs) Handles PartyBuffSkill.SelectedIndexChanged
        _Client.SetControl(PartyBuffSkill.Name, PartyBuffSkill.SelectedItem)
    End Sub

    Private Sub CheckBox18_CheckedChanged(sender As Object, e As EventArgs) Handles PartyAc.CheckedChanged
        _Client.SetControl(PartyAc.Name, PartyAc.Checked)
    End Sub

    Private Sub ComboBox5_SelectedIndexChanged(sender As Object, e As EventArgs) Handles PartyAcSkill.SelectedIndexChanged
        _Client.SetControl(PartyAcSkill.Name, PartyAcSkill.SelectedItem)
    End Sub

    Private Sub CheckBox19_CheckedChanged(sender As Object, e As EventArgs) Handles PartyMind.CheckedChanged
        _Client.SetControl(PartyMind.Name, PartyMind.Checked)
    End Sub

    Private Sub ComboBox6_SelectedIndexChanged(sender As Object, e As EventArgs) Handles PartyMindSkill.SelectedIndexChanged
        _Client.SetControl(PartyMindSkill.Name, PartyMindSkill.SelectedItem)
    End Sub

    Private Sub CheckBox20_CheckedChanged(sender As Object, e As EventArgs) Handles PartyCure.CheckedChanged
        _Client.SetControl(PartyCure.Name, PartyCure.Checked)
    End Sub

    Private Sub CheckBox21_CheckedChanged(sender As Object, e As EventArgs) Handles PartyStrength.CheckedChanged
        _Client.SetControl(PartyStrength.Name, PartyStrength.Checked)
    End Sub

    Private Sub CheckBox15_CheckedChanged(sender As Object, e As EventArgs) Handles PartyHeal.CheckedChanged
        _Client.SetControl(PartyHeal.Name, PartyHeal.Checked)
    End Sub
    Private Sub NumericUpDown4_ValueChanged(sender As Object, e As EventArgs) Handles PartyHealPercent.ValueChanged
        _Client.SetControl(PartyHealPercent.Name, PartyHealPercent.Value)
    End Sub
    Private Sub ComboBox3_SelectedIndexChanged(sender As Object, e As EventArgs) Handles PartyHealSkill.SelectedIndexChanged
        _Client.SetControl(PartyHealSkill.Name, PartyHealSkill.SelectedItem)
    End Sub

    Private Sub CheckBox16_CheckedChanged(sender As Object, e As EventArgs)
        _Client.SetControl(PartyHeal.Name, PartyHeal.Checked)
    End Sub

    Private Sub CheckBox22_CheckedChanged(sender As Object, e As EventArgs) Handles PartyMinor.CheckedChanged
        _Client.SetControl(PartyMinor.Name, PartyMinor.Checked)
    End Sub

    Private Sub NumericUpDown7_ValueChanged(sender As Object, e As EventArgs) Handles PartyMinorPercent.ValueChanged
        _Client.SetControl(PartyMinorPercent.Name, PartyMinorPercent.Value)
    End Sub

    Private Sub CheckBox28_CheckedChanged(sender As Object, e As EventArgs) Handles Transformation.CheckedChanged
        If Transformation.Checked Then
            AttackScroll.Checked = False
            AttackScroll.Enabled = False
        Else
            AttackScroll.Enabled = True
        End If

        _Client.SetControl(Transformation.Name, Transformation.Checked)
    End Sub

    Private Sub CheckBox29_CheckedChanged(sender As Object, e As EventArgs) Handles RepairSunderies.CheckedChanged
        If RepairSunderies.Checked Then
            RepairMagicHammer.Checked = False
            RepairMagicHammer.Enabled = False
        Else
            RepairMagicHammer.Enabled = True
        End If

        _Client.SetControl(RepairSunderies.Name, RepairSunderies.Checked)
    End Sub

    Private Sub CheckBox30_CheckedChanged(sender As Object, e As EventArgs) Handles RepairMagicHammer.CheckedChanged
        If RepairMagicHammer.Checked Then
            RepairSunderies.Checked = False
            RepairSunderies.Enabled = False
        Else
            RepairSunderies.Enabled = True
        End If

        _Client.SetControl(RepairMagicHammer.Name, RepairMagicHammer.Checked)
    End Sub

    Private Sub CheckBox31_CheckedChanged(sender As Object, e As EventArgs) Handles AreaHeal.CheckedChanged
        _Client.SetControl(AreaHeal.Name, AreaHeal.Checked)
    End Sub

    Private Sub CheckBox32_CheckedChanged(sender As Object, e As EventArgs) Handles StatScroll.CheckedChanged
        _Client.SetControl(StatScroll.Name, StatScroll.Checked)
    End Sub

    Private Sub CheckBox33_CheckedChanged(sender As Object, e As EventArgs) Handles AttackScroll.CheckedChanged
        If AttackScroll.Checked Then
            Transformation.Checked = False
            Transformation.Enabled = False
        Else
            Transformation.Enabled = True
        End If

        _Client.SetControl(AttackScroll.Name, AttackScroll.Checked)
    End Sub

    Private Sub CheckBox34_CheckedChanged(sender As Object, e As EventArgs) Handles AcScroll.CheckedChanged
        _Client.SetControl(AcScroll.Name, AcScroll.Checked)
    End Sub

    Private Sub CheckBox35_CheckedChanged(sender As Object, e As EventArgs) Handles DropScroll.CheckedChanged
        _Client.SetControl(DropScroll.Name, DropScroll.Checked)
    End Sub

    Private Sub ComboBox7_SelectedIndexChanged(sender As Object, e As EventArgs) Handles TransformationName.SelectedIndexChanged
        _Client.SetControl(TransformationName.Name, TransformationName.SelectedItem)
    End Sub

    Private Sub CheckBox36_CheckedChanged(sender As Object, e As EventArgs) Handles SupplyHpPotion.CheckedChanged
        _Client.SetControl(SupplyHpPotion.Name, SupplyHpPotion.Checked)
    End Sub

    Private Sub CheckBox37_CheckedChanged(sender As Object, e As EventArgs) Handles SupplyMpPotion.CheckedChanged
        _Client.SetControl(SupplyMpPotion.Name, SupplyMpPotion.Checked)
    End Sub

    Private Sub CheckBox38_CheckedChanged(sender As Object, e As EventArgs) Handles SupplyArrow.CheckedChanged
        _Client.SetControl(SupplyArrow.Name, SupplyArrow.Checked)
    End Sub

    Private Sub CheckBox39_CheckedChanged(sender As Object, e As EventArgs) Handles SupplyWolf.CheckedChanged
        _Client.SetControl(SupplyWolf.Name, SupplyWolf.Checked)
    End Sub

    Private Sub CheckBox40_CheckedChanged(sender As Object, e As EventArgs) Handles SupplyTsGem.CheckedChanged
        _Client.SetControl(SupplyTsGem.Name, SupplyTsGem.Checked)
    End Sub

    Private Sub NumericUpDown8_ValueChanged(sender As Object, e As EventArgs) Handles SupplyHpPotionCount.ValueChanged
        _Client.SetControl(SupplyHpPotionCount.Name, SupplyHpPotionCount.Value)
    End Sub

    Private Sub NumericUpDown9_ValueChanged(sender As Object, e As EventArgs) Handles SupplyMpPotionCount.ValueChanged
        _Client.SetControl(SupplyMpPotionCount.Name, SupplyMpPotionCount.Value)
    End Sub

    Private Sub NumericUpDown10_ValueChanged(sender As Object, e As EventArgs) Handles SupplyArrowCount.ValueChanged
        _Client.SetControl(SupplyArrowCount.Name, SupplyArrowCount.Value)
    End Sub

    Private Sub NumericUpDown11_ValueChanged(sender As Object, e As EventArgs) Handles SupplyWolfCount.ValueChanged
        _Client.SetControl(SupplyWolfCount.Name, SupplyWolfCount.Value)
    End Sub

    Private Sub NumericUpDown12_ValueChanged(sender As Object, e As EventArgs) Handles SupplyTsGemCount.ValueChanged
        _Client.SetControl(SupplyTsGemCount.Name, SupplyTsGemCount.Value)
    End Sub

    Private Sub ComboBox8_SelectedIndexChanged(sender As Object, e As EventArgs) Handles SupplyHpPotionItem.SelectedIndexChanged
        _Client.SetControl(SupplyHpPotionItem.Name, SupplyHpPotionItem.SelectedItem)
    End Sub

    Private Sub ComboBox9_SelectedIndexChanged(sender As Object, e As EventArgs) Handles SupplyMpPotionItem.SelectedIndexChanged
        _Client.SetControl(SupplyMpPotionItem.Name, SupplyMpPotionItem.SelectedItem)
    End Sub

    Private Sub CheckBox23_CheckedChanged(sender As Object, e As EventArgs) Handles SupplyBook.CheckedChanged
        _Client.SetControl(SupplyBook.Name, SupplyBook.Checked)
    End Sub

    Private Sub NumericUpDown13_ValueChanged(sender As Object, e As EventArgs) Handles SupplyBookCount.ValueChanged
        _Client.SetControl(SupplyBookCount.Name, SupplyBookCount.Value)
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        _Client.StartUpgradeEvent()
    End Sub

    Private Sub CheckBox24_CheckedChanged(sender As Object, e As EventArgs) Handles SpeedHack.CheckedChanged
        _Client.SetControl(SpeedHack.Name, SpeedHack.Checked)
    End Sub

    Private Sub CheckBox25_CheckedChanged(sender As Object, e As EventArgs) Handles SupplyMasterStone.CheckedChanged
        _Client.SetControl(SupplyMasterStone.Name, SupplyMasterStone.Checked)
    End Sub

    Private Sub NumericUpDown14_ValueChanged(sender As Object, e As EventArgs) Handles SupplyMasterStoneCount.ValueChanged
        _Client.SetControl(SupplyMasterStoneCount.Name, SupplyMasterStoneCount.Value)
    End Sub

    Private Sub NumericUpDown16_ValueChanged(sender As Object, e As EventArgs) Handles TargetRange.ValueChanged
        _Client.SetControl(TargetRange.Name, TargetRange.Value)
    End Sub

    Private Sub NumericUpDown15_ValueChanged(sender As Object, e As EventArgs) Handles AttackSpeed.ValueChanged
        _Client.SetControl(AttackSpeed.Name, AttackSpeed.Value)
    End Sub

    Private Sub CheckBox26_CheckedChanged(sender As Object, e As EventArgs) Handles FollowDisable.CheckedChanged
        _Client.SetControl(FollowDisable.Checked, FollowDisable.Checked)
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        If ToolCoordinateX.Text <> 0 And ToolCoordinateY.Text <> 0 Then
            _Client.Move(ToolCoordinateX.Text, ToolCoordinateY.Text)
        End If
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        If ToolCoordinateX.Text <> 0 And ToolCoordinateY.Text <> 0 Then
            _Client.SetCoordinate(ToolCoordinateX.Text, ToolCoordinateY.Text)
        End If
    End Sub

    Private Sub NumericUpDown17_ValueChanged(sender As Object, e As EventArgs) Handles BlackMarketerLoop.ValueChanged
        _Client.SetControl(BlackMarketerLoop.Name, BlackMarketerLoop.Value)
    End Sub

    Private Sub NumericUpDown18_ValueChanged(sender As Object, e As EventArgs) Handles BlackMarketerEventTime.ValueChanged
        _Client.SetControl(BlackMarketerEventTime.Name, BlackMarketerEventTime.Value)
    End Sub

    Private Sub Button10_Click(sender As Object, e As EventArgs) Handles Button10.Click
        ToolCoordinateX.Text = _Client.GetX()
        ToolCoordinateY.Text = _Client.GetY()
    End Sub

    Private Sub Button12_Click(sender As Object, e As EventArgs) Handles Button12.Click
        _Client.SendPacket(Packet.Text)
    End Sub

    Private Sub CheckBox12_CheckedChanged(sender As Object, e As EventArgs) Handles SupplyInnHpPotion.CheckedChanged
        _Client.SetControl(SupplyInnHpPotion.Name, SupplyInnHpPotion.Checked)
    End Sub

    Private Sub CheckBox46_CheckedChanged(sender As Object, e As EventArgs) Handles SupplyInnMpPotion.CheckedChanged
        _Client.SetControl(SupplyInnMpPotion.Name, SupplyInnMpPotion.Checked)
    End Sub

    Private Sub NumericUpDown20_ValueChanged(sender As Object, e As EventArgs) Handles SupplyInnHpPotionCount.ValueChanged
        _Client.SetControl(SupplyInnHpPotionCount.Name, SupplyInnHpPotionCount.Value)
    End Sub

    Private Sub NumericUpDown21_ValueChanged(sender As Object, e As EventArgs) Handles SupplyInnMpPotionCount.ValueChanged
        _Client.SetControl(SupplyInnMpPotionCount.Name, SupplyInnMpPotionCount.Value)
    End Sub

    Private Sub ComboBox11_SelectedIndexChanged(sender As Object, e As EventArgs) Handles SupplyInnHpPotionItem.SelectedIndexChanged
        _Client.SetControl(SupplyInnHpPotionItem.Name, SupplyInnHpPotionItem.SelectedItem)
    End Sub

    Private Sub ComboBox12_SelectedIndexChanged(sender As Object, e As EventArgs) Handles SupplyInnMpPotionItem.SelectedIndexChanged
        _Client.SetControl(SupplyInnMpPotionItem.Name, SupplyInnMpPotionItem.SelectedItem)
    End Sub

    Private Sub Black_Marketer_CheckedChanged(sender As Object, e As EventArgs) Handles BlackMarketer.CheckedChanged
        _Client.SetControl(BlackMarketer.Name, BlackMarketer.Checked)
    End Sub

    Private Sub Area_Control_X_ValueChanged(sender As Object, e As EventArgs) Handles AreaControlX.ValueChanged
        _Client.SetControl(AreaControlX.Name, AreaControlX.Value)
    End Sub

    Private Sub Area_Control_Y_ValueChanged(sender As Object, e As EventArgs) Handles AreaControlY.ValueChanged
        _Client.SetControl(AreaControlY.Name, AreaControlY.Value)
    End Sub
End Class