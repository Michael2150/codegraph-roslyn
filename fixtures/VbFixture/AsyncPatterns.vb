Imports System.Threading.Tasks
Imports System.Collections.Generic

Namespace Fixtures

    ' Covers: interface with Async function signatures
    Public Interface IDataFetcher
        Function FetchAsync(url As String) As Task(Of String)
        Function FetchManyAsync(urls As IEnumerable(Of String)) As Task(Of String())
    End Interface

    ' Covers: Async Function returning Task(Of T), Await expression,
    '         Async Function returning Task(Of T()), method-to-method call
    Public Class HttpDataFetcher
        Implements IDataFetcher

        Public Async Function FetchAsync(url As String) As Task(Of String) _
            Implements IDataFetcher.FetchAsync
            Await Task.Delay(1)
            Return String.Format("response from {0}", url)
        End Function

        Public Async Function FetchManyAsync(urls As IEnumerable(Of String)) As Task(Of String()) _
            Implements IDataFetcher.FetchManyAsync
            Dim results = New List(Of String)()
            For Each url In urls
                Dim result = Await FetchAsync(url)
                results.Add(result)
            Next
            Return results.ToArray()
        End Function

    End Class

    ' Covers: Async Function returning Task(Of Integer), Await on interface method,
    '         Async Sub (fire-and-forget), non-async method calling async
    Public Class UrlProcessor
        Implements IDataFetcher

        Private ReadOnly _inner As IDataFetcher

        Public Sub New(inner As IDataFetcher)
            _inner = inner
        End Sub

        Public Async Function FetchAsync(url As String) As Task(Of String) _
            Implements IDataFetcher.FetchAsync
            Return Await _inner.FetchAsync(url)
        End Function

        Public Async Function FetchManyAsync(urls As IEnumerable(Of String)) As Task(Of String()) _
            Implements IDataFetcher.FetchManyAsync
            Return Await _inner.FetchManyAsync(urls)
        End Function

        Public Async Function ProcessUrlAsync(url As String) As Task(Of Integer)
            Dim content = Await FetchAsync(url)
            Return content.Length
        End Function

        ' Covers: Async Sub (isAsync = true, return type is void-equivalent)
        Public Async Sub FireAndForget(url As String)
            Await ProcessUrlAsync(url)
        End Sub

        Public Function GetLengthSync(url As String) As Integer
            Return ProcessUrlAsync(url).GetAwaiter().GetResult()
        End Function

    End Class

End Namespace
