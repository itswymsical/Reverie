using Reverie.Utilities;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;

namespace Reverie.Core.Missions;

/// <summary>
/// Centralized mission system. Handles:
/// </summary>
/// <remarks>
/// - Mission registration (auto via reflection)
/// - Event subscriptions (all game events)
/// - Event broadcasting (to missions)
/// </remarks>
public class MissionSystem : ModSystem
{
    private static readonly Dictionary<int, Type> MissionTypes = new();

    public override void Load()
    {
        RegisterMissionsViaReflection();
        SubscribeToEvents();
    }

    public override void Unload()
    {
        UnsubscribeFromEvents();
        MissionTypes.Clear();
    }
    public static IEnumerable<Mission> GetAllAvailableMissions(MissionPlayer player)
    {
        var sideline = player.AvailableMissions();

        var mainline = MissionWorld.Instance.GetAllMissions()
            .Where(m => m.Status == MissionStatus.Unlocked && m.Progress != MissionProgress.Ongoing);

        return sideline.Concat(mainline);
    }

    #region Auto-Registration
    private void RegisterMissionsViaReflection()
    {
        var missions = Mod.Code.GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Mission)));

        foreach (var type in missions)
        {
            try
            {
                var instance = (Mission)Activator.CreateInstance(type);
                MissionTypes[instance.ID] = type;
                Mod.Logger.Debug($"Auto-registered mission: {instance.ID} ({type.Name})");
            }
            catch (Exception ex)
            {
                Mod.Logger.Error($"Failed to register mission {type.Name}: {ex.Message}");
            }
        }

        Mod.Logger.Info($"Auto-registered {MissionTypes.Count} missions");
    }

    public static Mission CreateMission(int missionId)
    {
        if (MissionTypes.TryGetValue(missionId, out var type))
        {
            return (Mission)Activator.CreateInstance(type);
        }
        return null;
    }

    public static bool IsMissionRegistered(int missionId) => MissionTypes.ContainsKey(missionId);
    #endregion

    #region Event Subscriptions
    private void SubscribeToEvents()
    {
        ObjectiveEventTile.OnTileBreak += OnTileBreak;
        ObjectiveEventTile.OnTilePlace += OnTilePlace;
        ObjectiveEventTile.OnTileInteract += OnTileInteract;
        ObjectiveEventNPC.OnNPCKill += OnNPCKill;
        ObjectiveEventNPC.OnNPCChat += OnNPCChat;
        ObjectiveEventItem.OnItemPickup += OnItemPickup;
        ObjectiveEventItem.OnItemCreated += OnItemCraft;
        ObjectiveEventItem.OnItemUse += OnItemUse;
        ObjectiveEventPlayer.OnBiomeEnter += OnBiomeEnter;
    }

    private void UnsubscribeFromEvents()
    {
        ObjectiveEventTile.OnTileBreak -= OnTileBreak;
        ObjectiveEventTile.OnTilePlace -= OnTilePlace;
        ObjectiveEventTile.OnTileInteract -= OnTileInteract;
        ObjectiveEventNPC.OnNPCKill -= OnNPCKill;
        ObjectiveEventNPC.OnNPCChat -= OnNPCChat;
        ObjectiveEventItem.OnItemPickup -= OnItemPickup;
        ObjectiveEventItem.OnItemCreated -= OnItemCraft;
        ObjectiveEventItem.OnItemUse -= OnItemUse;
        ObjectiveEventPlayer.OnBiomeEnter -= OnBiomeEnter;
    }
    #endregion

    #region Event Handlers
    private void OnTileBreak(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (fail || effectOnly) return;
        var evt = MissionEvent.TileBreak(i, j, type, Main.LocalPlayer);
        BroadcastEvent(evt);
    }

    private void OnTilePlace(int i, int j, int type)
    {
        var evt = MissionEvent.TilePlace(i, j, type, Main.LocalPlayer);
        BroadcastEvent(evt);
    }

    private void OnTileInteract(int i, int j, int type)
    {
        var evt = MissionEvent.TileInteract(i, j, type, Main.LocalPlayer);
        BroadcastEvent(evt);
    }

    private void OnNPCKill(NPC npc)
    {
        var evt = MissionEvent.NPCKill(npc, Main.LocalPlayer);
        BroadcastEvent(evt);
    }

    private void OnNPCChat(NPC npc, ref string chat)
    {
        var evt = MissionEvent.NPCChat(npc, Main.LocalPlayer);
        BroadcastEvent(evt);
    }

    private void OnItemPickup(Item item, Player player)
    {
        var evt = MissionEvent.ItemPickup(item, player);
        BroadcastEvent(evt);
    }

    private void OnItemCraft(Item item, ItemCreationContext context)
    {
        var evt = MissionEvent.ItemCraft(item, context, Main.LocalPlayer);
        BroadcastEvent(evt);
    }

    private void OnItemUse(Item item, Player player)
    {
        var evt = MissionEvent.ItemUse(item, player);
        BroadcastEvent(evt);
    }

    private void OnBiomeEnter(Player player, BiomeType biome, int timeSpent)
    {
        var evt = MissionEvent.BiomeEnter(biome, timeSpent, player);
        BroadcastEvent(evt);
    }
    #endregion

    #region Event Broadcasting
    private static void BroadcastEvent(MissionEvent evt)
    {
        if (evt.Player == null) return;

        // Player missions (sideline)
        var missionPlayer = evt.Player.GetModPlayer<MissionPlayer>();
        missionPlayer?.OnMissionEvent(evt);

        // World missions (mainline)
        MissionWorld.Instance?.OnMissionEvent(evt);
    }
    #endregion
}