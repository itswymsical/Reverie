using Reverie.Common.Players;
using Reverie.Common.UI.Missions;
using Reverie.Utilities;

using System.Collections.Generic;
using System.Linq;

using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Reverie.Core.Missions.Core;

public enum MissionProgress
{
    Inactive,
    Ongoing,
    Completed
}

public enum MissionStatus
{
    Locked,
    Unlocked,
    Completed
}

/// <summary>
/// Represents a mission with objectives, rewards, and progression tracking.
/// Objectives are grouped into sets and must be completed in order.
/// </summary>
/// <remarks>
/// When all objective sets are complete, the mission finishes and rewards are given.
/// </remarks>
public abstract class Mission
{

    #region Fields
    // Remove the fixed local player reference - missions now work for any player
    protected bool isDirty = false;
    protected bool eventsRegistered = false;
    protected HashSet<Point> interactedTiles = new HashSet<Point>();
    protected HashSet<int> interactedItems = new HashSet<int>();
    #endregion

    #region Properties
    public int ID { get; set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public List<ObjectiveSet> Objective { get; protected set; }
    public List<Item> Rewards { get; private set; }
    public bool IsMainline { get; }
    public int ProviderNPC { get; set; }
    public int Experience { get; private set; }
    public int NextMissionID { get; private set; }
    public MissionProgress Progress { get; set; } = MissionProgress.Inactive;
    public MissionStatus Status { get; set; } = MissionStatus.Locked;
    public bool Unlocked { get; set; } = false;
    public int CurrentIndex { get; set; } = 0;
    public bool IsDirty => isDirty;
    #endregion

    #region Initialization
    protected Mission(int id, string name, string description, List<List<(string, int)>> objectiveList,
        List<Item> rewards, bool isMainline, int providerNPC, int nextMissionID = -1, int xpReward = 0)
    {
        ID = id;
        Name = name;
        Description = description;
        Objective = objectiveList.Select(set =>
            new ObjectiveSet(set.Select(o =>
                new Objective(o.Item1, o.Item2)).ToList())).ToList();
        Rewards = rewards;
        IsMainline = isMainline;
        ProviderNPC = providerNPC;
        Experience = xpReward;
        NextMissionID = nextMissionID;
    }
    #endregion

    #region Event Registration

    /// <summary>
    /// Registers event handlers specific to this mission
    /// </summary>
    protected virtual void RegisterEventHandlers()
    {
        if (eventsRegistered) return;
        eventsRegistered = true;

        ModContent.GetInstance<Reverie>().Logger.Info($"Mission {Name} registered event handlers");
    }

    /// <summary>
    /// Unregisters event handlers specific to this mission
    /// </summary>
    protected virtual void UnregisterEventHandlers()
    {
        if (!eventsRegistered) return;
        eventsRegistered = false;

        ModContent.GetInstance<Reverie>().Logger.Info($"Mission {Name} unregistered event handlers");
    }

    #endregion

    #region Virtual Event Handlers
    /// <summary>
    /// remember to call "base.OnMissionStart()" in derived classes; calls "RegisterEventHandlers()"
    /// </summary>
    public virtual void OnMissionStart() => RegisterEventHandlers();

    /// <summary>
    /// Called when an objective set within the current mission is completed.
    /// </summary>
    protected virtual void OnObjectiveIndexComplete(int setIndex, ObjectiveSet completedSet) { }

    /// <summary>
    /// Called when an objective within the current set is completed.
    /// </summary>
    protected virtual void OnObjectiveComplete(int objectiveIndexWithinCurrentSet) { }

    public void HandleObjectiveCompletion(int objectiveIndex)
    {
        try
        {
            var currentSet = Objective[CurrentIndex];
            var objective = currentSet.Objectives[objectiveIndex];

            // First handle the specific objective completion
            OnObjectiveComplete(objectiveIndex);

            // Then check if the entire set is completed
            if (currentSet.IsCompleted)
            {
                OnObjectiveIndexComplete(CurrentIndex, currentSet);
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in HandleObjectiveCompletion for mission {Name}: {ex.Message}");
        }
    }

    public void ClearInteractedTiles() => interactedTiles.Clear();
    public void ClearInteractedItems() => interactedItems.Clear();

    public bool WasTileInteracted(int i, int j) => interactedTiles.Contains(new Point(i, j));
    public bool WasItemInteracted(int type) => interactedItems.Contains(type);
    public void MarkItemInteracted(int type) => interactedItems.Add(type);
    #endregion

    #region Core Mission Logic
    /// <summary>
    /// Updates the progress of an objective, IN the current set.
    /// Now accepts a player parameter to specify which player triggered the progress.
    /// </summary>
    /// <param name="objective">the objective index</param>
    /// <param name="amount">progress amount</param>
    /// <param name="triggeringPlayer">the player who triggered this progress update</param>
    /// <returns></returns>
    public bool UpdateProgress(int objective, int amount = 1, Player triggeringPlayer = null)
    {
        if (Progress != MissionProgress.Ongoing)
            return false;

        // Use the triggering player or fall back to any active player for rewards/notifications
        var targetPlayer = triggeringPlayer ?? GetAnyActivePlayerWithThisMission();
        if (targetPlayer == null) return false;

        var currentSet = Objective[CurrentIndex];
        if (objective >= 0 && objective < currentSet.Objectives.Count)
        {
            var obj = currentSet.Objectives[objective];
            if (!obj.IsCompleted || amount < 0)
            {
                var wasCompleted = obj.UpdateProgress(amount);

                var missionPlayer = targetPlayer.GetModPlayer<MissionPlayer>();
                missionPlayer.NotifyMissionUpdate(this);

                if (wasCompleted && amount > 0)
                {
                    HandleObjectiveCompletion(objective);
                    SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}ObjectiveComplete") with { Volume = 0.75f }, targetPlayer.position);
                }

                if (currentSet.IsCompleted)
                {
                    if (CurrentIndex < Objective.Count - 1)
                    {
                        CurrentIndex++;
                        return true;
                    }
                    if (Objective.All(set => set.IsCompleted))
                    {
                        Complete(targetPlayer);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Finds any active player who has this mission active.
    /// Used when we need to give rewards or show notifications but don't have a specific triggering player.
    /// </summary>
    private Player GetAnyActivePlayerWithThisMission()
    {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            var player = Main.player[i];
            if (player?.active == true)
            {
                var missionPlayer = player.GetModPlayer<MissionPlayer>();
                if (missionPlayer.missionDict.ContainsKey(ID) &&
                    missionPlayer.missionDict[ID].Progress == MissionProgress.Ongoing)
                {
                    return player;
                }
            }
        }
        return null;
    }

    public void Reset()
    {
        UnregisterEventHandlers();
        Progress = MissionProgress.Inactive;
        CurrentIndex = 0;
        interactedTiles.Clear();
        interactedItems.Clear();
        foreach (var set in Objective)
        {
            set.Reset();
        }
    }

    public void Complete(Player rewardPlayer = null)
    {
        if (Progress == MissionProgress.Ongoing && Objective.All(set => set.IsCompleted))
        {
            UnregisterEventHandlers();
            Progress = MissionProgress.Completed;
            Status = MissionStatus.Completed;
            isDirty = true;

            OnMissionComplete(rewardPlayer);
        }
        interactedTiles.Clear();
        interactedItems.Clear();
    }

    /// <summary>
    /// Use to set new missions or trigger events. By default, gives rewards and plays the mission complete notification.
    /// Now accepts a player parameter to specify who should receive rewards.
    /// </summary>
    /// <param name="rewardPlayer">The player who should receive rewards and notifications</param>
    /// <param name="giveRewards">Whether to give rewards</param>
    public virtual void OnMissionComplete(Player rewardPlayer = null, bool giveRewards = true)
    {
        // If no specific player is provided, find any player with this mission
        var targetPlayer = rewardPlayer ?? GetAnyActivePlayerWithThisMission();

        if (targetPlayer != null)
        {
            if (giveRewards)
                GiveRewards(targetPlayer);

            // Only show notification to the target player (or all players in multiplayer)
            if (targetPlayer.whoAmI == Main.myPlayer || Main.netMode != NetmodeID.MultiplayerClient)
            {
                InGameNotificationsTracker.AddNotification(new MissionCompleteNotification(this));
            }
        }
    }

    /// <summary>
    /// Update things while the mission is active.
    /// By default, registers event handlers if not already done.
    /// </summary>
    public virtual void Update()
    {
        if (Progress != MissionProgress.Ongoing && Status != MissionStatus.Unlocked) return;

        if (Progress == MissionProgress.Ongoing && !eventsRegistered)
        {
            RegisterEventHandlers();
        }
    }
    #endregion

    #region Helper Methods
    private void GiveRewards(Player targetPlayer)
    {
        foreach (var reward in Rewards)
        {
            targetPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), reward.type, reward.stack);
        }
        if (Experience > 0)
        {
            ExperiencePlayer.AddExperience(targetPlayer, Experience);
            Main.NewText($"{targetPlayer.name} " +
                $"Gained [c/73d5ff:{Experience} Exp.] " +
                $"from completing [c/73d5ff:{Name}]!", Color.White);
        }
    }

    public void ClearDirtyFlag() => isDirty = false;

    /// <summary>
    /// makes an objective visible on the indicator.
    /// </summary>
    public void SetObjectiveVisibility(int setIndex, int objectiveIndex, bool isVisible)
    {
        if (setIndex >= 0 && setIndex < Objective.Count &&
            objectiveIndex >= 0 && objectiveIndex < Objective[setIndex].Objectives.Count)
        {
            Objective[setIndex].Objectives[objectiveIndex].IsVisible = isVisible;
        }
    }

    /// <summary>
    /// makes an objective visible on the indicator, if a condition is met.
    /// </summary>
    public void SetObjectiveVisibilityCondition(int setIndex, int objectiveIndex,
                                               Objective.VisibilityCondition condition)
    {
        if (setIndex >= 0 && setIndex < Objective.Count &&
            objectiveIndex >= 0 && objectiveIndex < Objective[setIndex].Objectives.Count)
        {
            Objective[setIndex].Objectives[objectiveIndex].VisibilityCheck = condition;
        }
    }

    /// <summary>
    /// Shows an objective on the indicator after a specific objective is completed.
    /// </summary>
    public void ShowObjectiveAfterCompletion(int setIndex, int objectiveToShow, int dependencyObjective)
    {
        SetObjectiveVisibilityCondition(setIndex, objectiveToShow, (mission) => {
            var set = mission.Objective[setIndex];
            return dependencyObjective >= 0 &&
                   dependencyObjective < set.Objectives.Count &&
                   set.Objectives[dependencyObjective].IsCompleted;
        });
    }
    #endregion
}

/// <summary>
/// A container class that stores mission state data.
/// Maintains progress, completion status, and objective states of a mission
/// without storing the full definition.
/// </summary>
/// <remarks>
/// container stores:
/// - mission ID and state
/// - Current obj progress and completion status
/// - Mission availability and unlock status
/// - Links to next missions in a sequence
/// </remarks>
public class MissionDataContainer
{
    public int ID { get; set; }
    public MissionProgress Progress { get; set; }
    public MissionStatus Availability { get; set; }
    public bool Unlocked { get; set; }
    public int CurObjectiveIndex { get; set; }
    public List<ObjectiveIndexState> ObjectiveIndex { get; set; } = [];
    public int NextMissionID { get; set; }

    public TagCompound Serialize()
    {
        return new TagCompound
        {
            ["ID"] = ID,
            ["Progress"] = (int)Progress,
            ["Status"] = (int)Availability,
            ["Unlocked"] = Unlocked,
            ["CurrentIndex"] = CurObjectiveIndex,
            ["Objective"] = ObjectiveIndex.Select(set => set.Serialize()).ToList(),
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
                Availability = (MissionStatus)tag.GetInt("Status"),
                Unlocked = tag.GetBool("Unlocked"),
                CurObjectiveIndex = tag.GetInt("CurrentIndex"),
                ObjectiveIndex = tag.GetList<TagCompound>("Objective")
                    .Select(t => ObjectiveIndexState.Deserialize(t))
                    .ToList(),
                NextMissionID = tag.GetInt("NextMissionID")
            };
        }
        catch (Exception ex)
        {
            // Log error and return null to trigger fallback
            Instance.Logger.Error($"Failed to deserialize mission container: {ex.Message}");
            return null;
        }
    }
}