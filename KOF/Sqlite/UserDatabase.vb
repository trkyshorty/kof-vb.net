Imports System.Data.SQLite

Public Class UserDatabase
#Region "Entry Point"
    Private _Connection As SQLiteConnection

    Public Sub New(Connection As SQLiteConnection)
        _Connection = Connection
    End Sub

    Public Sub Initialize()
        Dim SqliteCommand As SQLiteCommand = _Connection.CreateCommand()
        SqliteCommand.CommandTimeout = 60
        SqliteCommand.CommandText = "
            CREATE TABLE IF NOT EXISTS [control] 
            (
                [character]     TEXT NOT NULL,
                [param]         TEXT NOT NULL,
                [value]         TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS [active_skill] 
            (
                [character]     TEXT NOT NULL,
                [id]            TEXT NOT NULL,
                [type]          TEXT NOT NULL
            );
        "
        SqliteCommand.ExecuteNonQuery()
    End Sub
#End Region

#Region "Control Table"
    Public Function GetAllControl(ByVal Character As String) As Dictionary(Of String, String)
        Dim SqliteCommand As SQLiteCommand = _Connection.CreateCommand()
        SqliteCommand.CommandTimeout = 60
        SqliteCommand.Parameters.AddWithValue("@character", Character)
        SqliteCommand.CommandText = "SELECT * FROM control Where character = @character;"

        Dim SqliteDataReader As SQLiteDataReader = SqliteCommand.ExecuteReader()
        Dim ControlList As New Dictionary(Of String, String)()

        While (SqliteDataReader.Read())
            ControlList.Add(SqliteDataReader.GetString(1), SqliteDataReader.GetString(2))
        End While

        GetAllControl = ControlList
    End Function

    Public Sub SetControl(ByVal Character As String, ByVal Param As String, ByVal Value As String)
        Dim SqliteCommand As SQLiteCommand = _Connection.CreateCommand()
        SqliteCommand.CommandTimeout = 60
        SqliteCommand.Parameters.AddWithValue("@character", Character)
        SqliteCommand.Parameters.AddWithValue("@param", Param)
        SqliteCommand.Parameters.AddWithValue("@value", Value)
        If GetControl(Character, Param) = "" Then
            SqliteCommand.CommandText = "INSERT INTO control (character, param, value) VALUES (@character, @param, @value);"
        Else
            SqliteCommand.CommandText = "UPDATE control SET value = @value WHERE character = @character AND param = @param;"
        End If
        SqliteCommand.ExecuteNonQuery()
    End Sub

    Public Function GetControl(ByVal Character As String, ByVal Param As String) As String
        Dim SqliteCommand As SQLiteCommand = _Connection.CreateCommand()
        SqliteCommand.CommandTimeout = 60
        SqliteCommand.Parameters.AddWithValue("@character", Character)
        SqliteCommand.Parameters.AddWithValue("@param", Param)
        SqliteCommand.CommandText = "SELECT * FROM control WHERE character = @character AND param = @param LIMIT 1;"
        Dim SqliteDataReader As SQLiteDataReader = SqliteCommand.ExecuteReader()
        While (SqliteDataReader.Read())
            Return SqliteDataReader.GetString(2)
        End While
        GetControl = ""
    End Function
#End Region

#Region "Skill Table"
    Public Function GetAllActiveSkill(ByVal Character As String) As Dictionary(Of String, String)
        Dim SqliteCommand As SQLiteCommand = _Connection.CreateCommand()
        SqliteCommand.CommandTimeout = 60
        SqliteCommand.Parameters.AddWithValue("@character", Character)
        SqliteCommand.CommandText = "SELECT * FROM active_skill WHERE character = @character;"
        Dim SqliteDataReader As SQLiteDataReader = SqliteCommand.ExecuteReader()
        Dim SkillList As New Dictionary(Of String, String)
        While (SqliteDataReader.Read())
            SkillList.Add(SqliteDataReader.GetString(1), SqliteDataReader.GetString(2))
        End While
        GetAllActiveSkill = SkillList
    End Function

    Public Function GetActiveSkill(ByVal Character As String, ByVal Id As String, ByVal Type As String) As String
        Dim SqliteCommand As SQLiteCommand = _Connection.CreateCommand()
        SqliteCommand.CommandTimeout = 60
        SqliteCommand.Parameters.AddWithValue("@character", Id)
        SqliteCommand.Parameters.AddWithValue("@id", Id)
        SqliteCommand.Parameters.AddWithValue("@type", Type)
        SqliteCommand.CommandText = "SELECT * FROM active_skill WHERE character = @character AND id = @id AND type = @type LIMIT 1;"
        Dim SqliteDataReader As SQLiteDataReader = SqliteCommand.ExecuteReader()
        While (SqliteDataReader.Read())
            Return SqliteDataReader.GetString(1)
        End While
        GetActiveSkill = ""
    End Function

    Public Sub SetActiveSkill(ByVal Character As String, ByVal Id As String, ByVal Type As String)
        Dim SqliteCommand As SQLiteCommand = _Connection.CreateCommand()
        SqliteCommand.CommandTimeout = 60
        SqliteCommand.Parameters.AddWithValue("@character", Character)
        SqliteCommand.Parameters.AddWithValue("@id", Id)
        SqliteCommand.Parameters.AddWithValue("@type", Type)
        SqliteCommand.CommandText = "INSERT INTO active_skill (character, id, type) VALUES (@character, @id, @type);"
        SqliteCommand.ExecuteNonQuery()
    End Sub

    Public Sub DeleteActiveSkill(ByVal Character As String, ByVal Id As String, ByVal Type As String)
        Dim SqliteCommand As SQLiteCommand = _Connection.CreateCommand()
        SqliteCommand.CommandTimeout = 60
        SqliteCommand.Parameters.AddWithValue("@character", Character)
        SqliteCommand.Parameters.AddWithValue("@id", Id)
        SqliteCommand.Parameters.AddWithValue("@type", Type)
        SqliteCommand.CommandText = "DELETE FROM active_skill WHERE character = @character AND id = @id AND type = @type"
        SqliteCommand.ExecuteNonQuery()
    End Sub
#End Region



End Class
