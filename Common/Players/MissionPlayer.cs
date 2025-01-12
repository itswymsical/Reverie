using Microsoft.Xna.Framework;
using Reverie.Common.MissionAttributes;
using Reverie.Common.Systems;
using Reverie.Content.Terraria.Missions.Mainline;
using Reverie.Core.Cutscenes;
using Reverie.Core.Missions;
using Reverie.Cutscenes;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Reverie.Common.Players
{
    public partial class MissionPlayer : ModPlayer
    {
        public static MissionPlayer Instance => ModContent.GetInstance<MissionPlayer>();
        public Dictionary<int, MissionData> missionDataDict = [];
        public Dictionary<int, Mission> missionDict = [];
        public Dictionary<int, List<int>> npcMissionsDict = [];
        public MissionDataFactory missionDataFactory = new();
        public bool flag;

        public Mission GetMission(int missionId)
        {
            if (!missionDict.TryGetValue(missionId, out var mission))
            {
                if (!missionDataDict.TryGetValue(missionId, out var missionData))
                {
                    missionData = missionDataFactory.GetMissionData(missionId);
                    if (missionData != null)
                    {
                        missionDataDict[missionId] = missionData;
                    }
                }

                if (missionData != null)
                {
                    mission = new Mission(missionData);
                    missionDict[missionId] = mission;
                }
            }
            return mission;
        }

        public void ResetMission(int missionId)
        {
            if (missionDict.TryGetValue(missionId, out var mission))
            {
                mission.Reset();
                mission.State = MissionState.Unlocked;
            }
        }
        public void DebugMissionStates()
        {
            foreach (var mission in missionDict.Values)
            {
                Main.NewText($"Mission: {mission.MissionData.Name}");
                Main.NewText($"  State: {mission.State}");
                Main.NewText($"  Progress: {mission.Progress}");
                Main.NewText($"  NextMissionID: {mission.MissionData.NextMissionID}");
            }
        }

        public void StartNextMission(Mission completedMission)
        {
            int nextMissionID = completedMission.MissionData.NextMissionID;
            if (nextMissionID != -1)
            {
                Mission nextMission = GetMission(nextMissionID);
                if (nextMission != null)
                {
                    if (nextMission.Progress != MissionProgress.Completed &&
                        nextMission.Progress != MissionProgress.Active)
                    {
                        nextMission.State = MissionState.Unlocked;
                        nextMission.Progress = MissionProgress.Active;
                        StartMission(nextMissionID);  // Add this line to register the handler
                        Main.NewText($"New mission available: {nextMission.MissionData.Name}", Color.Yellow);
                    }
                }
            }
        }

        public void UnlockMission(int missionId)
        {
            Mission mission = GetMission(missionId);
            if (mission != null)
            {
                mission.State = MissionState.Unlocked;
                mission.Progress = MissionProgress.Inactive;
                mission.Unlocked = true;
            }
        }

        public void StartMission(int missionId)
        {
            var mission = GetMission(missionId);
            mission.Progress = MissionProgress.Active;
            MissionHandlerManager.Instance.RegisterMissionHandler(mission);
        }

        public IEnumerable<Mission> GetAvailableMissions()
            => missionDict.Values.Where
            (m => m.State == MissionState.Unlocked && m.Progress == MissionProgress.Inactive);

        public IEnumerable<Mission> GetActiveMissions()
            => missionDict.Values.Where
            (m => m.Progress == MissionProgress.Active);

        public IEnumerable<Mission> GetCompletedMissions()
            => missionDict.Values.Where
            (m => m.Progress == MissionProgress.Completed);

        public void CompleteMission(int missionId)
        {
            var mission = GetMission(missionId);
            mission.Complete();
        }

        public void AssignMissionToNPC(int npcType, int missionId)
        {
            if (!npcMissionsDict.TryGetValue(npcType, out var missionIds))
            {
                missionIds = [];
                npcMissionsDict[npcType] = missionIds;
            }

            if (!missionIds.Contains(missionId))
            {
                missionIds.Add(missionId);
            }
        }

        public void RemoveMissionFromNPC(int npcType, int missionId)
        {
            if (npcMissionsDict.TryGetValue(npcType, out var missionIds))
            {
                missionIds.Remove(missionId);
            }
        }


        private HashSet<int> notifiedMissions = [];

        public bool NPCHasAvailableMission(int npcType)
        {
            if (npcMissionsDict.TryGetValue(npcType, out var missionIds))
            {
                foreach (var missionId in missionIds)
                {
                    var mission = GetMission(missionId);
                    if (mission != null && mission.State == MissionState.Unlocked && mission.Progress == MissionProgress.Inactive)
                    {
                        if (!mission.MissionData.IsMainline)
                        {
                            string npcName = Lang.GetNPCNameValue(mission.MissionData.Commissioner);
                            Main.NewText($"{npcName} has a job opportunity!", Color.Yellow);
                            notifiedMissions.Add(missionId);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public override void ResetEffects()
        {
            base.ResetEffects();
            notifiedMissions.Clear();
        }

        public override void SaveData(TagCompound tag)
        {
            var activeMissionIds = missionDict.Values
                .Where(m => m.Progress != MissionProgress.Completed)
                .Select(m => m.ID)
                .ToList();

            tag["ActiveMissionIDs"] = activeMissionIds;
            tag["CompletedMissionIDs"] = GetCompletedMissions().Select(m => m.ID).ToList();
            tag["NotifiedMissions"] = notifiedMissions.ToList();

            var missionData = new List<TagCompound>();
            foreach (var mission in missionDict.Values.Where(m => m.Progress != MissionProgress.Completed))
            {
                var objectiveSetsData = new List<TagCompound>();
                foreach (var set in mission.MissionData.ObjectiveSets)
                {
                    objectiveSetsData.Add(new TagCompound
                    {
                        ["Objectives"] = set.Objectives.Select(o => o.Save()).ToList()
                    });
                }

                missionData.Add(new TagCompound
                {
                    ["ID"] = mission.ID,
                    ["Version"] = mission.MissionData.Version,
                    ["Progress"] = (int)mission.Progress,
                    ["State"] = (int)mission.State,
                    ["Unlocked"] = mission.Unlocked,
                    ["CurrentSetIndex"] = mission.CurrentSetIndex,
                    ["ObjectiveSets"] = objectiveSetsData,
                    ["NextMissionID"] = mission.MissionData.NextMissionID  // Add this line
                });
            }
            tag["MissionData"] = missionData;

            tag["NPCMissions"] = npcMissionsDict.Select(kvp => new TagCompound
            {
                ["NpcType"] = kvp.Key,
                ["Missions"] = kvp.Value
            }).ToList();
        }

        public override void LoadData(TagCompound tag)
        {
            missionDict.Clear();
            MissionHandlerManager.Instance.Reset();

            var activeMissionIds = tag.GetList<int>("ActiveMissionIDs");
            var missionData = tag.GetList<TagCompound>("MissionData");
            notifiedMissions = new HashSet<int>(tag.GetList<int>("NotifiedMissions"));

            // Load completed missions first to avoid resetting them
            var completedMissionIds = tag.GetList<int>("CompletedMissionIDs");
            foreach (var missionId in completedMissionIds)
            {
                var mission = GetMission(missionId);
                if (mission != null)
                {
                    mission.Progress = MissionProgress.Completed;
                    mission.State = MissionState.Completed;
                    missionDict[missionId] = mission;
                }
            }

            foreach (var missionTag in missionData)
            {
                var missionId = missionTag.GetInt("ID");

                // Skip if this mission is already marked as completed
                if (completedMissionIds.Contains(missionId))
                    continue;

                var savedVersion = missionTag.Get<int>("Version");
                var mission = GetMission(missionId);

                if (mission != null)
                {
                    mission.Progress = (MissionProgress)missionTag.GetInt("Progress");
                    mission.State = (MissionState)missionTag.GetInt("State");
                    mission.Unlocked = missionTag.GetBool("Unlocked");
                    mission.CurrentSetIndex = missionTag.GetInt("CurrentSetIndex");

                    // Handle version mismatch for active missions
                    if (savedVersion != mission.MissionData.Version && mission.Progress == MissionProgress.Active)
                    {
                        // Reset only the objective data, not the whole mission state
                        foreach (var set in mission.MissionData.ObjectiveSets)
                        {
                            set.Reset();
                        }
                        mission.CurrentSetIndex = 0;

                        // Check if there's a next mission to unlock
                        int nextMissionId = missionTag.GetInt("NextMissionID");
                        if (nextMissionId != -1)
                        {
                            var nextMission = GetMission(nextMissionId);
                            if (nextMission != null && nextMission.State == MissionState.Locked)
                            {
                                nextMission.State = MissionState.Unlocked;
                            }
                        }
                    }
                    else
                    {
                        // Load objectives normally if versions match
                        var savedObjectiveSets = missionTag.GetList<TagCompound>("ObjectiveSets");
                        for (int i = 0; i < mission.MissionData.ObjectiveSets.Count && i < savedObjectiveSets.Count; i++)
                        {
                            var savedObjectives = savedObjectiveSets[i].GetList<TagCompound>("Objectives");
                            var objectiveSet = mission.MissionData.ObjectiveSets[i];

                            if (objectiveSet.Objectives.Count == savedObjectives.Count)
                            {
                                for (int j = 0; j < objectiveSet.Objectives.Count; j++)
                                {
                                    objectiveSet.Objectives[j] = Objective.Load(savedObjectives[j]);
                                }
                            }
                        }
                    }

                    missionDict[missionId] = mission;

                    // Re-register handler if mission is active
                    if (mission.Progress == MissionProgress.Active)
                    {
                        MissionHandlerManager.Instance.RegisterMissionHandler(mission);
                    }
                }
            }

            npcMissionsDict = tag.GetList<TagCompound>("NPCMissions")
                .ToDictionary(
                    t => t.GetInt("NpcType"),
                    t => t.GetList<int>("Missions").ToList());
        }
    }
}