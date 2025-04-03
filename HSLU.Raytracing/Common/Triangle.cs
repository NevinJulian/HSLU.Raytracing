using System;

namespace Common
{
    public class Triangle : IRaycastable
    {
        public Vector3D V1 { get; }
        public Vector3D V2 { get; }
        public Vector3D V3 { get; }
        public MyColor Color { get; }
        public Vector3D Normal { get; }

        public Triangle(Vector3D v1, Vector3D v2, Vector3D v3, MyColor color)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
            Color = color;

            // Calculate face normal using cross product
            Vector3D edge1 = V2 - V1;
            Vector3D edge2 = V3 - V1;
            Normal = edge1.Cross(edge2).Normalize();
        }

        public (bool hasHit, float intersectionDistance) Intersect(Ray ray)
        {
            // Use a slightly larger epsilon for better numerical stability
            const float EPSILON = 0.000001f;

            // Implement Möller–Trumbore algorithm
            Vector3D edge1 = V2 - V1;
            Vector3D edge2 = V3 - V1;
            Vector3D h = ray.Direction.Cross(edge2);
            float a = edge1.Dot(h);

            // Check if ray is parallel to triangle (or nearly so)
            if (Math.Abs(a) < EPSILON)
                return (false, float.MaxValue);

            float f = 1.0f / a;
            Vector3D s = ray.Origin - V1;
            float u = f * s.Dot(h);

            // If intersection is outside first edge
            if (u < 0.0f || u > 1.0f)
                return (false, float.MaxValue);

            Vector3D q = s.Cross(edge1);
            float v = f * ray.Direction.Dot(q);

            // If intersection is outside second or third edge
            if (v < 0.0f || u + v > 1.0f)
                return (false, float.MaxValue);

            // Calculate intersection distance
            float t = f * edge2.Dot(q);

            // Valid intersection (must be in front of the ray origin)
            if (t > EPSILON)
                return (true, t);

            // No intersection or behind ray origin
            return (false, float.MaxValue);
        }

        public Vector3D GetNormal(Vector3D intersectionPoint)
        {
            // For a flat triangle, normal is the same at any point
            return Normal;
        }
    }
}