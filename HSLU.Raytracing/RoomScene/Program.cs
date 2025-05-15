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
            const string filePath = "colored_room_scene.png";

            var scene = new Scene();
            var camera = new Camera(new Vector3D(0, 0, -3.0));

            // Reflective sphere material - highly reflective
            var sphereMaterial = new Material(
                MaterialType.SILVER,
                new MyColor(30, 30, 30),      // Dark ambient
                new MyColor(200, 200, 200),   // Gray diffuse
                new MyColor(255, 255, 255),   // Bright specular
                0.9f,                         // High shininess
                0.8f                          // Very high reflectivity
            );

            // Floating cube material - reflective and colorful
            var cubeMaterial = new Material(
                MaterialType.PEARL,
                new MyColor(40, 40, 40),      // Dark ambient
                new MyColor(220, 180, 220),   // Light purple/pink diffuse
                new MyColor(255, 255, 255),   // Bright specular
                0.8f,                         // High shininess
                0.7f                          // High reflectivity
            );

            // Wall materials - bright colors with low reflectivity

            // Cyan wall material (top and back)
            var cyanWallMaterial = new Material(
                MaterialType.CYAN_PLASTIC,
                new MyColor(0, 40, 40),       // Dark cyan ambient
                new MyColor(0, 220, 220),     // Bright cyan diffuse
                new MyColor(60, 60, 60),      // Low specular
                0.1f,                         // Low shininess
                0.1f                          // Low reflectivity
            );

            // Purple wall material (left)
            var purpleWallMaterial = new Material(
                MaterialType.RED_PLASTIC,
                new MyColor(40, 0, 40),       // Dark purple ambient
                new MyColor(180, 0, 180),     // Purple diffuse
                new MyColor(60, 60, 60),      // Low specular
                0.1f,                         // Low shininess
                0.1f                          // Low reflectivity
            );

            // Yellow wall material (right)
            var yellowWallMaterial = new Material(
                MaterialType.YELLOW_PLASTIC,
                new MyColor(40, 40, 0),       // Dark yellow ambient
                new MyColor(220, 220, 0),     // Bright yellow diffuse
                new MyColor(60, 60, 60),      // Low specular
                0.1f,                         // Low shininess
                0.1f                          // Low reflectivity
            );

            // Create a dark floor material
            var floorMaterial = new Material(
                MaterialType.BLACK_PLASTIC,
                new MyColor(5, 5, 5),         // Very dark ambient
                new MyColor(30, 30, 30),      // Dark diffuse
                new MyColor(0, 0, 0),         // No specular
                0.0f,                         // No shininess
                0.2f                          // Slight reflectivity
            );

            // Add spheres - position them to match the reference image
            // Left sphere
            scene.AddObject(new Sphere(
                new Vector3D(-1.0f, 0.0f, 0.5f),  // Position
                0.8f,                             // Radius
                sphereMaterial
            ));

            // Right sphere
            scene.AddObject(new Sphere(
                new Vector3D(1.0f, 0.0f, 0.5f),   // Position
                0.8f,                             // Radius
                sphereMaterial
            ));

            // Add floating cube - slightly rotated
            scene.AddObject(new RotatedCube(
                new Vector3D(0.0f, 0.9f, 0.2f),   // Position above the spheres
                0.4f,                             // Size
                cubeMaterial,
                30f, 45f, 15f                     // Rotation angles
            ));

            // Create the room walls
            float roomSize = 3.0f;

            // Floor (dark)
            scene.AddObject(new Triangle(
                new Vector3D(-roomSize, -roomSize, roomSize),
                new Vector3D(roomSize, -roomSize, roomSize),
                new Vector3D(-roomSize, -roomSize, -roomSize),
                floorMaterial
            ));
            scene.AddObject(new Triangle(
                new Vector3D(roomSize, -roomSize, roomSize),
                new Vector3D(roomSize, -roomSize, -roomSize),
                new Vector3D(-roomSize, -roomSize, -roomSize),
                floorMaterial
            ));

            // Ceiling (cyan)
            scene.AddObject(new Triangle(
                new Vector3D(-roomSize, roomSize, roomSize),
                new Vector3D(-roomSize, roomSize, -roomSize),
                new Vector3D(roomSize, roomSize, roomSize),
                cyanWallMaterial
            ));
            scene.AddObject(new Triangle(
                new Vector3D(roomSize, roomSize, roomSize),
                new Vector3D(-roomSize, roomSize, -roomSize),
                new Vector3D(roomSize, roomSize, -roomSize),
                cyanWallMaterial
            ));

            // Back wall (cyan)
            scene.AddObject(new Triangle(
                new Vector3D(-roomSize, -roomSize, roomSize),
                new Vector3D(-roomSize, roomSize, roomSize),
                new Vector3D(roomSize, -roomSize, roomSize),
                cyanWallMaterial
            ));
            scene.AddObject(new Triangle(
                new Vector3D(roomSize, -roomSize, roomSize),
                new Vector3D(-roomSize, roomSize, roomSize),
                new Vector3D(roomSize, roomSize, roomSize),
                cyanWallMaterial
            ));

            // Left wall (purple)
            scene.AddObject(new Triangle(
                new Vector3D(-roomSize, -roomSize, -roomSize),
                new Vector3D(-roomSize, roomSize, -roomSize),
                new Vector3D(-roomSize, -roomSize, roomSize),
                purpleWallMaterial
            ));
            scene.AddObject(new Triangle(
                new Vector3D(-roomSize, -roomSize, roomSize),
                new Vector3D(-roomSize, roomSize, -roomSize),
                new Vector3D(-roomSize, roomSize, roomSize),
                purpleWallMaterial
            ));

            // Right wall (yellow)
            scene.AddObject(new Triangle(
                new Vector3D(roomSize, -roomSize, -roomSize),
                new Vector3D(roomSize, -roomSize, roomSize),
                new Vector3D(roomSize, roomSize, -roomSize),
                yellowWallMaterial
            ));
            scene.AddObject(new Triangle(
                new Vector3D(roomSize, -roomSize, roomSize),
                new Vector3D(roomSize, roomSize, roomSize),
                new Vector3D(roomSize, roomSize, -roomSize),
                yellowWallMaterial
            ));

            // Set up lighting to illuminate the room
            // Main light from behind camera
            scene.AddLight(new Light(
                new Vector3D(0, 0, -2.5),
                new MyColor(255, 255, 255),  // White light
                1.0f                         // Full intensity
            ));

            // Secondary light from above
            scene.AddLight(new Light(
                new Vector3D(0, 2.5, 0),
                new MyColor(200, 200, 255),  // Slightly blue-tinted
                0.6f                         // Medium intensity
            ));

            // Left fill light
            scene.AddLight(new Light(
                new Vector3D(-2.5, 0, 0),
                new MyColor(255, 180, 255),  // Purple tinted
                0.5f                         // Medium intensity
            ));

            // Right fill light
            scene.AddLight(new Light(
                new Vector3D(2.5, 0, 0),
                new MyColor(255, 255, 180),  // Yellow tinted
                0.5f                         // Medium intensity
            ));

            // Create render settings with higher reflection depth for complex reflections
            var settings = new RenderSettings
            {
                Width = width,
                Height = height,
                MaxReflectionDepth = 8,       // Higher depth for multiple room reflections
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
    }
}