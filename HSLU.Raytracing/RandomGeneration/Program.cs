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

        using (SKBitmap bitmap = new SKBitmap(width, height))
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    SKColor color = new SKColor(
                        (byte)random.Next(256), // Random Red
                        (byte)random.Next(256), // Random Green
                        (byte)random.Next(256), // Random Blue
                        255                      // Full opacity
                    );

                    bitmap.SetPixel(x, y, color);
                }
            }

            string filePath = "skia_random_colors.png";
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                bitmap.Encode(fs, SKEncodedImageFormat.Png, 100);
            }

            Console.WriteLine($"Random color image saved to {filePath}");
        }
    }
}
