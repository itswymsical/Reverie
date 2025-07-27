/// <summary>
/// Credits to Spirit Reforged for the original implementation: 
/// https://github.com/GabeHasWon/SpiritReforged/blob/master/Common/TileCommon/TileSway/TileSwaySystem.cs#L6
/// </summary>
/// 
using System.Collections.Generic;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ObjectData;
using static Terraria.GameContent.Drawing.TileDrawing;

namespace Reverie.Common.Systems;

internal class TileSwaySystem : ModSystem
{
    public static TileSwaySystem Instance;

    public static event Action PreUpdateWind;
    private static readonly Dictionary<int, int> TileSwayTypes = [];

    public double TreeWindCounter { get; private set; }
    public double GrassWindCounter { get; private set; }
    public double SunflowerWindCounter { get; private set; }

    /// <summary> Checks whether this tile type is an <see cref="ISwayTile"/> and outputs the counter corresponding to <see cref="ISwayTile.Style"/>. </summary>
    /// <param name="type"> The tile type. </param>
    /// <param name="counter"> The counter type. </param>
    /// <returns> Whether this tile sways (implements ISwayTile) </returns>
    public static bool DoesSway(int type, out TileCounterType counter)
    {
        if (TileSwayTypes.TryGetValue(type, out var value))
        {
            counter = (TileCounterType)value;
            return true;
        }

        counter = (TileCounterType)(-1);
        return false;
    }

    public override void Load()
    {
        Instance = this;
        On_TileDrawing.Update += UpdateClients;
    }

    private void UpdateClients(On_TileDrawing.orig_Update orig, TileDrawing self)
    {
        orig(self);

        if (Main.dedServ)
            return;

        PreUpdateWind?.Invoke();

        double num = Math.Abs(Main.WindForVisuals);
        num = Utils.GetLerpValue(0.08f, 1.2f, (float)num, clamped: true);

        TreeWindCounter += 0.0041666666666666666 + 0.0041666666666666666 * num * 2.0;
        GrassWindCounter += 0.0055555555555555558 + 0.0055555555555555558 * num * 4.0;
        SunflowerWindCounter += 0.002380952380952 + 0.0023809523809523810 * num * 5.0;
    }

    public override void PostSetupContent()
    {
        var modTiles = ModContent.GetContent<ModTile>();

        foreach (var tile in modTiles)
        {
            if (tile is ISwayTile sway)
            {
                TileSwayTypes.Add(tile.Type, sway.Style);
                var counter = (TileCounterType)sway.Style;

                if (counter is TileCounterType.MultiTileVine or TileCounterType.MultiTileGrass) //Assign required sets
                    TileID.Sets.MultiTileSway[tile.Type] = true;
                else if (counter == TileCounterType.Vine)
                    TileID.Sets.VineThreads[tile.Type] = true;
                else if (counter == TileCounterType.ReverseVine)
                    TileID.Sets.ReverseVineThreads[tile.Type] = true;
            }
        }
    }
}


/// <summary> Assign <see cref="Style"/> for vanilla sway styles or use <see cref="DrawSway"/> for custom drawing.<br/>
/// <see cref="Physics"/> changes how the tile responds to wind and player interaction. </summary>
public interface ISwayTile
{
    #region inst rotation
    /// <summary> Coordinate specific rotation values tied to <see cref="Physics"/> by default. </summary>
    [WorldBound]
    private static readonly Dictionary<Point16, float> Rotation = [];

    public static bool SetInstancedRotation(int i, int j, float value, bool fail = true)
    {
        if (Main.dedServ)
            return false;

        TileExtensions.GetTopLeft(ref i, ref j);
        var pt = new Point16(i, j);

        if (Rotation.ContainsKey(pt))
        {
            if (!fail)
                Rotation.Remove(pt);
            else
                Rotation[pt] = value;

            return true;
        }

        return Rotation.TryAdd(pt, value);
    }

    public static float GetInstancedRotation(int i, int j)
    {
        if (Main.dedServ)
            return 0;

        TileExtensions.GetTopLeft(ref i, ref j);
        Rotation.TryGetValue(new Point16(i, j), out float value);

        return value;
    }
    #endregion

    /// <summary> The default sway physics style for this tile according to <see cref="TileDrawing.TileCounterType"/>. Defaults to -1 which enables custom drawing only. </summary>
    public int Style => -1;

    /// <summary> Add wind grid math here. Called once per multitile. </summary>
    public float Physics(Point16 topLeft)
    {
        if (Rotation.TryGetValue(topLeft, out float value))
        {
            Rotation[topLeft] = MathHelper.Lerp(value, 0, .2f);

            if ((int)(value * 100) == 0)
                Rotation.Remove(topLeft);

            return value;
        }

        return 0;
    }

    /// <summary> Draw this tile transformed by <see cref="Physics"/>. </summary>
    public void DrawSway(int i, int j, SpriteBatch spriteBatch, Vector2 offset, float rotation, Vector2 origin)
    {
        var tile = Framing.GetTileSafely(i, j);
        var data = TileObjectData.GetTileData(tile);

        var drawPos = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y);
        var source = new Rectangle(tile.TileFrameX, tile.TileFrameY, data.CoordinateWidth, data.CoordinateHeights[tile.TileFrameY / 18]);
        var dataOffset = new Vector2(data.DrawXOffset, data.DrawYOffset);

        spriteBatch.Draw(TextureAssets.Tile[tile.TileType].Value, drawPos + offset + dataOffset,
            source, Lighting.GetColor(i, j), rotation, origin, 1, SpriteEffects.None, 0);
    }
}

public static class TileExtensions
{
    public static void GetTopLeft(ref int i, ref int j)
    {
        var tile = Framing.GetTileSafely(i, j);
        var data = TileObjectData.GetTileData(tile);

        if (data is null)
            return;

        (i, j) = (i - tile.TileFrameX % data.CoordinateFullWidth / 18, j - tile.TileFrameY % data.CoordinateFullHeight / 18);
    }



    /// <summary>
    /// Mutually merges the given tile with all of the ids in <paramref name="otherIds"/>.
    /// </summary>
    /// <param name="tile">The tile to merge with.</param>
    /// <param name="otherIds">All other tiles to merge with.</param>
    public static void Merge(this ModTile tile, params int[] otherIds)
    {
        foreach (int id in otherIds)
        {
            Main.tileMerge[tile.Type][id] = true;
            Main.tileMerge[id][tile.Type] = true;
        }
    }
}

/// <summary>Resets this static field to its original value when <see cref="WorldGen.clearWorld()"/> is called.<br/>
/// For <see cref="IEnumerable"/>s, this will call any Clear method (such as <see cref="HashSet{T}.Clear"/>) instead of nulling the value.</summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
internal class WorldBoundAttribute : Attribute
{
    /// <summary> When true, prevents alternative reset behaviour from taking place. </summary>
    public bool Manual;
}

internal class WorldBoundSystem : ModSystem
{
    private readonly record struct FieldData(object Obj, MethodInfo Alt = null)
    {
        public readonly object Default = Obj;
        public readonly MethodInfo Alternative = Alt;
    }
    private static readonly Dictionary<FieldInfo, FieldData> Defaults = [];

    public override void Load()
    {
        foreach (var type in Mod.Code.GetTypes())
        {
            foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (field.GetCustomAttribute<WorldBoundAttribute>() is WorldBoundAttribute attr)
                    Defaults.Add(field, new(field.GetValue(null), attr.Manual ? null : GetOptionalInfo(field)));
            }
        }

        static MethodInfo GetOptionalInfo(FieldInfo info) => info.FieldType.GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance);
    }

    public override void ClearWorld()
    {
        foreach (var info in Defaults.Keys)
        {
            var data = Defaults[info];

            if (data.Alternative is null)
                info.SetValue(null, data.Default);
            else
                data.Alternative.Invoke(info.GetValue(data), null);
        }
    }
}