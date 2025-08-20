using ReLogic.Content;
using Reverie.Content.Tiles.TemperateForest.Furniture;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ObjectInteractions;
using Terraria.Localization;
using Terraria.ObjectData;
using Terraria.Utilities;

namespace Reverie.Core.Tiles.Actors;

public enum FurnitureType
{
    Chair,
    Table,
    Workbench,
    Bed,
    Dresser,
    Chest,
    Sink,
    Bathtub,
    Toilet,
    Bookcase,
    Piano,
    Lamp,
    Sofa,
    Chandelier,
    Lantern,
    Candelabra,
    Clock,
    Candle,
    Torch,
    Campfire,
    Platform,
    OpenDoor,
    ClosedDoor
}

public abstract class FurnitureActor : ModTile
{
    public FurnitureType FurnitureType { get; set; }
    public virtual bool IsOpen { get; }
    public virtual int ClosedDoorType { get; }
    public virtual int OpenDoorType { get; }
    public int DustID { get; set; }
    public Color LightColor { get; set; } = new Color(224, 255, 197); // Default warm light
    protected Asset<Texture2D> flameTexture;

    protected virtual bool CanSitOn => FurnitureType switch
    {
        FurnitureType.Chair => true,
        FurnitureType.Sofa => true,
        FurnitureType.Bed => true,
        _ => false
    };

    protected virtual bool CanBeOpened => FurnitureType switch
    {
        FurnitureType.Dresser => true,
        FurnitureType.Chest => true,
        FurnitureType.OpenDoor => true,
        FurnitureType.ClosedDoor => true,
        _ => false
    };

    protected virtual bool HasLighting => FurnitureType switch
    {
        FurnitureType.Lamp => true,
        FurnitureType.Chandelier => true,
        FurnitureType.Lantern => true,
        FurnitureType.Candelabra => true,
        FurnitureType.Candle => true,
        FurnitureType.Torch => true,
        FurnitureType.Campfire => true,
        _ => false
    };

    public override void SetStaticDefaults()
    {
        SetupCommonProperties();
        SetupTypeSpecificProperties();
        SetupTileObjectData();
        LoadTextures();
    }

    protected virtual void SetupCommonProperties()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        DustType = DustID;
    }

    protected virtual void SetupTypeSpecificProperties()
    {
        switch (FurnitureType)
        {
            case FurnitureType.Chair:
                TileID.Sets.HasOutlines[Type] = true;
                TileID.Sets.CanBeSatOnForNPCs[Type] = true;
                TileID.Sets.CanBeSatOnForPlayers[Type] = true;
                AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);
                AdjTiles = [TileID.Chairs];
                AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.Chair"));
                break;

            case FurnitureType.Table:
                Main.tileTable[Type] = true;
                Main.tileSolidTop[Type] = true;
                TileID.Sets.IgnoredByNpcStepUp[Type] = true;
                AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
                AdjTiles = [TileID.Tables];
                AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.Table"));
                break;

            case FurnitureType.Workbench:
                Main.tileTable[Type] = true;
                Main.tileSolidTop[Type] = true;
                TileID.Sets.IgnoredByNpcStepUp[Type] = true;
                AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
                AdjTiles = [TileID.WorkBenches];
                AddMapEntry(new Color(200, 200, 200), Language.GetText("ItemName.WorkBench"));
                break;

            case FurnitureType.Bed:
                TileID.Sets.HasOutlines[Type] = true;
                TileID.Sets.CanBeSleptIn[Type] = true;
                TileID.Sets.InteractibleByNPCs[Type] = true;
                TileID.Sets.IsValidSpawnPoint[Type] = true;
                AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);
                AdjTiles = [TileID.Beds];
                AddMapEntry(new Color(191, 142, 111), Language.GetText("ItemName.Bed"));
                break;

            case FurnitureType.Dresser:
                Main.tileSolidTop[Type] = true;
                Main.tileTable[Type] = true;
                Main.tileContainer[Type] = true;
                TileID.Sets.HasOutlines[Type] = true;
                TileID.Sets.BasicDresser[Type] = true;
                TileID.Sets.AvoidedByNPCs[Type] = true;
                TileID.Sets.InteractibleByNPCs[Type] = true;
                TileID.Sets.IsAContainer[Type] = true;
                AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
                AdjTiles = [TileID.Dressers];
                AddMapEntry(new Color(200, 200, 200), CreateMapEntryName(), FurnitureHelpers.MapDresserName);
                break;

            case FurnitureType.Chest:
                Main.tileContainer[Type] = true;
                TileID.Sets.HasOutlines[Type] = true;
                TileID.Sets.BasicChest[Type] = true;
                TileID.Sets.AvoidedByNPCs[Type] = true;
                TileID.Sets.InteractibleByNPCs[Type] = true;
                TileID.Sets.IsAContainer[Type] = true;
                TileID.Sets.GeneralPlacementTiles[Type] = false;
                AdjTiles = [TileID.Containers];
                AddMapEntry(new Color(200, 200, 200), CreateMapEntryName(), FurnitureHelpers.MapChestName);
                break;

            case FurnitureType.Sink:
                Main.tileTable[Type] = true;
                Main.tileSolidTop[Type] = true;
                TileID.Sets.IgnoredByNpcStepUp[Type] = true;
                AdjTiles = [TileID.Sinks];
                AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.Sink"));
                break;

            case FurnitureType.Bathtub:
                Main.tileSolid[Type] = false;
                Main.tileLighted[Type] = true;
                AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
                AdjTiles = [TileID.Bathtubs];
                AddMapEntry(new Color(100, 100, 60), Language.GetText("ItemName.Bathtub"));
                break;

            case FurnitureType.Bookcase:
                AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
                AdjTiles = [TileID.Bookcases];
                AddMapEntry(new Color(100, 100, 60), Language.GetText("ItemName.Bookcase"));
                break;

            case FurnitureType.Piano:
                AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
                AdjTiles = [TileID.Pianos];
                AddMapEntry(new Color(100, 100, 60), Language.GetText("ItemName.Piano"));
                break;

            case FurnitureType.Lamp:
                Main.tileLighted[Type] = true;
                AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
                AdjTiles = [TileID.Lamps];
                AddMapEntry(new Color(100, 100, 60), Language.GetText("ItemName.LampPost"));
                break;

            case FurnitureType.Sofa:
                TileID.Sets.CanBeSatOnForNPCs[Type] = true;
                TileID.Sets.CanBeSatOnForPlayers[Type] = true;
                TileID.Sets.HasOutlines[Type] = true;
                AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);
                AdjTiles = [TileID.Benches];
                AddMapEntry(new Color(100, 100, 60), Language.GetText("ItemName.Bench"));
                break;

            case FurnitureType.Chandelier:
                Main.tileLighted[Type] = true;
                AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
                AdjTiles = [TileID.Chandeliers];
                AddMapEntry(new Color(100, 100, 60), Language.GetText("MapObject.Chandelier"));
                break;

            case FurnitureType.Lantern:
                Main.tileFrameImportant[Type] = true;
                Main.tileNoAttach[Type] = true;
                Main.tileLighted[Type] = true;
                Main.tileLavaDeath[Type] = true;

                AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
                AdjTiles = [TileID.HangingLanterns];
                AddMapEntry(new Color(100, 100, 60), Language.GetText("MapObject.Lantern"));
                break;

            case FurnitureType.Candelabra:
                Main.tileLighted[Type] = true;
                AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
                AdjTiles = [TileID.Candelabras];
                AddMapEntry(new Color(100, 100, 60), Language.GetText("ItemName.Candelabra"));
                break;

            case FurnitureType.Clock:
                TileID.Sets.HasOutlines[Type] = true;
                TileID.Sets.Clock[Type] = true;
                AdjTiles = [TileID.GrandfatherClocks];
                AddMapEntry(new Color(200, 200, 200), Language.GetText("ItemName.GrandfatherClock"));
                break;

            case FurnitureType.Torch:
                Main.tileLighted[Type] = true;
                Main.tileSolid[Type] = false;
                Main.tileNoFail[Type] = true;
                Main.tileWaterDeath[Type] = true;
                TileID.Sets.FramesOnKillWall[Type] = true;
                TileID.Sets.DisableSmartInteract[Type] = true;
                TileID.Sets.Torch[Type] = true;
                AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
                AdjTiles = [TileID.Torches];
                AddMapEntry(new Color(200, 200, 200), Language.GetText("ItemName.Torch"));
                break;

            case FurnitureType.Campfire:
                Main.tileLighted[Type] = true;
                Main.tileWaterDeath[Type] = true;
                TileID.Sets.HasOutlines[Type] = true;
                TileID.Sets.InteractibleByNPCs[Type] = true;
                TileID.Sets.Campfire[Type] = true;
                AdjTiles = [TileID.Campfire];
                AddMapEntry(new Color(254, 121, 2), Language.GetText("ItemName.Campfire"));
                break;

            case FurnitureType.Platform:
                Main.tileSolidTop[Type] = true;
                Main.tileSolid[Type] = true;
                Main.tileTable[Type] = true;
                TileID.Sets.Platforms[Type] = true;
                AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);
                AdjTiles = [TileID.Platforms];
                AddMapEntry(new Color(200, 200, 200));
                break;

            case FurnitureType.OpenDoor:
                DustType = DustID;
                AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);
                AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.Door"));

                Main.tileSolid[Type] = false;
                Main.tileNoSunLight[Type] = true;

                TileID.Sets.HousingWalls[Type] = true;
                TileID.Sets.HasOutlines[Type] = true;
                TileID.Sets.CloseDoorID[Type] = ClosedDoorType;

                AdjTiles = [TileID.OpenDoor];
                break;

            case FurnitureType.ClosedDoor:
                DustType = DustID;
                AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);
                AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.Door"));

                Main.tileBlockLight[Type] = true;
                Main.tileSolid[Type] = true;

                TileID.Sets.NotReallySolid[Type] = true;
                TileID.Sets.DrawsWalls[Type] = true;
                TileID.Sets.OpenDoorID[Type] = OpenDoorType; 
                TileID.Sets.HasOutlines[Type] = true;

                AdjTiles = [TileID.ClosedDoor];
                break;
        }
    }

    protected virtual void SetupTileObjectData()
    {
        switch (FurnitureType)
        {
            case FurnitureType.Chair:
                TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
                TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
                TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, 2);
                TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
                TileObjectData.newTile.StyleWrapLimit = 2;
                TileObjectData.newTile.StyleMultiplier = 2;
                TileObjectData.newTile.StyleHorizontal = true;
                TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
                TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
                TileObjectData.addAlternate(1);
                break;

            case FurnitureType.Table:
                TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
                TileObjectData.newTile.StyleHorizontal = true;
                TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
                break;

            case FurnitureType.Workbench:
                TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
                TileObjectData.newTile.CoordinateHeights = [18];
                break;

            case FurnitureType.Bed:
                TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);
                TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
                TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, -2);
                break;

            case FurnitureType.Dresser:
                TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
                TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(Chest.FindEmptyChest, -1, 0, true);
                TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Chest.AfterPlacement_Hook, -1, 0, false);
                TileObjectData.newTile.AnchorInvalidTiles = new int[] {
                        TileID.MagicalIceBlock, TileID.Boulder, TileID.BouncyBoulder,
                        TileID.LifeCrystalBoulder, TileID.RollingCactus
                    };
                TileObjectData.newTile.LavaDeath = false;
                break;

            case FurnitureType.Chest:
                TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
                TileObjectData.newTile.Origin = new Point16(0, 1);
                TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
                TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(Chest.FindEmptyChest, -1, 0, true);
                TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Chest.AfterPlacement_Hook, -1, 0, false);
                TileObjectData.newTile.AnchorInvalidTiles = new int[] {
                    TileID.MagicalIceBlock, TileID.Boulder, TileID.BouncyBoulder,
                    TileID.LifeCrystalBoulder, TileID.RollingCactus
                };
                TileObjectData.newTile.StyleHorizontal = true;
                TileObjectData.newTile.LavaDeath = false;
                TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
                break;

            case FurnitureType.Sink:
                TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
                TileObjectData.newTile.StyleHorizontal = true;
                TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
                break;

            case FurnitureType.Bathtub:
                TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);
                TileObjectData.newTile.StyleHorizontal = true;
                TileObjectData.newTile.Origin = new Point16(1, 1);
                TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile | AnchorType.Table, TileObjectData.newTile.Width, 0);
                TileObjectData.newTile.CoordinateHeights = [16, 18];
                TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
                TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
                TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
                TileObjectData.addAlternate(1);
                break;

            case FurnitureType.Bookcase:
                TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
                TileObjectData.newTile.Origin = new Point16(2, 3);
                TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 18];
                break;

            case FurnitureType.Piano:
                TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
                TileObjectData.newTile.Origin = new Point16(2, 1);
                TileObjectData.newTile.CoordinateHeights = [16, 16];
                break;

            case FurnitureType.Lamp:
                TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
                TileObjectData.newTile.Origin = new Point16(0, 2);
                TileObjectData.newTile.Height = 3;
                TileObjectData.newTile.CoordinateHeights = [16, 16, 18];
                break;

            case FurnitureType.Sofa:
                TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
                TileObjectData.newTile.Origin = new Point16(1, 1);
                TileObjectData.newTile.Width = 3;
                TileObjectData.newTile.Height = 2;
                TileObjectData.newTile.CoordinateHeights = [16, 18];
                break;

            case FurnitureType.Chandelier:
                TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
                TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
                TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
                TileObjectData.newTile.Origin = new Point16(1, 0);
                break;

            case FurnitureType.Lantern:
                TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
                TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 0);
                TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
                TileObjectData.newTile.Origin = new Point16(0, 0);
                TileObjectData.newTile.CoordinateHeights = [16, 18];
                break;

            case FurnitureType.Candelabra:
                TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
                TileObjectData.newTile.Origin = new Point16(1, 1);
                TileObjectData.newTile.CoordinateHeights = [16, 18];
                break;

            case FurnitureType.Clock:
                TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
                TileObjectData.newTile.Height = 5;
                TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16];
                break;

            case FurnitureType.Torch:
                TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.Torches, 0));
                TileObjectData.newSubTile.CopyFrom(TileObjectData.newTile);
                TileObjectData.newSubTile.LinkedAlternates = true;
                TileObjectData.newSubTile.WaterDeath = false;
                TileObjectData.newSubTile.LavaDeath = false;
                TileObjectData.newSubTile.WaterPlacement = LiquidPlacement.Allowed;
                TileObjectData.newSubTile.LavaPlacement = LiquidPlacement.Allowed;
                TileObjectData.addSubTile(1);
                break;

            case FurnitureType.Campfire:
                TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.Campfire, 0));
                TileObjectData.newTile.StyleLineSkip = 9;
                break;

            case FurnitureType.Platform:
                TileObjectData.newTile.CoordinateHeights = new[] { 16 };
                TileObjectData.newTile.CoordinateWidth = 16;
                TileObjectData.newTile.CoordinatePadding = 2;
                TileObjectData.newTile.StyleHorizontal = true;
                TileObjectData.newTile.StyleMultiplier = 27;
                TileObjectData.newTile.StyleWrapLimit = 27;
                TileObjectData.newTile.UsesCustomCanPlace = false;
                TileObjectData.newTile.LavaDeath = true;
                break;

            case FurnitureType.OpenDoor:
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
                break;

            case FurnitureType.ClosedDoor:
                TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.ClosedDoor, 0));
                break;
        }

        TileObjectData.addTile(Type);
    }

    protected virtual void LoadTextures()
    {
        if (FurnitureType == FurnitureType.Campfire || FurnitureType == FurnitureType.Torch)
        {
            flameTexture = ModContent.Request<Texture2D>(Texture + "_Flame");
        }
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
    {
        if (CanSitOn)
            return settings.player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance);

        if (CanBeOpened)
            return true;

        if (FurnitureType == FurnitureType.Clock || FurnitureType == FurnitureType.Campfire)
            return true;

        return false;
    }

    public override LocalizedText DefaultContainerName(int frameX, int frameY)
    {
        return CreateMapEntryName();
    }

    public override void MouseOver(int i, int j)
    {
        if (CanBeOpened && !CanSitOn)
        {
            var player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = TileLoader.GetItemDropFromTypeAndStyle(Type);
        }

        if (FurnitureType == FurnitureType.Dresser)
        {
            Player player = Main.LocalPlayer;
            FurnitureHelpers.MouseOverNearAndFarSharedLogic(player, i, j);
            if (Main.tile[i, j].TileFrameY > 0)
            {
                player.cursorItemIconID = ItemID.FamiliarShirt;
                player.cursorItemIconText = "";
            }
        }

        if (FurnitureType == FurnitureType.Chest)
        {
            Player player = Main.LocalPlayer;
            Tile tile = Main.tile[i, j];
            int left = i;
            int top = j;
            if (tile.TileFrameX % 36 != 0)
                left--;
            if (tile.TileFrameY != 0)
                top--;

            int chest = Chest.FindChest(left, top);
            player.cursorItemIconID = -1;
            if (chest < 0)
            {
                player.cursorItemIconText = Language.GetTextValue("LegacyChestType.0");
            }
            else
            {
                string defaultName = TileLoader.DefaultContainerName(tile.TileType, tile.TileFrameX, tile.TileFrameY);
                player.cursorItemIconText = Main.chest[chest].name.Length > 0 ? Main.chest[chest].name : defaultName;
                if (player.cursorItemIconText == defaultName)
                {
                    player.cursorItemIconID = TileLoader.GetItemDropFromTypeAndStyle(Type);
                    player.cursorItemIconText = "";
                }
            }
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
        }
    }

    public override bool RightClick(int i, int j)
    {
        Player player = Main.LocalPlayer;

        if (CanSitOn)
        {
            if (player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance))
            {
                player.GamepadEnableGrappleCooldown();
                player.sitting.SitDown(player, i, j);
            }
        }

        if (FurnitureType == FurnitureType.Dresser)
        {
            int left = Main.tile[i, j].TileFrameX / 18;
            left %= 3;
            left = i - left;
            int top = j - Main.tile[i, j].TileFrameY / 18;
            if (Main.tile[i, j].TileFrameY == 0)
            {
                Main.CancelClothesWindow(true);
                Main.mouseRightRelease = false;
                player.CloseSign();
                player.SetTalkNPC(-1);
                Main.npcChatCornerItem = 0;
                Main.npcChatText = "";
                if (Main.editChest)
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    Main.editChest = false;
                    Main.npcChatText = string.Empty;
                }
                if (player.editedChestName)
                {
                    NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f);
                    player.editedChestName = false;
                }
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    if (left == player.chestX && top == player.chestY && player.chest != -1)
                    {
                        player.chest = -1;
                        Recipe.FindRecipes();
                        SoundEngine.PlaySound(SoundID.MenuClose);
                    }
                    else
                    {
                        NetMessage.SendData(MessageID.RequestChestOpen, -1, -1, null, left, top);
                        Main.stackSplit = 600;
                    }
                }
                else
                {
                    player.piggyBankProjTracker.Clear();
                    player.voidLensChest.Clear();
                    int chestIndex = Chest.FindChest(left, top);
                    if (chestIndex != -1)
                    {
                        Main.stackSplit = 600;
                        if (chestIndex == player.chest)
                        {
                            player.chest = -1;
                            Recipe.FindRecipes();
                            SoundEngine.PlaySound(SoundID.MenuClose);
                        }
                        else if (chestIndex != player.chest && player.chest == -1)
                        {
                            player.OpenChest(left, top, chestIndex);
                            SoundEngine.PlaySound(SoundID.MenuOpen);
                        }
                        else
                        {
                            player.OpenChest(left, top, chestIndex);
                            SoundEngine.PlaySound(SoundID.MenuTick);
                        }
                        Recipe.FindRecipes();
                    }
                }
            }
            else
            {
                Main.playerInventory = false;
                player.chest = -1;
                Recipe.FindRecipes();
                player.SetTalkNPC(-1);
                Main.npcChatCornerItem = 0;
                Main.npcChatText = "";
                Main.interactedDresserTopLeftX = left;
                Main.interactedDresserTopLeftY = top;
                Main.OpenClothesWindow();
            }
        }

        if (FurnitureType == FurnitureType.Chest)
        {
            Tile tile = Main.tile[i, j];
            Main.mouseRightRelease = false;
            int left = i;
            int top = j;
            if (tile.TileFrameX % 36 != 0)
                left--;
            if (tile.TileFrameY != 0)
                top--;

            player.CloseSign();
            player.SetTalkNPC(-1);
            Main.npcChatCornerItem = 0;
            Main.npcChatText = "";
            if (Main.editChest)
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
                Main.editChest = false;
                Main.npcChatText = string.Empty;
            }

            if (player.editedChestName)
            {
                NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f);
                player.editedChestName = false;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                if (left == player.chestX && top == player.chestY && player.chest != -1)
                {
                    player.chest = -1;
                    Recipe.FindRecipes();
                    SoundEngine.PlaySound(SoundID.MenuClose);
                }
                else
                {
                    NetMessage.SendData(MessageID.RequestChestOpen, -1, -1, null, left, top);
                    Main.stackSplit = 600;
                }
            }
            else
            {
                int chest = Chest.FindChest(left, top);
                if (chest != -1)
                {
                    Main.stackSplit = 600;
                    if (chest == player.chest)
                    {
                        player.chest = -1;
                        SoundEngine.PlaySound(SoundID.MenuClose);
                    }
                    else
                    {
                        SoundEngine.PlaySound(player.chest < 0 ? SoundID.MenuOpen : SoundID.MenuTick);
                        player.OpenChest(left, top, chest);
                    }
                    Recipe.FindRecipes();
                }
            }
        }

        if (FurnitureType == FurnitureType.Clock)
        {
            string text = "AM";
            double time = Main.time;
            if (!Main.dayTime)
                time += 54000.0;

            time = (time / 86400.0) * 24.0;
            time = time - 7.5 - 12.0;
            if (time < 0.0)
                time += 24.0;

            if (time >= 12.0)
                text = "PM";

            int intTime = (int)time;
            double deltaTime = time - intTime;
            deltaTime = (int)(deltaTime * 60.0);
            string text2 = string.Concat(deltaTime);
            if (deltaTime < 10.0)
                text2 = "0" + text2;

            if (intTime > 12)
                intTime -= 12;

            if (intTime == 0)
                intTime = 12;

            Main.NewText($"Time: {intTime}:{text2} {text}", 255, 240, 20);
        }

        if (FurnitureType == FurnitureType.Campfire)
        {
            SoundEngine.PlaySound(SoundID.Mech, new Vector2(i * 16, j * 16));
            ToggleTile(i, j);
        }

        return true;
    }

    public override void ModifySmartInteractCoords(ref int width, ref int height, ref int frameWidth, ref int frameHeight, ref int extraY)
    {
        if (FurnitureType == FurnitureType.Dresser)
        {
            width = 3;
            height = 1;
            extraY = 0;
            return;
        }
        if (FurnitureType == FurnitureType.Bed)
        {
            width = 2;
            height = 2;
            return;
        }
    }

    public override void MouseOverFar(int i, int j)
    {
        if (FurnitureType == FurnitureType.Dresser)
        {
            Player player = Main.LocalPlayer;
            FurnitureHelpers.MouseOverNearAndFarSharedLogic(player, i, j);
            if (player.cursorItemIconText == "")
            {
                player.cursorItemIconEnabled = false;
                player.cursorItemIconID = 0;
            }
        }
        if (FurnitureType == FurnitureType.Chest)
        {
            MouseOver(i, j);
            Player player = Main.LocalPlayer;
            if (player.cursorItemIconText == "")
            {
                player.cursorItemIconEnabled = false;
                player.cursorItemIconID = 0;
            }
        }
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        if (FurnitureType == FurnitureType.Dresser || FurnitureType == FurnitureType.Chest)
        {
            Chest.DestroyChest(i, j);
        }
    }

    public override void AnimateTile(ref int frame, ref int frameCounter)
    {
        if (FurnitureType == FurnitureType.Campfire)
        {
            if (++frameCounter >= 4)
            {
                frameCounter = 0;
                frame = ++frame % 8;
            }
        }
    }

    public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
    {
        if (FurnitureType == FurnitureType.Campfire)
        {
            var tile = Main.tile[i, j];
            if (tile.TileFrameY < 36)
            {
                frameYOffset = Main.tileFrame[type] * 36;
            }
            else
            {
                frameYOffset = 252;
            }
        }
    }

    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
    {
        if (FurnitureType == FurnitureType.Campfire)
        {
            if (Main.gamePaused || !Main.instance.IsActive)
                return;

            if (!Lighting.UpdateEveryFrame || new FastRandom(Main.TileFrameSeed).WithModifier(i, j).Next(4) == 0)
            {
                var tile = Main.tile[i, j];
                if (tile.TileFrameY == 0 && Main.rand.NextBool(3) && (Main.drawToScreen && Main.rand.NextBool(4) || !Main.drawToScreen))
                {
                    var dust = Dust.NewDustDirect(new Vector2(i * 16 + 2, j * 16 - 4), 4, 8, Terraria.ID.DustID.Smoke, 0f, 0f, 100);
                    if (tile.TileFrameX == 0)
                        dust.position.X += Main.rand.Next(8);

                    if (tile.TileFrameX == 36)
                        dust.position.X -= Main.rand.Next(8);

                    dust.alpha += Main.rand.Next(100);
                    dust.velocity *= 0.2f;
                    dust.velocity.Y -= 0.5f + Main.rand.Next(10) * 0.1f;
                    dust.fadeIn = 0.5f + Main.rand.Next(10) * 0.1f;
                }
            }
        }
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        if (!HasLighting) return;

        var tile = Main.tile[i, j];

        switch (FurnitureType)
        {
            case FurnitureType.Campfire:
                if (tile.TileFrameY < 36)
                {
                    var pulse = Main.rand.Next(28, 42) * 0.005f;
                    pulse += (270 - Main.mouseTextColor) / 700f;
                    r = 0.8f + pulse;
                    g = 0.7f + pulse;
                    b = 0.55f + pulse;
                }
                break;

            case FurnitureType.Torch:
                r = 0.8f;
                g = 0.7f;
                b = 0.55f;
                break;

            case FurnitureType.Lamp:
                if (tile.TileFrameX < 18 && tile.TileFrameY == 0)
                {
                    (r, g, b) = (LightColor.R / 255f, LightColor.G / 255f, LightColor.B / 255f);
                }
                break;

            case FurnitureType.Chandelier:
                if (tile.TileFrameX == 18 && tile.TileFrameY == 18)
                {
                    (r, g, b) = (LightColor.R / 255f, LightColor.G / 255f, LightColor.B / 255f);
                }
                break;

            case FurnitureType.Candelabra:
                if (tile.TileFrameX == 18 && tile.TileFrameY == 0)
                {
                    (r, g, b) = (LightColor.R / 255f, LightColor.G / 255f, LightColor.B / 255f);
                }
                break;

            case FurnitureType.Lantern:
                if (tile.TileFrameX < 18 && tile.TileFrameY == 0)
                {
                    (r, g, b) = (LightColor.R / 255f, LightColor.G / 255f, LightColor.B / 255f);
                }
                break;

            default:
                (r, g, b) = (LightColor.R / 255f, LightColor.G / 255f, LightColor.B / 255f);
                break;
        }
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        var tile = Main.tile[i, j];

        if (!TileDrawing.IsVisible(tile))
            return;

        var zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
        if (Main.drawToScreen)
            zero = Vector2.Zero;

        switch (FurnitureType)
        {
            case FurnitureType.Campfire:
                if (tile.TileFrameY < 36 && flameTexture != null)
                {
                    var color = new Color(255, 255, 255, 0);
                    var width = 16;
                    var offsetY = 0;
                    var height = 16;
                    var frameX = tile.TileFrameX;
                    var frameY = tile.TileFrameY;
                    var addFrX = 0;
                    var addFrY = 0;

                    TileLoader.SetDrawPositions(i, j, ref width, ref offsetY, ref height, ref frameX, ref frameY);
                    TileLoader.SetAnimationFrame(Type, i, j, ref addFrX, ref addFrY);

                    var drawRectangle = new Rectangle(tile.TileFrameX, tile.TileFrameY + addFrY, 16, 16);

                    spriteBatch.Draw(flameTexture.Value, new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y + offsetY) + zero, drawRectangle, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
                break;

            case FurnitureType.Torch:
                if (flameTexture != null)
                {
                    var offsetY = 0;
                    if (WorldGen.SolidTile(i, j - 1))
                        offsetY = 4;

                    var randSeed = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (uint)i);
                    var color = new Color(100, 100, 100, 0);
                    var width = 20;
                    var height = 20;
                    int frameX = tile.TileFrameX;
                    int frameY = tile.TileFrameY;

                    for (var k = 0; k < 7; k++)
                    {
                        var xx = Utils.RandomInt(ref randSeed, -10, 11) * 0.15f;
                        var yy = Utils.RandomInt(ref randSeed, -10, 1) * 0.35f;

                        spriteBatch.Draw(flameTexture.Value, new Vector2(i * 16 - (int)Main.screenPosition.X - (width - 16f) / 2f + xx, j * 16 - (int)Main.screenPosition.Y + offsetY + yy) + zero, new Rectangle(frameX, frameY, width, height), color, 0f, default, 1f, SpriteEffects.None, 0f);
                    }
                }
                break;

            case FurnitureType.Lamp:

            case FurnitureType.Chandelier:
                var glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
                if (glowTexture != null)
                {
                    int height = tile.TileFrameY == 36 ? 18 : 16;
                    spriteBatch.Draw(glowTexture.Value, new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, height), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
                break;
        }
    }

    public override void NearbyEffects(int i, int j, bool closer)
    {
        if (FurnitureType == FurnitureType.Campfire)
        {
            if (Main.tile[i, j].TileFrameY < 36)
            {
                Main.SceneMetrics.HasCampfire = true;
            }
        }
    }

    public override void HitWire(int i, int j)
    {
        switch (FurnitureType)
        {
            case FurnitureType.Campfire:
                ToggleTile(i, j);
                break;

            case FurnitureType.Lamp:
                ToggleLamp(i, j);
                break;

            case FurnitureType.Chandelier:
                ToggleChandelier(i, j);
                break;

            case FurnitureType.Candelabra:
                ToggleCandelabra(i, j);
                break;
        }
    }

    protected virtual void ToggleTile(int i, int j)
    {
        if (FurnitureType == FurnitureType.Campfire)
        {
            var tile = Main.tile[i, j];
            var topX = i - tile.TileFrameX % 54 / 18;
            var topY = j - tile.TileFrameY % 36 / 18;

            var frameAdjustment = (short)(tile.TileFrameY >= 36 ? -36 : 36);

            for (var x = topX; x < topX + 3; x++)
            {
                for (var y = topY; y < topY + 2; y++)
                {
                    Main.tile[x, y].TileFrameY += frameAdjustment;

                    if (Wiring.running)
                    {
                        Wiring.SkipWire(x, y);
                    }
                }
            }

            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                NetMessage.SendTileSquare(-1, topX, topY, 3, 2);
            }
        }
    }

    protected virtual void ToggleLamp(int i, int j)
    {
        var data = TileObjectData.GetTileData(Type, 0);
        int width = data.CoordinateFullWidth;

        j -= Framing.GetTileSafely(i, j).TileFrameY / 18;

        for (int h = 0; h < 3; h++)
        {
            var tile = Framing.GetTileSafely(i, j + h);
            tile.TileFrameX += (short)((tile.TileFrameX < width) ? width : -width);

            Wiring.SkipWire(i, j + h);
        }

        NetMessage.SendTileSquare(-1, i, j, data.Width, data.Height);
    }

    protected virtual void ToggleChandelier(int i, int j)
    {
        var data = TileObjectData.GetTileData(Type, 0);
        int width = data.CoordinateFullWidth;

        j -= Framing.GetTileSafely(i, j).TileFrameY / 18;

        for (int h = 0; h < 2; h++)
        {
            var tile = Framing.GetTileSafely(i, j + h);
            tile.TileFrameX += (short)((tile.TileFrameX < width) ? width : -width);

            Wiring.SkipWire(i, j + h);
        }

        NetMessage.SendTileSquare(-1, i, j, data.Width, data.Height);
    }

    protected virtual void ToggleCandelabra(int i, int j)
    {
        var data = TileObjectData.GetTileData(Type, 0);
        int width = data.CoordinateFullWidth;

        (i, j) = (i - Framing.GetTileSafely(i, j).TileFrameY / 18, j - Framing.GetTileSafely(i, j).TileFrameX % width / 18);

        for (int y = 0; y < 2; y++)
        {
            for (int x = 0; x < 2; x++)
            {
                var tile = Framing.GetTileSafely(i + x, j + y);
                tile.TileFrameX += (short)((tile.TileFrameX < width) ? width : -width);

                Wiring.SkipWire(i + x, j + y);
            }
        }

        NetMessage.SendTileSquare(-1, i, j, data.Width, data.Height);
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
    {
        if (FurnitureType == FurnitureType.Torch)
        {
            offsetY = 0;
            if (WorldGen.SolidTile(i, j - 1))
                offsetY = 4;
        }
    }
}

public static class FurnitureHelpers
{
    public static void MouseOverNearAndFarSharedLogic(Player player, int i, int j)
    {
        Tile tile = Main.tile[i, j];
        int left = i;
        int top = j;
        left -= tile.TileFrameX % 54 / 18;
        if (tile.TileFrameY % 36 != 0)
        {
            top--;
        }
        int chestIndex = Chest.FindChest(left, top);
        player.cursorItemIconID = -1;
        if (chestIndex < 0)
        {
            player.cursorItemIconText = Language.GetTextValue("LegacyDresserType.0");
        }
        else
        {
            string defaultName = TileLoader.DefaultContainerName(tile.TileType, tile.TileFrameX, tile.TileFrameY);

            if (Main.chest[chestIndex].name != "")
            {
                player.cursorItemIconText = Main.chest[chestIndex].name;
            }
            else
            {
                player.cursorItemIconText = defaultName;
            }
            if (player.cursorItemIconText == defaultName)
            {
                player.cursorItemIconID = ModContent.ItemType<BirchDresserItem>();
                player.cursorItemIconText = "";
            }
        }
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
    }

    public static string MapDresserName(string name, int i, int j)
    {
        int left = i;
        int top = j;
        Tile tile = Main.tile[i, j];
        if (tile.TileFrameX % 36 != 0)
        {
            left--;
        }

        if (tile.TileFrameY != 0)
        {
            top--;
        }

        int chest = Chest.FindChest(left, top);
        if (chest < 0)
        {
            return Language.GetTextValue("LegacyDresserType.0");
        }

        if (Main.chest[chest].name == "")
        {
            return name;
        }

        return name + ": " + Main.chest[chest].name;
    }

    public static string MapChestName(string name, int i, int j)
    {
        int left = i;
        int top = j;
        Tile tile = Main.tile[i, j];
        if (tile.TileFrameX % 36 != 0)
        {
            left--;
        }

        if (tile.TileFrameY != 0)
        {
            top--;
        }

        int chest = Chest.FindChest(left, top);
        if (chest < 0)
        {
            return Language.GetTextValue("LegacyChestType.0");
        }

        if (Main.chest[chest].name == "")
        {
            return name;
        }

        return name + ": " + Main.chest[chest].name;
    }
}