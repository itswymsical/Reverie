using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.Localization;
using Terraria.ObjectData;
using Terraria.Utilities;

namespace Reverie.Content.Tiles.AssailantStronghold;

public class EbonwoodTallFenceTile : ModTile
{
    //public override void Load()
    //{
    //    base.Load();
    //    On_Main.DrawBackGore += RenderFence;
    //}
    //public override void Unload()
    //{
    //    base.Unload();
    //    On_Main.DrawBackGore -= RenderFence;
    //}

    //private void RenderFence(On_Main.orig_DrawBackGore orig, Main self)
    //{
    //    throw new NotImplementedException();
    //}

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileBlockLight[Type] = false;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinateHeights = [14];
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.DrawYOffset = 2;
        AddMapEntry(new Color(156, 87, 170), CreateMapEntryName());
        DustType = DustID.WoodFurniture;
        HitSound = SoundID.Dig;

        TileObjectData.addTile(Type);
    }

    public override void PlaceInWorld(int i, int j, Item item)
    {
        UpdateFenceColumn(i, j);
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!fail)
        {
            UpdateFenceColumn(i, j);
        }
    }

    private void UpdateFenceColumn(int centerX, int centerY)
    {
        int topY = FindFenceTop(centerX, centerY);
        int bottomY = FindFenceBottom(centerX, centerY);

        for (int y = topY; y <= bottomY; y++)
        {
            if (WorldGen.InWorld(centerX, y))
            {
                var tile = Main.tile[centerX, y];
                if (tile.HasTile && tile.TileType == Type)
                {
                    UpdateSingleFenceFrame(centerX, y);
                }
            }
        }

        if (Main.netMode != NetmodeID.SinglePlayer && topY <= bottomY)
        {
            NetMessage.SendTileSquare(-1, centerX, topY, 1, bottomY - topY + 1, TileChangeType.None);
        }
    }

    private int FindFenceTop(int x, int startY)
    {
        int y = startY;

        while (y > 0)
        {
            var tile = Framing.GetTileSafely(x, y - 1);
            if (!tile.HasTile || tile.TileType != Type)
                break;
            y--;
        }

        return y;
    }

    private int FindFenceBottom(int x, int startY)
    {
        int y = startY;

        while (y < Main.maxTilesY - 1)
        {
            var tile = Framing.GetTileSafely(x, y + 1);
            if (!tile.HasTile || tile.TileType != Type)
                break;
            y++;
        }

        return y;
    }

    private void UpdateSingleFenceFrame(int x, int y)
    {
        var tile = Main.tile[x, y];
        if (!tile.HasTile || tile.TileType != Type)
            return;

        var tileAbove = Framing.GetTileSafely(x, y - 1);
        var tileBelow = Framing.GetTileSafely(x, y + 1);

        bool hasFenceAbove = tileAbove.HasTile && tileAbove.TileType == Type;
        bool hasFenceBelow = tileBelow.HasTile && tileBelow.TileType == Type;

        int frameY;

        if (!hasFenceAbove)
        {
            frameY = 0;
        }
        else if (!hasFenceBelow)
        {
            frameY = 18;
        }
        else if (IsDirectlyBelowTop(x, y))
        {
            frameY = 18;
        }
        else
        {
            frameY = 36;
        }

        int variant = GetVariantForPosition(x, y);
        int frameX = variant * 18;

        tile.TileFrameX = (short)frameX;
        tile.TileFrameY = (short)frameY;
    }

    private bool IsDirectlyBelowTop(int x, int y)
    {
        var tileAbove = Framing.GetTileSafely(x, y - 1);
        if (!tileAbove.HasTile || tileAbove.TileType != Type)
            return false;

        var tileTwoAbove = Framing.GetTileSafely(x, y - 2);
        return !tileTwoAbove.HasTile || tileTwoAbove.TileType != Type;
    }

    private int GetVariantForPosition(int x, int y)
    {
        int seed = x * 1337 + y * 7919;
        var random = new UnifiedRandom(seed);
        return random.Next(3);
    }
}

public class EbonwoodTallFenceItem : ModItem
{
    public override string Texture => PLACEHOLDER;
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<EbonwoodTallFenceTile>());
    }
}