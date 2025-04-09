using Common;

const int width = 800;
const int height = 600;
const string filePath = "room_scene_with_shadows.png";

// Create a back wall (at Z=500)
var backWall1 = new Triangle(
    new Vector3D(-2000, 2000, 500),         // Top-left
    new Vector3D(2000, 2000, 500),       // Top-right
    new Vector3D(-2000, -2000, 500),           // Bottom-left
    new MyColor(150, 150, 220)         // Light blue
);

var backWall2 = new Triangle(
    new Vector3D(2000, 2000, 500),       // Top-right
    new Vector3D(2000, -2000, 500),         // Bottom-right
    new Vector3D(-2000, -2000, 500),           // Bottom-left
    new MyColor(150, 150, 220)         // Light blue
);

// Add a floor plane with corrected vertex order
var floor1 = new Triangle(
    new Vector3D(-2000, 0, 0),           // Back-left
    new Vector3D(-2000, 0, 500),         // Front-left
    new Vector3D(2000, 0, 0),            // Back-right
    new MyColor(255, 255, 220)           // Light gray floor
);

var floor2 = new Triangle(
    new Vector3D(2000, 0, 0),            // Back-right
    new Vector3D(-2000, 0, 500),         // Front-left
    new Vector3D(2000, 0, 500),          // Front-right
    new MyColor(255, 255, 220)           // Light gray floor
);

// Position sphere and cube in the room
var sphere = new Sphere(
    new Vector3D(500, 350, 250),       // Right side of room
    70,                                // Size
    new MyColor(220, 30, 30)           // Red
);

var cube = new RotatedCube(
    new Vector3D(300, 390, 200),       // Left side of room
    80f,                               // Size
    new MyColor(30, 30, 150),          // Dark blue 
    15f, 45f, 0f                       // Rotation angles
);

// Create light sources with better positioning
var lights = new List<Light>
{
    new Light(
        new Vector3D(400, 500, -50),
        new MyColor(255, 255, 255),     // White light
        1.0f
    ),
    new Light(
        new Vector3D(600, 700, -100),
        new MyColor(255, 255, 200),     // Warm light
        0.7f
    )
};

// Create scene
var scene = new Scene();

// Add objects to the scene
scene.AddObject(backWall1);
scene.AddObject(backWall2);
scene.AddObject(floor1);
scene.AddObject(floor2);
scene.AddObject(sphere);
scene.AddObject(cube);

// Add lights to the scene
foreach (var light in lights)
{
    scene.AddLight(light);
}

// Create camera with a better viewing position
var camera = new Camera(new Vector3D(400, 250, -150));

// Create render settings with optimal parameters
var settings = new RenderSettings
{
    Width = width,
    Height = height,
    MaxReflectionDepth = 0,  // Disabling reflections as requested
    OutputFilename = filePath.Replace(".png", ""),
    OutputFormat = "png"
};

// Create ray tracer and render the scene
var rayTracer = new RayTracer();
rayTracer.RenderScene(scene, camera, settings);

Console.WriteLine($"Image saved to {filePath}");