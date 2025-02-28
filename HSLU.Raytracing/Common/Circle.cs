namespace Common;

public class Circle
{
    public Vector2D Center { get; }

    public MyColor Color { get; }

    public int radius;

    public Circle(Vector2D center, int radius, MyColor color)
    {
        Center = center;
        this.radius = radius;
        Color = color;
    }

    public bool IsInCircle(Vector2D point)
    {
        var dx = point.X - Center.X;
        var dy = point.Y - Center.Y;
        return (dx * dx + dy * dy) <= Math.Pow(radius, 2);
    }
}

