using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Common;
using static Common.RayTracer;

const int width = 800;
const int height = 600;
const string filePath = "dramatic_shadows.png";

// Create a visible floor that extends toward the camera
var floorPlane1 = new Triangle(
    new Vector3D(0, 400, 0),          // Close to camera
    new Vector3D(800, 400, 0),
    new Vector3D(0, 400, 250),        // Far away
    new MyColor(230, 230, 230)        // Very light gray to show shadows clearly
);

var floorPlane2 = new Triangle(
    new Vector3D(800, 400, 0),
    new Vector3D(800, 400, 250),
    new Vector3D(0, 400, 250),
    new MyColor(230, 230, 230)
);

// Position objects higher above the floor to cast longer shadows
var sphere = new Sphere(
    new Vector3D(500f, 250f, 150f),   // Higher position
    80,                               // Size
    new MyColor(0, 120, 0)            // Dark green
);

// Make the cube larger and higher
var cube = new Cube(
    new Vector3D(250f, 250f, 150f),   // Higher position
    80f,                              // Size
    new MyColor(20, 20, 150),         // Dark blue
    30f                               // Rotation angle
);

// Create a single, strong directional light positioned to create long shadows
var lights = new List<LightSource>
{
    new LightSource(
        new Vector3D(400, 100, 50),    // Light is above and slightly behind
        2.5f,                          // Very bright
        new MyColor(255, 255, 255)     // White light
    )
};

// Only include the floor and objects
var sceneObjects = new List<IRaycastable>
{
    floorPlane1,
    floorPlane2,
    sphere,
    cube
};

// Create render settings with minimal ambient light to emphasize shadows
var renderSettings = new RenderSettings
{
    BackgroundColor = new MyColor(30, 30, 40),    // Dark blue-gray background
    AmbientLight = new MyColor(15, 15, 20),       // Very low ambient for stark shadows
    SpecularPower = 64.0f,                        // Very sharp highlights
    SpecularIntensity = 1.0f                      // Maximum specular intensity
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
            // Create ray for this pixel
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