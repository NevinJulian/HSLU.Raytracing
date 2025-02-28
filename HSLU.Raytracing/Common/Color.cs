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
}

