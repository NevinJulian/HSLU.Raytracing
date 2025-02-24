using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VectorSphere3D
{
    public class Vector2D(int x, int y)
    {
        public int X => x;

        public int Y => y;

        public static Vector2D operator +(Vector2D a, Vector2D b) => new(a.X + b.X, a.Y + b.Y);

        public static Vector2D operator -(Vector2D a, Vector2D b) => new(a.X - b.X, a.Y - b.Y);

        public static Vector2D operator *(Vector2D a, int scalar) => new(a.X * scalar, a.Y * scalar);

        public double EuclideanDistance(Vector2D other)
        {
            var distance = this - other;
            return Math.Sqrt(distance.X * distance.X + distance.Y * distance.Y);
        }
    }
}
