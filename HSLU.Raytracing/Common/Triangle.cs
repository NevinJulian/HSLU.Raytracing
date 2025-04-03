namespace Common
{
    public class Triangle : IRaycastable
    {
        public Vector3D V1 { get; } // Renamed A to V1 to keep your naming convention
        public Vector3D V2 { get; } // Renamed B to V2
        public Vector3D V3 { get; } // Renamed C to V3
        public MyColor Color { get; }
        public Vector3D Normal { get; }

        public Triangle(Vector3D v1, Vector3D v2, Vector3D v3, MyColor color)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
            Color = color;

            // Calculate normal using cross product (similar to working example)
            Vector3D edge1 = V2 - V1;
            Vector3D edge2 = V3 - V1;
            Normal = edge1.Cross(edge2).Normalize();
        }

        public (bool hasHit, float intersectionDistance) Intersect(Ray ray)
        {
            // Using the approach from the working example
            var lambda = CalculateLambda(ray);

            if (lambda > 0)
            {
                // Calculate intersection point
                Vector3D q = ray.Origin + ray.Direction * lambda;

                // Vectors from intersection point to triangle vertices
                Vector3D aq = q - V1;
                Vector3D bq = q - V2;
                Vector3D cq = q - V3;

                // Vectors along triangle edges (in opposite direction)
                Vector3D ba = V1 - V2;
                Vector3D cb = V2 - V3;
                Vector3D ac = V3 - V1;

                // Cross products to check orientation
                Vector3D v1 = bq.Cross(ba);
                Vector3D v2 = cq.Cross(cb);
                Vector3D v3 = aq.Cross(ac);

                // Check if point is inside triangle
                if (CheckEqualSign(v1.Z, v2.Z, v3.Z))
                {
                    return (true, lambda);
                }
            }

            return (false, float.MaxValue);
        }

        private float CalculateLambda(Ray ray)
        {
            Vector3D p = ray.Origin;
            Vector3D u = ray.Direction;

            // Calculate denominator for plane intersection
            float denominator = u.Dot(Normal);

            // Check if ray is parallel to plane
            if (Math.Abs(denominator) < 1e-6f)
                return float.MaxValue;

            // Calculate distance to intersection
            float lambda = (V1 - p).Dot(Normal) / denominator;

            // Return only positive intersections
            return lambda > 0 ? lambda : float.MaxValue;
        }

        private bool CheckEqualSign(float v1z, float v2z, float v3z)
        {
            // All values must have the same sign (either all positive or all negative)
            return (v1z >= 0 && v2z >= 0 && v3z >= 0) || (v1z < 0 && v2z < 0 && v3z < 0);
        }

        public Vector3D GetNormal(Vector3D intersectionPoint)
        {
            // For a triangle, the normal is constant
            return Normal;
        }
    }
}