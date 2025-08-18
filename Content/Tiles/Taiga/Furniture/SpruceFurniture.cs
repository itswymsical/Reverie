/// <summary>
/// Some code structure adapted from Spirit Reforged: https://github.com/GabeHasWon/SpiritReforged/tree/master/Common/TileCommon/PresetTiles/Furniture
/// </summary>
using ReLogic.Content;
using Reverie.Content.Dusts;
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

public class SpruceChairTile : ModTile
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

        DustType = ModContent.DustType<SpruceDust>();
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
        var tile = Framing.GetTileSafely(i, j);

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
        var player = Main.LocalPlayer;

        if (player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance))
        {
            player.GamepadEnableGrappleCooldown();
            player.sitting.SitDown(player, i, j);
        }

        return true;
    }

    public override void MouseOver(int i, int j)
    {
        var player = Main.LocalPlayer;

        if (!player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance))
            return;

        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<SpruceChairItem>();

        if (Main.tile[i, j].TileFrameX / 18 < 1)
        {
            player.cursorItemIconReversed = true;
        }
    }
}

//public class SpruceDoorClosedTile : ModTile
//{
//    public override void SetStaticDefaults()
//    {
//        Main.tileFrameImportant[Type] = true;
//        Main.tileBlockLight[Type] = true;
//        Main.tileSolid[Type] = true;
//        Main.tileNoAttach[Type] = true;
//        Main.tileLavaDeath[Type] = true;
//        TileID.Sets.NotReallySolid[Type] = true;
//        TileID.Sets.DrawsWalls[Type] = true;
//        TileID.Sets.HasOutlines[Type] = true;
//        TileID.Sets.DisableSmartCursor[Type] = true;
//        TileID.Sets.OpenDoorID[Type] = ModContent.TileType<SpruceDoorOpenTile>();

//        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);

//        DustType = ModContent.DustType<SpruceDust>();
//        AdjTiles = [TileID.ClosedDoor];

//        AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.Door"));
//        TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.ClosedDoor, 0));

//        TileObjectData.addTile(Type);
//    }

//    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
//    {
//        return true;
//    }

//    public override void NumDust(int i, int j, bool fail, ref int num)
//    {
//        num = 1;
//    }

//    public override void MouseOver(int i, int j)
//    {
//        var player = Main.LocalPlayer;
//        player.noThrow = 2;
//        player.cursorItemIconEnabled = true;
//        player.cursorItemIconID = ModContent.ItemType<SpruceDoorItem>();
//    }
//}

//public class SpruceDoorOpenTile : ModTile
//{
//    public override void SetStaticDefaults()
//    {
//        Main.tileFrameImportant[Type] = true;
//        Main.tileSolid[Type] = false;
//        Main.tileLavaDeath[Type] = true;
//        Main.tileNoSunLight[Type] = true;
//        TileID.Sets.HousingWalls[Type] = true;
//        TileID.Sets.HasOutlines[Type] = true;
//        TileID.Sets.DisableSmartCursor[Type] = true;
//        TileID.Sets.CloseDoorID[Type] = ModContent.TileType<SpruceDoorClosedTile>();

//        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);

//        DustType = ModContent.DustType<SpruceDust>();
//        AdjTiles = [TileID.OpenDoor];
//        RegisterItemDrop(ModContent.ItemType<SpruceDoorItem>(), 0);
//        TileID.Sets.CloseDoorID[Type] = ModContent.TileType<SpruceDoorClosedTile>();

//        AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.Door"));

//        TileObjectData.newTile.Width = 2;
//        TileObjectData.newTile.Height = 3;
//        TileObjectData.newTile.Origin = new Point16(0, 0);
//        TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 0);
//        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
//        TileObjectData.newTile.UsesCustomCanPlace = true;
//        TileObjectData.newTile.LavaDeath = true;
//        TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
//        TileObjectData.newTile.CoordinateWidth = 16;
//        TileObjectData.newTile.CoordinatePadding = 2;
//        TileObjectData.newTile.StyleHorizontal = true;
//        TileObjectData.newTile.StyleMultiplier = 2;
//        TileObjectData.newTile.StyleWrapLimit = 2;
//        TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
//        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
//        TileObjectData.newAlternate.Origin = new Point16(0, 1);
//        TileObjectData.addAlternate(0);
//        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
//        TileObjectData.newAlternate.Origin = new Point16(0, 2);
//        TileObjectData.addAlternate(0);
//        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
//        TileObjectData.newAlternate.Origin = new Point16(1, 0);
//        TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
//        TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
//        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
//        TileObjectData.addAlternate(1);
//        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
//        TileObjectData.newAlternate.Origin = new Point16(1, 1);
//        TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
//        TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
//        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
//        TileObjectData.addAlternate(1);
//        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
//        TileObjectData.newAlternate.Origin = new Point16(1, 2);
//        TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
//        TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
//        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
//        TileObjectData.addAlternate(1);
//        TileObjectData.addTile(Type);
//    }

//    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
//    {
//        return true;
//    }

//    public override void NumDust(int i, int j, bool fail, ref int num)
//    {
//        num = 1;
//    }

//    public override void MouseOver(int i, int j)
//    {
//        var player = Main.LocalPlayer;
//        player.noThrow = 2;
//        player.cursorItemIconEnabled = true;
//        player.cursorItemIconID = ModContent.ItemType<SpruceDoorItem>();
//    }
//}

public class SpruceWorkbenchTile : ModTile
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

        DustType = ModContent.DustType<SpruceDust>();
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

public class SpruceTableTile : ModTile
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

        DustType = ModContent.DustType<SpruceDust>();
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

public class SpruceDresserTile : ModTile
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
        DustType = ModContent.DustType<SpruceDust>();

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
        var player = Main.LocalPlayer;
        var left = Main.tile[i, j].TileFrameX / 18;
        left %= 3;
        left = i - left;
        var top = j - Main.tile[i, j].TileFrameY / 18;
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
                var chestIndex = Chest.FindChest(left, top);
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
        var tile = Main.tile[i, j];
        var left = i;
        var top = j;
        left -= tile.TileFrameX % 54 / 18;
        if (tile.TileFrameY % 36 != 0)
        {
            top--;
        }
        var chestIndex = Chest.FindChest(left, top);
        player.cursorItemIconID = -1;
        if (chestIndex < 0)
        {
            player.cursorItemIconText = Language.GetTextValue("LegacyDresserType.0");
        }
        else
        {
            var defaultName = TileLoader.DefaultContainerName(tile.TileType, tile.TileFrameX, tile.TileFrameY); // This gets the ContainerName text for the currently selected language

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
                player.cursorItemIconID = ModContent.ItemType<SpruceDresserItem>();
                player.cursorItemIconText = "";
            }
        }
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
    }

    public override void MouseOverFar(int i, int j)
    {
        var player = Main.LocalPlayer;
        MouseOverNearAndFarSharedLogic(player, i, j);
        if (player.cursorItemIconText == "")
        {
            player.cursorItemIconEnabled = false;
            player.cursorItemIconID = 0;
        }
    }

    public override void MouseOver(int i, int j)
    {
        var player = Main.LocalPlayer;
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
        var left = i;
        var top = j;
        var tile = Main.tile[i, j];
        if (tile.TileFrameX % 36 != 0)
        {
            left--;
        }

        if (tile.TileFrameY != 0)
        {
            top--;
        }

        var chest = Chest.FindChest(left, top);
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

public class SpruceSinkTile : ModTile
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

        DustType = ModContent.DustType<SpruceDust>();
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

public class SpruceSofaTile : ModTile
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
        var player = Main.LocalPlayer;
        if (WithinRange(i, j, player))
        {
            player.GamepadEnableGrappleCooldown();
            player.sitting.SitDown(player, i, j);
        }

        return true;
    }

    public override void MouseOver(int i, int j)
    {
        var player = Main.LocalPlayer;
        if (WithinRange(i, j, player))
        {
            player.noThrow = 2;
            player.cursorItemIconID = ModContent.ItemType<SpruceSofaItem>();
            player.cursorItemIconEnabled = true;
        }
    }
}

//public class SpruceClockTile : ModTile
//{
//    public override void SetStaticDefaults()
//    {
//        Main.tileFrameImportant[Type] = true;
//        Main.tileNoAttach[Type] = true;
//        Main.tileLavaDeath[Type] = true;
//        TileID.Sets.Clock[Type] = true;

//        DustType = ModContent.DustType<SpruceDust>();
//        AdjTiles = [TileID.GrandfatherClocks];

//        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
//        TileObjectData.newTile.Height = 5;
//        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16];
//        TileObjectData.addTile(Type);

//        AddMapEntry(new Color(200, 200, 200), Language.GetText("ItemName.GrandfatherClock"));
//    }

//    public override bool RightClick(int x, int y)
//    {
//        var text = "AM";
//        var time = Main.time;
//        if (!Main.dayTime)
//        {
//            time += 54000.0;
//        }

//        time = time / 86400.0 * 24.0;
//        time = time - 7.5 - 12.0;
//        if (time < 0.0)
//        {
//            time += 24.0;
//        }

//        if (time >= 12.0)
//        {
//            text = "PM";
//        }

//        var intTime = (int)time;
//        var deltaTime = time - intTime;
//        deltaTime = (int)(deltaTime * 60.0);
//        var text2 = string.Concat(deltaTime);
//        if (deltaTime < 10.0)
//        {
//            text2 = "0" + text2;
//        }

//        if (intTime > 12)
//        {
//            intTime -= 12;
//        }

//        if (intTime == 0)
//        {
//            intTime = 12;
//        }
//        Main.NewText($"Time: {intTime}:{text2} {text}", 255, 240, 20);
//        return true;
//    }

//    public override void NumDust(int i, int j, bool fail, ref int num)
//    {
//        num = fail ? 1 : 3;
//    }
//}

public class SpruceBedTile : ModTile
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

        DustType = ModContent.DustType<SpruceDust>();
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
        var player = Main.LocalPlayer;
        var tile = Main.tile[i, j];
        var spawnX = i - tile.TileFrameX / 18 + (tile.TileFrameX >= 72 ? 5 : 2);
        var spawnY = j + 2;

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
        var player = Main.LocalPlayer;

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
            player.cursorItemIconID = ModContent.ItemType<SpruceBedItem>();
        }
    }
}

public class SpruceBookcaseTile : ModTile
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

        DustType = ModContent.DustType<SpruceDust>();
    }
}

public class SpruceBathtubTile : ModTile
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

        DustType = ModContent.DustType<SpruceDust>();
    }
}

public class SprucePianoTile : ModTile
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

        DustType = ModContent.DustType<SpruceDust>();
    }
}

public class SpruceCandelabraTile : ModTile
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
        var width = data.CoordinateFullWidth;

        //Move to the multitile's top left
        (i, j) = (i - Framing.GetTileSafely(i, j).TileFrameY / 18, j - Framing.GetTileSafely(i, j).TileFrameX % width / 18);

        for (var y = 0; y < 2; y++)
        {
            for (var x = 0; x < 2; x++)
            {
                var tile = Framing.GetTileSafely(i + x, j + y);
                tile.TileFrameX += (short)(tile.TileFrameX < width ? width : -width);

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

//public class SpruceChestTile : ModTile
//{
//    public override void SetStaticDefaults()
//    {
//        Main.tileContainer[Type] = true;
//        Main.tileFrameImportant[Type] = true;
//        Main.tileNoAttach[Type] = true;
//        TileID.Sets.HasOutlines[Type] = true;
//        TileID.Sets.BasicChest[Type] = true;
//        TileID.Sets.DisableSmartCursor[Type] = true;
//        TileID.Sets.AvoidedByNPCs[Type] = true;
//        TileID.Sets.InteractibleByNPCs[Type] = true;
//        TileID.Sets.IsAContainer[Type] = true;
//        //TileID.Sets.FriendlyFairyCanLureTo[Type] = true;
//        TileID.Sets.GeneralPlacementTiles[Type] = false;

//        DustType = ModContent.DustType<SpruceDust>();
//        AdjTiles = [TileID.Containers];

//        AddMapEntry(new Color(200, 200, 200), this.GetLocalization("MapEntry"), MapChestName);

//        RegisterItemDrop(ModContent.ItemType<SpruceChestItem>());
//        RegisterItemDrop(ItemID.Chest);

//        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
//        TileObjectData.newTile.Origin = new Point16(0, 1);
//        TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
//        TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(Chest.FindEmptyChest, -1, 0, true);
//        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Chest.AfterPlacement_Hook, -1, 0, false);
//        TileObjectData.newTile.AnchorInvalidTiles = new int[] {
//            TileID.MagicalIceBlock,
//            TileID.Boulder,
//            TileID.BouncyBoulder,
//            TileID.LifeCrystalBoulder,
//            TileID.RollingCactus
//        };
//        TileObjectData.newTile.StyleHorizontal = true;
//        TileObjectData.newTile.LavaDeath = false;
//        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
//        TileObjectData.addTile(Type);
//    }

//    public override ushort GetMapOption(int i, int j)
//    {
//        return 0;
//    }

//    public override LocalizedText DefaultContainerName(int frameX, int frameY)
//    {
//        return this.GetLocalization("MapEntry");
//    }

//    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
//    {
//        return true;
//    }

//    public static string MapChestName(string name, int i, int j)
//    {
//        var left = i;
//        var top = j;
//        var tile = Main.tile[i, j];
//        if (tile.TileFrameX % 36 != 0)
//        {
//            left--;
//        }

//        if (tile.TileFrameY != 0)
//        {
//            top--;
//        }

//        var chest = Chest.FindChest(left, top);
//        if (chest < 0)
//        {
//            return Language.GetTextValue("LegacyChestType.0");
//        }

//        if (Main.chest[chest].name == "")
//        {
//            return name;
//        }

//        return name + ": " + Main.chest[chest].name;
//    }

//    public override void NumDust(int i, int j, bool fail, ref int num)
//    {
//        num = 1;
//    }

//    public override void KillMultiTile(int i, int j, int frameX, int frameY)
//    {
//        Chest.DestroyChest(i, j);
//    }

//    public override bool RightClick(int i, int j)
//    {
//        var player = Main.LocalPlayer;
//        var tile = Main.tile[i, j];
//        Main.mouseRightRelease = false;
//        var left = i;
//        var top = j;
//        if (tile.TileFrameX % 36 != 0)
//        {
//            left--;
//        }

//        if (tile.TileFrameY != 0)
//        {
//            top--;
//        }

//        player.CloseSign();
//        player.SetTalkNPC(-1);
//        Main.npcChatCornerItem = 0;
//        Main.npcChatText = "";
//        if (Main.editChest)
//        {
//            SoundEngine.PlaySound(SoundID.MenuTick);
//            Main.editChest = false;
//            Main.npcChatText = string.Empty;
//        }

//        if (player.editedChestName)
//        {
//            NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f);
//            player.editedChestName = false;
//        }

//        if (Main.netMode == NetmodeID.MultiplayerClient)
//        {
//            if (left == player.chestX && top == player.chestY && player.chest != -1)
//            {
//                player.chest = -1;
//                Recipe.FindRecipes();
//                SoundEngine.PlaySound(SoundID.MenuClose);
//            }
//            else
//            {
//                NetMessage.SendData(MessageID.RequestChestOpen, -1, -1, null, left, top);
//                Main.stackSplit = 600;
//            }
//        }
//        else
//        {
//            var chest = Chest.FindChest(left, top);
//            if (chest != -1)
//            {
//                Main.stackSplit = 600;
//                if (chest == player.chest)
//                {
//                    player.chest = -1;
//                    SoundEngine.PlaySound(SoundID.MenuClose);
//                }
//                else
//                {
//                    SoundEngine.PlaySound(player.chest < 0 ? SoundID.MenuOpen : SoundID.MenuTick);
//                    player.OpenChest(left, top, chest);
//                }

//                Recipe.FindRecipes();
//            }
//        }

//        return true;
//    }

//    public override void MouseOver(int i, int j)
//    {
//        var player = Main.LocalPlayer;
//        var tile = Main.tile[i, j];
//        var left = i;
//        var top = j;
//        if (tile.TileFrameX % 36 != 0)
//        {
//            left--;
//        }

//        if (tile.TileFrameY != 0)
//        {
//            top--;
//        }

//        var chest = Chest.FindChest(left, top);
//        player.cursorItemIconID = -1;
//        if (chest < 0)
//        {
//            player.cursorItemIconText = Language.GetTextValue("LegacyChestType.0");
//        }
//        else
//        {
//            var defaultName = TileLoader.DefaultContainerName(tile.TileType, tile.TileFrameX, tile.TileFrameY);
//            player.cursorItemIconText = Main.chest[chest].name.Length > 0 ? Main.chest[chest].name : defaultName;
//            if (player.cursorItemIconText == defaultName)
//            {
//                player.cursorItemIconID = ModContent.ItemType<SpruceChestItem>();
//                player.cursorItemIconText = "";
//            }
//        }

//        player.noThrow = 2;
//        player.cursorItemIconEnabled = true;
//    }

//    public override void MouseOverFar(int i, int j)
//    {
//        MouseOver(i, j);
//        var player = Main.LocalPlayer;
//        if (player.cursorItemIconText == "")
//        {
//            player.cursorItemIconEnabled = false;
//            player.cursorItemIconID = 0;
//        }
//    }
//}

//public class SpruceCampfireTile : ModTile
//{
//    private Asset<Texture2D> flameTexture;

//    public override void SetStaticDefaults()
//    {
//        Main.tileLighted[Type] = true;
//        Main.tileFrameImportant[Type] = true;
//        Main.tileWaterDeath[Type] = true;
//        Main.tileLavaDeath[Type] = true;
//        TileID.Sets.HasOutlines[Type] = true;
//        TileID.Sets.InteractibleByNPCs[Type] = true;
//        TileID.Sets.Campfire[Type] = true;

//        DustType = -1;
//        AdjTiles = [TileID.Campfire];

//        TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.Campfire, 0));

//        TileObjectData.newTile.StyleLineSkip = 9;
//        TileObjectData.addTile(Type);

//        AddMapEntry(new Color(254, 121, 2), Language.GetText("ItemName.Campfire"));

//        flameTexture = ModContent.Request<Texture2D>(Texture + "_Flame");
//    }

//    public override void NearbyEffects(int i, int j, bool closer)
//    {
//        if (Main.tile[i, j].TileFrameY < 36)
//        {
//            Main.SceneMetrics.HasCampfire = true;
//        }
//    }

//    public override void MouseOver(int i, int j)
//    {
//        var player = Main.LocalPlayer;
//        player.noThrow = 2;
//        player.cursorItemIconEnabled = true;

//        var style = TileObjectData.GetTileStyle(Main.tile[i, j]);
//        player.cursorItemIconID = TileLoader.GetItemDropFromTypeAndStyle(Type, style);
//    }

//    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
//    {
//        return true;
//    }

//    public override bool RightClick(int i, int j)
//    {
//        SoundEngine.PlaySound(SoundID.Mech, new Vector2(i * 16, j * 16));
//        ToggleTile(i, j);
//        return true;
//    }

//    public override void HitWire(int i, int j)
//    {
//        ToggleTile(i, j);
//    }

//    public void ToggleTile(int i, int j)
//    {
//        var tile = Main.tile[i, j];
//        var topX = i - tile.TileFrameX % 54 / 18;
//        var topY = j - tile.TileFrameY % 36 / 18;

//        var frameAdjustment = (short)(tile.TileFrameY >= 36 ? -36 : 36);

//        for (var x = topX; x < topX + 3; x++)
//        {
//            for (var y = topY; y < topY + 2; y++)
//            {
//                Main.tile[x, y].TileFrameY += frameAdjustment;

//                if (Wiring.running)
//                {
//                    Wiring.SkipWire(x, y);
//                }
//            }
//        }

//        if (Main.netMode != NetmodeID.SinglePlayer)
//        {
//            NetMessage.SendTileSquare(-1, topX, topY, 3, 2);
//        }
//    }

//    public override void AnimateTile(ref int frame, ref int frameCounter)
//    {
//        if (++frameCounter >= 4)
//        {
//            frameCounter = 0;
//            frame = ++frame % 8;
//        }
//    }

//    public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
//    {
//        var tile = Main.tile[i, j];
//        if (tile.TileFrameY < 36)
//        {
//            frameYOffset = Main.tileFrame[type] * 36;
//        }
//        else
//        {
//            frameYOffset = 252;
//        }
//    }

//    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
//    {
//        if (Main.gamePaused || !Main.instance.IsActive)
//        {
//            return;
//        }
//        if (!Lighting.UpdateEveryFrame || new FastRandom(Main.TileFrameSeed).WithModifier(i, j).Next(4) == 0)
//        {
//            var tile = Main.tile[i, j];
//            if (tile.TileFrameY == 0 && Main.rand.NextBool(3) && (Main.drawToScreen && Main.rand.NextBool(4) || !Main.drawToScreen))
//            {
//                var dust = Dust.NewDustDirect(new Vector2(i * 16 + 2, j * 16 - 4), 4, 8, DustID.Smoke, 0f, 0f, 100);
//                if (tile.TileFrameX == 0)
//                    dust.position.X += Main.rand.Next(8);

//                if (tile.TileFrameX == 36)
//                    dust.position.X -= Main.rand.Next(8);

//                dust.alpha += Main.rand.Next(100);
//                dust.velocity *= 0.2f;
//                dust.velocity.Y -= 0.5f + Main.rand.Next(10) * 0.1f;
//                dust.fadeIn = 0.5f + Main.rand.Next(10) * 0.1f;
//            }
//        }
//    }

//    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
//    {
//        var tile = Main.tile[i, j];
//        if (tile.TileFrameY < 36)
//        {
//            var pulse = Main.rand.Next(28, 42) * 0.005f;
//            pulse += (270 - Main.mouseTextColor) / 700f;
//            r = 0.8f + pulse;
//            g = 0.7f + pulse;
//            b = 0.55f + pulse;
//        }
//    }

//    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
//    {
//        var tile = Main.tile[i, j];

//        if (!TileDrawing.IsVisible(tile))
//        {
//            return;
//        }

//        if (tile.TileFrameY < 36)
//        {
//            var color = new Color(255, 255, 255, 0);

//            var zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
//            if (Main.drawToScreen)
//            {
//                zero = Vector2.Zero;
//            }

//            var width = 16;
//            var offsetY = 0;
//            var height = 16;
//            var frameX = tile.TileFrameX;
//            var frameY = tile.TileFrameY;
//            var addFrX = 0;
//            var addFrY = 0;

//            TileLoader.SetDrawPositions(i, j, ref width, ref offsetY, ref height, ref frameX, ref frameY);
//            TileLoader.SetAnimationFrame(Type, i, j, ref addFrX, ref addFrY);

//            var drawRectangle = new Rectangle(tile.TileFrameX, tile.TileFrameY + addFrY, 16, 16);

//            spriteBatch.Draw(flameTexture.Value, new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y + offsetY) + zero, drawRectangle, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
//        }
//    }
//}

//public class SprucePlatformTile : ModTile
//{
//    public override void SetStaticDefaults()
//    {

//        Main.tileLighted[Type] = true;
//        Main.tileFrameImportant[Type] = true;
//        Main.tileSolidTop[Type] = true;
//        Main.tileSolid[Type] = true;
//        Main.tileNoAttach[Type] = true;
//        Main.tileTable[Type] = true;
//        Main.tileLavaDeath[Type] = true;
//        TileID.Sets.Platforms[Type] = true;
//        TileID.Sets.DisableSmartCursor[Type] = true;

//        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);
//        AddMapEntry(new Color(200, 200, 200));

//        DustType = ModContent.DustType<SpruceDust>();
//        AdjTiles = [TileID.Platforms];

//        TileObjectData.newTile.CoordinateHeights = new[] { 16 };
//        TileObjectData.newTile.CoordinateWidth = 16;
//        TileObjectData.newTile.CoordinatePadding = 2;
//        TileObjectData.newTile.StyleHorizontal = true;
//        TileObjectData.newTile.StyleMultiplier = 27;
//        TileObjectData.newTile.StyleWrapLimit = 27;
//        TileObjectData.newTile.UsesCustomCanPlace = false;
//        TileObjectData.newTile.LavaDeath = true;
//        TileObjectData.addTile(Type);
//    }

//    public override void PostSetDefaults() => Main.tileNoSunLight[Type] = false;

//    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
//}

//public class SpruceTorchTile : ModTile
//{
//    private Asset<Texture2D> flameTexture;

//    public override void SetStaticDefaults()
//    {
//        Main.tileLighted[Type] = true;
//        Main.tileFrameImportant[Type] = true;
//        Main.tileSolid[Type] = false;
//        Main.tileNoAttach[Type] = true;
//        Main.tileNoFail[Type] = true;
//        Main.tileWaterDeath[Type] = true;
//        TileID.Sets.FramesOnKillWall[Type] = true;
//        TileID.Sets.DisableSmartCursor[Type] = true;
//        TileID.Sets.DisableSmartInteract[Type] = true;
//        TileID.Sets.Torch[Type] = true;

//        DustType = ModContent.DustType<SpruceDust>();
//        AdjTiles = [TileID.Torches];

//        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);

//        TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.Torches, 0));

//        TileObjectData.newSubTile.CopyFrom(TileObjectData.newTile);
//        TileObjectData.newSubTile.LinkedAlternates = true;
//        TileObjectData.newSubTile.WaterDeath = false;
//        TileObjectData.newSubTile.LavaDeath = false;
//        TileObjectData.newSubTile.WaterPlacement = LiquidPlacement.Allowed;
//        TileObjectData.newSubTile.LavaPlacement = LiquidPlacement.Allowed;
//        TileObjectData.addSubTile(1);

//        TileObjectData.addTile(Type);

//        AddMapEntry(new Color(200, 200, 200), Language.GetText("ItemName.Torch"));

//        flameTexture = ModContent.Request<Texture2D>(Texture + "_Flame");
//    }

//    public override void MouseOver(int i, int j)
//    {
//        var player = Main.LocalPlayer;
//        player.noThrow = 2;
//        player.cursorItemIconEnabled = true;

//        var style = TileObjectData.GetTileStyle(Main.tile[i, j]);
//        player.cursorItemIconID = TileLoader.GetItemDropFromTypeAndStyle(Type, style);
//    }

//    //public override float GetTorchLuck(Player player) {

//    //	var inExampleUndergroundBiome = player.InModBiome<ExampleUndergroundBiome>();
//    //	return inExampleUndergroundBiome ? 1f : -0.1f; 
//    //}

//    public override void NumDust(int i, int j, bool fail, ref int num) => num = Main.rand.Next(1, 3);

//    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
//    {
//        r = 0.8f;
//        g = 0.7f;
//        b = 0.55f;
//    }

//    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
//    {
//        offsetY = 0;

//        if (WorldGen.SolidTile(i, j - 1))
//        {
//            offsetY = 4;
//        }
//    }

//    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
//    {
//        var tile = Main.tile[i, j];

//        if (!TileDrawing.IsVisible(tile))
//        {
//            return;
//        }

//        var offsetY = 0;

//        if (WorldGen.SolidTile(i, j - 1))
//        {
//            offsetY = 4;
//        }

//        var zero = new Vector2(Main.offScreenRange, Main.offScreenRange);

//        if (Main.drawToScreen)
//        {
//            zero = Vector2.Zero;
//        }

//        var randSeed = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (uint)i);
//        var color = new Color(100, 100, 100, 0);
//        var width = 20;
//        var height = 20;
//        int frameX = tile.TileFrameX;
//        int frameY = tile.TileFrameY;
//        var style = TileObjectData.GetTileStyle(Main.tile[i, j]);

//        for (var k = 0; k < 7; k++)
//        {
//            var xx = Utils.RandomInt(ref randSeed, -10, 11) * 0.15f;
//            var yy = Utils.RandomInt(ref randSeed, -10, 1) * 0.35f;

//            spriteBatch.Draw(flameTexture.Value, new Vector2(i * 16 - (int)Main.screenPosition.X - (width - 16f) / 2f + xx, j * 16 - (int)Main.screenPosition.Y + offsetY + yy) + zero, new Rectangle(frameX, frameY, width, height), color, 0f, default, 1f, SpriteEffects.None, 0f);
//        }
//    }
//}

public class SpruceWorkbenchItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = 150;
        Item.DefaultToPlaceableTile(ModContent.TileType<SpruceWorkbenchTile>());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<SpruceWoodItem>(10)
            .Register();
    }
}

public class SpruceTableItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = 150;
        Item.DefaultToPlaceableTile(ModContent.TileType<SpruceTableTile>());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<SpruceWoodItem>(8)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}

public class SpruceSofaItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = 150;
        Item.DefaultToPlaceableTile(ModContent.TileType<SpruceSofaTile>());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<SpruceWoodItem>(5)
            .AddIngredient(ItemID.Silk, 2)
            .AddTile(TileID.Sawmill)
            .Register();
    }
}

public class SpruceBedItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = 150;
        Item.DefaultToPlaceableTile(ModContent.TileType<SpruceBedTile>());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<SpruceWoodItem>(15)
            .AddIngredient(ItemID.Silk, 5)
            .AddTile(TileID.Sawmill)
            .Register();
    }
}

public class SpruceSinkItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = 150;
        Item.DefaultToPlaceableTile(ModContent.TileType<SpruceSinkTile>());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<SpruceWoodItem>(6)
            .AddIngredient(ItemID.WaterBucket)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}

public class SpruceChairItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = 150;
        Item.DefaultToPlaceableTile(ModContent.TileType<SpruceChairTile>());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<SpruceWoodItem>(4)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}

public class SpruceDresserItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = 150;
        Item.DefaultToPlaceableTile(ModContent.TileType<SpruceDresserTile>());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<SpruceWoodItem>(16)
            .AddTile(TileID.Sawmill)
            .Register();
    }
}

//public class SpruceDoorItem : ModItem
//{
//    public override void SetDefaults()
//    {
//        base.SetDefaults();
//        Item.value = 150;
//        Item.DefaultToPlaceableTile(ModContent.TileType<SpruceDoorClosedTile>());
//    }
//    public override void AddRecipes()
//    {
//        CreateRecipe()
//            .AddIngredient<SpruceWoodItem>(6)
//            .AddTile(TileID.WorkBenches)
//            .Register();
//    }
//}

//public class SpruceCandleItem : ModItem
//{
//    public override void SetDefaults()
//    {
//        base.SetDefaults();
//        Item.value = 150;
//        //Item.DefaultToPlaceableTile(ModContent.TileType<SpruceTableTile>());
//    }
//    public override void AddRecipes()
//    {
//        CreateRecipe()
//            .AddIngredient<SpruceWoodItem>(4)
//            .AddIngredient(ItemID.Torch)
//            .AddTile(TileID.WorkBenches)
//            .Register();
//    }
//}

public class SpruceCandelabraItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = Item.sellPrice(silver: 3);
        Item.DefaultToPlaceableTile(ModContent.TileType<SpruceCandelabraTile>());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<SpruceWoodItem>(5)
            .AddIngredient(ItemID.Torch, 5)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}

public class SpruceBookcaseItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = 150;
        Item.DefaultToPlaceableTile(ModContent.TileType<SpruceBookcaseTile>());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<SpruceWoodItem>(20)
            .AddIngredient(ItemID.Book, 10)
            .AddTile(TileID.Sawmill)
            .Register();
    }
}

public class SpruceBathtubItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = 150;
        Item.DefaultToPlaceableTile(ModContent.TileType<SpruceBathtubTile>());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<SpruceWoodItem>(14)
            .AddTile(TileID.Sawmill)
            .Register();
    }
}

public class SprucePianoItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = 150;
        Item.DefaultToPlaceableTile(ModContent.TileType<SprucePianoTile>());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Bone, 4)
            .AddIngredient<SpruceWoodItem>(15)
            .AddIngredient(ItemID.Book)
            .AddTile(TileID.Sawmill)
            .Register();
    }
}

//public class SpruceClockItem : ModItem
//{
//    public override void SetDefaults()
//    {
//        base.SetDefaults();
//        Item.value = 150;
//        Item.DefaultToPlaceableTile(ModContent.TileType<SpruceClockTile>());
//    }
//    public override void AddRecipes()
//    {
//        CreateRecipe()
//            .AddIngredient<SpruceWoodItem>(10)
//            .AddIngredient(ItemID.Glass, 6)
//            .AddRecipeGroup(RecipeGroupID.IronBar, 3)
//            .AddTile(TileID.Sawmill)
//            .Register();
//    }
//}

//public class SpruceChestItem : ModItem
//{
//    public override void SetDefaults()
//    {
//        base.SetDefaults();
//        Item.value = Item.sellPrice(silver: 1);
//        Item.DefaultToPlaceableTile(ModContent.TileType<SpruceChestTile>());
//    }
//    public override void AddRecipes()
//    {
//        CreateRecipe()
//            .AddIngredient<SpruceWoodItem>(8)
//            .AddRecipeGroup(RecipeGroupID.IronBar, 2)
//            .AddTile(TileID.WorkBenches)
//            .Register();
//    }
//}

//public class SprucePlatformItem : ModItem
//{
//    public override void SetDefaults()
//    {
//        Item.DefaultToPlaceableTile(ModContent.TileType<SprucePlatformTile>());
//        Item.width = 8;
//        Item.height = 10;
//    }

//    public override void AddRecipes()
//    {
//        CreateRecipe(2)
//        .AddIngredient<SpruceWoodItem>()
//        .Register();
//    }
//}

//public class SpruceCampfireItem : ModItem
//{
//    public override void SetDefaults()
//    {
//        Item.DefaultToPlaceableTile(ModContent.TileType<SpruceCampfireTile>(), 0);
//    }

//    public override void AddRecipes()
//    {
//        CreateRecipe()
//            .AddRecipeGroup(RecipeGroupID.Wood, 10)
//            .AddIngredient<SpruceTorchItem>(5)
//            .Register();

//        CreateRecipe()
//            .AddIngredient<SpruceWoodItem>(10)
//            .AddIngredient<SpruceTorchItem>(5)
//            .Register();
//    }
//}

//public class SpruceTorchItem : ModItem
//{
//    public override void SetStaticDefaults()
//    {
//        Item.ResearchUnlockCount = 100;

//        ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.ShimmerTorch;
//        ItemID.Sets.SingleUseInGamepad[Type] = true;
//        ItemID.Sets.Torches[Type] = true;
//    }

//    public override void SetDefaults()
//    {
//        Item.DefaultToTorch(ModContent.TileType<SpruceTorchTile>(), 1, true);
//        Item.value = 50;
//    }

//    public override void HoldItem(Player player)
//    {
//        if (Main.rand.NextBool(player.itemAnimation > 0 ? 7 : 30))
//        {
//            var dust = Dust.NewDustDirect(new Vector2(player.itemLocation.X + (player.direction == -1 ? -16f : 6f), player.itemLocation.Y - 14f * player.gravDir), 4, 4, DustID.OrangeTorch, 0f, 0f, 100);
//            if (!Main.rand.NextBool(3))
//            {
//                dust.noGravity = true;
//            }

//            dust.velocity *= 0.3f;
//            dust.velocity.Y -= 1.5f;
//            dust.position = player.RotatedRelativePoint(dust.position);
//        }

//        var position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 12f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);

//        Lighting.AddLight(position, 0.8f, 0.7f, 0.55f);
//    }

//    public override void PostUpdate()
//    {
//        Lighting.AddLight(Item.Center, 0.8f, 0.7f, 0.55f);
//    }

//    public override void AddRecipes()
//    {
//        CreateRecipe(3)
//            .AddIngredient<SpruceWoodItem>()
//            .AddIngredient(ItemID.Gel)
//            .Register();
//    }
//}