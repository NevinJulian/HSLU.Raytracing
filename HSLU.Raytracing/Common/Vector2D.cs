﻿namespace Common;

public readonly struct Vector2D
{
    public readonly float X { get; }

    public readonly float Y { get; }

    public Vector2D(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Vector2D(float x, float y)
    {
        X = (int)x;
        Y = (int)y;
    }

    public double Length => Math.Sqrt(X * X + Y * Y);

    public static Vector2D operator +(Vector2D a, Vector2D b) => new(a.X + b.X, a.Y + b.Y);

    public static Vector2D operator -(Vector2D a, Vector2D b) => new(a.X - b.X, a.Y - b.Y);

    public static Vector2D operator *(Vector2D a, int scalar) => new(a.X * scalar, a.Y * scalar);

    public readonly double EuclideanDistance(Vector2D other)
    {
        var distance = this - other;
        return Math.Sqrt(distance.X * distance.X + distance.Y * distance.Y);
    }

    public double ScalarProduct(Vector2D other) => (this.X * other.X) + (this.Y * other.Y);

    public double ScalarProduct(Vector2D b, double angle) => this.Length * b.Length * Math.Cos(angle);
}

