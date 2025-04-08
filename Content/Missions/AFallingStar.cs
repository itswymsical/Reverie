using Reverie.Common.Systems;
using Reverie.Core.Cinematics.Cutscenes;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Reverie.Utilities;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;

namespace Reverie.Content.Missions;

public class AFallingStar : Mission
{
    public AFallingStar() : base(MissionID.AFallingStar,
      "Falling Star...",
      "'Well, that's one way to make an appearance...'" +
      "\nBegin your journey in Terraria, discovering knowledge and power...",
      [
        [("Talk to Guide", 1), ("Talk to Merchant", 1), ("Talk to Demolitionist", 1), ("Talk to Nurse", 1)],
        [("Gather Wood", 50), ("Break Pots", 20)],
        [("Harvest Ore", 30), ("Discover accessories", 2)],
        [("Explore the Underground", 1), 
            ("Discover a Glowing Mushroom Biome", 1), ("Explore the Jungle", 1), 
            ("Explore the Underground Desert", 1),  ("Explore the Tundra", 1)],
        [("Check in with the Guide", 1)],
        [("Clear out slimes", 10)],
        [("Defend the Town", 10)],
        [("Clear slime infestation", 100)],
        [("Defeat King Slime", 1)]
      ],

      [new Item(ItemID.RegenerationPotion), 
          new Item(ItemID.IronskinPotion), 
          new Item(ItemID.MagicMirror), 
          new Item(ItemID.GoldCoin, Main.rand.Next(4, 6))],
      isMainline: true,
      NPCID.Guide,
      xpReward: 100)
    {
        ModContent.GetInstance<Reverie>().Logger.Info("[A Falling Star] Mission constructed");
    }


    private readonly List<Item> starterItems =
    [
        new Item(ItemID.CopperShortsword),
        new Item(ItemID.CopperPickaxe),
        new Item(ItemID.CopperAxe)
    ];

    internal enum Objectives
    {
        TalkToTownies = 0,
        GatherResources = 1,
        AquireItems = 2,
        ExploreBiomes = 3,
        CheckIn = 4,
        ClearSlimes = 5,
        DefendTown = 6,
        ClearSlimeRain = 7,
        DefeatKingSlime = 8
    }

    public override void OnMissionStart()
    {
        base.OnMissionStart();
        CutsceneSystem.PlayCutscene(new FallingStarCutscene());
    }

    public override void Update()
    {
        base.Update();

        if (CurrentIndex < (int)Objectives.ClearSlimeRain)
        {
            Main.slimeRain = false;
            Main.slimeRainTime = 0;
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
                case Objectives.CheckIn:
                    DialogueManager.Instance.StartDialogueByKey(
                    NPCManager.GuideData,
                    DialogueKeys.FallingStar.SlimeInfestation,
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
                case Objectives.CheckIn:
                    DialogueManager.Instance.StartDialogueByKey(
                    NPCManager.GuideData,
                    DialogueKeys.FallingStar.SlimeInfestation,
                    lineCount: 2,
                    zoomIn: true);
                    break;
                case Objectives.ClearSlimes:
                    StartSlimeRain();
                    break;
                case Objectives.ClearSlimeRain:
                    SpawnKingSlime();
                    break;
                case Objectives.DefeatKingSlime:
                    DialogueManager.Instance.StartDialogueByKey(
                    NPCManager.GuideData,
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
    protected override void HandleChat(NPC npc)
    {
        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.TalkToTownies:
                if (npc.type == NPCID.Guide)
                {
                    DialogueManager.Instance.StartDialogueByKey(
                        NPCManager.GuideData,
                        DialogueKeys.FallingStar.GatheringResources,
                        lineCount: 6,
                        zoomIn: true,
                        modifications:
                        [(line: 1, delay: 2, emote: 3),
                        (line: 2, delay: 2, emote: 0),
                        (line: 3, delay: 3, emote: 2),
                        (line: 4, delay: 3, emote: 2),
                        (line: 5, delay: 5, emote: 0),
                        (line: 6, delay: 2, emote: 0)]);
                    UpdateProgress(0);
                }
                if (npc.type == NPCID.Merchant)
                {
                    DialogueManager.Instance.StartDialogueByKey(
                        NPCManager.MerchantData,
                        DialogueKeys.FallingStar.MerchantIntro,
                        lineCount: 5,
                        zoomIn: false,
                        modifications:
                       [(line: 1, delay: 3, emote: 0),
                            (line: 2, delay: 3, emote: 1),
                            (line: 3, delay: 3, emote: 0),
                            (line: 4, delay: 3, emote: 0),
                            (line: 5, delay: 3, emote: 1)]);

                    UpdateProgress(1);
                    GiveStarterItems();
                }
                if (npc.type == NPCID.Demolitionist)
                {
                    DialogueManager.Instance.StartDialogueByKey(
                        NPCManager.DemolitionistData,
                        DialogueKeys.FallingStar.DemolitionistIntro,
                        lineCount: 4,
                        zoomIn: false,
                        modifications:
                        [(line: 1, delay: 3, emote: 0),
                            (line: 2, delay: 3, emote: 0),
                            (line: 3, delay: 3, emote: 0),
                            (line: 4, delay: 3, emote: 0)]
                        );

                    UpdateProgress(2);
                }
                if (npc.type == NPCID.Nurse)
                {
                    DialogueManager.Instance.StartDialogueByKey(
                        NPCManager.NurseData,
                        DialogueKeys.FallingStar.NurseIntro,
                        lineCount: 4,
                        zoomIn: false,
                        modifications:
                        [(line: 1, delay: 3, emote: 0),
                            (line: 2, delay: 3, emote: 0),
                            (line: 3, delay: 3, emote: 0),
                            (line: 4, delay: 3, emote: 0)]
                        );

                    UpdateProgress(3);
                }
                break;
            case Objectives.CheckIn:
                if (npc.type == NPCID.Guide)
                {
                    UpdateProgress(0);
                }
                break;
        }
    }


    protected override void HandleCollected(Item item)
    {

        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.GatherResources:
                if (item.type == ItemID.Wood)
                    UpdateProgress(0, item.stack);
                break;
            case Objectives.AquireItems:
                if (item.IsOre())
                    UpdateProgress(0, item.stack);
                if (item.accessory)
                    UpdateProgress(1, item.stack);
                break;
        }
    }

    protected override void HandleKill(NPC npc)
    {

        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.ClearSlimes:
                if (npc.type == NPCAIStyleID.Slime)
                    UpdateProgress(0);
                break;
            case Objectives.ExploreBiomes:
                if (player.ZoneRockLayerHeight || player.ZoneDirtLayerHeight)
                    if (npc.aiStyle != NPCAIStyleID.Slime)
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

    protected override void HandleBreakTile(int type, ref bool fail, ref bool effectOnly)
    {
        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.GatherResources:
                if (type == TileID.Pots)
                    UpdateProgress(1);
                break;
        }
    }

    protected override void HandleBiomeEnter(Player player, BiomeType biome)
    {

        var objective = (Objectives)CurrentIndex;
        switch (objective)
        {
            case Objectives.ExploreBiomes:
                if (biome == BiomeType.Underground)
                    UpdateProgress(0);
                if (biome == BiomeType.Glowshroom)
                    UpdateProgress(1);
                if (biome == BiomeType.Jungle)
                    UpdateProgress(2);
                if (biome == BiomeType.UndergroundDesert)
                    UpdateProgress(3);
                if (biome == BiomeType.Snow)
                    UpdateProgress(4);
                break;

            case Objectives.DefendTown:
                if (biome == BiomeType.Forest)
                    UpdateProgress(0);
                break;
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
        NPCManager.GuideData,
        DialogueKeys.FallingStar.SlimeRain,
        lineCount: 2,
        zoomIn: false);
    }

    private void HandleSlimeRain(NPC npc)
    {
        if (npc.type == NPCAIStyleID.Slime)
        {
            UpdateProgress(0);
            if (Objective[CurrentIndex].Objectives[0].CurrentCount == 25)
            {
                DialogueManager.Instance.StartDialogueByKey(
                NPCManager.GuideData,
                DialogueKeys.FallingStar.SlimeRainCommentary,
                lineCount: 2,
                zoomIn: false);
            }
            if (Objective[CurrentIndex].Objectives[0].CurrentCount == 50)
            {
                DialogueManager.Instance.StartDialogueByKey(
                NPCManager.GuideData,
                DialogueKeys.FallingStar.SlimeRainWarning,
                lineCount: 2,
                zoomIn: false);
            }
        }
    }

    private void SpawnKingSlime()
    {
        DialogueManager.Instance.StartDialogueByKey(
        NPCManager.GuideData,
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