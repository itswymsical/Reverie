using Reverie.Core.Missions.Core;
using Reverie.Core.Missions.SystemClasses;
using Reverie.Common.UI.Missions;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.Audio;
using Terraria.ID;

namespace Reverie.Core.Missions;

/// <summary>
/// Container for mainline missions, it handles all mainline mission operations directly.
/// </summary>
public class WorldMissionSystem : ModSystem
{
    #region Fields & Properties
    private readonly Dictionary<int, Mission> mainlineMissions = [];
    private static WorldMissionSystem instance;
    public static WorldMissionSystem Instance => instance ??= ModContent.GetInstance<WorldMissionSystem>();
    #endregion

    #region Initialization
    public override void Load()
    {
        instance = this;
    }

    public override void Unload()
    {
        mainlineMissions.Clear();
        instance = null;
    }

    public override void OnWorldLoad()
    {
        mainlineMissions.Clear();

        MissionFactory.Instance.ClearCache();

        MissionManager.Instance.Reset();

        ModContent.GetInstance<Reverie>().Logger.Info("WorldMissionSystem: Cleared all mainline missions for new world");

        var journeysBegin = MissionFactory.Instance.GetMissionData(MissionID.JourneysBegin);
        if (journeysBegin?.IsMainline == true)
        {
            journeysBegin.Status = MissionStatus.Unlocked;
            journeysBegin.Unlocked = true;
            mainlineMissions[MissionID.JourneysBegin] = journeysBegin;
            ModContent.GetInstance<Reverie>().Logger.Info("WorldMissionSystem: Journey's Begin unlocked for new world");
        }
    }

    public override void OnWorldUnload()
    {
        // Clean shutdown - clear data
        // Event handlers will be cleaned up by missions themselves when they're destroyed
        mainlineMissions.Clear();
        ModContent.GetInstance<Reverie>().Logger.Info("WorldMissionSystem: Cleaned up all mainline missions on world unload");
    }

    public override void PostUpdateWorld()
    {
        // Update all active mainline missions
        foreach (var mission in mainlineMissions.Values.Where(m => m.Progress == MissionProgress.Ongoing))
        {
            mission.Update();
        }
    }
    #endregion

    #region Mission Access
    public Mission GetMainlineMission(int missionId)
    {
        if (mainlineMissions.TryGetValue(missionId, out var mission))
        {
            return mission;
        }

        var newMission = MissionFactory.Instance.GetMissionData(missionId);
        if (newMission?.IsMainline == true)
        {
            mainlineMissions[missionId] = newMission;
            return newMission;
        }

        return null;
    }

    public IEnumerable<Mission> GetAllMainlineMissions() => mainlineMissions.Values;

    public bool HasMainlineMission(int missionId) => mainlineMissions.ContainsKey(missionId);
    #endregion


    public bool UpdateProgress(int missionId, int objectiveIndex, int amount = 1, Player triggeringPlayer = null)
    {
        var mission = GetMainlineMission(missionId);
        if (mission?.Progress == MissionProgress.Ongoing)
        {
            triggeringPlayer ??= Main.LocalPlayer;

            var progressUpdated = UpdateProgressInternal(mission, objectiveIndex, amount, triggeringPlayer);

            if (progressUpdated)
            {
                NotifyUpdate(mission);
            }

            return progressUpdated;
        }
        return false;
    }

    /// <summary>
    /// Internal method that updates mission progress once at world level.
    /// </summary>
    private bool UpdateProgressInternal(Mission mission, int objectiveIndex, int amount, Player triggeringPlayer)
    {
        if (mission.Progress != MissionProgress.Ongoing)
            return false;

        var currentSet = mission.Objective[mission.CurrentIndex];
        if (objectiveIndex >= 0 && objectiveIndex < currentSet.Objectives.Count)
        {
            var obj = currentSet.Objectives[objectiveIndex];
            if (!obj.IsCompleted || amount < 0)
            {
                var wasCompleted = obj.UpdateProgress(amount);

                if (wasCompleted && amount > 0)
                {
                    mission.HandleObjectiveCompletion(objectiveIndex, triggeringPlayer);
                    if (triggeringPlayer == Main.LocalPlayer)
                    {
                        SoundEngine.PlaySound(SoundID.MenuTick, triggeringPlayer.position);
                    }
                }

                if (currentSet.IsCompleted)
                {
                    if (mission.CurrentIndex < mission.Objective.Count - 1)
                    {
                        mission.CurrentIndex++;
                        return true;
                    }
                    if (mission.Objective.All(set => set.IsCompleted))
                    {
                        CompleteMission(mission.ID, triggeringPlayer);
                        return true;
                    }
                }
                return true;
            }
        }
        return false;
    }

    public void StartMission(int missionId)
    {
        var mission = GetMainlineMission(missionId);
        if (mission?.Status == MissionStatus.Unlocked)
        {
            mission.Progress = MissionProgress.Ongoing;
            MissionManager.Instance.RegisterMission(mission);
            mission.OnMissionStart();

            NotifyStart(mission);
        }
    }

    public void UnlockMission(int missionId, bool broadcast = false)
    {
        var mission = GetMainlineMission(missionId);
        if (mission?.Status == MissionStatus.Locked)
        {
            mission.Status = MissionStatus.Unlocked;
            mission.Unlocked = true;

            if (broadcast && mission.ProviderNPC > 0)
            {
                var npcName = Lang.GetNPCNameValue(mission.ProviderNPC);
                Main.NewText($"{npcName} has a job opportunity!", Color.CornflowerBlue);
            }

            // Notify all players
            NotifyUnlock(mission);
        }
    }

    public void CompleteMission(int missionId, Player completingPlayer)
    {
        var mission = GetMainlineMission(missionId);
        if (mission?.Progress == MissionProgress.Ongoing && mission.Objective.All(set => set.IsCompleted))
        {
            mission.Progress = MissionProgress.Completed;
            mission.Status = MissionStatus.Completed;

            mission.OnMissionComplete(completingPlayer);

            NotifyComplete(mission);
        }
    }

    #region Notifications (UI Updates)
    private void NotifyUpdate(Mission mission)
    {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            if (Main.player[i].active)
            {
                var missionPlayer = Main.player[i].GetModPlayer<MissionPlayer>();
                missionPlayer.OnMainlineMissionUpdated(mission);
            }
        }
    }

    private void NotifyStart(Mission mission)
    {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            if (Main.player[i].active)
            {
                var missionPlayer = Main.player[i].GetModPlayer<MissionPlayer>();
                missionPlayer.OnStartedMainline(mission);
            }
        }
    }

    private void NotifyUnlock(Mission mission)
    {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            if (Main.player[i].active)
            {
                var missionPlayer = Main.player[i].GetModPlayer<MissionPlayer>();
                missionPlayer.OnUnlockedMainline(mission);
            }
        }
    }

    private void NotifyComplete(Mission mission)
    {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            if (Main.player[i].active)
            {
                var missionPlayer = Main.player[i].GetModPlayer<MissionPlayer>();
                missionPlayer.OnCompletedMainline(mission);
            }
        }
    }
    #endregion

    #region Serialization
    public override void SaveWorldData(TagCompound tag)
    {
        try
        {
            var mainlineMissionData = new List<TagCompound>();

            foreach (var mission in mainlineMissions.Values)
            {
                var missionData = new TagCompound
                {
                    ["ID"] = mission.ID,
                    ["Progress"] = (int)mission.Progress,
                    ["Status"] = (int)mission.Status,
                    ["Unlocked"] = mission.Unlocked,
                    ["CurrentIndex"] = mission.CurrentIndex,
                    ["Objectives"] = SerializeObjectiveSets(mission.Objective)
                };
                mainlineMissionData.Add(missionData);
            }

            tag["MainlineMissions"] = mainlineMissionData;
            ModContent.GetInstance<Reverie>().Logger.Info($"Saved {mainlineMissionData.Count} mainline missions to world data");
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Failed to save world mission data: {ex}");
        }
    }

    public override void LoadWorldData(TagCompound tag)
    {
        try
        {
            mainlineMissions.Clear();

            var mainlineMissionData = tag.GetList<TagCompound>("MainlineMissions");
            int missionsLoaded = 0;

            foreach (var missionTag in mainlineMissionData)
            {
                try
                {
                    int missionId = missionTag.GetInt("ID");
                    var mission = MissionFactory.Instance.GetMissionData(missionId);

                    if (mission?.IsMainline == true)
                    {
                        mission.Progress = (MissionProgress)missionTag.GetInt("Progress");
                        mission.Status = (MissionStatus)missionTag.GetInt("Status");
                        mission.Unlocked = missionTag.GetBool("Unlocked");
                        mission.CurrentIndex = missionTag.GetInt("CurrentIndex");

                        LoadObjectiveSets(mission, missionTag.GetList<TagCompound>("Objectives"));

                        mainlineMissions[missionId] = mission;
                        missionsLoaded++;

                        if (mission.Progress == MissionProgress.Ongoing)
                        {
                            MissionManager.Instance.RegisterMission(mission);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load mainline mission: {ex}");
                }
            }

            ModContent.GetInstance<Reverie>().Logger.Info($"Loaded {missionsLoaded} mainline missions from world data");

            // Ensure Journey's Begin is always available
            if (!mainlineMissions.ContainsKey(MissionID.JourneysBegin))
            {
                var journeysBegin = MissionFactory.Instance.GetMissionData(MissionID.JourneysBegin);
                if (journeysBegin?.IsMainline == true)
                {
                    journeysBegin.Status = MissionStatus.Unlocked;
                    journeysBegin.Unlocked = true;
                    mainlineMissions[MissionID.JourneysBegin] = journeysBegin;
                }
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load world mission data: {ex}");
        }
    }

    private static List<TagCompound> SerializeObjectiveSets(List<ObjectiveSet> objectiveSets)
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
        for (int i = 0; i < Math.Min(mission.Objective.Count, serializedSets.Count); i++)
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
}