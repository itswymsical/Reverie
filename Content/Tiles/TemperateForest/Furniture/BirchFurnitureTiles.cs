/// <summary>
/// Some code structure adapted from Spirit Reforged: https://github.com/GabeHasWon/SpiritReforged/tree/master/Common/TileCommon/PresetTiles/Furniture
/// </summary>
using Reverie.Content.Dusts;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ObjectInteractions;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Reverie.Content.Tiles.TemperateForest.Furniture;

public class BirchChairTile : ModTile
{
    public const int NEXT_STYLE_HEIGHT = 40;

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.CanBeSatOnForNPCs[Type] = true;
        TileID.Sets.CanBeSatOnForPlayers[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);

        DustType = ModContent.DustType<BirchDust>();
        AdjTiles = [TileID.Chairs];

        AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.Chair"));

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
        TileObjectData.addTile(Type);
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
    {
        return settings.player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance); // Avoid being able to trigger it from long range
    }

    public override void ModifySittingTargetInfo(int i, int j, ref TileRestingInfo info)
    {
        Tile tile = Framing.GetTileSafely(i, j);

        info.TargetDirection = -1;
        if (tile.TileFrameX != 0)
        {
            info.TargetDirection = 1;
        }

        info.AnchorTilePosition.X = i;
        info.AnchorTilePosition.Y = j;

        if (tile.TileFrameY % NEXT_STYLE_HEIGHT == 0)
        {
            info.AnchorTilePosition.Y++;
        }
    }

    public override bool RightClick(int i, int j)
    {
        Player player = Main.LocalPlayer;

        if (player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance))
        {
            player.GamepadEnableGrappleCooldown();
            player.sitting.SitDown(player, i, j);
        }

        return true;
    }

    public override void MouseOver(int i, int j)
    {
        Player player = Main.LocalPlayer;

        if (!player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance))
            return;

        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<BirchChairItem>();

        if (Main.tile[i, j].TileFrameX / 18 < 1)
        {
            player.cursorItemIconReversed = true;
        }
    }
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

public class BirchWorkbenchTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileTable[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileFrameImportant[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.IgnoredByNpcStepUp[Type] = true;

        DustType = ModContent.DustType<BirchDust>();
        AdjTiles = [TileID.WorkBenches];

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
        TileObjectData.newTile.CoordinateHeights = [18];
        TileObjectData.addTile(Type);

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);

        AddMapEntry(new Color(200, 200, 200), Language.GetText("ItemName.WorkBench"));
    }

    public override void NumDust(int x, int y, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }
}

public class BirchTableTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileTable[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileFrameImportant[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.IgnoredByNpcStepUp[Type] = true;

        DustType = ModContent.DustType<BirchDust>();
        AdjTiles = [TileID.Tables];

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
        TileObjectData.addTile(Type);

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);

        AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.Table"));
    }

    public override void NumDust(int x, int y, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }
}

public class BirchDresserTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolidTop[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true;
        Main.tileContainer[Type] = true;
        Main.tileLavaDeath[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.BasicDresser[Type] = true;
        TileID.Sets.AvoidedByNPCs[Type] = true;
        TileID.Sets.InteractibleByNPCs[Type] = true;
        TileID.Sets.IsAContainer[Type] = true;
        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);

        AdjTiles = new int[] { TileID.Dressers };
        DustType = ModContent.DustType<BirchDust>();

        AddMapEntry(new Color(200, 200, 200), CreateMapEntryName(), MapChestName);

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
        TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(Chest.FindEmptyChest, -1, 0, true);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Chest.AfterPlacement_Hook, -1, 0, false);
        TileObjectData.newTile.AnchorInvalidTiles = new int[] {
                TileID.MagicalIceBlock,
                TileID.Boulder,
                TileID.BouncyBoulder,
                TileID.LifeCrystalBoulder,
                TileID.RollingCactus
            };
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.addTile(Type);
    }

    public override LocalizedText DefaultContainerName(int frameX, int frameY)
    {
        return CreateMapEntryName();
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
    {
        return true;
    }

    public override void ModifySmartInteractCoords(ref int width, ref int height, ref int frameWidth, ref int frameHeight, ref int extraY)
    {
        width = 3;
        height = 1;
        extraY = 0;
    }

    public override bool RightClick(int i, int j)
    {
        Player player = Main.LocalPlayer;
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
        return true;
    }

    public void MouseOverNearAndFarSharedLogic(Player player, int i, int j)
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
            string defaultName = TileLoader.DefaultContainerName(tile.TileType, tile.TileFrameX, tile.TileFrameY); // This gets the ContainerName text for the currently selected language

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

    public override void MouseOverFar(int i, int j)
    {
        Player player = Main.LocalPlayer;
        MouseOverNearAndFarSharedLogic(player, i, j);
        if (player.cursorItemIconText == "")
        {
            player.cursorItemIconEnabled = false;
            player.cursorItemIconID = 0;
        }
    }

    public override void MouseOver(int i, int j)
    {
        Player player = Main.LocalPlayer;
        MouseOverNearAndFarSharedLogic(player, i, j);
        if (Main.tile[i, j].TileFrameY > 0)
        {
            player.cursorItemIconID = ItemID.FamiliarShirt;
            player.cursorItemIconText = "";
        }
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        Chest.DestroyChest(i, j);
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
            return Language.GetTextValue("LegacyDresserType.0");
        }

        if (Main.chest[chest].name == "")
        {
            return name;
        }

        return name + ": " + Main.chest[chest].name;
    }
}

public class BirchSinkTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileTable[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileFrameImportant[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.IgnoredByNpcStepUp[Type] = true;

        DustType = ModContent.DustType<BirchDust>();
        AdjTiles = [TileID.Sinks];

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.Sink"));
    }

    public override void NumDust(int x, int y, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }
}

public class BirchSofaTile : ModTile
{
    private static bool WithinRange(int i, int j, Player player) => player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance);

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.Origin = new Point16(1, 1);
        TileObjectData.newTile.Width = 3;
        TileObjectData.newTile.Height = 2;
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.addTile(Type);
        TileID.Sets.CanBeSatOnForNPCs[Type] = true;
        TileID.Sets.CanBeSatOnForPlayers[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);
        AddMapEntry(new Color(100, 100, 60), Language.GetText("ItemName.Bench"));
        AdjTiles = [TileID.Benches];
        DustType = -1;
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => WithinRange(i, j, settings.player);

    public override bool RightClick(int i, int j)
    {
        Player player = Main.LocalPlayer;
        if (WithinRange(i, j, player))
        {
            player.GamepadEnableGrappleCooldown();
            player.sitting.SitDown(player, i, j);
        }

        return true;
    }

    public override void MouseOver(int i, int j)
    {
        Player player = Main.LocalPlayer;
        if (WithinRange(i, j, player))
        {
            player.noThrow = 2;
            player.cursorItemIconID = ModContent.ItemType<BirchSofaItem>();
            player.cursorItemIconEnabled = true;
        }
    }
}

public class BirchClockTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        TileID.Sets.Clock[Type] = true;

        DustType = ModContent.DustType<BirchDust>();
        AdjTiles = [TileID.GrandfatherClocks];

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.Height = 5;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16];
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(200, 200, 200), Language.GetText("ItemName.GrandfatherClock"));
    }

    public override bool RightClick(int x, int y)
    {
        string text = "AM";
        double time = Main.time;
        if (!Main.dayTime)
        {
            time += 54000.0;
        }

        time = (time / 86400.0) * 24.0;
        time = time - 7.5 - 12.0;
        if (time < 0.0)
        {
            time += 24.0;
        }

        if (time >= 12.0)
        {
            text = "PM";
        }

        int intTime = (int)time;
        double deltaTime = time - intTime;
        deltaTime = (int)(deltaTime * 60.0);
        string text2 = string.Concat(deltaTime);
        if (deltaTime < 10.0)
        {
            text2 = "0" + text2;
        }

        if (intTime > 12)
        {
            intTime -= 12;
        }

        if (intTime == 0)
        {
            intTime = 12;
        }
        Main.NewText($"Time: {intTime}:{text2} {text}", 255, 240, 20);
        return true;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }
}

public class BirchBedTile : ModTile
{
    public const int NEXT_STYLE_HEIGHT = 38;

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.CanBeSleptIn[Type] = true;
        TileID.Sets.InteractibleByNPCs[Type] = true;
        TileID.Sets.IsValidSpawnPoint[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);

        DustType = ModContent.DustType<BirchDust>();
        AdjTiles = [TileID.Beds];

        TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);
        TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
        TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, -2);
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(191, 142, 111), Language.GetText("ItemName.Bed"));
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
    {
        return true;
    }

    public override void ModifySmartInteractCoords(ref int width, ref int height, ref int frameWidth, ref int frameHeight, ref int extraY)
    {
        // Because beds have special smart interaction, this splits up the left and right side into the necessary 2x2 sections
        width = 2; // Default to the Width defined for TileObjectData.newTile
        height = 2; // Default to the Height defined for TileObjectData.newTile
                    //extraY = 0; // Depends on how you set up frameHeight and CoordinateHeights and CoordinatePaddingFix.Y
    }

    public override void ModifySleepingTargetInfo(int i, int j, ref TileRestingInfo info)
    {
        // Default values match the regular vanilla bed
        // You might need to mess with the info here if your bed is not a typical 4x2 tile
        info.VisualOffset.Y += 4f; // Move player down a notch because the bed is not as high as a regular bed
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = 1;
    }

    public override bool RightClick(int i, int j)
    {
        Player player = Main.LocalPlayer;
        Tile tile = Main.tile[i, j];
        int spawnX = (i - (tile.TileFrameX / 18)) + (tile.TileFrameX >= 72 ? 5 : 2);
        int spawnY = j + 2;

        if (tile.TileFrameY % NEXT_STYLE_HEIGHT != 0)
        {
            spawnY--;
        }

        if (!Player.IsHoveringOverABottomSideOfABed(i, j))
        {
            if (player.IsWithinSnappngRangeToTile(i, j, PlayerSleepingHelper.BedSleepingMaxDistance))
            {
                player.GamepadEnableGrappleCooldown();
                player.sleeping.StartSleeping(player, i, j);
            }
        }
        else
        {
            player.FindSpawn();

            if (player.SpawnX == spawnX && player.SpawnY == spawnY)
            {
                player.RemoveSpawn();
                Main.NewText(Language.GetTextValue("Game.SpawnPointRemoved"), byte.MaxValue, 240, 20);
            }
            else if (Player.CheckSpawn(spawnX, spawnY))
            {
                player.ChangeSpawn(spawnX, spawnY);
                Main.NewText(Language.GetTextValue("Game.SpawnPointSet"), byte.MaxValue, 240, 20);
            }
        }

        return true;
    }

    public override void MouseOver(int i, int j)
    {
        Player player = Main.LocalPlayer;

        if (!Player.IsHoveringOverABottomSideOfABed(i, j))
        {
            if (player.IsWithinSnappngRangeToTile(i, j, PlayerSleepingHelper.BedSleepingMaxDistance))
            {
                player.noThrow = 2;
                player.cursorItemIconEnabled = true;
                player.cursorItemIconID = ItemID.SleepingIcon;
            }
        }
        else
        {
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<BirchBedItem>();
        }
    }
}

public class BirchBookcaseTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
        TileObjectData.newTile.Origin = new Point16(2, 3);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 18];
        TileObjectData.addTile(Type);

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
        AddMapEntry(new Color(100, 100, 60), Language.GetText("ItemName.Bookcase"));
        AdjTiles = [TileID.Bookcases];

        DustType = ModContent.DustType<BirchDust>();
    }
}

public class BirchBathtubTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileLighted[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.Origin = new Point16(1, 1);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile | AnchorType.Table, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.addAlternate(1);
        TileObjectData.addTile(Type);

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
        AddMapEntry(new Color(100, 100, 60), Language.GetText("ItemName.Bathtub"));
        AdjTiles = [TileID.Bathtubs];

        DustType = ModContent.DustType<BirchDust>();
    }
}

public class BirchPianoTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
        TileObjectData.newTile.Origin = new Point16(2, 1);
        TileObjectData.newTile.CoordinateHeights = [16, 16];
        TileObjectData.addTile(Type);

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
        AddMapEntry(new Color(100, 100, 60), Language.GetText("ItemName.Piano"));
        AdjTiles = [TileID.Pianos];

        DustType = ModContent.DustType<BirchDust>();
    }
}

public class BirchCandelabraTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLighted[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.Origin = new Point16(1, 1);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.addTile(Type);

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
        AddMapEntry(new Color(100, 100, 60), Language.GetText("ItemName.Candelabra"));
        AdjTiles = [TileID.Candelabras];
        DustType = -1;
    }

    public override void HitWire(int i, int j)
    {
        var data = TileObjectData.GetTileData(Type, 0);
        int width = data.CoordinateFullWidth;

        //Move to the multitile's top left
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

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        var tile = Framing.GetTileSafely(i, j);
        var color = Color.Orange;

        if (tile.TileFrameX == 18 && tile.TileFrameY == 0)
            (r, g, b) = (color.R / 255f, color.G / 255f, color.B / 255f);
    }

    //public virtual bool BlurGlowmask => true;

    //public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    //{
    //    var tile = Framing.GetTileSafely(i, j);
    //    if (!TileDrawing.IsVisible(tile))
    //        return;

    //    var texture = GlowmaskTile.TileIdToGlowmask[Type].Glowmask.Value;
    //    var data = TileObjectData.GetTileData(tile);
    //    int height = data.CoordinateHeights[tile.TileFrameY / data.CoordinateFullHeight];
    //    var source = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, height);

    //    if (BlurGlowmask)
    //    {
    //        ulong randSeed = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (uint)i);
    //        for (int c = 0; c < 7; c++) //Draw our glowmask with a randomized position
    //        {
    //            float shakeX = Utils.RandomInt(ref randSeed, -10, 11) * 0.15f;
    //            float shakeY = Utils.RandomInt(ref randSeed, -10, 1) * 0.35f;
    //            var offset = new Vector2(shakeX, shakeY);

    //            var position = new Vector2(i, j) * 16 - Main.screenPosition + offset + TileExtensions.TileOffset;
    //            spriteBatch.Draw(texture, position, source, new Color(100, 100, 100, 0), 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
    //        }
    //    }
    //    else
    //    {
    //        var position = new Vector2(i, j) * 16 - Main.screenPosition + TileExtensions.TileOffset;
    //        spriteBatch.Draw(texture, position, source, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
    //    }
    //}
}

public class BirchChestTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileContainer[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.BasicChest[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.AvoidedByNPCs[Type] = true;
        TileID.Sets.InteractibleByNPCs[Type] = true;
        TileID.Sets.IsAContainer[Type] = true;
        //TileID.Sets.FriendlyFairyCanLureTo[Type] = true;
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        DustType = ModContent.DustType<BirchDust>();
        AdjTiles = [TileID.Containers];

        AddMapEntry(new Color(200, 200, 200), this.GetLocalization("MapEntry"), MapChestName);

        RegisterItemDrop(ModContent.ItemType<BirchChestItem>());
        RegisterItemDrop(ItemID.Chest);

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
        TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(Chest.FindEmptyChest, -1, 0, true);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Chest.AfterPlacement_Hook, -1, 0, false);
        TileObjectData.newTile.AnchorInvalidTiles = new int[] {
            TileID.MagicalIceBlock,
            TileID.Boulder,
            TileID.BouncyBoulder,
            TileID.LifeCrystalBoulder,
            TileID.RollingCactus
        };
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.addTile(Type);
    }

    public override ushort GetMapOption(int i, int j)
    {
        return 0;
    }

    public override LocalizedText DefaultContainerName(int frameX, int frameY)
    {
        return this.GetLocalization("MapEntry");
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
    {
        return true;
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

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = 1;
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        Chest.DestroyChest(i, j);
    }

    public override bool RightClick(int i, int j)
    {
        Player player = Main.LocalPlayer;
        Tile tile = Main.tile[i, j];
        Main.mouseRightRelease = false;
        int left = i;
        int top = j;
        if (tile.TileFrameX % 36 != 0)
        {
            left--;
        }

        if (tile.TileFrameY != 0)
        {
            top--;
        }

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

        return true;
    }

    public override void MouseOver(int i, int j)
    {
        Player player = Main.LocalPlayer;
        Tile tile = Main.tile[i, j];
        int left = i;
        int top = j;
        if (tile.TileFrameX % 36 != 0)
        {
            left--;
        }

        if (tile.TileFrameY != 0)
        {
            top--;
        }

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
                player.cursorItemIconID = ModContent.ItemType<BirchChestItem>();
                player.cursorItemIconText = "";
            }
        }

        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
    }

    public override void MouseOverFar(int i, int j)
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