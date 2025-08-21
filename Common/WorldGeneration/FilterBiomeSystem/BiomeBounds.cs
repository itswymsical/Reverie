namespace Reverie.Common.WorldGeneration.FilterBiomeSystem;

public struct BiomeBounds
{
    public int Left;
    public int Right;
    public int Top;
    public int Bottom;
    public bool IsValid => Right > Left && Left > 0 && Right < Main.maxTilesX;
    public int Width => Right - Left;
    public int Height => Bottom - Top;
    public Rectangle ToRectangle() => new Rectangle(Left, Top, Width, Height);
}
