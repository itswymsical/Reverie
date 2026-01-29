using Reverie.Common.Commands;
using Reverie.Common.UI.Missions;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Reverie.Core.Missions;

/// <summary>
/// Stores sideline missions (player-specific).
/// Handles events for sideline missions.
/// </summary>
public partial class MissionPlayer : ModPlayer
{
    private List<Mission> sidelineMissions = new();

    #region Event Handling
    public void OnMissionEvent(MissionEvent evt)
    {
        foreach (var mission in sidelineMissions)
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

        for (var i = 0; i < objectives.Count; i++)
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
        var existing = sidelineMissions.FirstOrDefault(m => m.ID == missionId);
        if (existing != null)
            return existing;

        // Create new mission instance
        var mission = MissionSystem.CreateMission(missionId);
        if (mission != null && !mission.IsMainline)
        {
            sidelineMissions.Add(mission);
            return mission;
        }

        return null;
    }

    public void StartMission(int missionId)
    {
        var mission = GetOrCreateMission(missionId);
        if (mission?.Status == MissionStatus.Unlocked)
        {
            mission.Progress = MissionProgress.Ongoing;
            mission.OnMissionStart();

            if (Player.whoAmI == Main.myPlayer)
            {
                InGameNotificationsTracker.AddNotification(new MissionAcceptNotification(mission));
            }
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
        var mission = sidelineMissions.FirstOrDefault(m => m.ID == missionId);
        mission?.Complete();
    }

    public IEnumerable<Mission> ActiveMissions() =>
        sidelineMissions.Where(m => m.Progress == MissionProgress.Ongoing);

    public IEnumerable<Mission> AvailableMissions() =>
        sidelineMissions.Where(m => m.Status == MissionStatus.Unlocked);

    public IEnumerable<Mission> GetAllMissions() => sidelineMissions;

    public bool HasAvailableMissions() => AvailableMissions().Any();
    #endregion

    #region Persistence
    public override void SaveData(TagCompound tag)
    {
        var missionData = new List<TagCompound>();

        foreach (var mission in sidelineMissions)
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

    public override void LoadData(TagCompound tag)
    {
        sidelineMissions.Clear();

        var missionData = tag.GetList<TagCompound>("Missions");

        foreach (var missionTag in missionData)
        {
            var missionId = missionTag.GetInt("ID");
            var mission = MissionSystem.CreateMission(missionId);

            if (mission != null && !mission.IsMainline)
            {
                mission.Progress = (MissionProgress)missionTag.GetInt("Progress");
                mission.Status = (MissionStatus)missionTag.GetInt("Status");
                mission.CurrentList = missionTag.GetInt("CurrentList");

                DeserializeObjectives(mission.ObjectiveList, missionTag.GetList<TagCompound>("Objectives"));

                sidelineMissions.Add(mission);
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
        for (var i = 0; i < Math.Min(objectiveSets.Count, savedSets.Count); i++)
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

    #region UI Notifications
    public void NotifyMissionUpdate(Mission mission) { }
    public void OnStartedMainline(Mission mission) { }
    public void OnUnlockedMainline(Mission mission) { }
    public void OnCompletedMainline(Mission mission) { }
    public void OnMainlineMissionUpdated(Mission mission) { }
    public bool NPCHasAvailableMission(int npcType) =>
        AvailableMissions().Any(m => m.ProviderNPC == npcType);

    private bool notificationExists = false;

    public override void OnEnterWorld()
    {
        base.OnEnterWorld();
        //if (!DownedSystem.initialCutscene && Main.netMode != NetmodeID.MultiplayerClient)
        //    CutsceneSystem.PlayCutscene<ArrivalCutscene>();

        MissionCommandHelper.GetOrCreateMission(MissionID.JourneysBegin, Player);
        notificationExists = false;
    }

    public override void PostUpdate()
    {
        var sidelineMissions = this.ActiveMissions()
            .Concat(this.AvailableMissions());

        var mainlineMissions = MissionWorld.Instance.GetAllMissions()
            .Where(m => m.Progress == MissionProgress.Ongoing || m.Status == MissionStatus.Unlocked);

        var allMissions = sidelineMissions.Concat(mainlineMissions).ToList();

        bool hasMissions = allMissions.Any();

        if (hasMissions && !notificationExists)
        {
            var currentMission = allMissions.FirstOrDefault();

            if (currentMission != null)
            {
                MissionSidebarManager.Instance.SetNotification(new MissionSidebar(currentMission));
                notificationExists = true;
            }
        }
        else if (!hasMissions)
        {
            MissionSidebarManager.Instance.ClearNotification();
            notificationExists = false;
        }
    }
    #endregion
}