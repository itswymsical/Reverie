using Terraria.ID;
using Terraria;
using Terraria.DataStructures;
using System;
using System.Collections.Generic;
using Reverie.Core.Dialogue;
using Reverie.Common.MissionAttributes;
using Reverie.Common;


namespace Reverie.Core.Missions.Mainline
{
    [MissionHandler(MissionID.CrashLanding)]
    public class CrashLanding : MissionObjectiveHandler
    {
        private readonly List<Item> starterItems =
        [ 
            new Item(ItemID.CopperShortsword),
            new Item(ItemID.CopperPickaxe),
            new Item(ItemID.CopperAxe)
        ];

        public CrashLanding(Mission mission) : base(mission)
        {
            Reverie.Instance.Logger.Info("CrashLanding Handler constructed");
        }

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

                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Reverie.Instance.Logger.Error($"Error in OnObjectiveComplete: {ex.Message}");
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
                    Reverie.Instance.Logger.Error($"Error in OnItemCreated: {ex.Message}");
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

                            if (item.type is ItemID.Daybloom or ItemID.Blinkroot 
                                or ItemID.Deathweed or ItemID.Moonglow 
                                or ItemID.Fireblossom or ItemID.Shiverthorn or ItemID.Waterleaf)
                                Mission.UpdateProgress(2, item.stack);
                            break;
                        case 3:
                            if (item.headSlot != -1 && !item.vanity)
                                Mission.UpdateProgress(0, item.stack);

                            if (item.bodySlot != -1 && !item.vanity)
                                Mission.UpdateProgress(1, item.stack);

                            if (item.legSlot != -1 && !item.vanity)
                                Mission.UpdateProgress(2, item.stack);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Reverie.Instance.Logger.Error($"Error in OnItemPickup: {ex.Message}");
                }
            }
        }

        public override void OnNPCKill(NPC npc)
        {
            lock (handlerLock)
            {
                try
                {

                }
                catch (Exception ex)
                {
                    Reverie.Instance.Logger.Error($"Error in OnNPCKill: {ex.Message}");
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
                    Reverie.Instance.Logger.Error($"Error in OnNPCChat: {ex.Message}");
                }
            }
        }
    }
}