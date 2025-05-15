using Common;
using System;
using System.Collections.Generic;

namespace OptimizedTextRender
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Optimized 3D Text Render with BVH Acceleration");
            Console.WriteLine("==============================================");

            // Let user choose render quality
            Console.WriteLine("Select render quality:");
            Console.WriteLine("1. Quick preview (480x270, fast)");
            Console.WriteLine("2. Preview (960x540, medium speed)");
            Console.WriteLine("3. Final quality (1920x1080, slow)");
            Console.WriteLine("4. High quality (2560x1440, very slow)");

            Console.Write("Enter choice (1-4): ");
            string choice = Console.ReadLine();

            OptimizedRenderSettings settings;

            switch (choice)
            {
                case "1":
                    settings = OptimizedRenderSettings.CreateQuickPreview();
                    break;
                case "2":
                    settings = OptimizedRenderSettings.CreatePreview();
                    break;
                case "4":
                    settings = new OptimizedRenderSettings
                    {
                        Width = 2560,
                        Height = 1440,
                        MaxReflectionDepth = 10,
                        OutputFilename = "high_quality_render"
                    };
                    break;
                default:
                    settings = OptimizedRenderSettings.CreateDefault();
                    break;
            }

            Console.Write($"Output filename ({settings.OutputFilename}): ");
            string filename = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(filename))
            {
                settings.OutputFilename = filename;
            }

            Console.Write("Enter path to your 3D text OBJ file: ");
            string objFilePath = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(objFilePath))
            {
                objFilePath = "text3d.obj";
                Console.WriteLine($"Using default path: {objFilePath}");
            }

            var scene = new OptimizedScene();
            var camera = new Camera(new Vector3D(0, 1.5f, -7.0f));

            var wallMaterial = Material.Create(MaterialType.WHITE_RUBBER, 0.05f);
            var textMaterial = Material.Create(MaterialType.GOLD, 0.6f);

            var glassMaterial = new Material(
                MaterialType.CHROME,
                new MyColor(5, 5, 5),      // Very low ambient (reduced from 10)
                new MyColor(90, 90, 100),  // Light blue-ish diffuse (reduced from 180)
                new MyColor(255, 255, 255), // Bright white specular
                0.98f,                     // Very high shininess
                0.25f,                     // Slightly reduced reflectivity for better transparency
                0.98f                      // Very high transparency
            );

            Console.WriteLine("Setting up scene...");
            SetupLighting(scene);

            Room.AddRoom(scene, new Vector3D(0, 0, 0), 12f, 8f, 16f, wallMaterial);

            Console.WriteLine("Importing 3D text model...");
            Console.WriteLine("This might take a while for large models...");

            try
            {
                var objImporter = new ObjModelImporter();

                List<Triangle> textTriangles = objImporter.ImportObj(
                    objFilePath,
                    textMaterial,
                    new Vector3D(0, 0.5f, 5),  // Position further back for better visibility
                    0.7f,                      // Slightly larger scale for better visibility
                    new Vector3D(0, 0, 0)      // No rotation
                );

                Console.WriteLine($"Successfully imported {textTriangles.Count} triangles from OBJ file");

                if (textTriangles.Count > 50000)
                {
                    Console.WriteLine();
                    Console.WriteLine("WARNING: Your model contains more than 50,000 triangles!");
                    Console.WriteLine("This may result in very long render times even with acceleration.");
                    Console.WriteLine("Consider simplifying your model for better performance.");
                    Console.WriteLine();

                    Console.Write("Continue with render? (y/n): ");
                    if (Console.ReadLine()?.ToLower() != "y")
                    {
                        Console.WriteLine("Render canceled by user");
                        return;
                    }
                }

                foreach (var triangle in textTriangles)
                {
                    scene.AddObject(triangle);
                }

                scene.AddObject(new GlassSphere(
                    new Vector3D(0, 0.7f, 0),
                    1.7f,
                    glassMaterial,
                    1.45f
                ));

                scene.AddObject(new RotatedCube(
                    new Vector3D(0, -1.2f, 0),
                    0.7f,
                    Material.Create(MaterialType.SILVER, 0.3f),
                    0f, 0f, 0f
                ));

                AddDecorativeSpheres(scene);
                var rayTracer = new OptimizedRayTracer();

                Console.WriteLine("\nStarting render...");

                rayTracer.RenderScene(scene, camera, settings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during rendering: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void SetupLighting(OptimizedScene scene)
        {
            scene.AddLight(new Light(
                new Vector3D(3, 5, -3),     // Moved higher up
                new MyColor(255, 255, 255), // Pure white
                0.6f                        // Reduced intensity (was 1.0)
            ));

            scene.AddLight(new Light(
                new Vector3D(-4, 2, -2),
                new MyColor(220, 220, 255), // Slightly blue
                0.4f                        // Reduced intensity (was 0.7)
            ));

            scene.AddLight(new Light(
                new Vector3D(0, 3, 7),      // Moved higher and further back
                new MyColor(255, 240, 220), // Warm white
                0.5f                        // Reduced intensity (was 0.8)
            ));

            scene.AddLight(new Light(
                new Vector3D(-3, 1, 1),
                new MyColor(200, 150, 255), // Purple
                0.3f                        // Reduced intensity (was 0.5)
            ));

            scene.AddLight(new Light(
                new Vector3D(1, -3, -1),
                new MyColor(255, 200, 150), // Warm orange
                0.25f                       // Reduced intensity (was 0.4)
            ));
        }

        private static void AddDecorativeSpheres(OptimizedScene scene)
        {
            // Gold sphere
            scene.AddObject(new Sphere(
                new Vector3D(-2.5f, -0.8f, 1.8f),
                0.4f,
                Material.Create(MaterialType.GOLD, 0.6f)
            ));

            // Ruby sphere
            scene.AddObject(new Sphere(
                new Vector3D(2.5f, -0.9f, 1.5f),
                0.35f,
                Material.Create(MaterialType.RUBY, 0.5f)
            ));

            // Emerald sphere
            scene.AddObject(new Sphere(
                new Vector3D(2.2f, -0.7f, -1.3f),
                0.3f,
                Material.Create(MaterialType.EMERALD, 0.5f)
            ));

            // Silver sphere
            scene.AddObject(new Sphere(
                new Vector3D(-1.8f, -0.9f, -1.5f),
                0.25f,
                Material.Create(MaterialType.SILVER, 0.7f)
            ));

            // Jade sphere
            scene.AddObject(new Sphere(
                new Vector3D(0f, -0.8f, 2.8f),
                0.3f,
                Material.Create(MaterialType.JADE, 0.4f)
            ));
        }
    }
}