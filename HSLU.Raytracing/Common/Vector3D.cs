namespace Common;

public readonly struct Vector3D(float x, float y, float z)
{
    public readonly float X => x;

    public readonly float Y => y;

    public readonly float Z => z;

    public float Length => MathF.Sqrt(this.LengthSquared);

    public float LengthSquared => (X * X) + (Y * Y) + (Z * Z);

    public static Vector3D operator +(Vector3D a, Vector3D b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Vector3D operator -(Vector3D a, Vector3D b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Vector3D operator *(Vector3D a, int scalar) => new(a.X * scalar, a.Y * scalar, a.Z * scalar);

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

    public float DotProduct(Vector3D other) => (this.X * other.X) + (this.Y * other.Y) + (this.Z * other.Z);
}

