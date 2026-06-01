' Covers: Imports alias, plain Imports, Namespace block, nested namespace,
'         nested class, cross-namespace object creation
Imports Col = System.Collections.Generic
Imports IO = System.IO
Imports System.Text

Namespace Fixtures.Imports

    Public Class AliasDemo
        Private _items As New Col.List(Of String)
        Private _builder As New StringBuilder()

        Public Sub AddItem(item As String)
            _items.Add(item)
            _builder.AppendLine(item)
        End Sub

        Public Function GetSummary() As String
            Return _builder.ToString()
        End Function

        Public Function GetCount() As Integer
            Return _items.Count
        End Function

        Public Function ReadFile(path As String) As String
            Return IO.File.ReadAllText(path)
        End Function

    End Class

End Namespace

Namespace Fixtures.Nested

    Public Class OuterClass

        ' Covers: nested (inner) class
        Public Class InnerClass
            Public Sub DoWork()
                Dim helper As New Fixtures.Imports.AliasDemo()
                helper.AddItem("test")
            End Sub
        End Class

        Public Function CreateInner() As InnerClass
            Return New InnerClass()
        End Function

        Public Sub Run()
            Dim inner = CreateInner()
            inner.DoWork()
        End Sub

    End Class

End Namespace
