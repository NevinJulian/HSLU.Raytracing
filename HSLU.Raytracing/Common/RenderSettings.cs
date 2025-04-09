namespace Common
{
    public class RenderSettings
    {
        public int Width { get; set; } = 1920;
        public int Height { get; set; } = 1080;
        public int MaxReflectionDepth { get; set; } = 10;
        public int NumThreads { get; set; } = Environment.ProcessorCount;
        public string OutputFilename { get; set; } = "raytraced_image";
        public string OutputFormat { get; set; } = "png";

        public string GetOutputFile()
        {
            return $"{OutputFilename}.{OutputFormat}";
        }

        public static RenderSettings CreateDefault()
        {
            return new RenderSettings();
        }
    }
}