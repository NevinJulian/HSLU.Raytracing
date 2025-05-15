namespace Common
{
    public class PlaneObject : IRaycastable
    {
        public Vector3D Point { get; }
        public Vector3D Normal { get; }
        public MyColor Color { get; }
        public Material Material { get; }
        public int ObjectId { get; set; }

        public PlaneObject(Vector3D point, Vector3D normal, Material material)
        {
            Point = point;
            Normal = normal.Normalize();
            Material = material;
            Color = material.Diffuse;
        }

        public (bool hasHit, float intersectionDistance) Intersect(Ray ray)
        {
            float dotProduct = Normal.Dot(ray.Direction);

            if (Math.Abs(dotProduct) < 1e-6)
                return (false, float.MaxValue);

            Vector3D toPoint = Point - ray.Origin;
            float t = toPoint.Dot(Normal) / dotProduct;

            if (t < 0.0001f)
                return (false, float.MaxValue);

            return (true, t);
        }

        public Vector3D GetNormal(Vector3D intersectionPoint)
        {
            return Normal;
        }
    }
}