using Common;
using System;
using System.Collections.Generic;

namespace SpheresRender
{
    class Program
    {
        static void Main(string[] args)
        {
            const int width = 1920;
            const int height = 1080;
            const string filePath = "extreme_soap_bubbles.png";

            // Create scene
            var scene = new Scene();

            // Position camera for better composition
            var camera = new Camera(new Vector3D(0, 0, -5.5));

            // Create a pure black background
            var blackBackgroundMaterial = new Material(
                MaterialType.BLACK_PLASTIC,
                new MyColor(0, 0, 0),        // Pure black ambient
                new MyColor(0, 0, 0),        // Pure black diffuse
                new MyColor(0, 0, 0),        // No specular
                0.0f,                        // No shininess
                0.0f,                        // No reflectivity
                0.0f                         // No transparency
            );

            // Add distant background sphere
            scene.AddObject(new Sphere(
                new Vector3D(0, 0, 50),      // Far behind the bubbles
                100f,                        // Very large radius
                blackBackgroundMaterial      // Black material
            ));

            // Create realistic soap bubble material - highly transparent with strong specular
            var soapBubbleMaterial = new Material(
                MaterialType.PEARL,
                new MyColor(15, 15, 15),     // Low ambient
                new MyColor(200, 200, 200),  // Moderate diffuse
                new MyColor(255, 255, 255),  // Bright white specular
                0.95f,                       // Very high shininess
                0.4f,                        // Good reflectivity
                0.85f                        // High transparency
            );

            // Create well-distributed bubbles with natural variation
            Random random = new Random(42);  // Fixed seed for reproducibility

            // Create a better distribution of soap bubbles with more variety
            CreateExtremelyVariedBubbles(scene, soapBubbleMaterial, random);

            // Setup extremely bright lighting
            SetupPhysicalLighting(scene);

            // Set max reflection depth to capture multiple internal reflections in bubbles
            scene.SetMaxReflectionDepth(10);

            // Create render settings
            var settings = new RenderSettings
            {
                Width = width,
                Height = height,
                MaxReflectionDepth = 10,     // Higher reflection depth for multiple reflections
                OutputFilename = filePath.Replace(".png", ""),
                OutputFormat = "png",
                NumThreads = Environment.ProcessorCount
            };

            // Create ray tracer and render the scene
            var rayTracer = new RayTracer();
            Console.WriteLine("Starting rendering...");
            rayTracer.RenderScene(scene, camera, settings);
            Console.WriteLine($"Image saved to {filePath}");
        }

        private static void CreateExtremelyVariedBubbles(Scene scene, Material soapBubbleMaterial, Random random)
        {
            // Store bubble positions to prevent excessive overlap
            var bubblePositions = new List<(Vector3D center, float radius)>();

            // Number of bubbles to create
            int totalBubbles = 40; // More bubbles for a dense scene
            int attemptsPerBubble = 50; // Maximum attempts to place each bubble

            // Create bubbles with extreme variation
            for (int i = 0; i < totalBubbles; i++)
            {
                bool validPosition = false;
                Vector3D center = Vector3D.Zero;
                float radius = 0f;

                // Try multiple positions until we find one with acceptable overlap
                for (int attempt = 0; attempt < attemptsPerBubble && !validPosition; attempt++)
                {
                    // Generate wide random position in view volume
                    float x = (float)(random.NextDouble() * 14.0 - 7.0);  // -7 to 7
                    float y = (float)(random.NextDouble() * 10.0 - 5.0);  // -5 to 5
                    float z = (float)(random.NextDouble() * 8.0 + 1.0);   // 1 to 9 (in front of camera)

                    // Use extreme size variation
                    float sizeFactor = (float)random.NextDouble();

                    // Distribution favoring small bubbles but with a few large ones
                    if (sizeFactor < 0.7f)
                    {
                        // 70% small bubbles
                        radius = 0.15f + (float)random.NextDouble() * 0.5f;
                    }
                    else if (sizeFactor < 0.9f)
                    {
                        // 20% medium bubbles
                        radius = 0.6f + (float)random.NextDouble() * 0.7f;
                    }
                    else
                    {
                        // 10% large bubbles
                        radius = 1.3f + (float)random.NextDouble() * 1.0f;
                    }

                    center = new Vector3D(x, y, z);

                    // Check overlap with existing bubbles
                    validPosition = true;
                    foreach (var (existingCenter, existingRadius) in bubblePositions)
                    {
                        // Calculate distance between centers
                        float distance = (existingCenter - center).Length;
                        float combinedRadii = radius + existingRadius;

                        // Allow more overlap for smaller bubbles (helps create clusters)
                        float overlapFactor = 0.7f;
                        if (radius < 0.4f && existingRadius < 0.4f)
                        {
                            overlapFactor = 0.5f; // Small bubbles can overlap more
                        }

                        // Reject excessive overlap
                        if (distance < combinedRadii * overlapFactor)
                        {
                            validPosition = false;
                            break;
                        }
                    }
                }

                // If we found a valid position, add the bubble
                if (validPosition)
                {
                    // Add to our tracking list
                    bubblePositions.Add((center, radius));

                    // Generate extremely varied bubble properties
                    float age = (float)Math.Pow(random.NextDouble(), 1.5); // More young bubbles than old

                    // Uniqueness factor - controls how distinctive this bubble looks
                    float uniquenessFactor = 0.4f + (float)random.NextDouble() * 0.6f;

                    // Film variation - controls how much the film thickness varies
                    float filmVariation = 0.3f + (float)random.NextDouble() * 0.7f;

                    // Create the soap bubble with varied properties
                    scene.AddObject(new SoapBubble(
                        center,
                        radius,
                        soapBubbleMaterial,
                        age,
                        uniquenessFactor,
                        filmVariation
                    ));
                }
            }
        }

        private static void SetupPhysicalLighting(Scene scene)
        {
            // EXTREMELY bright lighting setup to ensure bubbles are clearly visible

            // Main key light - maximum intensity
            scene.AddLight(new Light(
                new Vector3D(4, 5, -6),
                new MyColor(255, 250, 245),  // Slightly warm white
                4.0f                         // Maximum intensity
            ));

            // Fill light from opposite side - very bright
            scene.AddLight(new Light(
                new Vector3D(-5, -2, -4),
                new MyColor(220, 230, 255),  // Slightly cool
                2.5f                         // Very high intensity
            ));

            // Rim light from behind - very bright
            scene.AddLight(new Light(
                new Vector3D(0, 1, 8),
                new MyColor(255, 255, 255),  // Pure white
                3.0f                         // Very high intensity
            ));

            // Super bright colored accent lights for vibrant reflection colors

            // Magenta accent
            scene.AddLight(new Light(
                new Vector3D(-4, 3, -3),
                new MyColor(255, 100, 200),  // Magenta
                2.0f                         // Very high intensity
            ));

            // Cyan accent
            scene.AddLight(new Light(
                new Vector3D(4, -3, -3),
                new MyColor(100, 220, 255),  // Cyan
                2.0f                         // Very high intensity
            ));

            // Golden accent
            scene.AddLight(new Light(
                new Vector3D(0, -4, -2),
                new MyColor(255, 200, 100),  // Golden yellow
                2.0f                         // Very high intensity
            ));

            // Additional colored lights for more variety

            // Green accent
            scene.AddLight(new Light(
                new Vector3D(5, 2, -3),
                new MyColor(120, 255, 120),  // Green
                1.5f                         // High intensity
            ));

            // Blue accent
            scene.AddLight(new Light(
                new Vector3D(-5, 2, -1),
                new MyColor(100, 100, 255),  // Blue
                1.5f                         // High intensity
            ));

            // Add multiple lights from camera direction to ensure front illumination
            scene.AddLight(new Light(
                new Vector3D(0, 0, -5),      // Directly from camera
                new MyColor(255, 255, 255),  // White light
                1.5f                         // High intensity
            ));

            scene.AddLight(new Light(
                new Vector3D(2, 2, -5),      // Slightly up-right from camera
                new MyColor(255, 255, 255),  // White light
                1.0f                         // Medium intensity
            ));

            scene.AddLight(new Light(
                new Vector3D(-2, -2, -5),    // Slightly down-left from camera
                new MyColor(255, 255, 255),  // White light
                1.0f                         // Medium intensity
            ));
        }
    }
}