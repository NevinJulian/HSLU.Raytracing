namespace Common
{
    public class OptimizedRenderSettings
    {
        public int Width { get; set; } = 1920;
        public int Height { get; set; } = 1080;
        public int MaxReflectionDepth { get; set; } = 10;
        public int NumThreads { get; set; } = Environment.ProcessorCount;
        public string OutputFilename { get; set; } = "raytraced_image";
        public string OutputFormat { get; set; } = "png";
        public bool UseAcceleration { get; set; } = true;
        public bool ShowProgressBar { get; set; } = true;

        public bool PreviewMode { get; set; } = false;
        public float PreviewScale { get; set; } = 0.5f;

        public string GetOutputFile()
        {
            return $"{OutputFilename}.{OutputFormat}";
        }

        public static OptimizedRenderSettings CreateDefault()
        {
            return new OptimizedRenderSettings();
        }

        public static OptimizedRenderSettings CreatePreview()
        {
            return new OptimizedRenderSettings
            {
                PreviewMode = true,
                Width = 960,
                Height = 540,
                MaxReflectionDepth = 3,
                UseAcceleration = true,
                OutputFilename = "preview_render"
            };
        }

        public static OptimizedRenderSettings CreateQuickPreview()
        {
            return new OptimizedRenderSettings
            {
                PreviewMode = true,
                Width = 480,
                Height = 270,
                MaxReflectionDepth = 2,
                UseAcceleration = true,
                OutputFilename = "quick_preview"
            };
        }
    }
}