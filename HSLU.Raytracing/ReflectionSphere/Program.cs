using Common;
using System;
using System.Collections.Generic;

namespace SpheresRender
{
    class Program
    {
        static void Main(string[] args)
        {
            const int width = 1600;
            const int height = 900;
            const string filePath = "reference_scene.png";

            var scene = new Scene();
            var camera = new Camera(new Vector3D(0, 0.8, -4.0));

            var floorMaterial = new Material(
                MaterialType.WHITE_PLASTIC,
                new MyColor(30, 30, 30),      // Ambient
                new MyColor(240, 240, 240),   // Light diffuse color
                new MyColor(200, 200, 200),   // Specular
                0.05f,                        // Low shininess
                0.1f                          // Slight reflectivity
            );

            // Create a large floor plane
            float floorY = 0f;
            float floorSize = 40f;

            var floor1 = new Triangle(
                new Vector3D(-floorSize, floorY, floorSize),
                new Vector3D(floorSize, floorY, floorSize),
                new Vector3D(-floorSize, floorY, -floorSize),
                floorMaterial
            );

            var floor2 = new Triangle(
                new Vector3D(floorSize, floorY, floorSize),
                new Vector3D(floorSize, floorY, -floorSize),
                new Vector3D(-floorSize, floorY, -floorSize),
                floorMaterial
            );

            scene.AddObject(floor1);
            scene.AddObject(floor2);

            // Create the three main spheres

            // Left sphere - orangish/gold metallic
            var leftSphereMaterial = new Material(
                MaterialType.GOLD,
                new MyColor(80, 50, 20),      // Ambient
                new MyColor(220, 160, 80),    // Diffuse - orange/gold
                new MyColor(255, 240, 180),   // Specular
                0.6f,                         // Shininess
                0.6f                          // Medium-high reflectivity
            );

            scene.AddObject(new Sphere(
                new Vector3D(-3.0f, 1.0f, 0),  // Left position
                1.0f,                          // Radius of 1
                leftSphereMaterial
            ));

            // Center sphere - reflective half-black half-silver
            // This is tricky - we'll use a highly reflective material
            var centerSphereMaterial = new Material(
                MaterialType.SILVER,
                new MyColor(5, 5, 5),         // Very dark ambient (near black)
                new MyColor(220, 220, 220),   // Silver diffuse
                new MyColor(255, 255, 255),   // Bright specular
                0.9f,                         // High shininess
                0.95f                         // Very high reflectivity
            );

            scene.AddObject(new Sphere(
                new Vector3D(0, 1.0f, 0),      // Center position
                1.0f,                          // Radius of 1
                centerSphereMaterial
            ));

            // Right sphere - matte white
            var rightSphereMaterial = new Material(
                MaterialType.WHITE_PLASTIC,
                new MyColor(60, 60, 60),      // Ambient
                new MyColor(250, 250, 250),   // Bright white diffuse
                new MyColor(30, 30, 30),      // Low specular for matte appearance
                0.1f,                         // Low shininess for matte appearance
                0.0f                          // No reflectivity for true matte look
            );

            scene.AddObject(new Sphere(
                new Vector3D(3.0f, 1.0f, 0),   // Right position
                1.0f,                          // Radius of 1
                rightSphereMaterial
            ));

            // Generate many small colored spheres on the floor
            Random random = new Random(42);  // Use fixed seed for reproducibility

            // Create a collection of vibrant colors for small spheres
            var colors = new List<MyColor>
            {
                new MyColor(220, 40, 40),    // Red
                new MyColor(40, 220, 40),    // Green
                new MyColor(40, 40, 220),    // Blue
                new MyColor(220, 220, 40),   // Yellow
                new MyColor(220, 40, 220),   // Magenta
                new MyColor(40, 220, 220),   // Cyan
                new MyColor(220, 140, 40),   // Orange
                new MyColor(140, 40, 220),   // Purple
                new MyColor(200, 220, 40),   // Lime
                new MyColor(220, 180, 180),  // Pink
                new MyColor(40, 40, 40),     // Black
                new MyColor(220, 220, 220),  // White/Silver
                new MyColor(140, 100, 40),   // Brown
            };

            // Material types to choose from
            var materialTypes = new List<(MaterialType type, float reflectivity)>
            {
                (MaterialType.RED_PLASTIC, 0.0f),
                (MaterialType.GREEN_PLASTIC, 0.0f),
                (MaterialType.WHITE_PLASTIC, 0.0f),
                (MaterialType.GOLD, 0.7f),
                (MaterialType.SILVER, 0.9f),
                (MaterialType.RUBY, 0.3f),
                (MaterialType.EMERALD, 0.3f),
                (MaterialType.PEARL, 0.2f),
            };

            int gridSizeX = 40;
            int gridSizeZ = 30;
            float gridWidth = 16f;
            float gridDepth = 12f;
            float cellWidth = gridWidth / gridSizeX;
            float cellDepth = gridDepth / gridSizeZ;

            List<Vector3D> spherePositions = new List<Vector3D>();

            for (int i = 0; i < gridSizeX; i++)
            {
                for (int j = 0; j < gridSizeZ; j++)
                {
                    if (random.NextDouble() < 0.7)
                        continue;

                    float baseX = i * cellWidth - gridWidth / 2 + cellWidth / 2;
                    float baseZ = j * cellDepth - gridDepth / 4 + cellDepth / 2;

                    float jitterX = (float)(random.NextDouble() * 0.8 - 0.4) * cellWidth;
                    float jitterZ = (float)(random.NextDouble() * 0.8 - 0.4) * cellDepth;

                    spherePositions.Add(new Vector3D(
                        baseX + jitterX,
                        0,
                        baseZ + jitterZ
                    ));
                }
            }

            for (int i = spherePositions.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                Vector3D temp = spherePositions[i];
                spherePositions[i] = spherePositions[j];
                spherePositions[j] = temp;
            }

            foreach (var position in spherePositions)
            {
                if (IsNearMainSpheres(position))
                    continue;

                float radius = (float)(random.NextDouble() * 0.1 + 0.05);

                float y = radius + floorY;
                Vector3D finalPosition = new Vector3D(position.X, y, position.Z);

                var color = colors[random.Next(colors.Count)];
                var (materialType, reflectivity) = materialTypes[random.Next(materialTypes.Count)];

                if (random.NextDouble() < 0.2)
                {
                    var glassMaterial = new Material(
                        MaterialType.PEARL,
                        new MyColor(240, 240, 240),   // Ambient
                        new MyColor(240, 240, 240),   // Diffuse
                        new MyColor(255, 255, 255),   // Specular
                        0.9f,                         // Shininess
                        0.95f                         // Very high reflectivity
                    );

                    scene.AddObject(new Sphere(
                        finalPosition,
                        radius,
                        glassMaterial
                    ));
                }
                else
                {
                    Material material;

                    if (random.NextDouble() < 0.5)
                    {
                        material = Material.Create(materialType, reflectivity);
                    }
                    else
                    {
                        material = new Material(
                            materialType,
                            new MyColor((int)(color.R * 0.2f), (int)(color.G * 0.2f), (int)(color.B * 0.2f)),  // Ambient
                            color,                                                                              // Diffuse
                            new MyColor(Math.Min(color.R + 50, 255), Math.Min(color.G + 50, 255), Math.Min(color.B + 50, 255)), // Specular
                            (float)random.NextDouble() * 0.5f + 0.1f,                                           // Shininess
                            (float)random.NextDouble() * 0.3f                                                   // Reflectivity
                        );
                    }

                    scene.AddObject(new Sphere(
                        finalPosition,
                        radius,
                        material
                    ));
                }
            }

            scene.AddLight(new Light(
                new Vector3D(0, 8, -8),
                new MyColor(255, 255, 255),  // White light
                1.0f                         // Full intensity
            ));

            // Fill light from left
            scene.AddLight(new Light(
                new Vector3D(-6, 6, -2),
                new MyColor(220, 220, 240),  // Slightly blue
                0.6f                         // Medium intensity
            ));

            // Fill light from right
            scene.AddLight(new Light(
                new Vector3D(6, 6, -2),
                new MyColor(240, 220, 220),  // Slightly warm
                0.6f                         // Medium intensity
            ));

            // Create render settings with optimal parameters
            var settings = new RenderSettings
            {
                Width = width,
                Height = height,
                MaxReflectionDepth = 6,       // Increased for better reflections
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

        private static bool IsNearMainSpheres(Vector3D position)
        {
            var spherePositions = new List<Vector3D>
            {
                new Vector3D(-3.0f, 1.0f, 0),
                new Vector3D(0.0f, 1.0f, 0),
                new Vector3D(3.0f, 1.0f, 0)
            };

            foreach (var center in spherePositions)
            {
                Vector3D projectedPos = new Vector3D(position.X, 1.0f, position.Z);
                float distance = (projectedPos - center).Length;

                if (distance < 1.2f)
                    return true;
            }

            return false;
        }
    }
}