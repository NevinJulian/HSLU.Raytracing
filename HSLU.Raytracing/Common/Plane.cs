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
            float dotProduct = Normal.Dot(rayDirection);

            if (Math.Abs(dotProduct) < 1e-6)
            {
                return (false, new Vector3D(0, 0, 0));
            }

            Vector3D toPoint = new Vector3D(
                Point.X - rayOrigin.X,
                Point.Y - rayOrigin.Y,
                Point.Z - rayOrigin.Z
            );

            float t = Normal.Dot(toPoint) / dotProduct;

            if (t < 0)
            {
                return (false, new Vector3D(0, 0, 0));
            }

            Vector3D intersection = new Vector3D(
                rayOrigin.X + t * rayDirection.X,
                rayOrigin.Y + t * rayDirection.Y,
                rayOrigin.Z + t * rayDirection.Z
            );

            return (true, intersection);
        }

        public Vector3D GetNormal(Vector3D intersectionPoint)
        {
            return Normal;
        }
    }
}
