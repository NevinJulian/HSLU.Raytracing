namespace Common
{
    public class SceneModifier
    {
        // Helper method to calculate refraction vector
        public static Vector3D CalculateRefraction(Vector3D incident, Vector3D normal, float refractionRatio)
        {
            float cosI = -incident.Dot(normal);
            float sinI2 = 1.0f - cosI * cosI;
            float sinR2 = refractionRatio * refractionRatio * sinI2;

            // Check for total internal reflection
            if (sinR2 >= 1.0f)
            {
                // Total internal reflection - calculate reflection
                return incident - normal * (2 * cosI);
            }

            float cosR = MathF.Sqrt(1.0f - sinR2);
            return (incident * refractionRatio) + (normal * (refractionRatio * cosI - cosR));
        }

        // Helper to add triangles to a scene from an OBJ model
        public static void AddModelTriangles(Scene scene, List<Triangle> triangles)
        {
            foreach (var triangle in triangles)
            {
                scene.AddObject(triangle);
            }
        }
    }
}