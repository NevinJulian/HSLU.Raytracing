using SkiaSharp;

class Program
{
    static bool IsInCircle(int x, int y, int center_x, int center_y, int radius)
    {
        return Math.Pow(x - center_x, 2) + Math.Pow(y - center_y, 2) <= Math.Pow(radius, 2);
    }

    static void Main()
    {
        const int width = 800;
        const int height = 500;
        const int circle_center_x = width / 2;
        const int circle_center_y = height / 2;
        const int circle_radius = 100;
        const string filePath = "skia_raster_image.png";

        // Create a SkiaSharp bitmap (raster surface)
        var bitmap = new SKBitmap(width, height);

        // Get the pixel buffer
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var color = SKColor.Parse("#000000"); // background color is black

                if (IsInCircle(x, y, circle_center_x, circle_center_y, circle_radius))
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
    }
}
