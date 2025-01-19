using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;

using Reverie.Common.Players;
using Reverie.Common.Systems.Subworlds.Archaea;
using Reverie.Common.Extensions;
using Reverie.Common.MissionAttributes;
using Reverie.Content.Archaea.NPCs.Surface;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;

using SubworldLibrary;
using static Reverie.Common.Players.MissionPlayer;

namespace Reverie.Common.MissionEventTrackers
{
    public class ObjectiveHandlerItem : GlobalItem
    {
        public override void OnCreated(Item item, ItemCreationContext context)
        {
            Main.NewText($"Item Created: {item.Name} (Type: {item.type})"); // Debug log
            MissionHandlerManager.Instance.OnItemCreated(item, context);
            base.OnCreated(item, context);
        }

        public override bool OnPickup(Item item, Player player)
        {
            MissionHandlerManager.Instance.OnItemPickup(item);
            return base.OnPickup(item, player);
        }
    }

    public class ObjectiveHandlerNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public Texture2D missionAvailableTexture = 
            ModContent.Request<Texture2D>($"{Assets.UI.MissionUI}MissionObjectives").Value;

        public override bool? CanChat(NPC npc)
            => (npc.isLikeATownNPC || npc.townNPC) && !DialogueManager.Instance.IsAnyActive();

        public override void GetChat(NPC npc, ref string chat)
        {
            base.GetChat(npc, ref chat);
            MissionHandlerManager.Instance.OnNPCChat(npc);
        }

        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            MissionPlayer mPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
            Mission Reawakening = mPlayer.GetMission(MissionID.Reawakening);

            if (Reawakening?.Progress == MissionProgress.Active)
            {
                var currentSet = Reawakening.MissionData.ObjectiveSets[Reawakening.CurrentSetIndex];
                if (!currentSet.IsCompleted && Reawakening.CurrentSetIndex < 2)
                {
                    pool.Clear();
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
            if (npc.isLikeATownNPC)
                npc.TownNPC_TalkState();
        }

        public override bool CheckActive(NPC npc)
        {
            if (npc.immortal)
                return false;

            if (NPC.AnyNPCs(NPCID.Merchant))
            {
                MissionPlayer missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
                Mission dirtiestBlockMission = missionPlayer.GetMission(MissionID.DirtiestBlock);

                if (dirtiestBlockMission?.State == MissionState.Locked)
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
            if (!npc.immortal)
                MissionHandlerManager.Instance.OnNPCKill(npc);   
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
            if (NPCHasAvailableMission(missionPlayer, npc.type))
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