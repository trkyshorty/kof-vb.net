Imports System.Runtime.InteropServices

Module Module1

#Region "Constants"
    Public Const PROCESS_TERMINATE = &H1
    Public Const PROCESS_VM_OPERATION = &H8
    Public Const PROCESS_VM_READ = &H10
    Public Const PROCESS_VM_WRITE = &H20
    Public Const PROCESS_CREATE_THREAD = &H2
    Public Const PROCESS_QUERY_INFORMATION = &H400
    Public Const PAGE_READWRITE As Int32 = &H4
    Public Const INFINITE As UInteger = 4294967295
    Public Const MEM_COMMIT As Int32 = &H1000
    Public Const MEM_RELEASE As Int32 = &H8000
    Public Const PROCESS_ALL_ACCESS As Long = &H1F0FFF
    Public Const MEM_RESERVE As Int32 = &H2000
#End Region

#Region "Pointers"
    Public Const KO_PTR_CHR As Long = &H10131C0
    Public Const KO_PTR_DLG As Long = &H100D654
    Public Const KO_PTR_PKT As Long = &H100D620
    Public Const KO_PTR_SND As Long = &H4BD350
    Public KO_PTR_FLDB As Long = KO_PTR_CHR - &H4
    Public Const KO_PTR_FNCZ As Long = &H577C40
    Public Const KO_PTR_FAKE_ITEM As Long = &H5CB5B0
    Public Const KO_PTR_MULTI As Long = &HD21B0C
    Public Const KO_PTR_FMBS As Long = &H4E8F00
    Public Const KO_PTR_FPBS As Long = &H4F6370
#End Region

#Region "Offsets"
    Public Const KO_OFF_WH As Long = &H6C0
    Public Const KO_OFF_MAX_MP As Long = &HB38
    Public Const KO_OFF_MP As Long = &HB3C
    Public Const KO_OFF_MAX_HP As Long = &H6B8
    Public Const KO_OFF_HP As Long = &H6BC
    Public Const KO_OFF_NAME As Long = &H688
    Public Const KO_OFF_NAME_LEN As Long = &H698
    Public Const KO_OFF_ID As Long = &H680
    Public Const KO_OFF_X As Long = &HD8
    Public Const KO_OFF_Y As Long = &HE0
    Public Const KO_OFF_Z As Long = &HDC
    Public Const KO_OFF_CLASS As Long = &H6B0
    Public Const KO_OFF_NT As Long = &H6A8
    Public Const KO_OFF_MOB As Long = &H644
    Public Const KO_OFF_MOB_CORD As Long = &H408
    Public Const KO_OFF_MOB_X As Long = &H7C
    Public Const KO_OFF_MOB_Y As Long = &H84
    Public Const KO_OFF_MOB_Z As Long = &H80
    Public Const KO_OFF_MOVE As Long = &HF88
    Public Const KO_OFF_GO_X As Long = &HF94
    Public Const KO_OFF_GO_Y As Long = &HF9C
    Public Const KO_OFF_GO_Z As Long = &HF98
    Public Const KO_OFF_MOVE_TYPE As Long = &H3F0
    Public Const KO_OFF_STATE As Long = &H2A0
    Public Const KO_OFF_ZONE As Long = &HBE0
    Public Const KO_OFF_MX As Long = &HF94
    Public Const KO_OFF_MY As Long = &HF9C
    Public Const KO_OFF_MZ As Long = &HF98
    Public Const KO_OFF_EXP As Long = &HBA4
    Public Const KO_OFF_MAX_EXP As Long = &HB9C
    Public Const KO_OFF_GOLD As Long = &HB48
    Public Const KO_OFF_SWIFT As Long = &H79E
    Public Const KO_OFF_WEIGHT As Long = &HB88
    Public Const KO_OFF_SKILL_BASE As Long = &H1CC
    Public Const KO_OFF_SKILL_ID As Long = &H12C
    Public Const KO_OFF_SKILL_POINT_BASE As Long = &H1E8
    Public Const KO_OFF_SKILL_POINT1 As Long = &H180
    Public Const KO_OFF_SKILL_POINT2 As Long = &H184
    Public Const KO_OFF_SKILL_POINT3 As Long = &H188
    Public Const KO_OFF_SKILL_POINT4 As Long = &H18C
    Public Const KO_OFF_PARTY As Long = &H2FC
    Public Const KO_OFF_PARTY_BASE As Long = &H1E4
    Public Const KO_OFF_PARTY_COUNT As Long = &H300
    Public Const KO_OFF_PARTY_ID As Long = &H8
    Public Const KO_OFF_PARTY_CLASS As Long = &H10
    Public Const KO_OFF_PARTY_HP As Long = &H14
    Public Const KO_OFF_PARTY_MAXHP As Long = &H18
    Public Const KO_OFF_PARTY_CURE1 As Long = &H24
    Public Const KO_OFF_PARTY_CURE2 As Long = &H25
    Public Const KO_OFF_PARTY_CURE3 As Long = &H26
    Public Const KO_OFF_PARTY_CURE4 As Long = &H27
    Public Const KO_OFF_PARTY_NAME As Long = &H30
    Public Const KO_OFF_PARTY_NAME_LEN As Long = &H40
    Public Const KO_OFF_INVENTORY_BASE As Long = &H1B4
    Public Const KO_OFF_INVENTORY_SLOT As Long = &H20C
    Public Const KO_OFF_WAREHOUSE_BASE As Long = &H204
    Public Const KO_OFF_WAREHOUSE_SLOT As Long = &H128
#End Region

#Region "Kernel Functions"
    Public Declare Function OpenProcess Lib "kernel32.dll" (ByVal dwDesiredAcess As UInt32, ByVal bInheritHandle As Boolean, ByVal dwProcessId As Int32) As IntPtr
    Public Declare Function ReadProcessMemory Lib "kernel32.dll" Alias "ReadProcessMemory" (ByVal hProcess As Integer, ByVal lpBaseAddress As Integer, ByRef lpBuffer As Integer, ByVal nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Integer
    Public Declare Function ReadFloatMemory Lib "kernel32.dll" Alias "ReadProcessMemory" (ByVal hProcess As IntPtr, ByVal lpBaseAddress As IntPtr, ByRef buffer As Single, ByVal size As Int32, ByRef lpNumberOfBytesRead As Int32) As Boolean
    Public Declare Function WriteProcessMemory Lib "kernel32.dll" (ByVal hProcess As IntPtr, ByVal lpBaseAddress As IntPtr, ByRef lpBuffer As Long, ByVal nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Boolean
    Public Declare Function WriteProcessMemory1 Lib "kernel32.dll" Alias "WriteProcessMemory" (ByVal hProcess As IntPtr, ByVal lpBaseAddress As Integer, ByRef lpBuffer As Byte, ByVal nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Boolean
    Public Declare Function WriteProcessMemory2 Lib "kernel32.dll" Alias "WriteProcessMemory" (ByVal hProcess As IntPtr, ByVal lpBaseAddress As IntPtr, ByVal lpBuffer As Byte(), ByVal nSize As System.UInt32, <Out()> ByRef lpNumberOfBytesWritten As IntPtr) As Boolean
    Public Declare Function WriteFloatMemory Lib "kernel32.dll" Alias "WriteProcessMemory" (ByVal hProcess As IntPtr, ByVal lpBaseAddress As Integer, ByRef lpBuffer As Single, ByVal nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Integer
    Public Declare Function GetTickCount Lib "kernel32.dll" () As Long
    Public Declare Function CloseHandle Lib "kernel32.dll" (ByVal hObject As IntPtr) As Boolean
    Public Declare Function CreateRemoteThread Lib "kernel32.dll" (ByVal hProcess As Integer, ByVal lpThreadAttributes As Integer, ByVal dwStackSize As Integer, ByVal lpStartAddress As Integer, ByVal lpParameter As Integer, ByVal dwCreationFlags As Integer, ByRef lpThreadId As Integer) As Integer
    Public Declare Function CreateMailslot Lib "kernel32.dll" Alias "CreateMailslotA" (ByVal lpName As String, ByVal nMaxMessageSize As Int32, ByVal lReadTimeout As Int32, ByVal lpSecurityAttributes As Int32) As IntPtr
    Public Declare Function GetProcAddress Lib "kernel32.dll" (ByVal hModule As Integer, ByVal lpProcName As String) As Integer
    Public Declare Function GetModuleHandle Lib "kernel32.dll" Alias "GetModuleHandleA" (ByVal lpModuleName As String) As IntPtr
    Public Declare Function GetAsyncKeyState Lib "user32.dll" (ByVal vkey As Integer) As Short
    Public Declare Auto Function SetWindowText Lib "user32.dll" (ByVal hWnd As IntPtr, ByVal lpstring As String) As Boolean
    Public Declare Function VirtualFreeEx Lib "kernel32.dll" (ByVal hProcess As IntPtr, ByVal lpAddress As IntPtr, ByVal dwSize As Integer, ByVal dwFreeType As Integer) As Boolean
    Public Declare Function GetMailslotInfo Lib "kernel32.dll" (ByVal hMailSlot As IntPtr, ByRef lpMaxMessageSize As Int32, ByRef lpNextSize As Int32, ByRef lpMessageCount As Int32, ByRef lpReadTimeout As Int32) As Int32
    Public Declare Function ReadFile Lib "kernel32.dll" (ByVal hFile As IntPtr, ByRef lpBuffer As [Byte], ByVal nNumberOfBytesToRead As Int32, ByRef lpNumberOfBytesRead As Int32, ByVal lpOverlapped As Int32) As IntPtr
    <DllImport("user32.dll", SetLastError:=True)>
    Public Function GetForegroundWindow() As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Public Function GetWindowThreadProcessId(ByVal hWnd As IntPtr, ByRef lpdwProcessId As UInteger) As Integer
    End Function
    <DllImport("kernel32.dll", SetLastError:=True, ExactSpelling:=True)>
    Public Function VirtualAllocEx(ByVal hProcess As IntPtr, ByVal lpAddress As IntPtr,
     ByVal dwSize As UInteger, ByVal flAllocationType As UInteger,
     ByVal flProtect As UInteger) As IntPtr
    End Function

    <DllImport("kernel32", SetLastError:=True)>
    Public Function WaitForSingleObject(ByVal handle As IntPtr, ByVal milliseconds As UInt32) As UInt32
    End Function
#End Region

#Region "Read Memory Functions"
    Public Function ReadByte(ByVal Handle As Long, ByVal Address As Long) As Byte
        On Error Resume Next
        Dim Value As Byte : ReadProcessMemory(Handle, Address, Value, 1, 0&) : Return Value
    End Function

    Public Function ReadLong(ByVal Handle As Long, ByVal Address As Long) As Long
        On Error Resume Next
        Dim Value As Long : ReadProcessMemory(Handle, Address, Value, 4, 0&) : Return Value
    End Function

    Public Function ReadFloat(ByVal Handle As Long, ByVal Address As Long) As Single
        On Error Resume Next
        Dim Value As Single : ReadFloatMemory(Handle, Address, Value, 4, 0&) : Return Value
    End Function

    Public Function ReadString(ByVal Handle As Long, ByVal Address As Integer, ByVal Size As Integer) As String
        On Error Resume Next
        Dim TempInt As Integer = 0 : ReadString = ""
        For n As Integer = 0 To Size
            ReadProcessMemory(Handle, Address + n, TempInt, 1, 0)
            If TempInt > 0 Then ReadString += Chr(TempInt) Else Exit Function
        Next
    End Function
#End Region

#Region "Write Memory Functions"
    Public Sub WriteLong(ByVal Handle As Long, ByVal Address As Long, ByVal Value As Long)
        On Error Resume Next
        WriteProcessMemory(Handle, Address, Value, 4, 0)
    End Sub
    Public Sub WriteFloat(ByVal Handle As Long, ByVal Address As Long, ByVal Value As Long)
        On Error Resume Next
        WriteFloatMemory(Handle, Address, Value, 4, 0)
    End Sub
    Public Sub WriteByte(ByVal Handle As Long, ByVal Address As Long, ByVal Value As Long)
        On Error Resume Next
        WriteProcessMemory(Handle, Address, Value, 1, 0)
    End Sub
#End Region

#Region "Game Functions"
    Public Sub InjectPatch(ByVal Handle As Long, ByVal Addr As Long, ByVal Code As String)
        On Error Resume Next
        Dim Bytes() As Byte
        Bytes = StringToByte(Code)
        WriteProcessMemory1(Handle, Addr, Bytes(LBound(Bytes)), UBound(Bytes) - LBound(Bytes), 0&)
    End Sub

    Public Sub ExecuteRemoteCode(ByVal Handle As Long, ByVal CodeMem As Long, ByVal Code As String)
        On Error Resume Next
        Dim Thread As IntPtr
        Dim CodeBytes() As Byte = StringToByte(Code)
        Dim CodeSize As Long = UBound(CodeBytes) - LBound(CodeBytes)
        If CodeMem <> 0 Then
            WriteProcessMemory2(Handle, CodeMem, CodeBytes, CodeSize, 0)
            Thread = CreateRemoteThread(Handle, 0, 0, CodeMem, 0, 0, 0)
            WaitForSingleObject(Thread, &HFFFF)
            CloseHandle(Thread)
        End If
    End Sub

    Public Sub SendPacket(ByVal Handle As Long, ByVal PacketMem As Long, ByVal CodeMem As Long, ByVal Packet As String)
        On Error Resume Next
        Dim CodeStr As String
        Dim PacketBytes() As Byte = StringToByte(Packet)
        Dim PacketSize As Long = UBound(PacketBytes) - LBound(PacketBytes)
        If PacketMem <> 0 Then
            WriteProcessMemory2(Handle, PacketMem, PacketBytes, PacketSize, 0)
            CodeStr = "608B0D" & AlignDWORD(KO_PTR_PKT) & "68" & AlignDWORD(PacketSize) & "68" & AlignDWORD(PacketMem) & "BF" & AlignDWORD(KO_PTR_SND) & "FFD7C605" & AlignDWORD(KO_PTR_PKT + &HC5) & "0061C3"
            ExecuteRemoteCode(Handle, CodeMem, CodeStr)
        End If
    End Sub
#End Region

#Region "Convertions"
    Public Function AlignDWORD(ByVal pParam As Long) As String
        Dim HiW As Integer, LoW As Integer, HiBHiW As Byte, HiBLoW As Byte, LoBHiW As Byte, LoBLoW As Byte
        HiW = HiWord(pParam) : LoW = LoWord(pParam) : HiBHiW = HiByte(HiW) : HiBLoW = HiByte(LoW) : LoBHiW = LoByte(HiW) : LoBLoW = LoByte(LoW)
        AlignDWORD = IIf(Len(Hex(LoBLoW)) = 1, "0" & Hex(LoBLoW), Hex(LoBLoW)) & IIf(Len(Hex(HiBLoW)) = 1, "0" & Hex(HiBLoW), Hex(HiBLoW)) & IIf(Len(Hex(LoBHiW)) = 1, "0" & Hex(LoBHiW), Hex(LoBHiW)) & IIf(Len(Hex(HiBHiW)) = 1, "0" & Hex(HiBHiW), Hex(HiBHiW))
    End Function

    Public Function HiByte(ByVal wParam As Integer) As Byte
        HiByte = (wParam And &HFF00&) \ (&H100)
    End Function

    Public Function LoByte(ByVal wParam As Integer) As Byte
        LoByte = wParam And &HFF&
    End Function

    Public Function LoWord(ByVal DWord As Long) As Integer
        If DWord And &H8000& Then LoWord = DWord Or &HFFFF0000 Else : LoWord = DWord And &HFFFF&
    End Function

    Public Function HiWord(ByVal DWord As Long) As Integer
        HiWord = (DWord And &HFFFF0000) \ &H10000
    End Function

    Public Function StringToByte(ByVal pStr As String) As Byte()
        Dim i As Long, j As Long, pByte() As Byte : ReDim pByte(0 To Len(pStr) / 2)
        j = LBound(pByte) - 1
        For i = 1 To Len(pStr) Step 2
            j = j + 1
            pByte(j) = CByte("&H" & Mid(pStr, i, 2))
        Next
        Return pByte
    End Function

    Public Function ByteToString(ByVal Data() As Byte) As String
        Dim i As Short, s As String
        s = ""
        For i = 0 To UBound(Data) - 1
            s = s & Data(i).ToString("x2").ToUpper
        Next i
        Return System.Text.Encoding.Unicode.GetString(Data)
    End Function

    Public Function StringToHex(ByVal StrToHex As String) As String
        Dim strTemp As String, strReturn As String, i As Long : strReturn = ""
        For i = 1 To Len(StrToHex)
            strTemp = Hex$(Asc(Mid$(StrToHex, i, 1)))
            If Len(strTemp) = 1 Then strTemp = "0" & strTemp
            strReturn = strReturn & strTemp
        Next i : Return strReturn
    End Function

    Public Function AddressDistance(ByVal addr As Long, ByVal targetAddr As Long) As String
        If addr < targetAddr Then
            Return AlignDWORD(targetAddr - (addr + 5))
        Else
            Return AlignDWORD(((&HFFFFFFFF - addr) + (targetAddr - 4)))
        End If
        Return "00000000"
    End Function
#End Region

End Module
