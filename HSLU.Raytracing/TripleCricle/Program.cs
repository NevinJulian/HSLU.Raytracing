using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Common;

const int width = 800;
const int height = 600;
const string filePath = "rgb_circles.png";

var circles = new List<Circle>
{
    new Circle(new Vector2D(width / 2 - 100, height / 2 + 50), 150, MyColor.Red),
    new Circle(new Vector2D(width / 2 + 100, height / 2 + 50), 150, MyColor.Green),
    new Circle(new Vector2D(width / 2, height / 2 - 50), 150, MyColor.Blue)
};

using (var image = new Image<Rgba32>(width, height))
{
    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            var pixel = new Vector2D(x, y);
            byte r = 0, g = 0, b = 0;

            foreach (var circle in circles)
            {
                if (circle.IsInCircle(pixel))
                {
                    r = (byte)Math.Min(r + circle.Color.R, 255);
                    g = (byte)Math.Min(g + circle.Color.G, 255);
                    b = (byte)Math.Min(b + circle.Color.B, 255);
                }
            }

            image[x, y] = new Rgba32(r, g, b);
        }
    }

    image.Save(filePath);
}

Console.WriteLine($"Image saved to {filePath}");