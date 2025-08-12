using Reverie.Common.UI.Missions;
using Reverie.Core.Indicators;
using Reverie.Core.Missions.Core;
using Reverie.Core.Missions.System;
using Reverie.Utilities;
using Reverie.Utilities.Extensions;

using System.Collections.Generic;
using System.IO;
using System.Linq;

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

            ModContent.GetInstance<Reverie>().Logger.Debug($"Mission {mission.Name} progress updated: Set {mission.CurrentIndex}");
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Failed to notify mission update: {ex}");
        }
    }

    private void SyncMissionState(Mission mission)
    {
        if (mission == null) return;

        _ = mission.ToState();
        if (missionDict.ContainsKey(mission.ID))
        {
            missionDict[mission.ID] = mission;
        }
    }

    public bool UpdateMissionProgress(int missionId, int objectiveIndex, int amount = 1)
    {
        if (missionDict.TryGetValue(missionId, out var mission))
        {
            var updated = mission.UpdateProgress(objectiveIndex, amount, Player);
            if (updated)
            {
                SyncMissionState(mission);
            }
            return updated;
        }
        return false;
    }

    private void ResetToCleanState()
    {
        missionDict.Clear();
        notifiedMissions.Clear();
        MissionManager.Instance.Reset();
    }

    public void DebugMissionState()
    {
        foreach (var mission in missionDict.Values)
        {
            Main.NewText($"Mission: {mission.Name}");
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

            var activeMissionData = new List<TagCompound>();
            foreach (var mission in missionDict.Values.Where(m => m.Progress != MissionProgress.Completed))
            {
                var state = mission.ToState();
                var missionData = new TagCompound
                {
                    ["ID"] = mission.ID,
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

            ModContent.GetInstance<Reverie>().Logger.Info($"Successfully saved mission data for {activeMissionData.Count} active missions");
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Failed to save mission data: {ex}");
        }
    }

    public override void LoadData(TagCompound tag)
    {
        savedMissionData = tag;
        ResetToCleanState();
        ModContent.GetInstance<Reverie>().Logger.Info("Mission data stored for deferred loading");
    }

    private void ProcessDeferredLoad()
    {
        if (hasDeferredLoadRun || savedMissionData == null)
        {
            return;
        }

        hasDeferredLoadRun = true;

        try
        {
            ModContent.GetInstance<Reverie>().Logger.Info("Starting deferred mission data loading");

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
                    ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load completed mission {missionId}: {ex.Message}");
                }
            }

            ModContent.GetInstance<Reverie>().Logger.Info($"Loaded {completedMissionsLoaded}/{completedMissionIds.Count} completed missions");

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
                    ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load active mission {missionId}: {ex.Message}");
                }
            }

            ModContent.GetInstance<Reverie>().Logger.Info($"Loaded {activeMissionsLoaded}/{activeMissionData.Count} active missions");

            try
            {
                notifiedMissions.UnionWith([.. savedMissionData.GetList<int>("NotifiedMissions")]);
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load notified missions: {ex.Message}");
            }

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
                    ModContent.GetInstance<Reverie>().Logger.Error($"Failed to register active mission {mission.ID}: {ex.Message}");
                }
            }
            ModContent.GetInstance<Reverie>().Logger.Info($"Registered {missionsRegistered} active missions with manager");

            ValidateStates();
            savedMissionData = null;

            ModContent.GetInstance<Reverie>().Logger.Info("Completed deferred mission loading successfully");
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Critical failure in deferred mission loading: {ex}");
            ResetToCleanState();
        }
    }

    private void ValidateStates()
    {
        foreach (var mission in missionDict.Values)
        {
            try
            {
                if (mission.CurrentIndex < 0 || mission.CurrentIndex >= mission.Objective.Count)
                {
                    ModContent.GetInstance<Reverie>().Logger.Warn($"Fixing invalid CurrentIndex for mission {mission.ID}");
                    mission.CurrentIndex = 0;
                }

                foreach (var set in mission.Objective)
                {
                    foreach (var objective in set.Objectives)
                    {
                        if (objective.CurrentCount > objective.RequiredCount)
                        {
                            ModContent.GetInstance<Reverie>().Logger.Warn($"Objective count exceeds required count, clamping");
                            objective.CurrentCount = objective.RequiredCount;
                        }

                        if (objective.IsCompleted && objective.CurrentCount < objective.RequiredCount)
                        {
                            ModContent.GetInstance<Reverie>().Logger.Warn($"Objective marked complete but count is insufficient");
                            objective.CurrentCount = objective.RequiredCount;
                        }
                    }
                }

                if (mission.Progress == MissionProgress.Completed)
                {
                    bool allSetsComplete = mission.Objective.All(set => set.IsCompleted);
                    if (!allSetsComplete)
                    {
                        ModContent.GetInstance<Reverie>().Logger.Warn($"Mission {mission.ID} marked complete but has incomplete objectives");
                    }
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Error validating mission {mission.ID}: {ex.Message}");
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
            var stateTag = missionTag.GetCompound("State");

            var mission = MissionFactory.Instance.GetMissionData(missionId);
            if (mission == null)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Failed to create mission with ID {missionId}");
                return;
            }

            var progress = (MissionProgress)stateTag.GetInt("Progress");
            var availability = (MissionStatus)stateTag.GetInt("Status");
            var unlocked = stateTag.GetBool("Unlocked");
            var currentIndex = stateTag.GetInt("CurrentIndex");

            if (!Enum.IsDefined(typeof(MissionProgress), progress))
            {
                ModContent.GetInstance<Reverie>().Logger.Warn($"Invalid mission progress value {(int)progress} for mission {missionId}, defaulting to Inactive");
                progress = MissionProgress.Inactive;
            }

            if (!Enum.IsDefined(typeof(MissionStatus), availability))
            {
                ModContent.GetInstance<Reverie>().Logger.Warn($"Invalid mission availability value {(int)availability} for mission {missionId}, defaulting to Locked");
                availability = MissionStatus.Locked;
            }

            if (currentIndex < 0 || currentIndex >= mission.Objective.Count)
            {
                ModContent.GetInstance<Reverie>().Logger.Warn($"Invalid currentIndex {currentIndex} for mission {missionId}, defaulting to 0");
                currentIndex = 0;
            }

            var container = new MissionDataContainer
            {
                ID = missionId,
                Progress = progress,
                Availability = availability,
                Unlocked = unlocked,
                CurObjectiveIndex = currentIndex
            };

            try
            {
                container.ObjectiveIndex = DeserializeObjectives(stateTag.GetList<TagCompound>("Objective"));
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Failed to deserialize objectives for mission {missionId}: {ex.Message}");
                container.ObjectiveIndex = new List<ObjectiveIndexState>();

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

                container.Progress = MissionProgress.Inactive;
            }

            mission.LoadState(container);
            missionDict[missionId] = mission;
            dirtyMissions.Add(missionId);

            ModContent.GetInstance<Reverie>().Logger.Info($"Successfully loaded mission {mission.Name} (ID: {missionId}) with state {mission.Progress}");
        }
        catch (Exception ex)
        {
            var idString = missionTag.ContainsKey("ID") ? missionTag.GetInt("ID").ToString() : "unknown";
            ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load active mission {idString}: {ex.Message}");
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

    #region Mission Access
    public Mission GetMission(int missionId)
    {
        if (missionDict.TryGetValue(missionId, out var mission))
        {
            return mission;
        }

        mission = MissionFactory.Instance.GetMissionData(missionId);
        if (mission != null)
        {
            missionDict[missionId] = mission;
        }
        return mission;
    }

    public void ResetMission(int missionId)
    {
        if (missionDict.TryGetValue(missionId, out var mission))
        {
            mission.Reset();
            mission.Status = MissionStatus.Unlocked;
        }
    }

    public void CompleteMission(int missionId)
    {
        var mission = GetMission(missionId);
        if (mission != null)
        {
            mission.Complete(Player);
            SyncMissionState(mission);
        }
    }

    /// <summary>
    /// Starts a mission for this player. If the mission is mainline, also starts it for all other players via networking.
    /// </summary>
    public void StartMission(int missionId)
    {
        var mission = GetMission(missionId);
        if (mission != null)
        {
            if (mission.IsMainline && Main.netMode != NetmodeID.SinglePlayer)
            {
                // For mainline missions, send packet to start for all players
                Reverie.SendStartMainlineMission(missionId);
            }

            // Always start locally as well
            StartMissionLocal(missionId);
        }
    }

    /// <summary>
    /// Local version of StartMission that doesn't send network packets.
    /// Used by networking system and single player.
    /// </summary>
    public void StartMissionLocal(int missionId)
    {
        var mission = GetMission(missionId);
        if (mission != null)
        {
            mission.Progress = MissionProgress.Ongoing;
            MissionManager.Instance.RegisterMission(mission);
            SyncMissionState(mission);

            mission.OnMissionStart();

            if (Player == Main.LocalPlayer)
            {
                InGameNotificationsTracker.AddNotification(new MissionAcceptNotification(mission));
            }
        }
    }

    /// <summary>
    /// Unlocks a mission for this player. If the mission is mainline, also unlocks it for all other players via networking.
    /// </summary>
    public void UnlockMission(int missionId, bool broadcast = false)
    {
        var mission = GetMission(missionId);
        if (mission != null)
        {
            if (mission.IsMainline && Main.netMode != NetmodeID.SinglePlayer)
            {
                // For mainline missions, send packet to unlock for all players
                Reverie.SendUnlockMainlineMission(missionId, broadcast);
            }

            // Always unlock locally as well
            UnlockMissionLocal(missionId, broadcast);
        }
    }

    /// <summary>
    /// Local version of UnlockMission that doesn't send network packets.
    /// Used by networking system and single player.
    /// </summary>
    public void UnlockMissionLocal(int missionId, bool broadcast = false)
    {
        var mission = GetMission(missionId);
        if (mission != null)
        {
            mission.Status = MissionStatus.Unlocked;
            mission.Progress = MissionProgress.Inactive;
            mission.Unlocked = true;
            SyncMissionState(mission);

            if (broadcast && mission.ProviderNPC > 0 && Player == Main.LocalPlayer)
            {
                var npcName = Lang.GetNPCNameValue(mission.ProviderNPC);
                Main.NewText($"{npcName} has a job opportunity!", Color.CornflowerBlue);
                notifiedMissions.Add(mission.ID);
            }
        }
    }

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
        if (mission != null)
        {
            mission.ProviderNPC = npcType;
            missionDict[missionId] = mission;
            SyncMissionState(mission);
        }
    }

    public void RemoveMissionFromNPC(int npcType, int missionId)
    {
        var mission = GetMission(missionId);
        if (mission != null && mission.ProviderNPC == npcType)
        {
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

            if (!mission.IsMainline && !notifiedMissions.Contains(mission.ID) && broadcast && Player == Main.LocalPlayer)
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
        notifiedMissions.Clear();

        // Handle mainline mission syncing for new players
        SyncMainlineMissionsOnWorldJoin();
    }

    /// <summary>
    /// Syncs mainline missions when a player joins the world.
    /// Unlocks Journey's Begin, and requests sync data from server for multiplayer.
    /// </summary>
    private void SyncMainlineMissionsOnWorldJoin()
    {
        try
        {
            // Always unlock Journey's Begin for new/returning players
            var journeysBegin = GetMission(MissionID.JourneysBegin);
            if (journeysBegin != null && journeysBegin.Status == MissionStatus.Locked)
            {
                UnlockMissionLocal(MissionID.JourneysBegin, broadcast: false);
                ModContent.GetInstance<Reverie>().Logger.Info($"Unlocked Journey's Begin for {Player.name}");
            }

            // In multiplayer, request sync data from server instead of reading other players directly
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Request server to send mainline mission progress
                Reverie.RequestMainlineMissionSync();
                ModContent.GetInstance<Reverie>().Logger.Info($"Requested mainline mission sync from server for {Player.name}");
            }
            // Single player doesn't need syncing
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error syncing mainline missions on world join: {ex.Message}");
        }
    }

    /// <summary>
    /// Receives mainline mission sync data from the server and applies it to catch up this player.
    /// Called by networking system when server sends sync data.
    /// </summary>
    public void ReceiveMainlineMissionSyncData(BinaryReader reader)
    {
        try
        {
            int missionCount = reader.ReadInt32();
            ModContent.GetInstance<Reverie>().Logger.Info($"Receiving mainline mission sync data: {missionCount} missions");

            for (int i = 0; i < missionCount; i++)
            {
                int missionId = reader.ReadInt32();
                int currentIndex = reader.ReadInt32();
                int progress = reader.ReadInt32();
                bool isUnlocked = reader.ReadBoolean();
                bool isStarted = reader.ReadBoolean();

                var missionProgress = (MissionProgress)progress;
                SyncPlayerToMainlineMissionProgress(missionId, currentIndex, missionProgress, isUnlocked, isStarted);
            }

            ModContent.GetInstance<Reverie>().Logger.Info($"Applied mainline mission sync data for {Player.name}");
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error receiving mainline mission sync data: {ex}");
        }
    }

    /// <summary>
    /// Syncs this player's progress on a specific mainline mission to match the server's furthest progress.
    /// </summary>
    private void SyncPlayerToMainlineMissionProgress(int missionId, int targetIndex, MissionProgress targetProgress, bool isUnlocked, bool shouldStart)
    {
        var mission = GetMission(missionId);
        if (mission == null || !mission.IsMainline)
            return;

        try
        {
            // Handle completed missions
            if (targetProgress == MissionProgress.Completed && mission.Progress != MissionProgress.Completed)
            {
                // Mark as completed
                mission.Progress = MissionProgress.Completed;
                mission.Status = MissionStatus.Completed;

                // Complete all objective sets
                foreach (var set in mission.Objective)
                {
                    foreach (var objective in set.Objectives)
                    {
                        objective.CurrentCount = objective.RequiredCount;
                        objective.IsCompleted = true;
                    }
                }

                mission.CurrentIndex = mission.Objective.Count - 1;
                SyncMissionState(mission);

                ModContent.GetInstance<Reverie>().Logger.Info($"Auto-completed mainline mission {mission.Name} for {Player.name} to sync with server");
                return;
            }

            // Handle unlocked/started missions
            if (isUnlocked && mission.Status == MissionStatus.Locked)
            {
                UnlockMissionLocal(missionId, broadcast: false);
                ModContent.GetInstance<Reverie>().Logger.Info($"Auto-unlocked mainline mission {mission.Name} for {Player.name} to sync with server");
            }

            if (shouldStart && mission.Progress == MissionProgress.Inactive && mission.Status == MissionStatus.Unlocked)
            {
                StartMissionLocal(missionId);
                ModContent.GetInstance<Reverie>().Logger.Info($"Auto-started mainline mission {mission.Name} for {Player.name} to sync with server");
            }

            // Catch up to the target objective set if mission is ongoing
            if (mission.Progress == MissionProgress.Ongoing)
            {
                CatchUpMissionProgress(mission, targetIndex);
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error syncing mission {missionId} progress: {ex.Message}");
        }
    }

    /// <summary>
    /// Catches up a mission's progress to a target objective set index.
    /// Instantly completes all previous objective sets so the player is on the same set as others.
    /// IMPORTANT: This performs LOCAL-ONLY updates to avoid triggering multiplayer progress for all players.
    /// </summary>
    private void CatchUpMissionProgress(Mission mission, int targetIndex)
    {
        if (targetIndex <= mission.CurrentIndex)
            return; // Already at or past target

        try
        {
            // CRITICAL: Direct local updates only - bypass UpdateProgress() to avoid multiplayer triggers

            // Complete all objective sets up to (but not including) the target index
            for (int setIndex = 0; setIndex < targetIndex && setIndex < mission.Objective.Count; setIndex++)
            {
                var objectiveSet = mission.Objective[setIndex];

                foreach (var objective in objectiveSet.Objectives)
                {
                    if (!objective.IsCompleted)
                    {
                        // Direct local update - no UpdateProgress() call to avoid multiplayer sync
                        objective.CurrentCount = objective.RequiredCount;
                        objective.IsCompleted = true;
                    }
                }
            }

            // Set the current index to match the target (direct assignment, no progress system)
            mission.CurrentIndex = Math.Min(targetIndex, mission.Objective.Count - 1);

            // Local sync only - this won't trigger multiplayer updates
            SyncMissionState(mission);

            ModContent.GetInstance<Reverie>().Logger.Info($"Caught up mission {mission.Name} progress for {Player.name}: skipped to objective set {targetIndex} (LOCAL ONLY)");
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error catching up mission progress: {ex.Message}");
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