Namespace Fixtures

    ' Part 1 of 2: fields, constructor, and read-only properties.
    ' Covers: Partial Class declaration, private fields, constructor, ReadOnly properties
    Partial Public Class DataProcessor

        Private ReadOnly _inputPath As String
        Private ReadOnly _outputPath As String
        Private _recordCount As Integer
        Private _errorCount As Integer

        Public Sub New(inputPath As String, outputPath As String)
            _inputPath = inputPath
            _outputPath = outputPath
        End Sub

        Public ReadOnly Property InputPath As String
            Get
                Return _inputPath
            End Get
        End Property

        Public ReadOnly Property OutputPath As String
            Get
                Return _outputPath
            End Get
        End Property

        Public ReadOnly Property RecordCount As Integer
            Get
                Return _recordCount
            End Get
        End Property

        Public ReadOnly Property ErrorCount As Integer
            Get
                Return _errorCount
            End Get
        End Property

    End Class

End Namespace
