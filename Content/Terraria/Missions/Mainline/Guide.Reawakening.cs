using System.Collections.Generic;
using Terraria.ID;
using Terraria;
using Reverie.Core.Dialogue;
using Terraria.DataStructures;
using Reverie.Core.Missions;
using Reverie.Common.Players;

namespace Reverie.Content.Terraria.Missions.Mainline
{
    public class Reawakening_Mission(MissionData missionData) : Mission(missionData)
    {
        private readonly List<Item> giveStarterItems =
            [new Item(ItemID.CopperPickaxe),
             new Item(ItemID.CopperAxe)];

        private readonly List<Item> giveItems =
            [new Item(ItemID.MagicMirror),
             new Item(ItemID.WoodHelmet), new Item(ItemID.WoodBreastplate), new Item(ItemID.WoodGreaves)];


        public override void OnMissionComplete(bool rewards)
        {
            base.OnMissionComplete(rewards);
        }

        public override void OnObjectiveComplete(int objectiveIndex)
        {
            ReveriePlayer player = Main.LocalPlayer.GetModPlayer<ReveriePlayer>();
            if (CurrentSetIndex == 0 && objectiveIndex == 0) // "Talk to the Guide"
            {
                DialogueManager.Instance.PlayDialogueSequence(NPCDataManager.GuideData, DialogueID.GuideYappingAboutReverieLore);
                foreach (var GiveStarterItems in giveStarterItems)
                    Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), GiveStarterItems.type, GiveStarterItems.stack);
            }

            if (CurrentSetIndex == 1 && objectiveIndex == 0) // "Pick a class of choice"
            {
                Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), ItemID.TargetDummy);
                if (player.pathWarrior)
                {
                    DialogueManager.Instance.PlayDialogueSequence(NPCDataManager.GuideData, DialogueID.SelectedClass_Warrior);
                    Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), ItemID.CopperBroadsword);
                }
                else if (player.pathMarksman)
                {
                    DialogueManager.Instance.PlayDialogueSequence(NPCDataManager.GuideData, DialogueID.SelectedClass_Marksman);
                    Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), ItemID.CopperBow);
                    Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), ItemID.WoodenArrow, 80);
                }
                else if(player.pathMage)
                {
                    DialogueManager.Instance.PlayDialogueSequence(NPCDataManager.GuideData, DialogueID.SelectedClass_Mage);
                    Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), ItemID.AmethystStaff);
                }
                else if(player.pathConjurer)
                {
                    DialogueManager.Instance.PlayDialogueSequence(NPCDataManager.GuideData, DialogueID.SelectedClass_Conjurer);
                    Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), ItemID.BabyBirdStaff);
                }
            }

            if (CurrentSetIndex == 2 && objectiveIndex == 0) // "Attack the target dummy"
                DialogueManager.Instance.PlayDialogueSequence(NPCDataManager.GuideData, DialogueID.GuideGivingPropsTrainingArc);

            if (CurrentSetIndex == 2 && objectiveIndex == 1) // "Talk to the Guide"
            {
                DialogueManager.Instance.PlayDialogueSequence(NPCDataManager.GuideData, DialogueID.GuideGivesYouAMagicMirror);
                foreach (var GiveItems in giveItems)
                    Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), GiveItems.type, GiveItems.stack);
            }
                
            if (CurrentSetIndex == 4 && objectiveIndex == 2) // "Check in with the Guide"
                DialogueManager.Instance.PlayDialogueSequence(NPCDataManager.GuideData, DialogueID.ReawakeningEndingDialogue);
        }
    }
}