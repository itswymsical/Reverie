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
        // Use the new mission utils method that handles mainline vs regular missions
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

    public override void OnStack(Item destination, Item source, int numToTransfer)
    {
        base.OnStack(destination, source, numToTransfer);
    }
}

public class ObjectiveEventNPC : GlobalNPC
{
    public delegate void NPCChatHandler(NPC npc, ref string chat);
    public delegate void NPCKillHandler(NPC npc, Player killingPlayer);
    public delegate void NPCHitHandler(NPC npc, Player hittingPlayer, int damage);
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
            // Find the player who killed the NPC
            Player killingPlayer = Main.LocalPlayer; // Default fallback

            // Try to find the actual killing player (this is simplified - you might need better logic)
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                var player = Main.player[i];
                if (player.active && Vector2.Distance(player.Center, npc.Center) < 1000f)
                {
                    killingPlayer = player;
                    break;
                }
            }

            OnNPCKill?.Invoke(npc, killingPlayer);
        }
    }

    public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitByItem(npc, player, item, hit, damageDone);
        OnNPCHit?.Invoke(npc, player, damageDone);
    }

    public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitByProjectile(npc, projectile, hit, damageDone);

        // Try to find the player who owns the projectile
        Player owner = Main.LocalPlayer; // Default fallback
        if (projectile.owner >= 0 && projectile.owner < Main.maxPlayers)
        {
            owner = Main.player[projectile.owner];
        }

        OnNPCHit?.Invoke(npc, owner, damageDone);
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
    public delegate void TileBreakHandler(int i, int j, int type, Player breakingPlayer, ref bool fail, ref bool effectOnly, ref bool noItem);
    public delegate void TilePlaceHandler(int i, int j, int type, Player placingPlayer);
    public delegate void TileContactHandler(int type, Player player);
    public delegate void TileInteractHandler(int i, int j, int type, Player interactingPlayer);

    public static event TileBreakHandler OnTileBreak;
    public static event TilePlaceHandler OnTilePlace;
    public static event TileContactHandler OnTileContact;
    public static event TileInteractHandler OnTileInteract;

    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        base.KillTile(i, j, type, ref fail, ref effectOnly, ref noItem);

        // Find the player who broke the tile (this is simplified)
        Player breakingPlayer = Main.LocalPlayer;
        for (int p = 0; p < Main.maxPlayers; p++)
        {
            var player = Main.player[p];
            if (player.active && Vector2.Distance(player.Center, new Vector2(i * 16, j * 16)) < 200f)
            {
                breakingPlayer = player;
                break;
            }
        }

        OnTileBreak?.Invoke(i, j, type, breakingPlayer, ref fail, ref effectOnly, ref noItem);
    }

    public override void PlaceInWorld(int i, int j, int type, Item item)
    {
        base.PlaceInWorld(i, j, type, item);

        // Find the player who placed the tile
        Player placingPlayer = Main.LocalPlayer;
        for (int p = 0; p < Main.maxPlayers; p++)
        {
            var player = Main.player[p];
            if (player.active && Vector2.Distance(player.Center, new Vector2(i * 16, j * 16)) < 200f)
            {
                placingPlayer = player;
                break;
            }
        }

        OnTilePlace?.Invoke(i, j, type, placingPlayer);
    }

    public override void FloorVisuals(int type, Player player)
    {
        base.FloorVisuals(type, player);
        OnTileContact?.Invoke(type, player);
    }

    public override void RightClick(int i, int j, int type)
    {
        base.RightClick(i, j, type);

        // Send packet to notify all players about the interaction
        if (Main.netMode == NetmodeID.MultiplayerClient || Main.netMode == NetmodeID.SinglePlayer)
        {
            // Send to server (which will broadcast to all clients)
            SendTileInteractPacket(i, j, type, Main.LocalPlayer.whoAmI);
        }

        // Handle locally as well
        HandleTileInteract(i, j, type, Main.LocalPlayer);
    }

    private void SendTileInteractPacket(int i, int j, int type, int playerWhoAmI)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            // Single player - just handle directly
            HandleTileInteract(i, j, type, Main.LocalPlayer);
            return;
        }

        ModPacket packet = ModContent.GetInstance<Reverie>().GetPacket();
        packet.Write((byte)MessageType.TileInteract); // Add this to your MessageType enum
        packet.Write(i);
        packet.Write(j);
        packet.Write(type);
        packet.Write(playerWhoAmI);
        packet.Send();
    }

    public static void HandleTileInteract(int i, int j, int type, Player interactingPlayer)
    {
        OnTileInteract?.Invoke(i, j, type, interactingPlayer);
    }
}

public class ObjectiveEventPlayer : ModPlayer
{
    /// <summary>
    /// Delegate for when a player enters a biome and meets time requirements.
    /// </summary>
    /// <param name="player">you</param>
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

            foreach (int timeReq in activeTimeRequirements)
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