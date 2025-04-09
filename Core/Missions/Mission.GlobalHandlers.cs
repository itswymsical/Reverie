using Reverie.Core.CustomEntities;
using Reverie.Core.Dialogue;
using Reverie.Utilities;
using Reverie.Utilities.Extensions;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;

namespace Reverie.Core.Missions;

public partial class MissionPlayer
{
    public void PlayerTriggerEvents()
    {
        bool merchantPresent = NPC.AnyNPCs(NPCID.Merchant);
        var copperStandard = GetMission(MissionID.CopperStandard);
        if (copperStandard.Availability == MissionAvailability.Locked && copperStandard.Progress == MissionProgress.Inactive
            && merchantPresent && Player.HasItemInAnyInventory(ItemID.CopperBar))
        {
            UnlockMission(MissionID.CopperStandard);
        }
    }
}

public class ObjectiveEventItem : GlobalItem
{
    public delegate void ItemCreatedHandler(Item item, ItemCreationContext context);
    public delegate void ItemPickupHandler(Item item, Player player);
    public delegate void ItemUpdateHandler(Item item, Player player);

    public static event ItemCreatedHandler OnItemCreated;
    public static event ItemPickupHandler OnItemPickup;
    public static event ItemUpdateHandler OnItemUpdate;

    public override void OnCreated(Item item, ItemCreationContext context)
    {
        OnItemCreated?.Invoke(item, context);
        base.OnCreated(item, context);
    }

    public override bool OnPickup(Item item, Player player)
    {
        // Only fire the event if the item hasn't contributed yet and might be relevant
        if (MissionUtils.TryUpdateProgressForItem(item, player))
        {
            // The item is relevant and hasn't contributed yet, so fire the event
            OnItemPickup?.Invoke(item, player);
        }

        return base.OnPickup(item, player);
    }

    public override void UpdateEquip(Item item, Player player)
    {
        base.UpdateEquip(item, player);
        OnItemUpdate?.Invoke(item, player);
    }

    public override void UpdateInventory(Item item, Player player)
    {
        if (MissionUtils.TryUpdateProgressForItem(item, player))
        {
            // The item is relevant and hasn't contributed yet, so fire the event
            OnItemUpdate?.Invoke(item, player);
        }
        base.UpdateInventory(item, player);
    }
}

public class ObjectiveEventNPC : GlobalNPC
{
    public delegate void NPCChatHandler(NPC npc, ref string chat);
    public delegate void NPCKillHandler(NPC npc);
    public delegate void NPCHitHandler(NPC npc, int damage);
    public delegate void NPCSpawnHandler(NPC npc);

    public static event NPCChatHandler OnNPCChat;
    public static event NPCKillHandler OnNPCKill;
    public static event NPCHitHandler OnNPCHit;
    public static event NPCSpawnHandler OnNPCSpawn;

    public override bool InstancePerEntity => true;

    public override bool? CanChat(NPC npc) => (npc.isLikeATownNPC || npc.townNPC)
        && !DialogueManager.Instance.IsAnyActive();

    public override void GetChat(NPC npc, ref string chat)
    {
        base.GetChat(npc, ref chat);
        OnNPCChat?.Invoke(npc, ref chat);
    }

    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        var mPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
        var AFallingStar = mPlayer.GetMission(MissionID.AFallingStar);

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

        var AFallingStar = mPlayer.GetMission(MissionID.AFallingStar);

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
        {
            npc.ForceBubbleChatState();

            var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();

            if (missionPlayer.NPCHasAvailableMission(npc.type))
            {
                var mission = missionPlayer.AvailableMissions().FirstOrDefault(m => npc.type == m.ProviderNPC);

                if (mission == null)
                    return;

                MissionIndicatorManager.Instance.CreateIndicatorForNPC(npc, mission);
            }
        }
    }

    public override void OnKill(NPC npc)
    {
        base.OnKill(npc);

        if (!npc.immortal)
            OnNPCKill?.Invoke(npc);
    }

    public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitByItem(npc, player, item, hit, damageDone);
        OnNPCHit?.Invoke(npc, damageDone);
    }
    public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitByProjectile(npc, projectile, hit, damageDone);
        OnNPCHit?.Invoke(npc, damageDone);
    }

}

public class ObjectiveEventTile : GlobalTile
{
    public delegate void TileBreakHandler(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem);

    public static event TileBreakHandler OnTileBreak;

    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        base.KillTile(i, j, type, ref fail, ref effectOnly, ref noItem);
        OnTileBreak?.Invoke(i, j, type, ref fail, ref effectOnly, ref noItem);
    }
}

public class ObjectiveEventPlayer : ModPlayer
{
    public delegate void BiomeEnterHandler(Player player, BiomeType biome);

    public static event BiomeEnterHandler OnBiomeEnter;

    public override void PostUpdate()
    {
        base.PostUpdate();

        foreach (var biome in Enum.GetValues<BiomeType>())
        {
            if (biome.IsPlayerInBiome(Player))
            {
                OnBiomeEnter?.Invoke(Player, biome);
            }
        }
    }
}