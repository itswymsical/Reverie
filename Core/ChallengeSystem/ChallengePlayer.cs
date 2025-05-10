using Reverie.Core.Missions.Core;
using Reverie.Core.Missions.System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace Reverie.Core.ChallengeSystem;

/// <summary>
/// ModPlayer component for challenge tracking
/// </summary>
public class ChallengePlayer : ModPlayer
{
    public override void Initialize()
    {
        base.Initialize();
    }


    public override void OnEnterWorld()
    {
        // Make sure the challenge manager is initialized
        ChallengeManager.Instance.Initialize();

        // Register events for active challenges
        foreach (var challenge in ChallengeManager.Instance.GetDiscoveredChallenges())
        {
            challenge.Register();
        }

        base.OnEnterWorld();
    }

    public override void SaveData(TagCompound tag)
    {
        // Save challenge data
        var challengeTag = new TagCompound();
        ChallengeManager.Instance.SaveChallenges(challengeTag);
        tag["ChallengeData"] = challengeTag;

        base.SaveData(tag);
    }

    public override void LoadData(TagCompound tag)
    {
        // Load challenge data
        if (tag.ContainsKey("ChallengeData") && tag["ChallengeData"] is TagCompound challengeTag)
        {
            ChallengeManager.Instance.LoadChallenges(challengeTag);
        }

        base.LoadData(tag);
    }

    public override void PostUpdate()
    {
        // Update challenge manager
        ChallengeManager.Instance.Update();
        base.PostUpdate();
    }

    /// <summary>
    /// Discovers a challenge by ID
    /// </summary>
    public void DiscoverChallenge(int challengeId)
    {
        var challenge = ChallengeManager.Instance.GetChallenge(challengeId);
        if (challenge != null && !challenge.IsDiscovered)
        {
            challenge.Discover();
            challenge.Register(); // Start tracking events
        }
    }

    /// <summary>
    /// Gets challenge completion percentage
    /// </summary>
    public float GetChallengeCompletionPercentage()
    {
        var allChallenges = ChallengeManager.Instance.GetAllChallenges().Count();
        if (allChallenges == 0) return 0f;

        var completed = ChallengeManager.Instance.GetCompletedChallenges().Count();
        return (float)completed / allChallenges;
    }
}