using Reverie.Common.MonoMod;
using Reverie.Core.Dialogue;
using Terraria.GameContent.UI;

namespace Reverie.Utilities.Extensions;

// Extension class for additional dialogue-related NPC behaviors
public static partial class DialogueNPCExtensions
{
    /// <summary>
    /// Forces an NPC to look at a specific position during dialogue
    /// </summary>
    public static void LookAt(this NPC npc, Vector2 position)
    {
        if (position.X < npc.Center.X)
        {
            npc.direction = -1;
            npc.spriteDirection = -1;
        }
        else
        {
            npc.direction = 1;
            npc.spriteDirection = 1;
        }
    }

    /// <summary>
    /// Makes an NPC perform a specific emote
    /// </summary>
    public static void ShowEmote(this NPC npc, int emoteType, int duration = 120)
    {
        EmoteBubble.NewBubble(emoteType, new WorldUIAnchor(npc), duration);
    }

    /// <summary>
    /// Makes the NPC hold out their hand/arm (like offering an item)
    /// </summary>
    public static void HoldOutItem(this NPC npc, int duration = 300)
    {
        npc.ai[0] = 9f; // Hand remains out
        npc.ai[1] = duration;
        npc.ai[2] = 0f;
        npc.localAI[3] = 0f;
    }

    /// <summary>
    /// Makes the NPC talk while holding out their hand (like offering an item while speaking)
    /// </summary>
    public static void TalkWithItemOffer(this NPC npc, int duration = 300)
    {
        // Alternate between states 3 and 4 for talking + hand out combo
        var state = Main.rand.NextBool() ? 3f : 4f;
        npc.ai[0] = state;
        npc.ai[1] = duration;
        npc.ai[2] = 0f;
        npc.localAI[3] = 0f;
    }

    /// <summary>
    /// Makes the NPC use enhanced talking animation
    /// </summary>
    public static void StartTalking(this NPC npc, int duration = 300)
    {
        npc.ai[0] = 19f; // Enhanced talking state
        npc.ai[1] = duration;
        npc.ai[2] = 0f;
        npc.localAI[3] = 0f;
    }

    /// <summary>
    /// Makes the NPC blink/look thoughtful
    /// </summary>
    public static void Blink(this NPC npc, int duration = 120)
    {
        npc.ai[0] = 2f; // Blinking state
        npc.ai[1] = duration;
        npc.ai[2] = 0f;
        npc.localAI[3] = 0f;
    }

    /// <summary>
    /// Makes the NPC celebrate with confetti
    /// </summary>
    public static void Celebrate(this NPC npc, int duration = 180)
    {
        npc.ai[0] = 6f; // Confetti/celebration state
        npc.ai[1] = duration;
        npc.ai[2] = 0f;
        npc.localAI[3] = 0f;
    }

    /// <summary>
    /// Makes the NPC sit down (if there's a valid seat)
    /// </summary>
    public static void SitDown(this NPC npc, int duration = 600)
    {
        npc.ai[0] = 5f; // Sitting state
        npc.ai[1] = duration;
        npc.ai[2] = 0f;
        npc.localAI[3] = 0f;
    }

    /// <summary>
    /// Stops any current action and returns NPC to idle
    /// </summary>
    public static void StopAction(this NPC npc)
    {
        npc.ai[0] = 0f; // Idle state
        npc.ai[1] = 60 + Main.rand.Next(60);
        npc.ai[2] = 0f;
        npc.localAI[3] = 30f;
    }

    /// <summary>
    /// Makes the NPC perform a sequence of actions with timing
    /// </summary>
    public static void PerformActionSequence(this NPC npc, params (float action, int duration)[] sequence)
    {
        if (sequence.Length == 0) return;

        var globalNPC = npc.GetGlobalNPC<DialogueNPCBehavior>();
        globalNPC.StartActionSequence(npc, sequence);
    }

    /// <summary>
    /// Checks if an NPC is currently in dialogue mode
    /// </summary>
    public static bool IsInDialogue(this NPC npc)
    {
        return DialogueManager.Instance.IsAnyActive() && npc.townNPC;
    }

    /// <summary>
    /// Checks if the NPC is currently performing a specific action
    /// </summary>
    public static bool IsPerformingAction(this NPC npc, float actionState)
    {
        return Math.Abs(npc.ai[0] - actionState) < 0.1f;
    }

    /// <summary>
    /// Gets the current action state of the NPC
    /// </summary>
    public static float GetCurrentAction(this NPC npc)
    {
        return npc.ai[0];
    }

    /// <summary>
    /// Helper methods to check specific action states
    /// </summary>
    public static bool IsHoldingOutItem(this NPC npc) => Math.Abs(npc.ai[0] - 9f) < 0.1f;
    public static bool IsTalking(this NPC npc) => Math.Abs(npc.ai[0] - 19f) < 0.1f || Math.Abs(npc.ai[0] - 7f) < 0.1f;
    public static bool IsTalkingWithItemOffer(this NPC npc) => Math.Abs(npc.ai[0] - 3f) < 0.1f || Math.Abs(npc.ai[0] - 4f) < 0.1f;
    public static bool IsBlinking(this NPC npc) => Math.Abs(npc.ai[0] - 2f) < 0.1f;
    public static bool IsCelebrating(this NPC npc) => Math.Abs(npc.ai[0] - 6f) < 0.1f;
    public static bool IsSitting(this NPC npc) => Math.Abs(npc.ai[0] - 5f) < 0.1f;
}