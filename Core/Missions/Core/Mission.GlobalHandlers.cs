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
        // Fire event for ANY player who picks up an item
        if (MissionUtils.TryUpdateProgressForItem(item, player))
        {
            OnItemPickup?.Invoke(item, player);
        }

        return base.OnPickup(item, player);
    }

    public override bool? UseItem(Item item, Player player)
    {
        // Fire event for ANY player who uses an item
        OnItemUse?.Invoke(item, player);
        return base.UseItem(item, player);
    }

    public override void UpdateEquip(Item item, Player player)
    {
        base.UpdateEquip(item, player);
        // Fire event for ANY player who has this item equipped
        if (MissionUtils.TryUpdateProgressForItem(item, player))
        {
            OnItemUpdate?.Invoke(item, player);
        }
    }

    public override void UpdateInventory(Item item, Player player)
    {
        base.UpdateInventory(item, player);

        // Fire event for ANY player who has this item in inventory
        if (MissionUtils.TryUpdateProgressForItem(item, player))
        {
            OnItemUpdate?.Invoke(item, player);
        }
    }

    public override void OnConsumeItem(Item item, Player player)
    {
        base.OnConsumeItem(item, player);
        // Fire event for ANY player who consumes an item
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
    public delegate void NPCKillHandler(NPC npc, Player player);
    public delegate void NPCHitHandler(NPC npc, Player player, int damage);
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
            // Check if ANY player has available missions for this NPC
            bool anyPlayerHasMission = false;
            Mission availableMission = null;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                var player = Main.player[i];
                if (player?.active == true)
                {
                    var missionPlayer = player.GetModPlayer<MissionPlayer>();
                    if (missionPlayer.NPCHasAvailableMission(npc.type))
                    {
                        availableMission = missionPlayer.AvailableMissions().FirstOrDefault(m => npc.type == m.ProviderNPC);
                        anyPlayerHasMission = true;
                        break;
                    }
                }
            }

            if (anyPlayerHasMission && availableMission != null && npc.active)
            {
                ScreenIndicatorManager.Instance.CreateMissionIndicatorForNPC(npc, availableMission);
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
            // Find which player killed this NPC
            Player killingPlayer = GetPlayerWhoKilledNPC(npc);
            OnNPCKill?.Invoke(npc, killingPlayer);
        }
    }

    public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitByItem(npc, player, item, hit, damageDone);
        // Pass the specific player who hit the NPC
        OnNPCHit?.Invoke(npc, player, damageDone);
    }

    public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitByProjectile(npc, projectile, hit, damageDone);
        // Find the player who owns this projectile
        Player ownerPlayer = GetProjectileOwner(projectile);
        OnNPCHit?.Invoke(npc, ownerPlayer, damageDone);
    }

    public override void OnCaughtBy(NPC npc, Player player, Item item, bool failed)
    {
        base.OnCaughtBy(npc, player, item, failed);
        // Pass the specific player who caught the NPC
        OnNPCCatch?.Invoke(npc, player, item, failed);
    }

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        base.OnSpawn(npc, source);
        OnNPCSpawn?.Invoke(npc);
    }

    /// <summary>
    /// Helper method to determine which player killed an NPC
    /// </summary>
    private Player GetPlayerWhoKilledNPC(NPC npc)
    {
        // Check if any player is close enough to have killed this NPC
        // This is a heuristic since we don't have direct access to who dealt the killing blow
        float closestDistance = float.MaxValue;
        Player closestPlayer = null;

        for (int i = 0; i < Main.maxPlayers; i++)
        {
            var player = Main.player[i];
            if (player?.active == true)
            {
                float distance = Vector2.Distance(player.Center, npc.Center);
                if (distance < closestDistance && distance < 1000f) // Within reasonable range
                {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }
        }

        return closestPlayer ?? Main.player[Main.myPlayer]; // Fallback to local player
    }

    /// <summary>
    /// Helper method to get the owner of a projectile
    /// </summary>
    private Player GetProjectileOwner(Projectile projectile)
    {
        if (projectile.owner >= 0 && projectile.owner < Main.maxPlayers)
        {
            return Main.player[projectile.owner];
        }
        return Main.player[Main.myPlayer]; // Fallback
    }
}

public class ObjectiveEventTile : GlobalTile
{
    public delegate void TileBreakHandler(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem);
    public delegate void TilePlaceHandler(int i, int j, int type);
    public delegate void TileContactHandler(int type, Player player);
    public delegate void TileInteractHandler(int i, int j, int type); // dunno if we need a player check for tile interactions

    public static event TileBreakHandler OnTileBreak;
    public static event TilePlaceHandler OnTilePlace;
    public static event TileContactHandler OnTileContact;
    public static event TileInteractHandler OnTileInteract;

    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        base.KillTile(i, j, type, ref fail, ref effectOnly, ref noItem);
        // Find which player is breaking this tile
        Player breakingPlayer = GetPlayerNearTile(i, j);
        OnTileBreak?.Invoke(i, j, type, ref fail, ref effectOnly, ref noItem);
    }

    public override void PlaceInWorld(int i, int j, int type, Item item)
    {
        base.PlaceInWorld(i, j, type, item);
        // Find which player placed this tile
        Player placingPlayer = GetPlayerNearTile(i, j);
        OnTilePlace?.Invoke(i, j, type);
    }

    public override void FloorVisuals(int type, Player player)
    {
        base.FloorVisuals(type, player);
        // Pass the specific player who is standing on this tile
        OnTileContact?.Invoke(type, player);
    }

    public override void RightClick(int i, int j, int type)
    {
        base.RightClick(i, j, type);
        // Find which player right-clicked this tile
        Player interactingPlayer = GetPlayerNearTile(i, j);
        OnTileInteract?.Invoke(i, j, type);
    }

    /// <summary>
    /// Helper method to find which player is closest to a tile position
    /// </summary>
    private Player GetPlayerNearTile(int i, int j)
    {
        Vector2 tilePosition = new Vector2(i * 16, j * 16);
        float closestDistance = float.MaxValue;
        Player closestPlayer = null;

        for (int p = 0; p < Main.maxPlayers; p++)
        {
            var player = Main.player[p];
            if (player?.active == true)
            {
                float distance = Vector2.Distance(player.Center, tilePosition);
                if (distance < closestDistance && distance < 320f) // Within reasonable tile interaction range
                {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }
        }

        return closestPlayer ?? Main.player[Main.myPlayer]; // Fallback to local player
    }
}

public class ObjectiveEventPlayer : ModPlayer
{
    /// <summary>
    /// Delegate for when a player enters a biome and meets time requirements.
    /// </summary>
    /// <param name="player">the player who entered the biome</param>
    /// <param name="biome">the current biome</param>
    /// <param name="timeSpent">time in seconds</param>
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
        BiomeType? playerBiome = GetCurrentBiome();

        // Reset timer if player changed biomes or left all biomes
        if (currentBiome != playerBiome)
        {
            ResetAllTimers();
            currentBiome = playerBiome;
        }

        // If player is in a biome, increment its timer
        if (currentBiome.HasValue)
        {
            if (!biomeTimers.ContainsKey(currentBiome.Value))
                biomeTimers[currentBiome.Value] = 0;

            biomeTimers[currentBiome.Value]++;

            // Check all active time requirements
            foreach (int timeReq in activeTimeRequirements)
            {
                if (biomeTimers[currentBiome.Value] == timeReq)
                {
                    // Ensure we only fire once per time requirement
                    if (!triggeredTimeRequirements.ContainsKey(currentBiome.Value))
                        triggeredTimeRequirements[currentBiome.Value] = new HashSet<int>();

                    if (!triggeredTimeRequirements[currentBiome.Value].Contains(timeReq))
                    {
                        triggeredTimeRequirements[currentBiome.Value].Add(timeReq);
                        // Pass THIS specific player to the event handler
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
        return biomeTimers.TryGetValue(biome, out int time) ? time : 0;
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