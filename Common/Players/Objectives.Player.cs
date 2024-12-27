using Microsoft.Xna.Framework;
using Reverie.Common.Systems;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;

using Terraria;
using Terraria.ID;


namespace Reverie.Common.Players
{
    partial class MissionPlayer
    {
        public override void OnEnterWorld()
        {
            Mission Reawakening = GetMission(MissionID.Reawakening);
            ReveriePlayer player = Main.LocalPlayer.GetModPlayer<ReveriePlayer>();

            if (Reawakening != null && Reawakening.State != MissionState.Completed)
            {
                if (Reawakening.Progress != MissionProgress.Active)
                {
                    //CutsceneLoader.PlayCutscene(new IntroCutscene());
                    UnlockMission(MissionID.Reawakening);
                    StartMission(MissionID.Reawakening);

                    Reawakening.Progress = MissionProgress.Active;
                }

                if (Reawakening.CurrentSetIndex == 1)
                {
                    if (!player.pathWarrior && !player.pathMarksman && !player.pathMage && !player.pathConjurer)
                    {
                        ReverieUISystem.Instance.ClassInterface.SetState(ReverieUISystem.Instance.classUI);
                    }
                }
            }
        }

        public override bool OnPickup(Item item)
        {
            UpdateMissionProgress(item);
            return base.OnPickup(item);
        }

        private void UpdateMissionProgress(Item item)
        {
            MissionPlayer player = Main.LocalPlayer.GetModPlayer<MissionPlayer>();

            // DirtiestBlock mission
            Mission dirtiestBlockMission = player.GetMission(MissionID.DirtiestBlock);
            if (dirtiestBlockMission != null && dirtiestBlockMission.Progress == MissionProgress.Active)
            {
                if (item.type == ItemID.DirtiestBlock)
                {
                    dirtiestBlockMission.UpdateProgress(0);
                }
            }

            // TestMission
            Mission testMission = player.GetMission(MissionID.TestMission);
            if (testMission != null && testMission.Progress == MissionProgress.Active)
            {
                if (item.type == ItemID.DirtBlock)
                {
                    testMission.UpdateProgress(0);
                }
            }

            // Reawakening mission
            Mission reawakening = player.GetMission(MissionID.Reawakening);
            if (reawakening != null && reawakening.Progress == MissionProgress.Active)
            {
                if (reawakening.CurrentSetIndex == 3)
                {
                    if (ItemID.Sets.Torches[item.type] == true)
                    {
                        reawakening.UpdateProgress(1, item.stack);
                    }
                }
                if (reawakening.CurrentSetIndex == 4)
                {
                    if (item.IsPickaxe() && item.type != ItemID.CopperPickaxe)
                    {
                        reawakening.UpdateProgress(0);
                    }
                    if (item.type == ItemID.IronOre || item.type == ItemID.LeadOre)
                    {
                        reawakening.UpdateProgress(1, item.stack);
                    }
                }
            }

            // RedEyedRetribution mission
            Mission redEyedRetribution = player.GetMission(MissionID.RedEyedRetribution);
            if (redEyedRetribution != null && redEyedRetribution.Progress == MissionProgress.Active)
            {
                if (redEyedRetribution.CurrentSetIndex == 0)
                {
                    if (item.type == ItemID.Lens)
                    {
                        redEyedRetribution.UpdateProgress(0, item.stack);
                    }
                    else if (item.type == ItemID.SuspiciousLookingEye)
                    {
                        redEyedRetribution.UpdateProgress(0, 6);
                    }
                    if (item.type == ItemID.LifeCrystal)
                    {
                        redEyedRetribution.UpdateProgress(1, item.stack);
                    }
                }
                if (redEyedRetribution.CurrentSetIndex == 1)
                {
                    if (item.type == ItemID.IronskinPotion || item.type == ItemID.RegenerationPotion || item.type == ItemID.SwiftnessPotion)
                    {
                        redEyedRetribution.UpdateProgress(0);
                    }
                }
            }
        }

        public override void PostUpdate()
        {
            if (Player.JustDroppedAnItem)
            {
                Item droppedItem = Main.item[Player.selectedItem];
                RemoveMissionProgress(droppedItem);
            }
        }

        public override void PostUpdateMiscEffects()
        {       
            if (Player.ZoneCanopy() && !DownedBossSystem.enteredTheWoodlandCanopyBeforeProgression)
            {
                DialogueManager.Instance.PlayDialogueSequence(NPCDataManager.GuideData, DialogueID.EnteredWoodlandCanopyBeforeProgression);
                DownedBossSystem.enteredTheWoodlandCanopyBeforeProgression = true;
            }
        }

        private void RemoveMissionProgress(Item item)
        {
            // DirtiestBlock mission
            Mission dirtiestBlockMission = GetMission(MissionID.DirtiestBlock);
            if (dirtiestBlockMission != null && dirtiestBlockMission.Progress == MissionProgress.Active)
            {
                if (item.type == ItemID.DirtiestBlock)
                {
                    dirtiestBlockMission.RemoveProgress(0, 1);
                }
            }

            // TestMission
            Mission testMission = GetMission(MissionID.TestMission);
            if (testMission != null && testMission.Progress == MissionProgress.Active)
            {
                if (item.type == ItemID.DirtBlock)
                {
                    testMission.RemoveProgress(0, 1);
                }
            }

            // Reawakening mission
            Mission reawakening = GetMission(MissionID.Reawakening);
            if (reawakening != null && reawakening.Progress == MissionProgress.Active)
            {
                if (reawakening.CurrentSetIndex == 3)
                {
                    if (ItemID.Sets.Torches[item.type] == true)
                    {
                        reawakening.RemoveProgress(1, item.stack);
                    }
                }
                if (reawakening.CurrentSetIndex == 4)
                {
                    if (item.IsPickaxe() && item.type != ItemID.CopperPickaxe)
                    {
                        reawakening.RemoveProgress(0, 1);
                    }
                    if (item.type == ItemID.IronOre || item.type == ItemID.LeadOre)
                    {
                        reawakening.RemoveProgress(1, item.stack);
                    }
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            MissionPlayer missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
            Mission Reawakening = missionPlayer.GetMission(MissionID.Reawakening);

            if (Reawakening != null && Reawakening.Progress == MissionProgress.Active)
            {
                if (Reawakening.CurrentSetIndex == 2)
                {
                    if (target.type == NPCID.TargetDummy)
                        Reawakening.UpdateProgress(0);
                }
            }
            base.OnHitNPC(target, hit, damageDone);
        }
    }
}
