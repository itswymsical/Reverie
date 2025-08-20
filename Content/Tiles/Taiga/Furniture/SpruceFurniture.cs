/// <summary>
/// Some code structure adapted from Spirit Reforged: https://github.com/GabeHasWon/SpiritReforged/tree/master/Common/TileCommon/PresetTiles/Furniture
/// </summary>
using ReLogic.Content;
using Reverie.Content.Dusts;
using Reverie.Content.Tiles.TemperateForest.Furniture;
using Reverie.Core.Items.Components;
using Reverie.Core.Tiles.Actors;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ObjectInteractions;
using Terraria.Localization;
using Terraria.ObjectData;
using Terraria.Utilities;

namespace Reverie.Content.Tiles.Taiga.Furniture;

public class SpruceChairTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Chair;
        DustID = ModContent.DustType<SpruceDust>();
        base.SetStaticDefaults();
    }
}
public class SpruceChairItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Chair;
    protected override int TileType => ModContent.TileType<SpruceChairTile>();
    protected override int MaterialType => ModContent.ItemType<SpruceWoodItem>();
}

public class SpruceTableTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Table;
        DustID = ModContent.DustType<SpruceDust>();
        base.SetStaticDefaults();
    }
}
public class SpruceTableItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Table;
    protected override int TileType => ModContent.TileType<SpruceTableTile>();
    protected override int MaterialType => ModContent.ItemType<SpruceWoodItem>();
}

public class SpruceWorkBenchTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Workbench;
        DustID = ModContent.DustType<SpruceDust>();
        base.SetStaticDefaults();
    }
}
public class SpruceWorkBenchItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Workbench;
    protected override int TileType => ModContent.TileType<SpruceWorkBenchTile>();
    protected override int MaterialType => ModContent.ItemType<SpruceWoodItem>();
}

public class SpruceDresserTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Dresser;
        DustID = ModContent.DustType<SpruceDust>();
        base.SetStaticDefaults();
    }
}
public class SpruceDresserItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Dresser;
    protected override int TileType => ModContent.TileType<SpruceDresserTile>();
    protected override int MaterialType => ModContent.ItemType<SpruceWoodItem>();
}

public class SpruceBookcaseTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Bookcase;
        DustID = ModContent.DustType<SpruceDust>();
        base.SetStaticDefaults();
    }
}
public class SpruceBookcaseItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Bookcase;
    protected override int TileType => ModContent.TileType<SpruceBookcaseTile>();
    protected override int MaterialType => ModContent.ItemType<SpruceWoodItem>();
}

public class SpruceSofaTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Sofa;
        DustID = ModContent.DustType<SpruceDust>();
        base.SetStaticDefaults();
    }
}
public class SpruceSofaItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Sofa;
    protected override int TileType => ModContent.TileType<SpruceSofaTile>();
    protected override int MaterialType => ModContent.ItemType<SpruceWoodItem>();
}

public class SpruceBedTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Bed;
        DustID = ModContent.DustType<SpruceDust>();
        base.SetStaticDefaults();
    }
}
public class SpruceBedItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Bed;
    protected override int TileType => ModContent.TileType<SpruceBedTile>();
    protected override int MaterialType => ModContent.ItemType<SpruceWoodItem>();
}

public class SpruceSinkTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Sink;
        DustID = ModContent.DustType<SpruceDust>();
        base.SetStaticDefaults();
    }
}
public class SpruceSinkItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Sink;
    protected override int TileType => ModContent.TileType<SpruceSinkTile>();
    protected override int MaterialType => ModContent.ItemType<SpruceWoodItem>();
}


public class SpruceBathtubTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Bathtub;
        DustID = ModContent.DustType<SpruceDust>();
        base.SetStaticDefaults();
    }
}
public class SpruceBathtubItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Bathtub;
    protected override int TileType => ModContent.TileType<SpruceBathtubTile>();
    protected override int MaterialType => ModContent.ItemType<SpruceWoodItem>();
}

public class SpruceLampTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Lamp;
        DustID = ModContent.DustType<SpruceDust>();
        LightColor = new Color(255, 150, 70);
        base.SetStaticDefaults();
    }
}
public class SpruceLampItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Lamp;
    protected override int TileType => ModContent.TileType<SpruceLampTile>();
    protected override int MaterialType => ModContent.ItemType<SpruceWoodItem>();
    protected override int TorchType => ItemID.Torch/*ModContent.ItemType<SpruceTorchItem>()*/;
}

public class SpruceChandelierTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Chandelier;
        DustID = ModContent.DustType<SpruceDust>();
        LightColor = new Color(255, 150, 70);
        base.SetStaticDefaults();
    }
}
public class SpruceChandelierItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Chandelier;
    protected override int TileType => ModContent.TileType<SpruceChandelierTile>();
    protected override int MaterialType => ModContent.ItemType<SpruceWoodItem>();
    protected override int TorchType => ItemID.Torch/*ModContent.ItemType<SpruceTorchItem>()*/;
}

public class SpruceCandelabraTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Candelabra;
        DustID = ModContent.DustType<SpruceDust>();
        LightColor = new Color(255, 150, 70);
        base.SetStaticDefaults();
    }
}
public class SpruceCandelabraItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Candelabra;
    protected override int TileType => ModContent.TileType<SpruceCandelabraTile>();
    protected override int MaterialType => ModContent.ItemType<SpruceWoodItem>();
    protected override int TorchType => ItemID.Torch/*ModContent.ItemType<SpruceTorchItem>()*/;
}
public class SpruceLanternTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Lantern;
        DustID = ModContent.DustType<SpruceDust>();
        LightColor = new Color(255, 150, 70);
        base.SetStaticDefaults();
    }
}
public class SpruceLanternItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Lantern;
    protected override int TileType => ModContent.TileType<SpruceLanternTile>();
    protected override int MaterialType => ModContent.ItemType<SpruceWoodItem>();
    protected override int TorchType => ItemID.Torch/*ModContent.ItemType<SpruceTorchItem>()*/;
}
public class SprucePianoTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Piano;
        DustID = ModContent.DustType<SpruceDust>();
        base.SetStaticDefaults();
    }
}
public class SprucePianoItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Piano;
    protected override int TileType => ModContent.TileType<SprucePianoTile>();
    protected override int MaterialType => ModContent.ItemType<SpruceWoodItem>();
}

public class SpruceChestTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Chest;
        DustID = ModContent.DustType<SpruceDust>();
        base.SetStaticDefaults();
    }
}
public class SpruceChestItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Chest;
    protected override int TileType => ModContent.TileType<SpruceChestTile>();
    protected override int MaterialType => ModContent.ItemType<SpruceWoodItem>();
}
public class SpruceDoorOpenTile : FurnitureActor
{
    public override int ClosedDoorType => ModContent.TileType<SpruceDoorClosedTile>();
    public override int OpenDoorType => ModContent.TileType<SpruceDoorOpenTile>();
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.OpenDoor;
        DustID = ModContent.DustType<SpruceDust>();
        base.SetStaticDefaults();
    }
}
public class SpruceDoorClosedTile : FurnitureActor
{
    public override int ClosedDoorType => ModContent.TileType<SpruceDoorClosedTile>();
    public override int OpenDoorType => ModContent.TileType<SpruceDoorOpenTile>();
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.ClosedDoor;
        DustID = ModContent.DustType<SpruceDust>();
        base.SetStaticDefaults();
    }
}
public class SpruceDoorItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.ClosedDoor;
    protected override int TileType => ModContent.TileType<SpruceDoorClosedTile>();
    protected override int MaterialType => ModContent.ItemType<SpruceWoodItem>();
}
public class SprucePlatformTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Platform;
        DustID = ModContent.DustType<SpruceDust>();
        base.SetStaticDefaults();
    }
}
public class SprucePlatformItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Platform;
    protected override int TileType => ModContent.TileType<SprucePlatformTile>();
    protected override int MaterialType => ModContent.ItemType<SpruceWoodItem>();
}