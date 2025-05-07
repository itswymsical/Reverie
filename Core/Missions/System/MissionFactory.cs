using Reverie.Content.Missions;
using Reverie.Content.Missions.Argie;
using Reverie.Content.Missions.Demolitionist;
using Reverie.Content.Missions.Merchant;
using Reverie.Core.Missions.Core;
using Reverie.Utilities;
using System.Collections.Generic;

namespace Reverie.Core.Missions.System;

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