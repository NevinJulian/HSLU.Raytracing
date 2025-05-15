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

            bvhAccelerator = null;
        }

        public void AddLight(Light light)
        {
            lights.Add(light);
        }

        public void BuildAccelerationStructure()
        {
            bvhAccelerator = new BVHAccelerator(objects);
            Console.WriteLine($"Built BVH with {objects.Count} objects");
        }

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
            if (useAcceleration && bvhAccelerator != null)
            {
                return bvhAccelerator.FindClosestIntersection(ray);
            }

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

            bool isGlassSphere = obj is GlassSphere;

            int red = (int)(material.Ambient.R);
            int green = (int)(material.Ambient.G);
            int blue = (int)(material.Ambient.B);

            foreach (Light light in lights)
            {
                Vector3D lightVector = light.Position - hitPoint;
                float lightDistance = lightVector.Length;
                Vector3D lightDirection = lightVector * (1.0f / lightDistance);

                float cosAngle = normal.Dot(lightDirection);

                bool inShadow = false;
                if (!isGlassSphere)
                {
                    inShadow = IsPointInShadow(hitPoint, normal, lightDirection, lightDistance, obj.ObjectId);
                }
                else
                {
                    inShadow = false;
                }

                if (!inShadow || isGlassSphere)
                {
                    float diffuseFactor = Math.Max(0, cosAngle);

                    if (isGlassSphere)
                    {
                        diffuseFactor *= 0.2f;
                    }

                    red += (int)(obj.Color.R * light.Intensity * diffuseFactor * light.Color.R / 255.0f);
                    green += (int)(obj.Color.G * light.Intensity * diffuseFactor * light.Color.G / 255.0f);
                    blue += (int)(obj.Color.B * light.Intensity * diffuseFactor * light.Color.B / 255.0f);

                    if (material.Shininess > 0)
                    {
                        Vector3D reflection = CalculateReflection(-lightDirection, normal);
                        float specularFactor = Math.Max(0, reflection.Dot(-ray.Direction));

                        float shininessPower = material.Shininess * 128;
                        if (isGlassSphere)
                        {
                            shininessPower = 300;
                        }

                        specularFactor = (float)Math.Pow(specularFactor, shininessPower);

                        red += (int)(material.Specular.R * light.Intensity * specularFactor * light.Color.R / 255.0f);
                        green += (int)(material.Specular.G * light.Intensity * specularFactor * light.Color.G / 255.0f);
                        blue += (int)(material.Specular.B * light.Intensity * specularFactor * light.Color.B / 255.0f);
                    }
                }
            }

            float reflectivity = material.Reflectivity;
            MyColor reflectionColor = MyColor.Black;

            if (reflectivity > 0 && depth < maxReflectionDepth)
            {
                Vector3D reflectionDir = CalculateReflection(ray.Direction, normal);
                Ray reflectionRay = new Ray(hitPoint + normal * 0.01f, reflectionDir);

                reflectionColor = Trace(reflectionRay, depth + 1);

                if (!isGlassSphere)
                {
                    red = (int)(red * (1.0f - reflectivity) + reflectionColor.R * reflectivity);
                    green = (int)(green * (1.0f - reflectivity) + reflectionColor.G * reflectivity);
                    blue = (int)(blue * (1.0f - reflectivity) + reflectionColor.B * reflectivity);
                }
            }

            float transparency = material.Transparency;
            if (transparency > 0 && depth < maxReflectionDepth)
            {
                if (isGlassSphere)
                {
                    GlassSphere glassSphere = (GlassSphere)obj;
                    Vector3D refractedDir = CalculateRefraction(ray.Direction, normal, glassSphere.RefractionIndex);
                    Ray refractedRay = new Ray(hitPoint + refractedDir * 0.015f, refractedDir);
                    MyColor refractedColor = Trace(refractedRay, depth + 1);

                    float r0 = (1.0f - glassSphere.RefractionIndex) / (1.0f + glassSphere.RefractionIndex);
                    r0 = r0 * r0;

                    float viewDot = Math.Abs(normal.Dot(-ray.Direction));
                    float fresnelFactor = r0 + (1.0f - r0) * (float)Math.Pow(1.0f - viewDot, 5.0f);

                    fresnelFactor *= 0.85f;

                    if (reflectivity <= 0)
                    {
                        Vector3D reflectionDir = CalculateReflection(ray.Direction, normal);
                        Ray reflectionRay = new Ray(hitPoint + normal * 0.01f, reflectionDir);
                        reflectionColor = Trace(reflectionRay, depth + 1);
                    }

                    float effectiveReflectivity = Math.Max(reflectivity * 0.7f, fresnelFactor * 0.7f);
                    float effectiveTransparency = transparency * (1.0f - fresnelFactor * 0.5f);

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
                    Ray transparencyRay = new Ray(hitPoint + ray.Direction * 0.01f, ray.Direction);
                    MyColor transparencyColor = Trace(transparencyRay, depth + 1);

                    red = (int)(red * (1.0f - transparency) + transparencyColor.R * transparency);
                    green = (int)(green * (1.0f - transparency) + transparencyColor.G * transparency);
                    blue = (int)(blue * (1.0f - transparency) + transparencyColor.B * transparency);
                }
            }

            red = ClampValue(red, 0, 255);
            green = ClampValue(green, 0, 255);
            blue = ClampValue(blue, 0, 255);

            return new MyColor(red, green, blue);
        }

        private Vector3D CalculateReflection(Vector3D incident, Vector3D normal)
        {
            float dot = incident.Dot(normal);
            return incident - normal * (2 * dot);
        }

        private Vector3D CalculateRefraction(Vector3D incident, Vector3D normal, float indexOfRefraction)
        {
            float cosi = ClampValue(incident.Dot(normal), -1.0f, 1.0f);
            float etai = 1.0f;
            float etat = indexOfRefraction;
            Vector3D n = normal;

            if (cosi < 0)
            {
                cosi = -cosi;
            }
            else
            {
                float temp = etai;
                etai = etat;
                etat = temp;
                n = -normal;
            }

            float eta = etai / etat;
            float k = 1.0f - eta * eta * (1.0f - cosi * cosi);

            if (k < 0)
            {
                return CalculateReflection(incident, normal);
            }

            return (incident * eta) + (n * (eta * cosi - (float)Math.Sqrt(k)));
        }

        private bool IsPointInShadow(Vector3D hitPoint, Vector3D normal, Vector3D lightDir, float lightDistance, int sourceObjectId)
        {
            const float BASE_EPSILON = 0.1f;

            float cosAngle = Math.Max(0.01f, normal.Dot(lightDir));
            float adaptiveOffset = BASE_EPSILON / cosAngle;
            adaptiveOffset = Math.Min(adaptiveOffset, 0.5f);

            Vector3D offsetPoint = hitPoint + normal * adaptiveOffset + lightDir * 0.01f;
            Ray shadowRay = new Ray(offsetPoint, lightDir);

            if (useAcceleration && bvhAccelerator != null)
            {
                HitInfo hit = bvhAccelerator.FindClosestIntersection(shadowRay);
                return hit != null && hit.Distance < lightDistance - adaptiveOffset && hit.ObjectId != sourceObjectId;
            }

            foreach (IRaycastable obj in objects)
            {
                if (obj.ObjectId == sourceObjectId)
                    continue;

                var (hasHit, distance) = obj.Intersect(shadowRay);

                if (hasHit && distance > 0.001f && distance < lightDistance - adaptiveOffset)
                {
                    if (obj is SoapBubble soapBubble)
                    {
                        return false;
                    }

                    return true;
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

        private int ClampValue(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private float ClampValue(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}