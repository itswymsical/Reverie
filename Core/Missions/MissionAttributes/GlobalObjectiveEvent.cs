using System.Collections.Generic;

using Terraria.DataStructures;

using Reverie.Core.Dialogue;
using Reverie.Utilities.Extensions;
using static Reverie.Core.Missions.MissionPlayer;
using Reverie.Core.Missions.MissionHandlers;

namespace Reverie.Core.Missions.MissionAttributes;

public class ObjectiveEventItem : GlobalItem
{
    public override void OnCreated(Item item, ItemCreationContext context)
    {
        MissionHandlerManager.Instance.OnItemCreated(item, context);
        base.OnCreated(item, context);
    }

    public override bool OnPickup(Item item, Player player)
    {
        MissionProgressHelper.TryUpdateProgressForItem(item, player);
        return base.OnPickup(item, player);
    }

    public override void UpdateEquip(Item item, Player player)
    {
        base.UpdateEquip(item, player);
        MissionProgressHelper.TryUpdateProgressForItem(item, player);
    }
    public override void UpdateInventory(Item item, Player player)
    {
        MissionProgressHelper.TryUpdateProgressForItem(item, player);
        base.UpdateInventory(item, player);
    }
}

public class ObjectiveEventNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public Texture2D missionAvailableTexture =
        ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Missions/MissionAvailable").Value;

    public override bool? CanChat(NPC npc)
        => (npc.isLikeATownNPC || npc.townNPC) && !DialogueManager.Instance.IsAnyActive();

    public override void GetChat(NPC npc, ref string chat)
    {
        base.GetChat(npc, ref chat);

        MissionHandlerManager.Instance.OnNPCChat(npc);
    }

    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        var mPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
        Mission AFallingStar = mPlayer.GetMission(MissionID.AFallingStar);

        if (AFallingStar?.Progress == MissionProgress.Active)
        {
            var rateWeakSlimes = 0.1f;
            var rateStrongSlimes = 0.04f;
            var rateRareSlimes = 0.002f;

            var currentSet = AFallingStar.ObjectiveIndex[AFallingStar.CurObjectiveIndex];

            if (!currentSet.IsCompleted && AFallingStar.CurObjectiveIndex < 2)
                pool.Clear();
            
            if (spawnInfo.PlayerInTown)
            {
                if (AFallingStar.CurObjectiveIndex >= 3 && Main.LocalPlayer.ZoneOverworldHeight)
                {
                    pool.Add(NPCID.GreenSlime, rateWeakSlimes);
                    pool.Add(NPCID.BlueSlime, rateWeakSlimes);
                    pool.Add(NPCID.PurpleSlime, rateStrongSlimes);
                    pool.Add(NPCID.RedSlime, rateRareSlimes);
                    pool.Add(NPCID.YellowSlime, rateRareSlimes);
                    pool.Add(NPCID.Pinky, rateRareSlimes);
                    pool.Add(NPCID.MotherSlime, rateRareSlimes);

                }
                else if (AFallingStar.CurObjectiveIndex >= 6 && Main.LocalPlayer.ZoneOverworldHeight)
                {
                    rateWeakSlimes = 0.17f;
                    rateStrongSlimes = 0.09f;
                    rateRareSlimes = 0.008f;
                }
                else if (AFallingStar.CurObjectiveIndex >= 9 && AFallingStar.CurObjectiveIndex <= 10
                    && Main.LocalPlayer.ZoneOverworldHeight)
                {
                    rateWeakSlimes = 0.2f;
                    rateStrongSlimes = 0.11f;
                    rateRareSlimes = 0.02f;
                }
            }
        }
        else
            base.EditSpawnPool(pool, spawnInfo);
    }
    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        var mPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();

        Mission AFallingStar = mPlayer.GetMission(MissionID.AFallingStar);

        if (AFallingStar?.Progress == MissionProgress.Active && player.ZoneOverworldHeight)
        {
            var currentSet = AFallingStar.ObjectiveIndex[AFallingStar.CurObjectiveIndex];
            if (AFallingStar.CurObjectiveIndex >= 3)
            {
                spawnRate = 3;
                maxSpawns = 8;
            }
            if (AFallingStar.CurObjectiveIndex >= 5)
            {
                spawnRate = 2;
                maxSpawns = 9;
            }
            else if (AFallingStar.CurObjectiveIndex >= 7)
            {
                spawnRate = 4;
                maxSpawns = 10;
            }
            else if (AFallingStar.CurObjectiveIndex >= 9)
            {
                spawnRate = 5;
                maxSpawns = 15;
            }
        }
        else
            base.EditSpawnRate(player, ref spawnRate, ref maxSpawns);
    }
    public override void AI(NPC npc)
    {
        base.AI(npc);
        if (npc.isLikeATownNPC)
            npc.ForceBubbleChatState();
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
            var hoverOffset = (float)Math.Sin(Main.GameUpdateCount * 1f * 0.1f) * 4f;
            var drawPos = npc.Top + new Vector2(-missionAvailableTexture.Width / 2f, -missionAvailableTexture.Height - 10f - hoverOffset);
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