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
using Reverie.Common.UI;

namespace Reverie.Common.MissionEventTrackers
{
    public class ObjectiveHandlerItem : GlobalItem
    {
        public override void OnCreated(Item item, ItemCreationContext context)
        {
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
            ModContent.GetInstance<MissionUISystem>().ShowMissionInterface(npc.type);
            MissionHandlerManager.Instance.OnNPCChat(npc);
        }

        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            MissionPlayer mPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
            Mission CrashLanding = mPlayer.GetMission(MissionID.CrashLanding);

            if (CrashLanding?.Progress == MissionProgress.Active)
            {
                var currentSet = CrashLanding.ObjectiveSets[CrashLanding.CurrentSetIndex];
                if (!currentSet.IsCompleted && CrashLanding.CurrentSetIndex < 1)
                {
                    pool.Clear();
                }
                if (CrashLanding.CurrentSetIndex == 2)
                {
                    pool.Add(NPCID.GreenSlime, 0.2f);
                    pool.Add(NPCID.BlueSlime, 0.2f);
                    pool.Add(NPCID.PurpleSlime, 0.1f);
                }
                else if (CrashLanding.CurrentSetIndex == 5)
                {
                    pool.Add(NPCID.GreenSlime, 0.27f);
                    pool.Add(NPCID.BlueSlime, 0.27f);
                    pool.Add(NPCID.YellowSlime, 0.21f);
                    pool.Add(NPCID.PurpleSlime, 0.13f);
                    pool.Add(NPCID.RedSlime, 0.11f);
                    pool.Add(NPCID.YellowSlime, 0.11f);
                    pool.Add(NPCID.Pinky, 0.03f);
                }
                else if (CrashLanding.CurrentSetIndex == 7)
                {
                    pool.Add(NPCID.GreenSlime, 0.27f);
                    pool.Add(NPCID.BlueSlime, 0.27f);
                    pool.Add(NPCID.YellowSlime, 0.24f);
                    pool.Add(NPCID.PurpleSlime, 0.19f);
                    pool.Add(NPCID.RedSlime, 0.17f);
                    pool.Add(NPCID.YellowSlime, 0.17f);
                    pool.Add(NPCID.Pinky, 0.07f);
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