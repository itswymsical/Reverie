using Reverie.Core.Dialogue;
using Terraria.GameContent.UI;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System.Collections.Generic;

namespace Reverie.Common.MonoMod;

public class NPCDialogueFrames
{
    public int[] TalkingFrames { get; set; } = [0];
    public int[] HandOutFrames { get; set; } = [0];
    public int TalkingFrameSpeed { get; set; } = 8;
    public int HandOutFrameSpeed { get; set; } = 6;
}

public class DialogueNPCBehavior : GlobalNPC
{
    private static readonly Dictionary<int, NPCDialogueFrames> FrameConfig = new()
    {
        [NPCID.Guide] = new()
        {
            TalkingFrames = [19, 0],
            HandOutFrames = [17],
            TalkingFrameSpeed = 10,
            HandOutFrameSpeed = 8
        },
    };

    private static readonly NPCDialogueFrames DefaultFrameConfig = new()
    {
        TalkingFrames = [0, 1],
        HandOutFrames = [0],
        TalkingFrameSpeed = 8,
        HandOutFrameSpeed = 6
    };

    private bool wasInDialogue = false;
    private float originalAI0 = -1f;
    private float originalAI1 = -1f;
    private int emoteTimer = 0;
    private int nextEmoteTime = 0;

    public int heldItemType = 0;
    public bool isHoldingItem = false;

    public int dialogueFrameCounter = 0;
    public int currentDialogueFrameIndex = 0;
    public bool useHandOutFrames = false;

    public override bool InstancePerEntity => true;

    public override void Load()
    {
        On_NPC.AI_007_TownEntities += OverrideTownNPCAI;
    }

    private void OverrideTownNPCAI(On_NPC.orig_AI_007_TownEntities orig, NPC self)
    {
        bool dialogueActive = DialogueManager.Instance.IsAnyActive();

        if (dialogueActive && self.townNPC && !self.dontTakeDamage)
        {
            HandleDialogueAI(self);
            return;
        }
        else
        {
            var globalNPC = self.GetGlobalNPC<DialogueNPCBehavior>();
            if (globalNPC.wasInDialogue)
            {
                globalNPC.RestoreOriginalAI(self);
            }

            orig(self);
        }
    }

    private void HandleDialogueAI(NPC npc)
    {
        var globalNPC = npc.GetGlobalNPC<DialogueNPCBehavior>();

        if (!globalNPC.wasInDialogue)
        {
            globalNPC.originalAI0 = npc.ai[0];
            globalNPC.originalAI1 = npc.ai[1];
            globalNPC.wasInDialogue = true;
            globalNPC.nextEmoteTime = Main.rand.Next(120, 300);
            globalNPC.ResetFrameAnimation();
        }

        globalNPC.UpdateItemHolding(npc);

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

        globalNPC.HandleMovementDuringDialogue(npc);

        // Keep NPC in idle state
        npc.ai[0] = 0f;
        npc.ai[1] = 300f;

        globalNPC.UpdateDialogueFrames(npc);
        globalNPC.HandleEmotes(npc);

        npc.TargetClosest(false);
    }

    private void UpdateItemHolding(NPC npc)
    {
        bool shouldHoldItem = false;
        int itemToHold = 0;

        // Define item holding conditions
        var holdingConditions = new Dictionary<string, int>
        {
            ["JourneysBegin.MirrorGiven"] = ItemID.MagicMirror
        };

        // Check each condition
        foreach (var condition in holdingConditions)
        {
            if (IsNPCValidForDialogue(npc, condition.Key))
            {
                shouldHoldItem = true;
                itemToHold = condition.Value;
                break;
            }
        }

        if (shouldHoldItem && !isHoldingItem)
        {
            StartHoldingItem(itemToHold);
        }
        else if (!shouldHoldItem && isHoldingItem)
        {
            StopHoldingItem();
        }

        useHandOutFrames = isHoldingItem;
    }

    private bool IsNPCValidForDialogue(NPC npc, string dialogueKey)
    {
        return dialogueKey switch
        {
            "JourneysBegin.MirrorGiven" => npc.type == NPCID.Guide && DialogueManager.Instance.IsDialogueActive(dialogueKey),
            _ => false
        };
    }

    private void UpdateDialogueFrames(NPC npc)
    {
        var frameConfig = GetFrameConfig(npc.type);
        int frameSpeed = useHandOutFrames ? frameConfig.HandOutFrameSpeed : frameConfig.TalkingFrameSpeed;

        dialogueFrameCounter++;

        if (dialogueFrameCounter >= frameSpeed)
        {
            dialogueFrameCounter = 0;
            currentDialogueFrameIndex++;

            int[] currentFrameArray = useHandOutFrames ? frameConfig.HandOutFrames : frameConfig.TalkingFrames;

            if (currentDialogueFrameIndex >= currentFrameArray.Length)
            {
                currentDialogueFrameIndex = 0;
            }
        }
    }

    public void StartHoldingItem(int itemType)
    {
        heldItemType = itemType;
        isHoldingItem = true;
        useHandOutFrames = true;
        ResetFrameAnimation();
    }

    public void StopHoldingItem()
    {
        heldItemType = 0;
        isHoldingItem = false;
        useHandOutFrames = false;
        ResetFrameAnimation();
    }

    public void ResetFrameAnimation()
    {
        currentDialogueFrameIndex = 0;
        dialogueFrameCounter = 0;
    }

    public void SetFrameMode(bool handOut)
    {
        useHandOutFrames = handOut;
        ResetFrameAnimation();
    }

    private void HandleMovementDuringDialogue(NPC npc)
    {
        npc.velocity.X *= 0.8f;
        if (Math.Abs(npc.velocity.X) < 0.1f)
            npc.velocity.X = 0f;
    }

    private void HandleEmotes(NPC npc)
    {
        emoteTimer++;

        if (emoteTimer >= nextEmoteTime)
        {
            int[] talkingEmotes = {
                EmoteID.EmoteNote,
                EmoteID.EmoteConfused,
                EmoteID.EmoteHappiness,
                EmoteID.WeatherSunny
            };

            int selectedEmote = Main.rand.Next(talkingEmotes);
            EmoteBubble.NewBubble(selectedEmote, new WorldUIAnchor(npc), 120);

            emoteTimer = 0;
            nextEmoteTime = Main.rand.Next(180, 420);
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

        StopHoldingItem();
        ResetFrameAnimation();

        npc.localAI[3] = 30f;
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
                if (distance < closestDistance && distance < 400f)
                {
                    closest = player;
                    closestDistance = distance;
                }
            }
        }

        return closest;
    }

    public NPCDialogueFrames GetFrameConfig(int npcType)
    {
        return FrameConfig.TryGetValue(npcType, out var config) ? config : DefaultFrameConfig;
    }

    public override bool PreAI(NPC npc)
    {
        return true;
    }

    public override void PostAI(NPC npc)
    {
        if (DialogueManager.Instance.IsAnyActive() && npc.townNPC)
        {
            if (Math.Abs(npc.velocity.X) > 0.5f)
            {
                npc.velocity.X *= 0.5f;
            }
        }
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (isHoldingItem && heldItemType > 0 && DialogueManager.Instance.IsAnyActive())
        {
            DrawHeldItem(npc, spriteBatch, screenPos, drawColor);
        }
    }

    private void DrawHeldItem(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D itemTexture = TextureAssets.Item[heldItemType].Value;
        Vector2 handOffset = GetHandPosition(npc);
        Vector2 drawPos = npc.Center - screenPos + handOffset;
        float rotation = npc.spriteDirection == 1 ? -0.3f : 0.3f;
        float scale = 0.95f;

        spriteBatch.Draw(
            itemTexture,
            drawPos,
            null,
            drawColor,
            rotation,
            itemTexture.Size() * 0.5f,
            scale,
            npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
            0f
        );
    }

    private Vector2 GetHandPosition(NPC npc)
    {
        var handPositions = new Dictionary<int, Vector2>
        {
            [NPCID.Guide] = new(20, -4),
        };

        Vector2 baseOffset = handPositions.TryGetValue(npc.type, out var pos) ? pos : new Vector2(12, -8);

        if (npc.spriteDirection == -1)
            baseOffset.X *= -1;

        return baseOffset;
    }

    public override void FindFrame(NPC npc, int frameHeight)
    {
        if (DialogueManager.Instance.IsAnyActive() && npc.townNPC)
        {
            var globalNPC = npc.GetGlobalNPC<DialogueNPCBehavior>();
            if (globalNPC.wasInDialogue)
            {
                var frameConfig = GetFrameConfig(npc.type);
                int[] currentFrameArray = globalNPC.useHandOutFrames ? frameConfig.HandOutFrames : frameConfig.TalkingFrames;

                int actualFrame = currentFrameArray[globalNPC.currentDialogueFrameIndex];
                npc.frame.Y = actualFrame * frameHeight;
            }
        }
    }

    public override void Unload()
    {
        // Cleanup handled automatically
    }
}