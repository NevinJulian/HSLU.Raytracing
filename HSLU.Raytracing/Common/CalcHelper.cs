namespace Common
{
    public static class CalcHelper
    {
        public static Vector3D IntersectionPoint(Ray ray, double distance)
        {
            return ray.Origin + ray.Direction * distance;
        }
    }
}
