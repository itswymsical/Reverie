﻿using Reverie.Core.Dialogue;
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
          [("Gather wood", 30), ("Gather stone", 10), ("Build a shelter", 1)],
          [("Harvest ore", 30),("Smelt bars", 15), ("Discover accessories", 2)],
          [("Obtain a helmet", 1), ("Obtain a chestplate", 1), ("Obtain leggings", 1), ("Obtain a greater Pickaxe", 1)],
          [("Clear slimes", 6)],
          [("Explore the Underground", 1), ("Loot items", 20)],
          [("Clear out slimes, again...", 12)],
          [("Return to looting", 100)],
          [("Return to Guide", 1)],
          [("Clear Slime Infestation", 50)],
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
    private readonly List<Item> starterItems =
    [
        new Item(ItemID.WoodenSword),
        new Item(ItemID.CopperPickaxe),
        new Item(ItemID.CopperAxe)
    ];

    internal enum Objectives
    {
        TalkToGuide = 0,
        GatherResources = 1,
        ExploreAndGather = 2,
        ObtainEquipment = 3,
        ClearInitialSlimes = 4,
        ExploreUnderground = 5,
        ClearSecondSlimes = 6,
        ContinueLooting = 7,
        ReturnToGuide = 8,
        ClearInfestation = 9,
        DefeatKingSlime = 10
    }

    public override void WhileActive()
    {
        base.WhileActive();

        if (CurObjectiveIndex < (int)Objectives.ClearInfestation)
        {
            Main.slimeRain = false;
            Main.slimeRainTime = 0;
            Main.dayTime = true;
            Main.time = 18000;
        }

        if (CurObjectiveIndex == (int)Objectives.ClearInfestation)
        {
            if (!Main.slimeRain)
            {
                Main.StartSlimeRain();
            }
        }
        Main.bloodMoon = false;
    }

    #region Completion Handlers
    protected override void HandleObjectiveSetComplete(int setIndex, ObjectiveSet set)
    {
        try
        {
            var objective = (Objectives)setIndex;
            if (!set.Objectives.All(o => o.IsCompleted)) return;

            switch (objective)
            {
                case Objectives.GatherResources:
                    DialogueManager.Instance.StartDialogueByKey(
                    NPCDataManager.GuideData,
                    DialogueKeys.CrashLanding.GiveGuideResources,
                    lineCount: 5,
                    zoomIn: true);
                    break;
                case Objectives.ObtainEquipment:
                    DialogueManager.Instance.StartDialogueByKey(
                    NPCDataManager.GuideData,
                    DialogueKeys.CrashLanding.WildlifeWoes,
                    lineCount: 3,
                    zoomIn: true);
                    break;
                case Objectives.ClearInitialSlimes:
                    DialogueManager.Instance.StartDialogueByKey(
                    NPCDataManager.GuideData,
                    DialogueKeys.CrashLanding.SlimeInfestation,
                    lineCount: 2,
                    zoomIn: true);
                    break;
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in HandleObjectiveSetComplete: {ex.Message}");
        }
    }

    protected override void HandleObjectiveComplete(int objectiveIndex)
    {
        try
        {
            var objective = (Objectives)CurObjectiveIndex;
            switch (objective)
            {
                case Objectives.TalkToGuide:
                    GiveStarterItems();
                    break;
                case Objectives.ClearInitialSlimes:
                    DialogueManager.Instance.StartDialogueByKey(
                    NPCDataManager.GuideData,
                    DialogueKeys.CrashLanding.SlimeInfestation,
                    lineCount: 2,
                    zoomIn: true);
                    break;
                case Objectives.ExploreUnderground:
                    DialogueManager.Instance.StartDialogueByKey(
                    NPCDataManager.GuideData,
                    DialogueKeys.CrashLanding.SlimeInfestationCommentary,
                    lineCount: 2,
                    zoomIn: true);
                    break;
                case Objectives.ContinueLooting:
                    StartSlimeRainEvent();
                    break;
                case Objectives.ClearInfestation:
                    StartKingSlimeEncounter();
                    break;
                case Objectives.DefeatKingSlime:
                    DialogueManager.Instance.StartDialogueByKey(
                    NPCDataManager.GuideData,
                    DialogueKeys.CrashLanding.KSVictory,
                    lineCount: 4,
                    zoomIn: true);
                    break;
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in HandleObjectiveComplete: {ex.Message}");
        }
    }
    #endregion

    #region Event Handlers
    public override void OnItemObtained(Item item)
    {
        try
        {
            var objective = (Objectives)CurObjectiveIndex;
            switch (objective)
            {
                case Objectives.GatherResources:
                    HandleResourceGathering(item);
                    break;
                case Objectives.ObtainEquipment:
                    HandleEquipmentCollection(item);
                    break;
                case Objectives.ExploreAndGather:
                    HandleExplorationGathering(item);
                    break;
                case Objectives.ExploreUnderground:
                    HandleUndergroundLoot(item);
                    break;
                case Objectives.ContinueLooting:
                    HandleContinuedLooting(item);
                    break;
            }
        }
        catch (Exception ex)
        {
            Instance.Logger.Error($"Error in OnItemPickup: {ex.Message}");
        }
    }

    public override void OnNPCKill(NPC npc)
    {
        try
        {
            var objective = (Objectives)CurObjectiveIndex;
            switch (objective)
            {
                case Objectives.ClearInitialSlimes:
                case Objectives.ClearSecondSlimes:
                    if (npc.type == NPCAIStyleID.Slime)
                        UpdateProgress(0);
                    break;
                case Objectives.ClearInfestation:
                    HandleSlimeInfestation(npc);
                    break;
                case Objectives.DefeatKingSlime:
                    if (npc.type == NPCID.KingSlime)
                        UpdateProgress(0);
                    break;
            }
        }
        catch (Exception ex)
        {
            Instance.Logger.Error($"Error in OnNPCKill: {ex.Message}");
        }
    }

    public override void OnBiomeEnter(Player player, BiomeType biome)
    {
        try
        {
            var objective = (Objectives)CurObjectiveIndex;
            switch (objective)
            {
                case Objectives.ExploreUnderground:
                    if (biome == BiomeType.Underground)
                        UpdateProgress(0);
                    break;
                case Objectives.ReturnToGuide:
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


    public override void OnNPCChat(NPC npc)
    {
        try
        {
            var objective = (Objectives)CurObjectiveIndex;
            switch (objective)
            {
                case Objectives.TalkToGuide:
                    DialogueManager.Instance.StartDialogueByKey(
                    NPCDataManager.GuideData,
                    DialogueKeys.CrashLanding.GatheringResources,
                    lineCount: 2,
                    zoomIn: true);
                    UpdateProgress(0);
                    break;
            }
        }
        catch (Exception ex)
        {
            Instance.Logger.Error($"Error in OnNPCChat: {ex.Message}");
        }
    }

    public override void OnValidHousingFound()
    {
        try
        {
            var objective = (Objectives)CurObjectiveIndex;
            switch (objective)
            {
                case Objectives.GatherResources:
                    DialogueManager.Instance.StartDialogueByKey(
                    NPCDataManager.GuideData,
                    DialogueKeys.CrashLanding.GiveGuideResources,
                    lineCount: 5,
                    zoomIn: true);
                    UpdateProgress(2);
                    break;
            }
        }
        catch (Exception ex)
        {
            Instance.Logger.Error($"Error in OnValidHousing: {ex.Message}");
        }
    }
    #endregion

    #region Helper Methods
    private void GiveStarterItems()
    {
        foreach (var item in starterItems)
        {
            player.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), item.type, item.stack);
        }
    }

    private void StartSlimeRainEvent()
    {
        Main.StartSlimeRain(true);
        DialogueManager.Instance.StartDialogueByKey(
        NPCDataManager.GuideData,
        DialogueKeys.CrashLanding.SlimeRain,
        lineCount: 2,
        zoomIn: true);
    }

    private void StartKingSlimeEncounter()
    {
        DialogueManager.Instance.StartDialogueByKey(
        NPCDataManager.GuideData,
        DialogueKeys.CrashLanding.KSEncounter,
        lineCount: 3,
        zoomIn: true);
        SpawnKingSlime();
    }

    private void SpawnKingSlime()
    {
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

    private void HandleResourceGathering(Item item)
    {
        if (item.type == ItemID.StoneBlock)
            UpdateProgress(1, item.stack);
        if (item.type == ItemID.Wood)
            UpdateProgress(0, item.stack);
    }

    private void HandleEquipmentCollection(Item item)
    {
        if (item.headSlot != -1 && !item.vanity)
            UpdateProgress(0, item.stack);
        if (item.bodySlot != -1 && !item.vanity)
            UpdateProgress(1, item.stack);
        if (item.legSlot != -1 && !item.vanity)
            UpdateProgress(2, item.stack);
        if (item.IsMiningTool())
            UpdateProgress(3);
    }

    private void HandleExplorationGathering(Item item)
    {
        if (item.IsOre())
            UpdateProgress(0, item.stack);

        if (item.Name.EndsWith("Bar"))
            UpdateProgress(1, item.stack);

        if (item.accessory)
            UpdateProgress(2, item.stack);
    }

    private void HandleUndergroundLoot(Item item)
    {
        if (IsValuableLoot(item))
        {
            UpdateProgress(1, item.stack);
            if (ObjectiveIndex[CurObjectiveIndex].Objectives[1].CurrentCount == LOOT_NOTIFICATION_THRESHOLD)
            {
                DialogueManager.Instance.StartDialogueByKey(
                NPCDataManager.GuideData,
                DialogueKeys.CrashLanding.SlimeInfestation,
                lineCount: 2,
                zoomIn: true);
            }
        }
    }

    private void HandleContinuedLooting(Item item)
    {
        if (IsValuableLoot(item))
        {
            UpdateProgress(0, item.stack);
        }
    }

    private bool IsValuableLoot(Item item)
    {
        return (item.rare >= ItemRarityID.Blue
            || item.accessory
            || item.damage > 0 || item.pick > 0 || item.axe > 0 || item.hammer > 0
            || item.value >= Item.buyPrice(silver: 1))
            || item.type != ItemID.CopperCoin;
    }

    private void HandleSlimeInfestation(NPC npc)
    {
        if (npc.type == NPCAIStyleID.Slime)
        {
            UpdateProgress(0);
            if (ObjectiveIndex[CurObjectiveIndex].Objectives[0].CurrentCount == SLIME_COMMENTARY_THRESHOLD)
            {
                DialogueManager.Instance.StartDialogueByKey(
                NPCDataManager.GuideData,
                DialogueKeys.CrashLanding.SlimeRainCommentary,
                lineCount: 2,
                zoomIn: true);
            }
        }
    }
    #endregion
}