using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Reverie.Common.Players;
using SubworldLibrary;
using Reverie.Common.Systems.Subworlds.Archaea;
using Reverie.Content.Archaea.NPCs.Surface;


namespace Reverie.Common.MissionEventTrackers
{
    public class GlobalMissionNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        private const float AnimationSpeed = 1f;
        private const float AnimationAmplitude = 4f;
        public Texture2D missionAvailableTexture = ModContent.Request<Texture2D>($"{Assets.UI.MissionUI}MissionObjectives").Value;

        private void UpdateMissionProgress(MissionPlayer missionPlayer)
        {
            Mission Reawakening = missionPlayer.GetMission(MissionID.Reawakening);
            if (Reawakening != null && Reawakening.Progress == MissionProgress.Active)
            {
                var currentSet = Reawakening.MissionData.ObjectiveSets[Reawakening.CurrentSetIndex];

                if (!currentSet.IsCompleted)
                {
                    if (Reawakening.CurrentSetIndex == 0 && !currentSet.Objectives[0].IsCompleted)
                        Reawakening.UpdateProgress(0);

                    if (Reawakening.CurrentSetIndex == 2)
                        Reawakening.UpdateProgress(1);

                    if (Reawakening.CurrentSetIndex == 4 &&
                        currentSet.Objectives[0].IsCompleted && currentSet.Objectives[1].IsCompleted)
                        Reawakening.UpdateProgress(2);

                }
            }
        }

        private static void UpdateMissionProgress(NPC npc)
        {
            MissionPlayer player = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
            Mission Reawakening = player.GetMission(MissionID.Reawakening);
            Mission RedEyedRetribution = player.GetMission(MissionID.Reawakening);

            if (Reawakening != null && Reawakening.Progress == MissionProgress.Active)
            {
                if (Reawakening.CurrentSetIndex == 3)
                {
                    if (npc.aiStyle == NPCAIStyleID.Slime)
                    {
                        Reawakening.UpdateProgress(0);
                    }
                }
            }

            if (RedEyedRetribution != null && RedEyedRetribution.Progress == MissionProgress.Active)
            {
                if (RedEyedRetribution.CurrentSetIndex == 3)
                {
                    if (npc.aiStyle == NPCAIStyleID.EyeOfCthulhu || NPC.downedBoss1)
                    {
                        RedEyedRetribution.UpdateProgress(0);
                    }
                }
            }
        }

        public override bool? CanChat(NPC npc)
            => (npc.isLikeATownNPC || npc.townNPC) && !DialogueManager.Instance.IsAnyDialogueActive();

        public override void GetChat(NPC npc, ref string chat)
        {
            var player = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
            UpdateMissionProgress(player);
        }

        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            MissionPlayer mPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
            Mission Reawakening = mPlayer.GetMission(MissionID.Reawakening);

            if (Reawakening != null && Reawakening.Progress == MissionProgress.Active)
            {
                var currentSet = Reawakening.MissionData.ObjectiveSets[Reawakening.CurrentSetIndex];
                if (!currentSet.IsCompleted)
                {
                    if (Reawakening.CurrentSetIndex < 3)
                    {
                        pool.Clear();
                    }
                }
            }
            if (SubworldSystem.IsActive<ArchaeaSubworld>())
            {
                pool.Clear();
                pool.Add(ModContent.NPCType<Scarab>(), 0.7f);
            }
            else
                base.EditSpawnPool(pool, spawnInfo);
        }

        public override void AI(NPC npc)
        {
            base.AI(npc);
            HandleDialogueMovement(npc);
        }

        public override bool CheckActive(NPC npc)
        {
            if (NPC.AnyNPCs(NPCID.Merchant))
            {
                MissionPlayer missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();

                Mission dirtiestBlockMission = missionPlayer.GetMission(MissionID.DirtiestBlock);
                if (dirtiestBlockMission != null && dirtiestBlockMission.State == MissionState.Locked)
                {
                    dirtiestBlockMission.State = MissionState.Unlocked;
                    missionPlayer.AssignMissionToNPC(NPCID.Merchant, MissionID.DirtiestBlock);

                }
            }
            return base.CheckActive(npc);
        }

        private void HandleDialogueMovement(NPC npc)
        {
            static bool IsNPCInActiveDialogue(NPC npc) // i know what you're thinking
            {
                npc.immortal = true;
                var activeDialogue = DialogueManager.Instance.GetActiveDialogue();
                if (activeDialogue != null)
                {
                    return activeDialogue.npcData.NpcID == npc.type;
                }
                return false;
            }

            if (!IsNPCInActiveDialogue(npc))
            {
                npc.immortal = false;
                return;
            }
            npc.velocity = Vector2.Zero;

            Player player = Main.player[Main.myPlayer];
            if (player.Center.X < npc.Center.X)
                npc.direction = -1;

            else
                npc.direction = 1;

            npc.spriteDirection = npc.direction;

            npc.frameCounter++;
            if (npc.frameCounter > 20)
            {
                npc.frame.Y = (npc.frame.Y + npc.frame.Height) % (npc.frame.Height * 2);
                npc.frameCounter = 0;
            }
        }

        public override void OnKill(NPC npc)
        {
            base.OnKill(npc);
            UpdateMissionProgress(npc);     
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
            if (NPCHasAvailableMission(missionPlayer, npc.type))
            {
                float hoverOffset = (float)Math.Sin(Main.GameUpdateCount * AnimationSpeed * 0.1f) * AnimationAmplitude;
                Vector2 drawPos = npc.Top + new Vector2(-missionAvailableTexture.Width / 2f, -missionAvailableTexture.Height - 10f - hoverOffset);
                drawPos = Vector2.Transform(drawPos - Main.screenPosition, Main.GameViewMatrix.ZoomMatrix);
                spriteBatch.Draw(
                    missionAvailableTexture,
                    drawPos,
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    1f,
                    SpriteEffects.None,
                    0f
                );
            }
            base.PostDraw(npc, spriteBatch, screenPos, drawColor);
        }

        private static bool NPCHasAvailableMission(MissionPlayer missionPlayer, int npcType)
        {
            if (missionPlayer.npcMissionsDict.TryGetValue(npcType, out var missionIds))
            {
                foreach (var missionId in missionIds)
                {
                    var mission = missionPlayer.GetMission(missionId);
                    if (mission != null && mission.State == MissionState.Unlocked && mission.Progress != MissionProgress.Completed)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}