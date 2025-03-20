namespace Common
{
    public class Plane
    {
        public Vector3D Point { get; }
        public Vector3D Normal { get; }
        public MyColor Color { get; }

        public Plane(Vector3D point, Vector3D normal, MyColor color)
        {
            Point = point;
            Normal = normal.Normalize();
            Color = color;
        }

        public (bool hit, Vector3D intersection) IntersectRay(Vector3D rayOrigin, Vector3D rayDirection)
        {
            // Calculate dot product between normal and ray direction
            float dotProduct = Normal.Dot(rayDirection);

            // If ray is parallel to the plane
            if (Math.Abs(dotProduct) < 1e-6)
            {
                return (false, new Vector3D(0, 0, 0));
            }

            // Calculate distance to plane
            Vector3D toPoint = new Vector3D(
                Point.X - rayOrigin.X,
                Point.Y - rayOrigin.Y,
                Point.Z - rayOrigin.Z
            );

            float t = Normal.Dot(toPoint) / dotProduct;

            // If plane is behind the ray
            if (t < 0)
            {
                return (false, new Vector3D(0, 0, 0));
            }

            // Calculate intersection point
            Vector3D intersection = new Vector3D(
                rayOrigin.X + t * rayDirection.X,
                rayOrigin.Y + t * rayDirection.Y,
                rayOrigin.Z + t * rayDirection.Z
            );

            return (true, intersection);
        }
    }
}
