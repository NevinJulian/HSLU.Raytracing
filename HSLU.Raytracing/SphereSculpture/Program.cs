using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Common;

const int width = 800;
const int height = 600;
const string filePath = "spheres.png";

// Light source positioned directly in front
var lightSource = new Vector3D(width / 2, height / 2, 1000);

// Define spheres with adjusted positions
var spheres = new List<Sphere>
{
    new Sphere(new Vector3D(width / 2, height / 2, 50), 200, MyColor.Blue), // Base blue sphere
    new Sphere(new Vector3D(width / 2 - 90, height / 2 - 90, 130), 100, new MyColor(0, 255, 255)), // Left cyan sphere moved slightly forward
    new Sphere(new Vector3D(width / 2 + 90, height / 2 - 90, 130), 100, new MyColor(0, 255, 255)), // Right cyan sphere moved slightly forward
    new Sphere(new Vector3D(width / 2 - 90, height / 2 - 90, 140), 60, new MyColor(0, 255, 0)), // Left green sphere moved forward
    new Sphere(new Vector3D(width / 2 + 90, height / 2 - 90, 140), 60, new MyColor(0, 255, 0)), // Right green sphere moved forward
    new Sphere(new Vector3D(width / 2, height / 2 + 50, 160), 70, MyColor.Red) // Center red sphere moved forward
};

// Create an ImageSharp image
using (var image = new Image<Rgba32>(width, height))
{
    // Fill background with black by default
    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            var pixel = new Vector2D(x, y);
            double maxDepth = double.NegativeInfinity;
            Rgba32 finalColor = Color.Black;
            double depthFactor = 1.0;
            bool pixelRendered = false;

            foreach (var sphere in spheres.OrderByDescending(s => s.Center.Z)) // Sort for correct depth layering
            {
                if (sphere.IsInSphere(pixel))
                {
                    // Calculate depth (Z-value) using the sphere equation
                    float dx = pixel.X - sphere.Center.X;
                    float dy = pixel.Y - sphere.Center.Y;
                    float dz = (float)Math.Sqrt(Math.Max(0, Math.Pow(sphere.Radius, 2) - (dx * dx + dy * dy))); // Solving for Z
                    float pixelDepth = sphere.Center.Z + dz;

                    if (pixelDepth > maxDepth) // Ensuring correct depth layering
                    {
                        maxDepth = pixelDepth;
                        depthFactor = 1 - ((pixelDepth - 50) / 200.0); // Depth cueing for smooth fading
                        depthFactor = Math.Clamp(depthFactor, 0.7, 1.0); // More variation in depth cueing

                        // Simulating a spherical shading effect
                        double normalZ = dz / sphere.Radius; // Normalized depth component
                        double shading = 0.5 + 0.5 * normalZ; // Smoother depth cueing
                        double brightness = depthFactor * shading; // Combine both effects

                        // Only render spheres where they are visible
                        if (sphere != spheres[0] && spheres[0].IsInSphere(pixel) && pixelDepth <= spheres[0].Center.Z)
                        {
                            continue; // Skip spheres that are behind the blue sphere
                        }

                        // Adjust color with depth cueing and shading
                        byte r = (byte)Math.Min(sphere.Color.R * brightness, 255);
                        byte g = (byte)Math.Min(sphere.Color.G * brightness, 255);
                        byte b = (byte)Math.Min(sphere.Color.B * brightness, 255);

                        finalColor = new Rgba32(r, g, b);
                        pixelRendered = true;
                    }
                }
            }

            // Ensure background stays black where no sphere is drawn
            image[x, y] = pixelRendered ? finalColor : Color.Black;
        }
    }

    // Save the image
    image.Save(filePath);
}

Console.WriteLine($"Image saved to {filePath}");
