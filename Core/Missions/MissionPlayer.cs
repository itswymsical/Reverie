using Reverie.Common.UI.Missions;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Reverie.Core.Missions;

/// <summary>
/// Handles sideline (non-mainline) missions for individual players.
/// Mainline missions are handled by WorldMissionSystem.
/// Single player only.
/// </summary>
public partial class MissionPlayer : ModPlayer
{
    #region Properties & Fields
    /// <summary>
    /// Dictionary containing only sideline (non-mainline) missions for this player.
    /// Mainline missions are stored in WorldMissionSystem.
    /// </summary>
    public readonly Dictionary<int, Mission> sidelineMissions = [];

    private readonly HashSet<int> notifiedMissions = [];
    private TagCompound savedMissionData = null;
    private bool hasDeferredLoadRun = false;
    #endregion

    #region Mission Logic
    public void NotifyMissionUpdate(Mission mission)
    {
        if (mission == null) return;

        try
        {
            if (mission.IsMainline)
            {
                ModContent.GetInstance<Reverie>().Logger.Debug($"Mainline mission {mission.Name} progress updated");
            }
            else
            {
                sidelineMissions[mission.ID] = mission;
                ModContent.GetInstance<Reverie>().Logger.Debug($"Sideline mission {mission.Name} progress updated");
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Failed to notify mission update: {ex}");
        }
    }

    public bool UpdateMissionProgress(int missionId, int objectiveIndex, int amount = 1)
    {
        var mission = GetMission(missionId);
        if (mission != null)
        {
            if (mission.IsMainline)
            {
                return MissionWorld.Instance.UpdateProgress(missionId, objectiveIndex, amount);
            }
            else
            {
                var updated = mission.UpdateProgress(objectiveIndex, amount);
                if (updated)
                {
                    sidelineMissions[missionId] = mission;
                }
                return updated;
            }
        }
        return false;
    }

    private void ResetToCleanState()
    {
        sidelineMissions.Clear();
        notifiedMissions.Clear();
        MissionManager.Instance.Reset();
    }
    #endregion

    #region Serialization
    public override void SaveData(TagCompound tag)
    {
        try
        {
            var sidelineMissionData = new List<TagCompound>();

            foreach (var mission in sidelineMissions.Values.Where(m => !m.IsMainline))
            {
                var missionData = new TagCompound
                {
                    ["ID"] = mission.ID,
                    ["Progress"] = (int)mission.Progress,
                    ["Status"] = (int)mission.Status,
                    ["Unlocked"] = mission.Unlocked,
                    ["CurrentIndex"] = mission.CurrentIndex,
                    ["Objectives"] = SerializeObjectives(mission.Objective)
                };
                sidelineMissionData.Add(missionData);
            }

            tag["SidelineMissions"] = sidelineMissionData;
            tag["CompletedSidelineMissionIDs"] = CompletedSidelineMissions().Select(m => m.ID).ToList();
            tag["NotifiedMissions"] = notifiedMissions.ToList();

            ModContent.GetInstance<Reverie>().Logger.Info($"Saved {sidelineMissionData.Count} sideline missions for player");
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Failed to save player mission data: {ex}");
        }
    }

    public override void LoadData(TagCompound tag)
    {
        savedMissionData = tag;
        ResetToCleanState();
        ModContent.GetInstance<Reverie>().Logger.Info("Player mission data stored for deferred loading");
    }

    private void ProcessDeferredLoad()
    {
        if (hasDeferredLoadRun || savedMissionData == null)
            return;

        hasDeferredLoadRun = true;

        try
        {
            ModContent.GetInstance<Reverie>().Logger.Info("Loading player sideline mission data");

            var completedMissionIds = savedMissionData.GetList<int>("CompletedSidelineMissionIDs");
            foreach (var missionId in completedMissionIds)
            {
                try
                {
                    LoadCompletedSidelineMission(missionId);
                }
                catch (Exception ex)
                {
                    ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load completed sideline mission {missionId}: {ex}");
                }
            }

            var sidelineMissionData = savedMissionData.GetList<TagCompound>("SidelineMissions");
            foreach (var missionTag in sidelineMissionData)
            {
                try
                {
                    LoadActiveSidelineMission(missionTag);
                }
                catch (Exception ex)
                {
                    ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load active sideline mission: {ex}");
                }
            }

            try
            {
                notifiedMissions.UnionWith([.. savedMissionData.GetList<int>("NotifiedMissions")]);
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load notification history: {ex}");
            }

            foreach (var mission in ActiveSidelineMissions())
            {
                try
                {
                    MissionManager.Instance.RegisterMission(mission);
                }
                catch (Exception ex)
                {
                    ModContent.GetInstance<Reverie>().Logger.Error($"Failed to register sideline mission {mission.ID}: {ex}");
                }
            }

            savedMissionData = null;
            ModContent.GetInstance<Reverie>().Logger.Info("Completed player mission loading");
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Critical failure in player mission loading: {ex}");
            ResetToCleanState();
        }
    }

    private void LoadActiveSidelineMission(TagCompound missionTag)
    {
        try
        {
            var missionId = missionTag.GetInt("ID");
            var mission = MissionFactory.Instance.GetMissionData(missionId);

            if (mission?.IsMainline == false)
            {
                mission.Progress = (MissionProgress)missionTag.GetInt("Progress");
                mission.Status = (MissionStatus)missionTag.GetInt("Status");
                mission.Unlocked = missionTag.GetBool("Unlocked");
                mission.CurrentIndex = missionTag.GetInt("CurrentIndex");

                LoadObjectiveSets(mission, missionTag.GetList<TagCompound>("Objectives"));
                sidelineMissions[missionId] = mission;

                ModContent.GetInstance<Reverie>().Logger.Info($"Loaded sideline mission {mission.Name}");
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load sideline mission: {ex}");
        }
    }

    private void LoadCompletedSidelineMission(int missionId)
    {
        var mission = MissionFactory.Instance.GetMissionData(missionId);
        if (mission?.IsMainline == false)
        {
            mission.Progress = MissionProgress.Completed;
            mission.Status = MissionStatus.Completed;
            sidelineMissions[missionId] = mission;
        }
    }

    private static List<TagCompound> SerializeObjectives(List<ObjectiveSet> objectiveSets)
    {
        var serializedSets = new List<TagCompound>();

        foreach (var set in objectiveSets)
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

    private static void LoadObjectiveSets(Mission mission, IList<TagCompound> serializedSets)
    {
        for (var i = 0; i < Math.Min(mission.Objective.Count, serializedSets.Count); i++)
        {
            var setTag = serializedSets[i];
            var currentSet = mission.Objective[i];
            var objectiveTags = setTag.GetList<TagCompound>("Objectives");

            currentSet.HasCheckedInitialInventory = setTag.GetBool("HasCheckedInventory");

            foreach (var currentObj in currentSet.Objectives)
            {
                var matchingObjTag = objectiveTags.FirstOrDefault(tag =>
                    tag.GetString("Description").Equals(currentObj.Description, StringComparison.OrdinalIgnoreCase));

                if (matchingObjTag != null)
                {
                    currentObj.IsCompleted = matchingObjTag.GetBool("IsCompleted");
                    currentObj.CurrentCount = Math.Min(matchingObjTag.GetInt("CurrentCount"), currentObj.RequiredCount);

                    if (currentObj.IsCompleted && currentObj.CurrentCount < currentObj.RequiredCount)
                    {
                        currentObj.CurrentCount = currentObj.RequiredCount;
                    }
                }
            }
        }
    }
    #endregion

    #region Mission Access
    /// <summary>
    /// Gets a mission from either world data (mainline) or player data (sideline).
    /// </summary>
    public Mission GetMission(int missionId)
    {
        var mainlineMission = MissionWorld.Instance.GetMainlineMission(missionId);
        if (mainlineMission != null)
            return mainlineMission;

        if (sidelineMissions.TryGetValue(missionId, out var sidelineMission))
            return sidelineMission;

        var mission = MissionFactory.Instance.GetMissionData(missionId);
        if (mission != null)
        {
            if (mission.IsMainline)
            {
                return mission;
            }
            else
            {
                sidelineMissions[missionId] = mission;
                return mission;
            }
        }

        return null;
    }

    public void StartMission(int missionId)
    {
        var mission = GetMission(missionId);
        if (mission != null)
        {
            if (mission.IsMainline)
            {
                MissionWorld.Instance.StartMission(missionId);
            }
            else
            {
                StartSidelineMission(missionId);
            }
        }
    }

    public void UnlockMission(int missionId, bool broadcast = false)
    {
        var mission = GetMission(missionId);
        if (mission != null)
        {
            if (mission.IsMainline)
            {
                MissionWorld.Instance.UnlockMission(missionId, broadcast);
            }
            else
            {
                UnlockSidelineMission(missionId, broadcast);
            }
        }
    }

    public void CompleteMission(int missionId)
    {
        var mission = GetMission(missionId);
        if (mission != null)
        {
            if (mission.IsMainline)
            {
                MissionWorld.Instance.CompleteMission(missionId);
            }
            else
            {
                CompleteSidelineMission(missionId);
            }
        }
    }

    private void StartSidelineMission(int missionId)
    {
        var mission = GetMission(missionId);
        if (mission?.IsMainline == false)
        {
            mission.Progress = MissionProgress.Ongoing;
            MissionManager.Instance.RegisterMission(mission);
            sidelineMissions[missionId] = mission;
            mission.OnMissionStart();

            InGameNotificationsTracker.AddNotification(new MissionAcceptNotification(mission));
        }
    }

    private void UnlockSidelineMission(int missionId, bool broadcast = false)
    {
        var mission = GetMission(missionId);
        if (mission?.IsMainline == false)
        {
            mission.Status = MissionStatus.Unlocked;
            mission.Progress = MissionProgress.Inactive;
            mission.Unlocked = true;
            sidelineMissions[missionId] = mission;

            if (broadcast && mission.ProviderNPC > 0)
            {
                var npcName = Lang.GetNPCNameValue(mission.ProviderNPC);
                Main.NewText($"{npcName} has a job opportunity!", Color.CornflowerBlue);
                notifiedMissions.Add(mission.ID);
            }
        }
    }

    private void CompleteSidelineMission(int missionId)
    {
        var mission = GetMission(missionId);
        if (mission?.IsMainline == false && mission.Progress == MissionProgress.Ongoing)
        {
            mission.Complete();
            sidelineMissions[missionId] = mission;
        }
    }

    public void ResetMission(int missionId)
    {
        var mission = GetMission(missionId);
        if (mission != null)
        {
            mission.Reset();
            mission.Status = MissionStatus.Unlocked;

            if (mission.IsMainline)
            {
                // Mainline mission reset is handled by world system
            }
            else
            {
                sidelineMissions[missionId] = mission;
            }
        }
    }

    public IEnumerable<Mission> AvailableMissions()
    {
        var mainlineAvailable = MissionWorld.Instance.GetAllMainlineMissions()
            .Where(m => m.Status == MissionStatus.Unlocked && m.Progress == MissionProgress.Inactive);

        var sidelineAvailable = sidelineMissions.Values
            .Where(m => m.Status == MissionStatus.Unlocked && m.Progress == MissionProgress.Inactive);

        return mainlineAvailable.Concat(sidelineAvailable);
    }

    public IEnumerable<Mission> ActiveMissions()
    {
        var mainlineActive = MissionWorld.Instance.GetAllMainlineMissions()
            .Where(m => m.Progress == MissionProgress.Ongoing);

        var sidelineActive = sidelineMissions.Values
            .Where(m => m.Progress == MissionProgress.Ongoing);

        return mainlineActive.Concat(sidelineActive);
    }

    public IEnumerable<Mission> CompletedMissions()
    {
        var mainlineCompleted = MissionWorld.Instance.GetAllMainlineMissions()
            .Where(m => m.Progress == MissionProgress.Completed);

        var sidelineCompleted = sidelineMissions.Values
            .Where(m => m.Progress == MissionProgress.Completed);

        return mainlineCompleted.Concat(sidelineCompleted);
    }

    private IEnumerable<Mission> CompletedSidelineMissions()
    {
        return sidelineMissions.Values.Where(m => m.Progress == MissionProgress.Completed);
    }

    private IEnumerable<Mission> ActiveSidelineMissions()
    {
        return sidelineMissions.Values.Where(m => m.Progress == MissionProgress.Ongoing);
    }

    public bool NPCHasAvailableMission(int npcType, bool broadcast = false)
    {
        var hasAvailableMission = false;

        foreach (var mission in AvailableMissions().Where(m => m.ProviderNPC == npcType))
        {
            hasAvailableMission = true;

            if (!mission.IsMainline && !notifiedMissions.Contains(mission.ID) && broadcast)
            {
                var npcName = Lang.GetNPCNameValue(mission.ProviderNPC);
                Main.NewText($"{npcName} has a job opportunity!", Color.CornflowerBlue);
                notifiedMissions.Add(mission.ID);
            }
        }

        return hasAvailableMission;
    }
    #endregion

    #region Mainline Mission UI Callbacks
    // These are much simpler now - just UI updates for single player

    public void OnMainlineMissionUpdated(Mission mission)
    {
        //SoundEngine.PlaySound(SoundID.MenuTick, Main.LocalPlayer.position);
    }

    public void OnStartedMainline(Mission mission)
    {
        InGameNotificationsTracker.AddNotification(new MissionAcceptNotification(mission));
    }

    public void OnUnlockedMainline(Mission mission)
    {
        if (mission.ProviderNPC > 0)
        {
            var npcName = Lang.GetNPCNameValue(mission.ProviderNPC);
            Main.NewText($"{npcName} has new information!", Color.CornflowerBlue);
            notifiedMissions.Add(mission.ID);
        }
    }

    public void OnCompletedMainline(Mission mission)
    {
        InGameNotificationsTracker.AddNotification(new MissionCompleteNotification(mission));
    }
    #endregion

    #region Init & Update
    public override void OnEnterWorld()
    {
        ProcessDeferredLoad();
        notifiedMissions.Clear();

        ModContent.GetInstance<Reverie>().Logger.Info($"Player {Player.name} entered world with {sidelineMissions.Count} sideline missions");
    }

    public override void PostUpdate()
    {
        base.PostUpdate();

        if (!hasDeferredLoadRun)
        {
            ProcessDeferredLoad();
        }
    }
    #endregion
}