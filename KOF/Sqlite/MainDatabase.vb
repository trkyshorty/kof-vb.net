Imports System.Data.SQLite

Public Class MainDatabase

#Region "Entry Point"
    Private _Connection As SQLiteConnection


    Public Sub New(Connection As SQLiteConnection)
        _Connection = Connection
    End Sub

    Public Sub Initialize()
        Dim SqliteCommand As SQLiteCommand = _Connection.CreateCommand()

        SqliteCommand.CommandTimeout = 60
        SqliteCommand.CommandText = "
            CREATE TABLE IF NOT EXISTS [account] 
            (
                [user]          TEXT NOT NULL,
                [hash]          TEXT NOT NULL,
                [path]          TEXT NOT NULL,
                PRIMARY KEY (user)
            );

            CREATE TABLE IF NOT EXISTS [supply_location] 
            (
                [id]            TEXT NOT NULL,
                [type]          TEXT NOT NULL,
                [zone]          TEXT NOT NULL,
                [nation]        TEXT NOT NULL,
                [x]             TEXT NOT NULL,
                [y]             TEXT NOT NULL,
                PRIMARY KEY (id)
            );

            CREATE TABLE IF NOT EXISTS [skill] 
            (
                [id]            TEXT NOT NULL,
                [job]           TEXT NOT NULL,
                [name]          TEXT NOT NULL,
                [cooldown]      TEXT NOT NULL,
                [type]          TEXT NOT NULL
            );
        "
        SqliteCommand.ExecuteNonQuery()

        InitializeTable()
        InitializeSupplyLocation()
        InitializeRogueSkill()
        InitializePriestSkill()
        InitializeWarriorSkill()
        InitializeMageSkill()
    End Sub
#End Region

#Region "Skill Table"
    Public Function GetAllSkill() As List(Of Tuple(Of String, String, String, String, String))
        Dim SqliteCommand As SQLiteCommand = _Connection.CreateCommand()
        SqliteCommand.CommandTimeout = 60
        SqliteCommand.CommandText = "SELECT * FROM skill;"
        Dim SqliteDataReader As SQLiteDataReader = SqliteCommand.ExecuteReader()
        Dim AccountList As New List(Of Tuple(Of String, String, String, String, String))()
        While (SqliteDataReader.Read())
            AccountList.Add(New Tuple(Of String, String, String, String, String)(SqliteDataReader.GetString(0), SqliteDataReader.GetString(1),
                                                                         SqliteDataReader.GetString(2), SqliteDataReader.GetString(3),
                                                                         SqliteDataReader.GetString(4)))
        End While
        GetAllSkill = AccountList
    End Function

    Public Sub SetSkill(ByVal Id As String, ByVal Job As String, ByVal Name As String, ByVal Cooldown As String, ByVal Type As String)
        Dim SqliteCommand As SQLiteCommand = _Connection.CreateCommand()
        SqliteCommand.CommandTimeout = 60
        SqliteCommand.Parameters.AddWithValue("@id", Id)
        SqliteCommand.Parameters.AddWithValue("@job", Job)
        SqliteCommand.Parameters.AddWithValue("@name", Name)
        SqliteCommand.Parameters.AddWithValue("@cooldown", Cooldown)
        SqliteCommand.Parameters.AddWithValue("@type", Type)
        If GetSkill(Name, Job) Is Nothing Then
            SqliteCommand.CommandText = "INSERT INTO skill (id, job, name, cooldown, type) VALUES (@id, @job, @name, @cooldown, @type);"
        Else
            SqliteCommand.CommandText = "UPDATE skill SET id = @id, job = @job, cooldown = @cooldown, type = @type WHERE name = @name AND job = @job;"
        End If
        SqliteCommand.ExecuteNonQuery()
    End Sub

    Public Function GetSkill(ByVal Name As String, ByVal Job As String) As Tuple(Of String, String, String, String, String)
        Dim SqliteCommand As SQLiteCommand = _Connection.CreateCommand()
        SqliteCommand.CommandTimeout = 60
        SqliteCommand.Parameters.AddWithValue("@name", Name)
        SqliteCommand.Parameters.AddWithValue("@job", Job)
        SqliteCommand.CommandText = "SELECT * FROM skill WHERE name = @name AND job = @job LIMIT 1;"
        Dim SqliteDataReader As SQLiteDataReader = SqliteCommand.ExecuteReader()
        While (SqliteDataReader.Read())
            Return New Tuple(Of String, String, String, String, String)(SqliteDataReader.GetString(0), SqliteDataReader.GetString(1),
                                                                        SqliteDataReader.GetString(2), SqliteDataReader.GetString(3),
                                                                        SqliteDataReader.GetString(4))
        End While
        GetSkill = Nothing
    End Function
#End Region

#Region "Supply Table"
    Public Function GetAllSupplyLocation() As List(Of Tuple(Of String, String, String, String, String, String))
        Dim SqliteCommand As SQLiteCommand = _Connection.CreateCommand()
        SqliteCommand.CommandTimeout = 60
        SqliteCommand.CommandText = "SELECT * FROM supply_location;"
        Dim SqliteDataReader As SQLiteDataReader = SqliteCommand.ExecuteReader()
        Dim LocationList As New List(Of Tuple(Of String, String, String, String, String, String))()
        While (SqliteDataReader.Read())
            LocationList.Add(New Tuple(Of String, String, String, String, String, String)(SqliteDataReader.GetString(0), SqliteDataReader.GetString(1),
                                                                                                         SqliteDataReader.GetString(2), SqliteDataReader.GetString(3),
                                                                                                         SqliteDataReader.GetString(4), SqliteDataReader.GetString(5)))
        End While
        GetAllSupplyLocation = LocationList
    End Function

    Public Sub SetSupplyLocation(ByVal Id As String, ByVal Type As String, ByVal Zone As String, ByVal Nation As String, ByVal X As String, ByVal Y As String)
        Dim SqliteCommand As SQLiteCommand = _Connection.CreateCommand()
        SqliteCommand.CommandTimeout = 60
        SqliteCommand.Parameters.AddWithValue("@id", Id)
        SqliteCommand.Parameters.AddWithValue("@type", Type)
        SqliteCommand.Parameters.AddWithValue("@zone", Zone)
        SqliteCommand.Parameters.AddWithValue("@nation", Nation)
        SqliteCommand.Parameters.AddWithValue("@x", X)
        SqliteCommand.Parameters.AddWithValue("@y", Y)
        If GetSupplyLocation(Id) Is Nothing Then
            SqliteCommand.CommandText = "INSERT INTO supply_location (id, type, zone, nation, x, y) VALUES (@id, @type, @zone, @nation, @x, @y);"
        Else
            SqliteCommand.CommandText = "UPDATE supply_location SET type = @type, zone = @zone, nation = @nation, x = @x, y = @y  WHERE id = @id;"
        End If
        SqliteCommand.ExecuteNonQuery()
    End Sub

    Public Function GetSupplyLocation(ByVal Id As String) As Tuple(Of String, String, String, String, String, String)
        Dim SqliteCommand As SQLiteCommand = _Connection.CreateCommand()
        SqliteCommand.CommandTimeout = 60
        SqliteCommand.Parameters.AddWithValue("@id", Id)
        SqliteCommand.CommandText = "SELECT * FROM supply_location WHERE id = @id LIMIT 1;"
        Dim SqliteDataReader As SQLiteDataReader = SqliteCommand.ExecuteReader()
        While (SqliteDataReader.Read())
            Return New Tuple(Of String, String, String, String, String, String)(SqliteDataReader.GetString(0), SqliteDataReader.GetString(1),
                                                                        SqliteDataReader.GetString(2), SqliteDataReader.GetString(3),
                                                                        SqliteDataReader.GetString(4), SqliteDataReader.GetString(5))
        End While
        GetSupplyLocation = Nothing
    End Function
#End Region

#Region "Account Table"
    Public Function GetAllAccount() As Dictionary(Of String, Tuple(Of String, String, String))
        Dim SqliteCommand As SQLiteCommand = _Connection.CreateCommand()
        SqliteCommand.CommandTimeout = 60
        SqliteCommand.CommandText = "SELECT * FROM account;"
        Dim SqliteDataReader As SQLiteDataReader = SqliteCommand.ExecuteReader()
        Dim AccountList As New Dictionary(Of String, Tuple(Of String, String, String))()
        While (SqliteDataReader.Read())
            AccountList.Add(SqliteDataReader.GetString(0), New Tuple(Of String, String, String)(SqliteDataReader.GetString(0), SqliteDataReader.GetString(1), SqliteDataReader.GetString(2)))
        End While
        GetAllAccount = AccountList
    End Function

    Public Sub SetAccount(ByVal User As String, ByVal Hash As String, ByVal Path As String)
        Dim SqliteCommand As SQLiteCommand = _Connection.CreateCommand()
        SqliteCommand.CommandTimeout = 60
        SqliteCommand.Parameters.AddWithValue("@user", User)
        SqliteCommand.Parameters.AddWithValue("@hash", Hash)
        SqliteCommand.Parameters.AddWithValue("@path", Path)
        If GetAccount(User) Is Nothing Then
            SqliteCommand.CommandText = "INSERT INTO account (user, hash, path) VALUES (@user, @hash, @path);"
        Else
            SqliteCommand.CommandText = "UPDATE account SET hash = @hash WHERE user = @user AND hash = @hash;"
        End If
        SqliteCommand.ExecuteNonQuery()
    End Sub

    Public Function GetAccount(ByVal User As String) As Tuple(Of String, String, String)
        Dim SqliteCommand As SQLiteCommand = _Connection.CreateCommand()
        SqliteCommand.CommandTimeout = 60
        SqliteCommand.Parameters.AddWithValue("@user", User)
        SqliteCommand.CommandText = "SELECT * FROM account WHERE user = @user LIMIT 1;"
        Dim SqliteDataReader As SQLiteDataReader = SqliteCommand.ExecuteReader()
        While (SqliteDataReader.Read())
            Return New Tuple(Of String, String, String)(SqliteDataReader.GetString(0), SqliteDataReader.GetString(1), SqliteDataReader.GetString(2))
        End While
        GetAccount = Nothing
    End Function

#End Region

#Region "Initialize Main Data"
    Public Sub InitializeTable()
        SetSupplyLocation(12931, 1, 2, 2, 1601, 394)
        SetSupplyLocation(12939, 2, 2, 2, 1618, 366)
        SetSupplyLocation(12926, 3, 2, 2, 1687, 370)

        SetSupplyLocation(13736, 1, 12, 2, 539, 562)
        SetSupplyLocation(13894, 2, 12, 2, 499, 563)
        SetSupplyLocation(13906, 3, 12, 2, 508, 575)

        SetSupplyLocation(14381, 1, 21, 0, 753, 554)
        SetSupplyLocation(14383, 2, 21, 0, 760, 524)
        SetSupplyLocation(14395, 3, 21, 0, 762, 648)

        SetSupplyLocation(17781, 2, 71, 2, 631, 898)
        SetSupplyLocation(17780, 3, 71, 2, 607, 926)

    End Sub
    Public Sub InitializeSupplyLocation()
        SetSupplyLocation(12931, 1, 2, 2, 1601, 394)
        SetSupplyLocation(12928, 2, 2, 2, 1629, 418)
        SetSupplyLocation(12926, 3, 2, 2, 1687, 370)

        SetSupplyLocation(13736, 1, 12, 2, 539, 562)
        SetSupplyLocation(13894, 2, 12, 2, 499, 563)
        SetSupplyLocation(13906, 3, 12, 2, 508, 575)

        SetSupplyLocation(14381, 1, 21, 0, 753, 554)
        SetSupplyLocation(14383, 2, 21, 0, 760, 524)
        SetSupplyLocation(14395, 3, 21, 0, 762, 648)

        SetSupplyLocation(17781, 2, 71, 2, 631, 898)
        SetSupplyLocation(17780, 3, 71, 2, 607, 926)
    End Sub

    Public Sub InitializeRogueSkill()
        SetSkill("005", "Rogue", "Stab", 5, 1)
        SetSkill("006", "Rogue", "Stab2", 5, 1)
        SetSkill("600", "Rogue", "Jab", 5, 1)
        SetSkill("610", "Rogue", "Blood Drain", 60, 1)
        SetSkill("615", "Rogue", "Pierce", 11, 1)
        SetSkill("620", "Rogue", "Shock", 6, 1)
        SetSkill("635", "Rogue", "Thrust", 11, 1)
        SetSkill("640", "Rogue", "Cut", 6, 1)
        SetSkill("650", "Rogue", "Vampiric Touch", 63, 1)
        SetSkill("655", "Rogue", "Spike", 12, 1)
        SetSkill("670", "Rogue", "Blody Beast", 6, 1)
        SetSkill("675", "Rogue", "Blinding", 60, 1)

        SetSkill("003", "Rogue", "Archery", 0, 1)
        SetSkill("500", "Rogue", "Through Shot", 0, 1)
        SetSkill("505", "Rogue", "Fire Arrow", 3, 1)
        SetSkill("510", "Rogue", "Poison Arrow", 3, 1)
        SetSkill("515", "Rogue", "Multiple Shot", 1, 1)
        SetSkill("520", "Rogue", "Guided Arrow", 0, 1)
        SetSkill("525", "Rogue", "Perfect Shot", 0, 1)
        SetSkill("530", "Rogue", "Fire Shot", 4, 1)
        SetSkill("535", "Rogue", "Poison Shot", 4, 1)
        SetSkill("540", "Rogue", "Arc Shot", 0, 1)
        SetSkill("545", "Rogue", "Explosive Shot", 4, 1)
        SetSkill("550", "Rogue", "Viper", 0, 1)
        SetSkill("552", "Rogue", "Counter Strike", 60, 1)
        SetSkill("555", "Rogue", "Arrow Shower", 1, 1)
        SetSkill("557", "Rogue", "Shadow Shot", 0, 1)
        SetSkill("560", "Rogue", "Shadow Hunter", 0, 1)
        SetSkill("562", "Rogue", "Ice Shot", 6, 1)
        SetSkill("566", "Rogue", "Lightning Shot", 6, 1)
        SetSkill("570", "Rogue", "Dark Pursuer", 0, 1)
        SetSkill("572", "Rogue", "Blow Arrow", 0, 1)
        SetSkill("580", "Rogue", "Blinding Strafe", 60, 1)
        SetSkill("1000", "Rogue", "Süper Archer", 0, 1)

        SetSkill("002", "Rogue", "Sprint", 0, 2)
        SetSkill("010", "Rogue", "Swift", 0, 2)
        SetSkill("030", "Rogue", "Wolf", 0, 2)
        SetSkill("645", "Rogue", "Stealth", 80, 2)
        SetSkill("700", "Rogue", "Hide", 40, 2)
        SetSkill("710", "Rogue", "Evade", 30, 2)
        SetSkill("725", "Rogue", "Light Feet", 10, 2)
        SetSkill("730", "Rogue", "Safety", 35, 2)
        SetSkill("735", "Rogue", "Lupine Eyes", 10, 2)
    End Sub

    Public Sub InitializePriestSkill()
        SetSkill("001", "Priest", "Stroke", 0, 1)
        SetSkill("006", "Priest", "Holy Attack", 0, 1)
        SetSkill("511", "Priest", "Collision", 0, 1)
        SetSkill("520", "Priest", "Shuddering", 6, 1)
        SetSkill("542", "Priest", "Ruin", 2, 1)
        SetSkill("551", "Priest", "Hellish", 3, 1)
        SetSkill("611", "Priest", "Wrath", 0, 1)
        SetSkill("620", "Priest", "Wield", 0, 1)
        SetSkill("641", "Priest", "Harsh", 2, 1)
        SetSkill("650", "Priest", "Collapse", 3, 1)
        SetSkill("712", "Priest", "Tilt", 0, 1)
        SetSkill("721", "Priest", "Bloody", 0, 1)
        SetSkill("739", "Priest", "Raving Edge", 2, 1)
        SetSkill("750", "Priest", "Hades", 3, 1)
        SetSkill("802", "Priest", "Judgement", 3, 1)
        SetSkill("815", "Priest", "Helis", 0, 1)

        SetSkill("004", "Priest", "Strength", 0, 2)
        SetSkill("012", "Priest", "Prayer Of Cronos", 10, 2)
        SetSkill("020", "Priest", "Prayer Of God's Power", 10, 2)
        SetSkill("529", "Priest", "Blasting", 0, 2)
        SetSkill("629", "Priest", "Wildness", 0, 2)
        SetSkill("729", "Priest", "Eruption", 0, 2)

        SetSkill("703", "Priest", "Malice", 11, 3)
        SetSkill("715", "Priest", "Confusion", 13, 3)
        SetSkill("724", "Priest", "Slow", 13, 3)
        SetSkill("727", "Priest", "Reverse Life", 13, 3)
        SetSkill("730", "Priest", "Sleep Wing", 13, 3)
        SetSkill("745", "Priest", "Parasite", 13, 3)
        SetSkill("751", "Priest", "Sleep Carpet", 15, 3)
        SetSkill("757", "Priest", "Torment", 15, 3)
        SetSkill("760", "Priest", "Massive", 18, 3)
    End Sub

    Public Sub InitializeWarriorSkill()
        SetSkill("003", "Warrior", "Slash", 3, 1)
        SetSkill("005", "Warrior", "Crash", 3, 1)
        SetSkill("009", "Warrior", "Piercing", 3, 1)
        SetSkill("010", "Warrior", "Whipping", 3, 1)
        SetSkill("500", "Warrior", "Hash", 3, 1)
        SetSkill("505", "Warrior", "Hoodwink", 0, 1)
        SetSkill("510", "Warrior", "Shear", 3, 1)
        SetSkill("515", "Warrior", "Pierce", 0, 1)
        SetSkill("525", "Warrior", "Carwing", 0, 1)
        SetSkill("530", "Warrior", "Sever", 3, 1)
        SetSkill("535", "Warrior", "Prick", 0, 1)
        SetSkill("540", "Warrior", "Multiple Shock", 3, 1)
        SetSkill("545", "Warrior", "Cleave", 0, 1)
        SetSkill("550", "Warrior", "Mangling", 3, 1)
        SetSkill("555", "Warrior", "Thrust", 0, 1)
        SetSkill("557", "Warrior", "Sword Aura", 0, 1)
        SetSkill("560", "Warrior", "Sword Dancing", 0, 1)
        SetSkill("570", "Warrior", "Howling Sword", 0, 1)
        SetSkill("575", "Warrior", "Blooding", 21, 1)
        SetSkill("580", "Warrior", "Hell Blade", 1, 1)

        SetSkill("002", "Warrior", "Sprint", 0, 2)
        SetSkill("007", "Warrior", "Defense", 0, 2)
        SetSkill("705", "Warrior", "Gain", 0, 2)
    End Sub

    Public Sub InitializeMageSkill()
        SetSkill("503", "Mage", "Burn", 1, 1)
        SetSkill("509", "Mage", "Blaze", 6, 1)
        SetSkill("515", "Mage", "Fire Ball", 5, 1)
        SetSkill("518", "Mage", "Ignition", 0, 1)
        SetSkill("527", "Mage", "Fire Spear", 5, 1)
        SetSkill("533", "Mage", "Fire Burst", 1, 1)
        SetSkill("535", "Mage", "Fire Blast", 5, 1)
        SetSkill("539", "Mage", "Hell Fire", 5, 1)
        SetSkill("543", "Mage", "Specter Of Fire", 0, 1)
        SetSkill("551", "Mage", "Pillar Of Fire", 6, 1)
        SetSkill("554", "Mage", "Fire Thorn", 6, 1)
        SetSkill("556", "Mage", "Manes Of Fire", 0, 1)
        SetSkill("557", "Mage", "Fire Impact", 21, 1)
        SetSkill("542", "Mage", "Flame Blade", 0, 1)
        SetSkill("545", "Mage", "Inferno", 16, 1)
        SetSkill("560", "Mage", "Super Nova", 16, 1)
        SetSkill("570", "Mage", "Incineration", 22, 1)
        SetSkill("571", "Mage", "Meteor Fall", 19, 1)

        SetSkill("603", "Mage", "Freeze", 1, 1)
        SetSkill("609", "Mage", "Chill", 6, 1)
        SetSkill("618", "Mage", "Solid", 0, 1)
        SetSkill("627", "Mage", "Ice Orb", 5, 1)
        SetSkill("633", "Mage", "Ice Burst", 1, 1)
        SetSkill("635", "Mage", "Ice Blast", 5, 1)
        SetSkill("639", "Mage", "Frostbite", 5, 1)
        SetSkill("642", "Mage", "Frozen Blade", 0, 1)
        SetSkill("643", "Mage", "Specter Of Ice", 0, 1)
        SetSkill("645", "Mage", "Blizzard", 16, 1)
        SetSkill("651", "Mage", "Ice Comet", 6, 1)
        SetSkill("656", "Mage", "Manes Of Ice", 0, 1)
        SetSkill("657", "Mage", "Ice Impact", 21, 1)
        SetSkill("660", "Mage", "Frost Nova", 16, 1)
        SetSkill("671", "Mage", "Ice Storm", 19, 1)

        SetSkill("703", "Mage", "Charge", 1, 1)
        SetSkill("709", "Mage", "Counter Spell", 6, 1)
        SetSkill("715", "Mage", "Lightining", 5, 1)
        SetSkill("718", "Mage", "Static Hemispher", 0, 1)
        SetSkill("727", "Mage", "Thunder", 5, 1)
        SetSkill("733", "Mage", "Thunder Burst", 1, 1)
        SetSkill("735", "Mage", "Thunder Blast", 5, 1)
        SetSkill("742", "Mage", "Charged Blade", 0, 1)
        SetSkill("745", "Mage", "Thundercloud", 16, 1)
        SetSkill("743", "Mage", "Specter Of Thunder", 0, 1)
        SetSkill("751", "Mage", "Static Orb", 6, 1)
        SetSkill("754", "Mage", "Static Thorn", 6, 1)
        SetSkill("756", "Mage", "Manes Of Thunder", 0, 1)
        SetSkill("757", "Mage", "Thunder Impact", 21, 1)
        SetSkill("760", "Mage", "Static Nova", 16, 1)
        SetSkill("771", "Mage", "Chain Lightning", 19, 1)

        SetSkill("506", "Mage", "Resist Fire", 0, 2)
        SetSkill("524", "Mage", "Endure Fire", 0, 2)
        SetSkill("548", "Mage", "Immunity Fire", 0, 2)
        SetSkill("606", "Mage", "Resist Cold", 0, 2)
        SetSkill("612", "Mage", "Frozen Armor", 0, 2)
        SetSkill("624", "Mage", "Endure Cold", 0, 2)
        SetSkill("648", "Mage", "Immunity Cold", 0, 2)
        SetSkill("706", "Mage", "Resist Lightning", 0, 2)
        SetSkill("724", "Mage", "Endure Lightning", 0, 2)
        SetSkill("748", "Mage", "Immunity Lightning", 0, 2)
    End Sub
#End Region

End Class
