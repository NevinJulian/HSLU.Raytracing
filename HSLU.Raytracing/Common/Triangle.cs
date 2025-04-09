namespace Common
{
    public class Triangle : IRaycastable
    {
        public Vector3D V1 { get; }
        public Vector3D V2 { get; }
        public Vector3D V3 { get; }
        public MyColor Color { get; }
        public Material Material { get; }
        public Vector3D Normal { get; }
        public int ObjectId { get; set; }
        public int ParentId { get; set; } = -1; // -1 means no parent

        public Triangle(Vector3D v1, Vector3D v2, Vector3D v3, MyColor color, MaterialType materialType = MaterialType.WHITE_PLASTIC, float reflectivity = 0f)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
            Color = color;
            Material = Common.Material.Create(materialType, reflectivity);

            // Calculate face normal using cross product of two edges (ensure consistent winding)
            Vector3D edge1 = V2 - V1;
            Vector3D edge2 = V3 - V1;
            Normal = edge1.Cross(edge2).Normalize();
        }

        public Triangle(Vector3D v1, Vector3D v2, Vector3D v3, Material material)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
            Material = material;
            Color = material.Diffuse; // Use diffuse color as the main color

            // Calculate face normal using cross product of two edges
            Vector3D edge1 = V2 - V1;
            Vector3D edge2 = V3 - V1;
            Normal = edge1.Cross(edge2).Normalize();
        }

        public (bool hasHit, float intersectionDistance) Intersect(Ray ray)
        {
            // Implement Möller–Trumbore algorithm
            const float EPSILON = 0.0001f;

            Vector3D edge1 = V2 - V1;
            Vector3D edge2 = V3 - V1;
            Vector3D h = ray.Direction.Cross(edge2);
            float a = edge1.Dot(h);

            // Check if ray is parallel to triangle
            if (MathF.Abs(a) < EPSILON)
                return (false, float.MaxValue);

            float f = 1.0f / a;
            Vector3D s = ray.Origin - V1;
            float u = f * s.Dot(h);

            // Check if intersection point is outside the triangle
            if (u < 0.0f || u > 1.0f)
                return (false, float.MaxValue);

            Vector3D q = s.Cross(edge1);
            float v = f * ray.Direction.Dot(q);

            // Check if intersection point is outside the triangle
            if (v < 0.0f || u + v > 1.0f)
                return (false, float.MaxValue);

            // Calculate intersection distance
            float t = f * edge2.Dot(q);

            // Check if intersection is behind the ray origin
            if (t <= EPSILON)
                return (false, float.MaxValue);

            return (true, t);
        }

        public Vector3D GetNormal(Vector3D intersectionPoint)
        {
            // For a flat triangle, the normal is constant regardless of the intersection point
            // Ensure normal faces the correct way relative to incoming ray
            return Normal;
        }
    }
}