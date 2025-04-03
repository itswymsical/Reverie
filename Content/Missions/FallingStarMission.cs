using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Reverie.Utilities;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;

namespace Reverie.Content.Missions;

public class FallingStarMission : Mission
{
    public FallingStarMission() : base(MissionID.A_FALLING_STAR,
      "Falling Star...",
      "'Well, that's one way to make an appearance...'" +
      "\nBegin your journey in Terraria, discovering knowledge and power...",
      [
          [("Talk to Guide", 1)],
          [("Gather wood", 50), ("Build a shelter", 1)],
          [("Harvest ore", 30),("Craft a pickaxe", 1), ("Discover accessories", 2)],
          [("Obtain a helmet", 1), ("Obtain a chestplate", 1), ("Obtain leggings", 1), ("Obtain a new weapon", 1)],
          [("Explore the Underground", 1), ("Discover a Glowing Mushroom Biome", 1), ("Kill enemies", 15)],
          [("Clear out slimes", 10)],
          [("Clear out slimes, again...", 10)],
          [("Defend the Forest", 1)],
          [("Clear Slime Infestation", 80)],
          [("Defeat the King Slime", 1)]
      ],

      [new Item(ItemID.MagicMirror), new Item(ItemID.GoldCoin, Main.rand.Next(3, 4))],
      isMainline: true,
      NPCID.Guide,
      xpReward: 80)
    {
        ModContent.GetInstance<Reverie>().Logger.Info("[A Falling Star] Mission constructed");
    }

    private const int LOOT_NOTIFICATION_THRESHOLD = 10;
    private const int SLIME_COMMENTARY_THRESHOLD = 20;
    private const int SLIME_WARNING_THRESHOLD = 45;

    private readonly List<Item> starterItems =
    [
        new Item(ItemID.WoodenSword),
        new Item(ItemID.CopperPickaxe),
        new Item(ItemID.CopperAxe)
    ];

    internal enum Objectives
    {
        TalkToGuide = 0,
        WoodShelter = 1,
        AquireMetal = 2,
        SuitUp = 3,
        Explore = 4,
        ClearSlimes = 5,
        ClearSlimes2 = 6,
        DefendForest = 7,
        ClearSlimeRain = 8,
        DefeatKingSlime = 9
    }

    public override void Update()
    {
        base.Update();

        if (CurrentIndex < (int)Objectives.ClearSlimeRain)
        {
            Main.slimeRain = false;
            Main.slimeRainTime = 0;
            Main.dayTime = true;
            Main.time = 18000;
        }

        if (CurrentIndex == (int)Objectives.ClearSlimeRain)
        {
            if (!Main.slimeRain)
            {
                Main.StartSlimeRain();
            }
        }
        Main.bloodMoon = false;
    }

    protected override void OnIndexComplete(int setIndex, ObjectiveSet set)
    {
        try
        {
            var objective = (Objectives)setIndex;
            if (!set.Objectives.All(o => o.IsCompleted)) return;

            switch (objective)
            {
                case Objectives.SuitUp: //get armor and weapon
                    DialogueManager.Instance.StartDialogueByKey(
                    NPCDataManager.GuideData,
                    DialogueKeys.FallingStar.WildlifeWoes,
                    lineCount: 3,
                    zoomIn: true);
                    break;
                //case Objectives.WoodShelter: //chop trees , build shelter
                //    DialogueManager.Instance.StartDialogueByKey(
                //    NPCDataManager.GuideData,
                //    DialogueKeys.FallingStar.BuildShelter,
                //    lineCount: 5,
                //    zoomIn: true);
                //    break;
                case Objectives.ClearSlimes:
                    DialogueManager.Instance.StartDialogueByKey(
                    NPCDataManager.GuideData,
                    DialogueKeys.FallingStar.SlimeInfestation,
                    lineCount: 2,
                    zoomIn: true);
                    break;
                case Objectives.Explore: //post-exploring slimes attack the guide
                    DialogueManager.Instance.StartDialogueByKey(
                    NPCDataManager.GuideData,
                    DialogueKeys.FallingStar.SlimeInfestationCommentary,
                    lineCount: 2,
                    zoomIn: true);
                    break;
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnIndexComplete: {ex.Message}");
        }
    }

    protected override void OnObjectiveComplete(int objectiveIndex)
    {
        try
        {
            var objective = (Objectives)CurrentIndex;
            switch (objective)
            {
                case Objectives.TalkToGuide:
                    GiveStarterItems();
                    break;
                case Objectives.ClearSlimes:
                    DialogueManager.Instance.StartDialogueByKey(
                    NPCDataManager.GuideData,
                    DialogueKeys.FallingStar.SlimeInfestation,
                    lineCount: 2,
                    zoomIn: true);
                    break;
                case Objectives.ClearSlimes2:
                    StartSlimeRain();
                    break;
                case Objectives.ClearSlimeRain:
                    SpawnKingSlime();
                    break;
                case Objectives.DefeatKingSlime:
                    DialogueManager.Instance.StartDialogueByKey(
                    NPCDataManager.GuideData,
                    DialogueKeys.FallingStar.KingSlimeDefeat,
                    lineCount: 4,
                    zoomIn: true);
                    break;
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnObjectiveComplete: {ex.Message}");
        }
    }

    public override void OnMissionComplete(bool giveRewards = true)
    {
        base.OnMissionComplete(giveRewards);
        //MissionPlayer player = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
        //player.StartNextMission(...);
    }

    #region Event Handlers
    public override void OnChat(NPC npc)
    {
        try
        {
            var objective = (Objectives)CurrentIndex;
            switch (objective)
            {
                case Objectives.TalkToGuide:
                    DialogueManager.Instance.StartDialogueByKey(
                    NPCDataManager.GuideData,
                    DialogueKeys.FallingStar.GatheringResources,
                    lineCount: 7,
                    zoomIn: true,
                    modifications:
                    [(line: 1, delay: 2, emote: 0),
                    (line: 2, delay: 2, emote: 3),
                    (line: 3, delay: 3, emote: 1),
                    (line: 4, delay: 3, emote: 0),
                    (line: 5, delay: 5, emote: 2),
                    (line: 6, delay: 2, emote: 0),
                    (line: 7, delay: 2, emote: 0)]);

                    UpdateProgress(0);
                    break;
            }
        }
        catch (Exception ex)
        {
            Instance.Logger.Error($"Error in OnChat: {ex.Message}");
        }
    }

    public override void OnCollected(Item item)
    {
        try
        {
            var objective = (Objectives)CurrentIndex;
            switch (objective)
            {
                case Objectives.WoodShelter:
                    if (item.type == ItemID.Wood)
                        UpdateProgress(0, item.stack);
                    break;
                case Objectives.SuitUp:
                    if (item.headSlot != -1 && !item.vanity)
                        UpdateProgress(0, item.stack);
                    if (item.bodySlot != -1 && !item.vanity)
                        UpdateProgress(1, item.stack);
                    if (item.legSlot != -1 && !item.vanity)
                        UpdateProgress(2, item.stack);
                    if (item.DealsDamage())
                        UpdateProgress(3);
                    break;
                case Objectives.AquireMetal:
                    if (item.IsOre())
                        UpdateProgress(0, item.stack);
                    if (item.IsMiningTool())
                        UpdateProgress(1, item.stack);
                    if (item.accessory)
                        UpdateProgress(2, item.stack);
                    break;
            }
        }
        catch (Exception ex)
        {
            Instance.Logger.Error($"Error in OnItemPickup: {ex.Message}");
        }
    }

    public override void OnKill(NPC npc)
    {
        try
        {
            var objective = (Objectives)CurrentIndex;
            switch (objective)
            {
                case Objectives.ClearSlimes:
                case Objectives.ClearSlimes2:
                    if (npc.type == NPCAIStyleID.Slime)
                        UpdateProgress(0);
                    break;
                case Objectives.Explore:
                    if (!player.ZoneOverworldHeight || !player.ZoneSkyHeight)
                        if (!npc.CountsAsACritter || npc.type != NPCAIStyleID.Slime)
                            UpdateProgress(2);
                    break;
                case Objectives.ClearSlimeRain:
                    HandleSlimeRain(npc);
                    break;
                case Objectives.DefeatKingSlime:
                    if (npc.type == NPCID.KingSlime)
                        UpdateProgress(0);
                    break;
            }
        }
        catch (Exception ex)
        {
            Instance.Logger.Error($"Error in OnKill: {ex.Message}");
        }
    }

    public override void OnHousingFound()
    {
        try
        {
            var objective = (Objectives)CurrentIndex;
            switch (objective)
            {
                case Objectives.WoodShelter:
                    UpdateProgress(1);
                    break;
            }
        }
        catch (Exception ex)
        {
            Instance.Logger.Error($"Error in OnValidHousing: {ex.Message}");
        }
    }

    public override void OnBiomeEnter(Player player, BiomeType biome)
    {
        try
        {
            var objective = (Objectives)CurrentIndex;
            switch (objective)
            {
                case Objectives.Explore:
                    if (biome == BiomeType.Underground)
                        UpdateProgress(0);
                    if (biome == BiomeType.Glowshroom)
                        UpdateProgress(1);
                    break;

                case Objectives.DefendForest:
                    if (biome == BiomeType.Forest)
                        UpdateProgress(0);
                    break;
            }
        }
        catch (Exception ex)
        {
            Instance.Logger.Error($"Error in OnBiomeEnter: {ex.Message}");
        }
    }
    #endregion

    #region Helper Methods
    private void GiveStarterItems()
    {
        foreach (var item in starterItems)
        {
            Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), item.type, item.stack);
        }
    }

    private void StartSlimeRain()
    {
        Main.StartSlimeRain(true);
        DialogueManager.Instance.StartDialogueByKey(
        NPCDataManager.GuideData,
        DialogueKeys.FallingStar.SlimeRain,
        lineCount: 2,
        zoomIn: false);
    }

    private void HandleSlimeRain(NPC npc)
    {
        if (npc.type == NPCAIStyleID.Slime)
        {
            UpdateProgress(0);
            if (ObjectiveIndex[CurrentIndex].Objectives[0].CurrentCount == SLIME_COMMENTARY_THRESHOLD)
            {
                DialogueManager.Instance.StartDialogueByKey(
                NPCDataManager.GuideData,
                DialogueKeys.FallingStar.SlimeRainCommentary,
                lineCount: 2,
                zoomIn: false);
            }
            if (ObjectiveIndex[CurrentIndex].Objectives[0].CurrentCount == SLIME_WARNING_THRESHOLD)
            {
                DialogueManager.Instance.StartDialogueByKey(
                NPCDataManager.GuideData,
                DialogueKeys.FallingStar.SlimeRainWarning,
                lineCount: 2,
                zoomIn: false);
            }
        }
    }

    private void SpawnKingSlime()
    {
        DialogueManager.Instance.StartDialogueByKey(
        NPCDataManager.GuideData,
        DialogueKeys.FallingStar.KSEncounter,
        lineCount: 3,
        zoomIn: false);

        if (!NPC.AnyNPCs(NPCID.KingSlime) && Main.LocalPlayer.whoAmI == Main.myPlayer)
        {
            SoundEngine.PlaySound(SoundID.Roar, Main.LocalPlayer.position);
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.SpawnOnPlayer(Main.LocalPlayer.whoAmI, NPCID.KingSlime);
            }
            else
            {
                NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent,
                    number: Main.LocalPlayer.whoAmI,
                    number2: NPCID.KingSlime);
            }
        }
    }

    #endregion
}