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

        // Simplified version to avoid shadow problems with soap bubbles
        // This lets the bubbles show their colors more clearly
        private MyColor CalculateColor(HitInfo hitInfo, Ray ray, int depth)
        {
            IRaycastable obj = hitInfo.Object;
            Material material = obj.Material;
            Vector3D hitPoint = hitInfo.HitPoint;
            Vector3D normal = hitInfo.Normal;

            // Special handling for Soap Bubbles
            bool isSoapBubble = obj is SoapBubble;
            MyColor objectColor;

            if (isSoapBubble)
            {
                // For soap bubbles, use special color calculation
                SoapBubble soapBubble = (SoapBubble)obj;
                objectColor = soapBubble.GetInterferenceColor(hitPoint, -ray.Direction);
            }
            else
            {
                // Regular object color
                objectColor = obj.Color;
            }

            // Initialize color components with ambient light
            int red = (int)(material.Ambient.R);
            int green = (int)(material.Ambient.G);
            int blue = (int)(material.Ambient.B);

            // Add special handling for soap bubbles - completely skip shadows for them
            bool skipShadowCheck = isSoapBubble;

            foreach (Light light in lights)
            {
                // Calculate light vector and distance
                Vector3D lightVector = light.Position - hitPoint;
                float lightDistance = lightVector.Length;
                Vector3D lightDirection = lightVector * (1.0f / lightDistance); // Normalize

                // Calculate dot product with normal for diffuse lighting
                float cosAngle = normal.Dot(lightDirection);

                // For soap bubbles, we want to skip shadow checks to make them more visible
                bool inShadow = false;
                if (!skipShadowCheck)
                {
                    inShadow = IsPointInShadow(hitPoint, normal, lightDirection, lightDistance, obj.ObjectId);
                }

                if (!inShadow)
                {
                    // Calculate diffuse lighting using Lambert's cosine law
                    float diffuseFactor = MathF.Max(0, cosAngle);

                    if (isSoapBubble)
                    {
                        // For soap bubbles, we want to enhance the diffuse lighting
                        diffuseFactor = 0.5f + 0.5f * diffuseFactor; // Soften the falloff

                        // Apply diffuse lighting
                        red += (int)(objectColor.R * light.Intensity * diffuseFactor * light.Color.R / 255.0f);
                        green += (int)(objectColor.G * light.Intensity * diffuseFactor * light.Color.G / 255.0f);
                        blue += (int)(objectColor.B * light.Intensity * diffuseFactor * light.Color.B / 255.0f);
                    }
                    else
                    {
                        // Regular diffuse calculation for other objects
                        red += (int)(obj.Color.R * light.Intensity * diffuseFactor * light.Color.R / 255.0f);
                        green += (int)(obj.Color.G * light.Intensity * diffuseFactor * light.Color.G / 255.0f);
                        blue += (int)(obj.Color.B * light.Intensity * diffuseFactor * light.Color.B / 255.0f);
                    }

                    // Add specular highlights
                    if (material.Shininess > 0)
                    {
                        Vector3D reflection = CalculateReflection(-lightDirection, normal);
                        float specularFactor = MathF.Max(0, reflection.Dot(-ray.Direction));

                        if (isSoapBubble)
                        {
                            // Sharper specular for soap bubbles
                            specularFactor = MathF.Pow(specularFactor, 64);

                            // Enhance the specular highlights for better visibility
                            float specularMultiplier = 2.0f;

                            // Add specular highlight
                            red += (int)(material.Specular.R * light.Intensity * specularFactor * light.Color.R / 255.0f * specularMultiplier);
                            green += (int)(material.Specular.G * light.Intensity * specularFactor * light.Color.G / 255.0f * specularMultiplier);
                            blue += (int)(material.Specular.B * light.Intensity * specularFactor * light.Color.B / 255.0f * specularMultiplier);
                        }
                        else
                        {
                            // Regular specular for other objects
                            specularFactor = MathF.Pow(specularFactor, material.Shininess * 128);

                            // Add specular highlight
                            red += (int)(material.Specular.R * light.Intensity * specularFactor * light.Color.R / 255.0f);
                            green += (int)(material.Specular.G * light.Intensity * specularFactor * light.Color.G / 255.0f);
                            blue += (int)(material.Specular.B * light.Intensity * specularFactor * light.Color.B / 255.0f);
                        }
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

                if (isSoapBubble)
                {
                    // Calculate Fresnel factor (more reflective at glancing angles)
                    float viewDot = MathF.Abs(normal.Dot(-ray.Direction));
                    float fresnelFactor = MathF.Pow(1.0f - viewDot, 5.0f);

                    // Boost reflectivity at edges using Fresnel
                    float effectiveReflectivity = reflectivity + (1.0f - reflectivity) * fresnelFactor;

                    // Apply reflection
                    red = (int)(red * (1.0f - effectiveReflectivity) + reflectionColor.R * effectiveReflectivity);
                    green = (int)(green * (1.0f - effectiveReflectivity) + reflectionColor.G * effectiveReflectivity);
                    blue = (int)(blue * (1.0f - effectiveReflectivity) + reflectionColor.B * effectiveReflectivity);
                }
                else
                {
                    // Standard reflection for other objects
                    red = (int)(red * (1.0f - reflectivity) + reflectionColor.R * reflectivity);
                    green = (int)(green * (1.0f - reflectivity) + reflectionColor.G * reflectivity);
                    blue = (int)(blue * (1.0f - reflectivity) + reflectionColor.B * reflectivity);
                }
            }

            // Add transparency component if we haven't reached the maximum depth
            float transparency = material.Transparency;
            if (transparency > 0 && depth < maxReflectionDepth)
            {
                // Create ray for transparency
                Ray transparencyRay;

                if (isSoapBubble)
                {
                    // Use slightly simplified refraction for better performance
                    // Just use the incident direction with a small random perturbation
                    Vector3D refractedDir = ray.Direction;

                    // Add a very small random perturbation
                    Random random = new Random(obj.ObjectId * 1000 + (int)(hitPoint.X * 100 + hitPoint.Y * 10 + hitPoint.Z));
                    Vector3D perturbation = new Vector3D(
                        (float)(random.NextDouble() * 0.005 - 0.0025),
                        (float)(random.NextDouble() * 0.005 - 0.0025),
                        (float)(random.NextDouble() * 0.005 - 0.0025)
                    );

                    refractedDir = (refractedDir + perturbation).Normalize();
                    transparencyRay = new Ray(hitPoint, refractedDir);
                }
                else
                {
                    // Standard transparency for other objects
                    transparencyRay = new Ray(hitPoint, ray.Direction);
                }

                // Get the color from transparency ray
                MyColor transparencyColor = Trace(transparencyRay, depth + 1);

                if (isSoapBubble)
                {
                    // Calculate view-dependent transparency
                    float viewDot = MathF.Abs(normal.Dot(-ray.Direction));
                    float fresnelFactor = MathF.Pow(1.0f - viewDot, 5.0f);

                    // Less transparent at glancing angles (Fresnel effect)
                    float effectiveTransparency = transparency * (1.0f - fresnelFactor * 0.8f);

                    // Apply transparency
                    red = (int)(red * (1.0f - effectiveTransparency) + transparencyColor.R * effectiveTransparency);
                    green = (int)(green * (1.0f - effectiveTransparency) + transparencyColor.G * effectiveTransparency);
                    blue = (int)(blue * (1.0f - effectiveTransparency) + transparencyColor.B * effectiveTransparency);
                }
                else
                {
                    // Standard transparency for other objects
                    red = (int)(red * (1.0f - transparency) + transparencyColor.R * transparency);
                    green = (int)(green * (1.0f - transparency) + transparencyColor.G * transparency);
                    blue = (int)(blue * (1.0f - transparency) + transparencyColor.B * transparency);
                }
            }

            // For soap bubbles, apply a final brightness boost
            if (isSoapBubble)
            {
                float boostFactor = 1.2f;
                red = (int)(red * boostFactor);
                green = (int)(green * boostFactor);
                blue = (int)(blue * boostFactor);
            }

            // Clamp RGB values to valid range [0-255]
            red = Math.Clamp(red, 0, 255);
            green = Math.Clamp(green, 0, 255);
            blue = Math.Clamp(blue, 0, 255);

            return new MyColor(red, green, blue);
        }

        // Modified shadow test for soap bubbles
        private bool IsPartiallyInShadow(Vector3D hitPoint, Vector3D normal, Vector3D lightDir, float lightDistance, int sourceObjectId)
        {
            // Use a more relaxed epsilon for transparent soap bubbles
            const float BASE_EPSILON = 0.15f;

            // Calculate an adaptive offset similar to regular shadow test
            float cosAngle = Math.Max(0.01f, normal.Dot(lightDir));
            float adaptiveOffset = BASE_EPSILON / cosAngle;
            adaptiveOffset = Math.Min(adaptiveOffset, 0.6f);

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
                    // If the shadowing object is a soap bubble, it casts only partial shadow
                    if (obj is SoapBubble)
                    {
                        // Soap bubbles are mostly transparent, so they don't cast strong shadows
                        // Return false to allow light through
                        return false;
                    }

                    // Otherwise it's a solid object casting a full shadow
                    return true;
                }
            }

            return false; // Not in shadow
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