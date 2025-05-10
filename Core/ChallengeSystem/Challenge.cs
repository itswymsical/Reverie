using Reverie.Common.UI;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Reverie.Core.ChallengeSystem;

public enum ChallengeCategory
{
    Combat,
    Exploration,
    Building,
    Fishing,
    Mining,
    Crafting,
    Boss,
    Event,
    Collection,
    Quest
}

public enum ChallengeTier
{
    Copper = 0,
    Silver = 1,
    Gold = 2,
    Master = 3
}

/// <summary>
/// Base class for all challenges in the game.
/// Challenges track long-term player accomplishments and reward players for completion.
/// </summary>
public abstract class Challenge
{
    #region Fields
    protected Player player = Main.LocalPlayer;
    protected bool isDirty = false;
    protected bool eventsRegistered = false;
    #endregion

    #region Properties
    public int ID { get; set; }
    public string Name { get; protected set; }
    public string Description { get; protected set; }
    public ChallengeCategory Category { get; protected set; }

    // Progress tracking
    public int RequiredCount { get; protected set; }
    public int CurrentCount { get; protected set; }
    public bool IsCompleted { get; protected set; }
    public bool IsDiscovered { get; protected set; } // For hidden challenges

    // Tiered progression
    public ChallengeTier CurrentTier { get; protected set; } = ChallengeTier.Copper;
    public Dictionary<ChallengeTier, int> TierRequirements { get; protected set; } = new Dictionary<ChallengeTier, int>();

    // Rewards
    public List<Item> Rewards { get; protected set; } = new List<Item>();
    public Dictionary<ChallengeTier, List<Item>> TieredRewards { get; protected set; } = new Dictionary<ChallengeTier, List<Item>>();
    public int Experience { get; protected set; }

    public bool IsDirty => isDirty;
    #endregion

    #region Initialization
    protected Challenge(int id, string name, string description, ChallengeCategory category, int requiredCount)
    {
        ID = id;
        Name = name;
        Description = description;
        Category = category;
        RequiredCount = requiredCount;

        // Default setup for basic challenge
        TierRequirements[ChallengeTier.Copper] = requiredCount;
    }

    protected Challenge(int id, string name, string description, ChallengeCategory category,
                     Dictionary<ChallengeTier, int> tierRequirements)
    {
        ID = id;
        Name = name;
        Description = description;
        Category = category;
        TierRequirements = tierRequirements;
        RequiredCount = tierRequirements.Values.Max(); // Set the max as the ultimate required count
    }
    #endregion

    #region Event Registration
    /// <summary>
    /// Registers event handlers specific to this challenge
    /// </summary>
    public virtual void Register()
    {
        if (eventsRegistered) return;
        eventsRegistered = true;

        ModContent.GetInstance<Reverie>().Logger.Info($"Challenge {Name} registered event handlers");
    }

    /// <summary>
    /// Unregisters event handlers specific to this challenge
    /// </summary>
    public virtual void Unregister()
    {
        if (!eventsRegistered) return;
        eventsRegistered = false;

        ModContent.GetInstance<Reverie>().Logger.Info($"Challenge {Name} unregistered event handlers");
    }
    #endregion

    #region Core Challenge Logic
    /// <summary>
    /// Updates the progress of this challenge and checks for completion
    /// </summary>
    /// <param name="amount">Amount to increment progress by</param>
    /// <returns>True if the challenge tier advanced or was completed</returns>
    public virtual bool UpdateProgress(int amount = 1)
    {
        if (IsCompleted && CurrentTier == GetHighestTier())
            return false;

        // Update progress
        CurrentCount += amount;
        Main.NewText($"Progress Updated");
        isDirty = true;

        // Check for tier advancement
        ChallengeTier highestTier = GetHighestTier();
        for (ChallengeTier tier = highestTier; tier >= ChallengeTier.Copper; tier--)
        {
            if (TierRequirements.TryGetValue(tier, out int requirement) &&
                CurrentCount >= requirement &&
                tier > CurrentTier)
            {
                ChallengeTier previousTier = CurrentTier;
                CurrentTier = tier;
                OnTierAdvance(previousTier, CurrentTier);

                // Check if this is the highest tier
                if (tier == highestTier)
                {
                    IsCompleted = true;
                    OnComplete();
                }

                return true;
            }
        }

        // No tier advancement
        return false;
    }

    /// <summary>
    /// Sets progress to a specific value
    /// </summary>
    public virtual void SetProgress(int value)
    {
        CurrentCount = value;
        UpdateProgress(0); // Check for tier advancement without incrementing
    }

    /// <summary>
    /// Called when the challenge tier advances
    /// </summary>
    protected virtual void OnTierAdvance(ChallengeTier previousTier, ChallengeTier newTier)
    {
        // Display notification
        var notification = new ChallengeNotification(this,
            name: Name,
            current: CurrentCount,
            max: RequiredCount,
            tier: (int)CurrentTier,
            maxTiers: (int)TierRequirements.Keys.Max());

        InGameNotificationsTracker.AddNotification(notification);

        SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}MissionComplete") with { Volume = 0.75f },
            Main.LocalPlayer.position);

        if (TieredRewards.TryGetValue(newTier, out var rewards))
        {
            GiveRewards(rewards);
        }
    }

    /// <summary>
    /// Called when the challenge is completed at its highest tier
    /// </summary>
    public virtual void OnComplete()
    {
        var notification = new ChallengeNotification(this,
        name: Name,
        current: CurrentCount,
        max: RequiredCount,
        tier: (int)CurrentTier,
        maxTiers: (int)TierRequirements.Keys.Max());

        InGameNotificationsTracker.AddNotification(notification);

        SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}ChallengeComplete") with { Volume = 0.8f },
            Main.LocalPlayer.position);

        GiveRewards(Rewards);
    }

    /// <summary>
    /// Returns the highest tier for this challenge
    /// </summary>
    protected ChallengeTier GetHighestTier()
    {
        return TierRequirements.Keys.Max();
    }

    /// <summary>
    /// Gets the next tier requirement from the current tier
    /// </summary>
    public int GetNextTierRequirement()
    {
        if (CurrentTier == GetHighestTier())
            return RequiredCount;

        ChallengeTier nextTier = CurrentTier + 1;
        return TierRequirements.TryGetValue(nextTier, out int requirement)
            ? requirement
            : RequiredCount;
    }

    /// <summary>
    /// Gets the progress percentage toward the next tier or completion
    /// </summary>
    public float GetProgressPercentage()
    {
        int nextTierRequirement = GetNextTierRequirement();
        int previousTierValue = 0;

        if (CurrentTier > ChallengeTier.Copper)
        {
            TierRequirements.TryGetValue(CurrentTier, out previousTierValue);
        }

        float range = nextTierRequirement - previousTierValue;
        float current = CurrentCount - previousTierValue;

        return range > 0 ? current / range : 1.0f;
    }
    #endregion

    #region Discovery
    /// <summary>
    /// Discovers this challenge, making it visible to the player
    /// </summary>
    public virtual void Discover()
    {
        if (!IsDiscovered)
        {
            IsDiscovered = true;
            isDirty = true;

            // Display notification for discovery
            //InGameNotificationsTracker.AddNotification(new ChallengeDiscoveredNotification(this));
        }
    }
    #endregion

    #region Rewards
    protected void GiveRewards(List<Item> rewardItems)
    {
        if (rewardItems == null || rewardItems.Count == 0)
            return;

        foreach (var reward in rewardItems)
        {
            Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Challenge_Reward"), reward.type, reward.stack);
        }

        if (Experience > 0)
        {
            // If you have an experience system
            // ExperiencePlayer.AddExperience(Main.LocalPlayer, Experience);
            Main.NewText($"{Main.LocalPlayer.name} " +
                $"Gained [c/73d5ff:{Experience} Exp.] " +
                $"from Challenge [c/73d5ff:{Name}]!", Color.White);
        }
    }
    #endregion

    #region Utility
    public void ClearDirtyFlag() => isDirty = false;

    /// <summary>
    /// Creates a data container for saving this challenge
    /// </summary>
    public ChallengeDataContainer ToDataContainer()
    {
        return new ChallengeDataContainer
        {
            ID = ID,
            CurrentCount = CurrentCount,
            IsCompleted = IsCompleted,
            IsDiscovered = IsDiscovered,
            CurrentTier = (int)CurrentTier
        };
    }

    /// <summary>
    /// Loads challenge data from a container
    /// </summary>
    public void LoadFromContainer(ChallengeDataContainer container)
    {
        if (container == null) return;

        CurrentCount = container.CurrentCount;
        IsCompleted = container.IsCompleted;
        IsDiscovered = container.IsDiscovered;
        CurrentTier = (ChallengeTier)container.CurrentTier;
    }
    #endregion
}

/// <summary>
/// Data container for saving challenge state
/// </summary>
public class ChallengeDataContainer
{
    public int ID { get; set; }
    public int CurrentCount { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsDiscovered { get; set; }
    public int CurrentTier { get; set; }

    public TagCompound Serialize()
    {
        return new TagCompound
        {
            ["ID"] = ID,
            ["CurrentCount"] = CurrentCount,
            ["IsCompleted"] = IsCompleted,
            ["IsDiscovered"] = IsDiscovered,
            ["CurrentTier"] = CurrentTier
        };
    }

    public static ChallengeDataContainer Deserialize(TagCompound tag)
    {
        try
        {
            return new ChallengeDataContainer
            {
                ID = tag.GetInt("ID"),
                CurrentCount = tag.GetInt("CurrentCount"),
                IsCompleted = tag.GetBool("IsCompleted"),
                IsDiscovered = tag.GetBool("IsDiscovered"),
                CurrentTier = tag.GetInt("CurrentTier")
            };
        }
        catch (Exception ex)
        {
            Instance.Logger.Error(
                $"Failed to deserialize challenge container: {ex.Message}");
            return null;
        }
    }
}
