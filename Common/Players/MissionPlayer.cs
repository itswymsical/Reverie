using Microsoft.Xna.Framework;
using Reverie.Common.MissionAttributes;
using Reverie.Common.Systems;
using Reverie.Core.Graphics;
using Reverie.Core.Missions;
using Reverie.Cutscenes;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Reverie.Common.Players
{
    public partial class MissionPlayer : ModPlayer
    {
        public readonly Dictionary<int, MissionData> missionDataDict = [];
        public readonly Dictionary<int, Mission> missionDict = [];
        public readonly Dictionary<int, List<int>> npcMissionsDict = [];
        private readonly HashSet<int> notifiedMissions = [];
        private readonly MissionDataFactory missionDataFactory = new();
        
        public static bool NPCHasAvailableMission(MissionPlayer missionPlayer, int npcType)
        {
            if (missionPlayer.npcMissionsDict.TryGetValue(npcType, out var missionIds))
            {
                foreach (var missionId in missionIds)
                {
                    var mission = missionPlayer.GetMission(missionId);
                    if (mission != null && mission.State == MissionState.Unlocked && mission.Progress != MissionProgress.Completed)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override void SaveData(TagCompound tag)
        {
            try
            {
                var missionStates = missionDict.Values
                    .Where(m => m.Progress != MissionProgress.Completed)
                    .Select(m => m.ToState().Serialize())
                    .ToList();

                tag["MissionStates"] = missionStates;
                tag["CompletedMissionIDs"] = GetCompletedMissions().Select(m => m.ID).ToList();
                tag["NotifiedMissions"] = notifiedMissions.ToList();

                // Serialize NPC missions
                tag["NPCMissions"] = npcMissionsDict.Select(kvp => new TagCompound
                {
                    ["NpcType"] = kvp.Key,
                    ["Missions"] = kvp.Value
                }).ToList();
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Failed to save mission data: {ex}");
                // Continue without throwing to prevent save corruption
            }
        }

        public override void LoadData(TagCompound tag)
        {
            try
            {
                // Clear existing state
                missionDict.Clear();
                MissionHandlerManager.Instance.Reset();

                // Load completed missions first
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

                // Load active missions
                var missionStates = tag.GetList<TagCompound>("MissionStates");
                foreach (var stateTag in missionStates)
                {
                    var state = MissionDataContainer.Deserialize(stateTag);
                    if (state == null) continue;

                    var mission = GetMission(state.ID);
                    if (mission == null) continue;

                    // Skip if already marked as completed
                    if (completedMissionIds.Contains(state.ID))
                        continue;

                    mission.LoadState(state);
                    missionDict[state.ID] = mission;

                    // Re-register handler if mission is active
                    if (mission.Progress == MissionProgress.Active)
                    {
                        MissionHandlerManager.Instance.RegisterMissionHandler(mission);
                    }
                }

                // Load notification state
                notifiedMissions.Clear();
                notifiedMissions.UnionWith(tag.GetList<int>("NotifiedMissions"));

                // Load NPC missions
                npcMissionsDict.Clear();
                foreach (var npcTag in tag.GetList<TagCompound>("NPCMissions"))
                {
                    var npcType = npcTag.GetInt("NpcType");
                    var missions = npcTag.GetList<int>("Missions");
                    npcMissionsDict[npcType] = missions.ToList();
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load mission data: {ex}");
                // Reset to clean state on load failure
                ResetToCleanState();
            }
        }

        private void ResetToCleanState()
        {
            missionDict.Clear();
            missionDataDict.Clear();
            npcMissionsDict.Clear();
            notifiedMissions.Clear();
            MissionHandlerManager.Instance.Reset();
        }

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

        public override void OnEnterWorld()
        {
            MissionHandlerManager.Instance.Reset(); // Reset handlers on world enter

            Mission Reawakening = GetMission(MissionID.Reawakening);
            ReveriePlayer player = Main.LocalPlayer.GetModPlayer<ReveriePlayer>();

            if (Reawakening != null && Reawakening.State != MissionState.Completed)
            {
                if (Reawakening.Progress != MissionProgress.Active)
                {
                    CutsceneLoader.PlayCutscene(new IntroCutscene());
                    UnlockMission(MissionID.Reawakening);
                    StartMission(MissionID.Reawakening);

                    Reawakening.Progress = MissionProgress.Active;
                }

                if (Reawakening.CurrentSetIndex == 1)
                {
                    if (!player.pathWarrior && !player.pathMarksman && !player.pathMage && !player.pathConjurer)
                    {
                        ReverieUISystem.Instance.ClassInterface.SetState(ReverieUISystem.Instance.classUI);
                    }
                }
            }
        }

        public override bool OnPickup(Item item)
        {
            MissionHandlerManager.Instance.OnItemPickup(item);
            return base.OnPickup(item);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            MissionHandlerManager.Instance.OnNPCHit(target, hit.Damage);
        }

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
    }
}