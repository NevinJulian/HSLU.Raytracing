using System;

namespace Common
{
    public class SoapBubble : IRaycastable
    {
        public Vector3D Center { get; }
        public MyColor Color { get; }
        public Material Material { get; }
        public float Radius { get; }
        public int ObjectId { get; set; }

        // Soap bubble specific properties
        private readonly float age;             // Age parameter (affects the interference pattern)
        private readonly float iridescence;     // Intensity of the iridescence effect (0-1)
        private readonly float filmVariation;   // Variation in film thickness (0-1)

        public SoapBubble(Vector3D center, float radius, Material material, float age = 0.5f,
                          float iridescence = 0.8f, float filmVariation = 0.7f)
        {
            Center = center;
            Radius = radius;
            Material = material;
            Color = material.Diffuse;
            this.age = age;
            this.iridescence = iridescence;
            this.filmVariation = filmVariation;
        }

        public (bool hasHit, float intersectionDistance) Intersect(Ray ray)
        {
            // Vector from ray origin to sphere center
            Vector3D oc = ray.Origin - Center;

            // Quadratic equation coefficients
            float a = ray.Direction.Dot(ray.Direction);
            float b = 2.0f * oc.Dot(ray.Direction);
            float c = oc.Dot(oc) - Radius * Radius;

            // Calculate discriminant
            float discriminant = b * b - 4 * a * c;

            // No intersection if discriminant is negative
            if (discriminant < 0)
                return (false, float.MaxValue);

            // Calculate the two intersection points
            float sqrt = MathF.Sqrt(discriminant);
            float t1 = (-b - sqrt) / (2 * a);
            float t2 = (-b + sqrt) / (2 * a);

            // Return the closest positive intersection
            if (t1 > 0.0001f)
                return (true, t1);
            else if (t2 > 0.0001f)
                return (true, t2);
            else
                return (false, float.MaxValue);
        }

        public Vector3D GetNormal(Vector3D intersectionPoint)
        {
            // Normal is the vector from center to intersection point, normalized
            return (intersectionPoint - Center).Normalize();
        }

        // Calculate the thin-film interference color at a specific point on the bubble
        public MyColor GetInterferenceColor(Vector3D intersectionPoint, Vector3D viewDirection)
        {
            // Calculate angle between normal and view direction (determines the interference color)
            Vector3D normal = GetNormal(intersectionPoint);
            float cosAngle = Math.Abs(normal.Dot(viewDirection));

            // Calculate the "thickness" based on position and bubble parameters
            // We use various trig functions to create patterns similar to real soap bubbles

            // Normalize point on sphere surface for pattern calculation
            Vector3D normalizedPoint = (intersectionPoint - Center).Normalize();

            // Calculate a varying film thickness based on position on the bubble
            float x = normalizedPoint.X;
            float y = normalizedPoint.Y;
            float z = normalizedPoint.Z;

            // Create swirling patterns based on position
            float theta = (float)Math.Atan2(y, x);
            float phi = (float)Math.Acos(z / Radius);

            // Vary thickness with position using wave patterns
            float swirl = 0.3f * MathF.Sin(15 * x + age * 20)
            + 0.3f * MathF.Cos(10 * y + age * 10)
            + 0.3f * MathF.Sin(20 * z + age * 15)
            + 0.2f * MathF.Sin(30 * (x + y + z) + age * 5);

            float thickness = 0.5f + 0.5f * swirl;

            // Add some noise and variation
            thickness += 0.2f * (float)Math.Sin(20 * normalizedPoint.X * normalizedPoint.Y + age * 15);
            thickness += 0.15f * (float)Math.Cos(15 * normalizedPoint.Y * normalizedPoint.Z + age * 10);
            thickness += 0.1f * (float)Math.Sin(25 * normalizedPoint.X * normalizedPoint.Z + age * 5);

            float bubbleNoise = (float)(Math.Sin(age * 123.456 + x * 17.3f + y * 41.2f) * 0.2f);
            thickness += bubbleNoise;

            // Scale by film variation parameter
            thickness *= filmVariation;

            // Normalize to 0-1 range
            thickness = (thickness + 1) * 0.5f;
            thickness = (float)(1.0 / (1.0 + Math.Exp(-5 * (thickness - 0.5f))));

            // Viewing angle affects apparent thickness (optical path length)
            float effectiveThickness = thickness / cosAngle;

            // Calculate RGB colors based on simplified thin-film interference
            // In real soap bubbles, different wavelengths interfere constructively/destructively
            // at different film thicknesses, creating the rainbow pattern

            // Phase shifts for R, G, B components (these create the color separation)
            float phaseR = effectiveThickness * 6.0f;
            float phaseG = effectiveThickness * 6.0f + 2.0f;
            float phaseB = effectiveThickness * 6.0f + 4.0f;

            // Calculate interference pattern for each color component
            float r = (float)Math.Pow(Math.Sin(phaseR) * 0.5f + 0.5f, 2);
            float g = (float)Math.Pow(Math.Sin(phaseG) * 0.5f + 0.5f, 2);
            float b = (float)Math.Pow(Math.Sin(phaseB) * 0.5f + 0.5f, 2);

            r = MathF.Pow(r, 1.2f);
            g = MathF.Pow(g, 1.2f);
            b = MathF.Pow(b, 1.2f);

            // Scale by iridescence factor and convert to 0-255 range
            int red = (int)(r * 255 * iridescence);
            int green = (int)(g * 255 * iridescence);
            int blue = (int)(b * 255 * iridescence);

            // Add a base white component to ensure some light always reflects
            int baseLight = (int)(255 * (1 - iridescence) * 0.8f);
            red += baseLight;
            green += baseLight;
            blue += baseLight;

            // Clamp to valid range
            red = Math.Clamp(red, 0, 255);
            green = Math.Clamp(green, 0, 255);
            blue = Math.Clamp(blue, 0, 255);

            return new MyColor(red, green, blue);
        }
    }
}