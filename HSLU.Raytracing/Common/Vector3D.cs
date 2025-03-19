namespace Common;
public readonly struct Vector3D
{
    public readonly float X { get; }
    public readonly float Y { get; }
    public readonly float Z { get; }

    // Constructor for float values
    public Vector3D(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    // Constructor for double values - converts to float
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
    public static Vector3D operator *(Vector3D a, int scalar) => new(a.X * scalar, a.Y * scalar, a.Z * scalar);
    public static Vector3D operator *(Vector3D a, float scalar) => new(a.X * scalar, a.Y * scalar, a.Z * scalar);
    public static Vector3D operator *(Vector3D a, double scalar) => new(a.X * (float)scalar, a.Y * (float)scalar, a.Z * (float)scalar);

    public readonly float EuclideanDistance(Vector3D other)
    {
        var distance = this - other;
        return MathF.Sqrt(distance.X * distance.X + distance.Y * distance.Y + distance.Z * distance.Z);
    }

    public float ScalarProduct(Vector3D other) => (this.X * other.X) + (this.Y * other.Y) + (this.Z * other.Z);
    public float ScalarProduct(Vector3D other, float angle) => this.Length * other.Length * MathF.Cos(angle);

    public Vector3D Normalize()
    {
        var length = this.Length;
        return new Vector3D(X / length, Y / length, Z / length);
    }

    public float Dot(Vector3D other) => (this.X * other.X) + (this.Y * other.Y) + (this.Z * other.Z);

    // Cross product method for calculating normal vectors
    public Vector3D Cross(Vector3D other) => new(
        (Y * other.Z) - (Z * other.Y),
        (Z * other.X) - (X * other.Z),
        (X * other.Y) - (Y * other.X)
    );
}