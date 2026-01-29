using Reverie.Utilities;
using Terraria;
using Terraria.DataStructures;

namespace Reverie.Core.Missions;

public struct MissionEvent
{
    public MissionEventType Type;
    public Player Player;
    public int Amount;

    // Tile events
    public int? TileType;
    public int? TileX;
    public int? TileY;

    // NPC events
    public int? NPCType;
    public NPC NPC;

    // Item events
    public int? ItemType;
    public Item Item;
    public ItemCreationContext Context;

    // Biome events
    public BiomeType? Biome;
    public int? TimeSpent;

    public static MissionEvent TileBreak(int i, int j, int type, Player player)
    {
        return new MissionEvent
        {
            Type = MissionEventType.TileBreak,
            TileX = i,
            TileY = j,
            TileType = type,
            Player = player,
            Amount = 1
        };
    }

    public static MissionEvent TilePlace(int i, int j, int type, Player player)
    {
        return new MissionEvent
        {
            Type = MissionEventType.TilePlace,
            TileX = i,
            TileY = j,
            TileType = type,
            Player = player,
            Amount = 1
        };
    }

    public static MissionEvent TileInteract(int i, int j, int type, Player player)
    {
        return new MissionEvent
        {
            Type = MissionEventType.TileInteract,
            TileX = i,
            TileY = j,
            TileType = type,
            Player = player,
            Amount = 1
        };
    }

    public static MissionEvent NPCKill(NPC npc, Player player)
    {
        return new MissionEvent
        {
            Type = MissionEventType.NPCKill,
            NPCType = npc.type,
            NPC = npc,
            Player = player,
            Amount = 1
        };
    }

    public static MissionEvent NPCChat(NPC npc, Player player)
    {
        return new MissionEvent
        {
            Type = MissionEventType.NPCChat,
            NPCType = npc.type,
            NPC = npc,
            Player = player,
            Amount = 1
        };
    }

    public static MissionEvent ItemPickup(Item item, Player player)
    {
        return new MissionEvent
        {
            Type = MissionEventType.ItemPickup,
            ItemType = item.type,
            Item = item,
            Player = player,
            Amount = item.stack
        };
    }

    public static MissionEvent ItemCraft(Item item, ItemCreationContext context, Player player)
    {
        return new MissionEvent
        {
            Type = MissionEventType.ItemCraft,
            ItemType = item.type,
            Item = item,
            Context = context,
            Player = player,
            Amount = item.stack
        };
    }

    public static MissionEvent ItemUse(Item item, Player player)
    {
        return new MissionEvent
        {
            Type = MissionEventType.ItemUse,
            ItemType = item.type,
            Item = item,
            Player = player,
            Amount = 1
        };
    }

    public static MissionEvent BiomeEnter(BiomeType biome, int timeSpent, Player player)
    {
        return new MissionEvent
        {
            Type = MissionEventType.BiomeEnter,
            Biome = biome,
            TimeSpent = timeSpent,
            Player = player,
            Amount = timeSpent
        };
    }
}

public enum MissionEventType
{
    TileBreak,
    TilePlace,
    TileInteract,
    NPCKill,
    NPCChat,
    ItemPickup,
    ItemCraft,
    ItemUse,
    BiomeEnter
}