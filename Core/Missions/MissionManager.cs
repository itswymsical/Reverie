using Reverie.Content.Missions;
using Reverie.Content.Missions.Argie;
using Reverie.Content.Missions.Demolitionist;
using Reverie.Content.Missions.Merchant;
using Reverie.Utilities;
using System.Collections.Generic;

namespace Reverie.Core.Missions;

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

public class MissionFactory : ModSystem
{
    #region Properties and Fields
    private readonly Dictionary<int, Type> missionTypes = [];
    private readonly Dictionary<int, Mission> missionCache = [];
    private static MissionFactory instance;

    public static MissionFactory Instance
    {
        get
        {
            if (instance == null)
            {
                instance = ModContent.GetInstance<MissionFactory>();
                if (instance == null)
                {
                    Reverie.Instance.Logger.Error("Failed to get MissionFactory instance!");
                }
            }
            return instance;
        }
    }
    #endregion

    #region Initialization & Registration
    public override void Load()
    {
        instance = this;
        RegisterMissionTypes();
        Reverie.Instance.Logger.Info("MissionFactory loaded and initialized");
    }

    public override void OnWorldLoad()
    {
        // Clear cache when loading a world to ensure fresh mission instances
        missionCache.Clear();
        Reverie.Instance.Logger.Info("MissionFactory cache cleared for world load");
    }

    public override void Unload()
    {
        missionTypes.Clear();
        missionCache.Clear();
        instance = null;
    }

    private void RegisterMissionTypes()
    {
        try
        {
            Reverie.Instance.Logger.Info("Registering mission types...");

            #region Missions
            missionTypes[MissionID.AFallingStar] = typeof(AFallingStar);
            missionTypes[MissionID.BloomcapHunt] = typeof(BloomcapHunt);
            missionTypes[MissionID.CopperStandard] = typeof(CopperStandard);
            missionTypes[MissionID.LightEmUp] = typeof(LightEmUp);
            #endregion

            Reverie.Instance.Logger.Info($"Registered {missionTypes.Count} mission types");

            foreach (var (id, type) in missionTypes)
            {

                Reverie.Instance.Logger.Debug($"Registered mission type: {id} -> {type.Name}");
            }
        }
        catch (Exception ex)
        {
            Reverie.Instance.Logger.Error($"Error registering mission types: {ex}");
        }
    }
    #endregion

    public Mission GetMissionData(int missionId)
    {
        try
        {
            // Check cache first
            if (missionCache.TryGetValue(missionId, out var cachedMission))
            {
                return cachedMission;
            }

            // Create new instance if type is registered
            if (missionTypes.TryGetValue(missionId, out var missionType))
            {
                Reverie.Instance.Logger.Debug($"Creating new instance of mission type: {missionType.Name}");
                var mission = (Mission)Activator.CreateInstance(missionType);
                missionCache[missionId] = mission;
                return mission;
            }

            Reverie.Instance.Logger.Warn($"No mission type registered for ID: {missionId}");
            return null;
        }
        catch (Exception ex)
        {
            Reverie.Instance.Logger.Error($"Error creating mission instance: {ex}");
            return null;
        }
    }

    public void LoadMissionState(int missionId, MissionDataContainer state)
    {
        var mission = GetMissionData(missionId);
        if (mission != null)
        {
            mission.LoadState(state);
            missionCache[missionId] = mission;
        }
    }

    public void Reset()
    {
        missionCache.Clear();
    }
}