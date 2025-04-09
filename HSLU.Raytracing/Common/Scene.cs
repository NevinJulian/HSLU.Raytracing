namespace Common
{
    public class Scene
    {
        private readonly List<IRaycastable> objects;
        private readonly List<Light> lights;
        private static readonly MyColor BACKGROUND_COLOR = MyColor.Black;
        private int maxReflectionDepth = 10; // Default value

        public Scene()
        {
            objects = new List<IRaycastable>();
            lights = new List<Light>();
        }

        public void AddObject(IRaycastable obj)
        {
            objects.Add(obj);
        }

        public void AddLight(Light light)
        {
            lights.Add(light);
        }

        public MyColor Trace(Ray ray)
        {
            return Trace(ray, 0);
        }

        private MyColor Trace(Ray ray, int depth)
        {
            HitInfo? hitInfo = FindClosestIntersection(ray);

            if (hitInfo != null)
            {
                return CalculateColor(hitInfo, ray, depth);
            }
            return BACKGROUND_COLOR;
        }

        private HitInfo? FindClosestIntersection(Ray ray)
        {
            HitInfo? closestHit = null;
            float closestDistance = float.MaxValue;

            foreach (IRaycastable obj in objects)
            {
                var (hasHit, distance) = obj.Intersect(ray);
                if (hasHit && distance > 0.001f && distance < closestDistance)
                {
                    Vector3D hitPoint = ray.GetPointAt(distance);
                    Vector3D normal = obj.GetNormal(hitPoint);
                    closestHit = new HitInfo(obj, hitPoint, normal, distance);
                    closestDistance = distance;
                }
            }

            return closestHit;
        }

        private MyColor CalculateColor(HitInfo hitInfo, Ray ray, int depth)
        {
            IRaycastable obj = hitInfo.Object;
            Material material = obj.Material;
            Vector3D hitPoint = hitInfo.HitPoint;
            Vector3D normal = hitInfo.Normal;
            Vector3D viewDirection = (ray.Origin - hitPoint).Normalize();

            // Start with ambient light component
            MyColor ambient = material.Ambient;
            int red = ambient.R;
            int green = ambient.G;
            int blue = ambient.B;

            // Add contribution from each light source (simplified lighting model)
            foreach (Light light in lights)
            {
                // Create a vector from the hit point to the light source
                Vector3D lightDirection = (light.Position - hitPoint).Normalize();

                // Check for shadows
                bool inShadow = IsInShadow(hitPoint, lightDirection, light.Position);

                if (!inShadow)
                {
                    // Calculate diffuse lighting using Lambert's cosine law
                    float diffuseFactor = MathF.Max(0, normal.Dot(lightDirection));

                    // Simple diffuse lighting - using object's color directly
                    red += (int)(obj.Color.R * light.Intensity * diffuseFactor * light.Color.R / 255.0f);
                    green += (int)(obj.Color.G * light.Intensity * diffuseFactor * light.Color.G / 255.0f);
                    blue += (int)(obj.Color.B * light.Intensity * diffuseFactor * light.Color.B / 255.0f);
                }
            }

            // Add reflection component if we haven't reached the maximum depth
            float reflectivity = material.Reflectivity;
            if (reflectivity > 0 && depth < maxReflectionDepth)
            {
                Vector3D reflectionDir = CalculateReflection(ray.Direction, normal);
                Ray reflectionRay = new Ray(hitPoint, reflectionDir);

                // Get the color from the reflection ray
                MyColor reflectionColor = Trace(reflectionRay, depth + 1);

                // Add reflection component weighted by reflectivity
                red = (int)(red * (1 - reflectivity) + reflectionColor.R * reflectivity);
                green = (int)(green * (1 - reflectivity) + reflectionColor.G * reflectivity);
                blue = (int)(blue * (1 - reflectivity) + reflectionColor.B * reflectivity);
            }

            // Clamp RGB values to valid range [0-255]
            red = Math.Clamp(red, 0, 255);
            green = Math.Clamp(green, 0, 255);
            blue = Math.Clamp(blue, 0, 255);

            return new MyColor(red, green, blue);
        }

        // Helper method to calculate reflection vector
        private Vector3D CalculateReflection(Vector3D incident, Vector3D normal)
        {
            float dot = incident.Dot(normal);
            return incident - normal * (2 * dot);
        }

        private bool IsInShadow(Vector3D hitPoint, Vector3D lightDirection, Vector3D lightPosition)
        {
            // Create a ray from hit point toward light
            Ray shadowRay = new Ray(hitPoint, lightDirection);

            // Calculate distance to light
            float distanceToLight = (lightPosition - hitPoint).Length;

            // Check if any object blocks the light
            foreach (IRaycastable obj in objects)
            {
                var (hasHit, distance) = obj.Intersect(shadowRay);
                // Only count as shadow if the object is between the hit point and the light
                if (hasHit && distance > 0.001f && distance < distanceToLight)
                {
                    return true; // This point is in shadow
                }
            }
            return false;
        }

        public void SetMaxReflectionDepth(int depth)
        {
            maxReflectionDepth = depth;
        }

        public int GetMaxReflectionDepth()
        {
            return maxReflectionDepth;
        }
    }
}