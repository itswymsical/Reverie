using Reverie.Core.Dialogue;
using Terraria.GameContent.UI;

namespace Reverie.Common.MonoMod;

public class DialogueNPCBehavior : GlobalNPC
{
    // AI state constants from vanilla
    private const float AI_STATE_IDLE = 0f;
    private const float AI_STATE_WALKING = 1f;
    private const float AI_STATE_BLINKING = 2f;
    private const float AI_STATE_TALK_WITH_HAND_OUT_1 = 3f;
    private const float AI_STATE_TALK_WITH_HAND_OUT_2 = 4f;
    private const float AI_STATE_SITTING = 5f;
    private const float AI_STATE_CONFETTI = 6f;
    private const float AI_STATE_TALK_PLAYER = 7f;
    private const float AI_STATE_HAND_OUT = 9f;
    private const float AI_STATE_ENHANCED_TALKING = 19f;

    // Track which NPCs are in dialogue mode
    private bool wasInDialogue = false;
    private float originalAI0 = -1f;
    private float originalAI1 = -1f;
    private int emoteTimer = 0;
    private int nextEmoteTime = 0;

    // Action sequence system
    private (float action, int duration)[] actionSequence = null;
    private int currentSequenceIndex = 0;
    private int sequenceTimer = 0;

    public override bool InstancePerEntity => true;

    public override void Load()
    {
        // Hook the main town NPC AI method
        On_NPC.AI_007_TownEntities += OverrideTownNPCAI;
    }

    private void OverrideTownNPCAI(On_NPC.orig_AI_007_TownEntities orig, NPC self)
    {
        // Check if dialogue is active
        bool dialogueActive = DialogueManager.Instance.IsAnyActive();

        if (dialogueActive && self.townNPC && !self.dontTakeDamage)
        {
            HandleDialogueAI(self);
            return; // Skip original AI entirely
        }
        else
        {
            // Restore original AI state if dialogue just ended
            var globalNPC = self.GetGlobalNPC<DialogueNPCBehavior>();
            if (globalNPC.wasInDialogue)
            {
                globalNPC.RestoreOriginalAI(self);
            }

            // Run normal AI
            orig(self);
        }
    }

    private void HandleDialogueAI(NPC npc)
    {
        var globalNPC = npc.GetGlobalNPC<DialogueNPCBehavior>();

        // Store original AI state when entering dialogue
        if (!globalNPC.wasInDialogue)
        {
            globalNPC.originalAI0 = npc.ai[0];
            globalNPC.originalAI1 = npc.ai[1];
            globalNPC.wasInDialogue = true;
            globalNPC.nextEmoteTime = Main.rand.Next(120, 300); // Random emote timing
        }

        // Handle action sequences first
        if (globalNPC.UpdateActionSequence(npc))
        {
            // Action sequence is controlling the NPC
            globalNPC.HandleMovementDuringDialogue(npc);
            return;
        }

        // Find closest player
        Player targetPlayer = FindClosestPlayer(npc);
        if (targetPlayer == null) return;

        // Face the player
        if (targetPlayer.Center.X < npc.Center.X)
        {
            npc.direction = -1;
            npc.spriteDirection = -1;
        }
        else
        {
            npc.direction = 1;
            npc.spriteDirection = 1;
        }

        // Stop movement
        globalNPC.HandleMovementDuringDialogue(npc);

        // Set to enhanced talking state for animations
        npc.ai[0] = AI_STATE_ENHANCED_TALKING; // Enhanced talking state
        npc.ai[1] = 300f; // Keep talking state active
        npc.ai[2] = targetPlayer.whoAmI;

        // Handle talking animation timing
        globalNPC.HandleTalkingAnimation(npc);

        // Handle emote bubbles
        globalNPC.HandleEmotes(npc);

        // Essential updates that must still happen
        npc.TargetClosest(false); // Don't change direction

        // Handle sitting NPCs - make them stand up during dialogue
        if (npc.ai[0] == AI_STATE_SITTING) // Sitting state
        {
            npc.ai[0] = AI_STATE_ENHANCED_TALKING; // Enhanced talking state
            npc.ai[1] = 300f;
        }
    }

    private void HandleMovementDuringDialogue(NPC npc)
    {
        npc.velocity.X *= 0.8f;
        if (Math.Abs(npc.velocity.X) < 0.1f)
            npc.velocity.X = 0f;
    }

    private bool UpdateActionSequence(NPC npc)
    {
        if (actionSequence == null || actionSequence.Length == 0)
            return false;

        sequenceTimer++;

        // Check if current action is complete
        if (currentSequenceIndex < actionSequence.Length)
        {
            var currentAction = actionSequence[currentSequenceIndex];

            if (sequenceTimer == 1) // First frame of this action
            {
                npc.ai[0] = currentAction.action;
                npc.ai[1] = currentAction.duration;
                npc.ai[2] = 0f;
                npc.localAI[3] = 0f;
            }

            if (sequenceTimer >= currentAction.duration)
            {
                currentSequenceIndex++;
                sequenceTimer = 0;

                // If sequence is complete, clean up
                if (currentSequenceIndex >= actionSequence.Length)
                {
                    actionSequence = null;
                    currentSequenceIndex = 0;
                    sequenceTimer = 0;
                    return false;
                }
            }

            return true; // Sequence is still running
        }

        return false;
    }

    public void StartActionSequence(NPC npc, (float action, int duration)[] sequence)
    {
        actionSequence = sequence;
        currentSequenceIndex = 0;
        sequenceTimer = 0;
    }

    private void HandleTalkingAnimation(NPC npc)
    {
        // NPCs will use their talking animation frames automatically when ai[0] = 7
        // The frame counter will handle the animation timing

        // Add subtle head bobbing for more life
        if (Main.GameUpdateCount % 30 == 0)
        {
            npc.localAI[3] = Main.rand.Next(-10, 11); // Small random offset for variety
        }
    }

    private void HandleEmotes(NPC npc)
    {
        emoteTimer++;

        if (emoteTimer >= nextEmoteTime)
        {
            // Show random talking emotes
            int[] talkingEmotes = {
                    EmoteID.EmoteNote,
                    EmoteID.EmoteConfused,
                    EmoteID.EmoteHappiness,
                    EmoteID.WeatherSunny
                };

            int selectedEmote = Main.rand.Next(talkingEmotes);
            EmoteBubble.NewBubble(selectedEmote, new WorldUIAnchor(npc), 120);

            // Reset timer with random interval
            emoteTimer = 0;
            nextEmoteTime = Main.rand.Next(180, 420); // 3-7 seconds at 60fps
        }
    }

    private void RestoreOriginalAI(NPC npc)
    {
        if (originalAI0 >= 0)
        {
            npc.ai[0] = originalAI0;
            npc.ai[1] = originalAI1;
            originalAI0 = -1f;
            originalAI1 = -1f;
        }

        wasInDialogue = false;
        emoteTimer = 0;
        nextEmoteTime = 0;

        // Clean up action sequences
        actionSequence = null;
        currentSequenceIndex = 0;
        sequenceTimer = 0;

        // Small delay before resuming normal behavior
        npc.localAI[3] = 30f; // Give NPC a moment to "process" the conversation
    }

    private Player FindClosestPlayer(NPC npc)
    {
        Player closest = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player player = Main.player[i];
            if (player.active && !player.dead && !player.ghost)
            {
                float distance = Vector2.Distance(npc.Center, player.Center);
                if (distance < closestDistance && distance < 400f) // Reasonable dialogue range
                {
                    closest = player;
                    closestDistance = distance;
                }
            }
        }

        return closest;
    }

    // Alternative approach: Hook AI method more generally
    public override bool PreAI(NPC npc)
    {
        // This runs before all AI, could be used as alternative to the specific hook
        return true; // Allow normal AI to continue
    }

    public override void PostAI(NPC npc)
    {
        // This runs after AI, useful for cleanup or additional behaviors
        if (DialogueManager.Instance.IsAnyActive() && npc.townNPC)
        {
            // Ensure NPCs stay still during dialogue even if something else modified velocity
            if (Math.Abs(npc.velocity.X) > 0.5f)
            {
                npc.velocity.X *= 0.5f;
            }
        }
    }

    // Enhanced version with more dialogue-specific behaviors
    public override void FindFrame(NPC npc, int frameHeight)
    {
        if (DialogueManager.Instance.IsAnyActive() && npc.townNPC)
        {
            // Force talking animation frames
            if (npc.ai[0] == AI_STATE_TALK_PLAYER)
            {
                // Most town NPCs have talking frames
                // This will vary by NPC type, but many use similar patterns

                npc.frameCounter += 1.0;
                if (npc.frameCounter >= 8.0) // Adjust speed as needed
                {
                    npc.frameCounter = 0.0;
                    npc.frame.Y += frameHeight;

                    // Most NPCs have 2-4 talking frames
                    int maxTalkingFrames = GetTalkingFrameCount(npc.type);
                    if (npc.frame.Y >= frameHeight * maxTalkingFrames)
                    {
                        npc.frame.Y = 0; // Loop back to first talking frame
                    }
                }
            }
        }
    }

    private int GetTalkingFrameCount(int npcType)
    {
        // Return the number of talking animation frames for different NPC types
        // You'll need to configure this based on the NPCs you want to support
        return npcType switch
        {
            NPCID.Guide => 25, // Guide has many frames
            NPCID.Merchant => 25,
            NPCID.Nurse => 21,
            NPCID.Demolitionist => 25,
            NPCID.DyeTrader => 25,
            NPCID.Angler => 25,
            _ => 20 // Default fallback
        };
    }

    public override void Unload()
    {
        // Cleanup is handled automatically by tModLoader for method hooks
    }
}