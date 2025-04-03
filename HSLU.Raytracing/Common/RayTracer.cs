namespace Common
{
    public class RayTracer
    {
        // Scene components
        private List<IRaycastable> SceneObjects { get; }
        private List<LightSource> LightSources { get; }

        // Rendering parameters
        public RenderSettings Settings { get; }

        public RayTracer(
            List<IRaycastable> sceneObjects,
            List<LightSource> lightSources,
            RenderSettings? settings = null)
        {
            SceneObjects = sceneObjects;
            LightSources = lightSources;
            Settings = settings ?? new RenderSettings();
        }

        /// <summary>
        /// Traces a ray through the scene and calculates its color
        /// </summary>
        /// <param name="ray">Ray to trace</param>
        /// <returns>Calculated color for the ray</returns>
        public MyColor TraceRay(Ray ray)
        {
            var intersection = FindNearestIntersection(ray);

            // If no object was hit, return background color
            if (!intersection.HasHit)
                return Settings.BackgroundColor;

            // Calculate color at intersection point
            return CalculateColor(ray, intersection);
        }

        /// <summary>
        /// Finds the nearest intersection of a ray with scene objects
        /// </summary>
        private RayIntersection FindNearestIntersection(Ray ray)
        {
            float nearestDistance = float.MaxValue;
            IRaycastable nearestObject = null;
            Vector3D intersectionPoint = new Vector3D(0, 0, 0);

            foreach (var sceneObject in SceneObjects)
            {
                var (hasHit, distance) = sceneObject.Intersect(ray);

                if (hasHit && distance > 0 && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestObject = sceneObject;
                    intersectionPoint = ray.GetPointAt(distance);
                }
            }

            if (nearestObject != null)
            {
                return new RayIntersection(true, nearestDistance, nearestObject, intersectionPoint);
            }

            return RayIntersection.Miss();
        }

        /// <summary>
        /// Calculates the color for a ray intersection
        /// </summary>
        private MyColor CalculateColor(Ray ray, RayIntersection intersection)
        {
            var objectColor = intersection.Object.Color;
            var intersectionPoint = intersection.Point;
            var normal = intersection.Object.GetNormal(intersectionPoint);

            // Start with ambient light (equivalent to the "Ambientes Licht" in professor's code)
            MyColor resultColor = objectColor * Settings.AmbientLight;

            // Process all light sources
            foreach (var light in LightSources)
            {
                // Calculate normalized direction to light
                Vector3D lightDirection = (light.Position - intersectionPoint).Normalize();

                // Calculate the dot product for diffuse lighting (Lambert's law)
                float dotProduct = Math.Max(0, normal.Dot(lightDirection));

                // Check shadow only if the surface faces the light
                if (dotProduct > 0)
                {
                    // Calculate shadow using the optimized approach from professor's code
                    float shadowFactor = CalculateShadowFactor(intersectionPoint, light, intersection.Object);

                    // Apply diffuse lighting with shadow and light intensity
                    resultColor += (objectColor * light.Color * dotProduct * shadowFactor) * light.Intensity;

                    // Add specular highlight calculation (simplified from professor's code)
                    Vector3D viewDirection = -ray.Direction;
                    Vector3D reflectionDirection = lightDirection - normal * (2 * normal.Dot(lightDirection));
                    float specularFactor = (float)Math.Pow(
                        Math.Max(0, reflectionDirection.Dot(viewDirection)),
                        Settings.SpecularPower
                    ) * Settings.SpecularIntensity;

                    // Add specular component with shadow
                    resultColor += light.Color * specularFactor * shadowFactor;
                }
            }

            return resultColor;
        }



        /// <summary>
        /// Calculates the light contribution from a single light source
        /// </summary>
        private MyColor CalculateLightContribution(
            LightSource light,
            Ray ray,
            RayIntersection intersection,
            Vector3D normal)
        {
            var intersectionPoint = intersection.Point;
            var lightDirection = (light.Position - intersectionPoint).Normalize();
            var objectColor = intersection.Object.Color;

            // Calculate the angle between normal and light direction - key for diffuse shading
            float cosAngle = normal.Dot(lightDirection);

            // If light hits the backside of the surface, there should be no direct illumination
            // This is critical for proper cube face shading
            if (cosAngle <= 0)
            {
                // Just return ambient contribution for faces pointing away from light
                return objectColor * Settings.AmbientLight;
            }

            // Calculate shadow factor
            float shadowFactor = CalculateShadowFactor(intersectionPoint, light, intersection.Object);

            // Base diffuse lighting calculation - proportional to cosine of angle to light
            float diffuseFactor = cosAngle * light.Intensity;

            // Calculate diffuse color
            var diffuseColor = objectColor * light.Color * diffuseFactor;

            // Specular highlight calculation
            var viewDirection = -ray.Direction;
            var reflectionDirection = CalculateReflectionDirection(normal, lightDirection);
            float specularDot = Math.Max(0, reflectionDirection.Dot(viewDirection));

            float specularFactor = 0;
            if (specularDot > 0)
            {
                specularFactor = (float)Math.Pow(
                    specularDot,
                    Settings.SpecularPower
                ) * Settings.SpecularIntensity * light.Intensity;
            }

            // Apply shadow factor to both diffuse and specular components
            var litColor = (diffuseColor + (light.Color * specularFactor)) * shadowFactor;

            // Ensure a minimum color level based on ambient light
            return litColor;
        }

        /// <summary>
        /// Calculates soft shadow factor
        /// </summary>
        private float CalculateShadowFactor(
            Vector3D intersectionPoint,
            LightSource light,
            IRaycastable currentObject)
        {
            // Use a very small epsilon value to prevent self-intersection issues
            const float epsilon = 0.0001f;

            // Calculate direction and distance to light
            Vector3D lightDirection = (light.Position - intersectionPoint).Normalize();
            float distanceToLight = (light.Position - intersectionPoint).Length;

            // Create shadow ray with the small offset
            Vector3D shadowRayOrigin = intersectionPoint + lightDirection * epsilon;
            var shadowRay = new Ray(shadowRayOrigin, lightDirection);

            // Check for each object if it blocks the light
            foreach (var sceneObject in SceneObjects)
            {
                // Skip the current object to avoid self-shadowing
                if (sceneObject == currentObject)
                    continue;

                var (hasHit, distance) = sceneObject.Intersect(shadowRay);

                // If an object is hit before reaching the light, this point is in shadow
                if (hasHit && distance > 0 && distance < distanceToLight)
                {
                    return 0.2f; // Fixed shadow intensity as in the professor's code
                }
            }

            // No shadow
            return 1.0f;
        }

        /// <summary>
        /// Calculates the reflection direction
        /// </summary>
        private Vector3D CalculateReflectionDirection(Vector3D normal, Vector3D lightDirection)
        {
            float dotNL = normal.Dot(lightDirection);
            return lightDirection - normal * (2 * dotNL);
        }

        /// <summary>
        /// Represents the result of a ray intersection
        /// </summary>
        public class RayIntersection
        {
            public bool HasHit { get; }
            public float Distance { get; }
            public IRaycastable? Object { get; }
            public Vector3D Point { get; }

            public RayIntersection(bool hasHit, float distance, IRaycastable? obj, Vector3D point)
            {
                HasHit = hasHit;
                Distance = distance;
                Object = obj;
                Point = point;
            }

            /// <summary>
            /// Creates a "miss" intersection
            /// </summary>
            public static RayIntersection Miss() =>
                new(false, float.MaxValue, null, new Vector3D(0, 0, 0));
        }

        /// <summary>
        /// Render settings for the ray tracer
        /// </summary>
        public class RenderSettings
        {
            public MyColor BackgroundColor { get; set; } = MyColor.Black;
            public MyColor AmbientLight { get; set; } = new MyColor(25, 25, 25);
            public float SpecularPower { get; set; } = 32f;
            public float SpecularIntensity { get; set; } = 0.5f;
        }
    }
}