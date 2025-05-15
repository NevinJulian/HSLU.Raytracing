namespace Common
{
    public class Camera
    {
        private readonly Vector3D position;

        public Camera(Vector3D position)
        {
            this.position = position;
        }

        public static Camera CreateDefault()
        {
            return new Camera(new Vector3D(400, 300, -200));
        }

        public Ray CreateRay(float nx, float ny)
        {
            Vector3D direction = new Vector3D(nx, ny, 1).Normalize();
            return new Ray(position, direction);
        }
    }
}