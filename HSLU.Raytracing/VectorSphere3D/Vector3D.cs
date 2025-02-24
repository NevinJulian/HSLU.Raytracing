using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorSphere
{
    class Vector3D(int x, int y, int z)
    {
        public int X => x;

        public int Y => y;

        public int Z => z;

        public static Vector3D operator +(Vector3D a, Vector3D b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Vector3D operator -(Vector3D a, Vector3D b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Vector3D operator *(Vector3D a, int scalar) => new(a.X * scalar, a.Y * scalar, a.Z * scalar);

        public double EuclideanDistance(Vector3D other)
        {
            var distance = this - other;
            return Math.Sqrt(distance.X * distance.X + distance.Y * distance.Y + distance.Z * distance.Z);
        }
    }
}
