using SkiaSharp;
using System.Numerics;
using VectorCircle;

const int width = 800;
const int height = 500;
const int circleCenterX = width / 2;
const int circleCenterY = height / 2;
const int circleRadius = 100;
const string filePath = "skia_raster_image.png";

var circleCenter = new Vector2(circleCenterX, circleCenterY);

static bool IsInCircle(Vector2 pixel, Vector2 circleCenter, int radius)
{
    return pixel.EuclideanDistance(circleCenter) <= radius;
}

// Create a SkiaSharp bitmap (raster surface)
var bitmap = new SKBitmap(width, height);

// Get the pixel buffer
for (int y = 0; y < height; y++)
{
    for (int x = 0; x < width; x++)
    {
        var color = SKColor.Parse("#000000"); // background color is black

        var pixel = new Vector2(x, y);
        if (IsInCircle(pixel, circleCenter, circleRadius))
        {
            color = SKColor.Parse("#00FF00"); // circle color is green
        }
        bitmap.SetPixel(x, y, color);
    }
}

// Save the image as PNG
using (var fs = new FileStream(filePath, FileMode.Create))
{
    bitmap.Encode(fs, SKEncodedImageFormat.Png, 100);
}

Console.WriteLine($"Image saved to {filePath}");