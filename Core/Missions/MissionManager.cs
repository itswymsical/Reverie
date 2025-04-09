using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Reverie.Utilities;
using Reverie.Content.Missions;
using Reverie.Content.Missions.Argie;
using Reverie.Content.Missions.Merchant;

namespace Reverie.Core.Missions;

public partial class MissionManager
{
    private readonly Dictionary<int, Mission> activeMissions = [];
    private static MissionManager instance;
    public static MissionManager Instance => instance ??= new MissionManager();

    private bool isWorldFullyLoaded = false;
    private readonly HashSet<int> pendingRegistrations = [];

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

    public void Reset()
    {
        activeMissions.Clear();
        Reverie.Instance.Logger.Info("All active missions reset");
    }

    private IEnumerable<Mission> ActiveMissions
    {
        get
        {
            return activeMissions.Values
                .Where(m => m.Progress == MissionProgress.Active)
                .ToList();
        }
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

    #region Event Handlers
    public void OnItemCreated(Item item, ItemCreationContext context)
    {
        try
        {
            foreach (var mission in ActiveMissions)
            {
                mission.OnItemCreated(item, context);
            }
        }
        catch (Exception ex)
        {
            Reverie.Instance.Logger.Error($"Error in OnItemCreated: {ex.Message}");
        }
    }

    public void OnItemObtained(Item item)
    {
        try
        {
            foreach (var mission in ActiveMissions)
            {
                mission.OnCollected(item);
            }
        }
        catch (Exception ex)
        {
            Reverie.Instance.Logger.Error($"Error in OnCollected: {ex.Message}");
        }
    }

    public void OnNPCKill(NPC npc)
    {
        try
        {
            foreach (var mission in ActiveMissions)
            {
                mission.OnKill(npc);
            }
        }
        catch (Exception ex)
        {
            Reverie.Instance.Logger.Error($"Error in OnKill: {ex.Message}");
        }
    }

    public void OnNPCChat(NPC npc)
    {
        try
        {
            foreach (var mission in ActiveMissions)
            {
                mission.OnChat(npc);
            }
        }
        catch (Exception ex)
        {
            Reverie.Instance.Logger.Error($"Error in OnChat: {ex.Message}");
        }
    }

    public void OnNPCSpawn(NPC npc)
    {
        try
        {
            foreach (var mission in ActiveMissions)
            {
                mission.OnNPCSpawn(npc);
            }
        }
        catch (Exception ex)
        {
            Reverie.Instance.Logger.Error($"Error in OnNPCSpawn: {ex.Message}");
        }
    }

    public void OnNPCHit(NPC npc, int damage)
    {
        try
        {
            var missions = ActiveMissions.ToList();
            Reverie.Instance.Logger.Debug($"Processing NPC hit with {missions.Count} active missions");

            foreach (var mission in missions)
            {
                mission.OnHitTarget(npc, damage);
            }
        }
        catch (Exception ex)
        {
            Reverie.Instance.Logger.Error($"Error in OnHitTarget: {ex.Message}");
        }
    }

    public void OnBiomeEnter(Player player, BiomeType biome)
    {
        try
        {
            foreach (var mission in ActiveMissions)
            {
                mission.OnBiomeEnter(player, biome);
                Reverie.Instance.Logger.Debug($"OnBiomeEnter: {biome}");
            }
        }
        catch (Exception ex)
        {
            Reverie.Instance.Logger.Error($"Error in OnBiomeEnter: {ex.Message}");
        }
    }
    public void OnBreakTile(int type, ref bool fail, ref bool effectOnly)
    {
        try
        {
            foreach (var mission in ActiveMissions)
            {
                mission.OnBreakTile(type, ref fail, ref effectOnly);
                Reverie.Instance.Logger.Debug($"OnBreakTile: {type}");
            }
        }
        catch (Exception ex)
        {
            Reverie.Instance.Logger.Error($"Error in OnBreakTile: {ex.Message}");
        }
    }
    #endregion
}

public class MissionFactory : ModSystem
{
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