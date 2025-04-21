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
            const string filePath = "colored_room_transparent_adjusted.png";

            // Create scene
            var scene = new Scene();

            // Set up the camera - positioned to view inside the room
            var camera = new Camera(new Vector3D(0, 0, -3.0));

            // Create materials for each object and wall

            // Transparent silver sphere material - LESS TRANSPARENT
            var transparentSilverMaterial = new Material(
                MaterialType.SILVER,
                new MyColor(20, 20, 20),      // Dark ambient
                new MyColor(180, 180, 180),   // Silver diffuse
                new MyColor(220, 220, 220),   // Moderate specular
                0.8f,                         // High shininess
                0.4f,                         // Increased reflectivity
                0.2f                          // REDUCED transparency (40% instead of 70%)
            );

            // Transparent gold sphere material - LESS TRANSPARENT
            var transparentGoldMaterial = new Material(
                MaterialType.GOLD,
                new MyColor(30, 25, 5),       // Gold ambient
                new MyColor(212, 175, 55),    // Gold diffuse
                new MyColor(255, 215, 0),     // Gold specular
                0.8f,                         // High shininess
                0.4f,                         // Increased reflectivity
                0.2f                          // REDUCED transparency (40% instead of 70%)
            );

            // Transparent cube material - LESS TRANSPARENT
            var transparentCubeMaterial = new Material(
                MaterialType.PEARL,
                new MyColor(40, 40, 40),      // Dark ambient
                new MyColor(200, 200, 200),   // Light diffuse
                new MyColor(255, 255, 255),   // Bright specular
                0.7f,                         // High shininess
                0.3f,                         // Medium reflectivity
                0.3f                          // REDUCED transparency (50% instead of 80%)
            );

            // Middle sphere - slightly more transparent than the others
            var middleSphereMaterial = new Material(
                MaterialType.CYAN_PLASTIC,
                new MyColor(10, 30, 30),      // Dark cyan ambient
                new MyColor(70, 180, 180),    // Cyan diffuse
                new MyColor(200, 200, 200),   // Moderate specular
                0.7f,                         // High shininess
                0.2f,                         // Low reflectivity
                0.3f                          // Medium transparency (60%)
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

            // Add transparent spheres
            // Left sphere (silver transparent) - now less transparent
            scene.AddObject(new TransparentSphere(
                new Vector3D(-1.0f, 0.0f, 0.5f),  // Position
                0.8f,                             // Radius
                transparentSilverMaterial
            ));

            // Right sphere (gold transparent) - now less transparent
            scene.AddObject(new TransparentSphere(
                new Vector3D(1.0f, 0.0f, 0.5f),   // Position
                0.8f,                             // Radius
                transparentGoldMaterial
            ));

            // Add small transparent cyan sphere in middle
            scene.AddObject(new TransparentSphere(
                new Vector3D(0.0f, -0.2f, 0.0f),  // Position
                0.4f,                             // Radius
                middleSphereMaterial
            ));

            // Add transparent cube - now less transparent
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

            // Set up lighting to illuminate the room
            // Main light from behind camera
            scene.AddLight(new Light(
                new Vector3D(0, 0, -2.5),
                new MyColor(255, 255, 255),  // White light
                0.35f                        // Slightly increased intensity
            ));

            // Secondary light from above
            scene.AddLight(new Light(
                new Vector3D(0, 2.5, 0),
                new MyColor(200, 200, 255),  // Slightly blue-tinted
                0.25f                        // Slightly increased intensity
            ));

            // Left fill light - purple
            scene.AddLight(new Light(
                new Vector3D(-2.5, 0, 0),
                new MyColor(255, 180, 255),  // Purple tinted
                0.2f                         // Increased intensity
            ));

            // Right fill light - yellow
            scene.AddLight(new Light(
                new Vector3D(2.5, 0, 0),
                new MyColor(255, 255, 180),  // Yellow tinted
                0.2f                         // Increased intensity
            ));

            // Add a spot highlight for each sphere
            scene.AddLight(new Light(
                new Vector3D(-1.0, 2.0, -1.0),  // Above left sphere
                new MyColor(255, 255, 255),     // White light
                0.25f                           // Increased intensity
            ));

            scene.AddLight(new Light(
                new Vector3D(1.0, 2.0, -1.0),   // Above right sphere
                new MyColor(255, 255, 255),     // White light
                0.25f                           // Increased intensity
            ));

            // Create render settings with optimal parameters
            var settings = new RenderSettings
            {
                Width = width,
                Height = height,
                MaxReflectionDepth = 6,       // Increased depth for transparent effects
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