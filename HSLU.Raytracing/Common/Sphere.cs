namespace Common
{
    public class Sphere : IRaycastable
    {
        public Vector3D Center { get; }
        public MyColor Color { get; }
        public Material Material { get; }
        public float Radius { get; }

        public Sphere(Vector3D center, float radius, MyColor color, MaterialType materialType = MaterialType.RED_PLASTIC, float reflectivity = 0f)
        {
            Center = center;
            Radius = radius;
            Color = color;
            Material = Common.Material.Create(materialType, reflectivity);
        }

        public Sphere(Vector3D center, float radius, Material material)
        {
            Center = center;
            Radius = radius;
            Color = material.Diffuse; // Use diffuse color as the main color
            Material = material;
        }

        // Method to maintain backward compatibility with existing code
        public bool IsInSphere(Vector2D point)
        {
            var dx = point.X - Center.X;
            var dy = point.Y - Center.Y;
            return (dx * dx + dy * dy) <= (Radius * Radius);
        }

        public (bool hasHit, float intersectionDistance) Intersect(Ray ray)
        {
            // Vector from ray origin to sphere center
            Vector3D oc = ray.Origin - Center;

            // Quadratic equation coefficients
            float a = ray.Direction.Dot(ray.Direction);
            float b = 2.0f * oc.Dot(ray.Direction);
            float c = oc.Dot(oc) - Radius * Radius;

            // Calculate discriminant
            float discriminant = b * b - 4 * a * c;

            // No intersection if discriminant is negative
            if (discriminant < 0)
                return (false, float.MaxValue);

            // Calculate the two intersection points
            float sqrt = MathF.Sqrt(discriminant);
            float t1 = (-b - sqrt) / (2 * a);
            float t2 = (-b + sqrt) / (2 * a);

            // Return the closest positive intersection
            if (t1 > 0.0001f)
                return (true, t1);
            else if (t2 > 0.0001f)
                return (true, t2);
            else
                return (false, float.MaxValue);
        }

        public Vector3D GetNormal(Vector3D intersectionPoint)
        {
            // Normal is the vector from center to intersection point, normalized
            return (intersectionPoint - Center).Normalize();
        }
    }
}