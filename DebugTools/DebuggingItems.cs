using Reverie.Common.UI.Missions;
using Reverie.Content.Cutscenes;
using Reverie.Core.Cinematics;
using Reverie.Core.Dialogue;
using Reverie.Core.Indicators;
using Reverie.Core.Missions;
using Terraria.UI;
using Terraria.WorldBuilding;
using static Terraria.WorldBuilding.Shapes;

namespace Reverie.DebugItems;

public class MissionCompleteIndicator : ModItem
{
    public override string Texture => PLACEHOLDER;
    public override void SetDefaults()
    {
        Item.useTime = Item.useAnimation = 20;
        Item.value = Item.buyPrice(0);
        Item.rare = ItemRarityID.Quest;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }
    public override bool? UseItem(Player player)
    {
        if (Main.myPlayer == player.whoAmI)
        {
            var mplayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
            var mission = mplayer.GetMission(MissionID.SporeSplinter);
            InGameNotificationsTracker.AddNotification(new MissionCompleteNotification(mission));
        }
        return true;
    }
}

public class CutscenePlayer : ModItem
{
    public override string Texture => PLACEHOLDER;
    public override void SetDefaults()
    {
        Item.useTime = Item.useAnimation = 20;
        Item.value = Item.buyPrice(0);
        Item.rare = ItemRarityID.Quest;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }
    public override bool? UseItem(Player player)
    {
        if (Main.myPlayer == player.whoAmI)
            CutsceneSystem.PlayCutscene<OpeningCutscene>();

        return true;
    }
}

public class SpawnMissionIndicator : ModItem
{
    public override string Texture => PLACEHOLDER;
    public override void SetDefaults()
    {
        Item.useTime = Item.useAnimation = 20;
        Item.value = Item.buyPrice(0);
        Item.rare = ItemRarityID.Quest;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }
    public override bool? UseItem(Player player)
    {
        var mplayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
        var mission = mplayer.GetMission(MissionID.SporeSplinter);

        if (player.altFunctionUse != 0)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                ScreenIndicatorManager.Instance.CreateMissionIndicator(Main.MouseWorld, mission);
                Main.NewText($"Placed at position [X:{Main.MouseWorld.X} Y:{Main.MouseWorld.Y}]");
            }
           
        }
        else
        {
            Main.NewText($"[Cleared All Indicators] | Right-click to place an Indicator.");
            ScreenIndicatorManager.Instance.ClearAllIndicators();
        }

        return true;
    }
    public override bool AltFunctionUse(Player player)
    {
        return true;
    }
}

public class SpawnDialogueIndicator : ModItem
{
    public override string Texture => PLACEHOLDER;
    public override void SetDefaults()
    {
        Item.useTime = Item.useAnimation = 20;
        Item.value = Item.buyPrice(0);
        Item.rare = ItemRarityID.Quest;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }
    public override bool? UseItem(Player player)
    {
        if (player.altFunctionUse != 0)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                ScreenIndicatorManager.Instance.CreateDialogueIndicator(Main.MouseWorld,
                    "Argie.Intro", 9);

                Main.NewText($"Placed at position [X:{Main.MouseWorld.X} Y:{Main.MouseWorld.Y}]");
            }

        }
        else
        {
            DialogueManager.Instance.StartDialogue("Argie.Intro", 9, letterbox: true,
                music: MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}ArgiesTheme"));

            Main.NewText($"[Cleared All Indicators] | Right-click to place an Indicator.");
            ScreenIndicatorManager.Instance.ClearAllIndicators();
        }

        return true;
    }
    public override bool AltFunctionUse(Player player)
    {
        return true;
    }
}

public class GeodePlacer : ModItem
{
    public override string Texture => PLACEHOLDER;

    public override void SetDefaults()
    {
        Item.useTime = Item.useAnimation = 20;
        Item.value = Item.buyPrice(0);
        Item.rare = ItemRarityID.Purple;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }

    public override bool? UseItem(Player player)
    {
        if (Main.myPlayer == player.whoAmI)
        {
            Point tilePos = (Main.MouseWorld / 16).ToPoint();

            new Circle(8).Perform(tilePos, new Actions.SetTile(TileID.Stone));
            new Circle(6).Perform(tilePos, new Actions.ClearTile());

            new Circle(6).Perform(tilePos, new Actions.SetTile(TileID.Amethyst));
            new Circle(5).Perform(tilePos, new Actions.SetTile(TileID.ExposedGems));
            new Circle(4).Perform(tilePos, new Actions.ClearTile());

            new Circle(8).Perform(tilePos, new Actions.PlaceWall(WallID.AmethystUnsafe));

            Main.NewText($"Geode placed at [{tilePos.X}, {tilePos.Y}]");
        }
        return true;
    }
}