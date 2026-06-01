namespace Fixtures
{
    // Covers: enum, enum members (no explicit values)
    public enum Direction { North, South, East, West }

    // Covers: enum with explicit values and [Flags]
    [Flags]
    public enum Colour
    {
        Red    = 1,
        Green  = 2,
        Blue   = 4,
        Yellow = 8
    }

    // Covers: struct, fields, constructor, methods, override ToString
    public struct Point
    {
        public double X { get; }
        public double Y { get; }

        public Point(double x, double y) { X = x; Y = y; }

        public double DistanceTo(Point other)
        {
            var dx = X - other.X;
            var dy = Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public override string ToString() => $"({X}, {Y})";
    }

    // Covers: record struct (positional), computed properties, method
    public record struct Rectangle(Point TopLeft, Point BottomRight)
    {
        public double Width  => Math.Abs(BottomRight.X - TopLeft.X);
        public double Height => Math.Abs(BottomRight.Y - TopLeft.Y);

        public double Area() => Width * Height;

        public bool Contains(Point p) =>
            p.X >= TopLeft.X && p.X <= BottomRight.X &&
            p.Y >= TopLeft.Y && p.Y <= BottomRight.Y;
    }

    // Covers: record class (positional), computed property, method
    public record PersonRecord(string FirstName, string LastName)
    {
        public string FullName => $"{FirstName} {LastName}";
        public string Greet()  => $"Hello, I am {FullName}";
    }

    // Covers: static class, static methods, static property
    public static class GeometryUtils
    {
        public static Point Origin => new Point(0, 0);

        public static Point Midpoint(Point a, Point b) =>
            new Point((a.X + b.X) / 2, (a.Y + b.Y) / 2);

        public static bool AreColinear(Point a, Point b, Point c)
        {
            var area = a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y);
            return area == 0;
        }

        public static string DirectionName(Direction d) => d switch
        {
            Direction.North => "North",
            Direction.South => "South",
            Direction.East  => "East",
            Direction.West  => "West",
            _               => "Unknown"
        };
    }
}
