using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Common
{
    public class OptimizedRayTracer
    {
        public void RenderScene(OptimizedScene scene, Camera camera, OptimizedRenderSettings settings)
        {
            Console.WriteLine("Preparing to render scene...");

            if (settings.UseAcceleration)
            {
                Console.WriteLine("Building acceleration structure...");
                var buildTime = System.Diagnostics.Stopwatch.StartNew();
                scene.BuildAccelerationStructure();
                buildTime.Stop();
                Console.WriteLine($"Acceleration structure built in {buildTime.ElapsedMilliseconds}ms");
            }

            scene.SetMaxReflectionDepth(settings.MaxReflectionDepth);
            var timer = new RenderTimer(settings.Height);
            using var image = new Image<Rgba32>(settings.Width, settings.Height);

            List<int> scanLines = new(settings.Height);
            for (int y = 0; y < settings.Height; y++)
            {
                scanLines.Add(y);
            }

            Random random = new Random();
            scanLines = scanLines.OrderBy(x => random.Next()).ToList();

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

            var tasks = new List<Task>();
            object imageLock = new object();

            float aspectRatio = (float)settings.Width / settings.Height;

            Console.WriteLine($"Rendering image with resolution {settings.Width}x{settings.Height}");
            Console.WriteLine($"Using {threadTasks.Count} threads with {linesPerThread} lines per thread");
            Console.WriteLine($"Max reflection depth: {settings.MaxReflectionDepth}");
            Console.WriteLine($"Acceleration: {(settings.UseAcceleration ? "Enabled" : "Disabled")}");

            timer.Start();

            foreach (var taskLines in threadTasks)
            {
                var task = Task.Run(() =>
                {
                    foreach (int y in taskLines)
                    {
                        for (int x = 0; x < settings.Width; x++)
                        {
                            float nx = ((x - settings.Width / 2.0f) / (settings.Width / 2.0f)) * aspectRatio;
                            float ny = -((y - settings.Height / 2.0f) / (settings.Height / 2.0f));

                            Ray ray = camera.CreateRay(nx, ny);
                            MyColor pixelColor = scene.Trace(ray);

                            lock (imageLock)
                            {
                                image[x, y] = new Rgba32(
                                    (byte)pixelColor.R,
                                    (byte)pixelColor.G,
                                    (byte)pixelColor.B
                                );
                            }
                        }

                        timer.IncrementProgress();
                    }
                });

                tasks.Add(task);
            }

            Task.WhenAll(tasks).Wait();

            timer.Stop();

            string outputFile = settings.GetOutputFile();
            image.Save(outputFile);
            Console.WriteLine($"Image saved to {outputFile}");
        }
    }
}