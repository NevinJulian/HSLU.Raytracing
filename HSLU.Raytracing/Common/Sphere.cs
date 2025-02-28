namespace Common;

public class Sphere
{
    public Vector3D Center { get; }
    public MyColor Color { get; }
    public int Radius { get; }

    public Sphere(Vector3D center, int radius, MyColor color)
    {
        Center = center;
        Radius = radius;
        Color = color;
    }

    public bool IsInSphere(Vector2D point)
    {
        var dx = point.X - Center.X;
        var dy = point.Y - Center.Y;
        return (dx * dx + dy * dy) <= Math.Pow(Radius, 2);
    }
}

