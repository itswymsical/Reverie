using Reverie.Common.Players;
using Reverie.Common.Systems;
using Reverie.Core.Cinematics.Cutscenes;
using Reverie.Utilities;
using Reverie.Utilities.Extensions;

using System.Collections.Generic;
using System.IO;
using System.Linq;

using Terraria.ModLoader.IO;

namespace Reverie.Core.Missions;

public partial class MissionPlayer : ModPlayer
{
    #region Properties & Fields
    public readonly Dictionary<int, Mission> missionDict = [];

    public readonly Dictionary<int, List<int>> npcMissionsDict = [];

    private readonly HashSet<int> notifiedMissions = [];

    private readonly HashSet<int> dirtyMissions = [];
    private int check = 0;
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
        npcMissionsDict.Clear();
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

    public override void ResetEffects()
    {
        base.ResetEffects();

        notifiedMissions.Clear();
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
                        ["ObjectiveIndex"] = SerializeObjectives(mission.ObjectiveIndex)
                    }
                };
                activeMissionData.Add(missionData);
            }
            tag["ActiveMissions"] = activeMissionData;

            tag["CompletedMissionIDs"] = GetCompletedMissions().Select(m => m.ID).ToList();
            tag["NotifiedMissions"] = notifiedMissions.ToList();

            var npcMissionData = npcMissionsDict.Select(kvp => new TagCompound
            {
                ["NpcType"] = kvp.Key,
                ["MissionIDs"] = kvp.Value
            }).ToList();
            tag["NPCMissions"] = npcMissionData;
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Failed to save mission data: {ex}");
        }
    }

    public override void LoadData(TagCompound tag)
    {
        try
        {
            ResetToCleanState();

            // Load completed missions first
            var completedMissionIds = tag.GetList<int>("CompletedMissionIDs");
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
            var activeMissionData = tag.GetList<TagCompound>("ActiveMissions").ToList();

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
                notifiedMissions.UnionWith([.. tag.GetList<int>("NotifiedMissions")]);
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load notified missions: {ex.Message}");
            }

            // Load NPC mission assignments
            try
            {
                var npcMissionData = tag.GetList<TagCompound>("NPCMissions").ToList();
                foreach (var npcTag in npcMissionData)
                {
                    try
                    {
                        var npcType = npcTag.GetInt("NpcType");
                        var missionIds = npcTag.GetList<int>("MissionIDs").ToList();
                        npcMissionsDict[npcType] = missionIds;
                    }
                    catch (Exception ex)
                    {
                        ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load NPC mission assignment: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load NPC mission assignments: {ex.Message}");
            }

            // Re-register active missions
            int missionsRegistered = 0;
            foreach (var mission in GetActiveMissions())
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
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Critical failure loading mission data: {ex}");
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
                if (mission.CurrentIndex < 0 || mission.CurrentIndex >= mission.ObjectiveIndex.Count)
                {
                    ModContent.GetInstance<Reverie>().Logger.Warn($"Fixing invalid CurrentIndex for mission {mission.ID}");
                    mission.CurrentIndex = 0;
                }

                // Validate objective completion states
                foreach (var set in mission.ObjectiveIndex)
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
                    bool allSetsComplete = mission.ObjectiveIndex.All(set => set.IsCompleted);
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

            // Create container from saved state
            var container = new MissionDataContainer
            {
                ID = missionId,
                Progress = (MissionProgress)stateTag.GetInt("Progress"),
                Availability = (MissionAvailability)stateTag.GetInt("Availability"),
                Unlocked = stateTag.GetBool("Unlocked"),
                CurObjectiveIndex = stateTag.GetInt("CurrentIndex")
            };

            // Load objective data with validation
            try
            {
                container.ObjectiveIndex = DeserializeObjectives(stateTag.GetList<TagCompound>("ObjectiveIndex"));
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Failed to deserialize objectives for mission {missionId}: {ex.Message}");
                container.ObjectiveIndex = new List<ObjectiveIndexState>();

                // Create placeholder empty states
                for (int i = 0; i < mission.ObjectiveIndex.Count; i++)
                {
                    var set = new ObjectiveIndexState();
                    foreach (var obj in mission.ObjectiveIndex[i].Objectives)
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

            // Register with handler manager if active
            if (mission.Progress == MissionProgress.Active)
            {
                MissionManager.Instance.RegisterMission(mission);
            }
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
        // Check local cache first
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

    public IEnumerable<Mission> GetAvailableMissions()
        => missionDict.Values.Where
        (m => m.Availability == MissionAvailability.Unlocked && m.Progress == MissionProgress.Inactive);

    public IEnumerable<Mission> GetActiveMissions()
    {
        foreach (var mission in missionDict.Values.Where(m => m.Progress == MissionProgress.Active))
        {
            // Ensure we're working with the most up-to-date mission state
            if (dirtyMissions.Contains(mission.ID))
            {
                mission.LoadState(mission.ToState());
            }
            yield return mission;
        }
    }

    public IEnumerable<Mission> GetCompletedMissions()
        => missionDict.Values.Where
        (m => m.Progress == MissionProgress.Completed);
    #endregion

    #region NPC Mission Logic    
    public void AssignMissionToNPC(int npcType, int missionId)
    {
        if (!npcMissionsDict.TryGetValue(npcType, out var missionIds))
        {
            missionIds = [];
            npcMissionsDict[npcType] = missionIds;
        }

        if (!missionIds.Contains(missionId))
        {
            missionIds.Add(missionId);
        }
    }

    public void RemoveMissionFromNPC(int npcType, int missionId)
    {
        if (npcMissionsDict.TryGetValue(npcType, out var missionIds))
        {
            missionIds.Remove(missionId);
        }
    }

    public static bool NPCHasAvailableMission(MissionPlayer missionPlayer, int npcType)
    {
        if (missionPlayer.npcMissionsDict.TryGetValue(npcType, out var missionIds))
        {
            foreach (var missionId in missionIds)
            {
                var mission = missionPlayer.GetMission(missionId);
                if (mission != null && mission.Availability == MissionAvailability.Unlocked && mission.Progress != MissionProgress.Completed)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool NPCHasAvailableMission(int npcType)
    {
        if (npcMissionsDict.TryGetValue(npcType, out var missionIds))
        {
            foreach (var missionId in missionIds)
            {
                var mission = GetMission(missionId);
                if (mission != null && mission.Availability == MissionAvailability.Unlocked && mission.Progress == MissionProgress.Inactive)
                {
                    if (!mission.IsMainline)
                    {
                        var npcName = Lang.GetNPCNameValue(mission.Employer);
                        Main.NewText($"{npcName} has a job opportunity!", Color.Yellow);
                        notifiedMissions.Add(missionId);
                    }
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region Objective Tracking & Mission Handlers

    public override void PostUpdate()
    {
        base.PostUpdate();

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

    public override void OnEnterWorld()
    {
        var AFallingStar = GetMission(MissionID.A_FALLING_STAR);
        var player = Main.LocalPlayer.GetModPlayer<ReveriePlayer>();

        if (AFallingStar != null &&
            AFallingStar.Availability != MissionAvailability.Completed &&
            AFallingStar.Progress != MissionProgress.Active)
        {
            CutsceneSystem.PlayCutscene(new FallingStarCutscene());
            UnlockMission(MissionID.A_FALLING_STAR);
            StartMission(MissionID.A_FALLING_STAR);
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitNPC(target, hit, damageDone);

        MissionManager.Instance.OnNPCHit(target, damageDone);
    }

    #endregion
}