using Reverie.Common.UI.Missions;
using Reverie.Utilities;

using System.Collections.Generic;
using System.Linq;

using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Reverie.Core.Missions;

public static class MissionID
{
    public const int A_FALLING_STAR = 1;
    public const int BLOOMCAP = 2;

    public const int FUNGAL_FRACAS = 8;
}

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
/// Represents a mission, managing mission definition, state, and objective event handling.
/// A mission consists of a series of objectives organized into sets that the player must complete,
/// along with associated rewards, progression logic, and completion tracking.
/// </summary>
/// <remarks>
/// Mission progression occurs through objective indexes, where each index must be completed
/// in sequence. When all objective indexes are completed, the mission is marked as complete
/// and rewards are distributed to the player.
/// </remarks>
public abstract class Mission
{
    #region Fields
    protected Player player = Main.LocalPlayer;
    protected bool isDirty = false;
    #endregion

    #region Properties
    public int ID { get; set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public List<ObjectiveSet> Objective { get; protected set; }
    public List<Item> Rewards { get; private set; }
    public bool IsMainline { get; }
    public int Employer { get; set; }
    public int Experience { get; private set; }
    public int NextMissionID { get; private set; }
    public MissionProgress Progress { get; set; } = MissionProgress.Inactive;
    public MissionAvailability Availability { get; set; } = MissionAvailability.Locked;
    public bool Unlocked { get; set; } = false;
    public int CurrentIndex { get; set; } = 0;
    public bool IsDirty => isDirty;
    #endregion

    #region Initialization
    protected Mission(int id, string name, string description, List<List<(string, int)>> objectiveSetData,
        List<Item> rewards, bool isMainline, int npc, int nextMissionID = -1, int xpReward = 0)
    {
        ID = id;
        Name = name;
        Description = description;
        Objective = objectiveSetData.Select(set =>
            new ObjectiveSet(set.Select(o =>
                new Objective(o.Item1, o.Item2)).ToList())).ToList();
        Rewards = rewards;
        IsMainline = isMainline;
        Employer = npc;
        Experience = xpReward;
        NextMissionID = nextMissionID;
    }
    #endregion

    #region Virtual Event Handlers
    public virtual void OnMissionStart() { }
    public void HandleObjectiveCompletion(int objectiveIndex)
    {
        try
        {
            var currentSet = Objective[CurrentIndex];
            var objective = currentSet.Objectives[objectiveIndex];

            if (objective.IsCompleted)
            {
                HandleSpecificObjectiveComplete(CurrentIndex, objectiveIndex, objective);
            }

            if (currentSet.IsCompleted)
            {
                OnIndexComplete(CurrentIndex, currentSet);
            }

            OnObjectiveComplete(objectiveIndex);
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in HandleObjectiveCompletion for mission {Name}: {ex.Message}");
        }
    }

    public virtual void OnHousingFound() { }

    protected virtual void OnIndexComplete(int setIndex, ObjectiveSet set) { }
    protected virtual void HandleSpecificObjectiveComplete(int setIndex, int objectiveIndex, Objective objective) { }

    protected virtual void OnObjectiveComplete(int objectiveIndex) { }

    public virtual void OnCollected(Item item)
    {
        try
        {
            HandleCollected(item);
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnCollected for mission {Name}: {ex.Message}");
        }
    }

    protected virtual void HandleCollected(Item item) { }

    public virtual void OnItemCreated(Item item, ItemCreationContext context)
    {
        try
        {
            HandleItemCreated(item, context);
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnItemCreated for mission {Name}: {ex.Message}");
        }
    }

    protected virtual void HandleItemCreated(Item item, ItemCreationContext context) { }

    public virtual void OnKill(NPC npc)
    {
        try
        {
            HandleKill(npc);
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnKill for mission {Name}: {ex.Message}");
        }
    }

    protected virtual void HandleKill(NPC npc) { }

    public virtual void OnChat(NPC npc)
    {
        try
        {
            HandleChat(npc);
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnChat for mission {Name}: {ex.Message}");
        }
    }

    protected virtual void HandleChat(NPC npc) { }

    public virtual void OnNPCSpawn(NPC npc)
    {
        try
        {
            HandleNPCSpawn(npc);
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnNPCSpawn for mission {Name}: {ex.Message}");
        }
    }

    protected virtual void HandleNPCSpawn(NPC npc) { }

    public virtual void OnHitTarget(NPC npc, int damage)
    {
        try
        {
            ModContent.GetInstance<Reverie>().Logger.Debug($"NPC Hit detected in mission {Name}: Type={npc.type}, Damage={damage}");
            HandleHitTarget(npc, damage);
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnHitTarget for mission {Name}: {ex.Message}");
        }
    }

    protected virtual void HandleHitTarget(NPC npc, int damage) { }

    public virtual void OnBiomeEnter(Player player, BiomeType biome)
    {
        try
        {
            HandleBiomeEnter(player, biome);
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Error in OnBiomeEnter for mission {Name}: {ex.Message}");
        }
    }

    protected virtual void HandleBiomeEnter(Player player, BiomeType biome) { }

    #endregion

    #region Core Mission Logic
    public bool UpdateProgress(int objective, int amount = 1)
    {
        if (Progress != MissionProgress.Active)
            return false;

        var currentSet = Objective[CurrentIndex];
        if (objective >= 0 && objective < currentSet.Objectives.Count)
        {
            var obj = currentSet.Objectives[objective];
            if (!obj.IsCompleted || amount < 0)
            {
                bool wasCompleted = obj.UpdateProgress(amount);

                var player = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
                player.NotifyMissionUpdate(this);

                if (wasCompleted && amount > 0)
                {
                    HandleObjectiveCompletion(objective);
                    SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}ObjectiveComplete") with { Volume = 0.75f }, Main.LocalPlayer.position);
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
        CurrentIndex = 0;
        foreach (var set in Objective)
        {
            set.Reset();
        }
    }

    public void Complete()
    {
        if (Progress == MissionProgress.Active && Objective.All(set => set.IsCompleted))
        {          
            Progress = MissionProgress.Completed;
            Availability = MissionAvailability.Completed;
            isDirty = true;

            OnMissionComplete();

            //MissionPlayer player = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
            //player.StartNextMission(this);
        }
    }

    /// <summary>
    /// Called after the mission is marked as complete, use to set new missions or trigger events.
    /// </summary>
    /// <param name="giveRewards"></param>
    public virtual void OnMissionComplete(bool giveRewards = true)
    {
        if (giveRewards)
            GiveRewards();

        InGameNotificationsTracker.AddNotification(new MissionCompleteNotification(this));
    }

    /// <summary>
    /// Called every game update tick while the mission is active.
    /// Override this in derived classes to implement mission-specific behaviors.
    /// </summary>
    public virtual void Update()
    {
        if (Progress != MissionProgress.Active && Availability != MissionAvailability.Unlocked) return;
    }
    #endregion

    #region Helper Methods
    private void GiveRewards()
    {
        foreach (Item reward in Rewards)
        {
            Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Mission_Reward"), reward.type, reward.stack);
        }
        if (Experience > 0)
        {
            //ExperiencePlayer.AddExperience(Main.LocalPlayer, Experience);
            Main.NewText($"{Main.LocalPlayer.name} " +
                $"Gained [c/73d5ff:{Experience} Exp.] " +
                $"from completing [c/73d5ff:{Name}]!", Color.White);
        }
    }

    public void ClearDirtyFlag() => isDirty = false;
    #endregion
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
    public MissionAvailability Availability { get; set; }
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
            ["Availability"] = (int)Availability,
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
                Availability = (MissionAvailability)tag.GetInt("Availability"),
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
            ModContent.GetInstance<Reverie>().Logger.Error($"Failed to deserialize mission container: {ex.Message}");
            return null;
        }
    }
}