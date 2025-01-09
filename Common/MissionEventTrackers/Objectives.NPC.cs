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
using Terraria.GameContent.UI;
using Reverie.Common.Extensions;


namespace Reverie.Common.MissionEventTrackers
{
    public class GlobalMissionNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public Texture2D missionAvailableTexture = ModContent.Request<Texture2D>($"{Assets.UI.MissionUI}MissionObjectives").Value;

        private static void UpdateMissionProgress(MissionPlayer missionPlayer, NPC npc)
        {
            Mission Reawakening = missionPlayer.GetMission(MissionID.Reawakening);
            if (Reawakening != null && Reawakening.Progress == MissionProgress.Active)
            {
                var currentSet = Reawakening.MissionData.ObjectiveSets[Reawakening.CurrentSetIndex];

                if (!currentSet.IsCompleted)
                {
                    if (Reawakening.CurrentSetIndex == 0 && !currentSet.Objectives[0].IsCompleted)
                        Reawakening.UpdateProgress(0);

                    if (npc.type == NPCID.Guide)
                    {
                        if (Reawakening.CurrentSetIndex == 2)
                            Reawakening.UpdateProgress(1);
                    }

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
            base.GetChat(npc, ref chat);
            var player = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
            UpdateMissionProgress(player, npc);
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
            npc.TownNPC_TalkState();
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
        public override void OnKill(NPC npc)
        {
            base.OnKill(npc);
            UpdateMissionProgress(npc);     
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
            if (npc.NPCHasAvailableMission(missionPlayer, npc.type))
            {
                float hoverOffset = (float)Math.Sin(Main.GameUpdateCount * 1f * 0.1f) * 4f;
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
    }
}