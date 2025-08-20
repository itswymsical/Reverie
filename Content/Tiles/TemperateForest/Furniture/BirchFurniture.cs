/// <summary>
/// Some code structure adapted from Spirit Reforged: https://github.com/GabeHasWon/SpiritReforged/tree/master/Common/TileCommon/PresetTiles/Furniture
/// </summary>
using ReLogic.Content;
using Reverie.Common.Systems;
using Reverie.Content.Dusts;
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

namespace Reverie.Content.Tiles.TemperateForest.Furniture;

public class BirchChairTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Chair;
        DustID = ModContent.DustType<BirchDust>();
        base.SetStaticDefaults();
    }
}
public class BirchChairItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Chair;
    protected override int TileType => ModContent.TileType<BirchChairTile>();
    protected override int MaterialType => ModContent.ItemType<BirchWoodItem>();
}

public class BirchWorkbenchTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Workbench;
        DustID = ModContent.DustType<BirchDust>();
        base.SetStaticDefaults();
    }
}
public class BirchWorkbenchItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Workbench;
    protected override int TileType => ModContent.TileType<BirchWorkbenchTile>();
    protected override int MaterialType => ModContent.ItemType<BirchWoodItem>();
}

public class BirchTableTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Table;
        DustID = ModContent.DustType<BirchDust>();
        base.SetStaticDefaults();
    }
}
public class BirchTableItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Table;
    protected override int TileType => ModContent.TileType<BirchTableTile>();
    protected override int MaterialType => ModContent.ItemType<BirchWoodItem>();
}

public class BirchDoorClosedTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        TileID.Sets.NotReallySolid[Type] = true;
        TileID.Sets.DrawsWalls[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.OpenDoorID[Type] = ModContent.TileType<BirchDoorOpenTile>();

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);

        DustType = ModContent.DustType<BirchDust>();
        AdjTiles = [TileID.ClosedDoor];

        AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.Door"));
        TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.ClosedDoor, 0));

        TileObjectData.addTile(Type);
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
    {
        return true;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = 1;
    }

    public override void MouseOver(int i, int j)
    {
        var player = Main.LocalPlayer;
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<BirchDoorItem>();
    }
}
public class BirchDoorOpenTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileLavaDeath[Type] = true;
        Main.tileNoSunLight[Type] = true;
        TileID.Sets.HousingWalls[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.CloseDoorID[Type] = ModContent.TileType<BirchDoorClosedTile>();

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);

        DustType = ModContent.DustType<BirchDust>();
        AdjTiles = [TileID.OpenDoor];
        RegisterItemDrop(ModContent.ItemType<BirchDoorItem>(), 0);
        TileID.Sets.CloseDoorID[Type] = ModContent.TileType<BirchDoorClosedTile>();

        AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.Door"));

        TileObjectData.newTile.Width = 2;
        TileObjectData.newTile.Height = 3;
        TileObjectData.newTile.Origin = new Point16(0, 0);
        TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 0);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.StyleMultiplier = 2;
        TileObjectData.newTile.StyleWrapLimit = 2;
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Origin = new Point16(0, 1);
        TileObjectData.addAlternate(0);
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Origin = new Point16(0, 2);
        TileObjectData.addAlternate(0);
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Origin = new Point16(1, 0);
        TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
        TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.addAlternate(1);
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Origin = new Point16(1, 1);
        TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
        TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.addAlternate(1);
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Origin = new Point16(1, 2);
        TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
        TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.addAlternate(1);
        TileObjectData.addTile(Type);
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
    {
        return true;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = 1;
    }

    public override void MouseOver(int i, int j)
    {
        Player player = Main.LocalPlayer;
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<BirchDoorItem>();
    }
}
public class BirchDoorItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.ClosedDoor;
    protected override int TileType => ModContent.TileType<BirchDoorClosedTile>();
    protected override int MaterialType => ModContent.ItemType<BirchWoodItem>();
}

public class BirchDresserTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Dresser;
        DustID = ModContent.DustType<BirchDust>();
        base.SetStaticDefaults();
    }
}
public class BirchDresserItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Dresser;
    protected override int TileType => ModContent.TileType<BirchDresserTile>();
    protected override int MaterialType => ModContent.ItemType<BirchWoodItem>();
}

public class BirchSinkTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Sink;
        DustID = ModContent.DustType<BirchDust>();
        base.SetStaticDefaults();
    }
}
public class BirchSinkItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Sink;
    protected override int TileType => ModContent.TileType<BirchSinkTile>();
    protected override int MaterialType => ModContent.ItemType<BirchWoodItem>();
}

public class BirchSofaTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Sofa;
        DustID = ModContent.DustType<BirchDust>();
        base.SetStaticDefaults();
    }
}
public class BirchSofaItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Sofa;
    protected override int TileType => ModContent.TileType<BirchSofaTile>();
    protected override int MaterialType => ModContent.ItemType<BirchWoodItem>();
}

public class BirchClockTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Clock;
        DustID = ModContent.DustType<BirchDust>();
        base.SetStaticDefaults();
    }
}
public class BirchClockItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Clock;
    protected override int TileType => ModContent.TileType<BirchClockTile>();
    protected override int MaterialType => ModContent.ItemType<BirchWoodItem>();
}

public class BirchLanternTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Lantern;
        DustID = ModContent.DustType<BirchDust>();
        LightColor = new(224, 255, 197);
        base.SetStaticDefaults();
    }
}
public class BirchLanternItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Lantern;
    protected override int TileType => ModContent.TileType<BirchLanternTile>();
    protected override int MaterialType => ModContent.ItemType<BirchWoodItem>();
}

public class BirchBedTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Bed;
        DustID = ModContent.DustType<BirchDust>();
        base.SetStaticDefaults();
    }
}
public class BirchBedItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Bed;
    protected override int TileType => ModContent.TileType<BirchBedTile>();
    protected override int MaterialType => ModContent.ItemType<BirchWoodItem>();
}

public class BirchBookcaseTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Bookcase;
        DustID = ModContent.DustType<BirchDust>();
        base.SetStaticDefaults();
    }
}
public class BirchBookcaseItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Bookcase;
    protected override int TileType => ModContent.TileType<BirchBookcaseTile>();
    protected override int MaterialType => ModContent.ItemType<BirchWoodItem>();
}

public class BirchBathtubTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Bathtub;
        DustID = ModContent.DustType<BirchDust>();
        base.SetStaticDefaults();
    }
}
public class BirchBathtubItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Bathtub;
    protected override int TileType => ModContent.TileType<BirchBathtubTile>();
    protected override int MaterialType => ModContent.ItemType<BirchWoodItem>();
}

public class BirchPianoTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Piano;
        DustID = ModContent.DustType<BirchDust>();
        base.SetStaticDefaults();
    }
}
public class BirchPianoItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Piano;
    protected override int TileType => ModContent.TileType<BirchPianoTile>();
    protected override int MaterialType => ModContent.ItemType<BirchWoodItem>();
}

public class BirchCandelabraTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Candelabra;
        DustID = ModContent.DustType<BirchDust>();
        LightColor = new(224, 255, 197);
        base.SetStaticDefaults();
    }
}
public class BirchCandelabraItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Candelabra;
    protected override int TileType => ModContent.TileType<BirchCandelabraTile>();
    protected override int MaterialType => ModContent.ItemType<BirchWoodItem>();
}

public class BirchChandelierTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Chandelier;
        DustID = ModContent.DustType<BirchDust>();
        LightColor = Color.Orange;
        base.SetStaticDefaults();
    }
}
public class BirchChandelierItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Chandelier;
    protected override int TileType => ModContent.TileType<BirchChandelierTile>();
    protected override int MaterialType => ModContent.ItemType<BirchWoodItem>();
}

public class BirchLampTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Lamp;
        DustID = ModContent.DustType<BirchDust>();
        LightColor = new(224, 255, 197);
        base.SetStaticDefaults();
    }
}
public class BirchLampItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Lamp;
    protected override int TileType => ModContent.TileType<BirchLampTile>();
    protected override int MaterialType => ModContent.ItemType<BirchWoodItem>();
}

public class BirchChestTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Chest;
        DustID = ModContent.DustType<BirchDust>();
        base.SetStaticDefaults();
    }
}
public class BirchChestItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Chest;
    protected override int TileType => ModContent.TileType<BirchChestTile>();
    protected override int MaterialType => ModContent.ItemType<BirchWoodItem>();
}

public class BirchCampfireTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Campfire;
        DustID = ModContent.DustType<BirchDust>();
        base.SetStaticDefaults();
    }
}
public class BirchCampfireItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Campfire;
    protected override int TileType => ModContent.TileType<BirchCampfireTile>();
    protected override int MaterialType => ModContent.ItemType<BirchWoodItem>();
    protected override int TorchType => ModContent.ItemType<BirchTorchItem>();

}

public class BirchPlatformTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Platform;
        DustID = ModContent.DustType<BirchDust>();
        base.SetStaticDefaults();
    }
}
public class BirchPlatformItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Platform;
    protected override int TileType => ModContent.TileType<BirchPlatformTile>();
    protected override int MaterialType => ModContent.ItemType<BirchWoodItem>();
}

public class BirchTorchTile : FurnitureActor
{
    public override void SetStaticDefaults()
    {
        FurnitureType = FurnitureType.Torch;
        DustID = ModContent.DustType<BirchDust>();
        LightColor = Color.White;
        base.SetStaticDefaults();
    }
}
public class BirchTorchItem : FurnitureItem
{
    protected override FurnitureType FurnitureType => FurnitureType.Torch;
    protected override int TileType => ModContent.TileType<BirchTorchTile>();
    protected override int MaterialType => ModContent.ItemType<BirchWoodItem>();
}