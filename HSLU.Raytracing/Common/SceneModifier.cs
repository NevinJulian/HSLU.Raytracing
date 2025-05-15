namespace Common
{
    public class SceneModifier
    {
        public static Vector3D CalculateRefraction(Vector3D incident, Vector3D normal, float refractionRatio)
        {
            float cosI = -incident.Dot(normal);
            float sinI2 = 1.0f - cosI * cosI;
            float sinR2 = refractionRatio * refractionRatio * sinI2;

            if (sinR2 >= 1.0f)
            {
                return incident - normal * (2 * cosI);
            }

            float cosR = MathF.Sqrt(1.0f - sinR2);
            return (incident * refractionRatio) + (normal * (refractionRatio * cosI - cosR));
        }

        public static void AddModelTriangles(Scene scene, List<Triangle> triangles)
        {
            foreach (var triangle in triangles)
            {
                scene.AddObject(triangle);
            }
        }
    }
}