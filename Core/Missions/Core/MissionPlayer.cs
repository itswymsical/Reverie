using Reverie.Common.UI.Missions;
using Reverie.Core.Indicators;
using Reverie.Core.Missions.Core;
using Reverie.Core.Missions.System;
using Reverie.Utilities;
using Reverie.Utilities.Extensions;

using System.Collections.Generic;
using System.Linq;

using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Reverie.Core.Missions;

public partial class MissionPlayer : ModPlayer
{
    #region Properties & Fields
    public readonly Dictionary<int, Mission> missionDict = [];

    private readonly HashSet<int> notifiedMissions = [];

    /// <summary>
    /// IDs of missions that changed and may need syncing or state updates.
    /// </summary>
    /// /// <remarks>
    /// A "dirty" mission has changed state and needs to be saved, updated, or synced.
    /// The dirty flag helps track when mission data is out of date.
    /// </remarks>
    private readonly HashSet<int> dirtyMissions = [];

    private TagCompound savedMissionData = null;
    private bool hasDeferredLoadRun = false;
    private bool hasReceivedMainlineSync = false;

    #endregion

    #region Mission Logic
    public void NotifyMissionUpdate(Mission mission)
    {
        if (mission == null) return;

        try
        {
            missionDict[mission.ID] = mission;
            dirtyMissions.Add(mission.ID);

            var container = mission.ToState();

            // Update mainline mission state if this is a mainline mission
            if (mission.IsMainline)
            {
                MainlineMissionSyncSystem.UpdateMainlineMissionState(mission);
            }

            // Only show UI updates for the local player
            if (Player.whoAmI == Main.myPlayer)
            {
                ModContent.GetInstance<Reverie>().Logger.Debug($"Mission {mission.Name} progress updated: Set {mission.CurrentIndex}");
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Failed to notify mission update: {ex}");
        }
    }

    public void SyncMissionState(Mission mission)
    {
        if (mission == null) return;

        // Create or update mission state in storage
        _ = mission.ToState();
        if (missionDict.ContainsKey(mission.ID))
        {
            missionDict[mission.ID] = mission;
        }

        // Update mainline mission sync if this is a mainline mission
        if (mission.IsMainline)
        {
            MainlineMissionSyncSystem.UpdateMainlineMissionState(mission);
        }
    }

    public bool UpdateMissionProgress(int missionId, int objectiveIndex, int amount = 1)
    {
        // Use the utility method that handles mainline/sideline distinction
        return MissionUtils.UpdateMissionProgressForPlayers(missionId, objectiveIndex, amount, Player);
    }

    private void ResetToCleanState()
    {
        missionDict.Clear();
        notifiedMissions.Clear();
        MissionManager.Instance.Reset();
        hasReceivedMainlineSync = false;
    }

    public void DebugMissionState()
    {
        // Only show debug info for local player
        if (Player.whoAmI != Main.myPlayer) return;

        foreach (var mission in missionDict.Values)
        {
            Main.NewText($"Mission: {mission.Name}");
            Main.NewText($"  Type: {(mission.IsMainline ? "Mainline" : "Sideline")}");
            Main.NewText($"  State: {mission.Status}");
            Main.NewText($"  Progress: {mission.Progress}");
            Main.NewText($"  NextMissionID: {mission.NextMissionID}");
        }
    }
    #endregion

    #region Serialization

    public override void SaveData(TagCompound tag)
    {
        try
        {
            // Ensure all active missions are synced before saving
            foreach (var mission in missionDict.Values)
            {
                SyncMissionState(mission);
            }

            // Save ALL missions (mainline and sideline)
            // The sync system will handle mainline mission sharing separately
            var activeMissionData = new List<TagCompound>();
            foreach (var mission in missionDict.Values.Where(m => m.Progress != MissionProgress.Completed))
            {
                var state = mission.ToState();
                var missionData = new TagCompound
                {
                    ["ID"] = mission.ID,
                    ["IsMainline"] = mission.IsMainline,
                    ["State"] = new TagCompound
                    {
                        ["Progress"] = (int)mission.Progress,
                        ["Status"] = (int)mission.Status,
                        ["Unlocked"] = mission.Unlocked,
                        ["CurrentIndex"] = mission.CurrentIndex,
                        ["Objective"] = SerializeObjectives(mission.Objective)
                    }
                };
                activeMissionData.Add(missionData);
            }
            tag["ActiveMissions"] = activeMissionData;

            tag["CompletedMissionIDs"] = CompletedMissions().Select(m => m.ID).ToList();
            tag["NotifiedMissions"] = notifiedMissions.ToList();
            tag["HasReceivedMainlineSync"] = hasReceivedMainlineSync;

            ModContent.GetInstance<Reverie>().Logger.Info($"Successfully saved mission data for {activeMissionData.Count} active missions (Player: {Player.name})");
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Failed to save mission data for player {Player.name}: {ex}");
        }
    }

    public override void LoadData(TagCompound tag)
    {
        // Only store the data, don't process it yet
        savedMissionData = tag;

        // Reset to clean state to avoid any partial loading issues
        ResetToCleanState();

        ModContent.GetInstance<Reverie>().Logger.Info($"Mission data stored for deferred loading (Player: {Player.name})");
    }

    // New method for deferred loading
    private void ProcessDeferredLoad()
    {
        if (hasDeferredLoadRun || savedMissionData == null)
        {
            return;
        }

        hasDeferredLoadRun = true;

        try
        {
            ModContent.GetInstance<Reverie>().Logger.Info($"Starting deferred mission data loading for player {Player.name}");

            // Get sync status
            hasReceivedMainlineSync = savedMissionData.GetBool("HasReceivedMainlineSync");

            // Load completed missions first
            var completedMissionIds = savedMissionData.GetList<int>("CompletedMissionIDs");
            int completedMissionsLoaded = 0;

            foreach (var missionId in completedMissionIds)
            {
                try
                {
                    LoadCompletedMission(missionId);
                    completedMissionsLoaded++;
                }
                catch (Exception ex)
                {
                    ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load completed mission {missionId} for player {Player.name}: {ex.Message}");
                }
            }

            ModContent.GetInstance<Reverie>().Logger.Info($"Loaded {completedMissionsLoaded}/{completedMissionIds.Count} completed missions for player {Player.name}");

            // Load active missions (both mainline and sideline from save data)
            int activeMissionsLoaded = 0;
            var activeMissionData = savedMissionData.GetList<TagCompound>("ActiveMissions").ToList();

            foreach (var missionTag in activeMissionData)
            {
                try
                {
                    LoadActiveMission(missionTag);
                    activeMissionsLoaded++;
                }
                catch (Exception ex)
                {
                    int missionId = missionTag.ContainsKey("ID") ? missionTag.GetInt("ID") : -1;
                    ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load active mission {missionId} for player {Player.name}: {ex.Message}");
                }
            }

            ModContent.GetInstance<Reverie>().Logger.Info($"Loaded {activeMissionsLoaded}/{activeMissionData.Count} active missions for player {Player.name}");

            // Load notified missions
            try
            {
                notifiedMissions.UnionWith([.. savedMissionData.GetList<int>("NotifiedMissions")]);
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load notified missions for player {Player.name}: {ex.Message}");
            }

            // Re-register active missions
            int missionsRegistered = 0;
            foreach (var mission in ActiveMissions())
            {
                try
                {
                    MissionManager.Instance.RegisterMission(mission);
                    missionsRegistered++;
                }
                catch (Exception ex)
                {
                    ModContent.GetInstance<Reverie>().Logger.Error($"Failed to register active mission {mission.ID} for player {Player.name}: {ex.Message}");
                }
            }
            ModContent.GetInstance<Reverie>().Logger.Info($"Registered {missionsRegistered} active missions with manager for player {Player.name}");

            // Validate mission state consistency
            ValidateStates();

            // Clear saved data to free memory
            savedMissionData = null;

            ModContent.GetInstance<Reverie>().Logger.Info($"Completed deferred mission loading successfully for player {Player.name}");
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Critical failure in deferred mission loading for player {Player.name}: {ex}");
            ResetToCleanState();
        }
    }

    private void ValidateStates()
    {
        foreach (var mission in missionDict.Values)
        {
            try
            {
                // Ensure current index is valid
                if (mission.CurrentIndex < 0 || mission.CurrentIndex >= mission.Objective.Count)
                {
                    ModContent.GetInstance<Reverie>().Logger.Warn($"Fixing invalid CurrentIndex for mission {mission.ID} (Player: {Player.name})");
                    mission.CurrentIndex = 0;
                }

                // Validate objective completion states
                foreach (var set in mission.Objective)
                {
                    // Validate counts are within bounds
                    foreach (var objective in set.Objectives)
                    {
                        if (objective.CurrentCount > objective.RequiredCount)
                        {
                            ModContent.GetInstance<Reverie>().Logger.Warn($"Objective count exceeds required count, clamping (Player: {Player.name})");
                            objective.CurrentCount = objective.RequiredCount;
                        }

                        if (objective.IsCompleted && objective.CurrentCount < objective.RequiredCount)
                        {
                            ModContent.GetInstance<Reverie>().Logger.Warn($"Objective marked complete but count is insufficient (Player: {Player.name})");
                            objective.CurrentCount = objective.RequiredCount;
                        }
                    }
                }

                // Check if the mission progress state is valid
                if (mission.Progress == MissionProgress.Completed)
                {
                    // Ensure all objectives are actually complete
                    bool allSetsComplete = mission.Objective.All(set => set.IsCompleted);
                    if (!allSetsComplete)
                    {
                        ModContent.GetInstance<Reverie>().Logger.Warn($"Mission {mission.ID} marked complete but has incomplete objectives (Player: {Player.name})");
                    }
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Error validating mission {mission.ID} for player {Player.name}: {ex.Message}");
            }
        }
    }

    private static List<TagCompound> SerializeObjectives(List<ObjectiveSet> ObjectiveIndex)
    {
        var serializedSets = new List<TagCompound>();

        foreach (var set in ObjectiveIndex)
        {
            var objectiveData = new List<TagCompound>();

            foreach (var objective in set.Objectives)
            {
                objectiveData.Add(new TagCompound
                {
                    ["Description"] = objective.Description,
                    ["IsCompleted"] = objective.IsCompleted,
                    ["RequiredCount"] = objective.RequiredCount,
                    ["CurrentCount"] = objective.CurrentCount
                });
            }

            serializedSets.Add(new TagCompound
            {
                ["Objectives"] = objectiveData,
                ["HasCheckedInventory"] = set.HasCheckedInitialInventory
            });
        }

        return serializedSets;
    }

    private static List<ObjectiveIndexState> DeserializeObjectives(IList<TagCompound> serializedSets)
    {
        var objectiveStates = new List<ObjectiveIndexState>();

        foreach (var setTag in serializedSets)
        {
            try
            {
                var objectiveSet = new ObjectiveIndexState();
                var objectiveTags = setTag.GetList<TagCompound>("Objectives");

                foreach (var objTag in objectiveTags)
                {
                    try
                    {
                        var objective = new ObjectiveState
                        {
                            Description = objTag.GetString("Description"),
                            IsCompleted = objTag.GetBool("IsCompleted"),
                            RequiredCount = objTag.GetInt("RequiredCount"),
                            CurrentCount = objTag.GetInt("CurrentCount")
                        };
                        objectiveSet.Objectives.Add(objective);
                    }
                    catch (Exception ex)
                    {
                        ModContent.GetInstance<Reverie>().Logger.Error($"Error deserializing objective: {ex.Message}");
                    }
                }

                objectiveStates.Add(objectiveSet);
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Error deserializing objective set: {ex.Message}");
            }
        }

        return objectiveStates;
    }

    private void LoadActiveMission(TagCompound missionTag)
    {
        try
        {
            var missionId = missionTag.GetInt("ID");
            bool isMainline = missionTag.GetBool("IsMainline");
            var stateTag = missionTag.GetCompound("State");

            // Validate that we can create this mission
            var mission = MissionFactory.Instance.GetMissionData(missionId);
            if (mission == null)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Failed to create mission with ID {missionId} for player {Player.name}");
                return;
            }

            // For multiplayer, if this is a mainline mission and we're not the host,
            // we should get the state from the sync system instead of save data
            if (Main.netMode == NetmodeID.MultiplayerClient && isMainline && hasReceivedMainlineSync)
            {
                var syncedState = MainlineMissionSyncSystem.GetMainlineMissionState(missionId);
                if (syncedState != null)
                {
                    mission.LoadState(syncedState);
                    missionDict[missionId] = mission;
                    ModContent.GetInstance<Reverie>().Logger.Info($"Loaded mainline mission {mission.Name} from sync system for player {Player.name}");
                    return;
                }
            }

            // Load from save data (for singleplayer, sideline missions, or when sync isn't available)
            // Gather state data
            var progress = (MissionProgress)stateTag.GetInt("Progress");
            var availability = (MissionStatus)stateTag.GetInt("Status");
            var unlocked = stateTag.GetBool("Unlocked");
            var currentIndex = stateTag.GetInt("CurrentIndex");

            // Validate state data
            if (!Enum.IsDefined(typeof(MissionProgress), progress))
            {
                ModContent.GetInstance<Reverie>().Logger.Warn($"Invalid mission progress value {(int)progress} for mission {missionId} (Player: {Player.name}), defaulting to Inactive");
                progress = MissionProgress.Inactive;
            }

            if (!Enum.IsDefined(typeof(MissionStatus), availability))
            {
                ModContent.GetInstance<Reverie>().Logger.Warn($"Invalid mission availability value {(int)availability} for mission {missionId} (Player: {Player.name}), defaulting to Locked");
                availability = MissionStatus.Locked;
            }

            if (currentIndex < 0 || currentIndex >= mission.Objective.Count)
            {
                ModContent.GetInstance<Reverie>().Logger.Warn($"Invalid currentIndex {currentIndex} for mission {missionId} (Player: {Player.name}), defaulting to 0");
                currentIndex = 0;
            }

            // Create container from saved state
            var container = new MissionDataContainer
            {
                ID = missionId,
                Progress = progress,
                Availability = availability,
                Unlocked = unlocked,
                CurObjectiveIndex = currentIndex
            };

            // Load objective data with validation
            try
            {
                container.ObjectiveIndex = DeserializeObjectives(stateTag.GetList<TagCompound>("Objective"));
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Failed to deserialize objectives for mission {missionId} (Player: {Player.name}): {ex.Message}");
                container.ObjectiveIndex = new List<ObjectiveIndexState>();

                // Create placeholder empty states
                for (int i = 0; i < mission.Objective.Count; i++)
                {
                    var set = new ObjectiveIndexState();
                    foreach (var obj in mission.Objective[i].Objectives)
                    {
                        set.Objectives.Add(new ObjectiveState
                        {
                            Description = obj.Description,
                            IsCompleted = false,
                            RequiredCount = obj.RequiredCount,
                            CurrentCount = 0
                        });
                    }
                    container.ObjectiveIndex.Add(set);
                }

                // Mark as inactive if we couldn't load objectives
                container.Progress = MissionProgress.Inactive;
            }

            // Load state with enhanced recovery
            mission.LoadState(container);
            missionDict[missionId] = mission;

            // Mark mission as dirty to ensure proper sync
            dirtyMissions.Add(missionId);

            ModContent.GetInstance<Reverie>().Logger.Info($"Successfully loaded mission {mission.Name} (ID: {missionId}) with state {mission.Progress} for player {Player.name}");
        }
        catch (Exception ex)
        {
            var idString = missionTag.ContainsKey("ID") ? missionTag.GetInt("ID").ToString() : "unknown";
            ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load active mission {idString} for player {Player.name}: {ex.Message}");
        }
    }

    private void LoadCompletedMission(int missionId)
    {
        var mission = GetMission(missionId);
        if (mission != null)
        {
            mission.Progress = MissionProgress.Completed;
            mission.Status = MissionStatus.Completed;
            missionDict[missionId] = mission;
        }
    }

    #endregion

    #region Mission Access - Global Operations for Mainline Missions

    public Mission GetMission(int missionId)
    {
        if (missionDict.TryGetValue(missionId, out var mission))
        {
            return mission;
        }

        // Get from factory if not in cache
        mission = MissionFactory.Instance.GetMissionData(missionId);
        if (mission != null)
        {
            missionDict[missionId] = mission;
        }
        return mission;
    }

    public void ResetMission(int missionId)
    {
        var mission = GetMission(missionId);
        if (mission?.IsMainline == true)
        {
            // Mainline missions: reset for ALL players
            ResetMainlineMissionForAllPlayers(missionId);
        }
        else if (mission != null)
        {
            // Sideline missions: reset only for this player
            mission.Reset();
            mission.Status = MissionStatus.Unlocked;
            SyncMissionState(mission);
        }
    }

    public void CompleteMission(int missionId)
    {
        var mission = GetMission(missionId);
        if (mission?.IsMainline == true)
        {
            // Mainline missions: complete for ALL players
            CompleteMainlineMissionForAllPlayers(missionId);
        }
        else if (mission != null)
        {
            // Sideline missions: complete only for this player
            mission.Complete(Player);
            SyncMissionState(mission);
        }
    }

    public void StartMission(int missionId)
    {
        var mission = GetMission(missionId);
        if (mission?.IsMainline == true)
        {
            // Mainline missions: start for ALL players
            StartMainlineMissionForAllPlayers(missionId);
        }
        else if (mission != null)
        {
            // Sideline missions: start only for this player
            mission.Progress = MissionProgress.Ongoing;
            MissionManager.Instance.RegisterMission(mission);
            SyncMissionState(mission);

            mission.OnMissionStart();

            // Only show notification to the local player
            if (Player.whoAmI == Main.myPlayer)
            {
                InGameNotificationsTracker.AddNotification(new MissionAcceptNotification(mission));
            }
        }
    }

    public void UnlockMission(int missionId, bool broadcast = false)
    {
        var mission = GetMission(missionId);
        if (mission?.IsMainline == true)
        {
            // Mainline missions: unlock for ALL players
            UnlockMainlineMissionForAllPlayers(missionId, broadcast);
        }
        else if (mission != null)
        {
            // Sideline missions: unlock only for this player
            mission.Status = MissionStatus.Unlocked;
            mission.Progress = MissionProgress.Inactive;
            mission.Unlocked = true;
            SyncMissionState(mission);

            // Only show broadcast message to local player and only if they haven't been notified
            if (broadcast && mission.ProviderNPC > 0 && Player.whoAmI == Main.myPlayer)
            {
                var npcName = Lang.GetNPCNameValue(mission.ProviderNPC);
                Main.NewText($"{npcName} has a job opportunity!", Color.CornflowerBlue);
                notifiedMissions.Add(mission.ID);
            }
        }
    }

    #endregion

    #region Global Mainline Mission Operations

    private void UnlockMainlineMissionForAllPlayers(int missionId, bool broadcast = false)
    {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            var player = Main.player[i];
            if (player?.active == true)
            {
                var missionPlayer = player.GetModPlayer<MissionPlayer>();
                var mission = missionPlayer.GetMission(missionId);

                if (mission != null)
                {
                    mission.Status = MissionStatus.Unlocked;
                    mission.Progress = MissionProgress.Inactive;
                    mission.Unlocked = true;
                    missionPlayer.SyncMissionState(mission);

                    // Only show broadcast message to local player and only if they haven't been notified
                    if (broadcast && mission.ProviderNPC > 0 && player.whoAmI == Main.myPlayer)
                    {
                        var npcName = Lang.GetNPCNameValue(mission.ProviderNPC);
                        Main.NewText($"A new chapter begins! {npcName} has important information for everyone!", Color.Gold);
                        missionPlayer.notifiedMissions.Add(mission.ID);
                    }
                }
            }
        }

        ModContent.GetInstance<Reverie>().Logger.Info($"Unlocked mainline mission {missionId} for all players");
    }

    private void StartMainlineMissionForAllPlayers(int missionId)
    {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            var player = Main.player[i];
            if (player?.active == true)
            {
                var missionPlayer = player.GetModPlayer<MissionPlayer>();
                var mission = missionPlayer.GetMission(missionId);

                if (mission != null)
                {
                    mission.Progress = MissionProgress.Ongoing;
                    MissionManager.Instance.RegisterMission(mission);
                    missionPlayer.SyncMissionState(mission);

                    mission.OnMissionStart();

                    // Only show notification to the local player
                    if (player.whoAmI == Main.myPlayer)
                    {
                        InGameNotificationsTracker.AddNotification(new MissionAcceptNotification(mission));
                    }
                }
            }
        }

        ModContent.GetInstance<Reverie>().Logger.Info($"Started mainline mission {missionId} for all players");
    }

    private void CompleteMainlineMissionForAllPlayers(int missionId)
    {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            var player = Main.player[i];
            if (player?.active == true)
            {
                var missionPlayer = player.GetModPlayer<MissionPlayer>();
                var mission = missionPlayer.GetMission(missionId);

                if (mission != null)
                {
                    mission.Complete(player);
                    missionPlayer.SyncMissionState(mission);
                }
            }
        }

        ModContent.GetInstance<Reverie>().Logger.Info($"Completed mainline mission {missionId} for all players");
    }

    private void ResetMainlineMissionForAllPlayers(int missionId)
    {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            var player = Main.player[i];
            if (player?.active == true)
            {
                var missionPlayer = player.GetModPlayer<MissionPlayer>();
                var mission = missionPlayer.GetMission(missionId);

                if (mission != null)
                {
                    mission.Reset();
                    mission.Status = MissionStatus.Unlocked;
                    missionPlayer.SyncMissionState(mission);
                }
            }
        }

        ModContent.GetInstance<Reverie>().Logger.Info($"Reset mainline mission {missionId} for all players");
    }

    #endregion

    #region Mission Queries
    public IEnumerable<Mission> AvailableMissions()
        => missionDict.Values.Where
        (m => m.Status == MissionStatus.Unlocked && m.Progress == MissionProgress.Inactive);

    public IEnumerable<Mission> ActiveMissions()
    {
        foreach (var mission in missionDict.Values.Where(m => m.Progress == MissionProgress.Ongoing))
        {
            if (dirtyMissions.Contains(mission.ID))
            {
                mission.LoadState(mission.ToState());
            }
            yield return mission;
        }
    }

    public IEnumerable<Mission> CompletedMissions()
        => missionDict.Values.Where
        (m => m.Progress == MissionProgress.Completed);

    public void AssignMissionToNPC(int npcType, int missionId)
    {
        var mission = GetMission(missionId);
        if (mission?.IsMainline == true)
        {
            // Mainline missions: assign for ALL players
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                var player = Main.player[i];
                if (player?.active == true)
                {
                    var missionPlayer = player.GetModPlayer<MissionPlayer>();
                    var playerMission = missionPlayer.GetMission(missionId);
                    if (playerMission != null)
                    {
                        playerMission.ProviderNPC = npcType;
                        missionPlayer.missionDict[missionId] = playerMission;
                        missionPlayer.SyncMissionState(playerMission);
                    }
                }
            }
        }
        else if (mission != null)
        {
            // Sideline missions: assign only for this player
            mission.ProviderNPC = npcType;
            missionDict[missionId] = mission;
            SyncMissionState(mission);
        }
    }

    public void RemoveMissionFromNPC(int npcType, int missionId)
    {
        var mission = GetMission(missionId);
        if (mission?.IsMainline == true)
        {
            // Mainline missions: remove for ALL players
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                var player = Main.player[i];
                if (player?.active == true)
                {
                    var missionPlayer = player.GetModPlayer<MissionPlayer>();
                    var playerMission = missionPlayer.GetMission(missionId);
                    if (playerMission?.ProviderNPC == npcType)
                    {
                        playerMission.ProviderNPC = 0;
                        missionPlayer.missionDict[missionId] = playerMission;
                        missionPlayer.SyncMissionState(playerMission);
                    }
                }
            }
        }
        else if (mission?.ProviderNPC == npcType)
        {
            // Sideline missions: remove only for this player
            mission.ProviderNPC = 0;
            missionDict[missionId] = mission;
            SyncMissionState(mission);
        }
    }

    public void BroadcastAvailableMissionsForNPC(int npcType)
    {
        NPCHasAvailableMission(npcType, true);
    }

    public void CheckAndBroadcastAllAvailableMissions()
    {
        // Only broadcast for local player to avoid spam
        if (Player.whoAmI != Main.myPlayer) return;

        var npcTypesWithMissions = missionDict.Values
            .Where(m => m.ProviderNPC > 0 && m.Status == MissionStatus.Unlocked)
            .Select(m => m.ProviderNPC)
            .Distinct();

        foreach (var npcType in npcTypesWithMissions)
        {
            NPCHasAvailableMission(npcType, true);
        }
    }

    public bool NPCHasAvailableMission(int npcType, bool broadcast = false)
    {
        bool hasAvailableMission = false;

        foreach (var mission in missionDict.Values.Where(m =>
            m.ProviderNPC == npcType &&
            m.Status == MissionStatus.Unlocked))
        {
            hasAvailableMission = true;

            // Only broadcast for local player and non-mainline missions
            if (!mission.IsMainline && !notifiedMissions.Contains(mission.ID) && broadcast && Player.whoAmI == Main.myPlayer)
            {
                var npcName = Lang.GetNPCNameValue(mission.ProviderNPC);
                Main.NewText($"{npcName} has a job opportunity!", Color.CornflowerBlue);
                notifiedMissions.Add(mission.ID);
            }
        }

        return hasAvailableMission;
    }

    #endregion

    #region Init & Update
    public override void OnEnterWorld()
    {
        ProcessDeferredLoad();

        // Sync mainline missions for multiplayer
        if (Main.netMode == NetmodeID.MultiplayerClient && !hasReceivedMainlineSync)
        {
            // Request mainline mission sync from server
            MainlineMissionSyncSystem.Instance.OnPlayerJoin(Player);
            hasReceivedMainlineSync = true;
        }
        else if (Main.netMode == NetmodeID.Server)
        {
            // Initialize mainline missions on server
            MainlineMissionSyncSystem.Instance.OnWorldStart();
        }

        // Only clear notifications for local player
        if (Player.whoAmI == Main.myPlayer)
        {
            notifiedMissions.Clear();
        }
    }

    public override void PostUpdate()
    {
        base.PostUpdate();

        if (!hasDeferredLoadRun)
        {
            ProcessDeferredLoad();
        }

        if (dirtyMissions.Count > 0)
        {
            dirtyMissions.Clear();
        }
    }

    #endregion
}