using System;
using System.Collections.Generic;

namespace Common
{
    public class OptimizedScene
    {
        private readonly List<IRaycastable> objects;
        private readonly List<Light> lights;
        private static readonly MyColor BACKGROUND_COLOR = MyColor.Black;
        private int maxReflectionDepth = 10; // Default value
        private int nextObjectId = 0;
        private BVHAccelerator bvhAccelerator;
        private bool useAcceleration = true;

        public OptimizedScene()
        {
            objects = new List<IRaycastable>();
            lights = new List<Light>();
        }

        public void AddObject(IRaycastable obj)
        {
            obj.ObjectId = nextObjectId++;
            objects.Add(obj);

            // Mark that we need to rebuild the BVH
            bvhAccelerator = null;
        }

        public void AddLight(Light light)
        {
            lights.Add(light);
        }

        // Build or rebuild the BVH acceleration structure
        public void BuildAccelerationStructure()
        {
            bvhAccelerator = new BVHAccelerator(objects);
            Console.WriteLine($"Built BVH with {objects.Count} objects");
        }

        // Enable or disable acceleration
        public void SetAcceleration(bool enabled)
        {
            useAcceleration = enabled;
            if (enabled && bvhAccelerator == null && objects.Count > 0)
            {
                BuildAccelerationStructure();
            }
        }

        public MyColor Trace(Ray ray)
        {
            return Trace(ray, 0);
        }

        private MyColor Trace(Ray ray, int depth)
        {
            HitInfo hitInfo = FindClosestIntersection(ray);

            if (hitInfo != null)
            {
                return CalculateColor(hitInfo, ray, depth);
            }
            return BACKGROUND_COLOR;
        }

        private HitInfo FindClosestIntersection(Ray ray)
        {
            // Use BVH if enabled and built
            if (useAcceleration && bvhAccelerator != null)
            {
                return bvhAccelerator.FindClosestIntersection(ray);
            }

            // Fall back to brute-force method
            HitInfo closestHit = null;
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

            // Special handling for glass sphere
            bool isGlassSphere = obj is GlassSphere;

            // Initialize color components with ambient light
            int red = (int)(material.Ambient.R);
            int green = (int)(material.Ambient.G);
            int blue = (int)(material.Ambient.B);

            foreach (Light light in lights)
            {
                // Calculate light vector and distance
                Vector3D lightVector = light.Position - hitPoint;
                float lightDistance = lightVector.Length;
                Vector3D lightDirection = lightVector * (1.0f / lightDistance); // Normalize

                // Calculate dot product with normal for diffuse lighting
                float cosAngle = normal.Dot(lightDirection);

                // Shadow check - for glass we make it partial
                bool inShadow = false;
                if (!isGlassSphere) // Standard shadow test for non-glass objects
                {
                    inShadow = IsPointInShadow(hitPoint, normal, lightDirection, lightDistance, obj.ObjectId);
                }
                else
                {
                    // MODIFIED: For glass sphere, use special shadow calculation that allows more light through
                    inShadow = false; // Glass lets most light through
                }

                if (!inShadow || isGlassSphere) // Glass lets some light through
                {
                    // Calculate diffuse lighting using Lambert's cosine law
                    float diffuseFactor = Math.Max(0, cosAngle);

                    // MODIFIED: Scale down diffuse for glass for a more realistic look
                    if (isGlassSphere)
                    {
                        diffuseFactor *= 0.2f; // Further reduced diffuse component for glass (was 0.3f)
                    }

                    // Apply diffuse lighting
                    red += (int)(obj.Color.R * light.Intensity * diffuseFactor * light.Color.R / 255.0f);
                    green += (int)(obj.Color.G * light.Intensity * diffuseFactor * light.Color.G / 255.0f);
                    blue += (int)(obj.Color.B * light.Intensity * diffuseFactor * light.Color.B / 255.0f);

                    // Add specular highlights
                    if (material.Shininess > 0)
                    {
                        Vector3D reflection = CalculateReflection(-lightDirection, normal);
                        float specularFactor = Math.Max(0, reflection.Dot(-ray.Direction));

                        // MODIFIED: Sharper specular for glass
                        float shininessPower = material.Shininess * 128;
                        if (isGlassSphere)
                        {
                            shininessPower = 300; // Even sharper highlights for glass (was 256)
                        }

                        specularFactor = (float)Math.Pow(specularFactor, shininessPower);

                        // Add specular highlight
                        red += (int)(material.Specular.R * light.Intensity * specularFactor * light.Color.R / 255.0f);
                        green += (int)(material.Specular.G * light.Intensity * specularFactor * light.Color.G / 255.0f);
                        blue += (int)(material.Specular.B * light.Intensity * specularFactor * light.Color.B / 255.0f);
                    }
                }
            }

            // Add reflection component if we haven't reached the maximum depth
            float reflectivity = material.Reflectivity;
            MyColor reflectionColor = MyColor.Black; // Initialize

            if (reflectivity > 0 && depth < maxReflectionDepth)
            {
                Vector3D reflectionDir = CalculateReflection(ray.Direction, normal);
                Ray reflectionRay = new Ray(hitPoint + normal * 0.01f, reflectionDir);

                // Get the color from the reflection ray
                reflectionColor = Trace(reflectionRay, depth + 1);

                // Standard reflection for non-glass objects
                if (!isGlassSphere)
                {
                    red = (int)(red * (1.0f - reflectivity) + reflectionColor.R * reflectivity);
                    green = (int)(green * (1.0f - reflectivity) + reflectionColor.G * reflectivity);
                    blue = (int)(blue * (1.0f - reflectivity) + reflectionColor.B * reflectivity);
                }
            }

            // Add transparency/refraction component if we haven't reached the maximum depth
            float transparency = material.Transparency;
            if (transparency > 0 && depth < maxReflectionDepth)
            {
                if (isGlassSphere)
                {
                    // MODIFIED: For glass sphere, use improved refraction calculation
                    GlassSphere glassSphere = (GlassSphere)obj;

                    // Calculate refracted direction
                    Vector3D refractedDir = CalculateRefraction(ray.Direction, normal, glassSphere.RefractionIndex);

                    // Small offset to avoid self-intersection - MODIFIED: increased offset slightly
                    Ray refractedRay = new Ray(hitPoint + refractedDir * 0.015f, refractedDir);

                    // Trace the refracted ray
                    MyColor refractedColor = Trace(refractedRay, depth + 1);

                    // MODIFIED: Calculate Fresnel factor with adjusted parameters
                    float r0 = (1.0f - glassSphere.RefractionIndex) / (1.0f + glassSphere.RefractionIndex);
                    r0 = r0 * r0;

                    float viewDot = Math.Abs(normal.Dot(-ray.Direction));
                    float fresnelFactor = r0 + (1.0f - r0) * (float)Math.Pow(1.0f - viewDot, 5.0f);

                    // Reduce Fresnel effect slightly to allow more light through
                    fresnelFactor *= 0.85f;

                    // If we haven't calculated reflection yet, do it now
                    if (reflectivity <= 0)
                    {
                        Vector3D reflectionDir = CalculateReflection(ray.Direction, normal);
                        Ray reflectionRay = new Ray(hitPoint + normal * 0.01f, reflectionDir);
                        reflectionColor = Trace(reflectionRay, depth + 1);
                    }

                    // MODIFIED: Mix colors with adjusted parameters for better transparency
                    float effectiveReflectivity = Math.Max(reflectivity * 0.7f, fresnelFactor * 0.7f);
                    float effectiveTransparency = transparency * (1.0f - fresnelFactor * 0.5f);

                    // MODIFIED: Blend colors with more weight on transparency
                    float totalEffect = effectiveReflectivity + effectiveTransparency;
                    float remainingColor = Math.Max(0, 1.0f - totalEffect);

                    red = (int)(red * remainingColor +
                               reflectionColor.R * effectiveReflectivity +
                               refractedColor.R * effectiveTransparency);

                    green = (int)(green * remainingColor +
                                 reflectionColor.G * effectiveReflectivity +
                                 refractedColor.G * effectiveTransparency);

                    blue = (int)(blue * remainingColor +
                                reflectionColor.B * effectiveReflectivity +
                                refractedColor.B * effectiveTransparency);
                }
                else
                {
                    // Standard transparency for other objects
                    Ray transparencyRay = new Ray(hitPoint + ray.Direction * 0.01f, ray.Direction);
                    MyColor transparencyColor = Trace(transparencyRay, depth + 1);

                    red = (int)(red * (1.0f - transparency) + transparencyColor.R * transparency);
                    green = (int)(green * (1.0f - transparency) + transparencyColor.G * transparency);
                    blue = (int)(blue * (1.0f - transparency) + transparencyColor.B * transparency);
                }
            }

            // Clamp RGB values to valid range [0-255]
            red = ClampValue(red, 0, 255);
            green = ClampValue(green, 0, 255);
            blue = ClampValue(blue, 0, 255);

            return new MyColor(red, green, blue);
        }

        // Helper method to calculate reflection vector
        private Vector3D CalculateReflection(Vector3D incident, Vector3D normal)
        {
            float dot = incident.Dot(normal);
            return incident - normal * (2 * dot);
        }

        // Calculate refraction direction using Snell's law
        private Vector3D CalculateRefraction(Vector3D incident, Vector3D normal, float indexOfRefraction)
        {
            // Custom clamp function since MathF.Clamp might not be available in all .NET versions
            float cosi = ClampValue(incident.Dot(normal), -1.0f, 1.0f);
            float etai = 1.0f;
            float etat = indexOfRefraction;
            Vector3D n = normal;

            // Check if we're inside the object
            if (cosi < 0)
            {
                cosi = -cosi;
            }
            else
            {
                // Swap indices of refraction and flip normal
                float temp = etai;
                etai = etat;
                etat = temp;
                n = -normal;
            }

            float eta = etai / etat;
            float k = 1.0f - eta * eta * (1.0f - cosi * cosi);

            // Total internal reflection
            if (k < 0)
            {
                return CalculateReflection(incident, normal);
            }

            return (incident * eta) + (n * (eta * cosi - (float)Math.Sqrt(k)));
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

            // For shadow rays, we only need to know if there's any hit, not the closest one
            if (useAcceleration && bvhAccelerator != null)
            {
                // Use a faster "any hit" test for shadows if possible
                HitInfo hit = bvhAccelerator.FindClosestIntersection(shadowRay);
                return hit != null && hit.Distance < lightDistance - adaptiveOffset && hit.ObjectId != sourceObjectId;
            }

            // Fall back to brute-force shadow test
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

        // Helper method to clamp a value between min and max
        private int ClampValue(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        // Helper method to clamp a float value between min and max
        private float ClampValue(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}