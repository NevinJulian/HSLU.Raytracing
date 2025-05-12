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

        // Preview settings - lower quality for faster rendering
        public bool PreviewMode { get; set; } = false;
        public float PreviewScale { get; set; } = 0.5f; // Half resolution in preview

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
                Width = 960,      // Half resolution
                Height = 540,     // Half resolution
                MaxReflectionDepth = 3, // Fewer reflections
                UseAcceleration = true,  // Always use acceleration for preview
                OutputFilename = "preview_render"
            };
        }

        // Create a low-quality preview for quick testing
        public static OptimizedRenderSettings CreateQuickPreview()
        {
            return new OptimizedRenderSettings
            {
                PreviewMode = true,
                Width = 480,      // Quarter resolution
                Height = 270,     // Quarter resolution
                MaxReflectionDepth = 2, // Minimal reflections
                UseAcceleration = true,
                OutputFilename = "quick_preview"
            };
        }
    }
}