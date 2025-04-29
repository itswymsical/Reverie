namespace Reverie.Utilities.Extensions;

public static class DustExtensions
{
    public static void SpawnDustCloud(Vector2 position, int width, int height, int type, int amount = 10, float speedX = 0, float speedY = 0, int alpha = 0, Color c = default, float scale = 1f)
    {
        for (int i = 0; i < amount; ++i)
        {
            Dust.NewDust(position, width, height, type, speedX, speedY, alpha, c, scale);
        }
    }
}
