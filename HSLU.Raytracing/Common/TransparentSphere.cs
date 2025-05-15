namespace Common
{
    public class TransparentSphere : IRaycastable
    {
        public Vector3D Center { get; }
        public MyColor Color { get; }
        public Material Material { get; }
        public float Radius { get; }
        public int ObjectId { get; set; }

        public TransparentSphere(Vector3D center, float radius, Material material)
        {
            Center = center;
            Radius = radius;
            Color = material.Diffuse;
            Material = material;
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