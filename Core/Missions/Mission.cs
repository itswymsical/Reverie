using Terraria.ModLoader.IO;
using System.Collections.Generic;
using Terraria;
using System.Linq;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Reverie.Common.Players;
using Terraria.ID;
using Terraria.Audio;
using Reverie.Common.UI.MissionUI;
using Terraria.UI;
using Reverie.Common.MissionAttributes;
using System;
using Terraria.ModLoader;
using System.IO;

namespace Reverie.Core.Missions
{
    public enum MissionProgress
    {
        Inactive,
        Active,
        Completed
    }
    public enum MissionAvailability
    {
        Locked,
        Unlocked,
        Completed
    }

    public class MissionData(int id, string name, string description, List<List<(string, int)>> objectiveSets, List<Item> rewards, bool isMainline, int npc, int nextMissionID = -1, int xpReward = 0)
    {
        public int ID { get; set; } = id;
        public string Name { get; private set; } = name;
        public string Description { get; private set; } = description;
        public List<ObjectiveSet> ObjectiveSets { get; protected set; } = objectiveSets.Select(set => new ObjectiveSet(set.Select(o => new Objective(o.Item1, o.Item2)).ToList())).ToList();
        public List<Item> Rewards { get; private set; } = rewards;
        public bool IsMainline { get; } = isMainline;
        public int Commissioner { get; set; } = npc;
        public int XPReward { get; private set; } = xpReward;
        public int NextMissionID { get; private set; } = nextMissionID;

    }

    public class MissionDataContainer
    {
        public int ID { get; set; }
        public MissionProgress Progress { get; set; }
        public MissionAvailability State { get; set; }
        public bool Unlocked { get; set; }
        public int CurrentSetIndex { get; set; }
        public List<ObjectiveSetState> ObjectiveSets { get; set; } = [];
        public int NextMissionID { get; set; }

        public TagCompound Serialize()
        {
            return new TagCompound
            {
                ["ID"] = ID,
                ["Progress"] = (int)Progress,
                ["State"] = (int)State,
                ["Unlocked"] = Unlocked,
                ["CurrentSetIndex"] = CurrentSetIndex,
                ["ObjectiveSets"] = ObjectiveSets.Select(set => set.Serialize()).ToList(),
                ["NextMissionID"] = NextMissionID
            };
        }

        public static MissionDataContainer Deserialize(TagCompound tag)
        {
            try
            {
                return new MissionDataContainer
                {
                    ID = tag.GetInt("ID"),
                    Progress = (MissionProgress)tag.GetInt("Progress"),
                    State = (MissionAvailability)tag.GetInt("State"),
                    Unlocked = tag.GetBool("Unlocked"),
                    CurrentSetIndex = tag.GetInt("CurrentSetIndex"),
                    ObjectiveSets = tag.GetList<TagCompound>("ObjectiveSets")
                        .Select(t => ObjectiveSetState.Deserialize(t))
                        .ToList(),
                    NextMissionID = tag.GetInt("NextMissionID")
                };
            }
            catch (Exception ex)
            {
                // Log error and return null to trigger fallback
                ModContent.GetInstance<Reverie>().Logger.Error($"Failed to deserialize mission container: {ex.Message}");
                return null;
            }
        }
    }

    public class Mission(MissionData missionData)
    {
        public int ID { get; set; } = missionData.ID;
        public MissionData MissionData { get; } = missionData;
        public MissionProgress Progress { get; set; }
        public MissionAvailability State { get; set; }
        public bool Unlocked { get; set; }
        public int CurrentSetIndex { get; set; }

        /// <summary>
        /// Allows you to perform actions such as spawning items or creating a dialogue sequence when an objective is complete.
        /// Requires the objective's string description.
        /// </summary>
        /// <param name="objective"></param>
        public virtual void OnObjectiveComplete(int objectiveIndex) { }
        /// <summary>
        /// Allows you to perform actions when a mission is completed. 
        /// Ensure you are using "Base.OnMissionComplete(bool rewards, Mission nextMission)" to prevent issues.
        /// </summary>
        /// <param name="rewards"></param>
        /// <param name="nextMission"></param>
        public virtual void OnMissionComplete(bool rewards = true)
        {
            if (rewards)
                GiveRewards();

            InGameNotificationsTracker.AddNotification(new MissionCompleteNotification(this));
        }
        /// <summary>
        /// Use this method to update/complete an objective. You will need to manually check 'ObjectiveSet'.
        /// </summary>
        /// <param name="objectiveIndex">The objective we are currently updating.</param>
        /// <param name="amount">How many times we update the objective. If an objective only has a value of 1, no need to set this parameter.</param>
        /// <returns></returns>
        /// 
        private bool isDirty; // Track if state has changed

        public bool UpdateProgress(int objectiveIndex, int amount = 1)
        {
            if (Progress != MissionProgress.Active)
                return false;

            var currentSet = MissionData.ObjectiveSets[CurrentSetIndex];
            if (objectiveIndex >= 0 && objectiveIndex < currentSet.Objectives.Count)
            {
                var obj = currentSet.Objectives[objectiveIndex];
                if (!obj.IsCompleted || amount < 0)
                {
                    bool wasCompleted = obj.UpdateProgress(amount);

                    // Notify MissionPlayer of the update
                    var player = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
                    player.NotifyMissionUpdate(this);

                    if (wasCompleted && amount > 0)
                    {
                        OnObjectiveComplete(objectiveIndex);
                        MissionHandlerManager.Instance.OnObjectiveComplete(this, objectiveIndex);
                        SoundEngine.PlaySound(SoundID.ResearchComplete with { Volume = 0.65f }, Main.LocalPlayer.position);
                    }

                    if (currentSet.IsCompleted)
                    {
                        if (CurrentSetIndex < MissionData.ObjectiveSets.Count - 1)
                        {
                            CurrentSetIndex++;
                            return true;
                        }
                        if (MissionData.ObjectiveSets.All(set => set.IsCompleted))
                        {
                            Complete();
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void Reset()
        {
            Progress = MissionProgress.Inactive;
            CurrentSetIndex = 0;
            foreach (var set in MissionData.ObjectiveSets)
            {
                set.Reset();
            }
        }

        public void Complete()
        {
            if (Progress == MissionProgress.Active && MissionData.ObjectiveSets.All(set => set.IsCompleted))
            {
                Progress = MissionProgress.Completed;
                State = MissionAvailability.Completed;
                isDirty = true;
                OnMissionComplete();

                MissionPlayer player = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
                player.StartNextMission(this);
            }
        }

        public bool IsDirty => isDirty;

        public void ClearDirtyFlag() => isDirty = false; 

        private void GiveRewards()
        {
            foreach (Item reward in MissionData.Rewards)
            {
                Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), reward.type, reward.stack);
            }
            if (MissionData.XPReward > 0)
            {
                ExperiencePlayer.AddExperience(Main.LocalPlayer, MissionData.XPReward);
                Main.NewText($"{Main.LocalPlayer.name} " +
                    $"Gained [c/73d5ff:{MissionData.XPReward} Exp.] " +
                    $"from completing [c/73d5ff:{MissionData.Name}]!", Color.White);
            }
        }

        public byte[] SerializeState()
        {
            using (MemoryStream ms = new())
            {
                using (BinaryWriter writer = new(ms))
                {
                    writer.Write(ID);
                    writer.Write((int)Progress);
                    writer.Write((int)State);
                    writer.Write(Unlocked);
                    writer.Write(CurrentSetIndex);

                    // Write objective sets
                    writer.Write(MissionData.ObjectiveSets.Count);
                    foreach (var set in MissionData.ObjectiveSets)
                    {
                        set.WriteData(writer);
                    }
                }
                return ms.ToArray();
            }
        }

        public void DeserializeState(byte[] data)
        {
            using (MemoryStream ms = new(data))
            {
                using (BinaryReader reader = new(ms))
                {
                    ID = reader.ReadInt32();
                    Progress = (MissionProgress)reader.ReadInt32();
                    State = (MissionAvailability)reader.ReadInt32();
                    Unlocked = reader.ReadBoolean();
                    CurrentSetIndex = reader.ReadInt32();

                    // Read objective sets
                    int setCount = reader.ReadInt32();
                    for (int i = 0; i < setCount; i++)
                    {
                        MissionData.ObjectiveSets[i].ReadData(reader);
                    }
                }
            }
        }
    }
}