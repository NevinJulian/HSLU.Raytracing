using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Common;

const int width = 800;
const int height = 600;
const string filePath = "spheres_and_cube.png";
var random = new Random();

// Create spheres
var spheres = new List<Sphere>();
int sphereCount = 100;
for (int i = 0; i < sphereCount; i++)
{
    int radius = random.Next(20, 50);
    var position = new Vector3D(
        random.Next(radius, width - radius),
        random.Next(radius, height - radius),
        random.Next(20, 300));
    var color = new MyColor(random.Next(0, 256), random.Next(0, 256), random.Next(0, 256));
    spheres.Add(new Sphere(position, radius, color));
}

// Create a blue cube in the scene - larger and rotated
var cubeCenter = new Vector3D(400f, 300f, 150f);
float cubeSize = 120f; // Increased size
var cubeColor = new MyColor(0, 0, 255); // Blue
var cube = new Cube(cubeCenter, cubeSize, cubeColor, 45f); // 45-degree rotation

using (var image = new Image<Rgba32>(width, height))
{
    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            var pixel = new Vector2D(x, y);
            float maxDepth = float.NegativeInfinity;
            Rgba32 finalColor = Color.Black;
            bool pixelRendered = false;
            Vector3D normal = new Vector3D();

            // Check cube intersection
            var (cubeHit, cubeDepth, cubeNormal) = cube.IntersectRay(pixel);
            if (cubeHit && cubeDepth > maxDepth)
            {
                maxDepth = cubeDepth;
                normal = cubeNormal;

                // Apply diffuse lighting based on normal
                float normalZ = Math.Abs(cubeNormal.Z); // Use absolute value for lighting both sides
                double shading = 0.3 + 0.7 * normalZ;

                // Apply depth factor for distance effect
                double depthFactor = 1 - ((cubeDepth - 20) / 300.0);
                depthFactor = Math.Clamp(depthFactor, 0.8, 1.0);

                double brightness = depthFactor * shading;
                byte r = (byte)Math.Min(cube.Color.R * brightness, 255);
                byte g = (byte)Math.Min(cube.Color.G * brightness, 255);
                byte b = (byte)Math.Min(cube.Color.B * brightness, 255);
                finalColor = new Rgba32(r, g, b);
                pixelRendered = true;
            }

            // Check sphere intersections
            foreach (var sphere in spheres.OrderByDescending(s => s.Center.Z))
            {
                if (sphere.IsInSphere(pixel))
                {
                    float dx = pixel.X - sphere.Center.X;
                    float dy = pixel.Y - sphere.Center.Y;
                    float dz = (float)Math.Sqrt(Math.Max(0, Math.Pow(sphere.Radius, 2) - (dx * dx + dy * dy)));
                    float pixelDepth = sphere.Center.Z + dz;

                    if (pixelDepth > maxDepth)
                    {
                        maxDepth = pixelDepth;
                        double depthFactor = 1 - ((pixelDepth - 20) / 300.0);
                        depthFactor = Math.Clamp(depthFactor, 0.8, 1.0);
                        double normalZ = dz / sphere.Radius;
                        double shading = 0.3 + 0.7 * normalZ;
                        double brightness = depthFactor * shading;
                        byte r = (byte)Math.Min(sphere.Color.R * brightness, 255);
                        byte g = (byte)Math.Min(sphere.Color.G * brightness, 255);
                        byte b = (byte)Math.Min(sphere.Color.B * brightness, 255);
                        finalColor = new Rgba32(r, g, b);
                        pixelRendered = true;
                    }
                }
            }

            image[x, y] = pixelRendered ? finalColor : Color.Black;
        }
    }

    image.Save(filePath);
}

Console.WriteLine($"Image saved to {filePath}");