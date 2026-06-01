Imports System.IO

Namespace Fixtures

    ' Part 2 of 2: processing methods.
    ' Covers: Partial Class continuation, methods calling private methods defined in same logical class
    Partial Public Class DataProcessor

        Public Sub Process()
            Dim lines = ReadLines()
            Dim results = Transform(lines)
            WriteResults(results)
        End Sub

        Private Function ReadLines() As String()
            Return File.ReadAllLines(_inputPath)
        End Function

        Private Function Transform(lines As String()) As String()
            Dim output = New String(lines.Length - 1) {}
            For i = 0 To lines.Length - 1
                Dim trimmed = lines(i).Trim()
                If trimmed.Length = 0 Then
                    _errorCount += 1
                    output(i) = String.Empty
                Else
                    _recordCount += 1
                    output(i) = trimmed.ToUpperInvariant()
                End If
            Next
            Return output
        End Function

        Private Sub WriteResults(results As String())
            File.WriteAllLines(_outputPath, results)
        End Sub

    End Class

End Namespace
