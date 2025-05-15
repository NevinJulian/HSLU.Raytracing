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

            var scene = new Scene();
            var camera = new Camera(new Vector3D(0, 0, -5.5));

            var blackBackgroundMaterial = new Material(
                MaterialType.BLACK_PLASTIC,
                new MyColor(0, 0, 0),        // Pure black ambient
                new MyColor(0, 0, 0),        // Pure black diffuse
                new MyColor(0, 0, 0),        // No specular
                0.0f,                        // No shininess
                0.0f,                        // No reflectivity
                0.0f                         // No transparency
            );

            scene.AddObject(new Sphere(
                new Vector3D(0, 0, 50),      // Far behind the bubbles
                100f,                        // Very large radius
                blackBackgroundMaterial      // Black material
            ));

            var soapBubbleMaterial = new Material(
                MaterialType.PEARL,
                new MyColor(15, 15, 15),     // Low ambient
                new MyColor(200, 200, 200),  // Moderate diffuse
                new MyColor(255, 255, 255),  // Bright white specular
                0.95f,                       // Very high shininess
                0.4f,                        // Good reflectivity
                0.85f                        // High transparency
            );

            Random random = new Random(42);
            CreateExtremelyVariedBubbles(scene, soapBubbleMaterial, random);
            SetupPhysicalLighting(scene);
            scene.SetMaxReflectionDepth(10);

            // Create render settings
            var settings = new RenderSettings
            {
                Width = width,
                Height = height,
                MaxReflectionDepth = 10,
                OutputFilename = filePath.Replace(".png", ""),
                OutputFormat = "png",
                NumThreads = Environment.ProcessorCount
            };

            var rayTracer = new RayTracer();
            Console.WriteLine("Starting rendering...");
            rayTracer.RenderScene(scene, camera, settings);
            Console.WriteLine($"Image saved to {filePath}");
        }

        private static void CreateExtremelyVariedBubbles(Scene scene, Material soapBubbleMaterial, Random random)
        {
            var bubblePositions = new List<(Vector3D center, float radius)>();

            int totalBubbles = 40;
            int attemptsPerBubble = 50;

            for (int i = 0; i < totalBubbles; i++)
            {
                bool validPosition = false;
                Vector3D center = Vector3D.Zero;
                float radius = 0f;

                for (int attempt = 0; attempt < attemptsPerBubble && !validPosition; attempt++)
                {
                    float x = (float)(random.NextDouble() * 14.0 - 7.0);
                    float y = (float)(random.NextDouble() * 10.0 - 5.0);
                    float z = (float)(random.NextDouble() * 8.0 + 1.0);

                    float sizeFactor = (float)random.NextDouble();

                    if (sizeFactor < 0.7f)
                    {
                        radius = 0.15f + (float)random.NextDouble() * 0.5f;
                    }
                    else if (sizeFactor < 0.9f)
                    {
                        radius = 0.6f + (float)random.NextDouble() * 0.7f;
                    }
                    else
                    {
                        radius = 1.3f + (float)random.NextDouble() * 1.0f;
                    }

                    center = new Vector3D(x, y, z);

                    validPosition = true;
                    foreach (var (existingCenter, existingRadius) in bubblePositions)
                    {
                        float distance = (existingCenter - center).Length;
                        float combinedRadii = radius + existingRadius;

                        float overlapFactor = 0.7f;
                        if (radius < 0.4f && existingRadius < 0.4f)
                        {
                            overlapFactor = 0.5f;
                        }

                        if (distance < combinedRadii * overlapFactor)
                        {
                            validPosition = false;
                            break;
                        }
                    }
                }

                if (validPosition)
                {
                    bubblePositions.Add((center, radius));

                    float age = (float)Math.Pow(random.NextDouble(), 1.5);
                    float uniquenessFactor = 0.4f + (float)random.NextDouble() * 0.6f;
                    float filmVariation = 0.3f + (float)random.NextDouble() * 0.7f;

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
            scene.AddLight(new Light(
                new Vector3D(4, 5, -6),
                new MyColor(255, 250, 245),
                4.0f
            ));

            scene.AddLight(new Light(
                new Vector3D(-5, -2, -4),
                new MyColor(220, 230, 255),
                2.5f
            ));

            scene.AddLight(new Light(
                new Vector3D(0, 1, 8),
                new MyColor(255, 255, 255),
                3.0f
            ));

            scene.AddLight(new Light(
                new Vector3D(-4, 3, -3),
                new MyColor(255, 100, 200),
                2.0f
            ));

            scene.AddLight(new Light(
                new Vector3D(4, -3, -3),
                new MyColor(100, 220, 255),
                2.0f
            ));

            scene.AddLight(new Light(
                new Vector3D(0, -4, -2),
                new MyColor(255, 200, 100),
                2.0f
            ));

            scene.AddLight(new Light(
                new Vector3D(5, 2, -3),
                new MyColor(120, 255, 120),
                1.5f
            ));

            scene.AddLight(new Light(
                new Vector3D(-5, 2, -1),
                new MyColor(100, 100, 255),
                1.5f
            ));

            scene.AddLight(new Light(
                new Vector3D(0, 0, -5),
                new MyColor(255, 255, 255),
                1.5f
            ));

            scene.AddLight(new Light(
                new Vector3D(2, 2, -5),
                new MyColor(255, 255, 255),
                1.0f
            ));

            scene.AddLight(new Light(
                new Vector3D(-2, -2, -5),
                new MyColor(255, 255, 255),
                1.0f
            ));
        }
    }
}