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

            // Allow user to override the output filename
            Console.Write($"Output filename ({settings.OutputFilename}): ");
            string filename = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(filename))
            {
                settings.OutputFilename = filename;
            }

            // Path to your OBJ file with the 3D text
            Console.Write("Enter path to your 3D text OBJ file: ");
            string objFilePath = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(objFilePath))
            {
                objFilePath = "text3d.obj"; // Default path
                Console.WriteLine($"Using default path: {objFilePath}");
            }

            // Create scene
            var scene = new OptimizedScene();

            // Position camera
            var camera = new Camera(new Vector3D(0, 1.0f, -6.0f));

            // Create materials
            var wallMaterial = Material.Create(MaterialType.WHITE_RUBBER, 0.1f);
            var textMaterial = Material.Create(MaterialType.GOLD, 0.4f);

            // Create glass material with high transparency and reflectivity
            var glassMaterial = new Material(
                MaterialType.CHROME,
                new MyColor(10, 10, 10),     // Very low ambient
                new MyColor(180, 180, 200),  // Light blue-ish diffuse
                new MyColor(255, 255, 255),  // Bright white specular
                0.95f,                       // Very high shininess
                0.3f,                        // Medium reflectivity
                0.95f                        // High transparency
            );

            Console.WriteLine("Setting up scene...");

            // Set up the lighting for the scene
            SetupLighting(scene);

            // Add the room
            Room.AddRoom(scene, new Vector3D(0, 0, 0), 12f, 8f, 16f, wallMaterial);

            Console.WriteLine("Importing 3D text model...");
            Console.WriteLine("This might take a while for large models...");

            try
            {
                // Import the 3D text from OBJ file
                var objImporter = new ObjModelImporter();

                // Load the 3D text with proper positioning, scale, and rotation
                List<Triangle> textTriangles = objImporter.ImportObj(
                    objFilePath,
                    textMaterial,
                    new Vector3D(0, 0.5f, 4),  // Position at the back of the room
                    0.5f,                      // Scale 
                    new Vector3D(0, 0, 0)      // No rotation
                );

                Console.WriteLine($"Successfully imported {textTriangles.Count} triangles from OBJ file");

                // For models with massive numbers of triangles, show a warning
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

                // Add text triangles to the scene
                foreach (var triangle in textTriangles)
                {
                    scene.AddObject(triangle);
                }

                // Add a glass sphere in front of the text
                scene.AddObject(new GlassSphere(
                    new Vector3D(0, 0.5f, 0),  // Position between camera and text
                    1.5f,                      // Radius
                    glassMaterial,
                    1.5f                       // Refractive index for glass
                ));

                // Add a pedestal for the glass sphere
                scene.AddObject(new RotatedCube(
                    new Vector3D(0, -1.2f, 0),  // Position below the sphere
                    0.7f,                       // Size
                    Material.Create(MaterialType.SILVER, 0.4f),
                    0f, 0f, 0f                  // No rotation
                ));

                // Add some reflective spheres around for interest
                AddDecorativeSpheres(scene);

                // Create ray tracer and render the scene
                var rayTracer = new OptimizedRayTracer();

                Console.WriteLine("\nStarting render...");

                // Render the scene
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
            // Main key light - bright white light
            scene.AddLight(new Light(
                new Vector3D(3, 4, -3),
                new MyColor(255, 255, 255),  // Pure white
                1.0f                         // Full intensity
            ));

            // Fill light from opposite side
            scene.AddLight(new Light(
                new Vector3D(-4, 2, -2),
                new MyColor(220, 220, 255),  // Slightly blue
                0.7f                         // Medium intensity
            ));

            // Back light - helps separate objects from background
            scene.AddLight(new Light(
                new Vector3D(0, 2, 6),
                new MyColor(255, 240, 220),  // Warm white
                0.8f                         // Medium-high intensity
            ));

            // Add a colored accent light for interest
            scene.AddLight(new Light(
                new Vector3D(-3, 1, 1),
                new MyColor(200, 150, 255),  // Purple
                0.5f                         // Medium-low intensity
            ));

            // Add a low light from below for drama
            scene.AddLight(new Light(
                new Vector3D(1, -3, -1),
                new MyColor(255, 200, 150),  // Warm orange
                0.4f                         // Low intensity
            ));
        }

        private static void AddDecorativeSpheres(OptimizedScene scene)
        {
            // Add some small decorative spheres around the scene

            // Gold sphere
            scene.AddObject(new Sphere(
                new Vector3D(-2.5f, -0.8f, 1.5f),
                0.4f,
                Material.Create(MaterialType.GOLD, 0.6f)
            ));

            // Ruby sphere
            scene.AddObject(new Sphere(
                new Vector3D(2.5f, -0.9f, 1.2f),
                0.35f,
                Material.Create(MaterialType.RUBY, 0.5f)
            ));

            // Emerald sphere
            scene.AddObject(new Sphere(
                new Vector3D(2.2f, -0.7f, -1.0f),
                0.3f,
                Material.Create(MaterialType.EMERALD, 0.5f)
            ));

            // Silver sphere
            scene.AddObject(new Sphere(
                new Vector3D(-1.8f, -0.9f, -1.2f),
                0.25f,
                Material.Create(MaterialType.SILVER, 0.7f)
            ));

            // Jade sphere
            scene.AddObject(new Sphere(
                new Vector3D(0f, -0.8f, 2.5f),
                0.3f,
                Material.Create(MaterialType.JADE, 0.4f)
            ));
        }
    }
}