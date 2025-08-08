using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Reverie.Core.Missions.Core;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Reverie.Utilities;

namespace Reverie.Core.Missions.System;

/// <summary>
/// Tracks authoritative state for mainline missions and syncs them to joining players.
/// Sideline missions remain individual per player.
/// </summary>
public class MainlineMissionSyncSystem : ModSystem
{
    // Authoritative state for mainline missions (server/host tracks this)
    private static readonly Dictionary<int, MissionDataContainer> MainlineMissionStates = [];
    private static readonly HashSet<int> CompletedMainlineMissions = [];

    private static MainlineMissionSyncSystem instance;
    public static MainlineMissionSyncSystem Instance => instance ??= ModContent.GetInstance<MainlineMissionSyncSystem>();

    public override void Load()
    {
        instance = this;
    }

    public override void Unload()
    {
        MainlineMissionStates.Clear();
        CompletedMainlineMissions.Clear();
        instance = null;
    }

    #region Mainline Mission State Tracking

    /// <summary>
    /// Updates the authoritative state for a mainline mission.
    /// Only call this for mainline missions.
    /// </summary>
    public static void UpdateMainlineMissionState(Mission mission)
    {
        if (!mission.IsMainline) return;

        var state = mission.ToState();
        MainlineMissionStates[mission.ID] = state;

        if (mission.Progress == MissionProgress.Completed)
        {
            CompletedMainlineMissions.Add(mission.ID);
        }

        Reverie.Instance.Logger.Debug($"Updated mainline mission state: {mission.Name} - {mission.Progress}");
    }

    /// <summary>
    /// Gets the authoritative state for a mainline mission.
    /// </summary>
    public static MissionDataContainer GetMainlineMissionState(int missionId)
    {
        return MainlineMissionStates.TryGetValue(missionId, out var state) ? state : null;
    }

    /// <summary>
    /// Checks if a mainline mission is completed in the server state.
    /// </summary>
    public static bool IsMainlineMissionCompleted(int missionId)
    {
        return CompletedMainlineMissions.Contains(missionId);
    }

    /// <summary>
    /// Gets all mainline missions that should be synced to a new player.
    /// </summary>
    public static Dictionary<int, MissionDataContainer> GetAllMainlineMissionStates()
    {
        return new Dictionary<int, MissionDataContainer>(MainlineMissionStates);
    }

    #endregion

    #region New Player Sync

    /// <summary>
    /// Syncs all mainline missions to a joining player.
    /// Should be called when a player joins the world.
    /// </summary>
    public static void SyncMainlineMissionsToPlayer(Player player)
    {
        if (player?.active != true) return;

        var missionPlayer = player.GetModPlayer<MissionPlayer>();
        int syncedCount = 0;

        foreach (var (missionId, state) in MainlineMissionStates)
        {
            try
            {
                // Get the mission definition
                var mission = MissionFactory.Instance.GetMissionData(missionId);
                if (mission?.IsMainline == true)
                {
                    // Load the server state into the player's mission
                    mission.LoadState(state);
                    missionPlayer.missionDict[missionId] = mission;

                    // Register if it's active
                    if (mission.Progress == MissionProgress.Ongoing)
                    {
                        MissionManager.Instance.RegisterMission(mission);
                    }

                    syncedCount++;
                }
            }
            catch (Exception ex)
            {
                Reverie.Instance.Logger.Error($"Failed to sync mainline mission {missionId} to player {player.name}: {ex}");
            }
        }

        // Also sync completed mainline missions
        foreach (var completedMissionId in CompletedMainlineMissions)
        {
            if (!missionPlayer.missionDict.ContainsKey(completedMissionId))
            {
                try
                {
                    var mission = MissionFactory.Instance.GetMissionData(completedMissionId);
                    if (mission?.IsMainline == true)
                    {
                        mission.Progress = MissionProgress.Completed;
                        mission.Status = MissionStatus.Completed;
                        missionPlayer.missionDict[completedMissionId] = mission;
                        syncedCount++;
                    }
                }
                catch (Exception ex)
                {
                    Reverie.Instance.Logger.Error($"Failed to sync completed mainline mission {completedMissionId} to player {player.name}: {ex}");
                }
            }
        }

        Reverie.Instance.Logger.Info($"Synced {syncedCount} mainline missions to joining player {player.name}");
    }

    /// <summary>
    /// Initializes mainline mission states from the host player's current state.
    /// Should be called when the world starts or when the first player joins.
    /// </summary>
    public static void InitializeMainlineMissionsFromHost()
    {
        // Find the host player (usually player 0, or first active player)
        Player hostPlayer = null;

        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            hostPlayer = Main.player[Main.myPlayer];
        }
        else if (Main.netMode == NetmodeID.Server)
        {
            // On dedicated server, use the first player who joins as reference
            // or load from world data if available
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i]?.active == true)
                {
                    hostPlayer = Main.player[i];
                    break;
                }
            }
        }
        else if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            // On client, the local player initializes from the server
            return;
        }

        if (hostPlayer != null)
        {
            var hostMissionPlayer = hostPlayer.GetModPlayer<MissionPlayer>();

            foreach (var mission in hostMissionPlayer.missionDict.Values.Where(m => m.IsMainline))
            {
                UpdateMainlineMissionState(mission);
            }

            Reverie.Instance.Logger.Info($"Initialized mainline mission states from host player {hostPlayer.name}");
        }
    }

    #endregion

    #region World Data Persistence

    public override void SaveWorldData(TagCompound tag)
    {
        try
        {
            // Save mainline mission states
            var mainlineStates = new List<TagCompound>();
            foreach (var (missionId, state) in MainlineMissionStates)
            {
                var stateTag = state.Serialize();
                stateTag["MissionID"] = missionId;
                mainlineStates.Add(stateTag);
            }

            tag["MainlineMissionStates"] = mainlineStates;
            tag["CompletedMainlineMissions"] = CompletedMainlineMissions.ToList();

            Reverie.Instance.Logger.Info($"Saved {MainlineMissionStates.Count} mainline mission states to world data");
        }
        catch (Exception ex)
        {
            Reverie.Instance.Logger.Error($"Failed to save mainline mission data: {ex}");
        }
    }

    public override void LoadWorldData(TagCompound tag)
    {
        try
        {
            // Clear existing data
            MainlineMissionStates.Clear();
            CompletedMainlineMissions.Clear();

            // Load mainline mission states
            var mainlineStates = tag.GetList<TagCompound>("MainlineMissionStates");
            foreach (var stateTag in mainlineStates)
            {
                try
                {
                    var missionId = stateTag.GetInt("MissionID");
                    var container = MissionDataContainer.Deserialize(stateTag);

                    if (container != null)
                    {
                        MainlineMissionStates[missionId] = container;
                    }
                }
                catch (Exception ex)
                {
                    Reverie.Instance.Logger.Error($"Failed to load mainline mission state: {ex}");
                }
            }

            // Load completed mainline missions
            var completedMissions = tag.GetList<int>("CompletedMainlineMissions");
            CompletedMainlineMissions.UnionWith(completedMissions);

            Reverie.Instance.Logger.Info($"Loaded {MainlineMissionStates.Count} mainline mission states from world data");
        }
        catch (Exception ex)
        {
            Reverie.Instance.Logger.Error($"Failed to load mainline mission data: {ex}");
        }
    }

    #endregion

    #region Update Hooks

    public override void PostUpdateWorld()
    {
        // Periodically sync mainline mission states from active players
        if (Main.GameUpdateCount % (60 * 10) == 0) // Every 10 seconds
        {
            SyncMainlineStatesFromActivePlayers();
        }
    }

    /// <summary>
    /// Updates mainline mission states from all currently active players.
    /// This ensures the authoritative state stays current.
    /// </summary>
    private void SyncMainlineStatesFromActivePlayers()
    {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            var player = Main.player[i];
            if (player?.active == true)
            {
                var missionPlayer = player.GetModPlayer<MissionPlayer>();

                foreach (var mission in missionPlayer.missionDict.Values.Where(m => m.IsMainline))
                {
                    UpdateMainlineMissionState(mission);
                }
            }
        }
    }

    #endregion

    #region Player Event Handlers

    /// <summary>
    /// Call this when a player joins the world to sync mainline missions.
    /// </summary>
    public void OnPlayerJoin(Player player)
    {
        if (Main.netMode == NetmodeID.SinglePlayer) return;

        // Small delay to ensure player is fully loaded
        ModContent.GetInstance<Reverie>().Logger.Info($"Player {player.name} joined - scheduling mainline mission sync");

        // Sync mainline missions after a short delay
        Task.Run(async () =>
        {
            await Task.Delay(1000); // 1 second delay
            if (player?.active == true)
            {
                SyncMainlineMissionsToPlayer(player);
            }
        });
    }

    /// <summary>
    /// Call this when the world starts to initialize mainline mission tracking.
    /// </summary>
    public void OnWorldStart()
    {
        if (MainlineMissionStates.Count == 0)
        {
            InitializeMainlineMissionsFromHost();
        }
    }

    #endregion
}