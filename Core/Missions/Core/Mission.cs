using Reverie.Common.Players;
using Reverie.Common.UI.Missions;
using Reverie.Utilities;

using System.Collections.Generic;
using System.Linq;

using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Reverie.Core.Missions.Core;

public enum MissionProgress { Inactive, Ongoing, Completed }

public enum MissionStatus { Locked, Unlocked, Completed }

/// <summary>
/// Represents a mission with objectives, rewards, and progression tracking.
/// Objectives are grouped into sets and must be completed in order.
/// </summary>
/// <remarks>
/// Mainline missions are stored in world data and shared across all players.
/// Sideline missions are stored in individual player data.
/// </remarks>
public abstract class Mission
{
    #region Fields
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
    #endregion

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

    protected virtual void RegisterEventHandlers()
    {
        if (eventsRegistered) return;
        eventsRegistered = true;
        ModContent.GetInstance<Reverie>().Logger.Info($"Mission {Name} registered event handlers");
    }

    protected virtual void UnregisterEventHandlers()
    {
        if (!eventsRegistered) return;
        eventsRegistered = false;
        ModContent.GetInstance<Reverie>().Logger.Info($"Mission {Name} unregistered event handlers");
    }

    public virtual void OnMissionStart() => RegisterEventHandlers();
    protected virtual void OnObjectiveIndexComplete(int setIndex, ObjectiveSet completedSet) { }
    protected virtual void OnObjectiveComplete(int objectiveIndexWithinCurrentSet) { }

    public void HandleObjectiveCompletion(int objectiveIndex, Player player)
    {
        try
        {
            var currentSet = Objective[CurrentIndex];
            var objective = currentSet.Objectives[objectiveIndex];
            OnObjectiveComplete(objectiveIndex);

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

    public bool UpdateProgress(int objective, int amount = 1, Player triggeringPlayer = null)
    {
        triggeringPlayer ??= Main.LocalPlayer;
        return IsMainline
            ? WorldMissionSystem.Instance.UpdateProgress(ID, objective, amount, triggeringPlayer)
            : UpdateProgressInternal(objective, amount, triggeringPlayer);
    }

    public bool UpdateProgressInternal(int objective, int amount, Player player)
    {
        if (Progress != MissionProgress.Ongoing)
            return false;

        var currentSet = Objective[CurrentIndex];
        if (objective >= 0 && objective < currentSet.Objectives.Count)
        {
            var obj = currentSet.Objectives[objective];
            if (!obj.IsCompleted || amount < 0)
            {
                var wasCompleted = obj.UpdateProgress(amount);
                player.GetModPlayer<MissionPlayer>().NotifyMissionUpdate(this);

                if (wasCompleted && amount > 0)
                {
                    HandleObjectiveCompletion(objective, player);
                    if (player == Main.LocalPlayer)
                    {
                        SoundEngine.PlaySound(SoundID.MenuTick, player.position);
                    }
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
                        Complete(player);
                        return true;
                    }
                }
            }
        }
        return false;
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

    public void Complete(Player player)
    {
        if (Progress == MissionProgress.Ongoing && Objective.All(set => set.IsCompleted))
        {
            UnregisterEventHandlers();
            Progress = MissionProgress.Completed;
            Status = MissionStatus.Completed;
            OnMissionComplete(player);
        }
        interactedTiles.Clear();
        interactedItems.Clear();
    }

    public virtual void OnMissionComplete(Player player, bool giveRewards = true)
    {
        if (giveRewards)
            GiveRewards(player);

        if (player == Main.LocalPlayer)
        {
            InGameNotificationsTracker.AddNotification(new MissionCompleteNotification(this));
        }
    }

    public virtual void Update()
    {
        if (Progress != MissionProgress.Ongoing && Status != MissionStatus.Unlocked) return;
        if (Progress == MissionProgress.Ongoing && !eventsRegistered)
        {
            RegisterEventHandlers();
        }
    }

    private void GiveRewards(Player player)
    {
        foreach (var reward in Rewards)
        {
            player.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), reward.type, reward.stack);
        }
        if (Experience > 0)
        {
            ExperiencePlayer.AddExperience(player, Experience);
            if (player == Main.LocalPlayer)
            {
                Main.NewText($"{player.name} Gained [c/73d5ff:{Experience} Exp.] from completing [c/73d5ff:{Name}]!", Color.White);
            }
        }
    }

    public void SetObjectiveVisibility(int setIndex, int objectiveIndex, bool isVisible)
    {
        if (setIndex >= 0 && setIndex < Objective.Count &&
            objectiveIndex >= 0 && objectiveIndex < Objective[setIndex].Objectives.Count)
        {
            Objective[setIndex].Objectives[objectiveIndex].IsVisible = isVisible;
        }
    }

    public void SetObjectiveVisibilityCondition(int setIndex, int objectiveIndex,
        Objective.VisibilityCondition condition)
    {
        if (setIndex >= 0 && setIndex < Objective.Count &&
            objectiveIndex >= 0 && objectiveIndex < Objective[setIndex].Objectives.Count)
        {
            Objective[setIndex].Objectives[objectiveIndex].VisibilityCheck = condition;
        }
    }

    public void ShowObjectiveAfterCompletion(int setIndex, int objectiveToShow, int dependencyObjective)
    {
        SetObjectiveVisibilityCondition(setIndex, objectiveToShow, (mission) =>
        {
            var set = mission.Objective[setIndex];
            return dependencyObjective >= 0 &&
                   dependencyObjective < set.Objectives.Count &&
                   set.Objectives[dependencyObjective].IsCompleted;
        });
    }
}
/// <summary>
/// A container class that stores mission state data.
/// Maintains progress, completion status, and objective states of a mission
/// without storing the full definition.
/// </summary>
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
            Instance.Logger.Error($"Failed to deserialize mission container: {ex.Message}");
            return null;
        }
    }
}
