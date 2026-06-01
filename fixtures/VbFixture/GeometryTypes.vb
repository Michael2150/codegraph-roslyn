Namespace Fixtures

    ' Covers: Enum, enum members (no explicit values)
    Public Enum Direction
        North
        South
        East
        West
    End Enum

    ' Covers: Enum with explicit integer values
    Public Enum Colour
        Red = 1
        Green = 2
        Blue = 4
        Yellow = 8
    End Enum

    ' Covers: Structure, fields, constructor Sub New, methods, ToString override
    Public Structure Point
        Public X As Double
        Public Y As Double

        Public Sub New(x As Double, y As Double)
            Me.X = x
            Me.Y = y
        End Sub

        Public Function DistanceTo(other As Point) As Double
            Dim dx = X - other.X
            Dim dy = Y - other.Y
            Return Math.Sqrt(dx * dx + dy * dy)
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("({0}, {1})", X, Y)
        End Function

    End Structure

    ' Covers: Structure with ReadOnly properties backed by other structures
    Public Structure Rectangle
        Public TopLeft As Point
        Public BottomRight As Point

        Public ReadOnly Property Width As Double
            Get
                Return Math.Abs(BottomRight.X - TopLeft.X)
            End Get
        End Property

        Public ReadOnly Property Height As Double
            Get
                Return Math.Abs(BottomRight.Y - TopLeft.Y)
            End Get
        End Property

        Public Function Area() As Double
            Return Width * Height
        End Function

        Public Function Contains(p As Point) As Boolean
            Return p.X >= TopLeft.X AndAlso p.X <= BottomRight.X _
               AndAlso p.Y >= TopLeft.Y AndAlso p.Y <= BottomRight.Y
        End Function

    End Structure

    ' Covers: Module block (all members implicitly public/static),
    '         ReadOnly Property at module level, shared functions
    Public Module GeometryUtils

        Public ReadOnly Property Origin As Point
            Get
                Return New Point(0, 0)
            End Get
        End Property

        Public Function Midpoint(a As Point, b As Point) As Point
            Return New Point((a.X + b.X) / 2, (a.Y + b.Y) / 2)
        End Function

        Public Function AreColinear(a As Point, b As Point, c As Point) As Boolean
            Dim area = a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y)
            Return area = 0
        End Function

        Public Function DirectionName(d As Direction) As String
            Select Case d
                Case Direction.North : Return "North"
                Case Direction.South : Return "South"
                Case Direction.East  : Return "East"
                Case Direction.West  : Return "West"
                Case Else            : Return "Unknown"
            End Select
        End Function

    End Module

End Namespace
