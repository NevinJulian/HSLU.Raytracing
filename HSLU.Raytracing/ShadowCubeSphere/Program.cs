using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;

// Define light source class


const int width = 800;
const int height = 600;
const string filePath = "scene_with_shadows.png";

// Create a background plane (represented implicitly as Z=400)
float backgroundZ = 400;
Rgba32 backgroundColor = new Rgba32(255, 51, 255); // Pink

// Create a blue cube
var cubeCenter = new Vector3D(250f, 200f, 150f);
float cubeSize = 80f;
var cubeColor = new MyColor(20, 20, 150); // Dark blue
var cube = new Cube(cubeCenter, cubeSize, cubeColor, 45f); // 45-degree rotation

// Create a green sphere
var sphere = new Sphere(
    new Vector3D(500f, 350f, 180f), // Position 
    120,                          // Radius
    new MyColor(0, 100, 0)         // Dark green
);

// Create light sources
var lights = new List<LightSource>
{
    new LightSource(
        new Vector3D(200, 100, -200),  // Position (front-left)
        1.0f,                         // Intensity
        new MyColor(255, 255, 255)     // White light
    ),
    new LightSource(
        new Vector3D(600, 150, -150),   // Position (front-right)
        0.8f,                         // Intensity
        new MyColor(255, 255, 255)     // White light
    )
};

// Ambient light parameters
float ambientIntensity = 0.2f;
// Specular parameters
float specularPower = 20.0f;
float specularIntensity = 0.4f;

using (var image = new Image<Rgba32>(width, height))
{
    // Set background to pink
    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            image[x, y] = backgroundColor;
        }
    }

    // Render scene
    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            var pixel = new Vector2D(x, y);

            // Variables to track closest intersection
            float objectDepth = float.NegativeInfinity;
            Vector3D intersectionPoint = new Vector3D();
            Vector3D normal = new Vector3D();
            MyColor objectColor = null;
            bool hitObject = false;

            // Check cube intersection
            var (cubeHit, cubeDepth, cubeNormal) = cube.IntersectRay(pixel);
            if (cubeHit && cubeDepth > objectDepth)
            {
                objectDepth = cubeDepth;
                normal = cubeNormal;
                intersectionPoint = new Vector3D(x, y, cubeDepth);
                objectColor = cube.Color;
                hitObject = true;
            }

            // Check sphere intersection
            if (sphere.IsInSphere(pixel))
            {
                float dx = pixel.X - sphere.Center.X;
                float dy = pixel.Y - sphere.Center.Y;
                float dz = (float)Math.Sqrt(Math.Max(0, Math.Pow(sphere.Radius, 2) - (dx * dx + dy * dy)));
                float sphereDepth = sphere.Center.Z + dz;

                if (sphereDepth > objectDepth)
                {
                    objectDepth = sphereDepth;
                    // Calculate sphere normal
                    normal = new Vector3D(
                        dx / sphere.Radius,
                        dy / sphere.Radius,
                        dz / sphere.Radius
                    );
                    intersectionPoint = new Vector3D(x, y, sphereDepth);
                    objectColor = sphere.Color;
                    hitObject = true;
                }
            }

            // If no object hit, use background
            if (!hitObject)
            {
                // Check for shadows on background
                bool inShadow = false;
                float shadowIntensity = 0;

                foreach (var light in lights)
                {
                    // Background point
                    Vector3D bgPoint = new Vector3D(x, y, backgroundZ);

                    // Direction to light
                    Vector3D lightDir = new Vector3D(
                        light.Position.X - bgPoint.X,
                        light.Position.Y - bgPoint.Y,
                        light.Position.Z - bgPoint.Z
                    );

                    float lightDistance = (float)Math.Sqrt(
                        lightDir.X * lightDir.X +
                        lightDir.Y * lightDir.Y +
                        lightDir.Z * lightDir.Z
                    );

                    // Normalize
                    Vector3D normalizedLightDir = new Vector3D(
                        lightDir.X / lightDistance,
                        lightDir.Y / lightDistance,
                        lightDir.Z / lightDistance
                    );

                    // Shadow ray (from background to light)
                    Vector3D shadowRayOrigin = new Vector3D(x, y, backgroundZ - 0.1f);

                    // Check if cube blocks light
                    Vector2D shadowPixel = new Vector2D(shadowRayOrigin.X, shadowRayOrigin.Y);
                    var (shadowCubeHit, shadowCubeDepth, _) = cube.IntersectRay(shadowPixel);

                    if (shadowCubeHit && shadowCubeDepth > 0 && shadowCubeDepth < backgroundZ)
                    {
                        // Calculate shadow intensity based on distance
                        float distanceFactor = 1.0f - Math.Min(1.0f, (shadowCubeDepth / backgroundZ) * 0.5f);
                        shadowIntensity += light.Intensity * distanceFactor * 0.6f;
                        inShadow = true;
                    }

                    // Check if sphere blocks light
                    if (sphere.IsInSphere(shadowPixel))
                    {
                        float dx = shadowPixel.X - sphere.Center.X;
                        float dy = shadowPixel.Y - sphere.Center.Y;
                        float dz = (float)Math.Sqrt(Math.Max(0, Math.Pow(sphere.Radius, 2) - (dx * dx + dy * dy)));
                        float shadowSphereDepth = sphere.Center.Z + dz;

                        if (shadowSphereDepth > 0 && shadowSphereDepth < backgroundZ)
                        {
                            // Calculate shadow intensity based on distance
                            float distanceFactor = 1.0f - Math.Min(1.0f, (shadowSphereDepth / backgroundZ) * 0.5f);
                            shadowIntensity += light.Intensity * distanceFactor * 0.6f;
                            inShadow = true;
                        }
                    }
                }

                if (inShadow)
                {
                    // Apply shadow to background
                    shadowIntensity = Math.Min(0.85f, shadowIntensity); // Cap shadow darkness
                    image[x, y] = new Rgba32(
                        (byte)(backgroundColor.R * (1.0f - shadowIntensity)),
                        (byte)(backgroundColor.G * (1.0f - shadowIntensity)),
                        (byte)(backgroundColor.B * (1.0f - shadowIntensity))
                    );
                }

                continue; // Skip to next pixel
            }

            // Calculate lighting for objects
            float finalR = objectColor.R * ambientIntensity;
            float finalG = objectColor.G * ambientIntensity;
            float finalB = objectColor.B * ambientIntensity;

            // View direction (from camera to intersection point)
            Vector3D viewDir = new Vector3D(0, 0, -1); // Assuming camera at z = 0 looking in +z direction

            // For each light source
            foreach (var light in lights)
            {
                // Calculate direction and distance to light
                Vector3D lightDir = new Vector3D(
                    light.Position.X - intersectionPoint.X,
                    light.Position.Y - intersectionPoint.Y,
                    light.Position.Z - intersectionPoint.Z
                );

                float lightDistance = (float)Math.Sqrt(
                    lightDir.X * lightDir.X +
                    lightDir.Y * lightDir.Y +
                    lightDir.Z * lightDir.Z
                );

                // Normalize light direction
                Vector3D normalizedLightDir = new Vector3D(
                    lightDir.X / lightDistance,
                    lightDir.Y / lightDistance,
                    lightDir.Z / lightDistance
                );

                // Check if point is in shadow
                bool inShadow = false;

                // Shadow ray origin (slightly offset from intersection point)
                Vector3D shadowRayOrigin = new Vector3D(
                    intersectionPoint.X + normal.X * 0.1f,
                    intersectionPoint.Y + normal.Y * 0.1f,
                    intersectionPoint.Z + normal.Z * 0.1f
                );

                Vector2D shadowPixel = new Vector2D(shadowRayOrigin.X, shadowRayOrigin.Y);

                // Check if cube casts shadow (if we're not on the cube)
                if (objectColor != cube.Color)
                {
                    var (shadowCubeHit, shadowCubeDepth, _) = cube.IntersectRay(shadowPixel);

                    if (shadowCubeHit && shadowCubeDepth > 0 && shadowCubeDepth < lightDistance)
                    {
                        inShadow = true;
                    }
                }

                // Check if sphere casts shadow (if we're not on the sphere)
                if (objectColor != sphere.Color && !inShadow)
                {
                    if (sphere.IsInSphere(shadowPixel))
                    {
                        float dx = shadowPixel.X - sphere.Center.X;
                        float dy = shadowPixel.Y - sphere.Center.Y;
                        float dz = (float)Math.Sqrt(Math.Max(0, Math.Pow(sphere.Radius, 2) - (dx * dx + dy * dy)));
                        float shadowSphereDepth = sphere.Center.Z + dz;

                        if (shadowSphereDepth > 0 && shadowSphereDepth < lightDistance)
                        {
                            inShadow = true;
                        }
                    }
                }

                if (!inShadow)
                {
                    // Calculate diffuse lighting
                    float diffuse = Math.Max(0, normal.Dot(normalizedLightDir));

                    // Calculate specular reflection
                    // Reflect light around normal
                    float dotNL = normal.Dot(normalizedLightDir);
                    Vector3D reflectDir = new Vector3D(
                        normalizedLightDir.X - 2 * dotNL * normal.X,
                        normalizedLightDir.Y - 2 * dotNL * normal.Y,
                        normalizedLightDir.Z - 2 * dotNL * normal.Z
                    );

                    // Calculate specular factor
                    float specularFactor = Math.Max(0, reflectDir.Dot(viewDir));
                    specularFactor = (float)Math.Pow(specularFactor, specularPower) * specularIntensity;

                    // Calculate attenuation
                    float attenuation = 1.0f / (1.0f + 0.0005f * lightDistance + 0.00005f * lightDistance * lightDistance);

                    // Apply light contribution with intensity
                    float lightContrib = diffuse * light.Intensity * attenuation;
                    finalR += objectColor.R * lightContrib;
                    finalG += objectColor.G * lightContrib;
                    finalB += objectColor.B * lightContrib;

                    // Add specular highlight (white)
                    float specular = specularFactor * light.Intensity * attenuation;
                    finalR += 255 * specular;
                    finalG += 255 * specular;
                    finalB += 255 * specular;
                }
            }

            // Clamp final color values
            image[x, y] = new Rgba32(
                (byte)Math.Min(255, Math.Max(0, finalR)),
                (byte)Math.Min(255, Math.Max(0, finalG)),
                (byte)Math.Min(255, Math.Max(0, finalB))
            );
        }
    }

    image.Save(filePath);
}

Console.WriteLine($"Image saved to {filePath}");