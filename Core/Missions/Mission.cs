using Reverie.Common.Players;
using Reverie.Common.UI.Missions;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.UI;

namespace Reverie.Core.Missions;

public enum MissionProgress { Inactive, Ongoing, Completed }
public enum MissionStatus { Locked, Unlocked, Completed }

public abstract class Mission
{
    #region Properties
    public int ID { get; set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public List<ObjectiveList> ObjectiveList { get; protected set; }
    public List<Item> Rewards { get; private set; }
    public bool IsMainline { get; }
    public int ProviderNPC { get; set; }
    public int Experience { get; private set; }
    public MissionProgress Progress { get; set; } = MissionProgress.Inactive;
    public MissionStatus Status { get; set; } = MissionStatus.Locked;
    public int CurrentList { get; set; } = 0;
    #endregion

    protected Mission(int id, string name, string description, List<List<(string, int)>> objectiveList,
        List<Item> rewards, bool isMainline, int providerNPC, int xpReward = 0)
    {
        ID = id;
        Name = name;
        Description = description;
        ObjectiveList = objectiveList.Select(set =>
        new ObjectiveList(set.Select(o =>
                new Objective(o.Item1, o.Item2)).ToList())).ToList();
        Rewards = rewards;
        IsMainline = isMainline;
        ProviderNPC = providerNPC;
        Experience = xpReward;
    }

    /// <summary>
    /// Determines if this mission cares about a specific event for the current objective.
    /// Override in child classes to define event matching logic.
    /// </summary>
    /// <param name="evt">The event that occurred</param>
    /// <param name="currentSet">Current objective set index</param>
    /// <param name="objectiveIndex">Objective index within the set</param>
    /// <returns>True if this mission should react to the event</returns>
    public virtual bool MatchesEvent(MissionEvent evt, int currentSet, int objectiveIndex)
    {
        return false;
    }

    /// <summary>
    /// Called when a matching event occurs. Override to customize behavior.
    /// Default: just increment progress by event amount.
    /// </summary>
    /// <param name="evt">The event that matched</param>
    /// <param name="objectiveIndex">Which objective to update</param>
    /// <returns>True if progress was updated</returns>
    public virtual bool OnMatchedEvent(MissionEvent evt, int objectiveIndex)
    {
        return UpdateProgress(objectiveIndex, evt.Amount);
    }

    public virtual void OnMissionStart() { }
    public virtual void OnMissionComplete(bool giveRewards = true)
    {
        if (giveRewards)
            GiveRewards();

        InGameNotificationsTracker.AddNotification(new MissionCompleteNotification(this));
    }

    protected virtual void OnObjectiveIndexComplete(int setIndex, ObjectiveList completedSet) { }
    protected virtual void OnObjectiveComplete(int objectiveIndexWithinCurrentSet) { }

    public void HandleObjectiveCompletion(int objectiveIndex)
    {
        try
        {
            var currentList = ObjectiveList[CurrentList];
            OnObjectiveComplete(objectiveIndex);

            if (currentList.IsCompleted)
            {
                OnObjectiveIndexComplete(CurrentList, currentList);
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in HandleObjectiveCompletion for mission {Name}: {ex.Message}");
        }
    }

    public bool UpdateProgress(int objective, int amount)
    {
        if (Progress != MissionProgress.Ongoing)
            return false;

        var currentSet = ObjectiveList[CurrentList];
        if (objective >= 0 && objective < currentSet.Objective.Count)
        {
            var obj = currentSet.Objective[objective];
            if (!obj.IsCompleted || amount < 0)
            {
                var wasCompleted = obj.UpdateProgress(amount);
                Main.LocalPlayer.GetModPlayer<MissionPlayer>().NotifyMissionUpdate(this);

                if (wasCompleted && amount > 0)
                {
                    HandleObjectiveCompletion(objective);
                    SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}ObjectiveComplete") with { Volume = 0.75f }, Main.LocalPlayer.position);
                }

                if (currentSet.IsCompleted)
                {
                    if (CurrentList < ObjectiveList.Count - 1)
                    {
                        CurrentList++;
                        return true;
                    }
                    if (ObjectiveList.All(set => set.IsCompleted))
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
        CurrentList = 0;
        foreach (var set in ObjectiveList)
        {
            set.Reset();
        }
    }

    public void Complete()
    {
        if (Progress == MissionProgress.Ongoing && ObjectiveList.All(set => set.IsCompleted))
        {
            Progress = MissionProgress.Completed;
            Status = MissionStatus.Completed;
            OnMissionComplete();
        }
    }

    public virtual void Update()
    {
        if (Progress != MissionProgress.Ongoing && Status != MissionStatus.Unlocked) return;
    }

    private void GiveRewards()
    {
        foreach (var reward in Rewards)
        {
            Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), reward.type, reward.stack);
        }
        if (Experience > 0)
        {
            ExperiencePlayer.AddExperience(Main.LocalPlayer, Experience);
            Main.NewText($"{Main.LocalPlayer.name} Gained [c/73d5ff:{Experience} Exp.] from completing [c/73d5ff:{Name}]!", Color.White);
        }
    }

    public void SetObjectiveVisibility(int setIndex, int objectiveIndex, bool isVisible)
    {
        if (setIndex >= 0 && setIndex < ObjectiveList.Count &&
            objectiveIndex >= 0 && objectiveIndex < ObjectiveList[setIndex].Objective.Count)
        {
            ObjectiveList[setIndex].Objective[objectiveIndex].IsVisible = isVisible;
        }
    }

    public void SetObjectiveVisibilityCondition(int setIndex, int objectiveIndex,
        Objective.VisibilityCondition condition)
    {
        if (setIndex >= 0 && setIndex < ObjectiveList.Count &&
            objectiveIndex >= 0 && objectiveIndex < ObjectiveList[setIndex].Objective.Count)
        {
            ObjectiveList[setIndex].Objective[objectiveIndex].VisibilityCheck = condition;
        }
    }

    public void ShowObjectiveAfterCompletion(int setIndex, int objectiveToShow, int dependencyObjective)
    {
        SetObjectiveVisibilityCondition(setIndex, objectiveToShow, (mission) =>
        {
            var set = mission.ObjectiveList[setIndex];
            return dependencyObjective >= 0 &&
                   dependencyObjective < set.Objective.Count &&
                   set.Objective[dependencyObjective].IsCompleted;
        });
    }
}