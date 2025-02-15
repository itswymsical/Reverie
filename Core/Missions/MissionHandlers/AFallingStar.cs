using Terraria.DataStructures;
using System.Collections.Generic;
using Reverie.Core.Dialogue;
using Reverie.Utilities;
using Terraria.Audio;


namespace Reverie.Core.Missions.MissionHandlers;

public class AFallingStarMission : Mission
{
    private readonly List<Item> starterItems =
    [
        new Item(ItemID.CopperBroadsword),
        new Item(ItemID.CopperPickaxe),
        new Item(ItemID.CopperAxe)
    ];

    public AFallingStarMission() : base(
    MissionID.AFallingStar,
      "A Falling Star",
      "'Well, that's one way to make an appearance...'" +
      "\nBegin your journey in Terraria, discovering knowledge and power...",
      [
          [("Talk to Laine", 1)],
          [("Collect Stone", 25), ("Collect Wood", 50), ("Give Laine resources", 1)],
          [("Obtain a Helmet", 1), ("Obtain a Chestplate", 1), ("Obtain Leggings", 1), ("Obtain better weapon", 1)],
          [("Discover Accessories", 3), ("Mine 30 Ore", 30),("Obtain 15 bars of metal", 15)],
          [("Clear out slimes", 6)],
          [("Explore the Underground", 1), ("Loot items", 150)],
          [("Clear out slimes, again", 12)],
          [("Resume glorius looting", 100)],
          [("Return to Laine", 1)],
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

    public override void OnObjectiveComplete(int objectiveIndex)
    {

        lock (handlerLock)
        {
            try
            {
                switch (CurObjectiveIndex)
                {
                    case 0:
                        foreach (var item in starterItems)
                            player.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), item.type, item.stack);
                        break;

                    case 1:
                        DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_FixHouse, true);
                        break;

                    case 2:
                        if (ObjectiveIndex[CurObjectiveIndex].Objectives[0].IsCompleted 
                            && ObjectiveIndex[CurObjectiveIndex].Objectives[1].IsCompleted)
                            DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_WildlifeWoes, true);
                        break;

                    case 3:
                        DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_WildlifeWoes);
                        break;
                    case 4:
                        DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_SlimeInfestation, true);
                        break;
                    case 5:
                        DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_SlimeInfestation_Commentary, false);
                        break;

                    case 7:
                        Main.StartSlimeRain(true);
                        DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_SlimeRain);
                        break;

                    case 9:
                        DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_KS_Encounter, true);
                        if (!NPC.AnyNPCs(NPCID.KingSlime))
                        {
                            if (Main.LocalPlayer.whoAmI == Main.myPlayer)
                            {
                                SoundEngine.PlaySound(SoundID.Roar, Main.LocalPlayer.position);

                                int type = NPCID.KingSlime;

                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    NPC.SpawnOnPlayer(Main.LocalPlayer.whoAmI, type);
                                }
                                else
                                {
                                    NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: Main.LocalPlayer.whoAmI, number2: type);
                                }
                            }
                        }
                        break;

                    case 10:
                        DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_KS_Victory, true);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Instance.Logger.Error($"Error in OnObjectiveComplete: {ex.Message}");
            }
        }
    }

    public override void OnItemCreated(Item item, ItemCreationContext context)
    {
        lock (handlerLock)
        {
            try
            {
                switch (CurObjectiveIndex)
                {
                    case 1:
                        if (item.buffTime > 0 || item.potion || item.useStyle is ItemUseStyleID.DrinkLiquid)
                            UpdateProgress(2, item.stack);
                        break;

                    case 2:
                        if (item.headSlot != -1 && !item.vanity)
                            UpdateProgress(0, item.stack);

                        if (item.bodySlot != -1 && !item.vanity)
                            UpdateProgress(1, item.stack);

                        if (item.legSlot != -1 && !item.vanity)
                            UpdateProgress(2, item.stack);

                        if (item.IsWeapon())
                            UpdateProgress(3, item.stack);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Instance.Logger.Error($"Error in OnItemCreated: {ex.Message}");
            }
        }
    }

    public override void OnItemPickup(Item item)
    {
        lock (handlerLock)
        {
            try
            {
                switch (CurObjectiveIndex)
                {
                    case 1:
                        if (item.type == ItemID.StoneBlock)
                            UpdateProgress(0, item.stack);

                        if (item.type == ItemID.Wood)
                            UpdateProgress(1, item.stack);

                        if (item.buffTime > 0 || item.potion || item.useStyle is ItemUseStyleID.DrinkLiquid)
                            UpdateProgress(2, item.stack);
                        break;
                    case 2:
                        if (item.headSlot != -1 && !item.vanity)
                            UpdateProgress(0, item.stack);

                        if (item.bodySlot != -1 && !item.vanity)
                            UpdateProgress(1, item.stack);

                        if (item.legSlot != -1 && !item.vanity)
                            UpdateProgress(2, item.stack);

                        if (item.IsWeapon())
                            UpdateProgress(3, item.stack);
                        break;
                    case 3:
                        if (item.accessory)
                            UpdateProgress(0, item.stack);
                        if (item.IsOre())
                            UpdateProgress(1, item.stack);
                        if (item.Name.Contains("Bar"))
                            UpdateProgress(2, item.stack);
                        break;
                    case 5:
                        if (!item.IsCurrency && (item.accessory || item.IsWeapon() || item.IsMiningTool() || item.value > 0 || item.rare > ItemRarityID.White))
                        UpdateProgress(1, item.stack);

                        if (ObjectiveIndex[CurObjectiveIndex].Objectives[1].CurrentCount == 10)
                        {
                            DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_SlimeInfestation);
                        }
                        break;
                    case 7:
                        if (!item.IsCurrency && (item.accessory || item.IsWeapon() || item.IsMiningTool() || item.value > 0 || item.rare > ItemRarityID.White))
                            UpdateProgress(0, item.stack);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Instance.Logger.Error($"Error in OnItemPickup: {ex.Message}");
            }
        }
    }

    public override void OnNPCKill(NPC npc)
    {
        lock (handlerLock)
        {
            try
            {
                switch (CurObjectiveIndex)
                {
                    case 4:
                        if (npc.type == NPCAIStyleID.Slime)
                            UpdateProgress(0);
                        break;
                    case 6:
                        if (npc.type == NPCAIStyleID.Slime)
                            UpdateProgress(0);
                        break;
                    case 9:
                        if (npc.type == NPCAIStyleID.Slime)
                            UpdateProgress(0);
                        if (ObjectiveIndex[CurObjectiveIndex].Objectives[0].CurrentCount == 20)
                            DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_SlimeRain_Commentary, false);
                        break;
                    case 10:
                        if (npc.type == NPCID.KingSlime)
                            UpdateProgress(0);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Instance.Logger.Error($"Error in OnNPCKill: {ex.Message}");
            }
        }
    }

    public override void OnBiomeEnter(Player player, BiomeType biome)
    {
        lock (handlerLock)
        {
            try
            {
                switch (CurObjectiveIndex)
                {
                    case 5:
                        if (biome == BiomeType.Underground)
                            UpdateProgress(0);
                        break;
                    case 8:
                        if (biome == BiomeType.Forest)
                            UpdateProgress(0);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Instance.Logger.Error($"Error in OnNPCChat: {ex.Message}");
            }
        }
    }

    public override void OnNPCChat(NPC npc)
    {
        lock (handlerLock)
        {
            try
            {
                switch (CurObjectiveIndex)
                {
                    case 0:
                        DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_GatheringResources, true);
                        UpdateProgress(0);
                        break;
                    case 1:
                        if (ObjectiveIndex[1].Objectives[0].IsCompleted 
                            && ObjectiveIndex[1].Objectives[1].IsCompleted)
                        {
                            UpdateProgress(2);
                        }
                    
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Instance.Logger.Error($"Error in OnNPCChat: {ex.Message}");
            }
        }
    }
}