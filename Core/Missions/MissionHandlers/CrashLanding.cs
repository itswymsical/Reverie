﻿using Terraria.DataStructures;
using System.Collections.Generic;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions.MissionAttributes;
using Reverie.Utilities;

namespace Reverie.Core.Missions.MissionHandlers
{
    [MissionHandler(MissionID.CrashLanding)]
    public class CrashLanding : ObjectiveHandler
    {
        public CrashLanding(Mission mission) : base(mission)
            => Instance.Logger.Info("[Crash Landing] Mission handler constructed");

        private readonly List<Item> starterItems =
        [
            new Item(ItemID.CopperShortsword),
            new Item(ItemID.CopperPickaxe),
            new Item(ItemID.CopperAxe)
        ];

        public override void OnObjectiveComplete(int objectiveIndex)
        {

            lock (handlerLock)
            {
                try
                {
                    switch (Mission.CurrentSetIndex)
                    {
                        case 0:
                            foreach (var item in starterItems)
                                Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), item.type, item.stack);
                            break;

                        case 2:
                            DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_FixHouse, true);
                            break;
                        case 3:
                            if (Mission.ObjectiveSets[Mission.CurrentSetIndex].Objectives[3].IsCompleted)
                            {
                                DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_WildlifeWoes, true);
                            }
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
                    switch (Mission.CurrentSetIndex)
                    {
                        case 1:
                            if (item.buffTime > 0 || item.potion || item.useStyle is ItemUseStyleID.DrinkLiquid)
                                Mission.UpdateProgress(2, item.stack);
                            break;
                        case 3:
                            if (item.headSlot != -1 && !item.vanity)
                                Mission.UpdateProgress(0, item.stack);

                            if (item.bodySlot != -1 && !item.vanity)
                                Mission.UpdateProgress(1, item.stack);

                            if (item.legSlot != -1 && !item.vanity)
                                Mission.UpdateProgress(2, item.stack);

                            if (item.IsWeapon())
                                Mission.UpdateProgress(3, item.stack);
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
                    switch (Mission.CurrentSetIndex)
                    {
                        case 1:
                            if (item.type == ItemID.StoneBlock)
                                Mission.UpdateProgress(0, item.stack);

                            if (item.type == ItemID.Wood)
                                Mission.UpdateProgress(1, item.stack);

                            if (item.buffTime > 0 || item.potion || item.useStyle is ItemUseStyleID.DrinkLiquid)
                                Mission.UpdateProgress(2, item.stack);
                            break;
                        case 3:
                            if (item.headSlot != -1 && !item.vanity)
                                Mission.UpdateProgress(0, item.stack);

                            if (item.bodySlot != -1 && !item.vanity)
                                Mission.UpdateProgress(1, item.stack);

                            if (item.legSlot != -1 && !item.vanity)
                                Mission.UpdateProgress(2, item.stack);

                            if (item.IsWeapon())
                                Mission.UpdateProgress(3, item.stack);
                            break;
                        case 5:
                            if (item.type is not ItemID.CopperCoin or ItemID.SilverCoin or ItemID.GoldCoin or ItemID.PlatinumCoin)
                            Mission.UpdateProgress(1, item.stack);

                            if (Mission.ObjectiveSets[Mission.CurrentSetIndex].Objectives[1].CurrentCount == 10)
                            {
                                DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_SlimeInfestation);
                            }
                            break;
                        case 7:
                            if (item.type is not ItemID.CopperCoin or ItemID.SilverCoin or ItemID.GoldCoin or ItemID.PlatinumCoin)
                                Mission.UpdateProgress(0, item.stack);
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
                    switch (Mission.CurrentSetIndex)
                    {
                        case 4:
                            if (npc.type == NPCAIStyleID.Slime)
                                Mission.UpdateProgress(0);
                            break;
                        case 6:
                            if (npc.type == NPCAIStyleID.Slime)
                                Mission.UpdateProgress(0);
                            break;
                        case 9:
                            if (npc.type == NPCAIStyleID.Slime)
                                Mission.UpdateProgress(0);
                            if (Mission.ObjectiveSets[Mission.CurrentSetIndex].Objectives[0].CurrentCount == 20)
                                DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_SlimeRain_Commentary, false);
                            break;
                        case 10:
                            if (npc.type == NPCID.KingSlime)
                                Mission.UpdateProgress(0);
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
                    switch (Mission.CurrentSetIndex)
                    {
                        case 5:
                            if (biome == BiomeType.Underground)
                                Mission.UpdateProgress(0);
                            break;
                        case 8:
                            if (biome == BiomeType.Forest)
                                Mission.UpdateProgress(0);
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
                    switch (Mission.CurrentSetIndex)
                    {
                        case 0:
                            DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.CrashLanding_GatheringResources, true);
                            Mission.UpdateProgress(0);
                            break;
                        case 2:
                            Mission.UpdateProgress(0);
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
}