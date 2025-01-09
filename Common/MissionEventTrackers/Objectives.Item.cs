using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;

using Reverie.Core.Missions;
using Reverie.Common.Players;
using Reverie.Core.Dialogue;
using Reverie.Common.Systems;
using static Terraria.ModLoader.ModContent;
using Reverie.Content.Terraria.Items.Mission;

namespace Reverie.Common.MissionEventTrackers
{
    public class ObjectivesItem : GlobalItem
    {
        public override void OnCreated(Item item, ItemCreationContext context)
        {
            UpdateMissionProgress(item);
            base.OnCreated(item, context);
        }
        public override bool OnPickup(Item item, Player player)
        {
            base.OnPickup(item, player);
            if (item.playerIndexTheItemIsReservedFor == player.whoAmI)
            {
                // This suggests the item was recently dropped by this player
                return true;  // Allow pickup but don't update mission
            }
            else
                UpdateMissionProgress(item);

            return true;
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

            #region Translocator
            Mission translocator = player.GetMission(MissionID.Translocator);
            if (translocator != null && translocator.Progress == MissionProgress.Active)
            {
                if (translocator.CurrentSetIndex == 0)
                {
                    if (item.type == ItemType<RealmCrystal>())
                        translocator.UpdateProgress(0);
                    if (item.type == ItemType<CoilArray>())
                        translocator.UpdateProgress(0);
                    if (item.type == ItemType<DimensionalTuningFork>())
                        translocator.UpdateProgress(0);
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
                    if (item.potion == true && !(item.type == ItemID.HealingPotion || item.type == ItemID.LesserHealingPotion))
                    {
                        RedEyedRetribution.UpdateProgress(0, item.stack);
                    }
                    if (item.type == ItemID.HealingPotion || item.type == ItemID.LesserHealingPotion)
                    {
                        RedEyedRetribution.UpdateProgress(1, item.stack);
                    }

                }
            }
            #endregion
        }
    }
}