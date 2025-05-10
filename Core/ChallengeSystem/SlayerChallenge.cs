using Reverie.Core.Missions.Core;
using System.Collections.Generic;

namespace Reverie.Core.ChallengeSystem;

/// <summary>
/// Challenge for killing a specific type of NPC
/// </summary>
public class SlayerChallenge : Challenge
{
    private int _targetNPCType;
    private bool _anyNPC;

    public SlayerChallenge(int id, string name, string description, int targetNPC, int requiredCount)
        : base(id, name, description, ChallengeCategory.Combat, requiredCount)
    {
        _targetNPCType = targetNPC;
        _anyNPC = targetNPC == -1;
    }

    public SlayerChallenge(int id, string name, string description, int targetNPC,
                         Dictionary<ChallengeTier, int> tierRequirements)
        : base(id, name, description, ChallengeCategory.Combat, tierRequirements)
    {
        _targetNPCType = targetNPC;
        _anyNPC = targetNPC == -1;
    }

    public override void Register()
    {
        base.Register();
        ObjectiveEventNPC.OnNPCKill += OnNPCKill;
    }

    public override void Unregister()
    {
        base.Unregister();
        ObjectiveEventNPC.OnNPCKill -= OnNPCKill;
    }

    private void OnNPCKill(NPC npc)
    {
        if (_anyNPC || npc.type == _targetNPCType)
        {
            UpdateProgress();
            Main.NewText("Event Handler called, attempted to update progress");
        }
    }
}