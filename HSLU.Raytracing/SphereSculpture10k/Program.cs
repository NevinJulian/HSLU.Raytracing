using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Common;

const int width = 800;
const int height = 600;
const string filePath = "spheres.png";
var random = new Random();

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

using (var image = new Image<Rgba32>(width, height))
{
    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            var pixel = new Vector2D(x, y);
            double maxDepth = double.NegativeInfinity;
            Rgba32 finalColor = Color.Black;
            bool pixelRendered = false;

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