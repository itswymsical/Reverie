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
        private readonly List<Item> starterItems =
        [
            new Item(ItemID.CopperPickaxe),
            new Item(ItemID.CopperAxe)
        ];

        public Reawakening(Mission mission) : base(mission)
        {
            Reverie.Instance.Logger.Info("ReawakeningHandler constructed");
        }

        public override void OnObjectiveComplete(int objectiveIndex)
        {
            lock (handlerLock)
            {
                try
                {
                    Reverie.Instance.Logger.Debug($"Objective complete: Set={Mission.CurrentSetIndex}, Objective={objectiveIndex}");

                    switch (Mission.CurrentSetIndex)
                    {
                        case 0: // Initial Guide talk
                            DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData,
                                DialogueID.Reawakening_ProcessingSequence, true);
                            break;

                        case 1:
                            if (Mission.ObjectiveSets[1].IsCompleted)
                            {
                                DialogueManager.Instance.StartDialogue(
                                    NPCDataManager.GuideData,
                                    DialogueID.Reawakening_GuideResponse,
                                    
                                    nextDialogueId: DialogueID.Reawakening_TrainingSequence,
                                    nextNpcData: NPCDataManager.GuideData);

                                foreach (var item in starterItems)
                                    Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), item.type, item.stack);
                            }
                            break;

                        case 3:
                            var obj = Mission.ObjectiveSets[3].Objectives[0];
                            if (obj.CurrentCount == 5)
                            {
                                DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData,
                                    DialogueID.Reawakening_SylvanForeshadow);
                            }
                            break;

                        case 4: // Combat preparation
                            var obj2 = Mission.ObjectiveSets[4].Objectives[0];
                            if (obj2.CurrentCount == 2)
                            {
                                DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData,
                                    DialogueID.Reawakening_SylvanwaldeTeaser);
                            }
                            if (Mission.ObjectiveSets[4].IsCompleted)
                            {
                                DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData,
                                    DialogueID.Reawakening_ProgressCheck);
                            }
                            break;

                        case 6: // Mirror sequence
                            DialogueManager.Instance.StartDialogue(
                                NPCDataManager.GuideData,
                                DialogueID.Reawakening_TrainingComplete,

                                nextDialogueId: DialogueID.Reawakening_MagicMirror, 
                                nextNpcData: NPCDataManager.GuideData);

                            Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), ItemID.MagicMirror, 1);
                            break;

                        case 7:
                      
                            DialogueManager.Instance.StartDialogue(NPCDataManager.GuideData,
                                DialogueID.Reawakening_Closing);
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

                        case 2: // Resources
                            if (item.type == ItemID.Wood)
                                Mission.UpdateProgress(0, item.stack);
                            if (ItemID.Sets.Torches[item.type])
                                Mission.UpdateProgress(1, item.stack);
                            break;

                        case 3: // Underground prep
                            if (item.type is ItemID.IronOre or ItemID.LeadOre)
                                Mission.UpdateProgress(0, item.stack);
                            if (item.type is ItemID.IronAnvil or ItemID.LeadAnvil)
                                Mission.UpdateProgress(1);
                            break;

                        case 4: // Combat prep
                            if (item.CountsAsArmor())
                                Mission.UpdateProgress(0);
                            if (item.IsHealingPot())
                                Mission.UpdateProgress(1, item.stack);
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
                        case 2: // Resources
                            if (item.type == ItemID.Wood)
                                Mission.UpdateProgress(0, item.stack);
                            if (ItemID.Sets.Torches[item.type])
                                Mission.UpdateProgress(1, item.stack);
                            break;

                        case 3: // Underground prep
                            if (item.type is ItemID.IronOre or ItemID.LeadOre)
                                Mission.UpdateProgress(0, item.stack);
                            break;

                        case 5: // Life Crystals
                            if (item.type == ItemID.LifeCrystal)
                                Mission.UpdateProgress(0, item.stack);
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
                    if (Mission.CurrentSetIndex == 8 && npc.type == NPCID.EyeofCthulhu)
                    {
                        Mission.UpdateProgress(0);
                        Reverie.Instance.Logger.Debug("Eye of Cthulhu defeated");
                    }
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
                    if (npc.type == NPCID.Guide)
                    {
                        if (Mission.CurrentSetIndex == 0 ||
                            Mission.CurrentSetIndex == 6)
                        {
                            Mission.UpdateProgress(0);
                            Reverie.Instance.Logger.Debug($"Guide chat progress updated at step {Mission.CurrentSetIndex}");
                        }
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