namespace Common
{
    public class Scene
    {
        private readonly List<IRaycastable> objects;
        private readonly List<Light> lights;
        private static readonly MyColor BACKGROUND_COLOR = MyColor.Black;
        private int maxReflectionDepth = 10; // Default value
        private int nextObjectId = 0;

        public Scene()
        {
            objects = new List<IRaycastable>();
            lights = new List<Light>();
        }

        public void AddObject(IRaycastable obj)
        {
            obj.ObjectId = nextObjectId++;
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
                if (hasHit && distance > 0.05f && distance < closestDistance)
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

            // Start with ambient light component
            MyColor ambient = material.Ambient;
            int red = ambient.R;
            int green = ambient.G;
            int blue = ambient.B;

            foreach (Light light in lights)
            {
                // Calculate light vector and distance
                Vector3D lightVector = light.Position - hitPoint;
                float lightDistance = lightVector.Length;
                Vector3D lightDirection = lightVector * (1.0f / lightDistance); // Normalize

                // Calculate dot product with normal (but don't skip negative values yet)
                float cosAngle = normal.Dot(lightDirection);

                // Use the robust shadow test that fixes artifacts
                bool inShadow = IsPointInShadow(hitPoint, normal, lightDirection, lightDistance, obj.ObjectId);

                if (!inShadow)
                {
                    // Calculate diffuse lighting using Lambert's cosine law - same as original
                    float diffuseFactor = MathF.Max(0, cosAngle);

                    // IMPORTANT: Use obj.Color instead of material.Diffuse to match original colors
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

        private bool IsInShadow(Vector3D hitPoint, Vector3D normal, Vector3D lightDirection, Vector3D lightPosition, int originObjectId)
        {
            // Use a more aggressive offset for shadow rays
            float shadowBias = 0.15f;

            // Offset in both normal and light directions for maximum robustness
            Vector3D offsetPoint = hitPoint + normal * shadowBias + lightDirection * 0.05f;

            Ray shadowRay = new Ray(offsetPoint, lightDirection);
            float distanceToLight = (lightPosition - offsetPoint).Length;

            foreach (IRaycastable obj in objects)
            {
                // Skip the object that the ray hit originally
                if (obj.ObjectId == originObjectId)
                    continue;

                // Skip triangles that belong to the same parent object (cube)
                if (obj is Triangle triangle && triangle.ParentId == originObjectId)
                    continue;

                var (hasHit, distance) = obj.Intersect(shadowRay);
                if (hasHit && distance > 0.001f && distance < distanceToLight)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsPointInShadow(Vector3D hitPoint, Vector3D normal, Vector3D lightDir, float lightDistance, int sourceObjectId)
        {
            // Very robust offset calculations
            const float BASE_EPSILON = 0.1f; // A larger base epsilon

            // Calculate a robust offset that:
            // 1. Is larger when the light direction is almost perpendicular to the normal
            // 2. Has a minimum safe value based on scene scale
            float cosAngle = Math.Max(0.01f, normal.Dot(lightDir)); // Avoid division by zero
            float adaptiveOffset = BASE_EPSILON / cosAngle;
            adaptiveOffset = Math.Min(adaptiveOffset, 0.5f); // Cap maximum offset

            // Offset the ray origin along both normal and light direction
            Vector3D offsetPoint = hitPoint + normal * adaptiveOffset + lightDir * 0.01f;

            // Create a ray from the offset point toward the light
            Ray shadowRay = new Ray(offsetPoint, lightDir);

            // Test against all objects
            foreach (IRaycastable obj in objects)
            {
                // Skip self-intersection
                if (obj.ObjectId == sourceObjectId)
                    continue;

                var (hasHit, distance) = obj.Intersect(shadowRay);

                // Only count intersections between us and the light
                if (hasHit && distance > 0.001f && distance < lightDistance - adaptiveOffset)
                {
                    return true; // In shadow
                }
            }

            return false; // Not in shadow
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