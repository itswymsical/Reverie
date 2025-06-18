using System.IO;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Reverie.Content.Tiles.Rainforest.Surface.Trees;

// The tile entity that handles canopy logic and rendering
public class KapokCanopyEntity : ModTileEntity
{
    public int canopyStyle = 0;
    public int treeHeight = 30;

    public override bool IsTileValidForEntity(int x, int y)
    {
        var tile = Main.tile[x, y];
        return tile.HasTile && tile.TileType == ModContent.TileType<KapokCanopyTile>();
    }

    public override void Update()
    {
        // Check if the tree below still exists
        var belowTile = Framing.GetTileSafely(Position.X, Position.Y + 1);
        if (!belowTile.HasTile || belowTile.TileType != ModContent.TileType<KapokTree>())
        {
            // Tree is gone, remove this canopy
            Kill(Position.X, Position.Y);
        }
    }

    public override void SaveData(TagCompound tag)
    {
        tag["canopyStyle"] = canopyStyle;
        tag["treeHeight"] = treeHeight;
    }

    public override void LoadData(TagCompound tag)
    {
        canopyStyle = tag.GetInt("canopyStyle");
        treeHeight = tag.GetInt("treeHeight");
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(canopyStyle);
        writer.Write(treeHeight);
    }

    public override void NetReceive(BinaryReader reader)
    {
        canopyStyle = reader.ReadInt32();
        treeHeight = reader.ReadInt32();
    }

    // Helper method to get a canopy entity at specific coordinates
    public static KapokCanopyEntity GetEntityAt(int x, int y)
    {
        if (TileEntity.ByPosition.TryGetValue(new Point16(x, y), out TileEntity tileEntity))
        {
            return tileEntity as KapokCanopyEntity;
        }
        return null;
    }
}

// The actual tile that represents the canopy top
public class KapokCanopyTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileBlockLight[Type] = true; // This tile blocks light
        Main.tileSolid[Type] = false;

        // Make it a 1x1 tile for simple placement
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.CoordinateHeights = new int[] { 16 };
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;

        // Can only be placed on Kapok trees
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
        TileObjectData.newTile.AnchorValidTiles = new int[] { ModContent.TileType<KapokTree>() };

        TileObjectData.addTile(Type);

        AddMapEntry(new Color(34, 139, 34), CreateMapEntryName());

        DustType = DustID.Grass;
        HitSound = SoundID.Grass;

        // This makes it a tile entity
        //TileID.Sets.HasOutlines[Type] = true;
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!effectOnly && !fail)
        {
            // Remove the tile entity when tile is destroyed
            var entity = KapokCanopyEntity.GetEntityAt(i, j);
            if (entity != null)
            {
                entity.Kill(i, j);
            }
        }
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        // Don't draw the actual tile, we'll draw our custom canopy
        var entity = KapokCanopyEntity.GetEntityAt(i, j);
        if (entity != null)
        {
            DrawCanopy(spriteBatch, i, j, entity);
        }

        return false; // Don't draw the normal tile
    }

    private void DrawCanopy(SpriteBatch spriteBatch, int i, int j, KapokCanopyEntity entity)
    {
        var canopyTexture = ModContent.Request<Texture2D>("Reverie/Content/Tiles/Rainforest/Surface/Trees/KapokCanopy").Value;

        // Calculate world position for the canopy center
        // The tile entity is at the tree top, so we position the canopy around it
        var canopyWorldX = (i - 3) * 16; // Offset left to center 8-wide canopy on 1-wide trunk
        var canopyWorldY = j * 16 - 32; // Position slightly above the tree top

        // Convert to screen coordinates
        var screenPos = new Vector2(
            canopyWorldX - Main.screenPosition.X,
            canopyWorldY - Main.screenPosition.Y
        );

        // Only draw if on screen
        if (screenPos.X > -200 && screenPos.X < Main.screenWidth + 200 &&
            screenPos.Y > -200 && screenPos.Y < Main.screenHeight + 200)
        {
            // Source rectangle for the canopy style
            var sourceRect = new Rectangle(
                entity.canopyStyle * 124, // 126 pixels wide per style
                0,
                124, // Width of one canopy sprite
                98  // Height of canopy sprite
            );

            // Draw the canopy
            spriteBatch.Draw(
                canopyTexture,
                screenPos,
                sourceRect,
                Lighting.GetColor(i, j), // Use game lighting
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                0f
            );
        }
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        var entity = KapokCanopyEntity.GetEntityAt(i, j);
        if (entity != null)
        {
            // Create heavy shade directly under this tile
            r *= 0.2f;
            g *= 0.3f; // Slightly more green
            b *= 0.2f;
        }
    }
    public override void PlaceInWorld(int i, int j, Item item)
    {
        // Create and place the tile entity
        var entity = new KapokCanopyEntity();
        int entityID = entity.Place(i, j);

        if (entityID != -1)
        {
            entity.canopyStyle = WorldGen.genRand.Next(3);
            Main.NewText($"Canopy entity created at ({i}, {j}) with style {entity.canopyStyle}", Color.Yellow);
        }
    }
}