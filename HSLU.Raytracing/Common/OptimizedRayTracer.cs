using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Common
{
    public class OptimizedRayTracer
    {
        public void RenderScene(OptimizedScene scene, Camera camera, OptimizedRenderSettings settings)
        {
            Console.WriteLine("Preparing to render scene...");

            // Build the BVH acceleration structure if it's enabled
            if (settings.UseAcceleration)
            {
                Console.WriteLine("Building acceleration structure...");
                var buildTime = System.Diagnostics.Stopwatch.StartNew();
                scene.BuildAccelerationStructure();
                buildTime.Stop();
                Console.WriteLine($"Acceleration structure built in {buildTime.ElapsedMilliseconds}ms");
            }

            // Set the max reflection depth in the scene
            scene.SetMaxReflectionDepth(settings.MaxReflectionDepth);

            // Create render timer
            var timer = new RenderTimer(settings.Height);

            // Create the image
            using var image = new Image<Rgba32>(settings.Width, settings.Height);

            // Create an array of all scan lines
            List<int> scanLines = new(settings.Height);
            for (int y = 0; y < settings.Height; y++)
            {
                scanLines.Add(y);
            }

            // Shuffle the scan lines for better load balancing
            Random random = new Random();
            scanLines = scanLines.OrderBy(x => random.Next()).ToList();

            // Divide the scan lines among threads
            int linesPerThread = (int)Math.Ceiling((double)settings.Height / settings.NumThreads);
            List<List<int>> threadTasks = new(settings.NumThreads);

            for (int i = 0; i < settings.NumThreads; i++)
            {
                int startIdx = i * linesPerThread;
                int endIdx = Math.Min(startIdx + linesPerThread, settings.Height);

                if (startIdx < settings.Height)
                {
                    threadTasks.Add(scanLines.GetRange(startIdx, endIdx - startIdx));
                }
            }

            // Use CountDownLatch pattern with Task to wait for all threads
            var tasks = new List<Task>();
            object imageLock = new object(); // For synchronizing access to the shared image

            // Calculate aspect ratio
            float aspectRatio = (float)settings.Width / settings.Height;

            Console.WriteLine($"Rendering image with resolution {settings.Width}x{settings.Height}");
            Console.WriteLine($"Using {threadTasks.Count} threads with {linesPerThread} lines per thread");
            Console.WriteLine($"Max reflection depth: {settings.MaxReflectionDepth}");
            Console.WriteLine($"Acceleration: {(settings.UseAcceleration ? "Enabled" : "Disabled")}");

            // Start timing
            timer.Start();

            // Create and start tasks
            foreach (var taskLines in threadTasks)
            {
                var task = Task.Run(() =>
                {
                    foreach (int y in taskLines)
                    {
                        for (int x = 0; x < settings.Width; x++)
                        {
                            // Convert pixel coordinates to normalized device coordinates
                            float nx = ((x - settings.Width / 2.0f) / (settings.Width / 2.0f)) * aspectRatio;
                            float ny = -((y - settings.Height / 2.0f) / (settings.Height / 2.0f));

                            // Create a ray from the camera
                            Ray ray = camera.CreateRay(nx, ny);

                            // Trace the ray through the scene
                            MyColor pixelColor = scene.Trace(ray);

                            // Synchronize access to the shared image
                            lock (imageLock)
                            {
                                image[x, y] = new Rgba32(
                                    (byte)pixelColor.R,
                                    (byte)pixelColor.G,
                                    (byte)pixelColor.B
                                );
                            }
                        }

                        // Update progress after each scan line
                        timer.IncrementProgress();
                    }
                });

                tasks.Add(task);
            }

            // Wait for all tasks to complete
            Task.WhenAll(tasks).Wait();

            // Stop timing
            timer.Stop();

            // Save the image
            string outputFile = settings.GetOutputFile();
            image.Save(outputFile);
            Console.WriteLine($"Image saved to {outputFile}");
        }
    }
}