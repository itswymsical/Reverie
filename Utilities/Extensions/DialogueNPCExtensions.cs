using Reverie.Common.MonoMod;
using Reverie.Core.Dialogue;
using System.Linq;
using Terraria.GameContent.UI;

namespace Reverie.Utilities.Extensions;

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
    /// Makes an NPC use an emote bubble
    /// </summary>
    public static void ShowEmote(this NPC npc, int emoteType, int duration = 120)
    {
        EmoteBubble.NewBubble(emoteType, new WorldUIAnchor(npc), duration);
    }

    /// <summary>
    /// Makes the NPC hold an item (with a rendered texture)
    /// </summary>
    public static void HoldItem(this NPC npc, int itemType)
    {
        var globalNPC = npc.GetGlobalNPC<DialogueNPCBehavior>();
        globalNPC?.StartHoldingItem(itemType);
    }

    /// <summary>
    /// Makes the NPC stop holding the item in question
    /// </summary>
    public static void StopHoldingItem(this NPC npc)
    {
        var globalNPC = npc.GetGlobalNPC<DialogueNPCBehavior>();
        globalNPC?.StopHoldingItem();
    }

    /// <summary>
    /// Checks if an NPC is currently holding an item
    /// </summary>
    public static bool IsHoldingItem(this NPC npc)
    {
        var globalNPC = npc.GetGlobalNPC<DialogueNPCBehavior>();
        return globalNPC != null && globalNPC.isHoldingItem;
    }

    /// <summary>
    /// Gets the item type the NPC is currently holding (0 if none)
    /// </summary>
    public static int GetHeldItemType(this NPC npc)
    {
        var globalNPC = npc.GetGlobalNPC<DialogueNPCBehavior>();
        return globalNPC?.heldItemType ?? 0;
    }

    /// <summary>
    /// Checks if an NPC is currently in dialogue mode
    /// </summary>
    public static bool IsInDialogue(this NPC npc)
    {
        return DialogueManager.Instance.IsAnyActive() && npc.townNPC;
    }

    /// <summary>
    /// Checks if the NPC is currently using hand-out frames
    /// </summary>
    public static bool IsUsingHandOutFrames(this NPC npc)
    {
        var globalNPC = npc.GetGlobalNPC<DialogueNPCBehavior>();
        return globalNPC != null && globalNPC.useHandOutFrames;
    }

    /// <summary>
    /// Gets the current dialogue frame index in the frame array (not the actual frame number)
    /// </summary>
    public static int GetCurrentDialogueFrameIndex(this NPC npc)
    {
        var globalNPC = npc.GetGlobalNPC<DialogueNPCBehavior>();
        return globalNPC?.currentDialogueFrameIndex ?? 0;
    }

    /// <summary>
    /// Forces the NPC to use hand-out frames without holding an item
    /// </summary>
    public static void UseHandOutFrames(this NPC npc, bool useHandOut = true)
    {
        var globalNPC = npc.GetGlobalNPC<DialogueNPCBehavior>();
        globalNPC?.SetFrameMode(useHandOut);
    }

    /// <summary>
    /// Forces the NPC to use talking frames
    /// </summary>
    public static void UseTalkingFrames(this NPC npc)
    {
        var globalNPC = npc.GetGlobalNPC<DialogueNPCBehavior>();
        globalNPC?.SetFrameMode(handOut: false);
    }

    /// <summary>
    /// Resets the frame animation to the beginning
    /// </summary>
    public static void ResetDialogueAnimation(this NPC npc)
    {
        var globalNPC = npc.GetGlobalNPC<DialogueNPCBehavior>();
        globalNPC?.ResetFrameAnimation();
    }

    /// <summary>
    /// Sets a specific frame index in the current animation array (useful for specific poses)
    /// </summary>
    public static void SetDialogueFrameIndex(this NPC npc, int frameIndex, bool pauseAnimation = false)
    {
        var globalNPC = npc.GetGlobalNPC<DialogueNPCBehavior>();
        if (globalNPC != null)
        {
            var frameConfig = globalNPC.GetFrameConfig(npc.type);

            // Clamp to valid range
            int maxIndex = globalNPC.useHandOutFrames ?
                frameConfig.HandOutFrames.Length - 1 :
                frameConfig.TalkingFrames.Length - 1;

            globalNPC.currentDialogueFrameIndex = Math.Clamp(frameIndex, 0, maxIndex);

            if (pauseAnimation)
            {
                globalNPC.dialogueFrameCounter = -999; // Prevent frame updates
            }
        }
    }

    /// <summary>
    /// Resumes frame animation if it was paused
    /// </summary>
    public static void ResumeDialogueAnimation(this NPC npc)
    {
        var globalNPC = npc.GetGlobalNPC<DialogueNPCBehavior>();
        if (globalNPC != null)
        {
            globalNPC.dialogueFrameCounter = 0;
        }
    }

    /// <summary>
    /// Gets the current actual frame number being displayed
    /// </summary>
    public static int GetCurrentActualFrame(this NPC npc)
    {
        var globalNPC = npc.GetGlobalNPC<DialogueNPCBehavior>();
        if (globalNPC != null)
        {
            var frameConfig = globalNPC.GetFrameConfig(npc.type);
            int[] currentFrameArray = globalNPC.useHandOutFrames ?
                frameConfig.HandOutFrames :
                frameConfig.TalkingFrames;

            return currentFrameArray[globalNPC.currentDialogueFrameIndex];
        }
        return 0;
    }

    /// <summary>
    /// Forces a specific actual frame number (finds it in the current animation array)
    /// </summary>
    public static void SetActualDialogueFrame(this NPC npc, int actualFrame, bool pauseAnimation = false)
    {
        var globalNPC = npc.GetGlobalNPC<DialogueNPCBehavior>();
        if (globalNPC != null)
        {
            var frameConfig = globalNPC.GetFrameConfig(npc.type);
            int[] currentFrameArray = globalNPC.useHandOutFrames ?
                frameConfig.HandOutFrames :
                frameConfig.TalkingFrames;

            // Find the index of the actual frame
            for (int i = 0; i < currentFrameArray.Length; i++)
            {
                if (currentFrameArray[i] == actualFrame)
                {
                    globalNPC.currentDialogueFrameIndex = i;
                    break;
                }
            }

            if (pauseAnimation)
            {
                globalNPC.dialogueFrameCounter = -999;
            }
        }
    }


    /// <summary>
    /// Legacy method - now sets specific frame index instead of AI state
    /// </summary>
    public static void Celebrate(this NPC npc, int duration = 180)
    {
        // Reset to first frame
        npc.SetDialogueFrameIndex(0);
    }

    /// <summary>
    /// Legacy method - resets to talking frames
    /// </summary>
    public static void StopAction(this NPC npc)
    {
        npc.UseTalkingFrames();
        npc.StopHoldingItem();
    }


    /// <summary>
    /// Checks if NPC is holding out their hand (using hand-out frames)
    /// </summary>
    public static bool IsHoldingOutItem(this NPC npc) => npc.IsUsingHandOutFrames();

    /// <summary>
    /// Checks if NPC is talking (using talking frames)
    /// </summary>
    public static bool IsTalking(this NPC npc) => npc.IsInDialogue() && !npc.IsUsingHandOutFrames();

    /// <summary>
    /// Checks if NPC is talking while offering an item (same as holding out hand)
    /// </summary>
    public static bool IsTalkingWithItemOffer(this NPC npc) => npc.IsUsingHandOutFrames();

    // New helper methods for the configuration system

    /// <summary>
    /// Gets the available talking frames for this NPC type
    /// </summary>
    public static int[] GetAvailableTalkingFrames(this NPC npc)
    {
        var globalNPC = npc.GetGlobalNPC<DialogueNPCBehavior>();
        if (globalNPC != null)
        {
            var frameConfig = globalNPC.GetFrameConfig(npc.type);
            return frameConfig.TalkingFrames;
        }
        return [0];
    }

    /// <summary>
    /// Gets the available hand-out frames for this NPC type
    /// </summary>
    public static int[] GetAvailableHandOutFrames(this NPC npc)
    {
        var globalNPC = npc.GetGlobalNPC<DialogueNPCBehavior>();
        if (globalNPC != null)
        {
            var frameConfig = globalNPC.GetFrameConfig(npc.type);
            return frameConfig.HandOutFrames;
        }
        return [0];
    }

    /// <summary>
    /// Checks if a specific frame number is available in the current animation mode
    /// </summary>
    public static bool IsFrameAvailable(this NPC npc, int frameNumber)
    {
        var globalNPC = npc.GetGlobalNPC<DialogueNPCBehavior>();
        if (globalNPC != null)
        {
            var frameConfig = globalNPC.GetFrameConfig(npc.type);
            int[] currentFrameArray = globalNPC.useHandOutFrames ?
                frameConfig.HandOutFrames :
                frameConfig.TalkingFrames;

            return currentFrameArray.Contains(frameNumber);
        }
        return false;
    }
}