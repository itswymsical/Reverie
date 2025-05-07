using Reverie.Core.Missions.Core;
using System.Collections.Generic;

namespace Reverie.Core.Missions.System;

public partial class MissionManager
{
    #region Properties and Fields
    private readonly Dictionary<int, Mission> activeMissions = [];
    private static MissionManager instance;
    public static MissionManager Instance => instance ??= new MissionManager();

    private bool isWorldFullyLoaded = false;
    private readonly HashSet<int> pendingRegistrations = [];
    #endregion

    #region Initialization & Registration
    public void OnWorldLoad()
    {
        activeMissions.Clear();
        pendingRegistrations.Clear();
        isWorldFullyLoaded = false;
        Reverie.Instance.Logger.Info("MissionManager reset for world load");
    }

    public void OnWorldFullyLoaded()
    {
        isWorldFullyLoaded = true;

        // Register any pending missions
        foreach (var missionId in pendingRegistrations)
        {
            var mission = MissionFactory.Instance.GetMissionData(missionId);
            if (mission != null)
            {
                RegisterMissionInternal(mission);
            }
        }

        pendingRegistrations.Clear();
        Reverie.Instance.Logger.Info("MissionManager marked world as fully loaded");
    }

    private void RegisterMissionInternal(Mission mission)
    {
        try
        {
            Reverie.Instance.Logger.Debug($"Active mission count before registration: {activeMissions.Count}");

            if (activeMissions.ContainsKey(mission.ID))
            {
                Reverie.Instance.Logger.Debug($"Mission already registered: {mission.ID}");
                return;
            }

            activeMissions[mission.ID] = mission;
            Reverie.Instance.Logger.Info($"Registered mission: {mission.Name}");
            Reverie.Instance.Logger.Debug($"Active mission count after registration: {activeMissions.Count}");
            Reverie.Instance.Logger.Debug($"Current active missions: {string.Join(", ", activeMissions.Keys)}");
        }
        catch (Exception ex)
        {
            Reverie.Instance.Logger.Error($"Failed to register mission: {ex.Message}");
        }
    }

    public void RegisterMission(Mission mission)
    {
        if (mission == null)
            return;

        if (!isWorldFullyLoaded)
        {
            Reverie.Instance.Logger.Info($"Queueing mission {mission.ID} for registration after world load");
            pendingRegistrations.Add(mission.ID);
            return;
        }

        RegisterMissionInternal(mission);
    }
    #endregion

    public void Reset()
    {
        activeMissions.Clear();
        Reverie.Instance.Logger.Info("All active missions reset");
    }

    public void OnObjectiveComplete(Mission mission, int objectiveIndex)
    {
        try
        {
            if (activeMissions.TryGetValue(mission.ID, out var activeMission))
            {
                activeMission.HandleObjectiveCompletion(objectiveIndex);
            }
        }
        catch (Exception ex)
        {
            Reverie.Instance.Logger.Error($"Error in HandleObjectiveCompletion: {ex.Message}");
        }
    }

}
