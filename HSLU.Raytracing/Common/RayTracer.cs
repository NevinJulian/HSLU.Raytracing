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

                if (hasHit && distance > 0.001f && distance < nearestDistance)
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

        private MyColor CalculateColor(Ray ray, RayIntersection intersection)
        {
            var objectColor = intersection.Object.Color;
            var intersectionPoint = intersection.Point;
            var normal = intersection.Object.GetNormal(intersectionPoint);

            // Start with ambient light
            MyColor resultColor = objectColor * Settings.AmbientLight;

            // Process all light sources
            foreach (var light in LightSources)
            {
                // Calculate light direction (from intersection to light)
                Vector3D lightDirection = (light.Position - intersectionPoint).Normalize();

                // Calculate diffuse lighting using Lambert's cosine law
                float diffuseFactor = Math.Max(0, normal.Dot(lightDirection));

                // Skip if surface faces away from light
                if (diffuseFactor <= 0)
                    continue;

                // Shadow check - only apply lighting if not in shadow
                bool inShadow = IsPointInShadow(intersectionPoint, normal, light, intersection.Object);

                if (!inShadow)
                {
                    // Apply diffuse lighting
                    resultColor += (objectColor * light.Color * diffuseFactor) * light.Intensity;

                    // Add specular highlight (Phong reflection model)
                    Vector3D viewDirection = (ray.Origin - intersectionPoint).Normalize();
                    Vector3D reflectionDirection = CalculateReflectionDirection(normal, lightDirection);

                    float specularFactor = (float)Math.Pow(
                        Math.Max(0, viewDirection.Dot(reflectionDirection)),
                        Settings.SpecularPower
                    );

                    if (specularFactor > 0)
                    {
                        MyColor specularColor = light.Color * (specularFactor * Settings.SpecularIntensity);
                        resultColor += specularColor;
                    }
                }
            }

            return resultColor;
        }

        private bool IsPointInShadow(
            Vector3D intersectionPoint,
            Vector3D surfaceNormal,
            LightSource light,
            IRaycastable currentObject)
        {
            // Use a more aggressive epsilon to prevent self-shadowing artifacts
            const float SHADOW_EPSILON = 0.1f;

            // Calculate direction and distance to light
            Vector3D lightDirection = (light.Position - intersectionPoint).Normalize();
            float distanceToLight = (light.Position - intersectionPoint).Length;

            // Compute dot product to ensure we're offsetting in right direction
            float dotProduct = surfaceNormal.Dot(lightDirection);

            // Offset intersection point along normal to avoid self-intersection
            // If normal and light direction point in similar direction, offset in normal direction
            // Otherwise offset in opposite direction to avoid pushing point inside the object
            Vector3D offsetDirection = dotProduct > 0 ? surfaceNormal : lightDirection;
            Vector3D shadowRayOrigin = intersectionPoint + (offsetDirection * SHADOW_EPSILON);

            var shadowRay = new Ray(shadowRayOrigin, lightDirection);

            // Check if any object blocks the light
            foreach (var sceneObject in SceneObjects)
            {
                // Skip the object we're calculating lighting for to avoid self-shadowing
                if (sceneObject == currentObject)
                    continue;

                var (hasHit, distance) = sceneObject.Intersect(shadowRay);

                // If we hit something between the point and the light
                // Use a small epsilon to avoid precision issues
                if (hasHit && distance > 0.001f && distance < distanceToLight - 0.001f)
                {
                    return true; // Point is in shadow
                }
            }

            return false; // Point is not in shadow
        }

        /// <summary>
        /// Calculates the reflection direction for specular highlights
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