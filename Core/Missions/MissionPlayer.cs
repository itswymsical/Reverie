using Reverie.Common.UI.Missions;
using Reverie.Utilities;
using Reverie.Utilities.Extensions;

using System.Collections.Generic;
using System.Linq;

using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Reverie.Core.Missions;

public partial class MissionPlayer : ModPlayer
{
    #region Properties & Fields
    public readonly Dictionary<int, Mission> missionDict = [];

    private readonly HashSet<int> notifiedMissions = [];

    private readonly HashSet<int> dirtyMissions = [];

    private int check = 0;

    private TagCompound savedMissionData = null;
    private bool isWorldFullyLoaded = false;
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

        // Create or update mission state in storage
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
            var updated = mission.UpdateProgress(objectiveIndex, amount);
            if (updated)
            {
                // Sync the updated state
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
            Main.NewText($"  State: {mission.Availability}");
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
                        ["Availability"] = (int)mission.Availability,
                        ["Unlocked"] = mission.Unlocked,
                        ["CurrentIndex"] = mission.CurrentIndex,
                        ["Objective"] = SerializeObjectives(mission.Objective)
                    }
                };
                activeMissionData.Add(missionData);
            }
            tag["ActiveMissions"] = activeMissionData;

            // Remove the duplicate loop

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
        // Only store the data, don't process it yet
        savedMissionData = tag;

        // Reset to clean state to avoid any partial loading issues
        ResetToCleanState();

        ModContent.GetInstance<Reverie>().Logger.Info("Mission data stored for deferred loading");
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
            ModContent.GetInstance<Reverie>().Logger.Info("Starting deferred mission data loading");

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
                    ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load completed mission {missionId}: {ex.Message}");
                }
            }

            ModContent.GetInstance<Reverie>().Logger.Info($"Loaded {completedMissionsLoaded}/{completedMissionIds.Count} completed missions");

            // Load active missions
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

            // Load notified missions
            try
            {
                notifiedMissions.UnionWith([.. savedMissionData.GetList<int>("NotifiedMissions")]);
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load notified missions: {ex.Message}");
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
                    ModContent.GetInstance<Reverie>().Logger.Error($"Failed to register active mission {mission.ID}: {ex.Message}");
                }
            }
            ModContent.GetInstance<Reverie>().Logger.Info($"Registered {missionsRegistered} active missions with manager");

            // Validate mission state consistency
            ValidateStates();

            // Clear saved data to free memory
            savedMissionData = null;

            // Mark world as fully loaded
            isWorldFullyLoaded = true;

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
                // Ensure current index is valid
                if (mission.CurrentIndex < 0 || mission.CurrentIndex >= mission.Objective.Count)
                {
                    ModContent.GetInstance<Reverie>().Logger.Warn($"Fixing invalid CurrentIndex for mission {mission.ID}");
                    mission.CurrentIndex = 0;
                }

                // Validate objective completion states
                foreach (var set in mission.Objective)
                {
                    bool allCompleted = set.Objectives.All(o => o.IsCompleted);
                    bool anyIncomplete = set.Objectives.Any(o => !o.IsCompleted);

                    // If all objectives are complete but the set isn't marked complete
                    if (allCompleted && !set.IsCompleted)
                    {
                        ModContent.GetInstance<Reverie>().Logger.Warn($"Found completed objective set not marked as complete");
                        // The IsCompleted implementation is a computed property, so this is just for logging
                    }

                    // Validate counts are within bounds
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

                // Check if the mission progress state is valid
                if (mission.Progress == MissionProgress.Completed)
                {
                    // Ensure all objectives are actually complete
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

            // Validate that we can create this mission
            var mission = MissionFactory.Instance.GetMissionData(missionId);
            if (mission == null)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Failed to create mission with ID {missionId}");
                return;
            }

            // Gather state data
            var progress = (MissionProgress)stateTag.GetInt("Progress");
            var availability = (MissionAvailability)stateTag.GetInt("Availability");
            var unlocked = stateTag.GetBool("Unlocked");
            var currentIndex = stateTag.GetInt("CurrentIndex");

            // Validate state data
            if (!Enum.IsDefined(typeof(MissionProgress), progress))
            {
                ModContent.GetInstance<Reverie>().Logger.Warn($"Invalid mission progress value {(int)progress} for mission {missionId}, defaulting to Inactive");
                progress = MissionProgress.Inactive;
            }

            if (!Enum.IsDefined(typeof(MissionAvailability), availability))
            {
                ModContent.GetInstance<Reverie>().Logger.Warn($"Invalid mission availability value {(int)availability} for mission {missionId}, defaulting to Locked");
                availability = MissionAvailability.Locked;
            }

            if (currentIndex < 0 || currentIndex >= mission.Objective.Count)
            {
                ModContent.GetInstance<Reverie>().Logger.Warn($"Invalid currentIndex {currentIndex} for mission {missionId}, defaulting to 0");
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
                ModContent.GetInstance<Reverie>().Logger.Error($"Failed to deserialize objectives for mission {missionId}: {ex.Message}");
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
            mission.Availability = MissionAvailability.Completed;
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
        if (missionDict.TryGetValue(missionId, out var mission))
        {
            mission.Reset();
            mission.Availability = MissionAvailability.Unlocked;
        }
    }

    public void CompleteMission(int missionId)
    {
        var mission = GetMission(missionId);
        if (mission != null)
        {
            mission.Complete();
            SyncMissionState(mission);
        }
    }

    public void StartMission(int missionId)
    {
        var mission = GetMission(missionId);
        if (mission != null)
        {
            mission.Progress = MissionProgress.Active;
            MissionManager.Instance.RegisterMission(mission);
            SyncMissionState(mission);
            mission.OnMissionStart();

            if (!mission.IsMainline)
            {
                InGameNotificationsTracker.AddNotification(new MissionAcceptNotification(mission));
            }
        }
    }

    public void UnlockMission(int missionId)
    {
        var mission = GetMission(missionId);
        if (mission != null)
        {
            mission.Availability = MissionAvailability.Unlocked;
            mission.Progress = MissionProgress.Inactive;
            mission.Unlocked = true;
            SyncMissionState(mission);
        }
    }

    public IEnumerable<Mission> AvailableMissions()
        => missionDict.Values.Where
        (m => m.Availability == MissionAvailability.Unlocked && m.Progress == MissionProgress.Inactive);

    public IEnumerable<Mission> ActiveMissions()
    {
        foreach (var mission in missionDict.Values.Where(m => m.Progress == MissionProgress.Active))
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

    public bool NPCHasAvailableMission(int npcType)
    {
        foreach (var mission in missionDict.Values.Where(m =>
            m.ProviderNPC == npcType &&
            m.Availability == MissionAvailability.Unlocked)) // Only show for inactive missions
        {
            if (!mission.IsMainline && !notifiedMissions.Contains(mission.ID))
            {
                var npcName = Lang.GetNPCNameValue(mission.ProviderNPC);
                Main.NewText($"{npcName} has a job opportunity!", Color.CornflowerBlue);
                notifiedMissions.Add(mission.ID);
            }
            return true;
        }
        return false;
    }

    #endregion

    #region Objective Tracking & Mission Handlers
    public override void OnEnterWorld()
    {
        ProcessDeferredLoad();
        notifiedMissions.Clear();

        var fallingStar = GetMission(MissionID.AFallingStar);

        if (fallingStar != null && fallingStar.Availability != MissionAvailability.Completed 
            && fallingStar.Progress != MissionProgress.Active)
        {
            UnlockMission(MissionID.AFallingStar);
            StartMission(MissionID.AFallingStar);
        }
    }

    public override void PostUpdate()
    {
        base.PostUpdate();
        PlayerTriggerEvents();

        if (!hasDeferredLoadRun)
        {
            ProcessDeferredLoad();
        }

        MissionManager.Instance.OnUpdate();

        if (dirtyMissions.Count > 0)
        {
            dirtyMissions.Clear();
        }
        check++;
        if (check > 300)
        {
            foreach (var biome in Enum.GetValues<BiomeType>())
            {
                if (biome.IsPlayerInBiome(Player))
                {
                    MissionManager.Instance.OnBiomeEnter(Player, biome);
                }
            }
            check = 0;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitNPC(target, hit, damageDone);

        MissionManager.Instance.OnNPCHit(target, damageDone);
    }

    #endregion
}