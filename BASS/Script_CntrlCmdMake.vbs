Set oArgs = WScript.Arguments

intCaseSensitive = 0

Set oFSO = CreateObject("Scripting.FileSystemObject")

If Not oFSO.FileExists(oArgs(0)) Then
    WScript.Echo "Input file does not exist."
Else
    Set oFile = oFSO.OpenTextFile(oArgs(0), 1)
    strText = oFile.ReadAll
    oFile.Close

    strText = Replace(strText, oArgs(2), oArgs(3), 1, -1, intCaseSensitive)

    Set oFile = oFSO.CreateTextFile(oArgs(1) + oArgs(0), True, False)
    oFile.WriteLine strText
    oFile.Close
	
	WScript.Echo "Control Commands.txt made successful."
End If