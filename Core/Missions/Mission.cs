using Reverie.Common.Players;
using Reverie.Common.UI.Missions;
using Reverie.Core.Missions.MissionAttributes;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using Terraria.UI;

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

    /// <summary>
    /// Represents a mission within the Reverie mod, managing both mission definition and state.
    /// A mission consists of a series of objectives organized into sets that the player must complete,
    /// along with associated rewards, progression logic, and completion tracking.
    /// </summary>
    /// <remarks>
    /// Key features:
    /// - Manages mission progress and availability states
    /// - Handles objective completion and progression
    /// - Tracks rewards and experience points
    /// - Supports mainline (story) and side missions
    /// - Handles mission chaining through NextMissionID
    /// - Provides serialization support for save/load operations
    /// 
    /// Mission progression occurs through objective sets, where each set must be completed
    /// in sequence. When all objective sets are completed, the mission is marked as complete
    /// and rewards are distributed to the player.
    /// </remarks>
    public class Mission(int id, string name, string description, List<List<(string, int)>> objectiveSets,
        List<Item> rewards, bool isMainline, int npc, int nextMissionID = -1, int xpReward = 0)
    {
        public int ID { get; set; } = id;
        public string Name { get; private set; } = name;
        public string Description { get; private set; } = description;
        public List<ObjectiveSet> ObjectiveSets { get; protected set; } = objectiveSets.Select(set =>
                new ObjectiveSet(set.Select(o =>
                    new Objective(o.Item1, o.Item2)).ToList())).ToList();
        public List<Item> Rewards { get; private set; } = rewards;
        public bool IsMainline { get; } = isMainline;
        public int Commissioner { get; set; } = npc;
        public int XPReward { get; private set; } = xpReward;
        public int NextMissionID { get; private set; } = nextMissionID;

        // Original Mission properties
        public MissionProgress Progress { get; set; } = MissionProgress.Inactive;
        public MissionAvailability State { get; set; } = MissionAvailability.Locked;
        public bool Unlocked { get; set; } = false;
        public int CurrentSetIndex { get; set; } = 0;
        private bool isDirty = false;

        public virtual void OnObjectiveComplete(int objectiveIndex) { }

        public virtual void OnMissionComplete(bool rewards = true)
        {
            if (rewards)
                GiveRewards();

            InGameNotificationsTracker.AddNotification(new MissionCompleteNotification(this));
        }

        public bool UpdateProgress(int objectiveIndex, int amount = 1)
        {
            if (Progress != MissionProgress.Active)
                return false;

            var currentSet = ObjectiveSets[CurrentSetIndex];
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
                        SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}ObjectiveComplete") with{ Volume = 0.75f }, Main.LocalPlayer.position);
                    }

                    if (currentSet.IsCompleted)
                    {
                        if (CurrentSetIndex < ObjectiveSets.Count - 1)
                        {
                            CurrentSetIndex++;
                            return true;
                        }
                        if (ObjectiveSets.All(set => set.IsCompleted))
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
            foreach (var set in ObjectiveSets)
            {
                set.Reset();
            }
        }

        public void Complete()
        {
            if (Progress == MissionProgress.Active && ObjectiveSets.All(set => set.IsCompleted))
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
            foreach (Item reward in Rewards)
            {
                Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), reward.type, reward.stack);
            }
            if (XPReward > 0)
            {
                ExperiencePlayer.AddExperience(Main.LocalPlayer, XPReward);
                Main.NewText($"{Main.LocalPlayer.name} " +
                    $"Gained [c/73d5ff:{XPReward} Exp.] " +
                    $"from completing [c/73d5ff:{Name}]!", Color.White);
            }
        }

        public byte[] SerializeState()
        {
            using MemoryStream ms = new();
            using (BinaryWriter writer = new(ms))
            {
                writer.Write(ID);
                writer.Write((int)Progress);
                writer.Write((int)State);
                writer.Write(Unlocked);
                writer.Write(CurrentSetIndex);

                // Write objective sets
                writer.Write(ObjectiveSets.Count);
                foreach (var set in ObjectiveSets)
                {
                    set.WriteData(writer);
                }
            }
            return ms.ToArray();
        }

        public void DeserializeState(byte[] data)
        {
            using MemoryStream ms = new(data);
            using BinaryReader reader = new(ms);
            ID = reader.ReadInt32();
            Progress = (MissionProgress)reader.ReadInt32();
            State = (MissionAvailability)reader.ReadInt32();
            Unlocked = reader.ReadBoolean();
            CurrentSetIndex = reader.ReadInt32();

            // Read objective sets
            int setCount = reader.ReadInt32();
            for (int i = 0; i < setCount; i++)
            {
                ObjectiveSets[i].ReadData(reader);
            }
        }
    }

    /// <summary>
    /// A serializable container class that stores mission state data for save/load operations.
    /// This container maintains the progress, completion status, and objective states of a mission
    /// without storing the full mission definition. Used as an intermediary between Mission objects
    /// and their TagCompound representation in save files.
    /// </summary>
    /// <remarks>
    /// The container stores:
    /// - Basic mission identification and state
    /// - Current objective progress and completion status
    /// - Mission availability and unlock status
    /// - Links to next missions in a sequence
    /// </remarks>
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
}