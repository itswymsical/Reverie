using Reverie.Common.Commands;
using Reverie.Common.UI.Missions;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Reverie.Core.Missions;

/// <summary>
/// Stores mainline missions (shared across characters in single player).
/// Handles events for mainline missions.
/// </summary>
public class MissionWorld : ModSystem
{
    private readonly Dictionary<int, Mission> mainlineMissions = new();
    private static MissionWorld instance;
    public static MissionWorld Instance => instance ??= ModContent.GetInstance<MissionWorld>();

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
    }

    #region Event Handling
    public void OnMissionEvent(MissionEvent evt)
    {
        foreach (var mission in mainlineMissions.Values)
        {
            if (mission.Progress != MissionProgress.Ongoing)
                continue;

            ProcessEventForMission(mission, evt);
        }
    }

    private void ProcessEventForMission(Mission mission, MissionEvent evt)
    {
        var currentSet = mission.CurrentList;
        if (currentSet < 0 || currentSet >= mission.ObjectiveList.Count)
            return;

        var objectives = mission.ObjectiveList[currentSet].Objective;

        for (int i = 0; i < objectives.Count; i++)
        {
            if (objectives[i].IsCompleted)
                continue;

            if (mission.MatchesEvent(evt, currentSet, i))
            {
                mission.OnMatchedEvent(evt, i);
            }
        }
    }
    #endregion

    #region Mission Access
    public Mission GetOrCreateMission(int missionId)
    {
        if (mainlineMissions.TryGetValue(missionId, out var mission))
            return mission;

        // Create new mission instance
        mission = MissionSystem.CreateMission(missionId);
        if (mission?.IsMainline == true)
        {
            mainlineMissions[missionId] = mission;
            return mission;
        }

        return null;
    }

    public IEnumerable<Mission> GetAllMissions() => mainlineMissions.Values;

    public void StartMission(int missionId)
    {
        var mission = GetOrCreateMission(missionId);
        if (mission?.Status == MissionStatus.Unlocked)
        {
            mission.Progress = MissionProgress.Ongoing;
            mission.OnMissionStart();

            InGameNotificationsTracker.AddNotification(new MissionAcceptNotification(mission));
        }
    }

    public void UnlockMission(int missionId)
    {
        var mission = GetOrCreateMission(missionId);
        if (mission?.Status == MissionStatus.Locked)
        {
            mission.Status = MissionStatus.Unlocked;
        }
    }

    public void CompleteMission(int missionId)
    {
        var mission = GetOrCreateMission(missionId);
        if (mission != null)
        {
            mission.Complete();
            InGameNotificationsTracker.AddNotification(new MissionCompleteNotification(mission));
        }
    }
    #endregion

    #region Persistence
    public override void SaveWorldData(TagCompound tag)
    {
        var missionData = new List<TagCompound>();

        foreach (var mission in mainlineMissions.Values)
        {
            missionData.Add(new TagCompound
            {
                ["ID"] = mission.ID,
                ["Progress"] = (int)mission.Progress,
                ["Status"] = (int)mission.Status,
                ["CurrentList"] = mission.CurrentList,
                ["Objectives"] = SerializeObjectives(mission.ObjectiveList)
            });
        }

        tag["Missions"] = missionData;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        mainlineMissions.Clear();

        var missionData = tag.GetList<TagCompound>("Missions");

        foreach (var missionTag in missionData)
        {
            var missionId = missionTag.GetInt("ID");
            var mission = MissionSystem.CreateMission(missionId);

            if (mission?.IsMainline == true)
            {
                mission.Progress = (MissionProgress)missionTag.GetInt("Progress");
                mission.Status = (MissionStatus)missionTag.GetInt("Status");
                mission.CurrentList = missionTag.GetInt("CurrentList");

                DeserializeObjectives(mission.ObjectiveList, missionTag.GetList<TagCompound>("Objectives"));

                mainlineMissions[missionId] = mission;
            }
        }
    }

    private List<TagCompound> SerializeObjectives(List<ObjectiveList> objectiveSets)
    {
        var result = new List<TagCompound>();

        foreach (var set in objectiveSets)
        {
            var objectives = new List<TagCompound>();

            foreach (var obj in set.Objective)
            {
                objectives.Add(new TagCompound
                {
                    ["Desc"] = obj.Description,
                    ["Done"] = obj.IsCompleted,
                    ["Count"] = obj.Count,
                    ["Required"] = obj.RequiredCount
                });
            }

            result.Add(new TagCompound { ["Objs"] = objectives });
        }

        return result;
    }

    private void DeserializeObjectives(List<ObjectiveList> objectiveSets, IList<TagCompound> savedSets)
    {
        for (int i = 0; i < Math.Min(objectiveSets.Count, savedSets.Count); i++)
        {
            var savedObjs = savedSets[i].GetList<TagCompound>("Objs");
            var currentObjs = objectiveSets[i].Objective;

            foreach (var currentObj in currentObjs)
            {
                var match = savedObjs.FirstOrDefault(s =>
                    s.GetString("Desc") == currentObj.Description);

                if (match != null)
                {
                    currentObj.IsCompleted = match.GetBool("Done");
                    currentObj.Count = match.GetInt("Count");
                }
            }
        }
    }
    #endregion
}