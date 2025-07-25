using Reverie.Core.Dialogue;
using Reverie.Core.Indicators;
using Reverie.Utilities;
using Reverie.Utilities.Extensions;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;

namespace Reverie.Core.Missions.Core;

public class ObjectiveEventItem : GlobalItem
{
    public delegate void ItemCreatedHandler(Item item, ItemCreationContext context);
    public delegate void ItemPickupHandler(Item item, Player player);
    public delegate void ItemUpdateHandler(Item item, Player player);
    public delegate void ItemConsumeHandler(Item item, Player player);
    public delegate void ItemUseHandler(Item item, Player player);

    public static event ItemCreatedHandler OnItemCreated;
    public static event ItemPickupHandler OnItemPickup;
    public static event ItemUpdateHandler OnItemUpdate;
    public static event ItemConsumeHandler OnItemConsume;
    public static event ItemUseHandler OnItemUse;

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

    public override bool? UseItem(Item item, Player player)
    {
        OnItemUse?.Invoke(item, player);
        return base.UseItem(item, player);
    }
    public override void UpdateEquip(Item item, Player player)
    {
        base.UpdateEquip(item, player);
        if (MissionUtils.TryUpdateProgressForItem(item, player))
        {
            OnItemUpdate?.Invoke(item, player);
        }
    }

    public override void UpdateInventory(Item item, Player player)
    {
        base.UpdateInventory(item, player);

        if (MissionUtils.TryUpdateProgressForItem(item, player))
        {
            OnItemUpdate?.Invoke(item, player);
        }
    }

    public override void OnConsumeItem(Item item, Player player)
    {
        base.OnConsumeItem(item, player);
        OnItemConsume?.Invoke(item, player);
    }

    public override void OnStack(Item destination, Item source, int numToTransfer)
    {
        base.OnStack(destination, source, numToTransfer);
    }
}

public class ObjectiveEventNPC : GlobalNPC
{
    public delegate void NPCChatHandler(NPC npc, ref string chat);
    public delegate void NPCKillHandler(NPC npc);
    public delegate void NPCHitHandler(NPC npc, int damage);
    public delegate void NPCSpawnHandler(NPC npc);
    public delegate void NPCCatchHandler(NPC npc, Player player, Item item, bool failed);

    public static event NPCChatHandler OnNPCChat;
    public static event NPCKillHandler OnNPCKill;
    public static event NPCHitHandler OnNPCHit;
    public static event NPCSpawnHandler OnNPCSpawn;
    public static event NPCCatchHandler OnNPCCatch;
    public override bool InstancePerEntity => true;

    public override bool? CanChat(NPC npc)
    {
        return ((npc.friendly && !npc.CountsAsACritter) || npc.CanBeTalkedTo || npc.townNPC) && !DialogueManager.Instance.IsAnyActive();
    }

    public override void GetChat(NPC npc, ref string chat)
    {
        base.GetChat(npc, ref chat);
        OnNPCChat?.Invoke(npc, ref chat);
    }

    public override void AI(NPC npc)
    {
        base.AI(npc);

        if (npc.isLikeATownNPC)
        {
            //npc.ForceBubbleChatState();

            var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();

            if (missionPlayer.NPCHasAvailableMission(npc.type) && npc.active)
            {
                var mission = missionPlayer.AvailableMissions().FirstOrDefault(m => npc.type == m.ProviderNPC);

                if (mission == null)
                    return;

                ScreenIndicatorManager.Instance.CreateMissionIndicatorForNPC(npc, mission);
            }
            else
            {
                ScreenIndicatorManager.Instance.RemoveIndicatorForNPC(npc.whoAmI);
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

    public override void OnCaughtBy(NPC npc, Player player, Item item, bool failed)
    {
        base.OnCaughtBy(npc, player, item, failed);
        OnNPCCatch?.Invoke(npc, player, item, failed);
    }

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        base.OnSpawn(npc, source);
        OnNPCSpawn?.Invoke(npc);
    }
}

public class ObjectiveEventTile : GlobalTile
{
    public delegate void TileBreakHandler(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem);
    public delegate void TilePlaceHandler(int i, int j, int type);
    public delegate void TileContactHandler(int type, Player player);
    public delegate void TileInteractHandler(int i, int j, int type);

    public static event TileBreakHandler OnTileBreak;
    public static event TilePlaceHandler OnTilePlace;
    public static event TileContactHandler OnTileContact;
    public static event TileInteractHandler OnTileInteract;
    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        base.KillTile(i, j, type, ref fail, ref effectOnly, ref noItem);
        OnTileBreak?.Invoke(i, j, type, ref fail, ref effectOnly, ref noItem);
    }
    public override void PlaceInWorld(int i, int j, int type, Item item)
    {
        base.PlaceInWorld(i, j, type, item);
        OnTilePlace?.Invoke(i, j, type);
    }
    public override void FloorVisuals(int type, Player player)
    {
        base.FloorVisuals(type, player);
        OnTileContact?.Invoke(type, player);
    }
    public override void RightClick(int i, int j, int type)
    {
        base.RightClick(i, j, type);
        OnTileInteract?.Invoke(i, j, type);
    }
}

public class ObjectiveEventPlayer : ModPlayer
{
    public delegate void BiomeEnterHandler(Player player, BiomeType biome);

    public static event BiomeEnterHandler OnBiomeEnter;
    private int timer = 0;

    public override void PostUpdate()
    {
        base.PostUpdate();
        //TriggerEvents();

        timer++;
        if (timer > 7 * 60)
        {
            foreach (var biome in Enum.GetValues<BiomeType>())
            {
                if (biome.IsPlayerInBiome(Player))
                {
                    OnBiomeEnter?.Invoke(Player, biome);
                }
            }
            timer = 0;
        }
    }

    private void TriggerEvents()
    {
        var p = Player.GetModPlayer<MissionPlayer>();
        //var merchantPresent = NPC.AnyNPCs(NPCID.Merchant);
        //var copperStandard = p.GetMission(MissionID.CopperStandard);
        //if (copperStandard.Status == MissionStatus.Locked && copperStandard.Progress == MissionProgress.Inactive
        //    && merchantPresent && Player.HasItemInAnyInventory(ItemID.CopperBar))
        //{
        //    p.UnlockMission(MissionID.CopperStandard, true);
        //}
    }
}