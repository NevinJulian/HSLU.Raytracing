using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Common;

const int width = 800;
const int height = 600;
const string filePath = "spheres.png";

// Light source positioned directly in front (for a bright effect)
var lightSource = new Vector3D(width / 2, height / 2, 500);

// Define spheres with Z-depth
var spheres = new List<Sphere>
{
    new Sphere(new Vector3D(width / 2 - 50, height / 2 + 50, 50), 150, MyColor.Blue),
    new Sphere(new Vector3D(width / 2 + 50, height / 2 - 50, 100), 100, new MyColor(0,255,255))
};

// Create an ImageSharp image
using (var image = new Image<Rgba32>(width, height))
{
    // Fill background with black
    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            var pixel = new Vector2D(x, y);
            double maxDepth = double.NegativeInfinity;
            Rgba32 finalColor = Color.Black;
            double depthFactor = 1.0;

            foreach (var sphere in spheres.OrderByDescending(s => s.Center.Z)) // Sort for depth effect
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
                        depthFactor = 1 - ((pixelDepth - 50) / 200.0); // Depth cueing: fade towards the back
                        depthFactor = Math.Clamp(depthFactor, 0.8, 1.0);

                        // Adjust lighting to be mostly bright from the front
                        var normal = new Vector3D(dx, dy, dz).Normalize();
                        var lightDir = (lightSource - new Vector3D(pixel.X, pixel.Y, pixelDepth)).Normalize();
                        double brightness = Math.Max(0.8, normal.DotProduct(lightDir)); // Ensuring mostly bright shading

                        // Subtle shadow behind the cyan sphere
                        if (spheres[1].IsInSphere(new Vector2D(x + 5, y + 5)))
                        {
                            brightness *= 0.7;
                        }

                        // Adjust color with bright front light and subtle shadow
                        byte r = (byte)Math.Min(sphere.Color.R * brightness * depthFactor, 255);
                        byte g = (byte)Math.Min(sphere.Color.G * brightness * depthFactor, 255);
                        byte b = (byte)Math.Min(sphere.Color.B * brightness * depthFactor, 255);

                        finalColor = new Rgba32(r, g, b);
                    }
                }
            }

            image[x, y] = finalColor;
        }
    }

    // Save the image
    image.Save(filePath);
}

Console.WriteLine($"Image saved to {filePath}");
