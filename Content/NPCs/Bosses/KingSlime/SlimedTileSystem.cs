using System.Collections.Generic;

namespace Reverie.Content.NPCs.Bosses.KingSlime;

public class SlimedTileSystem : ModSystem
{
    private static Dictionary<Point, float> slimedTiles = new Dictionary<Point, float>();
    private static Dictionary<Point, float> npcSlimedTiles = new Dictionary<Point, float>();
    private const float SLIME_DURATION = 3.5f * 60f;

    public override void PostUpdateWorld()
    {
        UpdateSlimedTiles(slimedTiles);
        UpdateSlimedTiles(npcSlimedTiles);
    }

    private void UpdateSlimedTiles(Dictionary<Point, float> tiles)
    {
        var tilesToRemove = new List<Point>();
        var tilesToUpdate = new List<Point>(tiles.Keys);

        foreach (Point tilePos in tilesToUpdate)
        {
            float timeLeft = tiles[tilePos] - 1f;

            if (timeLeft <= 0f)
            {
                tilesToRemove.Add(tilePos);
            }
            else
            {
                tiles[tilePos] = timeLeft;
            }
        }

        foreach (Point tilePos in tilesToRemove)
        {
            tiles.Remove(tilePos);
        }
    }

    public static void AddSlimedTile(int i, int j)
    {
        Point tilePos = new Point(i, j);
        slimedTiles[tilePos] = SLIME_DURATION;
        npcSlimedTiles[tilePos] = SLIME_DURATION;

    }

    public static bool IsSlimed(int i, int j)
    {
        return slimedTiles.ContainsKey(new Point(i, j));
    }

    public static bool IsNPCSlimed(int i, int j)
    {
        return npcSlimedTiles.ContainsKey(new Point(i, j));
    }

    public static float GetSlimeIntensity(int i, int j)
    {
        return GetIntensityFromDictionary(slimedTiles, i, j);
    }

    public static float GetNPCSlimeIntensity(int i, int j)
    {
        return GetIntensityFromDictionary(npcSlimedTiles, i, j);
    }

    private static float GetIntensityFromDictionary(Dictionary<Point, float> tiles, int i, int j)
    {
        Point tilePos = new Point(i, j);
        if (tiles.TryGetValue(tilePos, out float timeLeft))
        {
            float intensity = timeLeft / SLIME_DURATION;
            return MathHelper.Clamp(intensity, 0.2f, 1f);
        }
        return 0f;
    }

    public override void ClearWorld()
    {
        slimedTiles.Clear();
        npcSlimedTiles.Clear();
    }
}
