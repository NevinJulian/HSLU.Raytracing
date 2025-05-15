using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Common;

const int width = 800;
const int height = 600;
const string filePath = "spheres.png";

var lightSource = new Vector3D(100, 100, 500);

var spheres = new List<Sphere>
{
    new Sphere(new Vector3D(width / 2, height / 2, 50), 200, new MyColor(0, 255, 0))
};

float AdjustBrightness(float baseBrightness, float factor)
{
    return Math.Clamp(baseBrightness * factor, 0, 1);
}

float brightnessFactor = 1.2f;

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
                var rayOrigin = new Vector3D(pixel.X, pixel.Y, 0);
                var rayDirection = new Vector3D(0, 0, 1);

                Vector3D oc = rayOrigin - sphere.Center;
                double a = rayDirection.Dot(rayDirection);
                double b = 2.0 * oc.Dot(rayDirection);
                double c = oc.Dot(oc) - sphere.Radius * sphere.Radius;
                double discriminant = b * b - 4 * a * c;

                if (discriminant >= 0)
                {
                    double sqrtD = Math.Sqrt(discriminant);
                    double t1 = (-b - sqrtD) / (2 * a);
                    double t2 = (-b + sqrtD) / (2 * a);
                    double pixelDepth = Math.Max(t1, t2);

                    if (pixelDepth > maxDepth)
                    {
                        maxDepth = pixelDepth;
                        double depthFactor = 1 - ((pixelDepth - 50) / 200.0);
                        depthFactor = Math.Clamp(depthFactor, 0.5, 1.0);

                        Vector3D hitPoint = rayOrigin + rayDirection * pixelDepth;
                        Vector3D normal = (hitPoint - sphere.Center).Normalize();

                        Vector3D lightDir = (lightSource - hitPoint).Normalize();
                        double shading = Math.Max(0, normal.Dot(lightDir));

                        double shadowFactor = Math.Pow(shading, 1.1) * brightnessFactor;
                        double visibilityFactor = Math.Clamp(shadowFactor + (brightnessFactor - 1) * 0.5, 0, 1);
                        double brightness = AdjustBrightness((float)(depthFactor * visibilityFactor), brightnessFactor);

                        byte red = (byte)Math.Min(sphere.Color.R * brightness, 255);
                        byte green = (byte)Math.Min(sphere.Color.G * brightness, 255);
                        byte blue = (byte)Math.Min(sphere.Color.B * brightness, 255);

                        finalColor = new Rgba32(red, green, blue);
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