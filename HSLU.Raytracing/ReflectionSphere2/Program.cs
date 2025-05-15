using Common;
using System;

namespace SpheresRender
{
    class Program
    {
        static void Main(string[] args)
        {
            const int width = 1600;
            const int height = 900;
            const string filePath = "gray_green_cube_scene.png";

            var scene = new Scene();
            var camera = new Camera(new Vector3D(0, 1.5, -5.0));

            // Gray sphere material - silver-like with high reflectivity
            var graySphereaterial = new Material(
                MaterialType.SILVER,
                new MyColor(30, 30, 30),      // Dark ambient
                new MyColor(180, 180, 180),   // Gray diffuse
                new MyColor(255, 255, 255),   // Bright specular
                0.8f,                         // High shininess
                0.7f                          // High reflectivity
            );

            // Green sphere material - emerald-like with reflections
            var greenSphereMaterial = new Material(
                MaterialType.EMERALD,
                new MyColor(10, 40, 10),      // Dark green ambient
                new MyColor(40, 180, 40),     // Bright green diffuse
                new MyColor(200, 255, 200),   // Green-tinted specular
                0.7f,                         // High shininess
                0.5f                          // Medium reflectivity
            );

            // Purple cube material
            var purpleCubeMaterial = new Material(
                MaterialType.PEARL,
                new MyColor(50, 10, 50),      // Dark purple ambient
                new MyColor(180, 40, 180),    // Purple diffuse
                new MyColor(200, 150, 200),   // Purple-tinted specular
                0.5f,                         // Medium shininess
                0.4f                          // Medium reflectivity
            );

            // Add left gray sphere
            scene.AddObject(new Sphere(
                new Vector3D(-2.0f, 1.0f, 0),  // Left position
                1.2f,                          // Radius
                graySphereaterial
            ));

            // Add purple cube in the middle
            scene.AddObject(new RotatedCube(
                new Vector3D(0.0f, 0.8f, 0),   // Center position
                0.8f,                          // Size
                purpleCubeMaterial,
                15f, 45f, 10f                  // Rotation angles
            ));

            // Add right green sphere
            scene.AddObject(new Sphere(
                new Vector3D(2.0f, 1.0f, 0),   // Right position
                1.2f,                          // Radius
                greenSphereMaterial
            ));

            // Create a dark floor plane (invisible in the image but needed for reflections)
            var floorMaterial = new Material(
                MaterialType.BLACK_PLASTIC,
                new MyColor(5, 5, 5),        // Very dark ambient
                new MyColor(20, 20, 20),     // Very dark diffuse
                new MyColor(0, 0, 0),        // No specular
                0.0f,                        // No shininess
                0.1f                         // Slight reflectivity
            );

            // Create a large floor plane below the objects
            float floorY = -0.5f;
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

            // Set up lighting to match the highlights visible in the image
            // Main light from top-left
            scene.AddLight(new Light(
                new Vector3D(-3, 5, -3),
                new MyColor(255, 255, 255),  // White light
                1.0f                         // Full intensity
            ));

            // Secondary light from top-right
            scene.AddLight(new Light(
                new Vector3D(3, 5, -3),
                new MyColor(220, 220, 240),  // Slightly blue-tinted
                0.7f                         // Medium-high intensity
            ));

            // Fill light from back to create rim highlights
            scene.AddLight(new Light(
                new Vector3D(0, 3, 5),
                new MyColor(200, 200, 200),  // White light
                0.5f                         // Medium intensity
            ));

            var settings = new RenderSettings
            {
                Width = width,
                Height = height,
                MaxReflectionDepth = 5,       // Good depth for reflections
                OutputFilename = filePath.Replace(".png", ""),
                OutputFormat = "png",
                NumThreads = Environment.ProcessorCount
            };

            var rayTracer = new RayTracer();
            Console.WriteLine("Starting rendering...");
            rayTracer.RenderScene(scene, camera, settings);
            Console.WriteLine($"Image saved to {filePath}");
        }
    }
}