using SkiaSharp;
using Common;

const int width = 800;
const int height = 500;
const int circleCenterX = width / 2;
const int circleCenterY = height / 2;
const int circleRadius = 100;
const string filePath = "skia_raster_image.png";

var circleCenter = new Vector2D(circleCenterX, circleCenterY);

static bool IsInCircle(Vector2D pixel, Vector2D circleCenter, int radius)
{
    return pixel.EuclideanDistance(circleCenter) <= radius;
}

var bitmap = new SKBitmap(width, height);

for (int y = 0; y < height; y++)
{
    for (int x = 0; x < width; x++)
    {
        var color = SKColor.Parse("#000000");

        var pixel = new Vector2D(x, y);
        if (IsInCircle(pixel, circleCenter, circleRadius))
        {
            color = SKColor.Parse("#00FF00");
        }
        bitmap.SetPixel(x, y, color);
    }
}

using (var fs = new FileStream(filePath, FileMode.Create))
{
    bitmap.Encode(fs, SKEncodedImageFormat.Png, 100);
}

Console.WriteLine($"Image saved to {filePath}");