using Microsoft.Xna.Framework;
using Reverie.Common.MissionAttributes;
using Reverie.Core.Dialogue;
using Reverie.Core.Graphics;
using Reverie.Core.Missions;
using Reverie.Cutscenes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Reverie.Common.Players
{
    public partial class MissionPlayer : ModPlayer
    {
        // Dictionary to store mission data templates

        // Dictionary to store active mission instances
        public readonly Dictionary<int, Mission> missionDict = [];

        // Dictionary to map NPCs to their available missions
        public readonly Dictionary<int, List<int>> npcMissionsDict = [];

        // Track missions that have shown notifications
        private readonly HashSet<int> notifiedMissions = [];

        // Factory instance for creating mission data
        private readonly MissionFactory missionFactory = new();
        private readonly HashSet<int> dirtyMissions = [];
        private BiomeState previousBiomeState;
        public override void OnEnterWorld()
        {
            Mission Reawakening = GetMission(MissionID.Reawakening);
            ReveriePlayer player = Main.LocalPlayer.GetModPlayer<ReveriePlayer>();

            if (Reawakening != null &&
                Reawakening.State != MissionAvailability.Completed &&
                Reawakening.Progress != MissionProgress.Active)
            {
                CutsceneLoader.PlayCutscene(new IntroCutscene());
                UnlockMission(MissionID.Reawakening);
                StartMission(MissionID.Reawakening);
            }
        }

        public void NotifyMissionUpdate(Mission mission)
        {
            if (mission == null) return;

            try
            {
                // Update the mission in our dictionary
                missionDict[mission.ID] = mission;

                // Mark mission as needing UI refresh
                dirtyMissions.Add(mission.ID);

                // Convert current state to container and store
                var container = mission.ToState();

                ModContent.GetInstance<Reverie>().Logger.Debug($"Mission {mission.Name} progress updated: Set {mission.CurrentSetIndex}");
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Failed to notify mission update: {ex}");
            }
        }

        private void SyncMissionAvailability(Mission mission)
        {
            if (mission == null) return;

            // Create or update mission state in storage
            _ = mission.ToState();
            if (missionDict.ContainsKey(mission.ID))
            {
                missionDict[mission.ID] = mission;
            }
        }

        // Update the UpdateProgress method to ensure state is synced
        public bool UpdateMissionProgress(int missionId, int objectiveIndex, int amount = 1)
        {
            if (missionDict.TryGetValue(missionId, out var mission))
            {
                bool updated = mission.UpdateProgress(objectiveIndex, amount);
                if (updated)
                {
                    // Sync the updated state
                    SyncMissionAvailability(mission);
                }
                return updated;
            }
            return false;
        }

        public override void SaveData(TagCompound tag)
        {
            try
            {
                // Ensure all active missions are synced before saving
                foreach (var mission in missionDict.Values)
                {
                    SyncMissionAvailability(mission);
                }

                var activeMissionData = new List<TagCompound>();
                foreach (var mission in missionDict.Values.Where(m => m.Progress != MissionProgress.Completed))
                {
                    var state = mission.ToState();
                    var MissionAvailability = new TagCompound
                    {
                        ["ID"] = mission.ID,
                        ["State"] = new TagCompound
                        {
                            ["Progress"] = (int)mission.Progress,
                            ["State"] = (int)mission.State,
                            ["Unlocked"] = mission.Unlocked,
                            ["CurrentSetIndex"] = mission.CurrentSetIndex,
                            ["ObjectiveSets"] = SerializeObjectiveSets(mission.ObjectiveSets)
                        }
                    };
                    activeMissionData.Add(MissionAvailability);
                }
                tag["ActiveMissions"] = activeMissionData;

                foreach (var mission in missionDict.Values.Where(m => m.Progress != MissionProgress.Completed))
                {
                    var MissionAvailability = new TagCompound
                    {
                        ["ID"] = mission.ID,
                        ["State"] = new TagCompound
                        {
                            ["Progress"] = (int)mission.Progress,
                            ["State"] = (int)mission.State,
                            ["Unlocked"] = mission.Unlocked,
                            ["CurrentSetIndex"] = mission.CurrentSetIndex,
                            ["ObjectiveSets"] = SerializeObjectiveSets(mission.ObjectiveSets)
                        }
                    };
                    activeMissionData.Add(MissionAvailability);
                }
                tag["ActiveMissions"] = activeMissionData;

                // Save completed mission IDs
                tag["CompletedMissionIDs"] = GetCompletedMissions().Select(m => m.ID).ToList();

                // Save notification state
                tag["NotifiedMissions"] = notifiedMissions.ToList();

                // Save NPC mission assignments
                var npcMissionData = npcMissionsDict.Select(kvp => new TagCompound
                {
                    ["NpcType"] = kvp.Key,
                    ["MissionIDs"] = kvp.Value
                }).ToList();
                tag["NPCMissions"] = npcMissionData;
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Failed to save mission data: {ex}");
            }
        }

        private List<TagCompound> SerializeObjectiveSets(List<ObjectiveSet> objectiveSets)
        {
            var serializedSets = new List<TagCompound>();
            foreach (var set in objectiveSets)
            {
                using var ms = new MemoryStream();
                using var writer = new BinaryWriter(ms);
                set.WriteData(writer);
                serializedSets.Add(new TagCompound
                {
                    ["SetData"] = ms.ToArray()
                });
            }
            return serializedSets;
        }

        public override void LoadData(TagCompound tag)
        {
            try
            {
                // Clear existing state
                ResetToCleanState();

                // Load completed missions first
                var completedMissionIds = tag.GetList<int>("CompletedMissionIDs");
                foreach (var missionId in completedMissionIds)
                {
                    LoadCompletedMission(missionId);
                }

                // Load active missions
                var activeMissionData = tag.GetList<TagCompound>("ActiveMissions").ToList();
                foreach (var missionTag in activeMissionData)
                {
                    LoadActiveMission(missionTag);
                }

                // Load notification state
                notifiedMissions.UnionWith([.. tag.GetList<int>("NotifiedMissions")]);

                // Load NPC mission assignments
                var npcMissionData = tag.GetList<TagCompound>("NPCMissions").ToList();
                foreach (var npcTag in npcMissionData)
                {
                    int npcType = npcTag.GetInt("NpcType");
                    var missionIds = npcTag.GetList<int>("MissionIDs").ToList();
                    npcMissionsDict[npcType] = missionIds;
                }

                // Re-register handlers for active missions
                foreach (var mission in GetActiveMissions())
                {
                    MissionHandlerManager.Instance.RegisterMissionHandler(mission);
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load mission data: {ex}");
                ResetToCleanState();
            }
        }

        private void LoadCompletedMission(int missionId)
        {
            var mission = GetMission(missionId);
            if (mission != null)
            {
                mission.Progress = MissionProgress.Completed;
                mission.State = MissionAvailability.Completed;
                missionDict[missionId] = mission;
            }
        }

        private void LoadActiveMission(TagCompound missionTag)
        {
            try
            {
                int missionId = missionTag.GetInt("ID");
                var stateTag = missionTag.GetCompound("State");

                var mission = GetMission(missionId);
                if (mission == null) return;

                // Load mission state
                mission.Progress = (MissionProgress)stateTag.GetInt("Progress");
                mission.State = (MissionAvailability)stateTag.GetInt("State");
                mission.Unlocked = stateTag.GetBool("Unlocked");
                mission.CurrentSetIndex = stateTag.GetInt("CurrentSetIndex");

                // Load objective sets
                var serializedSets = stateTag.GetList<TagCompound>("ObjectiveSets");
                LoadObjectiveSets(mission, serializedSets);

                missionDict[missionId] = mission;
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<Reverie>().Logger.Error($"Failed to load active mission: {ex}");
            }
        }

        private void LoadObjectiveSets(Mission mission, IList<TagCompound> serializedSets)
        {
            for (int i = 0; i < Math.Min(serializedSets.Count, mission.ObjectiveSets.Count); i++)
            {
                var setData = serializedSets[i].GetByteArray("SetData");
                using var ms = new MemoryStream(setData);
                using var reader = new BinaryReader(ms);
                mission.ObjectiveSets[i].ReadData(reader);
            }
        }

        private void ResetToCleanState()
        {
            missionDict.Clear();
            npcMissionsDict.Clear();
            notifiedMissions.Clear();
            MissionHandlerManager.Instance.Reset();
        }

        public Mission GetMission(int missionId)
        {
            if (!missionDict.TryGetValue(missionId, out var mission))
            {
                mission = missionFactory.GetMissionData(missionId);
                if (mission != null)
                {
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
                mission.State = MissionAvailability.Unlocked;
            }
        }

        public void DebugMissionAvailabilitys()
        {
            foreach (var mission in missionDict.Values)
            {
                Main.NewText($"Mission: {mission.Name}");
                Main.NewText($"  State: {mission.State}");
                Main.NewText($"  Progress: {mission.Progress}");
                Main.NewText($"  NextMissionID: {mission.NextMissionID}");
            }
        }

        public void StartNextMission(Mission completedMission)
        {
            int nextMissionID = completedMission.NextMissionID;
            if (nextMissionID != -1)
            {
                Mission nextMission = GetMission(nextMissionID);
                if (nextMission != null)
                {
                    if (nextMission.Progress != MissionProgress.Completed &&
                        nextMission.Progress != MissionProgress.Active)
                    {
                        nextMission.State = MissionAvailability.Unlocked;
                        nextMission.Progress = MissionProgress.Active;
                        StartMission(nextMissionID);
                        Main.NewText($"New mission available: {nextMission.Name}", Color.Yellow);
                    }
                }
            }
        }

        public void CompleteMission(int missionId)
        {
            var mission = GetMission(missionId);
            if (mission != null)
            {
                mission.Complete();
                SyncMissionAvailability(mission);
            }
        }

        // Add sync to other state-changing methods
        public void StartMission(int missionId)
        {
            var mission = GetMission(missionId);
            if (mission != null)
            {
                mission.Progress = MissionProgress.Active;
                MissionHandlerManager.Instance.RegisterMissionHandler(mission);
                SyncMissionAvailability(mission);
            }
        }

        public void UnlockMission(int missionId)
        {
            var mission = GetMission(missionId);
            if (mission != null)
            {
                mission.State = MissionAvailability.Unlocked;
                mission.Progress = MissionProgress.Inactive;
                mission.Unlocked = true;
                SyncMissionAvailability(mission);
            }
        }

        public IEnumerable<Mission> GetAvailableMissions()
            => missionDict.Values.Where
            (m => m.State == MissionAvailability.Unlocked && m.Progress == MissionProgress.Inactive);

        public IEnumerable<Mission> GetActiveMissions()
        {
            foreach (var mission in missionDict.Values.Where(m => m.Progress == MissionProgress.Active))
            {
                // Ensure we're working with the most up-to-date mission state
                if (dirtyMissions.Contains(mission.ID))
                {
                    mission.LoadState(mission.ToState());
                }
                yield return mission;
            }
        }

        public IEnumerable<Mission> GetCompletedMissions()
            => missionDict.Values.Where
            (m => m.Progress == MissionProgress.Completed);

        public static bool NPCHasAvailableMission(MissionPlayer missionPlayer, int npcType)
        {
            if (missionPlayer.npcMissionsDict.TryGetValue(npcType, out var missionIds))
            {
                foreach (var missionId in missionIds)
                {
                    var mission = missionPlayer.GetMission(missionId);
                    if (mission != null && mission.State == MissionAvailability.Unlocked && mission.Progress != MissionProgress.Completed)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool NPCHasAvailableMission(int npcType)
        {
            if (npcMissionsDict.TryGetValue(npcType, out var missionIds))
            {
                foreach (var missionId in missionIds)
                {
                    var mission = GetMission(missionId);
                    if (mission != null && mission.State == MissionAvailability.Unlocked && mission.Progress == MissionProgress.Inactive)
                    {
                        if (!mission.IsMainline)
                        {
                            string npcName = Lang.GetNPCNameValue(mission.Commissioner);
                            Main.NewText($"{npcName} has a job opportunity!", Color.Yellow);
                            notifiedMissions.Add(missionId);
                        }
                        return true;
                    }
                }
            }
            return false;
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

        private int timer = 0;
        private bool hasTriggered = false;
        public override void PostUpdate()
        {
            if (dirtyMissions.Count > 0)
            {
                dirtyMissions.Clear();
            }
            Mission Reawakening = GetMission(MissionID.Reawakening);

            if (Reawakening != null && Reawakening.Progress == MissionProgress.Active && Reawakening.CurrentSetIndex == 1 && !DialogueManager.Instance.IsAnyActive() && !hasTriggered)
            {
                timer++;

                if (timer >= 300)
                {
                    CutsceneLoader.PlayCutscene(new EyeAppearanceCutscene());
                    hasTriggered = true;
                }
            }
        }
        

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            MissionHandlerManager.Instance.OnNPCHit(target, damageDone);
        }

        public override void PreUpdate()
        {
            base.PreUpdate();

            BiomeState currentBiomeState = BiomeState.FromPlayer(Player);

            // Check for any biome changes
            if (!currentBiomeState.Equals(previousBiomeState))
            {
                // Only call if we're entering a biome (not leaving)
                if (currentBiomeState.ZoneBeach && !previousBiomeState.ZoneBeach ||
                    currentBiomeState.ZoneDungeon && !previousBiomeState.ZoneDungeon ||
                    currentBiomeState.ZoneCorrupt && !previousBiomeState.ZoneCorrupt ||
                    currentBiomeState.ZoneCrimson && !previousBiomeState.ZoneCrimson ||
                    currentBiomeState.ZoneDesert && !previousBiomeState.ZoneDesert ||
                    currentBiomeState.ZoneGlowshroom && !previousBiomeState.ZoneGlowshroom ||
                    currentBiomeState.ZoneHallow && !previousBiomeState.ZoneHallow ||
                    currentBiomeState.ZoneJungle && !previousBiomeState.ZoneJungle ||
                    currentBiomeState.ZoneMeteor && !previousBiomeState.ZoneMeteor ||
                    currentBiomeState.ZoneSnow && !previousBiomeState.ZoneSnow ||
                    currentBiomeState.ZoneUndergroundDesert && !previousBiomeState.ZoneUndergroundDesert ||
                    currentBiomeState.ZoneRain && !previousBiomeState.ZoneRain ||
                    currentBiomeState.ZoneSandstorm && !previousBiomeState.ZoneSandstorm ||
                    currentBiomeState.ZoneOldOneArmy && !previousBiomeState.ZoneOldOneArmy ||
                    currentBiomeState.ZonePeaceCandle && !previousBiomeState.ZonePeaceCandle ||
                    currentBiomeState.ZoneWaterCandle && !previousBiomeState.ZoneWaterCandle)
                {
                    MissionHandlerManager.Instance.OnBiomeEnter(Player, currentBiomeState);
                }
            }

            previousBiomeState = currentBiomeState;
        }

        public override void ResetEffects()
        {
            base.ResetEffects();
            notifiedMissions.Clear();
        }
    }
}