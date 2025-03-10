﻿using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Common;

const int width = 800;
const int height = 600;
const string filePath = "spheres.png";

var lightSource = new Vector3D(width / 2, height / 2, 1000);

var spheres = new List<Sphere>
{
    new Sphere(new Vector3D(width / 2, height / 2, 50), 200, MyColor.Blue),
    new Sphere(new Vector3D(width / 2 - 70, height / 2 - 70, 110), 130, new MyColor(0, 255, 255))
};

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
                        depthFactor = Math.Clamp(depthFactor, 0.7, 1.0);

                        Vector3D normal = (rayOrigin + rayDirection * pixelDepth - sphere.Center).Normalize();
                        double shading = 0.5 + 0.5 * normal.Z;
                        double brightness = depthFactor * shading;

                        if (spheres[1] == sphere && spheres[0].IsInSphere(pixel) && pixelDepth <= spheres[0].Center.Z)
                        {
                            continue;
                        }

                        byte r = (byte)Math.Min(sphere.Color.R * brightness, 255);
                        byte g = (byte)Math.Min(sphere.Color.G * brightness, 255);
                        byte b2 = (byte)Math.Min(sphere.Color.B * brightness, 255);

                        finalColor = new Rgba32(r, g, b2);
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