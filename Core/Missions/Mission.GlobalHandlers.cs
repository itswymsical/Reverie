using Reverie.Core.Dialogue;
using Reverie.Core.Indicators;
using Reverie.Utilities;
using Reverie.Utilities.Extensions;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;

namespace Reverie.Core.Missions;

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
        if (MissionUtils.TryUpdateProgressForItem(item, player))
        {
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
        return (npc.friendly && !npc.CountsAsACritter || npc.CanBeTalkedTo || npc.townNPC) && !DialogueManager.Instance.IsAnyActive();
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
        {
            OnNPCKill?.Invoke(npc);
        }
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
    /// <summary>
    /// Delegate for when a player enters a biome and meets time requirements.
    /// </summary>
    /// <param name="player">The player</param>
    /// <param name="biome">The current biome</param>
    /// <param name="timeSpent">Time in seconds</param>
    public delegate void BiomeEnterHandler(Player player, BiomeType biome, int timeSpent);
    public static event BiomeEnterHandler OnBiomeEnter;

    private readonly Dictionary<BiomeType, int> biomeTimers = new();
    private readonly Dictionary<BiomeType, HashSet<int>> triggeredTimeRequirements = new();
    private BiomeType? currentBiome = null;
    private readonly List<int> activeTimeRequirements = new() { 5 * 60 };

    public override void PostUpdate()
    {
        base.PostUpdate();
        TrackBiomeTime();
    }

    private void TrackBiomeTime()
    {
        var playerBiome = GetCurrentBiome();

        if (currentBiome != playerBiome)
        {
            ResetAllTimers();
            currentBiome = playerBiome;
        }

        if (currentBiome.HasValue)
        {
            if (!biomeTimers.ContainsKey(currentBiome.Value))
                biomeTimers[currentBiome.Value] = 0;

            biomeTimers[currentBiome.Value]++;

            foreach (var timeReq in activeTimeRequirements)
            {
                if (biomeTimers[currentBiome.Value] == timeReq)
                {
                    if (!triggeredTimeRequirements.ContainsKey(currentBiome.Value))
                        triggeredTimeRequirements[currentBiome.Value] = new HashSet<int>();

                    if (!triggeredTimeRequirements[currentBiome.Value].Contains(timeReq))
                    {
                        triggeredTimeRequirements[currentBiome.Value].Add(timeReq);
                        OnBiomeEnter?.Invoke(Player, currentBiome.Value, timeReq);
                    }
                }
            }
        }
    }

    private BiomeType? GetCurrentBiome()
    {
        foreach (var biome in Enum.GetValues<BiomeType>())
        {
            if (biome.IsPlayerInBiome(Player))
            {
                return biome;
            }
        }
        return null;
    }

    private void ResetAllTimers()
    {
        biomeTimers.Clear();
        triggeredTimeRequirements.Clear();
    }

    public int GetBiomeTime(BiomeType biome)
    {
        return biomeTimers.TryGetValue(biome, out var time) ? time : 0;
    }

    public void AddTimeRequirement(int ticks)
    {
        if (!activeTimeRequirements.Contains(ticks))
            activeTimeRequirements.Add(ticks);
    }

    public void RemoveTimeRequirement(int ticks)
    {
        activeTimeRequirements.Remove(ticks);
    }

    public void ClearTimeRequirements()
    {
        activeTimeRequirements.Clear();
    }
}