using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Common;
using static Common.RayTracer;

const int width = 800;
const int height = 600;
const string filePath = "room_scene_with_shadows.png";

// Create a back wall (at Z=600, similar to the working implementation)
var backWall1 = new Triangle(
    new Vector3D(0, 600, 600),         // Top-left
    new Vector3D(800, 600, 600),       // Top-right
    new Vector3D(0, 0, 600),           // Bottom-left
    new MyColor(100, 100, 200)         // Light blue
);

var backWall2 = new Triangle(
    new Vector3D(800, 600, 600),       // Top-right
    new Vector3D(800, 0, 600),         // Bottom-right
    new Vector3D(0, 0, 600),           // Bottom-left
    new MyColor(100, 100, 200)         // Light blue
);

// Add a floor plane (bottom of room)
var floor1 = new Triangle(
    new Vector3D(0, 600, 0),           // Back-left
    new Vector3D(800, 600, 0),         // Back-right
    new Vector3D(0, 600, 600),         // Front-left
    new MyColor(180, 180, 180)         // Light gray floor
);

var floor2 = new Triangle(
    new Vector3D(800, 600, 0),         // Back-right
    new Vector3D(800, 600, 600),       // Front-right
    new Vector3D(0, 600, 600),         // Front-left
    new MyColor(180, 180, 180)         // Light gray floor
);

// Position sphere and cube in the room
var sphere = new Sphere(
    new Vector3D(400, 300, 400),       // Left side of room
    100,                               // Size
    new MyColor(0, 150, 0)             // Green
);

var cube = new Cube(
    new Vector3D(100, 300, 200),       // Right side of room
    80f,                               // Size
    new MyColor(20, 20, 150),          // Dark blue
    30f                                // Rotation angle
);

// Create light sources positioned as in the working implementation
var lights = new List<LightSource>
{
    new LightSource(
        new Vector3D(400, 0, -250),    // Same as working example
        1.0f,
        new MyColor(128, 128, 128)     // Medium gray (equivalent to 0.5, 0.5, 0.5 in the example)
    ),
    new LightSource(
        new Vector3D(0, -500, -500),   // Same as working example
        0.8f,
        new MyColor(153, 153, 128)     // Equivalent to (0.6, 0.6, 0.5)
    )
};

// Scene with all objects
var sceneObjects = new List<IRaycastable>
{
    backWall1,
    backWall2,
    floor1,
    floor2,
    sphere,
    cube
};

// Create render settings similar to what might be in the working implementation
var renderSettings = new RenderSettings
{
    BackgroundColor = new MyColor(30, 30, 40),    // Dark background
    AmbientLight = new MyColor(60, 60, 60),       // Medium ambient light (allows some detail in shadows)
    SpecularPower = 40.0f,
    SpecularIntensity = 0.7f
};

// Create ray tracer
var rayTracer = new RayTracer(sceneObjects, lights, renderSettings);

// Create image
using (var image = new Image<Rgba32>(width, height))
{
    // Render scene
    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            // Create ray for this pixel - identical to the working example
            var ray = new Ray(
                new Vector3D(x, y, 0),
                new Vector3D(0, 0, 1)
            );

            // Trace the ray
            var color = rayTracer.TraceRay(ray);

            // Set pixel color
            image[x, y] = new Rgba32(
                (byte)color.R,
                (byte)color.G,
                (byte)color.B
            );
        }
    }

    // Save the image
    image.Save(filePath);
}

Console.WriteLine($"Image saved to {filePath}");