namespace Common
{
    public class Ray
    {
        public Vector3D Origin { get; }
        public Vector3D Direction { get; }

        public Ray(Vector3D origin, Vector3D direction)
        {
            Origin = origin;
            Direction = direction.Normalize();
        }

        public Vector3D GetPointAt(float distance)
        {
            return Origin + Direction * distance;
        }
    }
}
