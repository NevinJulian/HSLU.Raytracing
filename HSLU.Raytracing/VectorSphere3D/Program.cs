using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

const int width = 800;
const int height = 500;
const int sphereRadius = 100;
const string filePath = "sphere_3d_bright.png";

Vector3 sphereCenter = new Vector3(width / 2, height / 2, 0);
Vector3 lightDir = Vector3.Normalize(new Vector3(-1, -1, -1));

const float ambient = 0.3f;

var bitmap = new Image<Rgba32>(width, height);

for (int y = 0; y < height; y++)
{
    for (int x = 0; x < width; x++)
    {
        float dx = x - sphereCenter.X;
        float dy = y - sphereCenter.Y;

        float underRoot = sphereRadius * sphereRadius - dx * dx - dy * dy;

        if (underRoot >= 0)
        {
            float z = MathF.Sqrt(underRoot);

            Vector3 normal = Vector3.Normalize(new Vector3(dx, dy, z));
            float brightness = MathF.Max(Vector3.Dot(normal, lightDir), 0);

            // Apply ambient light
            brightness = ambient + (1 - ambient) * brightness;

            // Convert brightness to grayscale color
            byte intensity = (byte)(brightness * 255);
            bitmap[x, y] = new Rgba32(intensity, intensity, intensity);
        }
        else
        {
            bitmap[x, y] = Color.Black;
        }
    }
}

// Save image
bitmap.SaveAsPng(filePath);
Console.WriteLine($"3D Sphere image saved to {filePath}");
