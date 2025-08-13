using System.Collections.Generic;

namespace Reverie.Core.Missions;

/// <summary>
/// Manages active missions for single player.
/// Tracks which missions are currently running and need event handling.
/// </summary>
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
            if (activeMissions.ContainsKey(mission.ID))
            {
                Reverie.Instance.Logger.Debug($"Mission already registered: {mission.ID}");
                return;
            }

            activeMissions[mission.ID] = mission;
            Reverie.Instance.Logger.Info($"Registered mission: {mission.Name}");
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

    public void UnregisterMission(int missionId)
    {
        if (activeMissions.Remove(missionId))
        {
            Reverie.Instance.Logger.Info($"Unregistered mission: {missionId}");
        }
    }
    #endregion

    #region Mission Management
    public void Reset()
    {
        activeMissions.Clear();
        Reverie.Instance.Logger.Info("All active missions reset");
    }

    public Mission GetActiveMission(int missionId)
    {
        return activeMissions.TryGetValue(missionId, out var mission) ? mission : null;
    }

    public IEnumerable<Mission> GetAllActiveMissions()
    {
        return activeMissions.Values;
    }

    public bool IsRegistered(int missionId)
    {
        return activeMissions.ContainsKey(missionId);
    }

    /// <summary>
    /// Called when an objective is completed for a mission.
    /// Single player version - no need for player parameter.
    /// </summary>
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

    /// <summary>
    /// Updates all active missions. Called during world update.
    /// </summary>
    public void UpdateActiveMissions()
    {
        foreach (var mission in activeMissions.Values)
        {
            try
            {
                mission.Update();
            }
            catch (Exception ex)
            {
                Reverie.Instance.Logger.Error($"Error updating mission {mission.Name}: {ex.Message}");
            }
        }
    }
    #endregion
}