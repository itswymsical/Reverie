using Reverie.Common.Tiles;
using Reverie.Content.Items.Tiles.Canopy;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace Reverie.Content.Tiles.Canopy;

public class WoodgrassTile : GrassTile
{
    protected override int DirtType => TileID.LivingWood;
    public override int spreadChance => 2;
    public override bool CanGrowPlants => true;
    public override List<int> PlantTypes => [ModContent.TileType<CanopyFoliageTile>()];

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMerge[TileID.LivingWood][Type] = true;
        Main.tileMerge[Type][TileID.LivingWood] = true;
        Main.tileMerge[Type][Type] = true;
        Merge(DirtType, Type);
        MineResist = 0.2f;
        DustType = DustID.t_LivingWood;
        RegisterItemDrop(ItemID.Wood);
        AddMapEntry(new Color(100, 150, 8));
    }
    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!fail && Main.rand.NextBool(30))
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 48, ModContent.ItemType<WoodgrassSeedsItem>());
    }
}