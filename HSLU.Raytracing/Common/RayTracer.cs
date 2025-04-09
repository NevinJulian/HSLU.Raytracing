using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Common
{
    public class RayTracer
    {
        public void RenderScene(Scene scene, Camera camera, RenderSettings settings)
        {
            // Set the max reflection depth in the scene
            scene.SetMaxReflectionDepth(settings.MaxReflectionDepth);

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
                    }
                });

                tasks.Add(task);
            }

            // Wait for all tasks to complete
            Task.WhenAll(tasks).Wait();

            // Save the image
            image.Save(settings.GetOutputFile());
        }

        public static Scene CreateDemoScene()
        {
            var scene = new Scene();

            // Add spheres
            scene.AddObject(new Sphere(
                new Vector3D(-1.0f, 0.7f, 2f),
                1.0f,
                Common.Material.Create(MaterialType.RUBY, 0.4f)
            ));

            scene.AddObject(new Sphere(
                new Vector3D(1.0f, 0.1f, 1f),
                0.7f,
                Common.Material.Create(MaterialType.GOLD, 0.6f)
            ));

            scene.AddObject(new Sphere(
                new Vector3D(0f, -1001f, 0f),
                1000f,
                Common.Material.Create(MaterialType.JADE, 0.8f)
            ));

            scene.AddObject(new Sphere(
                new Vector3D(0f, 500f, 1020f),
                1000f,
                Common.Material.Create(MaterialType.JADE, 0f)
            ));

            // Add cubes
            scene.AddObject(new RotatedCube(
                new Vector3D(1.5f, 0.3f, 0.5f),
                1.0f,
                Common.Material.Create(MaterialType.PEARL, 0.3f),
                30f, 45f, 15f
            ));

            scene.AddObject(new RotatedCube(
                new Vector3D(-1.5f, 0.0f, 0.7f),
                0.8f,
                Common.Material.Create(MaterialType.SILVER, 0.7f),
                15f, -30f, 5f
            ));

            // Add lights
            scene.AddLight(new Light(
                new Vector3D(-5f, 5f, -5f),
                new MyColor(255, 255, 255),
                1.0f
            ));

            scene.AddLight(new Light(
                new Vector3D(3f, 3f, -3f),
                new MyColor(200, 200, 255),
                0.8f
            ));

            return scene;
        }
    }
}