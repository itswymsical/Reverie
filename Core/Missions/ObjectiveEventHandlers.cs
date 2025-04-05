using System.Collections.Generic;

using Terraria.DataStructures;

using Reverie.Core.Dialogue;

using Reverie.Utilities;
using Reverie.Utilities.Extensions;
using System.Linq;

namespace Reverie.Core.Missions;

public class ObjectiveEventItem : GlobalItem
{
    public override void OnCreated(Item item, ItemCreationContext context)
    {
        MissionManager.Instance.OnItemCreated(item, context);
        base.OnCreated(item, context);
    }

    public override bool OnPickup(Item item, Player player)
    {
        MissionUtils.TryUpdateProgressForItem(item, player);
        return base.OnPickup(item, player);
    }

    public override void UpdateEquip(Item item, Player player)
    {
        base.UpdateEquip(item, player);
        MissionUtils.TryUpdateProgressForItem(item, player);
    }
    public override void UpdateInventory(Item item, Player player)
    {
        MissionUtils.TryUpdateProgressForItem(item, player);
        base.UpdateInventory(item, player);
    }
}

public class ObjectiveEventNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public Texture2D missionTexture =
        ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Missions/MissionAvailable").Value;

    public override bool? CanChat(NPC npc)
        => (npc.isLikeATownNPC || npc.townNPC) && !DialogueManager.Instance.IsAnyActive();

    public override void GetChat(NPC npc, ref string chat)
    {
        base.GetChat(npc, ref chat);

        MissionManager.Instance.OnNPCChat(npc);
    }

    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        var mPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
        var AFallingStar = mPlayer.GetMission(MissionID.A_FALLING_STAR);

        if (AFallingStar?.Progress == MissionProgress.Active)
        {
            var currentSet = AFallingStar.Objective[AFallingStar.CurrentIndex];
            if (spawnInfo.PlayerInTown && AFallingStar.CurrentIndex >= 5)
            {
                if (AFallingStar.CurrentIndex >= 3 && Main.LocalPlayer.ZoneOverworldHeight)
                {
                    pool.Add(NPCID.BlueSlime, 0.08f);
                    pool.Add(NPCID.GreenSlime, 0.102f);
                }
            }
        }
        else
            base.EditSpawnPool(pool, spawnInfo);
    }

    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        var mPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();

        var AFallingStar = mPlayer.GetMission(MissionID.A_FALLING_STAR);

        if (AFallingStar?.Progress == MissionProgress.Active && player.ZoneOverworldHeight)
        {
            var currentSet = AFallingStar.Objective[AFallingStar.CurrentIndex];
            if (AFallingStar.CurrentIndex >= 4)
            {
                spawnRate = 3;
                maxSpawns = 7;
            }
            if (AFallingStar.CurrentIndex >= 5)
            {
                spawnRate = 2;
                maxSpawns = 9;
            }
            else if (AFallingStar.CurrentIndex >= 9)
            {
                spawnRate = 2;
                maxSpawns = 11;
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
            MissionManager.Instance.OnNPCKill(npc);
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
        if (missionPlayer.NPCHasAvailableMission(npc.type) && !missionPlayer.ActiveMissions().Any(m => npc.type == m.Employer))
        {
            var hoverOffset = (float)Math.Sin(Main.GameUpdateCount * 1f * 0.1f) * 4f;
            var drawPos = npc.Top + new Vector2(-missionTexture.Width / 2f, -missionTexture.Height - 10f - hoverOffset);
            drawPos = Vector2.Transform(drawPos - Main.screenPosition, Main.GameViewMatrix.ZoomMatrix);
            spriteBatch.Draw(
                missionTexture,
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