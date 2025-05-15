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
            const string filePath = "colored_room_solid_objects.png";

            var scene = new Scene();
            var camera = new Camera(new Vector3D(0, 0, -3.0));

            // Solid silver sphere material (minimal reflectivity)
            var silverSphereMaterial = new Material(
                MaterialType.SILVER,
                new MyColor(20, 20, 20),      // Dark ambient
                new MyColor(180, 180, 180),   // Silver diffuse
                new MyColor(230, 230, 230),   // High specular
                0.8f,                         // High shininess
                0.1f,                         // Very low reflectivity
                0.0f                          // No transparency (completely opaque)
            );

            // Solid gold sphere material (minimal reflectivity)
            var goldSphereMaterial = new Material(
                MaterialType.GOLD,
                new MyColor(30, 25, 5),       // Gold ambient
                new MyColor(212, 175, 55),    // Gold diffuse
                new MyColor(255, 215, 0),     // Gold specular
                0.8f,                         // High shininess
                0.1f,                         // Very low reflectivity
                0.0f                          // No transparency (completely opaque)
            );

            // Slightly transparent cube material
            var transparentCubeMaterial = new Material(
                MaterialType.PEARL,
                new MyColor(40, 40, 40),      // Dark ambient
                new MyColor(200, 200, 200),   // Light diffuse
                new MyColor(255, 255, 255),   // Bright specular
                0.8f,                         // High shininess
                0.1f,                         // Very low reflectivity
                0.3f                          // Low transparency (mostly solid)
            );

            // Slightly transparent cyan sphere material
            var transparentCyanMaterial = new Material(
                MaterialType.CYAN_PLASTIC,
                new MyColor(10, 30, 30),      // Dark cyan ambient
                new MyColor(60, 180, 180),    // Cyan diffuse
                new MyColor(180, 230, 230),   // Cyan specular
                0.8f,                         // High shininess
                0.1f,                         // Very low reflectivity
                0.5f                          // Medium transparency
            );

            // Wall materials - bright colors with NO reflectivity or transparency

            // Cyan wall material (top and back)
            var cyanWallMaterial = new Material(
                MaterialType.CYAN_PLASTIC,
                new MyColor(0, 40, 40),       // Dark cyan ambient
                new MyColor(0, 180, 180),     // Cyan diffuse
                new MyColor(20, 20, 20),      // Very low specular
                0.0f,                         // No shininess
                0.0f,                         // No reflectivity
                0.0f                          // No transparency
            );

            // Purple wall material (left)
            var purpleWallMaterial = new Material(
                MaterialType.RED_PLASTIC,
                new MyColor(40, 0, 40),       // Dark purple ambient
                new MyColor(130, 0, 130),     // Purple diffuse
                new MyColor(20, 20, 20),      // Very low specular
                0.0f,                         // No shininess
                0.0f,                         // No reflectivity
                0.0f                          // No transparency
            );

            // Yellow wall material (right)
            var yellowWallMaterial = new Material(
                MaterialType.YELLOW_PLASTIC,
                new MyColor(40, 40, 0),       // Dark yellow ambient
                new MyColor(180, 180, 0),     // Yellow diffuse
                new MyColor(20, 20, 20),      // Very low specular
                0.0f,                         // No shininess
                0.0f,                         // No reflectivity
                0.0f                          // No transparency
            );

            // Create a dark floor material
            var floorMaterial = new Material(
                MaterialType.BLACK_PLASTIC,
                new MyColor(5, 5, 5),         // Very dark ambient
                new MyColor(20, 20, 20),      // Very dark diffuse
                new MyColor(0, 0, 0),         // No specular
                0.0f,                         // No shininess
                0.0f,                         // No reflectivity
                0.0f                          // No transparency
            );

            // Left sphere (solid silver, minimal reflection)
            scene.AddObject(new Sphere(
                new Vector3D(-1.0f, 0.0f, 0.5f),  // Position
                0.8f,                             // Radius
                silverSphereMaterial
            ));

            // Right sphere (solid gold, minimal reflection)
            scene.AddObject(new Sphere(
                new Vector3D(1.0f, 0.0f, 0.5f),   // Position
                0.8f,                             // Radius
                goldSphereMaterial
            ));

            // Add small transparent cyan sphere in middle
            scene.AddObject(new TransparentSphere(
                new Vector3D(0.0f, -0.2f, 0.0f),  // Position
                0.4f,                             // Radius
                transparentCyanMaterial
            ));

            // Add slightly transparent cube floating above
            scene.AddObject(new TransparentCube(
                new Vector3D(0.0f, 1.0f, 0.3f),   // Position above the spheres
                0.4f,                             // Size
                transparentCubeMaterial,
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

            // Main light from behind camera
            scene.AddLight(new Light(
                new Vector3D(0, 0, -2.5),
                new MyColor(255, 255, 255),  // White light
                0.35f                        // Medium intensity
            ));

            // Secondary light from above
            scene.AddLight(new Light(
                new Vector3D(0, 2.5, 0),
                new MyColor(200, 200, 255),  // Slightly blue-tinted
                0.25f                        // Low-medium intensity
            ));

            // Left fill light - purple
            scene.AddLight(new Light(
                new Vector3D(-2.5, 0, 0),
                new MyColor(255, 180, 255),  // Purple tinted
                0.2f                         // Low intensity
            ));

            // Right fill light - yellow
            scene.AddLight(new Light(
                new Vector3D(2.5, 0, 0),
                new MyColor(255, 255, 180),  // Yellow tinted
                0.2f                         // Low intensity
            ));

            // Add a small bright spot highlight for each sphere (like in image 2)
            scene.AddLight(new Light(
                new Vector3D(-1.0, 1.0, -0.5),  // Above and in front of left sphere
                new MyColor(255, 255, 255),     // White light
                0.15f                          // Very focused intensity
            ));

            scene.AddLight(new Light(
                new Vector3D(1.0, 1.0, -0.5),   // Above and in front of right sphere
                new MyColor(255, 255, 255),     // White light
                0.15f                          // Very focused intensity
            ));

            var settings = new RenderSettings
            {
                Width = width,
                Height = height,
                MaxReflectionDepth = 4,
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