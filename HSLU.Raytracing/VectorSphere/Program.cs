using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Common;


const int width = 800;
const int height = 500;
const int sphereCenterX = width / 2;
const int sphereCenterY = height / 2;
const int sphereCenterZ = 0;
const int sphereRadius = 100;
const string filePath = "skia_raster_image.png";

var sphereCenter = new Vector3D(sphereCenterX, sphereCenterY, sphereCenterZ);

static bool IsInSphere(Vector2D pixel, Vector3D sphereCenter, int radius)
{
    int dx = pixel.X - sphereCenter.X;
    int dy = pixel.Y - sphereCenter.Y;

    return (dx * dx + dy * dy) <= Math.Pow(radius, 2);
}

var bitmap = new Image<Rgba32>(width, height);

for (int y = 0; y < height; y++)
{
    for (int x = 0; x < width; x++)
    {
        var color = Color.Black;

        var pixel = new Vector2D(x, y);
        if (IsInSphere(pixel, sphereCenter, sphereRadius))
        {
            color = Color.Green;
        }
        bitmap[x, y] = color;
    }
}

await bitmap.SaveAsPngAsync(filePath);

Console.WriteLine($"Image saved to {filePath}");