using Reverie.Core.Missions.MissionHandlers;
using System.Collections.Generic;

namespace Reverie.Core.Missions;
public static class MissionID
{
    public const int AFallingStar = 1;
}
public class MissionFactory : ModSystem
{
    private readonly Dictionary<int, Type> missionTypes = new();
    private readonly Dictionary<int, Mission> missionCache = new();
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
                    ModContent.GetInstance<Reverie>().Logger.Error("Failed to get MissionFactory instance!");
                }
            }
            return instance;
        }
    }

    public override void Load()
    {
        instance = this;
        RegisterMissionTypes();
        ModContent.GetInstance<Reverie>().Logger.Info("MissionFactory loaded and initialized");
    }

    public override void OnWorldLoad()
    {
        // Clear cache when loading a world to ensure fresh mission instances
        missionCache.Clear();
        ModContent.GetInstance<Reverie>().Logger.Info("MissionFactory cache cleared for world load");
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
            ModContent.GetInstance<Reverie>().Logger.Info("Registering mission types...");

            // Register each mission type with its ID
            missionTypes[MissionID.AFallingStar] = typeof(AFallingStarMission);

            ModContent.GetInstance<Reverie>().Logger.Info($"Registered {missionTypes.Count} mission types");

            // Log registered missions for debugging
            foreach (var (id, type) in missionTypes)
            {
                ModContent.GetInstance<Reverie>().Logger.Debug($"Registered mission type: {id} -> {type.Name}");
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error registering mission types: {ex}");
        }
    }

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
                ModContent.GetInstance<Reverie>().Logger.Debug($"Creating new instance of mission type: {missionType.Name}");
                var mission = (Mission)Activator.CreateInstance(missionType);
                missionCache[missionId] = mission;
                return mission;
            }

            ModContent.GetInstance<Reverie>().Logger.Warn($"No mission type registered for ID: {missionId}");
            return null;
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error creating mission instance: {ex}");
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