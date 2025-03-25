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

// located in core for namespace convenience.
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

            ModContent.GetInstance<Reverie>().Logger.Debug($"Mission {mission.Name} progress updated: Set {mission.CurObjectiveIndex}");
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
                var MissionAvailability = new TagCompound
                {
                    ["ID"] = mission.ID,
                    ["State"] = new TagCompound
                    {
                        ["Progress"] = (int)mission.Progress,
                        ["Availability"] = (int)mission.Availability,
                        ["Unlocked"] = mission.Unlocked,
                        ["CurObjectiveIndex"] = mission.CurObjectiveIndex,
                        ["ObjectiveIndex"] = SerializeObjectives(mission.ObjectiveIndex)
                    }
                };
                activeMissionData.Add(MissionAvailability);
            }
            tag["ActiveMissions"] = activeMissionData;

            foreach (var mission in missionDict.Values.Where(m => m.Progress != MissionProgress.Completed))
            {
                var MissionAvailability = new TagCompound
                {
                    ["ID"] = mission.ID,
                    ["State"] = new TagCompound
                    {
                        ["Progress"] = (int)mission.Progress,
                        ["Availability"] = (int)mission.Availability,
                        ["Unlocked"] = mission.Unlocked,
                        ["CurObjectiveIndex"] = mission.CurObjectiveIndex,
                        ["ObjectiveIndex"] = SerializeObjectives(mission.ObjectiveIndex)
                    }
                };
                activeMissionData.Add(MissionAvailability);
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
            foreach (var missionId in completedMissionIds)
            {
                LoadCompletedMission(missionId);
            }

            // Load active missions
            var activeMissionData = tag.GetList<TagCompound>("ActiveMissions").ToList();
            foreach (var missionTag in activeMissionData)
            {
                LoadActiveMission(missionTag);
            }

            notifiedMissions.UnionWith([.. tag.GetList<int>("NotifiedMissions")]);

            var npcMissionData = tag.GetList<TagCompound>("NPCMissions").ToList();
            foreach (var npcTag in npcMissionData)
            {
                var npcType = npcTag.GetInt("NpcType");
                var missionIds = npcTag.GetList<int>("MissionIDs").ToList();
                npcMissionsDict[npcType] = missionIds;
            }

            // Re-register active missions
            foreach (var mission in GetActiveMissions())
            {
                MissionManager.Instance.RegisterMission(mission);
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load mission data: {ex}");
            ResetToCleanState();
        }
    }

    private static List<TagCompound> SerializeObjectives(List<ObjectiveSet> ObjectiveIndex)
    {
        var serializedSets = new List<TagCompound>();
        foreach (var set in ObjectiveIndex)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            set.WriteData(writer);
            serializedSets.Add(new TagCompound
            {
                ["SetData"] = ms.ToArray()
            });
        }
        return serializedSets;
    }
    private static List<ObjectiveIndexState> DeserializeObjectives(IList<TagCompound> serializedSets)
    {
        var objectiveStates = new List<ObjectiveIndexState>();

        foreach (var setTag in serializedSets)
        {
            var setData = setTag.GetByteArray("SetData");
            using var ms = new MemoryStream(setData);
            using var reader = new BinaryReader(ms);

            var objectiveSet = new ObjectiveIndexState();
            int count = reader.ReadInt32();
            bool hasCheckedInventory = reader.ReadBoolean();

            for (int i = 0; i < count; i++)
            {
                var objective = new ObjectiveState
                {
                    Description = reader.ReadString(),
                    IsCompleted = reader.ReadBoolean(),
                    RequiredCount = reader.ReadInt32(),
                    CurrentCount = reader.ReadInt32()
                };
                objectiveSet.Objectives.Add(objective);
            }

            objectiveStates.Add(objectiveSet);
        }

        return objectiveStates;
    }

    private void LoadActiveMission(TagCompound missionTag)
    {
        try
        {
            var missionId = missionTag.GetInt("ID");
            var stateTag = missionTag.GetCompound("State");

            // Create container from saved state
            var container = new MissionDataContainer
            {
                ID = missionId,
                Progress = (MissionProgress)stateTag.GetInt("Progress"),
                Availability = (MissionAvailability)stateTag.GetInt("Availability"),
                Unlocked = stateTag.GetBool("Unlocked"),
                CurObjectiveIndex = stateTag.GetInt("CurObjectiveIndex"),
                ObjectiveIndex = DeserializeObjectives(stateTag.GetList<TagCompound>("ObjectiveIndex"))
            };

            // Get or create mission instance and load state
            var mission = MissionFactory.Instance.GetMissionData(missionId);
            if (mission != null)
            {
                mission.LoadState(container);
                missionDict[missionId] = mission;

                // Register with handler manager if active
                if (mission.Progress == MissionProgress.Active)
                {
                    MissionManager.Instance.RegisterMission(mission);
                }
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load active mission: {ex.Message}");
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

    public void StartNextMission(Mission completedMission)
    {
        var nextMissionID = completedMission.NextMissionID;
        if (nextMissionID != -1)
        {
            var nextMission = GetMission(nextMissionID);
            if (nextMission != null)
            {
                if (nextMission.Progress != MissionProgress.Completed &&
                    nextMission.Progress != MissionProgress.Active)
                {
                    nextMission.Availability = MissionAvailability.Unlocked;
                    nextMission.Progress = MissionProgress.Active;
                    StartMission(nextMissionID);
                    Main.NewText($"New mission available: {nextMission.Name}", Color.Yellow);
                }
            }
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
        var AFallingStar = GetMission(MissionID.BUILD_VALID_HOUSE);
        var player = Main.LocalPlayer.GetModPlayer<ReveriePlayer>();

        if (AFallingStar != null &&
            AFallingStar.Availability != MissionAvailability.Completed &&
            AFallingStar.Progress != MissionProgress.Active)
        {
            //CutsceneSystem.PlayCutscene(new FallingStarCutscene());
            UnlockMission(MissionID.BUILD_VALID_HOUSE);
            StartMission(MissionID.BUILD_VALID_HOUSE);
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitNPC(target, hit, damageDone);

        MissionManager.Instance.OnNPCHit(target, damageDone);
    }

    #endregion
}