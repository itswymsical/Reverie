using System.Collections.Generic;

namespace Reverie.Content.NPCs.Bosses.KingSlime;

public class SlimedTileSystem : ModSystem
{
    private static Dictionary<Point, float> slimedTiles = new Dictionary<Point, float>();
    private const float SLIME_DURATION = 7 * 60f;

    public override void PostUpdateWorld()
    {
        // Update slimed tile timers
        var tilesToRemove = new List<Point>();
        var tilesToUpdate = new List<Point>(slimedTiles.Keys);

        foreach (Point tilePos in tilesToUpdate)
        {
            float timeLeft = slimedTiles[tilePos] - 1f;

            if (timeLeft <= 0f)
            {
                tilesToRemove.Add(tilePos);
            }
            else
            {
                slimedTiles[tilePos] = timeLeft;
            }
        }

        // Remove expired tiles
        foreach (Point tilePos in tilesToRemove)
        {
            slimedTiles.Remove(tilePos);
        }
    }

    public static void AddSlimedTile(int i, int j)
    {
        Point tilePos = new Point(i, j);
        slimedTiles[tilePos] = SLIME_DURATION;
    }

    public static bool IsSlimed(int i, int j)
    {
        return slimedTiles.ContainsKey(new Point(i, j));
    }

    public static float GetSlimeIntensity(int i, int j)
    {
        Point tilePos = new Point(i, j);
        if (slimedTiles.TryGetValue(tilePos, out float timeLeft))
        {
            // Fade out over time
            float intensity = timeLeft / SLIME_DURATION;
            return MathHelper.Clamp(intensity, 0.2f, 1f);
        }
        return 0f;
    }

    public override void ClearWorld()
    {
        slimedTiles.Clear();
    }
}
