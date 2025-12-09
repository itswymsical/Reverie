using Reverie.Common.Players;
using Reverie.Common.UI.Missions;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Reverie.Core.Missions;

public enum MissionProgress { Inactive, Ongoing, Completed }
public enum MissionStatus { Locked, Unlocked, Completed }

/// <summary>
/// Represents a mission with objectives, rewards, and progression tracking.
/// Objectives are grouped into sets and must be completed in order.
/// </summary>
/// <remarks>
/// Mainline missions are stored in world data and shared across characters.
/// Sideline missions are stored in individual player data.
/// Single player only - no multiplayer support.
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
    /// <summary>
    /// List of objective sets for this mission.
    /// </summary>
    public List<ObjectiveList> ObjectiveList { get; protected set; }
    public List<Item> Rewards { get; private set; }
    public bool IsMainline { get; }
    public int ProviderNPC { get; set; }
    public int Experience { get; private set; }
    public int NextMissionID { get; private set; }
    public MissionProgress Progress { get; set; } = MissionProgress.Inactive;
    public MissionStatus Status { get; set; } = MissionStatus.Locked;
    public bool Unlocked { get; set; } = false;
    /// <summary>
    /// gets the current objective list
    /// </summary>
    public int CurrentList { get; set; } = 0;
    #endregion

    protected Mission(int id, string name, string description, List<List<(string, int)>> objectiveList,
        List<Item> rewards, bool isMainline, int providerNPC, int nextMissionID = -1, int xpReward = 0)
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
    /// <summary>
    /// Called when an objective set is completed.
    /// </summary>
    /// <param name="setIndex"></param>
    /// <param name="completedSet"></param>
    protected virtual void OnObjectiveIndexComplete(int setIndex, ObjectiveList completedSet) { }
    /// <summary>
    /// Called when an individual objective is completed.
    /// </summary>
    /// <param name="objectiveIndexWithinCurrentSet"></param>
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

    public void ClearInteractedTiles() => interactedTiles.Clear();
    public void ClearInteractedItems() => interactedItems.Clear();
    public bool WasTileInteracted(int i, int j) => interactedTiles.Contains(new Point(i, j));
    public bool WasItemInteracted(int type) => interactedItems.Contains(type);
    public void MarkItemInteracted(int type) => interactedItems.Add(type);

    public bool UpdateProgress(int objective, int amount = 1)
    {
        return IsMainline
            ? MissionWorld.Instance.UpdateProgress(ID, objective, amount)
            : UpdateProgressInternal(objective, amount);
    }

    public bool UpdateProgressInternal(int objective, int amount)
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
        UnregisterEventHandlers();
        Progress = MissionProgress.Inactive;
        CurrentList = 0;
        interactedTiles.Clear();
        interactedItems.Clear();
        foreach (var set in ObjectiveList)
        {
            set.Reset();
        }
    }

    public void Complete()
    {
        if (Progress == MissionProgress.Ongoing && ObjectiveList.All(set => set.IsCompleted))
        {
            UnregisterEventHandlers();
            Progress = MissionProgress.Completed;
            Status = MissionStatus.Completed;
            OnMissionComplete();
        }
        interactedTiles.Clear();
        interactedItems.Clear();
    }

    public virtual void OnMissionComplete(bool giveRewards = true)
    {
        if (giveRewards)
            GiveRewards();

        InGameNotificationsTracker.AddNotification(new MissionCompleteNotification(this));
    }

    /// <summary>
    /// Pauses world events like slime rain and blood moons while the mission is active.
    /// </summary>
    public bool PauseWorldEvents { get; set; } = false;

    public virtual void Update()
    {
        if (Progress != MissionProgress.Ongoing && Status != MissionStatus.Unlocked) return;
        if (Progress == MissionProgress.Ongoing && !eventsRegistered)
        {
            RegisterEventHandlers();
        }

        if (PauseWorldEvents)
        {
            Main.slimeRain = false;
            Main.slimeRainTime = 0;

            Main.bloodMoon = false;

            // 0 means no invasions are active
            Main.invasionType = 0;
            Main.raining = false;
            Main.rainTime = 0;
        }
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
            ["CurrentList"] = CurObjectiveIndex,
            ["ObjectiveList"] = ObjectiveIndex.Select(set => set.Serialize()).ToList(),
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
                CurObjectiveIndex = tag.GetInt("CurrentList"),
                ObjectiveIndex = tag.GetList<TagCompound>("ObjectiveList")
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
