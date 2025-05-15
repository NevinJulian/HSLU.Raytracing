using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Common
{
    public class RayTracer
    {
        public void RenderScene(Scene scene, Camera camera, RenderSettings settings)
        {
            scene.SetMaxReflectionDepth(settings.MaxReflectionDepth);
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
                    }
                });

                tasks.Add(task);
            }

            Task.WhenAll(tasks).Wait();
            image.Save(settings.GetOutputFile());
        }

        public static Scene CreateDemoScene()
        {
            var scene = new Scene();

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