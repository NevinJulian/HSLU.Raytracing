namespace Common;

public readonly struct Vector3D(int x, int y, int z)
{
    public readonly int X => x;

    public readonly int Y => y;

    public readonly int Z => z;

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
}

