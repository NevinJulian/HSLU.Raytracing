using System;
using System.IO;
using SkiaSharp;

class Program
{
    static void Main()
    {
        int width = 300;
        int height = 300;
        Random random = new Random();

        // Create a SkiaSharp bitmap (raster surface)
        using (SKBitmap bitmap = new SKBitmap(width, height))
        {
            // Loop through each pixel
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Generate a random color
                    SKColor color = new SKColor(
                        (byte)random.Next(256), // Random Red
                        (byte)random.Next(256), // Random Green
                        (byte)random.Next(256), // Random Blue
                        255                      // Full opacity
                    );

                    // Set the pixel color
                    bitmap.SetPixel(x, y, color);
                }
            }

            // Save the image as PNG
            string filePath = "skia_random_colors.png";
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                bitmap.Encode(fs, SKEncodedImageFormat.Png, 100);
            }

            Console.WriteLine($"Random color image saved to {filePath}");
        }
    }
}
