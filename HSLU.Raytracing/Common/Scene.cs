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

            // Special handling for Soap Bubbles
            MyColor objectColor;
            if (obj is SoapBubble soapBubble)
            {
                // Use the special interference color calculation for soap bubbles
                objectColor = soapBubble.GetInterferenceColor(hitPoint, -ray.Direction);
            }
            else
            {
                // Normal object color
                objectColor = obj.Color;
            }

            // Start with ambient light component
            MyColor ambient = material.Ambient;
            MyColor interferenceColor = obj is SoapBubble soap ? soap.GetInterferenceColor(hitPoint, -ray.Direction) : obj.Color;

            int red = (int)(ambient.R * 0.5f + interferenceColor.R * 0.5f);
            int green = (int)(ambient.G * 0.5f + interferenceColor.G * 0.5f);
            int blue = (int)(ambient.B * 0.5f + interferenceColor.B * 0.5f);

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
                    // Calculate diffuse lighting using Lambert's cosine law
                    float diffuseFactor = MathF.Max(0, cosAngle);

                    // Special handling for soap bubbles - they need their own color calculation
                    if (obj is SoapBubble)
                    {
                        // For soap bubbles, we already calculated the interference color
                        // Just apply the light intensity
                        red += (int)(objectColor.R * light.Intensity * diffuseFactor * light.Color.R / 255.0f);
                        green += (int)(objectColor.G * light.Intensity * diffuseFactor * light.Color.G / 255.0f);
                        blue += (int)(objectColor.B * light.Intensity * diffuseFactor * light.Color.B / 255.0f);
                    }
                    else
                    {
                        // For normal objects, use standard color calculation
                        red += (int)(objectColor.R * light.Intensity * diffuseFactor * light.Color.R / 255.0f);
                        green += (int)(objectColor.G * light.Intensity * diffuseFactor * light.Color.G / 255.0f);
                        blue += (int)(objectColor.B * light.Intensity * diffuseFactor * light.Color.B / 255.0f);
                    }

                    // Add specular highlight for shiny materials
                    if (material.Shininess > 0)
                    {
                        Vector3D reflection = CalculateReflection(-lightDirection, normal);
                        float specularFactor = MathF.Max(0, reflection.Dot(-ray.Direction));
                        specularFactor = MathF.Pow(specularFactor, material.Shininess * 128);
                        
                        red += (int)(material.Specular.R * light.Intensity * specularFactor * light.Color.R / 255.0f);
                        green += (int)(material.Specular.G * light.Intensity * specularFactor * light.Color.G / 255.0f);
                        blue += (int)(material.Specular.B * light.Intensity * specularFactor * light.Color.B / 255.0f);
                    }

                    if (obj is SoapBubble)
                    {
                        // Already using interference color - just boost it
                        float boostFactor = 1.25f;
                        red += (int)(objectColor.R * light.Intensity * diffuseFactor * light.Color.R / 255.0f * boostFactor);
                        green += (int)(objectColor.G * light.Intensity * diffuseFactor * light.Color.G / 255.0f * boostFactor);
                        blue += (int)(objectColor.B * light.Intensity * diffuseFactor * light.Color.B / 255.0f * boostFactor);
                    }
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

            // Add transparency component if we haven't reached the maximum depth
            float transparency = material.Transparency;
            if (transparency > 0 && depth < maxReflectionDepth)
            {
                // For soap bubbles, use a more accurate refraction model
                Ray transparencyRay;
                if (obj is SoapBubble)
                {
                    // Slightly perturb the ray direction for soap bubbles to simulate refraction effects
                    float refractionStrength = 0.05f; // Subtle perturbation
                    Vector3D perturbation = new Vector3D(
                        (float)(Math.Sin(hitPoint.X * 10) * refractionStrength),
                        (float)(Math.Sin(hitPoint.Y * 10) * refractionStrength),
                        (float)(Math.Sin(hitPoint.Z * 10) * refractionStrength)
                    );
                    
                    Vector3D refractedDir = (ray.Direction + perturbation).Normalize();
                    transparencyRay = new Ray(hitPoint, refractedDir);
                }
                else
                {
                    // Standard transparency for other objects
                    transparencyRay = new Ray(hitPoint, ray.Direction);
                }

                // Get the color from transparency ray
                MyColor transparencyColor = Trace(transparencyRay, depth + 1);

                if (obj is SoapBubble)
                {
                    transparencyColor = interferenceColor.Blend(transparencyColor, 0.7f);
                }

                // Blend the current color with the transparency color
                red = (int)(red * (1 - transparency) + transparencyColor.R * transparency);
                green = (int)(green * (1 - transparency) + transparencyColor.G * transparency);
                blue = (int)(blue * (1 - transparency) + transparencyColor.B * transparency);
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
                    // For soap bubbles, shadows are partial based on transparency
                    if (obj is SoapBubble soapBubble)
                    {
                        // Partial shadow based on bubble's transparency
                        return false; // Simplified - we'll let soap bubbles transmit light without casting shadows
                    }
                    
                    return true; // In shadow from a regular object
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