using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace Reverie.Core.ChallengeSystem;

/// <summary>
/// Central manager for all challenges in the game
/// </summary>
public class ChallengeManager
{
    #region Singleton
    private static ChallengeManager instance;
    public static ChallengeManager Instance => instance ??= new ChallengeManager();
    #endregion

    #region Fields
    private readonly Dictionary<int, Challenge> _allChallenges = [];
    private readonly Dictionary<ChallengeCategory, List<Challenge>> _challengesByCategory = [];
    private bool _initialized = false;
    #endregion

    #region Initialization
    /// <summary>
    /// Initializes the challenge manager and registers all challenges
    /// </summary>
    public void Initialize()
    {
        if (_initialized) return;

        _allChallenges.Clear();
        _challengesByCategory.Clear();

        foreach (ChallengeCategory category in Enum.GetValues(typeof(ChallengeCategory)))
        {
            _challengesByCategory[category] = [];
        }

        RegisterChallenges();

        _initialized = true;
        ModContent.GetInstance<Reverie>().Logger.Info($"Challenge Manager initialized with {_allChallenges.Count} challenges");
    }

    /// <summary>
    /// Register all challenges in the game
    /// </summary>
    private void RegisterChallenges()
    {
        RegisterChallenge(new SlayerChallenge(
            ChallengeID.SlimeSlayer,
            "Slime Slayer",
            "Defeat slimes",
            NPCID.BlueSlime,
            new Dictionary<ChallengeTier, int> {
                { ChallengeTier.Copper, 50 },
                { ChallengeTier.Silver, 250 },
                { ChallengeTier.Gold, 500 },
                { ChallengeTier.Master, 1000 }
            }
        ));

    }
    #endregion

    #region Challenge Management
    /// <summary>
    /// Registers a challenge with the manager
    /// </summary>
    public void RegisterChallenge(Challenge challenge)
    {
        if (challenge == null) return;

        _allChallenges[challenge.ID] = challenge;

        if (!_challengesByCategory.ContainsKey(challenge.Category))
        {
            _challengesByCategory[challenge.Category] = [];
        }

        _challengesByCategory[challenge.Category].Add(challenge);
    }

    /// <summary>
    /// Gets a challenge by ID
    /// </summary>
    public Challenge GetChallenge(int id)
    {
        _allChallenges.TryGetValue(id, out var challenge);
        return challenge;
    }

    /// <summary>
    /// Gets all challenges
    /// </summary>
    public IEnumerable<Challenge> GetAllChallenges()
    {
        return _allChallenges.Values;
    }

    /// <summary>
    /// Gets all challenges in a category
    /// </summary>
    public IEnumerable<Challenge> GetChallengesByCategory(ChallengeCategory category)
    {
        if (_challengesByCategory.TryGetValue(category, out var challenges))
        {
            return challenges;
        }

        return []; //empty enumerable
    }

    /// <summary>
    /// Gets all discovered challenges
    /// </summary>
    public IEnumerable<Challenge> GetDiscoveredChallenges()
    {
        return _allChallenges.Values.Where(c => c.IsDiscovered);
    }

    /// <summary>
    /// Gets all completed challenges
    /// </summary>
    public IEnumerable<Challenge> GetCompletedChallenges()
    {
        return _allChallenges.Values.Where(c => c.IsCompleted);
    }
    #endregion

    #region Save/Load
    /// <summary>
    /// Saves all challenge data
    /// </summary>
    public void SaveChallenges(TagCompound tag)
    {
        var challengeData = new List<TagCompound>();

        foreach (var challenge in _allChallenges.Values)
        {
            if (challenge.IsDirty || challenge.IsDiscovered || challenge.IsCompleted)
            {
                challengeData.Add(challenge.ToDataContainer().Serialize());
                challenge.ClearDirtyFlag();
            }
        }

        tag["Challenges"] = challengeData;
    }

    /// <summary>
    /// Loads all challenge data
    /// </summary>
    public void LoadChallenges(TagCompound tag)
    {
        if (!tag.ContainsKey("Challenges")) return;

        var challengeData = tag.GetList<TagCompound>("Challenges");

        foreach (var data in challengeData)
        {
            var container = ChallengeDataContainer.Deserialize(data);
            if (container != null && _allChallenges.TryGetValue(container.ID, out var challenge))
            {
                challenge.LoadFromContainer(container);
            }
        }
    }
    #endregion

    #region Game Loop
    /// <summary>
    /// Called every game update tick
    /// </summary>
    public void Update()
    {
        // Any global challenge updates can go here
    }
    #endregion
}