Imports System

Namespace Fixtures

    ' Covers: class inheriting EventArgs
    Public Class ThresholdReachedEventArgs
        Inherits EventArgs

        Public Property Threshold As Integer
        Public Property TimeReached As DateTime

        Public Sub New(threshold As Integer)
            Me.Threshold = threshold
            Me.TimeReached = DateTime.UtcNow
        End Sub

    End Class

    ' Covers: Event declaration, RaiseEvent, Protected Overridable method pattern
    Public Class Counter
        Public Event ThresholdReached As EventHandler(Of ThresholdReachedEventArgs)

        Private _count As Integer
        Private ReadOnly _threshold As Integer

        Public Sub New(threshold As Integer)
            _threshold = threshold
        End Sub

        Public Sub Increment()
            _count += 1
            If _count >= _threshold Then
                OnThresholdReached(New ThresholdReachedEventArgs(_threshold))
            End If
        End Sub

        Protected Overridable Sub OnThresholdReached(e As ThresholdReachedEventArgs)
            RaiseEvent ThresholdReached(Me, e)
        End Sub

        Public ReadOnly Property Count As Integer
            Get
                Return _count
            End Get
        End Property

        Public Sub Reset()
            _count = 0
        End Sub

    End Class

    ' Covers: WithEvents field, Handles clause, method-to-method call
    Public Class Alarm
        Private WithEvents _counter As Counter

        Public Sub New(counter As Counter)
            _counter = counter
        End Sub

        Private Sub Counter_ThresholdReached(sender As Object, e As ThresholdReachedEventArgs) _
            Handles _counter.ThresholdReached
            TriggerAlarm(e.Threshold)
        End Sub

        Private Sub TriggerAlarm(threshold As Integer)
            Console.WriteLine(String.Format("Alarm triggered at threshold {0}", threshold))
        End Sub

        Public Sub Detach()
            _counter = Nothing
        End Sub

    End Class

    ' Covers: second WithEvents consumer on same event source
    Public Class Logger
        Private WithEvents _counter As Counter
        Private _log As New System.Collections.Generic.List(Of String)

        Public Sub New(counter As Counter)
            _counter = counter
        End Sub

        Private Sub Counter_ThresholdReached(sender As Object, e As ThresholdReachedEventArgs) _
            Handles _counter.ThresholdReached
            RecordEvent(e.Threshold)
        End Sub

        Private Sub RecordEvent(threshold As Integer)
            _log.Add(String.Format("Threshold {0} at {1}", threshold, DateTime.UtcNow))
        End Sub

        Public ReadOnly Property Log As System.Collections.Generic.IReadOnlyList(Of String)
            Get
                Return _log
            End Get
        End Property

    End Class

End Namespace
