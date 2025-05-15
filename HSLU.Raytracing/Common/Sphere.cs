namespace Common
{
    public class Sphere : IRaycastable
    {
        public Vector3D Center { get; }
        public MyColor Color { get; }
        public Material Material { get; }
        public float Radius { get; }
        public int ObjectId { get; set; }

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
            Color = material.Diffuse;
            Material = material;
        }

        public bool IsInSphere(Vector2D point)
        {
            var dx = point.X - Center.X;
            var dy = point.Y - Center.Y;
            return (dx * dx + dy * dy) <= (Radius * Radius);
        }

        public (bool hasHit, float intersectionDistance) Intersect(Ray ray)
        {
            Vector3D oc = ray.Origin - Center;

            float a = ray.Direction.Dot(ray.Direction);
            float b = 2.0f * oc.Dot(ray.Direction);
            float c = oc.Dot(oc) - Radius * Radius;

            float discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
                return (false, float.MaxValue);

            float sqrt = MathF.Sqrt(discriminant);
            float t1 = (-b - sqrt) / (2 * a);
            float t2 = (-b + sqrt) / (2 * a);

            if (t1 > 0.0001f)
                return (true, t1);
            else if (t2 > 0.0001f)
                return (true, t2);
            else
                return (false, float.MaxValue);
        }

        public Vector3D GetNormal(Vector3D intersectionPoint)
        {
            return (intersectionPoint - Center).Normalize();
        }
    }
}