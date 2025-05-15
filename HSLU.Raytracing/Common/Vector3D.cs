namespace Common;
public readonly struct Vector3D
{
    public readonly float X { get; }
    public readonly float Y { get; }
    public readonly float Z { get; }

    public static Vector3D Zero => new Vector3D(0, 0, 0);

    public Vector3D(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Vector3D(double x, double y, double z)
    {
        X = (float)x;
        Y = (float)y;
        Z = (float)z;
    }

    public float Length => MathF.Sqrt(this.LengthSquared);
    public float LengthSquared => (X * X) + (Y * Y) + (Z * Z);

    public static Vector3D operator +(Vector3D a, Vector3D b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3D operator -(Vector3D a, Vector3D b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vector3D operator -(Vector3D a) => new(-a.X, -a.Y, -a.Z);
    public static Vector3D operator *(Vector3D a, float scalar) => new(a.X * scalar, a.Y * scalar, a.Z * scalar);
    public static Vector3D operator *(float scalar, Vector3D a) => new(a.X * scalar, a.Y * scalar, a.Z * scalar);

    public Vector3D Normalize()
    {
        var length = Length;
        if (length < float.Epsilon)
            return new Vector3D(0, 0, 0);

        return new Vector3D(X / length, Y / length, Z / length);
    }

    public float Dot(Vector3D other) => (this.X * other.X) + (this.Y * other.Y) + (this.Z * other.Z);

    public Vector3D Cross(Vector3D other) => new(
        (Y * other.Z) - (Z * other.Y),
        (Z * other.X) - (X * other.Z),
        (X * other.Y) - (Y * other.X)
    );

    public Vector3D ReflectAround(Vector3D normal)
    {
        float dot = this.Dot(normal);
        return this - normal * (2 * dot);
    }

    public override string ToString() => $"({X}, {Y}, {Z})";
}