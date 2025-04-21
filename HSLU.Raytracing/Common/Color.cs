namespace Common;

public class MyColor
{
    public static MyColor Black => new(0, 0, 0);
    public static MyColor Green => new(0, 255, 0);
    public static MyColor Red => new(255, 0, 0);
    public static MyColor White => new(255, 255, 255);

    public static MyColor Cyan => new(0, 255, 255);

    public static MyColor Blue => new(0, 0, 255);

    public int R { get; }

    public int G { get; }

    public int B { get; }

    public MyColor(int r, int g, int b)
    {
        R = r;
        G = g;
        B = b;
    }

    public static MyColor operator *(MyColor color, float scalar)
    {
        return new MyColor(
            (int)Math.Clamp(color.R * scalar, 0, 255),
            (int)Math.Clamp(color.G * scalar, 0, 255),
            (int)Math.Clamp(color.B * scalar, 0, 255)
        );
    }

    public static MyColor operator *(float scalar, MyColor color)
    {
        return color * scalar;
    }

    public static MyColor operator +(MyColor a, MyColor b)
    {
        return new MyColor(
            Math.Clamp(a.R + b.R, 0, 255),
            Math.Clamp(a.G + b.G, 0, 255),
            Math.Clamp(a.B + b.B, 0, 255)
        );
    }

    public static MyColor operator *(MyColor a, MyColor b)
    {
        return new MyColor(
            (int)Math.Clamp((a.R * b.R) / 255f, 0, 255),
            (int)Math.Clamp((a.G * b.G) / 255f, 0, 255),
            (int)Math.Clamp((a.B * b.B) / 255f, 0, 255)
        );
    }

    public MyColor Blend(MyColor other, float factor)
    {
        return new MyColor(
            (int)(this.R * factor + other.R * (1 - factor)),
            (int)(this.G * factor + other.G * (1 - factor)),
            (int)(this.B * factor + other.B * (1 - factor))
        );
    }
}

