using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Reverie.Utilities;
using Reverie.Content.Missions;

namespace Reverie.Core.Missions;

public partial class MissionManager
{
    private readonly Dictionary<int, Mission> activeMissions = [];
    private static MissionManager instance;
    public static MissionManager Instance => instance ??= new MissionManager();

    public void RegisterMission(Mission mission)
    {
        if (mission == null)
            return;

        try
        {
            ModContent.GetInstance<Reverie>().Logger.Debug($"Active mission count before registration: {activeMissions.Count}");

            if (activeMissions.ContainsKey(mission.ID))
            {
                ModContent.GetInstance<Reverie>().Logger.Debug($"Mission already registered: {mission.ID}");
                return;
            }

            activeMissions[mission.ID] = mission;
            ModContent.GetInstance<Reverie>().Logger.Info($"Registered mission: {mission.Name}");
            ModContent.GetInstance<Reverie>().Logger.Debug($"Active mission count after registration: {activeMissions.Count}");
            ModContent.GetInstance<Reverie>().Logger.Debug($"Current active missions: {string.Join(", ", activeMissions.Keys)}");
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Failed to register mission: {ex.Message}");
        }
    }

    public void Reset()
    {
        activeMissions.Clear();
        ModContent.GetInstance<Reverie>().Logger.Info("All active missions reset");
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
                activeMission.OnObjectiveComplete(objectiveIndex);
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnObjectiveComplete: {ex.Message}");
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
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnItemCreated: {ex.Message}");
        }
    }

    public void OnItemObtained(Item item)
    {
        try
        {
            foreach (var mission in ActiveMissions)
            {
                mission.OnItemObtained(item);
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnItemObtained: {ex.Message}");
        }
    }

    public void OnNPCKill(NPC npc)
    {
        try
        {
            foreach (var mission in ActiveMissions)
            {
                mission.OnNPCKill(npc);
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnNPCKill: {ex.Message}");
        }
    }

    public void OnNPCChat(NPC npc)
    {
        try
        {
            foreach (var mission in ActiveMissions)
            {
                mission.OnNPCChat(npc);
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnNPCChat: {ex.Message}");
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
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnNPCSpawn: {ex.Message}");
        }
    }

    public void OnNPCLoot(NPC npc)
    {
        try
        {
            foreach (var mission in ActiveMissions)
            {
                mission.OnNPCLoot(npc);
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnNPCLoot: {ex.Message}");
        }
    }

    public void OnNPCHit(NPC npc, int damage)
    {
        try
        {
            var missions = ActiveMissions.ToList();
            ModContent.GetInstance<Reverie>().Logger.Debug($"Processing NPC hit with {missions.Count} active missions");

            foreach (var mission in missions)
            {
                mission.OnNPCHit(npc, damage);
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnNPCHit: {ex.Message}");
        }
    }

    public void OnBiomeEnter(Player player, BiomeType biome)
    {
        try
        {
            foreach (var mission in ActiveMissions)
            {
                mission.OnBiomeEnter(player, biome);
                ModContent.GetInstance<Reverie>().Logger.Debug($"OnBiomeEnter: {biome}");
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnBiomeEnter: {ex.Message}");
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
            missionTypes[MissionID.A_FALLING_STAR] = typeof(AFallingStar);
            missionTypes[MissionID.BUILD_VALID_HOUSE] = typeof(HousingMission);
           

            ModContent.GetInstance<Reverie>().Logger.Info($"Registered {missionTypes.Count} mission types");

            foreach (var (id, type) in missionTypes)
                ModContent.GetInstance<Reverie>().Logger.Debug($"Registered mission type: {id} -> {type.Name}");
            
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