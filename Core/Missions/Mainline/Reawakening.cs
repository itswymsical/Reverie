using Terraria.ID;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

using System;
using System.Collections.Generic;

using Reverie.Core.Dialogue;
using Reverie.Common;
using Reverie.Common.MissionAttributes;

namespace Reverie.Core.Missions.Mainline
{
    [MissionHandler(MissionID.Reawakening)]
    public class Reawakening : MissionObjectiveHandler
    {
        private readonly List<Item> giveStarterItems = [new Item(ItemID.CopperPickaxe),
         new Item(ItemID.CopperAxe)];

        private readonly List<Item> giveItems = [new Item(ItemID.MagicMirror),
         new Item(ItemID.WoodHelmet),
         new Item(ItemID.WoodBreastplate),
         new Item(ItemID.WoodGreaves)];

        public Reawakening(Mission mission) : base(mission)
        {
            ModContent.GetInstance<Reverie>().Logger.Info("ReawakeningHandler constructed");
        }

        public override void OnObjectiveComplete(int objectiveIndex)
        {
            lock (handlerLock)
            {
                try
                {
                    ModContent.GetInstance<Reverie>().Logger.Debug($"Objective complete: Set={Mission.CurrentSetIndex}, Objective={objectiveIndex}");

                    switch (Mission.CurrentSetIndex)
                    {
                        case 0 when objectiveIndex == 0: // "Talk to the Guide"
                            DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData, DialogueID.Mission_01_Briefing, true);
                            foreach (var item in giveStarterItems)
                                Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), item.type, item.stack);
                            break;

                        case 1: // "Attack the target dummy"
                            if (objectiveIndex == 0)
                            {
                                DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData,
                                    DialogueID.Mission_01_TrainingComplete);
                            }
                            else if (objectiveIndex == 1) // "Talk to the Guide"
                            {
                                DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData,
                                    DialogueID.Mission_01_MagicMirror, true);
                                foreach (var item in giveItems)
                                    Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), item.type, item.stack);
                            }
                            break;

                        case 6: // "Check in with the Guide"
                            DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData,
                                DialogueID.Mission_01_Outro, true);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnObjectiveComplete: {ex.Message}");
                }
            }
        }

        public override void OnItemCreated(Item item, ItemCreationContext context)
        {
            lock (handlerLock)
            {
                try
                {
                    if (Mission.CurrentSetIndex == 2)
                    {
                        if (ItemID.Sets.Torches[item.type])
                        {
                            Mission.UpdateProgress(1, item.stack);
                            ModContent.GetInstance<Reverie>().Logger.Debug($"Updated torch progress: +{item.stack}");
                        }
                    }
                    else if (Mission.CurrentSetIndex == 3)
                    {
                        if (item.type == ItemID.IronOre || item.type == ItemID.LeadOre)
                        {
                            Mission.UpdateProgress(0, item.stack);
                            ModContent.GetInstance<Reverie>().Logger.Debug($"Updated ore creation progress: +{item.stack}");
                        }
                        if (item.type == ItemID.IronAnvil || item.type == ItemID.LeadAnvil)
                        {
                            Mission.UpdateProgress(1);
                            ModContent.GetInstance<Reverie>().Logger.Debug("Updated anvil progress");
                        }
                    }
                    else if (Mission.CurrentSetIndex == 4)
                    {
                        if (item.IsPickaxe() && item.type != ItemID.CopperPickaxe)
                        {
                            Mission.UpdateProgress(0);
                            ModContent.GetInstance<Reverie>().Logger.Debug("Updated pickaxe progress");
                        }
                        if (item.CountsAsArmor())
                        {
                            Mission.UpdateProgress(1, item.stack);
                            ModContent.GetInstance<Reverie>().Logger.Debug("Updated armor progress");
                        }
                    }
                    else if (Mission.CurrentSetIndex == 5)
                    {
                        if (item.IsHealingPot())
                        {
                            Mission.UpdateProgress(1, item.stack);
                            ModContent.GetInstance<Reverie>().Logger.Debug("Updated heal pot progress");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnItemPickup: {ex.Message}");
                }
            }
        }

        public override void OnItemPickup(Item item)
        {
            lock (handlerLock)
            {
                try
                {
                    if (Mission.CurrentSetIndex == 2)
                    {
                        if (ItemID.Sets.Torches[item.type])
                        {
                            Mission.UpdateProgress(1, item.stack);
                            ModContent.GetInstance<Reverie>().Logger.Debug($"Updated torch progress: +{item.stack}");
                        }
                    }
                    else if (Mission.CurrentSetIndex == 3)
                    {
                        if (item.type == ItemID.IronOre || item.type == ItemID.LeadOre)
                        {
                            Mission.UpdateProgress(0, item.stack);
                            ModContent.GetInstance<Reverie>().Logger.Debug($"Updated ore pickup progress: +{item.stack}");
                        }
                        if (item.type == ItemID.IronAnvil || item.type == ItemID.LeadAnvil)
                        {
                            Mission.UpdateProgress(1);
                            ModContent.GetInstance<Reverie>().Logger.Debug("Updated anvil progress");
                        }
                    }
                    else if (Mission.CurrentSetIndex == 4)
                    {
                        if (item.IsPickaxe() && item.type != ItemID.CopperPickaxe)
                        {
                            Mission.UpdateProgress(0);
                            ModContent.GetInstance<Reverie>().Logger.Debug("Updated pickaxe progress");
                        }
                        if (item.CountsAsArmor())
                        {
                            Mission.UpdateProgress(1, item.stack);
                            ModContent.GetInstance<Reverie>().Logger.Debug("Updated armor progress");
                        }
                    }
                    else if (Mission.CurrentSetIndex == 5)
                    {
                        if (item.type == ItemID.LifeCrystal)
                        {
                            Mission.UpdateProgress(0, item.stack);
                            ModContent.GetInstance<Reverie>().Logger.Debug("Updated life crystal progress");
                        }
                        if (item.IsHealingPot())
                        {
                            Mission.UpdateProgress(1, item.stack);
                            ModContent.GetInstance<Reverie>().Logger.Debug("Updated heal pot progress");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnItemPickup: {ex.Message}");
                }
            }
        }

        public override void OnNPCHit(NPC npc, int damage)
        {
            lock (handlerLock)
            {
                try
                {
                    ModContent.GetInstance<Reverie>().Logger.Debug($"NPC Hit: Type={npc.type}, Damage={damage}, CurrentSetIndex={Mission.CurrentSetIndex}");

                    if (Mission.CurrentSetIndex == 1 && npc.type == NPCID.TargetDummy)
                    {
                        Mission.UpdateProgress(0);
                        ModContent.GetInstance<Reverie>().Logger.Debug("Updated target dummy hit progress");
                    }
                }
                catch (Exception ex)
                {
                    ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnNPCHit: {ex.Message}");
                }
            }
        }

        public override void OnNPCKill(NPC npc)
        {
            lock (handlerLock)
            {
                try
                {
                    ModContent.GetInstance<Reverie>().Logger.Debug($"NPC Kill: Type={npc.type}, CurrentSetIndex={Mission.CurrentSetIndex}");

                    if (Mission.CurrentSetIndex == 2 && npc.aiStyle == NPCAIStyleID.Slime)
                    {
                        Mission.UpdateProgress(0);
                        ModContent.GetInstance<Reverie>().Logger.Debug("Updated slime kill progress");
                    }
                }
                catch (Exception ex)
                {
                    ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnNPCKill: {ex.Message}");
                }
            }
        }

        public override void OnNPCChat(NPC npc)
        {
            lock (handlerLock)
            {
                try
                {
                    ModContent.GetInstance<Reverie>().Logger.Debug($"NPC Chat: Type={npc.type}, CurrentSetIndex={Mission.CurrentSetIndex}");

                    if (npc.type == NPCID.Guide)
                    {
                        if (Mission.CurrentSetIndex == 0)
                        {
                            Mission.UpdateProgress(0);
                            ModContent.GetInstance<Reverie>().Logger.Debug("Updated initial Guide chat progress");
                        }
                        else if (Mission.CurrentSetIndex == 1)
                        {
                            Mission.UpdateProgress(1);
                            ModContent.GetInstance<Reverie>().Logger.Debug("Updated training Guide chat progress");
                        }
                        else if (Mission.CurrentSetIndex == 6)
                        {
                            Mission.UpdateProgress(0);
                            ModContent.GetInstance<Reverie>().Logger.Debug("Updated final Guide chat progress");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnNPCChat: {ex.Message}");
                }
            }
        }
    }
}