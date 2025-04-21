using Common;
using System;

namespace SpheresRender
{
    class Program
    {
        static void Main(string[] args)
        {
            const int width = 1920;
            const int height = 1080;
            const string filePath = "soap_bubbles_ultrabright.png";

            // Create scene
            var scene = new Scene();

            // Move camera back slightly
            var camera = new Camera(new Vector3D(0, 0, -5.5));

            // Create a pure black background material
            var blackBackgroundMaterial = new Material(
                MaterialType.BLACK_PLASTIC,
                new MyColor(0, 0, 0),        // Black ambient
                new MyColor(0, 0, 0),        // Black diffuse
                new MyColor(0, 0, 0),        // No specular
                0.0f,                        // No shininess
                0.0f,                        // No reflectivity
                0.0f                         // No transparency
            );

            // ULTRA-bright soap bubble material
            var soapBubbleMaterialBase = new Material(
                MaterialType.PEARL,
                new MyColor(50, 50, 50),      // Very high ambient (doubled)
                new MyColor(255, 255, 255),   // Full white diffuse
                new MyColor(255, 255, 255),   // Bright specular
                1.0f,                         // Maximum shininess
                0.99f,                        // Nearly perfect reflectivity
                0.5f                          // Medium transparency for better brightness
            );

            // Create only 12 LARGE soap bubbles with MAXIMUM vibrancy
            Random random = new Random(42); // Fixed seed for reproducibility

            // Create just 12 large feature bubbles - positioned very specifically
            int numBubbles = 12;
            for (int i = 0; i < numBubbles; i++)
            {
                // Calculate angle for even distribution around the center
                float angle = (float)(i * 2 * Math.PI / numBubbles);

                // Calculate position based on angle (spiral pattern)
                float distanceFromCenter = 2.0f + (i % 4) * 1.5f; // Varying distances
                float x = (float)(distanceFromCenter * Math.Cos(angle));
                float y = (float)(distanceFromCenter * Math.Sin(angle));

                // Vary z positions to create depth
                float z = (float)(random.NextDouble() * 5.0 - 2.5); // -2.5 to 2.5

                // Large bubbles with varying sizes
                float radius = (float)(random.NextDouble() * 1.5 + 1.0); // 1.0 to 2.5

                // Random "age" parameter - controls pattern
                float age = (float)(random.NextDouble());

                // Create the soap bubble with MAXIMUM iridescence
                scene.AddObject(new SoapBubble(
                    new Vector3D(x, y, z),
                    radius,
                    soapBubbleMaterialBase,
                    age,
                    1.0f,  // MAXIMUM iridescence intensity
                    1.0f   // MAXIMUM film thickness variation
                ));
            }

            // MASSIVELY boosted lighting setup

            // Primary front light - ULTRA bright
            scene.AddLight(new Light(
                new Vector3D(0, 0, -6),
                new MyColor(255, 255, 255),  // White light
                10.0f                        // ULTRA intensity - 10x normal
            ));

            // Secondary front lights - ULTRA bright
            scene.AddLight(new Light(
                new Vector3D(-5, 3, -6),
                new MyColor(255, 240, 220),  // Warm white light
                8.0f                         // ULTRA intensity
            ));

            scene.AddLight(new Light(
                new Vector3D(5, 3, -6),
                new MyColor(220, 240, 255),  // Cool white light
                8.0f                         // ULTRA intensity
            ));

            // Colored lights for iridescence - positioned for maximum effect

            // Magenta light - ULTRA bright
            scene.AddLight(new Light(
                new Vector3D(-5, -2, -3),
                new MyColor(255, 50, 200),   // Vibrant magenta
                7.0f                         // ULTRA intensity
            ));

            // Cyan light - ULTRA bright
            scene.AddLight(new Light(
                new Vector3D(5, -2, -3),
                new MyColor(50, 220, 255),   // Vibrant cyan
                7.0f                         // ULTRA intensity
            ));

            // Yellow light - ULTRA bright
            scene.AddLight(new Light(
                new Vector3D(0, 6, -3),
                new MyColor(255, 220, 50),   // Vibrant yellow
                7.0f                         // ULTRA intensity
            ));

            // Additional colored lights for more vibrant patterns

            // Orange light - ULTRA bright
            scene.AddLight(new Light(
                new Vector3D(0, -6, -3),
                new MyColor(255, 150, 50),   // Vibrant orange
                6.0f                         // ULTRA intensity
            ));

            // Purple light - ULTRA bright
            scene.AddLight(new Light(
                new Vector3D(-6, 0, -3),
                new MyColor(180, 50, 255),   // Vibrant purple
                6.0f                         // ULTRA intensity
            ));

            // Green light - ULTRA bright
            scene.AddLight(new Light(
                new Vector3D(6, 0, -3),
                new MyColor(50, 255, 100),   // Vibrant green
                6.0f                         // ULTRA intensity
            ));

            // Add a few lights behind the bubbles to create rim highlights

            // Rim light 1 - ULTRA bright
            scene.AddLight(new Light(
                new Vector3D(0, 0, 6),
                new MyColor(255, 255, 200),  // Warm white
                5.0f                         // Very high intensity
            ));

            // Rim light 2 - ULTRA bright
            scene.AddLight(new Light(
                new Vector3D(-3, 3, 6),
                new MyColor(200, 255, 255),  // Cool white
                5.0f                         // Very high intensity
            ));

            // Rim light 3 - ULTRA bright
            scene.AddLight(new Light(
                new Vector3D(3, -3, 6),
                new MyColor(255, 200, 255),  // Pink-white
                5.0f                         // Very high intensity
            ));

            // Create render settings
            var settings = new RenderSettings
            {
                Width = width,
                Height = height,
                MaxReflectionDepth = 12,      // Increased for better reflections
                OutputFilename = filePath.Replace(".png", ""),
                OutputFormat = "png",
                NumThreads = Environment.ProcessorCount
            };

            // Create ray tracer and render the scene
            var rayTracer = new RayTracer();
            Console.WriteLine("Starting rendering...");
            rayTracer.RenderScene(scene, camera, settings);
            Console.WriteLine($"Image saved to {filePath}");
        }
    }
}