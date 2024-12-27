using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;

using Reverie.Core.Missions;
using Reverie.Common.Players;
using Reverie.Core.Dialogue;
using Reverie.Common.Systems;

namespace Reverie.Common.MissionEventTrackers
{
    public class ObjectivesItem : GlobalItem
    {
        public override void OnCreated(Item item, ItemCreationContext context)
        {
            UpdateMissionProgress(item);
            base.OnCreated(item, context);
        }
        public override void UpdateInventory(Item item, Player player)
        {
            if (item.accessory && !DownedBossSystem.pickedUpAnAccessoryForTheFirstTime)
            {
                DialogueManager.Instance.PlayDialogueSequence(NPCDataManager.GuideData, DialogueID.GuideWhenYouFindAnAccessory);
                DownedBossSystem.pickedUpAnAccessoryForTheFirstTime = true;
            }
        }

        private static void UpdateMissionProgress(Item item)
        {     
            MissionPlayer player = Main.LocalPlayer.GetModPlayer<MissionPlayer>();

            Mission testMission = player.GetMission(MissionID.TestMission);
            if (testMission != null && testMission.Progress == MissionProgress.Active)
            {
                if (item.type == ItemID.DirtBlock)
                    testMission.UpdateProgress(0);
            }

            Mission argiesHunt = player.GetMission(MissionID.ArgiesHunt);
            if (argiesHunt != null && argiesHunt.Progress == MissionProgress.Active)
            {
                if (item.type == ItemID.Mushroom)
                    argiesHunt.UpdateProgress(0, item.stack);

                if (item.type == ItemID.GlowingMushroom)
                    argiesHunt.UpdateProgress(1, item.stack);
            }

            Mission foolsGold = player.GetMission(MissionID.FoolsGold);
            if (foolsGold != null && foolsGold.Progress == MissionProgress.Active)
            {
                if (item.type == ItemID.CopperOre)
                    foolsGold.UpdateProgress(0, item.stack);

                if (item.type == ItemID.LeadOre)
                    foolsGold.UpdateProgress(1, item.stack);
            }

            #region Reawakening
            Mission Reawakening = player.GetMission(MissionID.Reawakening);
            if (Reawakening != null && Reawakening.Progress == MissionProgress.Active)
            {
                if (Reawakening.CurrentSetIndex == 3)
                {
                    if (ItemID.Sets.Torches[item.type] == true)
                        Reawakening.UpdateProgress(1, item.stack);
                }
                if (Reawakening.CurrentSetIndex == 4)
                {
                    if (item.IsPickaxe() && item.type != ItemID.CopperPickaxe)
                        Reawakening.UpdateProgress(0, 1);

                    if (item.type == ItemID.IronOre || item.type == ItemID.LeadOre)
                        Reawakening.UpdateProgress(1, item.stack);
                }
            }
            #endregion

            #region Red Eyed Retribution
            Mission RedEyedRetribution = player.GetMission(MissionID.RedEyedRetribution);
            if (RedEyedRetribution != null && RedEyedRetribution.Progress == MissionProgress.Active)
            {
                if (RedEyedRetribution.CurrentSetIndex == 0)
                {
                    if (item.type == ItemID.Lens)
                        RedEyedRetribution.UpdateProgress(0, item.stack);
                    else if (item.type == ItemID.SuspiciousLookingEye)
                        RedEyedRetribution.UpdateProgress(0, 6);

                    if (item.type == ItemID.LifeCrystal)
                        RedEyedRetribution.UpdateProgress(1, item.stack);
                }
                if (RedEyedRetribution.CurrentSetIndex == 1)
                {
                    if (item.type == ItemID.IronskinPotion)
                    {
                        RedEyedRetribution.UpdateProgress(0, 1);
                    }
                    if (item.type == ItemID.RegenerationPotion)
                    {
                        RedEyedRetribution.UpdateProgress(0, 1);
                    }
                    if (item.type == ItemID.SwiftnessPotion)
                    {
                        RedEyedRetribution.UpdateProgress(0, 1);
                    }
                }
            }
            #endregion
        }
    }
}