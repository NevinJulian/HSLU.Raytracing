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
            const string filePath = "colored_room_very_dim.png";

            var scene = new Scene();
            var camera = new Camera(new Vector3D(0, 0, -3.0));


            // Silver sphere material - diffuse with reflective properties
            var silverSphereMaterial = new Material(
                MaterialType.SILVER,
                new MyColor(20, 20, 20),      // Slightly darker ambient
                new MyColor(180, 180, 180),   // Silver diffuse
                new MyColor(220, 220, 220),   // Moderate specular (not too mirror-like)
                0.6f,                         // Medium shininess
                0.4f                          // Medium reflectivity
            );

            // Gold sphere material - diffuse with reflective properties
            var goldSphereMaterial = new Material(
                MaterialType.GOLD,
                new MyColor(30, 25, 5),       // Slightly darker gold ambient
                new MyColor(212, 175, 55),    // Gold diffuse
                new MyColor(255, 215, 0),     // Gold specular
                0.6f,                         // Medium shininess
                0.4f                          // Medium reflectivity
            );

            // Cube material - colorful with sides matching walls
            var cubeMaterial = new Material(
                MaterialType.PEARL,
                new MyColor(40, 40, 40),      // Dark ambient
                new MyColor(200, 200, 200),   // Light diffuse
                new MyColor(255, 255, 255),   // Bright specular
                0.7f,                         // High shininess
                0.3f                          // Medium-low reflectivity
            );

            // Wall materials - bright colors with NO reflectivity

            // Cyan wall material (top and back)
            var cyanWallMaterial = new Material(
                MaterialType.CYAN_PLASTIC,
                new MyColor(0, 40, 40),       // Dark cyan ambient
                new MyColor(0, 180, 180),     // Slightly dimmer cyan diffuse
                new MyColor(20, 20, 20),      // Very low specular
                0.0f,                         // No shininess
                0.0f                          // No reflectivity
            );

            // Purple wall material (left)
            var purpleWallMaterial = new Material(
                MaterialType.RED_PLASTIC,
                new MyColor(40, 0, 40),       // Dark purple ambient
                new MyColor(130, 0, 130),     // Slightly dimmer purple diffuse
                new MyColor(20, 20, 20),      // Very low specular
                0.0f,                         // No shininess
                0.0f                          // No reflectivity
            );

            // Yellow wall material (right)
            var yellowWallMaterial = new Material(
                MaterialType.YELLOW_PLASTIC,
                new MyColor(40, 40, 0),       // Dark yellow ambient
                new MyColor(180, 180, 0),     // Slightly dimmer yellow diffuse
                new MyColor(20, 20, 20),      // Very low specular
                0.0f,                         // No shininess
                0.0f                          // No reflectivity
            );

            // Create a dark floor material
            var floorMaterial = new Material(
                MaterialType.BLACK_PLASTIC,
                new MyColor(5, 5, 5),         // Very dark ambient
                new MyColor(20, 20, 20),      // Very dark diffuse
                new MyColor(0, 0, 0),         // No specular
                0.0f,                         // No shininess
                0.0f                          // No reflectivity
            );

            // Add spheres - matching positions in the reference image
            // Left sphere (silver)
            scene.AddObject(new Sphere(
                new Vector3D(-1.0f, 0.0f, 0.5f),  // Position
                0.8f,                             // Radius
                silverSphereMaterial
            ));

            // Right sphere (gold)
            scene.AddObject(new Sphere(
                new Vector3D(1.0f, 0.0f, 0.5f),   // Position
                0.8f,                             // Radius
                goldSphereMaterial
            ));

            // Add multi-colored cube
            // Create a custom cube with different colored faces
            scene.AddObject(new CustomColoredCube(
                new Vector3D(0.0f, 1.0f, 0.3f),   // Position above the spheres
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

            // Set up lighting to illuminate the room - GREATLY REDUCED INTENSITY
            // Main light from behind camera
            scene.AddLight(new Light(
                new Vector3D(0, 0, -2.5),
                new MyColor(255, 255, 255),  // White light
                0.25f                        // FURTHER REDUCED from 0.5f to 0.25f
            ));

            // Secondary light from above
            scene.AddLight(new Light(
                new Vector3D(0, 2.5, 0),
                new MyColor(200, 200, 255),  // Slightly blue-tinted
                0.15f                        // FURTHER REDUCED from 0.35f to 0.15f
            ));

            // Left fill light
            scene.AddLight(new Light(
                new Vector3D(-2.5, 0, 0),
                new MyColor(255, 180, 255),  // Purple tinted
                0.15f                        // FURTHER REDUCED from 0.3f to 0.15f
            ));

            // Right fill light
            scene.AddLight(new Light(
                new Vector3D(2.5, 0, 0),
                new MyColor(255, 255, 180),  // Yellow tinted
                0.15f                        // FURTHER REDUCED from 0.3f to 0.15f
            ));

            // Add a spot highlight for each sphere
            scene.AddLight(new Light(
                new Vector3D(-1.0, 2.0, -1.0),  // Above left sphere
                new MyColor(255, 255, 255),     // White light
                0.2f                            // FURTHER REDUCED from 0.4f to 0.2f
            ));

            scene.AddLight(new Light(
                new Vector3D(1.0, 2.0, -1.0),   // Above right sphere
                new MyColor(255, 255, 255),     // White light
                0.2f                            // FURTHER REDUCED from 0.4f to 0.2f
            ));

            // Create render settings with optimal parameters
            var settings = new RenderSettings
            {
                Width = width,
                Height = height,
                MaxReflectionDepth = 5,       // Medium reflection depth is sufficient
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

    // Custom class to create a cube with different colored faces
    public class CustomColoredCube : IRaycastable
    {
        public Vector3D Center { get; }
        public float Size { get; }
        public MyColor Color { get; }
        public Material Material { get; }
        public int ObjectId { get; set; }

        private readonly float RotationX;
        private readonly float RotationY;
        private readonly float RotationZ;
        private readonly List<Triangle> Triangles;

        public CustomColoredCube(Vector3D center, float size, Material baseMaterial,
                                float rotationX = 0f, float rotationY = 0f, float rotationZ = 0f)
        {
            Center = center;
            Size = size;
            Material = baseMaterial;
            Color = baseMaterial.Diffuse;
            RotationX = rotationX * MathF.PI / 180f;
            RotationY = rotationY * MathF.PI / 180f;
            RotationZ = rotationZ * MathF.PI / 180f;
            Triangles = CreateTriangles();
        }

        private List<Triangle> CreateTriangles()
        {
            List<Triangle> result = new(12);

            float hs = Size / 2.0f;
            Vector3D[] vertices = new Vector3D[8];
            vertices[0] = new Vector3D(-hs, -hs, -hs); // bottom-left-back
            vertices[1] = new Vector3D(hs, -hs, -hs);  // bottom-right-back
            vertices[2] = new Vector3D(hs, hs, -hs);   // top-right-back
            vertices[3] = new Vector3D(-hs, hs, -hs);  // top-left-back
            vertices[4] = new Vector3D(-hs, -hs, hs);  // bottom-left-front
            vertices[5] = new Vector3D(hs, -hs, hs);   // bottom-right-front
            vertices[6] = new Vector3D(hs, hs, hs);    // top-right-front
            vertices[7] = new Vector3D(-hs, hs, hs);   // top-left-front

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = RotateVertex(vertices[i]);
                vertices[i] = new Vector3D(
                    vertices[i].X + Center.X,
                    vertices[i].Y + Center.Y,
                    vertices[i].Z + Center.Z
                );
            }

            var purpleMaterial = new Material(
                MaterialType.RED_PLASTIC,
                new MyColor(40, 0, 40),       // Ambient
                new MyColor(180, 30, 180),    // Purple
                Material.Specular,            // Same specular as base
                Material.Shininess,           // Same shininess
                Material.Reflectivity         // Same reflectivity
            );

            var yellowMaterial = new Material(
                MaterialType.YELLOW_PLASTIC,
                new MyColor(40, 40, 0),       // Ambient
                new MyColor(200, 200, 30),    // Yellow
                Material.Specular,            // Same specular as base
                Material.Shininess,           // Same shininess
                Material.Reflectivity         // Same reflectivity
            );

            var cyanMaterial = new Material(
                MaterialType.CYAN_PLASTIC,
                new MyColor(0, 40, 40),       // Ambient
                new MyColor(30, 180, 180),    // Cyan
                Material.Specular,            // Same specular as base
                Material.Shininess,           // Same shininess
                Material.Reflectivity         // Same reflectivity
            );

            // Create a base white material
            var whiteMaterial = Material;

            // Front face (white)
            result.Add(new Triangle(vertices[4], vertices[5], vertices[6], whiteMaterial));
            result.Add(new Triangle(vertices[4], vertices[6], vertices[7], whiteMaterial));

            // Back face (cyan)
            result.Add(new Triangle(vertices[1], vertices[0], vertices[3], cyanMaterial));
            result.Add(new Triangle(vertices[1], vertices[3], vertices[2], cyanMaterial));

            // Left face (purple)
            result.Add(new Triangle(vertices[0], vertices[4], vertices[7], purpleMaterial));
            result.Add(new Triangle(vertices[0], vertices[7], vertices[3], purpleMaterial));

            // Right face (yellow)
            result.Add(new Triangle(vertices[5], vertices[1], vertices[2], yellowMaterial));
            result.Add(new Triangle(vertices[5], vertices[2], vertices[6], yellowMaterial));

            // Top face (cyan)
            result.Add(new Triangle(vertices[7], vertices[6], vertices[2], cyanMaterial));
            result.Add(new Triangle(vertices[7], vertices[2], vertices[3], cyanMaterial));

            // Bottom face (white)
            result.Add(new Triangle(vertices[0], vertices[1], vertices[5], whiteMaterial));
            result.Add(new Triangle(vertices[0], vertices[5], vertices[4], whiteMaterial));

            foreach (var triangle in result)
            {
                triangle.ParentId = this.ObjectId;
            }

            return result;
        }

        private Vector3D RotateVertex(Vector3D v)
        {
            // Apply X rotation
            float y1 = v.Y * MathF.Cos(RotationX) - v.Z * MathF.Sin(RotationX);
            float z1 = v.Y * MathF.Sin(RotationX) + v.Z * MathF.Cos(RotationX);

            // Apply Y rotation
            float x2 = v.X * MathF.Cos(RotationY) + z1 * MathF.Sin(RotationY);
            float z2 = -v.X * MathF.Sin(RotationY) + z1 * MathF.Cos(RotationY);

            // Apply Z rotation
            float x3 = x2 * MathF.Cos(RotationZ) - y1 * MathF.Sin(RotationZ);
            float y3 = x2 * MathF.Sin(RotationZ) + y1 * MathF.Cos(RotationZ);

            return new Vector3D(x3, y3, z2);
        }

        public (bool hasHit, float intersectionDistance) Intersect(Ray ray)
        {
            bool hasHit = false;
            float closestDistance = float.MaxValue;

            // Check intersection with all triangles
            foreach (Triangle triangle in Triangles)
            {
                var (triangleHit, distance) = triangle.Intersect(ray);
                if (triangleHit && distance > 0.001f && distance < closestDistance)
                {
                    hasHit = true;
                    closestDistance = distance;
                }
            }

            return (hasHit, closestDistance);
        }

        public Vector3D GetNormal(Vector3D intersectionPoint)
        {
            // Find the triangle closest to intersection point
            Triangle? closestTriangle = null;
            float minDistance = float.MaxValue;

            foreach (var triangle in Triangles)
            {
                // Calculate distance to triangle plane
                float distance = Math.Abs(triangle.Normal.Dot(intersectionPoint - triangle.V1));

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTriangle = triangle;
                }
            }

            // Return normal of closest triangle, ensuring it's properly normalized
            return closestTriangle?.Normal.Normalize() ?? new Vector3D(0, 1, 0);
        }
    }
}