using Reverie.Core.Missions;
using Terraria.ID;
using Terraria;
using Reverie.Common.Players;
using Reverie.Core.Dialogue;
using System.Collections.Generic;
using Terraria.DataStructures;
using Reverie.Common;
using Reverie.Common.MissionAttributes;

namespace Reverie.Content.Terraria.Missions.Mainline
{
    [MissionHandler(MissionID.Reawakening)]
    public class ReawakeningHandler : MissionObjectiveHandler
    {
        private readonly List<Item> giveStarterItems = [new Item(ItemID.CopperPickaxe),
         new Item(ItemID.CopperAxe)];

        private readonly List<Item> giveItems = [new Item(ItemID.MagicMirror),
         new Item(ItemID.WoodHelmet),
         new Item(ItemID.WoodBreastplate),
         new Item(ItemID.WoodGreaves)];

        public ReawakeningHandler(Mission mission) : base(mission)
        {
            Main.NewText("ReawakeningHandler constructed");
        }
        public override void OnObjectiveComplete(int objectiveIndex)
        {
            ReveriePlayer player = Main.LocalPlayer.GetModPlayer<ReveriePlayer>();

            switch (Mission.CurrentSetIndex)
            {
                case 0 when objectiveIndex == 0: // "Talk to the Guide"
                    DialogueManager.Instance.PlayDialogueSequence(NPCDataManager.GuideData, DialogueID.Mission_01_Briefing, true);
                    foreach (var item in giveStarterItems)
                        Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), item.type, item.stack);
                    break;

                case 1 when objectiveIndex == 0: // "Pick a class of choice"
                    Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), ItemID.TargetDummy);
                    HandleClassSelection(player);
                    break;

                case 2:
                    switch (objectiveIndex)
                    {
                        case 0: // "Attack the target dummy"
                            DialogueManager.Instance.PlayDialogueSequence(NPCDataManager.GuideData,
                                DialogueID.Mission_01_TrainingComplete);
                            break;
                        case 1: // "Talk to the Guide"
                            DialogueManager.Instance.PlayDialogueSequence(NPCDataManager.GuideData,
                                DialogueID.Mission_01_MagicMirror, true);
                            foreach (var item in giveItems)
                                Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), item.type, item.stack);
                            break;
                    }
                    break;

                case 4 when objectiveIndex == 2: // "Check in with the Guide"
                    DialogueManager.Instance.PlayDialogueSequence(NPCDataManager.GuideData,
                        DialogueID.Mission_01_Outro, true);
                    break;
            }
        }

        private static void HandleClassSelection(ReveriePlayer player)
        {
            if (player.pathWarrior)
            {
                DialogueManager.Instance.PlayDialogueSequence(NPCDataManager.GuideData, DialogueID.SelectedClass_Warrior, true);
                Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), ItemID.CopperBroadsword);
            }
            else if (player.pathMarksman)
            {
                DialogueManager.Instance.PlayDialogueSequence(NPCDataManager.GuideData, DialogueID.SelectedClass_Marksman, true);
                Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), ItemID.CopperBow);
                Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), ItemID.WoodenArrow, 80);
            }
            else if (player.pathMage)
            {
                DialogueManager.Instance.PlayDialogueSequence(NPCDataManager.GuideData, DialogueID.SelectedClass_Mage, true);
                Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), ItemID.AmethystStaff);
            }
            else if (player.pathConjurer)
            {
                DialogueManager.Instance.PlayDialogueSequence(NPCDataManager.GuideData, DialogueID.SelectedClass_Conjurer, true);
                Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), ItemID.BabyBirdStaff);
            }
        }

        public override void OnItemPickup(Item item)
        {
            if (Mission.CurrentSetIndex == 3)
            {
                if (ItemID.Sets.Torches[item.type])
                {
                    Mission.UpdateProgress(1, item.stack);
                }
            }
            else if (Mission.CurrentSetIndex == 4)
            {
                if (item.IsPickaxe() && item.type != ItemID.CopperPickaxe)
                {
                    Mission.UpdateProgress(0);
                }
                if (item.type == ItemID.IronOre || item.type == ItemID.LeadOre)
                {
                    Mission.UpdateProgress(1, item.stack);
                }
            }
        }

        public override void OnNPCHit(NPC npc, int damage)
        {
            if (Mission.CurrentSetIndex == 2 && npc.type == NPCID.TargetDummy)
            {
                Mission.UpdateProgress(0);
            }
        }

        public override void OnNPCKill(NPC npc)
        {
            if (Mission.CurrentSetIndex == 3 && npc.aiStyle == NPCAIStyleID.Slime)
            {
                Mission.UpdateProgress(0);
            }
        }

        public override void OnNPCChat(NPC npc)
        {
            if (npc.type == NPCID.Guide)
            {
                if (Mission.CurrentSetIndex == 0)
                    Mission.UpdateProgress(0);
                else if (Mission.CurrentSetIndex == 2)
                    Mission.UpdateProgress(1);
                else if (Mission.CurrentSetIndex == 4 &&
                    Mission.MissionData.ObjectiveSets[4].Objectives[0].IsCompleted &&
                    Mission.MissionData.ObjectiveSets[4].Objectives[1].IsCompleted)
                {
                    Mission.UpdateProgress(2);
                }
            }
        }
    }
}
